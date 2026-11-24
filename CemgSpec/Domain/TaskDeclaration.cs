namespace CemgSpec.Domain;

public sealed record TaskKey(Guid ProjectId, Guid FeatureId, string LocalId);

public sealed record DocumentSourceLocation(Guid DocumentId, string RelativePath, int LineNumber);

// Checkbox state is strictly a documentary declaration; it never proves implementation.
public sealed record TaskDeclaration(
    TaskKey Key,
    string Text,
    bool MarkedComplete,
    DocumentSourceLocation SourceLocation,
    IReadOnlyList<string> ExplicitRequirementRefs,
    string? Phase,
    string? StoryRef);

