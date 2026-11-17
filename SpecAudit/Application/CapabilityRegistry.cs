using SpecAudit.Domain;
namespace SpecAudit.Application;
public enum ProtectedCapability { ReaderBoundary,GateAdapter,AnalyzeCodex,AnalyzeAntigravity,ExternalWriter }
public sealed class CapabilityRegistry
{
    private readonly Dictionary<ProtectedCapability,ProtectionAssessment> assessments=[];
    private static readonly IReadOnlyDictionary<ProtectedCapability,string[]> Required=new Dictionary<ProtectedCapability,string[]>
    {
        [ProtectedCapability.ReaderBoundary]=["authorized-read","code-config-denied","junction-denied","application-read-only","user-edit-preserved","origin-intact"],
        [ProtectedCapability.GateAdapter]=["package-integrity","offline-no-shell","core-equivalence","nested-evidence","read-time-toctou","cancel-zero-writes"],
        [ProtectedCapability.AnalyzeCodex]=["real-analysis","document-contract","configured-tools-and-disclosure","external-state","descendants-cancel"],
        [ProtectedCapability.AnalyzeAntigravity]=["real-analysis","document-contract","configured-tools-and-disclosure","external-state","descendants-cancel"],
        [ProtectedCapability.ExternalWriter]=["explicit-save","external-canonical-path","create-new","reparse-toctou","partial-failure","history"]
    };
    internal void RecordProof(ProtectedCapability capability,ProtectionAssessment assessment)
    {
        if(!Required.TryGetValue(capability,out var checks))throw new ArgumentOutOfRangeException(nameof(capability));
        if(assessment.State!=AssessmentState.Passed || checks.Any(c=>!assessment.Checks.TryGetValue(c,out var passed)||!passed))
            throw new InvalidOperationException("Required capability proofs are incomplete.");
        assessments[capability]=assessment;
    }
    public CapabilityAvailability Availability(ProtectedCapability capability,string version,string platform,string policy)
    {
        if(!Required.ContainsKey(capability))throw new ArgumentOutOfRangeException(nameof(capability));
        return assessments.TryGetValue(capability,out var assessment) && assessment.Allows(version,platform,policy) ? CapabilityAvailability.Available : CapabilityAvailability.PendingValidation;
    }
    public void Invalidate(ProtectedCapability capability){if(assessments.TryGetValue(capability,out var proof))proof.Invalidate();}
}
