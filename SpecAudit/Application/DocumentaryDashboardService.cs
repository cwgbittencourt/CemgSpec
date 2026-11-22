using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using SpecAudit.Domain;
using SpecAudit.Infrastructure.Documents;

namespace SpecAudit.Application;

public sealed partial class DocumentaryDashboardService
{
    private static readonly UTF8Encoding Utf8 = new(false, throwOnInvalidBytes: true);
    private static readonly string[] CodeExtensions =
        [".cs", ".razor", ".ts", ".tsx", ".js", ".jsx", ".py", ".csproj", ".sln", ".slnx"];
    private static bool IsArchivedPath(string relativePath) =>
        relativePath.Contains("\\archive\\", StringComparison.OrdinalIgnoreCase) ||
        relativePath.Contains("/archive/", StringComparison.OrdinalIgnoreCase);

    private static bool IsSetupOrFoundationPhase(string? phase) =>
        !string.IsNullOrWhiteSpace(phase) && (
            phase.Contains("Preparação", StringComparison.OrdinalIgnoreCase) ||
            phase.Contains("Setup", StringComparison.OrdinalIgnoreCase) ||
            phase.Contains("Fundação", StringComparison.OrdinalIgnoreCase) ||
            phase.Contains("Foundational", StringComparison.OrdinalIgnoreCase) ||
            phase.Contains("Migração", StringComparison.OrdinalIgnoreCase) ||
            phase.Contains("Migration", StringComparison.OrdinalIgnoreCase) ||
            phase.Contains("Regressão", StringComparison.OrdinalIgnoreCase) ||
            phase.Contains("Fechamento", StringComparison.OrdinalIgnoreCase) ||
            phase.Contains("Polish", StringComparison.OrdinalIgnoreCase));
    private readonly Dictionary<string, DashboardProjection> cache = new(StringComparer.Ordinal);

    public DashboardProjection Build(OpenProjectContext project, DashboardFilter? filter = null)
    {
        filter ??= new DashboardFilter();
        var key = $"{project.Session.ProjectId:N}|{project.Catalog.Generation}|{(project.Catalog.HasPendingChanges ? 1 : 0)}|{filter}";
        if (cache.TryGetValue(key, out var cached))
            return cached;

        var built = Build(project.Catalog, new DocumentBroker(project.Catalog), project.Session.Features, filter);
        if (cache.Count > 64) cache.Clear();
        cache[key] = built;
        return built;
    }

