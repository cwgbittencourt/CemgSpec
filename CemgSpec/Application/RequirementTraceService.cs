using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CemgSpec.Domain;
using CemgSpec.Infrastructure.Documents;

namespace CemgSpec.Application;

public sealed record RecordedDeclaration(
    Guid SourceDocumentId,
    string SourceRelativePath,
    string Kind,
    string DeclaredState,
    string? VersionOrRevision,
    string Summary,
    string ThirdPartyNotice);

public sealed record RequirementSheetProjection(
    Requirement Requirement,
    string FeatureTitle,
    long CatalogGeneration,
    Freshness Freshness,
    IReadOnlyList<EvidenceLink> Links,
    IReadOnlyList<RecordedDeclaration> RecordedDeclarations,
    IReadOnlyList<Finding> Findings,
    IReadOnlyList<string> Limitations);

public sealed partial class RequirementTraceService
{
    private static readonly UTF8Encoding Utf8 = new(false, throwOnInvalidBytes: true);
    private static readonly string[] CodeExtensions =
        [".cs", ".razor", ".ts", ".tsx", ".js", ".jsx", ".py", ".csproj", ".sln", ".slnx"];
    private readonly Dictionary<string, IReadOnlyList<RequirementSheetProjection>> cache = new(StringComparer.Ordinal);

    public IReadOnlyList<RequirementSheetProjection> ListSheets(OpenProjectContext project, Guid? featureId = null)
    {
        var key = $"{project.Session.ProjectId:N}|{project.Catalog.Generation}|{(project.Catalog.HasPendingChanges ? 1 : 0)}|{featureId?.ToString("N") ?? "*"}";
        if (cache.TryGetValue(key, out var cached))
            return cached;

        var built = ListSheets(project.Catalog, new DocumentBroker(project.Catalog), project.Session.Features, featureId);
        if (cache.Count > 64) cache.Clear();
        cache[key] = built;
        return built;
    }

