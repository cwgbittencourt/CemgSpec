namespace SpecAudit.Domain;
public enum AssessmentState { NotVerified, Passed, Failed, Stale }
public sealed class ProtectionAssessment
{
    public Guid AssessmentId { get; }=Guid.NewGuid();
    public string ComponentVersion { get; }
    public string PlatformFingerprint { get; }
    public string CapabilityDigest { get; }
    public AssessmentState State { get; private set; }=AssessmentState.NotVerified;
    private Dictionary<string,bool> checks=[];
    private string[] evidence=[];
    public IReadOnlyDictionary<string,bool> Checks=>new System.Collections.ObjectModel.ReadOnlyDictionary<string,bool>(checks);
    public IReadOnlyList<string> EvidenceRefs=>Array.AsReadOnly(evidence);
    public ProtectionAssessment(string version,string platform,string policy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);ArgumentException.ThrowIfNullOrWhiteSpace(platform);ArgumentException.ThrowIfNullOrWhiteSpace(policy);
        ComponentVersion=version;PlatformFingerprint=platform;CapabilityDigest=policy;
    }
    public bool Allows(string version,string platform,string policy)=>State==AssessmentState.Passed && ComponentVersion==version && PlatformFingerprint==platform && CapabilityDigest==policy;
    // Only a trusted proof runner may call this; this model cannot authenticate evidence.
    public void RecordPassed(IReadOnlyDictionary<string,bool> results,IReadOnlyList<string> references)
    {
        if(results.Count==0 || results.Any(c=>string.IsNullOrWhiteSpace(c.Key)||!c.Value) || references.Count==0 || references.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("Every required proof must pass and identify its evidence.");
        checks=new(results);evidence=references.ToArray();State=AssessmentState.Passed;
    }
    public void RecordFailed(IReadOnlyDictionary<string,bool> results,IReadOnlyList<string> references)
    {checks=new(results);evidence=references.ToArray();State=AssessmentState.Failed;}
    public void Invalidate()=>State=AssessmentState.Stale;
}