    internal DashboardProjection Build(
        DocumentCatalog catalog,
        DocumentBroker broker,
        IReadOnlyList<Feature> features,
        DashboardFilter? filter = null)
    {
        filter ??= new DashboardFilter();
        var generation = catalog.Generation;
        var freshness = catalog.HasPendingChanges ? Freshness.Stale : Freshness.Current;
        var selectedFeatures = filter.FeatureId is { } targetFeature
            ? features.Where(feature => feature.FeatureId == targetFeature).ToArray()
            : features.ToArray();
        var scopeLabel = filter.FeatureId is { } id && selectedFeatures.Length == 1
            ? selectedFeatures[0].Title
            : $"Projeto inteiro ({selectedFeatures.Length} funcionalidades)";

        var allTasks = new List<TaskDeclaration>();
        var allRequirements = new List<DocumentaryRequirementSummary>();
        var allGates = new List<RecordedGateState>();
        var allOccurrences = new List<DocumentaryOccurrence>();
        var limitations = new List<string>
        {
            "Progresso declarado reflete apenas caixas [x]/[ ] em tasks.md; não comprova implementação.",
            "Cobertura documental conta requisitos com vínculo explícito em tasks.md mantendo itens sem vínculo ou duplicados no denominador.",
            "Estados READY e hashes registrados em documentos/gates são declarações documentais e não comprovam testes funcionais ou aceite humano autenticado.",
            "Referências a arquivos de código-fonte permanecem não verificadas nesta versão (nenhum código é aberto ou analisado)."
        };

        foreach (var feature in selectedFeatures)
        {
            var entries = catalog.Entries
                .Where(entry => entry.FeatureId == feature.FeatureId && entry.Kind != CatalogKind.PresentationAsset)
                .ToArray();

            var specEntry = entries.FirstOrDefault(entry => entry.Kind == CatalogKind.Spec);
            var planEntry = entries.FirstOrDefault(entry => entry.Kind == CatalogKind.Plan);
            var tasksEntry = entries.FirstOrDefault(entry => entry.Kind == CatalogKind.Tasks);

            if (specEntry is null)
                AddMissingArtifact(allOccurrences, feature, "spec.md", entries.FirstOrDefault());
            if (planEntry is null)
                AddMissingArtifact(allOccurrences, feature, "plan.md", entries.FirstOrDefault());
            if (tasksEntry is null)
                AddMissingArtifact(allOccurrences, feature, "tasks.md", entries.FirstOrDefault());

            var rawRequirements = specEntry is not null
                ? ParseRequirements(catalog, broker, generation, feature, specEntry, allOccurrences, ref freshness)
                : [];

            var featureTasks = tasksEntry is not null
                ? ParseTasks(catalog, broker, generation, feature, tasksEntry, allOccurrences, ref freshness)
                : [];

            allTasks.AddRange(featureTasks);

            // Detect duplicate requirement IDs in the same feature without merging them.
            var duplicateReqIds = rawRequirements
                .GroupBy(item => item.LocalId, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var dupId in duplicateReqIds)
            {
                var first = rawRequirements.First(item => item.LocalId.Equals(dupId, StringComparison.OrdinalIgnoreCase));
                allOccurrences.Add(new(
                    Guid.NewGuid(), feature.FeatureId, feature.Title,
                    DocumentaryOccurrenceCategory.DuplicateRequirementId,
                    DocumentaryOccurrenceSeverity.High,
                    $"Requisito duplicado: {dupId}",
                    $"O identificador {dupId} aparece mais de uma vez em {first.Location.RelativePath}. Os registros não foram mesclados e permanecem no denominador.",
                    first.Location, null, first.StoryRef, "Ambíguo"));
            }

            var tasksByReq = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var task in featureTasks)
            {
                if (task.ExplicitRequirementRefs.Count == 0 &&
                    string.IsNullOrWhiteSpace(task.StoryRef) &&
                    !IsSetupOrFoundationPhase(task.Phase))
                {
                    allOccurrences.Add(new(
                        Guid.NewGuid(), feature.FeatureId, feature.Title,
                        DocumentaryOccurrenceCategory.TaskWithoutRequirement,
                        DocumentaryOccurrenceSeverity.Low,
                        $"Tarefa sem vínculo explícito a requisito ou história: {task.Key.LocalId}",
                        $"A tarefa {task.Key.LocalId} em {task.SourceLocation.RelativePath}:{task.SourceLocation.LineNumber} não cita requisito (FR-xxx) nem história ([USx]).",
                        task.SourceLocation, task.Phase, task.StoryRef, "Sem vínculo"));
                }
                foreach (var reqId in task.ExplicitRequirementRefs)
                {
                    if (!tasksByReq.TryGetValue(reqId, out var list))
                    {
                        list = [];
                        tasksByReq[reqId] = list;
                    }
                    if (!list.Contains(task.Key.LocalId, StringComparer.OrdinalIgnoreCase))
                        list.Add(task.Key.LocalId);
                }
            }

            foreach (var req in rawRequirements)
            {
                var linkedTasks = tasksByReq.TryGetValue(req.LocalId, out var list)
                    ? (IReadOnlyList<string>)list
                    : [];
                var isDuplicate = duplicateReqIds.Contains(req.LocalId);
                allRequirements.Add(new(
                    feature.FeatureId, feature.Title, req.LocalId, req.Description,
                    req.Location, linkedTasks, isDuplicate, req.StoryRef));

                var hasStoryTasks = !string.IsNullOrWhiteSpace(req.StoryRef) &&
                    featureTasks.Any(t => string.Equals(t.StoryRef, req.StoryRef, StringComparison.OrdinalIgnoreCase));

                if (!isDuplicate && linkedTasks.Count == 0 && !hasStoryTasks)
                {
                    allOccurrences.Add(new(
                        Guid.NewGuid(), feature.FeatureId, feature.Title,
                        DocumentaryOccurrenceCategory.RequirementWithoutTask,
                        DocumentaryOccurrenceSeverity.Medium,
                        $"Requisito sem tarefa vinculada: {req.LocalId}",
                        $"O requisito {req.LocalId} em {req.Location.RelativePath}:{req.Location.LineNumber} não possui vínculo documental explícito nem por história em tasks.md.",
                        req.Location, null, req.StoryRef, "Sem vínculo"));
                }
            }

            // Parse recorded gate JSON documents via broker without reading code targets (ignoring archived folders).
            foreach (var gateEntry in entries.Where(entry => entry.Kind == CatalogKind.GateRecord &&
                         entry.RelativePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
                         !IsArchivedPath(entry.RelativePath)))
            {
                ParseGateRecord(catalog, broker, generation, feature, gateEntry, allGates, allOccurrences, ref freshness);
            }
        }