    internal IReadOnlyList<RequirementSheetProjection> ListSheets(
        DocumentCatalog catalog,
        DocumentBroker broker,
        IReadOnlyList<Feature> features,
        Guid? featureId = null)
    {
        var generation = catalog.Generation;
        var freshness = catalog.HasPendingChanges ? Freshness.Stale : Freshness.Current;
        var scopeFeatures = featureId is { } target
            ? features.Where(feature => feature.FeatureId == target).ToArray()
            : features.ToArray();

        var sheets = new List<RequirementSheetProjection>();
        foreach (var feature in scopeFeatures)
        {
            var entries = catalog.Entries
                .Where(entry => entry.FeatureId == feature.FeatureId && entry.Kind != CatalogKind.PresentationAsset)
                .ToArray();
            var specEntry = entries.FirstOrDefault(entry => entry.Kind == CatalogKind.Spec);
            if (specEntry is null) continue;

            var planEntry = entries.FirstOrDefault(entry => entry.Kind == CatalogKind.Plan);
            var tasksEntry = entries.FirstOrDefault(entry => entry.Kind == CatalogKind.Tasks);

            var specText = TryReadUtf8(catalog, broker, generation, specEntry, ref freshness);
            if (specText is null) continue;
            var planText = planEntry is not null ? TryReadUtf8(catalog, broker, generation, planEntry, ref freshness) : null;
            var tasksText = tasksEntry is not null ? TryReadUtf8(catalog, broker, generation, tasksEntry, ref freshness) : null;

            var parsedRequirements = ParseSpecRequirements(catalog.ProjectId, feature, specEntry, specText);
            var parsedTasks = tasksEntry is not null && tasksText is not null
                ? ParseTaskLines(tasksEntry, tasksText)
                : [];
            var planLines = planEntry is not null && planText is not null
                ? planText.ReplaceLineEndings("\n").Split('\n')
                : [];
            var declarations = ParseDeclarations(catalog, broker, generation, entries, ref freshness);

            var duplicateIds = parsedRequirements
                .GroupBy(item => item.LocalId, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var raw in parsedRequirements)
            {
                var key = new RequirementKey(catalog.ProjectId, feature.FeatureId, raw.LocalId);
                var isDuplicate = duplicateIds.Contains(raw.LocalId);
                var links = new List<EvidenceLink>();
                var findings = new List<Finding>();

                // 1. Explicit and inferred task links.
                var explicitTasks = parsedTasks
                    .Where(task => task.ExplicitRefs.Contains(raw.LocalId, StringComparer.OrdinalIgnoreCase))
                    .ToArray();
                foreach (var task in explicitTasks)
                {
                    links.Add(new(
                        Guid.NewGuid(), task.Location, tasksEntry!.DocumentId,
                        $"{task.TaskId}: {task.Text}", "Tarefa vinculada",
                        LinkProvenance.Explicit, LinkResolution.Authorized,
                        $"Vínculo explícito em {task.Location.RelativePath}:{task.Location.LineNumber}."));
                }

                if (raw.StoryRefs.Count > 0)
                {
                    var inferredTasks = parsedTasks
                        .Where(task => !explicitTasks.Contains(task)
                            && task.StoryRef is not null
                            && raw.StoryRefs.Contains(task.StoryRef, StringComparer.OrdinalIgnoreCase))
                        .ToArray();
                    foreach (var task in inferredTasks)
                    {
                        links.Add(new(
                            Guid.NewGuid(), task.Location, tasksEntry!.DocumentId,
                            $"{task.TaskId}: {task.Text}", "Tarefa inferida pela história",
                            LinkProvenance.Inferred, LinkResolution.Ambiguous,
                            $"Vínculo inferido via história {task.StoryRef} sem citação explícita a {raw.LocalId}."));
                    }
                }

                // 2. Plan links & unverified code references mentioned alongside this requirement (or in plan/spec).
                if (planEntry is not null)
                {
                    for (var i = 0; i < planLines.Length; i++)
                    {
                        var line = planLines[i];
                        var lineNumber = i + 1;
                        var mentionsReq = line.Contains(raw.LocalId, StringComparison.OrdinalIgnoreCase);
                        var mentionsStory = !mentionsReq && raw.StoryRefs.Any(story =>
                            Regex.IsMatch(line, $@"\b{Regex.Escape(story)}\b", RegexOptions.IgnoreCase));

                        if (mentionsReq || mentionsStory)
                        {
                            var location = new DocumentSourceLocation(planEntry.DocumentId, planEntry.RelativePath, lineNumber);
                            links.Add(new(
                                Guid.NewGuid(), location, planEntry.DocumentId, line.Trim(), "Referência no plano",
                                mentionsReq ? LinkProvenance.Explicit : LinkProvenance.Inferred,
                                LinkResolution.Authorized,
                                mentionsReq
                                    ? $"Citação explícita no plano ({planEntry.RelativePath}:{lineNumber})."
                                    : $"Referência inferida no plano por história compartilhada ({planEntry.RelativePath}:{lineNumber})."));

                            AddCodeReferenceLinks(line, location, links, findings);
                        }
                    }
                }

                AddCodeReferenceLinks(raw.Description, raw.Location, links, findings);

                // 3. Criteria with CoverageState (Linked / MissingLink / Ambiguous — never Implemented/NotImplemented).
                var explicitTaskIds = explicitTasks.Select(task => task.TaskId).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                var criteria = new List<Criterion>();
                if (raw.Criteria.Count == 0)
                {
                    var state = isDuplicate
                        ? CoverageState.Ambiguous
                        : explicitTaskIds.Length > 0 ? CoverageState.Linked : CoverageState.MissingLink;
                    criteria.Add(new(
                        new CriterionKey(key, $"{raw.LocalId}-DOC"),
                        key, raw.Description, raw.Location, explicitTaskIds, state));
                }
                else
                {
                    for (var idx = 0; idx < raw.Criteria.Count; idx++)
                    {
                        var crit = raw.Criteria[idx];
                        var state = isDuplicate || crit.IsAmbiguous
                            ? CoverageState.Ambiguous
                            : explicitTaskIds.Length > 0 ? CoverageState.Linked : CoverageState.MissingLink;
                        criteria.Add(new(
                            new CriterionKey(key, crit.LocalId),
                            key, crit.Text, crit.Location, explicitTaskIds, state));
                    }
                }

                if (isDuplicate)
                {
                    findings.Add(new(
                        Guid.NewGuid(), null, "Ambiguity", FindingSeverity.High,
                        $"Identificador duplicado {raw.LocalId} na funcionalidade {feature.Title}. Os registros não foram mesclados.",
                        [raw.Location], "Renomear ou consolidar explicitamente os requisitos duplicados em spec.md.",
                        Fingerprint(key, "duplicate")));
                }
                else if (explicitTaskIds.Length == 0)
                {
                    findings.Add(new(
                        Guid.NewGuid(), null, "MissingLink", FindingSeverity.Medium,
                        $"Evidência documental insuficiente: {raw.LocalId} não possui vínculo explícito a tarefas em tasks.md. Ausência de registro não implica não implementado.",
                        [raw.Location], "Vincular o requisito explicitamente às tarefas correspondentes em tasks.md.",
                        Fingerprint(key, "missing-link")));
                }

                var requirement = new Requirement(
                    key, specEntry.DocumentId, raw.Location, raw.Description, raw.StoryRefs, criteria, isDuplicate);

                var limitations = new List<string>
                {
                    "Esta ficha é uma consolidação documental em memória (não é um arquivo SpecKit).",
                    "Ausência de vínculo indica evidência documental insuficiente, nunca que o requisito está não implementado.",
                    "Declarações de validação/aceite são registros de terceiros; o CemgSpec não aprova requisito, teste, PR ou história.",
                    "Análise de código-fonte é capacidade futura; referências a arquivos .cs/.razor/config permanecem texto não verificado."
                };

                sheets.Add(new(requirement, feature.Title, generation, freshness, links, declarations, findings, limitations));
            }
        }

        return sheets;
    }

