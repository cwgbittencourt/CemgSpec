namespace CemgSpec.Domain;

public enum LinkProvenance
{
    Explicit,
    Inferred
}

public enum LinkResolution
{
    Authorized,
    Missing,
    Ambiguous,
    OutOfScope
}

// References to .cs/.razor/config files stay OutOfScope and are never opened in V1.
public sealed record EvidenceLink(
    Guid LinkId,
    DocumentSourceLocation SourceLocation,
    Guid? TargetDocumentId,
    string TargetText,
    string Relation,
    LinkProvenance Provenance,
    LinkResolution Resolution,
    string? Explanation = null);

