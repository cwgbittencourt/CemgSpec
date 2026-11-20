using System.Security.Cryptography;
using System.Text;
using SpecAudit.Application;
using SpecAudit.Domain;
using SpecAudit.Infrastructure.Documents;

namespace SpecAudit.Infrastructure.Reports;

public sealed record SaveOperationResult(
    SaveState State,
    SavedRecord? Record,
    string? RecordDirectoryPath,
    string? ErrorCode,
    string? Message);

public sealed class ExternalReportWriter
{
    public const string PolicyVersion = "v4";
    public const string PlatformFingerprint = "win-x64";
    public const string WriterVersion = "1.0.0";

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    public SaveOperationResult Save(
        OpenProjectContext context,
        ReportExportPayload payload,
        Action<string>? beforeFileWrite = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(payload);

        if (payload.SelectedFormats == ExportFormat.None)
            return new(SaveState.SaveFailed, null, null, "NoFormatSelected", "Selecione ao menos um formato de exportação (Markdown, JSON ou CSV).");

        // Revalidate origin and external destination by Windows handle identity at save time (TOCTOU protection).
        string finalOrigin;
        string finalDestination;
        string destIdentity;
        try
        {
            using var originHandle = WindowsReadHandle.Open(context.Catalog.FinalOriginPath, directory: true);
            finalOrigin = WindowsReadHandle.FinalPath(originHandle);

            var candidateDest = Path.GetFullPath(context.OutputDisplayPath);
            if (HasReparsePointOnPath(candidateDest))
            {
                return new(SaveState.SaveFailed, null, null, "DestinationReparsePointDenied",
                    "Destino externo contém junção ou link simbólico; gravação recusada.");
            }

            using var destHandle = WindowsReadHandle.Open(candidateDest, directory: true);
            finalDestination = WindowsReadHandle.FinalPath(destHandle);
            destIdentity = WindowsReadHandle.Info(destHandle).Identity;

            if (string.Equals(finalOrigin, finalDestination, StringComparison.OrdinalIgnoreCase) ||
                WindowsReadHandle.Contains(finalOrigin, finalDestination))
            {
                return new(SaveState.SaveFailed, null, null, "DestinationInsideOrigin",
                    "Destino externo não pode coincidir nem estar contido na origem auditada.");
            }

            if (!string.Equals(destIdentity, context.Session.OutputDirectoryIdentity, StringComparison.Ordinal))
            {
                return new(SaveState.SaveFailed, null, null, "DestinationIdentityChanged",
                    "A identidade do diretório externo de destino mudou desde a seleção; reconfirme o destino antes de salvar.");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DocumentAccessException)
        {
            return new(SaveState.SaveFailed, null, null, "DestinationUnavailable",
                $"Destino externo inacessível no momento do salvamento: {ex.Message}");
        }

        var dirName = $"SpecAudit-{payload.CreatedUtc:yyyyMMddTHHmmssZ}-{payload.SavedRecordId:N}";
        var recordDir = Path.Combine(finalDestination, dirName);
        if (Directory.Exists(recordDir))
        {
            return new(SaveState.SaveFailed, null, recordDir, "RecordAlreadyExists",
                "Diretório de registro já existe; sobrescrita é proibida.");
        }

        var createdDir = false;
        var writtenHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            Directory.CreateDirectory(recordDir);
            createdDir = true;

            var filesToWrite = new List<(string Name, string Content)>();
            if (payload.SelectedFormats.HasFlag(ExportFormat.Markdown))
                filesToWrite.Add(("report.md", ReportSerializer.SerializeMarkdown(payload)));
            if (payload.SelectedFormats.HasFlag(ExportFormat.Json))
                filesToWrite.Add(("report.json", ReportSerializer.SerializeJson(payload)));
            if (payload.SelectedFormats.HasFlag(ExportFormat.Csv))
            {
                filesToWrite.Add(("findings.csv", ReportSerializer.SerializeFindingsCsv(payload)));
                filesToWrite.Add(("indicators.csv", ReportSerializer.SerializeIndicatorsCsv(payload)));
            }

            foreach (var (name, content) in filesToWrite)
            {
                beforeFileWrite?.Invoke(name);
                var target = Path.Combine(recordDir, name);
                var bytes = Utf8NoBom.GetBytes(content);
                using (var fs = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    fs.Write(bytes, 0, bytes.Length);
                writtenHashes[name] = Convert.ToHexStringLower(SHA256.HashData(bytes));
            }

            // Write manifest.json LAST with state=Complete and final SHA-256 hashes.
            beforeFileWrite?.Invoke("manifest.json");
            var manifestContent = ReportSerializer.SerializeManifest(payload, dirName, writtenHashes);
            var manifestBytes = Utf8NoBom.GetBytes(manifestContent);
            var manifestPath = Path.Combine(recordDir, "manifest.json");
            using (var fs = new FileStream(manifestPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                fs.Write(manifestBytes, 0, manifestBytes.Length);

            var savedRecord = new SavedRecord(
                payload.SavedRecordId,
                payload.ProjectId,
                payload.ProjectDisplayName,
                payload.Scope,
                payload.CreatedUtc,
                ReportSerializer.CurrentSchemaVersion,
                payload.SourceHashes,
                payload.ToolVersions,
                writtenHashes,
                payload.SelectedFormats,
                destIdentity,
                dirName,
                payload.IsStale,
                IsComplete: true);

            return new(SaveState.Saved, savedRecord, recordDir, null, $"Registro salvo em {recordDir}.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Never delete partial directory automatically; report honest SaveFailed with partial path.
            var partialNote = createdDir
                ? $" Falha parcial: diretório externo incompleto mantido para inspeção/exclusão humana em '{recordDir}'."
                : "";
            return new(SaveState.SaveFailed, null, createdDir ? recordDir : null, "PartialWriteFailure",
                $"Falha ao gravar relatório externo ({ex.Message}).{partialNote}");
        }
    }

    public static bool HasReparsePointOnPath(string fullPath)
    {
        var current = Path.GetFullPath(fullPath);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (Directory.Exists(current) || File.Exists(current))
            {
                if (File.GetAttributes(current).HasFlag(FileAttributes.ReparsePoint))
                    return true;
            }
            var parent = Path.GetDirectoryName(current);
            if (string.IsNullOrWhiteSpace(parent) || string.Equals(parent, current, StringComparison.OrdinalIgnoreCase))
                break;
            current = parent;
        }
        return false;
    }
}
