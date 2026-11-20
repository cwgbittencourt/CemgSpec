using System.Security.Cryptography;
using System.Text.Json;
using SpecAudit.Application;
using SpecAudit.Domain;
using SpecAudit.Infrastructure.Documents;

namespace SpecAudit.Infrastructure.Reports;

public sealed record SavedFindingEntry(
    Guid FindingId,
    string Category,
    string Severity,
    string Explanation,
    string? Suggestion,
    string ContextFingerprint);

public sealed record SavedPackageSummary(
    SavedRecord? Record,
    string DirectoryName,
    string DirectoryPath,
    bool IsValid,
    bool IsIncomplete,
    bool IsTampered,
    string? DiagnosticMessage,
    long TotalBytes,
    IReadOnlyList<SavedFindingEntry> Findings);

public sealed record DeleteSavedResult(
    bool Deleted,
    string? ErrorCode,
    string Message);

public sealed class SavedHistoryReader
{
    public IReadOnlyList<SavedPackageSummary> ListPackages(OpenProjectContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var (ok, finalDestination, _, _) = ValidateDestination(context);
        if (!ok || finalDestination is null)
            return [];

        var results = new List<SavedPackageSummary>();
        foreach (var dir in Directory.EnumerateDirectories(finalDestination, "SpecAudit-*", SearchOption.TopDirectoryOnly))
        {
            if (ExternalReportWriter.HasReparsePointOnPath(dir))
                continue;

            results.Add(ReadPackage(dir));
        }

        return results
            .OrderByDescending(p => p.Record?.CreatedUtc ?? DateTimeOffset.MinValue)
            .ThenByDescending(p => p.DirectoryName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public DeleteSavedResult DeletePackage(OpenProjectContext context, Guid savedRecordId)
    {
        ArgumentNullException.ThrowIfNull(context);

        var (ok, finalDestination, errorCode, errorMessage) = ValidateDestination(context);
        if (!ok || finalDestination is null)
            return new(false, errorCode ?? "DestinationUnavailable", errorMessage ?? "Destino externo indisponível.");

        var packages = ListPackages(context);
        var match = packages.FirstOrDefault(p => p.Record?.SavedRecordId == savedRecordId);
        if (match is null)
            return new(false, "RecordNotFound", "Registro salvo não encontrado no destino externo.");

        var fullTarget = Path.GetFullPath(match.DirectoryPath);
        if (!WindowsReadHandle.Contains(finalDestination, fullTarget) || ExternalReportWriter.HasReparsePointOnPath(fullTarget))
            return new(false, "UnsafeDeleteTarget", "Caminho de exclusão fora do destino externo ou contém reparse point.");

        try
        {
            Directory.Delete(fullTarget, recursive: true);
            return new(true, null, $"Registro '{match.DirectoryName}' excluído do destino externo.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new(false, "DeleteFailed", $"Falha ao excluir registro externo: {ex.Message}");
        }
    }

    private static (bool Ok, string? FinalDestination, string? ErrorCode, string? Message) ValidateDestination(OpenProjectContext context)
    {
        try
        {
            using var originHandle = WindowsReadHandle.Open(context.Catalog.FinalOriginPath, directory: true);
            var finalOrigin = WindowsReadHandle.FinalPath(originHandle);

            var candidateDest = Path.GetFullPath(context.OutputDisplayPath);
            if (ExternalReportWriter.HasReparsePointOnPath(candidateDest))
                return (false, null, "DestinationReparsePointDenied", "Destino externo contém reparse point.");

            using var destHandle = WindowsReadHandle.Open(candidateDest, directory: true);
            var finalDest = WindowsReadHandle.FinalPath(destHandle);
            var destIdentity = WindowsReadHandle.Info(destHandle).Identity;

            if (string.Equals(finalOrigin, finalDest, StringComparison.OrdinalIgnoreCase) ||
                WindowsReadHandle.Contains(finalOrigin, finalDest))
                return (false, null, "DestinationInsideOrigin", "Destino externo não pode estar dentro da origem.");

            if (!string.Equals(destIdentity, context.Session.OutputDirectoryIdentity, StringComparison.Ordinal))
                return (false, null, "DestinationIdentityChanged", "Identidade do destino externo mudou.");

            return (true, finalDest, null, null);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DocumentAccessException)
        {
            return (false, null, "DestinationUnavailable", ex.Message);
        }
    }

    private static SavedPackageSummary ReadPackage(string dirPath)
    {
        var dirName = Path.GetFileName(dirPath);
        long totalBytes = 0;
        try
        {
            foreach (var file in Directory.EnumerateFiles(dirPath, "*", SearchOption.AllDirectories))
                totalBytes += new FileInfo(file).Length;
        }
        catch (IOException) { }

        var manifestPath = Path.Combine(dirPath, "manifest.json");
        if (!File.Exists(manifestPath))
        {
            return new(
                null,
                dirName,
                dirPath,
                IsValid: false,
                IsIncomplete: true,
                IsTampered: false,
                DiagnosticMessage: "Pacote incompleto: manifest.json ausente.",
                TotalBytes: totalBytes,
                Findings: []);
        }

        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(manifestPath));
            var root = doc.RootElement;
            var schemaVersion = root.GetProperty("schema_version").GetInt32();
            var state = root.GetProperty("state").GetString();
            if (schemaVersion != ReportSerializer.CurrentSchemaVersion || !string.Equals(state, "Complete", StringComparison.Ordinal))
            {
                return new(null, dirName, dirPath, false, true, false,
                    $"Manifesto inválido ou incompleto (schema={schemaVersion}, state={state}).", totalBytes, []);
            }

            var savedRecordId = root.GetProperty("saved_record_id").GetGuid();
            var projectId = root.GetProperty("project_id").GetGuid();
            var projectDisplayName = root.GetProperty("project_display_name").GetString() ?? "";
            var scope = root.GetProperty("scope").GetString() ?? "";
            var createdUtc = root.GetProperty("created_utc").GetDateTimeOffset();
            var isStale = root.GetProperty("is_stale").GetBoolean();
            var destIdentity = root.GetProperty("destination_identity").GetString() ?? "";
            var formatsStr = root.GetProperty("selected_formats").GetString() ?? "";
            _ = Enum.TryParse<ExportFormat>(formatsStr, out var selectedFormats);

            var filesElement = root.GetProperty("files");
            var fileHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in filesElement.EnumerateObject())
            {
                var expectedHash = prop.Value.GetString() ?? "";
                fileHashes[prop.Name] = expectedHash;
                var filePath = Path.Combine(dirPath, prop.Name);
                if (!File.Exists(filePath))
                {
                    return new(null, dirName, dirPath, false, false, true,
                        $"Arquivo '{prop.Name}' listado no manifesto está ausente.", totalBytes, []);
                }
                var actualHash = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(filePath)));
                if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase))
                {
                    return new(null, dirName, dirPath, false, false, true,
                        $"Integridade violada: SHA-256 de '{prop.Name}' diverge do manifesto.", totalBytes, []);
                }
            }

            var sourceHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (root.TryGetProperty("source_hashes", out var sh) && sh.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in sh.EnumerateObject())
                    sourceHashes[prop.Name] = prop.Value.GetString() ?? "";
            }

            var toolVersions = new List<string>();
            if (root.TryGetProperty("tool_versions", out var tv) && tv.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in tv.EnumerateArray())
                    if (item.GetString() is { } s) toolVersions.Add(s);
            }

            var findings = ReadFindingsFromReportJson(Path.Combine(dirPath, "report.json"));

            var record = new SavedRecord(
                savedRecordId,
                projectId,
                projectDisplayName,
                scope,
                createdUtc,
                schemaVersion,
                sourceHashes,
                toolVersions,
                fileHashes,
                selectedFormats,
                destIdentity,
                dirName,
                isStale,
                IsComplete: true);

            return new(record, dirName, dirPath, true, false, false, null, totalBytes, findings);
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or FormatException or IOException)
        {
            return new(null, dirName, dirPath, false, false, true,
                $"Manifesto corrompido ou ilegível: {ex.Message}", totalBytes, []);
        }
    }

    private static IReadOnlyList<SavedFindingEntry> ReadFindingsFromReportJson(string reportJsonPath)
    {
        if (!File.Exists(reportJsonPath)) return [];
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(reportJsonPath));
            if (!doc.RootElement.TryGetProperty("findings", out var arr) || arr.ValueKind != JsonValueKind.Array)
                return [];

            var list = new List<SavedFindingEntry>();
            foreach (var item in arr.EnumerateArray())
            {
                var id = item.GetProperty("finding_id").GetGuid();
                var category = item.GetProperty("category").GetString() ?? "";
                var severity = item.GetProperty("severity").GetString() ?? "";
                var explanation = item.GetProperty("explanation").GetString() ?? "";
                var suggestion = item.TryGetProperty("suggestion", out var s) && s.ValueKind == JsonValueKind.String ? s.GetString() : null;
                var fp = item.GetProperty("context_fingerprint").GetString() ?? "";
                list.Add(new(id, category, severity, explanation, suggestion, fp));
            }
            return list;
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or IOException)
        {
            return [];
        }
    }
}
