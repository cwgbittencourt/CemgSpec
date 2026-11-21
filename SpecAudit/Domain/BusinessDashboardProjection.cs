namespace SpecAudit.Domain;

public sealed record UserStoryStatusRow(
    string StoryId,
    string Title,
    bool IsDeferred,
    int CompletedTasks,
    int TotalTasks,
    string DevValidationState,
    string PoAcceptanceState,
    Guid? ValidationDocId,
    Guid? AcceptanceDocId)
{
    public double ProgressPercentage => TotalTasks > 0 ? Math.Round(100.0 * CompletedTasks / TotalTasks, 1) : 0;
    public bool IsTechnicallyReady => !IsDeferred && TotalTasks > 0 && CompletedTasks == TotalTasks;
}

public sealed record EvolutionDelta(
    string BaselineLabel,
    DateTimeOffset BaselineUtc,
    double PreviousProgressPct,
    double CurrentProgressPct,
    int PreviousBlockers,
    int CurrentBlockers,
    int PreviousReadyGates,
    int CurrentReadyGates);

public sealed record BusinessDashboardProjection(
    string ProjectDisplayName,
    string ScopeLabel,
    int ReadyStories,
    int ActiveStories,
    int PendingDevCount,
    int PendingPoCount,
    int ActiveBlockersCount,
    int OpenGapsCount,
    int ReadyGatesCount,
    IReadOnlyList<UserStoryStatusRow> Stories,
    EvolutionDelta? Evolution);
