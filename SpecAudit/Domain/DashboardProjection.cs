namespace SpecAudit.Domain;

public enum MetricState
{
    Measured,
    NoItems
}

public sealed record DocumentaryMetric(int Numerator, int Denominator, string RuleDescription)
{
    public MetricState State => Denominator == 0 ? MetricState.NoItems : MetricState.Measured;
    public double? Percentage => Denominator == 0
        ? null
        : Math.Round(100.0 * Numerator / Denominator, 1, MidpointRounding.AwayFromZero);
    public string DisplayPercentage => Denominator == 0 ? "Sem itens" : $"{Percentage:0.#}%";
    public string DisplayRatio => Denominator == 0 ? "0 / 0" : $"{Numerator} / {Denominator}";
}

public sealed record RecordedGateState(
    Guid SourceDocumentId,
    string SourceRelativePath,
    Guid FeatureId,
    string FeatureTitle,
    string Stage,
    string RecordedState,
    string? Revision,
    IReadOnlyList<string> Blockers,
    bool HasUnverifiedCodeReference,
    string ProvenanceNote,
    Freshness Freshness);

public enum DocumentaryOccurrenceSeverity
{
    High,
    Medium,
    Low,
    Info
}

public enum DocumentaryOccurrenceCategory
{
    RecordedBlocker,
    RequirementWithoutTask,
    TaskWithoutRequirement,
    DuplicateRequirementId,
    DuplicateTaskId,
    UnverifiedCodeReference,
    InvalidReference,
    MissingArtifact
}

public sealed record DocumentaryOccurrence(
    Guid OccurrenceId,
    Guid FeatureId,
    string FeatureTitle,
    DocumentaryOccurrenceCategory Category,
    DocumentaryOccurrenceSeverity Severity,
    string Title,
    string Explanation,
    DocumentSourceLocation SourceLocation,
    string? Phase,
    string? StoryRef,
    string Situation);

public sealed record DocumentaryRequirementSummary(
    Guid FeatureId,
    string FeatureTitle,
    string LocalId,
    string Description,
    DocumentSourceLocation SourceLocation,
    IReadOnlyList<string> ExplicitTaskIds,
    bool HasDuplicateDefinition,
    string? StoryRef);

public sealed record DashboardFilter(
    Guid? FeatureId = null,
    string? StoryRef = null,
    string? Phase = null,
    DocumentaryOccurrenceSeverity? Severity = null,
    string? Situation = null);

// Documentary projection only. No implementation score or source-code analysis exists in V1.
public sealed record DashboardProjection(
    Guid ProjectId,
    Guid? FeatureId,
    string ScopeLabel,
    long CatalogGeneration,
    Freshness Freshness,
    DocumentaryMetric DeclaredTaskProgress,
    DocumentaryMetric DocumentaryCoverage,
    int CompletedTaskCount,
    int TaskCount,
    int ExplicitlyLinkedRequirementCount,
    int RequirementCount,
    IReadOnlyList<TaskDeclaration> Tasks,
    IReadOnlyList<DocumentaryRequirementSummary> Requirements,
    IReadOnlyList<RecordedGateState> RecordedGateStates,
    IReadOnlyList<DocumentaryOccurrence> Occurrences,
    IReadOnlyList<string> AvailablePhases,
    IReadOnlyList<string> AvailableStories,
    IReadOnlyList<string> Limitations);

