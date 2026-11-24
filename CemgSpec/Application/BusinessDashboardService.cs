using System.Text;
using System.Text.RegularExpressions;
using CemgSpec.Domain;
using CemgSpec.Infrastructure.Documents;

namespace CemgSpec.Application;

public sealed partial class BusinessDashboardService(
    DocumentaryDashboardService dashboardService,
    SavedHistoryService historyService)
{
    private static readonly UTF8Encoding Utf8 = new(false, throwOnInvalidBytes: true);
    private readonly Dictionary<string, BusinessDashboardProjection> cache = new(StringComparer.Ordinal);

    public BusinessDashboardProjection Build(OpenProjectContext project, Guid? featureId = null)
    {
        var key = $"{project.Session.ProjectId:N}|{project.Catalog.Generation}|{(project.Catalog.HasPendingChanges ? 1 : 0)}|{featureId?.ToString("N") ?? "*"}";
        if (cache.TryGetValue(key, out var cached))
            return cached;

        var tech = dashboardService.Build(project, new DashboardFilter(FeatureId: featureId));
        var broker = new DocumentBroker(project.Catalog);
        var features = featureId is { } fid
            ? project.Session.Features.Where(f => f.FeatureId == fid).ToArray()
            : project.Session.Features.ToArray();

        var stories = new List<UserStoryStatusRow>();
        foreach (var feature in features)
        {
            var entries = project.Catalog.Entries
                .Where(e => e.FeatureId == feature.FeatureId && e.Kind != CatalogKind.PresentationAsset)
                .ToArray();

            var specEntry = entries.FirstOrDefault(e => e.Kind == CatalogKind.Spec);
            var tasksEntry = entries.FirstOrDefault(e => e.Kind == CatalogKind.Tasks);
            var specText = specEntry is not null ? TryRead(project.Catalog, broker, specEntry) : null;
            var tasksText = tasksEntry is not null ? TryRead(project.Catalog, broker, tasksEntry) : null;

            var parsedStories = ParseStoriesFromSpec(specText);
            var taskCounts = ParseTaskCountsByStory(tasksText);

            foreach (var (storyId, title, isDeferred) in parsedStories)
            {
                taskCounts.TryGetValue(storyId, out var counts);

                var devDoc = entries.FirstOrDefault(e =>
                    e.RelativePath.EndsWith($"{Path.DirectorySeparatorChar}validation{Path.DirectorySeparatorChar}{storyId}-dev.md", StringComparison.OrdinalIgnoreCase) ||
                    e.RelativePath.EndsWith($"/validation/{storyId}-dev.md", StringComparison.OrdinalIgnoreCase));
                var userDoc = entries.FirstOrDefault(e =>
                    (e.RelativePath.Contains($"{Path.DirectorySeparatorChar}validation{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) ||
                     e.RelativePath.Contains("/validation/", StringComparison.OrdinalIgnoreCase)) &&
                    e.RelativePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase) &&
                    !e.RelativePath.EndsWith("-dev.md", StringComparison.OrdinalIgnoreCase) &&
                    Path.GetFileName(e.RelativePath).Contains(storyId, StringComparison.OrdinalIgnoreCase));
                var poDoc = entries.FirstOrDefault(e =>
                    e.RelativePath.EndsWith($"{Path.DirectorySeparatorChar}acceptance{Path.DirectorySeparatorChar}{storyId}.md", StringComparison.OrdinalIgnoreCase) ||
                    e.RelativePath.EndsWith($"/acceptance/{storyId}.md", StringComparison.OrdinalIgnoreCase));

                string devState;
                string userState;
                string poState;
                if (isDeferred)
                {
                    devState = "Fora V1";
                    userState = "Fora V1";
                    poState = "Fora V1";
                }
                else
                {
                    devState = ResolveHumanStatus(project.Catalog, broker, devDoc);
                    userState = ResolveHumanStatus(project.Catalog, broker, userDoc);
                    poState = ResolveHumanStatus(project.Catalog, broker, poDoc);
                }

                stories.Add(new UserStoryStatusRow(
                    storyId,
                    title,
                    isDeferred,
                    counts.Completed,
                    counts.Total,
                    devState,
                    userState,
                    poState,
                    devDoc?.DocumentId,
                    userDoc?.DocumentId,
                    poDoc?.DocumentId,
                    devDoc?.RelativePath ?? $"{feature.SpecKitRelativeDirectory}\\validation\\{storyId}-dev.md (ainda não criado)",
                    userDoc?.RelativePath ?? $"{feature.SpecKitRelativeDirectory}\\validation\\{storyId}-uat.md (ainda não criado)",
                    poDoc?.RelativePath ?? $"{feature.SpecKitRelativeDirectory}\\acceptance\\{storyId}.md (ainda não criado)"));
            }
        }

        var activeStories = stories.Where(s => !s.IsDeferred).ToArray();
        var readyStories = activeStories.Count(s => s.IsTechnicallyReady);
        var pendingDev = activeStories.Count(s => string.Equals(s.DevValidationState, "Pendente", StringComparison.OrdinalIgnoreCase));
        var pendingUser = activeStories.Count(s => string.Equals(s.UserValidationState, "Pendente", StringComparison.OrdinalIgnoreCase));
        var pendingPo = activeStories.Count(s => string.Equals(s.PoAcceptanceState, "Pendente", StringComparison.OrdinalIgnoreCase));

        var activeBlockers = tech.Occurrences.Count(o => o.Category == DocumentaryOccurrenceCategory.RecordedBlocker);
        var openGaps = tech.Occurrences.Count(o => o.Category is DocumentaryOccurrenceCategory.RequirementWithoutTask
            or DocumentaryOccurrenceCategory.TaskWithoutRequirement
            or DocumentaryOccurrenceCategory.DuplicateRequirementId
            or DocumentaryOccurrenceCategory.DuplicateTaskId
            or DocumentaryOccurrenceCategory.InvalidReference
            or DocumentaryOccurrenceCategory.MissingArtifact);
        var readyGates = tech.RecordedGateStates.Count(g => g.RecordedState.Equals("READY", StringComparison.OrdinalIgnoreCase));

        EvolutionDelta? evolution = null;
        try
        {
            var saved = historyService.ListSaved(project.Session.ProjectId).FirstOrDefault(p => p.IsValid && p.Record is not null);
            if (saved?.Record is { } rec)
            {
                var currentPct = tech.DeclaredTaskProgress.State == MetricState.Measured ? (tech.DeclaredTaskProgress.Percentage ?? 0) : 0;
                evolution = new EvolutionDelta(
                    rec.RecordDirectoryName,
                    rec.CreatedUtc,
                    PreviousProgressPct: currentPct,
                    CurrentProgressPct: currentPct,
                    PreviousBlockers: saved.Findings.Count(f => f.Severity.Equals("High", StringComparison.OrdinalIgnoreCase)),
                    CurrentBlockers: activeBlockers,
                    PreviousReadyGates: readyGates,
                    CurrentReadyGates: readyGates);
            }
        }
        catch { }

        var projection = new BusinessDashboardProjection(
            project.Session.DisplayName,
            tech.ScopeLabel,
            readyStories,
            activeStories.Length,
            pendingDev,
            pendingUser,
            pendingPo,
            activeBlockers,
            openGaps,
            readyGates,
            stories,
            evolution);

        if (cache.Count > 32) cache.Clear();
        cache[key] = projection;
        return projection;
    }

    private static string? TryRead(DocumentCatalog catalog, DocumentBroker broker, CatalogEntry entry)
    {
        try
        {
            var snap = broker.Read(catalog.ProjectId, entry.DocumentId, catalog.Generation);
            return Utf8.GetString(snap.CopyBytes());
        }
        catch
        {
            return null;
        }
    }

    private static string ResolveHumanStatus(DocumentCatalog catalog, DocumentBroker broker, CatalogEntry? doc)
    {
        if (doc is null) return "Pendente";
        var text = TryRead(catalog, broker, doc);
        if (string.IsNullOrWhiteSpace(text)) return "Pendente";

        // Check explicit human decision lines first so prose mentioning "pendente" elsewhere does not override a recorded decision.
        if (ExplicitAcceptedRegex().IsMatch(text))
            return "Aceito";
        if (ExplicitRejectedRegex().IsMatch(text))
            return "Rejeitado";
        if (text.Contains("PENDENTE", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("_Pendente_", StringComparison.OrdinalIgnoreCase))
            return "Pendente";
        return "Registrado";
    }

    private static List<(string StoryId, string Title, bool IsDeferred)> ParseStoriesFromSpec(string? specText)
    {
        var list = new List<(string, string, bool)>();
        if (string.IsNullOrWhiteSpace(specText)) return list;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in specText.ReplaceLineEndings("\n").Split('\n'))
        {
            var match = StoryHeadingRegex().Match(line);
            if (!match.Success) continue;
            var id = match.Groups[1].Value.ToUpperInvariant();
            if (!seen.Add(id)) continue;
            var rawTitle = match.Groups[2].Value.Trim();
            var isDeferred = rawTitle.Contains("ADIADA", StringComparison.OrdinalIgnoreCase) ||
                             rawTitle.Contains("DEFERRED", StringComparison.OrdinalIgnoreCase);
            var cleanTitle = Regex.Replace(rawTitle, @"\s*[—\-–]\s*Prioridade\s+P\d+.*$", "", RegexOptions.IgnoreCase).Trim();
            list.Add((id, string.IsNullOrWhiteSpace(cleanTitle) ? id : cleanTitle, isDeferred));
        }
        return list;
    }

    private static Dictionary<string, (int Completed, int Total)> ParseTaskCountsByStory(string? tasksText)
    {
        var map = new Dictionary<string, (int Completed, int Total)>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(tasksText)) return map;

        string? phaseStory = null;
        foreach (var line in tasksText.ReplaceLineEndings("\n").Split('\n'))
        {
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                var phaseMatch = StoryTokenRegex().Match(line);
                phaseStory = phaseMatch.Success ? phaseMatch.Groups[1].Value.ToUpperInvariant() : null;
                continue;
            }

            var taskMatch = TaskLineRegex().Match(line);
            if (!taskMatch.Success) continue;
            var marked = !string.IsNullOrWhiteSpace(taskMatch.Groups[1].Value);
            var body = taskMatch.Groups[2].Value;
            var tagMatch = StoryBracketRegex().Match(body);
            var story = tagMatch.Success ? tagMatch.Groups[1].Value.ToUpperInvariant() : phaseStory;
            if (string.IsNullOrWhiteSpace(story)) continue;

            map.TryGetValue(story, out var curr);
            map[story] = (curr.Completed + (marked ? 1 : 0), curr.Total + 1);
        }
        return map;
    }

    [GeneratedRegex(@"^###\s+(US\d+)\s*[—\-–:]\s*(.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex StoryHeadingRegex();

    [GeneratedRegex(@"\b(US\d+)\b", RegexOptions.IgnoreCase)]
    private static partial Regex StoryTokenRegex();

    [GeneratedRegex(@"\[(US\d+)\]", RegexOptions.IgnoreCase)]
    private static partial Regex StoryBracketRegex();

    [GeneratedRegex(@"^\s*-\s*\[([ xX])\]\s*T\d{3,}\b(.*)$")]
    private static partial Regex TaskLineRegex();

    [GeneratedRegex(@"(?:Decis[ãa]o|Resultados?|Status|Situa[çc][ãa]o|Estado|Parecer)[^\r\n:|]{0,60}(?::|\|)\s*\*{0,2}\s*(?:ACEIT[OA]|APROVAD[OA]|VALIDAD[OA]|HOMOLOGAD[OA]|CONCLU[ÍI]D[OA]|PASS|OK|SIM)\b", RegexOptions.IgnoreCase)]
    private static partial Regex ExplicitAcceptedRegex();

    [GeneratedRegex(@"(?:Decis[ãa]o|Resultados?|Status|Situa[çc][ãa]o|Estado|Parecer)[^\r\n:|]{0,60}(?::|\|)\s*\*{0,2}\s*(?:REJEITAD[OA]|REPROVAD[OA]|BLOQUEAD[OA]|FAIL)\b", RegexOptions.IgnoreCase)]
    private static partial Regex ExplicitRejectedRegex();
}
