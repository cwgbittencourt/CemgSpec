namespace SpecAudit.Domain;

public enum GateStage
{
    Spec,
    Plan,
    Tasks,
    Pr,
    Acceptance,
    DevImpossibility
}

public enum RevisionSource
{
    ArtifactSha256,
    UserConfirmedExternal
}

public static class GateStageExtensions
{
    public static string ToCliName(this GateStage stage) => stage switch
    {
        GateStage.Spec => "spec",
        GateStage.Plan => "plan",
        GateStage.Tasks => "tasks",
        GateStage.Pr => "pr",
        GateStage.Acceptance => "acceptance",
        GateStage.DevImpossibility => "dev-impossibility",
        _ => throw new ArgumentOutOfRangeException(nameof(stage))
    };

    public static bool TryParseCliName(string? value, out GateStage stage)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "spec": stage = GateStage.Spec; return true;
            case "plan": stage = GateStage.Plan; return true;
            case "tasks": stage = GateStage.Tasks; return true;
            case "pr": stage = GateStage.Pr; return true;
            case "acceptance": stage = GateStage.Acceptance; return true;
            case "dev-impossibility": stage = GateStage.DevImpossibility; return true;
            default: stage = default; return false;
        }
    }
}

public sealed class GateCandidate
{
    public Guid CandidateId { get; } = Guid.NewGuid();
    public Guid ProjectId { get; }
    public Guid FeatureId { get; }
    public GateStage Stage { get; }
    public Guid RecordDocumentId { get; }
    public string ExpectedRevision { get; }
    public RevisionSource RevisionSource { get; }
    public string RevisionSourceLabel { get; }
    public IReadOnlyList<Guid> EvidenceDocumentIds { get; }
    public long CatalogGeneration { get; }

    public GateCandidate(
        Guid projectId,
        Guid featureId,
        GateStage stage,
        Guid recordDocumentId,
        string expectedRevision,
        RevisionSource revisionSource,
        IReadOnlyList<Guid> evidenceDocumentIds,
        long catalogGeneration,
        string? revisionSourceLabel = null)
    {
        if (projectId == Guid.Empty) throw new ArgumentException("ProjectId is required.", nameof(projectId));
        if (featureId == Guid.Empty) throw new ArgumentException("FeatureId is required.", nameof(featureId));
        if (!Enum.IsDefined(stage)) throw new ArgumentOutOfRangeException(nameof(stage));
        if (recordDocumentId == Guid.Empty) throw new ArgumentException("RecordDocumentId is required.", nameof(recordDocumentId));
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedRevision);
        if (!Enum.IsDefined(revisionSource)) throw new ArgumentOutOfRangeException(nameof(revisionSource));
        if (catalogGeneration < 0) throw new ArgumentOutOfRangeException(nameof(catalogGeneration));
        ArgumentNullException.ThrowIfNull(evidenceDocumentIds);
        if (evidenceDocumentIds.Any(id => id == Guid.Empty))
            throw new ArgumentException("EvidenceDocumentIds cannot contain empty identifiers.", nameof(evidenceDocumentIds));

        ProjectId = projectId;
        FeatureId = featureId;
        Stage = stage;
        RecordDocumentId = recordDocumentId;
        ExpectedRevision = expectedRevision.Trim();
        RevisionSource = revisionSource;
        RevisionSourceLabel = string.IsNullOrWhiteSpace(revisionSourceLabel)
            ? revisionSource == RevisionSource.ArtifactSha256
                ? "SHA-256 derivado do artefato autorizado"
                : "Revisão externa informada explicitamente (não autenticada)"
            : revisionSourceLabel.Trim();
        EvidenceDocumentIds = evidenceDocumentIds.Distinct().ToArray();
        CatalogGeneration = catalogGeneration;
    }
}
