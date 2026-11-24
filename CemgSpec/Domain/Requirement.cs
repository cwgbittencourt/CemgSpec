namespace CemgSpec.Domain;

// Contextual key: local IDs like FR-001 are never globally unique across features or projects.
public sealed record RequirementKey(Guid ProjectId, Guid FeatureId, string LocalId);

public sealed record CriterionKey(RequirementKey RequirementKey, string LocalId);

// CoverageState is strictly documentary; Implemented / NotImplemented are forbidden in V1.
public enum CoverageState
{
    Linked,
    MissingLink,
    Ambiguous
}

public sealed record Criterion(
    CriterionKey Key,
    RequirementKey RequirementKey,
    string Text,
    DocumentSourceLocation DocumentLocation,
    IReadOnlyList<string> TaskLinks,
    CoverageState CoverageState);

public sealed record Requirement(
    RequirementKey Key,
    Guid SourceDocumentId,
    DocumentSourceLocation SourceLocation,
    string Description,
    IReadOnlyList<string> StoryRefs,
    IReadOnlyList<Criterion> Criteria,
    bool HasDuplicateDefinition);