        // Apply optional story/phase filters to task/requirement/occurrence projections.
        var filteredTasks = allTasks.Where(task =>
            (string.IsNullOrWhiteSpace(filter.Phase) || string.Equals(task.Phase, filter.Phase, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(filter.StoryRef) || string.Equals(task.StoryRef, filter.StoryRef, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        var filteredRequirements = allRequirements.Where(req =>
            string.IsNullOrWhiteSpace(filter.StoryRef) || string.Equals(req.StoryRef, filter.StoryRef, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var filteredOccurrences = allOccurrences.Where(occ =>
            (string.IsNullOrWhiteSpace(filter.Phase) || string.Equals(occ.Phase, filter.Phase, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(filter.StoryRef) || string.Equals(occ.StoryRef, filter.StoryRef, StringComparison.OrdinalIgnoreCase)) &&
            (filter.Severity is null || occ.Severity == filter.Severity) &&
            (string.IsNullOrWhiteSpace(filter.Situation) || string.Equals(occ.Situation, filter.Situation, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        var completedTasks = filteredTasks.Count(task => task.MarkedComplete);
        var totalTasks = filteredTasks.Length;
        var linkedRequirements = filteredRequirements.Count(req => !req.HasDuplicateDefinition && req.ExplicitTaskIds.Count > 0);
        var totalRequirements = filteredRequirements.Length;

        var declaredMetric = new DocumentaryMetric(
            completedTasks, totalTasks,
            "Tarefas marcadas [x] divididas pelo total de tarefas em tasks.md (declaração documental).");
        var coverageMetric = new DocumentaryMetric(
            linkedRequirements, totalRequirements,
            "Requisitos com vínculo explícito não ambíguo a tarefas divididos pelo total de requisitos em spec.md (cobertura documental).");

        var phases = allTasks.Select(task => task.Phase).Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).Cast<string>().ToArray();
        var stories = allTasks.Select(task => task.StoryRef)
            .Concat(allRequirements.Select(req => req.StoryRef))
            .Concat(allOccurrences.Select(occ => occ.StoryRef))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).Cast<string>().ToArray();

        return new DashboardProjection(
            catalog.ProjectId,
            filter.FeatureId,
            scopeLabel,
            generation,
            freshness,
            declaredMetric,
            coverageMetric,
            completedTasks,
            totalTasks,
            linkedRequirements,
            totalRequirements,
            filteredTasks,
            filteredRequirements,
            allGates,
            filteredOccurrences,
            phases,
            stories,
            limitations);
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

    private static List<(string LocalId, string Description, DocumentSourceLocation Location, string? StoryRef)> ParseRequirements(
        DocumentCatalog catalog, DocumentBroker broker, long generation, Feature feature, CatalogEntry entry,
        List<DocumentaryOccurrence> occurrences, ref Freshness freshness)
    {
        var results = new List<(string, string, DocumentSourceLocation, string?)>();
        var text = TryReadUtf8(catalog, broker, generation, entry, ref freshness);
        if (text is null) return results;

        string? currentStory = null;
        var lines = text.ReplaceLineEndings("\n").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var lineNumber = i + 1;
            if (line.StartsWith("## ", StringComparison.Ordinal))
                currentStory = null;

            var storyMatch = StoryHeadingRegex().Match(line);
            if (storyMatch.Success)
                currentStory = storyMatch.Groups[1].Value.ToUpperInvariant();

            var reqMatch = RequirementLineRegex().Match(line);
            if (reqMatch.Success)
            {
                var id = reqMatch.Groups[1].Value.ToUpperInvariant();
                var desc = reqMatch.Groups[2].Value.Trim();
                var inlineStory = PhaseStoryRegex().Match(desc);
                var effectiveStory = inlineStory.Success ? inlineStory.Groups[1].Value.ToUpperInvariant() : currentStory;
                results.Add((id, desc, new(entry.DocumentId, entry.RelativePath, lineNumber), effectiveStory));
            }

            DetectCodeReferences(feature, entry, line, lineNumber, null, currentStory, occurrences);
        }
        return results;
    }

    private static List<TaskDeclaration> ParseTasks(
        DocumentCatalog catalog, DocumentBroker broker, long generation, Feature feature, CatalogEntry entry,
        List<DocumentaryOccurrence> occurrences, ref Freshness freshness)
    {
        var results = new List<TaskDeclaration>();
        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var text = TryReadUtf8(catalog, broker, generation, entry, ref freshness);
        if (text is null) return results;

        string? currentPhase = null;
        string? currentPhaseStory = null;
        var lines = text.ReplaceLineEndings("\n").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var lineNumber = i + 1;
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                currentPhase = line[3..].Trim();
                var phaseStoryMatch = PhaseStoryRegex().Match(line);
                currentPhaseStory = phaseStoryMatch.Success ? phaseStoryMatch.Groups[1].Value.ToUpperInvariant() : null;
            }

            var match = TaskLineRegex().Match(line);
            if (match.Success)
            {
                var marked = !string.IsNullOrWhiteSpace(match.Groups[1].Value);
                var taskId = match.Groups[2].Value.ToUpperInvariant();
                var body = match.Groups[3].Value.Trim();
                var storyMatch = StoryTagRegex().Match(body);
                var story = storyMatch.Success ? storyMatch.Groups[1].Value.ToUpperInvariant() : currentPhaseStory;
                var reqRefs = RequirementRefRegex().Matches(body)
                    .Select(m => m.Value.ToUpperInvariant())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                var location = new DocumentSourceLocation(entry.DocumentId, entry.RelativePath, lineNumber);

                if (!seenIds.Add(taskId))
                {
                    occurrences.Add(new(
                        Guid.NewGuid(), feature.FeatureId, feature.Title,
                        DocumentaryOccurrenceCategory.DuplicateTaskId,
                        DocumentaryOccurrenceSeverity.Medium,
                        $"Tarefa duplicada: {taskId}",
                        $"O identificador {taskId} aparece mais de uma vez em {entry.RelativePath}.",
                        location, currentPhase, story, "Ambíguo"));
                }

                results.Add(new TaskDeclaration(
                    new TaskKey(catalog.ProjectId, feature.FeatureId, taskId),
                    body, marked, location, reqRefs, currentPhase, story));
            }

            DetectCodeReferences(feature, entry, line, lineNumber, currentPhase, null, occurrences);
        }
        return results;
    }

    private static void ParseGateRecord(
        DocumentCatalog catalog, DocumentBroker broker, long generation, Feature feature, CatalogEntry entry,
        List<RecordedGateState> gates, List<DocumentaryOccurrence> occurrences, ref Freshness freshness)
    {
        var text = TryReadUtf8(catalog, broker, generation, entry, ref freshness);
        if (text is null) return;

        try
        {
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return;

            var stage = root.TryGetProperty("stage", out var stageEl) && stageEl.ValueKind == JsonValueKind.String
                ? stageEl.GetString() ?? "desconhecido"
                : Path.GetFileNameWithoutExtension(entry.RelativePath);
            var state = root.TryGetProperty("state", out var stateEl) && stateEl.ValueKind == JsonValueKind.String
                ? stateEl.GetString() ?? "REGISTRADO"
                : "REGISTRADO";
            var revision = root.TryGetProperty("revision", out var revEl) && revEl.ValueKind == JsonValueKind.String
                ? revEl.GetString()
                : null;

            var blockers = new List<string>();
            if (root.TryGetProperty("blockers", out var blockersEl) && blockersEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in blockersEl.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                    {
                        var blockerText = item.GetString()!;
                        blockers.Add(blockerText);
                        occurrences.Add(new(
                            Guid.NewGuid(), feature.FeatureId, feature.Title,
                            DocumentaryOccurrenceCategory.RecordedBlocker,
                            DocumentaryOccurrenceSeverity.High,
                            $"Impedimento registrado ({stage}): {blockerText}",
                            $"Registro documental em {entry.RelativePath} declara impedimento ativo: {blockerText}.",
                            new(entry.DocumentId, entry.RelativePath, 1), null, null, "Impedimento"));
                    }
                }
            }

            var hasCodeRef = false;
            var paths = new List<string>();
            CollectReferencedPaths(root, paths);
            foreach (var rawPath in paths.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (IsCodeReference(rawPath))
                {
                    hasCodeRef = true;
                    occurrences.Add(new(
                        Guid.NewGuid(), feature.FeatureId, feature.Title,
                        DocumentaryOccurrenceCategory.UnverifiedCodeReference,
                        DocumentaryOccurrenceSeverity.Info,
                        $"Referência a código não verificada: {rawPath}",
                        $"O registro {entry.RelativePath} referencia {rawPath}. Referência a código não verificada nesta versão; o destino não foi lido.",
                        new(entry.DocumentId, entry.RelativePath, 1), null, null, "Não verificada"));
                }
                else if (rawPath.Contains("..", StringComparison.Ordinal))
                {
                    occurrences.Add(new(
                        Guid.NewGuid(), feature.FeatureId, feature.Title,
                        DocumentaryOccurrenceCategory.InvalidReference,
                        DocumentaryOccurrenceSeverity.Medium,
                        $"Referência fora do catálogo: {rawPath}",
                        $"O registro {entry.RelativePath} aponta para caminho fora do escopo autorizado ({rawPath}).",
                        new(entry.DocumentId, entry.RelativePath, 1), null, null, "Inválida"));
                }
            }

            var note = state.Equals("READY", StringComparison.OrdinalIgnoreCase)
                ? "Estado READY registrado em documento; não comprova funcionamento, teste executado pelo SpecAudit nem aceite humano autenticado."
                : "Estado documental registrado no arquivo de verificação; distinto do resultado de execução atual do check_gate.";

            gates.Add(new(
                entry.DocumentId, entry.RelativePath, feature.FeatureId, feature.Title,
                stage, state, revision, blockers, hasCodeRef, note, freshness));
        }
        catch (JsonException)
        {
            occurrences.Add(new(
                Guid.NewGuid(), feature.FeatureId, feature.Title,
                DocumentaryOccurrenceCategory.InvalidReference,
                DocumentaryOccurrenceSeverity.Medium,
                $"Registro JSON inválido: {Path.GetFileName(entry.RelativePath)}",
                $"Não foi possível interpretar o arquivo de verificação {entry.RelativePath} como JSON válido.",
                new(entry.DocumentId, entry.RelativePath, 1), null, null, "Inválida"));
        }
    }

    private static void CollectReferencedPaths(JsonElement element, List<string> paths)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name is "path" or "stdout" or "stderr" && property.Value.ValueKind == JsonValueKind.String)
                {
                    var value = property.Value.GetString();
                    if (!string.IsNullOrWhiteSpace(value)) paths.Add(value);
                }
                else
                {
                    CollectReferencedPaths(property.Value, paths);
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                CollectReferencedPaths(item, paths);
        }
    }

    private static void DetectCodeReferences(
        Feature feature, CatalogEntry entry, string line, int lineNumber, string? phase, string? story,
        List<DocumentaryOccurrence> occurrences)
    {
        foreach (Match match in CodePathRegex().Matches(line))
        {
            var raw = match.Value;
            occurrences.Add(new(
                Guid.NewGuid(), feature.FeatureId, feature.Title,
                DocumentaryOccurrenceCategory.UnverifiedCodeReference,
                DocumentaryOccurrenceSeverity.Info,
                $"Referência a código não verificada: {raw}",
                $"O documento {entry.RelativePath}:{lineNumber} menciona {raw}. Referência a código não verificada nesta versão; nenhum arquivo-fonte foi aberto.",
                new(entry.DocumentId, entry.RelativePath, lineNumber), phase, story, "Não verificada"));
        }
    }

    private static bool IsCodeReference(string value) =>
        CodeExtensions.Any(ext => value.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) ||
        value.EndsWith("appsettings.json", StringComparison.OrdinalIgnoreCase) ||
        value.EndsWith("package.json", StringComparison.OrdinalIgnoreCase);

    private static void AddMissingArtifact(List<DocumentaryOccurrence> occurrences, Feature feature, string name, CatalogEntry? fallback) =>
        occurrences.Add(new(
            Guid.NewGuid(), feature.FeatureId, feature.Title,
            DocumentaryOccurrenceCategory.MissingArtifact,
            DocumentaryOccurrenceSeverity.Medium,
            $"Artefato documental ausente: {name}",
            $"A funcionalidade {feature.Title} não possui {name} no catálogo autorizado.",
            new(fallback?.DocumentId ?? Guid.Empty, Path.Combine(feature.SpecKitRelativeDirectory, name), 0),
            null, null, "Ausente"));

    [GeneratedRegex(@"^###\s+(US\d+)\b", RegexOptions.IgnoreCase)]
    private static partial Regex StoryHeadingRegex();

    [GeneratedRegex(@"\b(US\d+)\b", RegexOptions.IgnoreCase)]
    private static partial Regex PhaseStoryRegex();

    [GeneratedRegex(@"^\s*-\s*\*{0,2}((?:FR|NFR|RF|RNF|REQ)-\d{3,})\*{0,2}\s*[:\-–—]\s*(.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex RequirementLineRegex();

    [GeneratedRegex(@"^\s*-\s*\[([ xX])\]\s*(T\d{3,})\b(.*)$")]
    private static partial Regex TaskLineRegex();

    [GeneratedRegex(@"\[(US\d+)\]", RegexOptions.IgnoreCase)]
    private static partial Regex StoryTagRegex();

    [GeneratedRegex(@"\b(?:FR|NFR|RF|RNF|REQ)-\d{3,}\b", RegexOptions.IgnoreCase)]
    private static partial Regex RequirementRefRegex();

    [GeneratedRegex(@"(?:[A-Za-z0-9_.\-/\\]+)\.(?:cs|razor|ts|tsx|js|jsx|py|csproj|sln|slnx)\b", RegexOptions.IgnoreCase)]
    private static partial Regex CodePathRegex();
}

