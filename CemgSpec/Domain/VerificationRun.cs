namespace CemgSpec.Domain;

public enum GateSemanticResult
{
    Ready,
    Blocked,
    DevImpossibilityRecorded
}

public enum AnalyzeSemanticResult
{
    Findings,
    NoFindings,
    Inconclusive
}

public sealed record SemanticOutcome(GateSemanticResult? GateResult, AnalyzeSemanticResult? AnalyzeResult)
{
    public static SemanticOutcome ForGate(GateSemanticResult result) => new(result, null);
    public static SemanticOutcome ForAnalyze(AnalyzeSemanticResult result) => new(null, result);
}

public sealed class VerificationRun
{
    public Guid RunId { get; } = Guid.NewGuid();
    public Guid ProjectId { get; }
    public Guid FeatureId { get; }
    public RequirementKey? FocusRequirementKey { get; }
    public VerificationType Type { get; }
    public Guid? CandidateId { get; }
    public Guid? AgentId { get; }
    public long CatalogGeneration { get; }
    public IReadOnlyList<Guid> InputSnapshotIds { get; private set; } = [];
    public RunState State { get; private set; } = RunState.Queued;
    public Freshness Freshness { get; private set; } = Freshness.Current;
    public bool CancelRequested { get; private set; }
    public bool TerminationConfirmed { get; private set; }
    public SemanticOutcome? SemanticResult { get; private set; }
    public DateTimeOffset CreatedUtc { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedUtc { get; private set; }
    public DateTimeOffset? EndedUtc { get; private set; }
    public int? ExitCode { get; private set; }
    public string ToolIdentifier { get; private set; } = string.Empty;
    public string Output { get; private set; } = string.Empty;
    public string ErrorOutput { get; private set; } = string.Empty;
    public string? Reason { get; private set; }
    public IReadOnlyList<Finding> Findings { get; private set; } = [];
    public IReadOnlyList<string> Limitations { get; private set; } = [];

    public VerificationRun(
        Guid projectId,
        Guid featureId,
        VerificationType type,
        long catalogGeneration,
        Guid? candidateId = null,
        Guid? agentId = null,
        RequirementKey? focusRequirementKey = null)
    {
        if (projectId == Guid.Empty) throw new ArgumentException("ProjectId is required.", nameof(projectId));
        if (featureId == Guid.Empty) throw new ArgumentException("FeatureId is required.", nameof(featureId));
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        if (catalogGeneration < 0) throw new ArgumentOutOfRangeException(nameof(catalogGeneration));

        if (type == VerificationType.CheckGate)
        {
            if (candidateId is null || candidateId == Guid.Empty)
                throw new ArgumentException("CandidateId is required for CheckGate runs.", nameof(candidateId));
            if (agentId is not null)
                throw new ArgumentException("CheckGate runs do not use an AI agent.", nameof(agentId));
        }
        else
        {
            if (agentId is null || agentId == Guid.Empty)
                throw new ArgumentException("AgentId is required for AnalyzeDocuments runs.", nameof(agentId));
            if (candidateId is not null)
                throw new ArgumentException("AnalyzeDocuments runs do not use a GateCandidate.", nameof(candidateId));
        }

        if (focusRequirementKey is not null &&
            (focusRequirementKey.ProjectId != projectId || focusRequirementKey.FeatureId != featureId))
        {
            throw new ArgumentException("FocusRequirementKey must belong to the same project and feature.", nameof(focusRequirementKey));
        }

        ProjectId = projectId;
        FeatureId = featureId;
        Type = type;
        CatalogGeneration = catalogGeneration;
        CandidateId = candidateId;
        AgentId = agentId;
        FocusRequirementKey = focusRequirementKey;
    }

    public void BeginPreparing(IReadOnlyList<Guid> inputSnapshotIds)
    {
        EnsureState(RunState.Queued);
        ArgumentNullException.ThrowIfNull(inputSnapshotIds);
        if (inputSnapshotIds.Any(id => id == Guid.Empty))
            throw new ArgumentException("InputSnapshotIds cannot contain empty identifiers.", nameof(inputSnapshotIds));
        InputSnapshotIds = inputSnapshotIds.Distinct().ToArray();
        State = RunState.Preparing;
    }

    public void BlockPreparation(string reason, IReadOnlyList<string>? limitations = null)
    {
        EnsureState(RunState.Preparing);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        State = RunState.Blocked;
        SemanticResult = null;
        Reason = reason.Trim();
        Limitations = (limitations ?? []).ToArray();
        EndedUtc = DateTimeOffset.UtcNow;
    }

    public void BeginRunning(string toolIdentifier)
    {
        EnsureState(RunState.Preparing);
        ArgumentException.ThrowIfNullOrWhiteSpace(toolIdentifier);
        ToolIdentifier = toolIdentifier.Trim();
        StartedUtc = DateTimeOffset.UtcNow;
        State = RunState.Running;
    }

    public void RequestCancel()
    {
        if (State is not (RunState.Queued or RunState.Preparing or RunState.Running))
            throw new InvalidOperationException($"Cannot request cancellation in state {State}.");
        CancelRequested = true;
    }

    public void CompleteGate(
        GateSemanticResult semanticResult,
        int exitCode,
        string output,
        string errorOutput,
        IReadOnlyList<Finding>? findings = null,
        IReadOnlyList<string>? limitations = null)
    {
        EnsureState(RunState.Running);
        if (Type != VerificationType.CheckGate)
            throw new InvalidOperationException("CompleteGate is only valid for CheckGate runs.");
        if (!Enum.IsDefined(semanticResult))
            throw new ArgumentOutOfRangeException(nameof(semanticResult));

        State = RunState.Completed;
        SemanticResult = SemanticOutcome.ForGate(semanticResult);
        ExitCode = exitCode;
        Output = output ?? string.Empty;
        ErrorOutput = errorOutput ?? string.Empty;
        Findings = (findings ?? []).ToArray();
        Limitations = (limitations ?? []).ToArray();
        TerminationConfirmed = true;
        EndedUtc = DateTimeOffset.UtcNow;
    }

    public void CompleteAnalyze(
        AnalyzeSemanticResult semanticResult,
        int exitCode,
        string output,
        string errorOutput,
        IReadOnlyList<Finding>? findings = null,
        IReadOnlyList<string>? limitations = null)
    {
        EnsureState(RunState.Running);
        if (Type != VerificationType.AnalyzeDocuments)
            throw new InvalidOperationException("CompleteAnalyze is only valid for AnalyzeDocuments runs.");
        if (!Enum.IsDefined(semanticResult))
            throw new ArgumentOutOfRangeException(nameof(semanticResult));

        State = RunState.Completed;
        SemanticResult = SemanticOutcome.ForAnalyze(semanticResult);
        ExitCode = exitCode;
        Output = output ?? string.Empty;
        ErrorOutput = errorOutput ?? string.Empty;
        Findings = (findings ?? []).ToArray();
        Limitations = (limitations ?? []).ToArray();
        TerminationConfirmed = true;
        EndedUtc = DateTimeOffset.UtcNow;
    }

    public void FinishCancellation(
        bool terminationConfirmed,
        string reason,
        int? exitCode = null,
        string output = "",
        string errorOutput = "")
    {
        if (!CancelRequested)
            throw new InvalidOperationException("Cancellation must be requested before finishing cancellation.");
        if (State is not (RunState.Queued or RunState.Preparing or RunState.Running))
            throw new InvalidOperationException($"Cannot finish cancellation in state {State}.");
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        SemanticResult = null;
        ExitCode = exitCode;
        Output = output ?? string.Empty;
        ErrorOutput = errorOutput ?? string.Empty;
        TerminationConfirmed = terminationConfirmed;
        EndedUtc = DateTimeOffset.UtcNow;

        if (terminationConfirmed)
        {
            State = RunState.Cancelled;
            Reason = reason.Trim();
        }
        else
        {
            State = RunState.Failed;
            Reason = $"{reason.Trim()} (término de processos não confirmado; novas execuções da integração bloqueadas)";
        }
    }

    public void MarkFailed(
        string reason,
        int? exitCode = null,
        string output = "",
        string errorOutput = "",
        bool terminationConfirmed = true,
        IReadOnlyList<string>? limitations = null)
    {
        if (State is not (RunState.Preparing or RunState.Running))
            throw new InvalidOperationException($"Cannot mark failure in state {State}.");
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        State = RunState.Failed;
        SemanticResult = null;
        Reason = reason.Trim();
        ExitCode = exitCode;
        Output = output ?? string.Empty;
        ErrorOutput = errorOutput ?? string.Empty;
        TerminationConfirmed = terminationConfirmed;
        Limitations = (limitations ?? []).ToArray();
        EndedUtc = DateTimeOffset.UtcNow;
    }

    public void MarkStale()
    {
        Freshness = Freshness.Stale;
    }

    private void EnsureState(RunState expected)
    {
        if (State != expected)
            throw new InvalidOperationException($"Expected run state {expected}, actual {State}.");
    }
}
