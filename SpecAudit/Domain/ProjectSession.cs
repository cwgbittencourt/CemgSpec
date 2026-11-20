namespace SpecAudit.Domain;
public enum VerificationType { AnalyzeDocuments, CheckGate }
public enum CapabilityAvailability { Available, PendingValidation, Unsupported, Unavailable }
public enum RunState { Queued, Preparing, Running, Completed, Failed, Cancelled, Blocked }
public enum Freshness { Current, Stale }
public enum CatalogKind { Constitution, Spec, Plan, Tasks, Checklist, Research, Design, GateRecord, EvidenceDocument, PresentationAsset }
public sealed record Feature(Guid FeatureId,Guid ProjectId,string SpecKitRelativeDirectory,string Title);
public sealed class ProjectSession
{
    private readonly Dictionary<Guid,Feature> features=[];
    public Guid ProjectId { get; }=Guid.NewGuid();
    public Guid OwnerSessionId { get; }
    public string DisplayName { get; }
    public string OriginHandleIdentity { get; }
    public string OriginDisplayPath { get; }
    public string OutputDirectoryIdentity { get; private set; }
    public DateTimeOffset CreatedUtc { get; }=DateTimeOffset.UtcNow;
    public Guid? ActiveFeatureId { get; private set; }
    public VerificationType SelectedVerification { get; private set; }=VerificationType.AnalyzeDocuments;
    public Guid? SelectedAgentId { get; private set; }
    public long Generation { get; private set; }
    public IReadOnlyList<Feature> Features=>features.Values.ToArray();
    public ProjectSession(Guid owner,string name,string origin,string display,string output)
    {
        if(owner==Guid.Empty)throw new ArgumentException("Owner session is required.",nameof(owner));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);ArgumentException.ThrowIfNullOrWhiteSpace(origin);ArgumentException.ThrowIfNullOrWhiteSpace(output);
        if(string.Equals(origin,output,StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Origin and destination identities cannot coincide.");
        OwnerSessionId=owner;DisplayName=name;OriginHandleIdentity=origin;OriginDisplayPath=display;OutputDirectoryIdentity=output;
    }
    public bool IsOwnedBy(Guid id)=>id!=Guid.Empty && OwnerSessionId==id;
    public Feature AddFeature(string path,string title,Guid? featureId=null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);ArgumentException.ThrowIfNullOrWhiteSpace(title);
        var segments=path.Replace('\\','/').Split('/');
        if(Path.IsPathRooted(path) || path.Contains(':') || segments.Any(s=>s is "" or "." or ".."))throw new ArgumentException("Feature must be a catalog-relative directory.",nameof(path));
        if(features.Values.Any(f=>string.Equals(f.SpecKitRelativeDirectory,path,StringComparison.OrdinalIgnoreCase)))throw new ArgumentException("Duplicate feature directory.",nameof(path));
        var id=featureId??Guid.NewGuid();if(id==Guid.Empty || features.ContainsKey(id))throw new ArgumentException("Invalid feature identity.");
        var feature=new Feature(id,ProjectId,path,title);features.Add(feature.FeatureId,feature);ActiveFeatureId??=feature.FeatureId;return feature;
    }
    internal void SynchronizeFeatures(IReadOnlyDictionary<Guid,string> catalogFeatures)
    {
        foreach(var id in features.Keys.Where(id=>!catalogFeatures.ContainsKey(id)).ToArray())features.Remove(id);
        if(ActiveFeatureId is {} active && !features.ContainsKey(active))ActiveFeatureId=null;
        foreach(var feature in catalogFeatures)
            if(!features.ContainsKey(feature.Key))AddFeature(feature.Value,Path.GetFileName(feature.Value),feature.Key);
        ActiveFeatureId??=features.Keys.Cast<Guid?>().FirstOrDefault();
    }
    public Feature RequireFeature(Guid id)=>features.TryGetValue(id,out var feature) ? feature : throw new KeyNotFoundException("Feature does not belong to this project.");
    public void SelectFeature(Guid id){RequireFeature(id);ActiveFeatureId=id;}
    public void SelectVerification(VerificationType value)
    {
        if(!Enum.IsDefined(value))throw new ArgumentOutOfRangeException(nameof(value));SelectedVerification=value;
    }
    public void SelectAgent(Guid? agentId)
    {
        if(agentId==Guid.Empty)throw new ArgumentException("AgentId cannot be empty Guid.",nameof(agentId));
        SelectedAgentId=agentId;
    }
    public void ChangeOutputIdentity(string identity){ArgumentException.ThrowIfNullOrWhiteSpace(identity);if(identity==OriginHandleIdentity)throw new ArgumentException("Output cannot coincide with origin.");OutputDirectoryIdentity=identity;}
    public long Invalidate()=>Generation=checked(Generation+1);
}
