namespace SpecAudit.Domain;

public enum FindingSeverity
{
    High,
    Medium,
    Low,
    Info
}

// Technical finding or documentary gap; never grants human acceptance.
public sealed record Finding(
    Guid FindingId,
    Guid? RunId,
    string Category,
    FindingSeverity Severity,
    string Explanation,
    IReadOnlyList<DocumentSourceLocation> SourceLocations,
    string? Suggestion,
    string ContextFingerprint);

