namespace SpecAudit.Domain;

public enum SaveState
{
    Unsaved,
    Saving,
    Saved,
    SaveFailed
}

[Flags]
public enum ExportFormat
{
    None = 0,
    Markdown = 1,
    Json = 2,
    Csv = 4,
    All = Markdown | Json | Csv
}

public sealed record SavedRecord(
    Guid SavedRecordId,
    Guid ProjectId,
    string ProjectDisplayName,
    string Scope,
    DateTimeOffset CreatedUtc,
    int SchemaVersion,
    IReadOnlyDictionary<string, string> SourceHashes,
    IReadOnlyList<string> ToolVersions,
    IReadOnlyDictionary<string, string> ContentManifest,
    ExportFormat SelectedFormats,
    string DestinationIdentity,
    string RecordDirectoryName,
    bool IsStale,
    bool IsComplete);