    private static void AddCodeReferenceLinks(
        string text, DocumentSourceLocation location, List<EvidenceLink> links, List<Finding> findings)
    {
        foreach (Match match in CodePathRegex().Matches(text))
        {
            var rawPath = match.Value;
            links.Add(new(
                Guid.NewGuid(), location, null, rawPath, "Referência a código (não verificada)",
                LinkProvenance.Explicit, LinkResolution.OutOfScope,
                "Referência a arquivo-fonte não verificada nesta versão; análise de código é capacidade futura e o destino não foi aberto."));
            findings.Add(new(
                Guid.NewGuid(), null, "UnverifiedCodeReference", FindingSeverity.Info,
                $"Referência a código {rawPath} em {location.RelativePath}:{location.LineNumber} permanece não verificada nesta versão.",
                [location], null,
                Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes($"{location.RelativePath}:{location.LineNumber}:{rawPath}")))));
        }
    }

    private static List<ParsedRequirement> ParseSpecRequirements(
        Guid projectId, Feature feature, CatalogEntry specEntry, string specText)
    {
        var results = new List<ParsedRequirement>();
        string? currentStory = null;
        var lines = specText.ReplaceLineEndings("\n").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (line.StartsWith("## ", StringComparison.Ordinal))
                currentStory = null;
            var lineNumber = i + 1;
            var storyMatch = StoryHeadingRegex().Match(line);
            if (storyMatch.Success)
                currentStory = storyMatch.Groups[1].Value.ToUpperInvariant();

            var reqMatch = RequirementLineRegex().Match(line);
            if (reqMatch.Success)
            {
                var localId = reqMatch.Groups[1].Value.ToUpperInvariant();
                var desc = reqMatch.Groups[2].Value.Trim();
                var stories = StoryRefRegex().Matches(desc)
                    .Select(m => m.Value.ToUpperInvariant())
                    .Concat(currentStory is not null ? [currentStory] : [])
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                results.Add(new(localId, desc, new(specEntry.DocumentId, specEntry.RelativePath, lineNumber), stories, []));
            }
        }
        return results;
    }

    private static List<ParsedTaskLine> ParseTaskLines(CatalogEntry tasksEntry, string tasksText)
    {
        var results = new List<ParsedTaskLine>();
        var lines = tasksText.ReplaceLineEndings("\n").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var match = TaskLineRegex().Match(lines[i]);
            if (!match.Success) continue;
            var taskId = match.Groups[2].Value.ToUpperInvariant();
            var body = match.Groups[3].Value.Trim();
            var storyMatch = StoryTagRegex().Match(body);
            var story = storyMatch.Success ? storyMatch.Groups[1].Value.ToUpperInvariant() : null;
            var refs = RequirementRefRegex().Matches(body)
                .Select(m => m.Value.ToUpperInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            results.Add(new(taskId, body, new(tasksEntry.DocumentId, tasksEntry.RelativePath, i + 1), refs, story));
        }
        return results;
    }

    private static List<RecordedDeclaration> ParseDeclarations(
        DocumentCatalog catalog, DocumentBroker broker, long generation, CatalogEntry[] entries, ref Freshness freshness)
    {
        var list = new List<RecordedDeclaration>();
        const string notice = "Declaração exibida como registro de terceiros; o CemgSpec não aprova requisito, teste, PR ou história.";

        foreach (var entry in entries.Where(e => e.Kind == CatalogKind.GateRecord &&
                     e.RelativePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
        {
            var text = TryReadUtf8(catalog, broker, generation, entry, ref freshness);
            if (text is null) continue;
            try
            {
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) continue;
                var stage = root.TryGetProperty("stage", out var s) && s.ValueKind == JsonValueKind.String
                    ? s.GetString() ?? "gate"
                    : Path.GetFileNameWithoutExtension(entry.RelativePath);
                var state = root.TryGetProperty("state", out var st) && st.ValueKind == JsonValueKind.String
                    ? st.GetString() ?? "REGISTRADO"
                    : "REGISTRADO";
                var rev = root.TryGetProperty("revision", out var r) && r.ValueKind == JsonValueKind.String
                    ? r.GetString()
                    : null;
                list.Add(new(entry.DocumentId, entry.RelativePath, $"Gate ({stage})", state, rev,
                    $"Registro documental de gate em {entry.RelativePath}.", notice));
            }
            catch (JsonException)
            {
            }
        }

        return list;
    }

    private static string? TryReadUtf8(DocumentCatalog catalog, DocumentBroker broker, long generation, CatalogEntry entry, ref Freshness freshness)
    {
        try
        {
            var snapshot = broker.Read(catalog.ProjectId, entry.DocumentId, generation);
            var bytes = snapshot.CopyBytes().AsSpan();
            if (bytes.StartsWith(Encoding.UTF8.Preamble))
                bytes = bytes[Encoding.UTF8.Preamble.Length..];
            return Utf8.GetString(bytes);
        }
        catch (DocumentAccessException ex) when (ex.Code == "Changed")
        {
            freshness = Freshness.Stale;
            return null;
        }
        catch (Exception ex) when (ex is DocumentAccessException or DecoderFallbackException or IOException)
        {
            return null;
        }
    }

    private static string Fingerprint(RequirementKey key, string category) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes($"{key.ProjectId}:{key.FeatureId}:{key.LocalId}:{category}")));

    private sealed record ParsedCriterion(string LocalId, string Text, DocumentSourceLocation Location, bool IsAmbiguous);
    private sealed record ParsedRequirement(string LocalId, string Description, DocumentSourceLocation Location, IReadOnlyList<string> StoryRefs, IReadOnlyList<ParsedCriterion> Criteria);
    private sealed record ParsedTaskLine(string TaskId, string Text, DocumentSourceLocation Location, IReadOnlyList<string> ExplicitRefs, string? StoryRef);

    [GeneratedRegex(@"^###\s+(US\d+)\b", RegexOptions.IgnoreCase)]
    private static partial Regex StoryHeadingRegex();

    [GeneratedRegex(@"^\s*-\s*\*{0,2}((?:FR|NFR|RF|RNF|REQ)-\d{3,})\*{0,2}\s*[:\-–—]\s*(.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex RequirementLineRegex();

    [GeneratedRegex(@"\bUS\d+\b", RegexOptions.IgnoreCase)]
    private static partial Regex StoryRefRegex();

    [GeneratedRegex(@"^\s*-\s*\[([ xX])\]\s*(T\d{3,})\b(.*)$")]
    private static partial Regex TaskLineRegex();

    [GeneratedRegex(@"\[(US\d+)\]", RegexOptions.IgnoreCase)]
    private static partial Regex StoryTagRegex();

    [GeneratedRegex(@"\b(?:FR|NFR|RF|RNF|REQ)-\d{3,}\b", RegexOptions.IgnoreCase)]
    private static partial Regex RequirementRefRegex();

    [GeneratedRegex(@"(?:[A-Za-z0-9_.\-/\\]+)\.(?:cs|razor|ts|tsx|js|jsx|py|csproj|sln|slnx)\b", RegexOptions.IgnoreCase)]
    private static partial Regex CodePathRegex();
}

