using System.Text;
using System.Text.Json;
using CemgSpec.Domain;

namespace CemgSpec.Infrastructure.Reports;

public sealed record ReportExportPayload(
    Guid SavedRecordId,
    Guid ProjectId,
    string ProjectDisplayName,
    string Scope,
    DateTimeOffset CreatedUtc,
    bool IsStale,
    ExportFormat SelectedFormats,
    string DestinationIdentity,
    IReadOnlyDictionary<string, string> SourceHashes,
    IReadOnlyList<string> ToolVersions,
    IReadOnlyList<string> Limitations,
    IReadOnlyList<Finding> Findings,
    IReadOnlyList<HumanDecision> Decisions,
    DashboardProjection? Indicators);

public static class ReportSerializer
{
    public const int CurrentSchemaVersion = 1;
    public const string CsvFormulaPolicy = "prefix-single-quote-for-leading-formula-chars(=,+,-, @,tab,cr)";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string SerializeMarkdown(ReportExportPayload payload)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Relatório Documental — {payload.ProjectDisplayName}");
        sb.AppendLine();
        sb.AppendLine($"- **SavedRecordId**: `{payload.SavedRecordId:D}`");
        sb.AppendLine($"- **SchemaVersion**: `{CurrentSchemaVersion}`");
        sb.AppendLine($"- **Escopo**: {payload.Scope}");
        sb.AppendLine($"- **Gerado em (UTC)**: `{payload.CreatedUtc:O}`");
        sb.AppendLine($"- **Atualidade**: {(payload.IsStale ? "DESATUALIZADO (Stale)" : "Atual")}");
        sb.AppendLine($"- **Ferramentas/Agentes**: {(payload.ToolVersions.Count == 0 ? "Leitor documental CemgSpec V1" : string.Join("; ", payload.ToolVersions))}");
        sb.AppendLine();

        if (payload.Indicators is { } ind)
        {
            sb.AppendLine("## Indicadores documentais");
            sb.AppendLine();
            sb.AppendLine($"- Progresso declarado (`[x]` / total): `{ind.CompletedTaskCount}/{ind.TaskCount}`");
            sb.AppendLine($"- Cobertura documental explícita: `{ind.ExplicitlyLinkedRequirementCount}/{ind.RequirementCount}`");
            sb.AppendLine("- Fórmula: `100 * n / d` quando `d > 0`; `Sem itens` quando `d = 0`. Não representa percentual de implementação.");
            sb.AppendLine();
        }

