namespace SpecAudit.Domain;

public sealed record RecentProjectEntry(
    Guid Id,
    string DisplayName,
    string OriginPath,
    string OutputPath,
    DateTimeOffset LastOpenedUtc);
