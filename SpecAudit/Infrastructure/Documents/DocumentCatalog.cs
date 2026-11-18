using SpecAudit.Domain;
using SpecAudit.Infrastructure.Execution;
using Microsoft.Win32.SafeHandles;
namespace SpecAudit.Infrastructure.Documents;
public sealed class DocumentAccessException(string code) : Exception(code){public string Code {get;}=code;}
public sealed class DocumentCatalog : IDisposable
{
    private readonly SafeFileHandle originHandle;
    internal readonly Dictionary<Guid,CatalogEntry> Index=[];
    internal readonly object SyncRoot=new();
    private Dictionary<string,Guid> documentIds=new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string,Guid> featureIds=new(StringComparer.OrdinalIgnoreCase);
    private int pending;
    private long changeRevision;
    public long ChangeRevision=>Interlocked.Read(ref changeRevision);
    public bool HasPendingChanges=>Volatile.Read(ref pending)!=0;
    public IReadOnlyDictionary<Guid,string> Features {get{lock(SyncRoot)return featureIds.ToDictionary(p=>p.Value,p=>p.Key);}}
    public Guid ProjectId { get; }
    public string OriginIdentity { get; }
    public string FinalOriginPath { get; }
    public long Generation { get; private set; }
    public IReadOnlyList<CatalogEntry> Entries {get{lock(SyncRoot)return Index.Values.ToArray();}}
    internal ExecutionLimits Limits { get; }=new();
    public DocumentCatalog(Guid projectId,string origin)
    {
        if(projectId==Guid.Empty)throw new ArgumentException("Project identity is required.");
        ProjectId=projectId;originHandle=WindowsReadHandle.Open(Path.GetFullPath(origin),true);
        OriginIdentity=WindowsReadHandle.Info(originHandle).Identity;FinalOriginPath=WindowsReadHandle.FinalPath(originHandle);
        try{Build();}catch{originHandle.Dispose();throw;}
    }
    private void Build()
    {
        Add(".specify/memory/constitution.md",CatalogKind.Constitution,null);
        var specs=Path.Combine(FinalOriginPath,"specs");
        foreach(var feature in IsPlainDirectory(specs)?Directory.EnumerateDirectories(specs):Array.Empty<string>())
        {
            if(!IsPlainDirectory(feature) || !System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileName(feature),@"^\d{3}-[A-Za-z0-9_-]+$"))continue;
            var relative=Path.GetRelativePath(FinalOriginPath,feature);
            if(!featureIds.TryGetValue(relative,out var id)){id=Guid.NewGuid();featureIds[relative]=id;}
            foreach(var (name,kind) in new[]{("spec.md",CatalogKind.Spec),("plan.md",CatalogKind.Plan),("tasks.md",CatalogKind.Tasks),("research.md",CatalogKind.Research),("data-model.md",CatalogKind.Design),("quickstart.md",CatalogKind.Design)})Add(Path.Combine(relative,name),kind,id);
            foreach(var folder in new[]{"checklists","contracts","design","evidence","verification","validation","acceptance"})
                Walk(Path.Combine(feature,folder),folder,id,0);
        }
        var broker=new DocumentBroker(this);
        foreach(var entry in Index.Values.ToArray())
        {
            var snapshot=broker.Read(ProjectId,entry.DocumentId,Generation);
            Index[entry.DocumentId]=entry with{ContentHash=snapshot.ContentHash};
        }
    }
    private static bool IsPlainDirectory(string path)=>Directory.Exists(path) && !File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint);
    private void Walk(string directory,string context,Guid feature,int depth)
    {
        if(!IsPlainDirectory(directory))return;Limits.CheckGraph(Index.Count,depth);
        foreach(var file in Directory.EnumerateFiles(directory))
        {
            var extension=Path.GetExtension(file).ToLowerInvariant();
            var asset=context=="design" && extension is ".png" or ".jpg" or ".jpeg" or ".webp" or ".svg";
            if(!asset && extension is not (".md" or ".json" or ".txt" or ".trx"))continue;
            var name=Path.GetFileName(file);
            if(new[]{"package.json","appsettings.json","launchSettings.json"}.Contains(name,StringComparer.OrdinalIgnoreCase))continue;
            Add(Path.GetRelativePath(FinalOriginPath,file),asset?CatalogKind.PresentationAsset:context=="verification"?CatalogKind.GateRecord:context=="checklists"?CatalogKind.Checklist:CatalogKind.EvidenceDocument,feature);
        }
        foreach(var child in Directory.EnumerateDirectories(directory))Walk(child,context,feature,depth+1);
    }
    private void Add(string relative,CatalogKind kind,Guid? feature)
    {
        var path=Path.GetFullPath(Path.Combine(FinalOriginPath,relative));
        relative=Path.GetRelativePath(FinalOriginPath,path);
        if(!File.Exists(path))return;
        if(File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint))return;
        // Each ancestor is also checked; enumeration must never traverse a junction.
        var ancestor=Path.GetDirectoryName(path);
        while(ancestor is not null && !string.Equals(ancestor,FinalOriginPath,StringComparison.OrdinalIgnoreCase))
        {if(!IsPlainDirectory(ancestor))return;ancestor=Path.GetDirectoryName(ancestor);}
        using var handle=WindowsReadHandle.Open(path,false);var info=WindowsReadHandle.Info(handle);
        if(info.Links!=1 || !string.Equals(WindowsReadHandle.FinalPath(handle),path,StringComparison.OrdinalIgnoreCase))return;
        Limits.CheckDocument(info.Length);Limits.CheckGraph(Index.Count+1,0);
        if(!documentIds.TryGetValue(relative,out var id)){id=Guid.NewGuid();documentIds[relative]=id;}Index[id]=new(id,ProjectId,feature,relative,kind,info.Identity,"",info.Length,DateTimeOffset.FromFileTime(info.WriteStamp),Generation);
    }
    internal void ConfirmOrigin()
    {
        using var current=WindowsReadHandle.Open(FinalOriginPath,true);
        if(WindowsReadHandle.Info(current).Identity!=OriginIdentity || WindowsReadHandle.Info(originHandle).Identity!=OriginIdentity)throw new DocumentAccessException("Changed");
    }
    public void MarkChanged(){Interlocked.Increment(ref changeRevision);Interlocked.Exchange(ref pending,1);}
    public void Refresh()
    {
        lock(SyncRoot)
        {
            var revision=ChangeRevision;Generation=checked(Generation+1);var previous=Index.ToDictionary();var oldFeatures=featureIds.ToDictionary(p=>p.Key,p=>p.Value,StringComparer.OrdinalIgnoreCase);
            Index.Clear();Interlocked.Exchange(ref pending,0);
            try{ConfirmOrigin();Build();if(revision!=ChangeRevision)throw new DocumentAccessException("Changed");var surviving=Index.Values.Where(e=>e.FeatureId.HasValue).Select(e=>e.FeatureId!.Value).ToHashSet();foreach(var key in featureIds.Where(p=>!surviving.Contains(p.Value)&&!Directory.Exists(Path.Combine(FinalOriginPath,p.Key))).Select(p=>p.Key).ToArray())featureIds.Remove(key);}
            catch{Index.Clear();foreach(var pair in previous)Index[pair.Key]=pair.Value;featureIds=oldFeatures;MarkChanged();throw;}
        }
    }
    public void Dispose()=>originHandle.Dispose();
}