        sb.AppendLine("## Ocorrências e decisões humanas");
        sb.AppendLine();
        if (payload.Findings.Count == 0)
        {
            sb.AppendLine("Nenhuma ocorrência documental incluída neste pacote.");
        }
        else
        {
            var decisionMap = payload.Decisions.ToDictionary(d => d.FindingId);
            foreach (var f in payload.Findings)
            {
                decisionMap.TryGetValue(f.FindingId, out var dec);
                sb.AppendLine($"### [{f.Severity}] {f.Category} (`{f.FindingId:D}`)");
                sb.AppendLine($"- **Explicação**: {f.Explanation}");
                if (!string.IsNullOrWhiteSpace(f.Suggestion))
                    sb.AppendLine($"- **Sugestão**: {f.Suggestion}");
                sb.AppendLine($"- **Decisão humana**: {dec?.Classification.ToString() ?? "Pending"}{(dec?.NeedsReview == true ? " (requer revisão)" : "")}");
                if (!string.IsNullOrWhiteSpace(dec?.Actor))
                    sb.AppendLine($"- **Responsável**: {dec.Actor} em `{dec.TimestampUtc:O}`");
                if (!string.IsNullOrWhiteSpace(dec?.Reason))
                    sb.AppendLine($"- **Justificativa**: {dec.Reason}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("## Limites explícitos");
        sb.AppendLine();
        foreach (var lim in payload.Limitations)
            sb.AppendLine($"- {lim}");

        return sb.ToString();
    }

    public static string SerializeJson(ReportExportPayload payload)
    {
        var dto = new
        {
            schema_version = CurrentSchemaVersion,
            saved_record_id = payload.SavedRecordId,
            project_id = payload.ProjectId,
            project_display_name = payload.ProjectDisplayName,
            scope = payload.Scope,
            created_utc = payload.CreatedUtc,
            is_stale = payload.IsStale,
            tool_versions = payload.ToolVersions,
            source_hashes = payload.SourceHashes,
            limitations = payload.Limitations,
            indicators = payload.Indicators is null ? null : new
            {
                completed_tasks = payload.Indicators.CompletedTaskCount,
                total_tasks = payload.Indicators.TaskCount,
                explicitly_linked_requirements = payload.Indicators.ExplicitlyLinkedRequirementCount,
                total_requirements = payload.Indicators.RequirementCount,
                formula = "100*n/d if d>0 else NoItems (documentary only)"
            },
            findings = payload.Findings.Select(f => new
            {
                finding_id = f.FindingId,
                category = f.Category,
                severity = f.Severity.ToString(),
                explanation = f.Explanation,
                suggestion = f.Suggestion,
                context_fingerprint = f.ContextFingerprint,
                sources = f.SourceLocations.Select(s => new { s.DocumentId, s.RelativePath, s.LineNumber })
            }),
            human_decisions = payload.Decisions.Select(d => new
            {
                decision_id = d.DecisionId,
                finding_id = d.FindingId,
                classification = d.Classification.ToString(),
                reason = d.Reason,
                actor = d.Actor,
                timestamp_utc = d.TimestampUtc,
                context_fingerprint = d.ContextFingerprint,
                needs_review = d.NeedsReview
            })
        };
        return JsonSerializer.Serialize(dto, JsonOptions);
    }

    public static string SerializeFindingsCsv(ReportExportPayload payload)
    {
        var sb = new StringBuilder();
        sb.AppendLine("FindingId,Category,Severity,Explanation,Suggestion,Classification,Actor,Reason,NeedsReview,ContextFingerprint");
        var decisionMap = payload.Decisions.ToDictionary(d => d.FindingId);
        foreach (var f in payload.Findings)
        {
            decisionMap.TryGetValue(f.FindingId, out var dec);
            sb.AppendLine(string.Join(",",
                CsvCell(f.FindingId.ToString("D")),
                CsvCell(f.Category),
                CsvCell(f.Severity.ToString()),
                CsvCell(f.Explanation),
                CsvCell(f.Suggestion ?? ""),
                CsvCell(dec?.Classification.ToString() ?? "Pending"),
                CsvCell(dec?.Actor ?? ""),
                CsvCell(dec?.Reason ?? ""),
                CsvCell((dec?.NeedsReview ?? false).ToString()),
                CsvCell(f.ContextFingerprint)));
        }
        return sb.ToString();
    }

    public static string SerializeIndicatorsCsv(ReportExportPayload payload)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ProjectDisplayName,Scope,CompletedTasks,TotalTasks,ExplicitlyLinkedRequirements,TotalRequirements,IsStale");
        var ind = payload.Indicators;
        sb.AppendLine(string.Join(",",
            CsvCell(payload.ProjectDisplayName),
            CsvCell(payload.Scope),
            CsvCell((ind?.CompletedTaskCount ?? 0).ToString()),
            CsvCell((ind?.TaskCount ?? 0).ToString()),
            CsvCell((ind?.ExplicitlyLinkedRequirementCount ?? 0).ToString()),
            CsvCell((ind?.RequirementCount ?? 0).ToString()),
            CsvCell(payload.IsStale.ToString())));
        return sb.ToString();
    }

    public static string SerializeManifest(
        ReportExportPayload payload,
        string recordDirectoryName,
        IReadOnlyDictionary<string, string> fileHashes)
    {
        var dto = new
        {
            schema_version = CurrentSchemaVersion,
            state = "Complete",
            saved_record_id = payload.SavedRecordId,
            project_id = payload.ProjectId,
            project_display_name = payload.ProjectDisplayName,
            scope = payload.Scope,
            created_utc = payload.CreatedUtc,
            is_stale = payload.IsStale,
            selected_formats = payload.SelectedFormats.ToString(),
            destination_identity = payload.DestinationIdentity,
            record_directory_name = recordDirectoryName,
            csv_formula_policy = CsvFormulaPolicy,
            tool_versions = payload.ToolVersions,
            source_hashes = payload.SourceHashes,
            files = fileHashes
        };
        return JsonSerializer.Serialize(dto, JsonOptions);
    }

    public static string CsvCell(string raw)
    {
        var value = raw ?? string.Empty;
        if (value.Length > 0 && value[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
            value = "'" + value;

        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
