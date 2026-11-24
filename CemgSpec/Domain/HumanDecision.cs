namespace CemgSpec.Domain;

public enum DecisionClassification
{
    Pending,
    Confirmed,
    FalsePositive,
    AcceptedRisk,
    Deferred
}

public sealed class HumanDecision
{
    public Guid DecisionId { get; }
    public Guid FindingId { get; }
    public DecisionClassification Classification { get; }
    public string? Reason { get; }
    public string? Actor { get; }
    public DateTimeOffset TimestampUtc { get; }
    public string ContextFingerprint { get; }
    public bool NeedsReview { get; private set; }

    public HumanDecision(
        Guid findingId,
        DecisionClassification classification,
        string? reason,
        string? actor,
        string contextFingerprint,
        DateTimeOffset? timestampUtc = null,
        Guid? decisionId = null,
        bool needsReview = false)
    {
        if (findingId == Guid.Empty) throw new ArgumentException("FindingId is required.", nameof(findingId));
        if (!Enum.IsDefined(classification)) throw new ArgumentOutOfRangeException(nameof(classification));
        ArgumentException.ThrowIfNullOrWhiteSpace(contextFingerprint);

        if (classification is DecisionClassification.FalsePositive or DecisionClassification.AcceptedRisk)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Justificativa obrigatória para Falso positivo ou Risco aceito.", nameof(reason));
            if (string.IsNullOrWhiteSpace(actor))
                throw new ArgumentException("Responsável obrigatório para Falso positivo ou Risco aceito.", nameof(actor));
        }

        DecisionId = decisionId is { } id && id != Guid.Empty ? id : Guid.NewGuid();
        FindingId = findingId;
        Classification = classification;
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        Actor = string.IsNullOrWhiteSpace(actor) ? null : actor.Trim();
        TimestampUtc = timestampUtc ?? DateTimeOffset.UtcNow;
        ContextFingerprint = contextFingerprint.Trim();
        NeedsReview = needsReview;
    }

    public bool EvaluateContextFingerprint(string currentFingerprint)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currentFingerprint);
        if (!string.Equals(ContextFingerprint, currentFingerprint.Trim(), StringComparison.Ordinal))
        {
            NeedsReview = true;
            return true;
        }
        return false;
    }
}
