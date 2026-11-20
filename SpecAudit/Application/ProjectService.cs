using SpecAudit.Domain;
using SpecAudit.Infrastructure.Documents;
using SpecAudit.Infrastructure.Projects;
namespace SpecAudit.Application;
public sealed class DocumentVersion
{
    public Guid ProjectId {get;}
    public long Generation {get;}
    public long Revision {get;}
    public CancellationToken Cancellation {get;}
    internal DocumentVersion(Guid projectId,long generation,long revision,CancellationToken cancellation)
    {ProjectId=projectId;Generation=generation;Revision=revision;Cancellation=cancellation;}
}
public sealed class OpenProjectContext : IDisposable
{
    public ProjectSession Session {get;}
    public DocumentCatalog Catalog {get;}
    public string OutputDisplayPath {get;internal set;}
    public DocumentChangeMonitor Monitor {get;}
    public string? LastError {get;internal set;}
    private readonly object workLock=new();
    private CancellationTokenSource work=new();
    private bool disposed;
    internal CancellationToken WorkToken {get{lock(workLock)return work.Token;}}
    internal void CancelWork()
    {
        CancellationTokenSource old;
        lock(workLock){if(disposed)return;old=work;work=new();}
        // Cancel outside the lock: work continuations may call back into this context.
        try{old.Cancel();}finally{old.Dispose();}
    }
    private void OnDocumentChanged(){Catalog.MarkChanged();CancelWork();}
    internal OpenProjectContext(ProjectSession session,DocumentCatalog catalog,string output)
    {Session=session;Catalog=catalog;OutputDisplayPath=output;Monitor=new(catalog.FinalOriginPath,path=>catalog.Entries.Any(e=>e.RelativePath.StartsWith(path+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)));Monitor.Changed+=OnDocumentChanged;}
    public void Dispose()
    {
        CancellationTokenSource old;
        lock(workLock){if(disposed)return;disposed=true;old=work;}
        Monitor.Changed-=OnDocumentChanged;Monitor.Dispose();
        try{old.Cancel();}finally{old.Dispose();Catalog.Dispose();}
    }
}
public sealed class ProjectService(LocalSessionService localSession,IFolderPicker picker,RecentProjectStore? recentStore=null) : IDisposable
{
    private readonly Dictionary<Guid,OpenProjectContext> projects=[];
    private int folderSelectionActive;
    public OpenProjectContext? Active {get;private set;}
    public IReadOnlyList<OpenProjectContext> Projects=>projects.Values.ToArray();
    public IReadOnlyList<RecentProjectEntry> RecentProjects=>recentStore?.List() ?? [];
    public event Action? Changed;
    public async Task<string?> PickProjectOriginAsync()=>await PickDirectoryAsync("Selecionar PROJETO (origem somente leitura)");
    public async Task<string?> PickReportDestinationAsync()=>await PickDirectoryAsync("Selecionar RELATORIOS (destino externo)");
    private async Task<string?> PickDirectoryAsync(string title)
    {
        if(Interlocked.CompareExchange(ref folderSelectionActive,1,0)!=0)throw new InvalidOperationException("FolderSelectionInProgress");
        try
        {
            var selected=await picker.PickAsync(title);if(selected is null)return null;
            using var handle=WindowsReadHandle.Open(Path.GetFullPath(selected),true);
            return WindowsReadHandle.FinalPath(handle);
        }
        finally{Volatile.Write(ref folderSelectionActive,0);}
    }
    public async Task<OpenProjectContext?> OpenProjectAsync()
    {
        if(Interlocked.CompareExchange(ref folderSelectionActive,1,0)!=0)throw new InvalidOperationException("FolderSelectionInProgress");
        try
        {
            var origin=await picker.PickAsync("1 de 2: selecionar PROJETO (origem somente leitura)");if(origin is null)return null;
            var output=await picker.PickAsync("2 de 2: selecionar RELATORIOS (destino externo)");if(output is null)return null;
            return OpenSelected(origin,output);
        }
        finally{Volatile.Write(ref folderSelectionActive,0);}
    }
    internal OpenProjectContext OpenSelected(string origin,string output)
    {
        using var originHandle=WindowsReadHandle.Open(Path.GetFullPath(origin),true);var originPath=WindowsReadHandle.FinalPath(originHandle);
        var destination=ValidateOutput(originPath,output);
        var identity=WindowsReadHandle.Info(originHandle).Identity;
        var existing=projects.Values.FirstOrDefault(p=>p.Session.OriginHandleIdentity==identity);
        if(existing is not null)
        {
            existing.Session.ChangeOutputIdentity(destination.Identity);existing.OutputDisplayPath=destination.Path;
            recentStore?.Upsert(existing.Session.DisplayName,originPath,destination.Path);
            SelectProject(existing.Session.ProjectId);return existing;
        }
        var session=new ProjectSession(localSession.SessionId,Path.GetFileName(originPath.TrimEnd(Path.DirectorySeparatorChar)),WindowsReadHandle.Info(originHandle).Identity,originPath,destination.Identity);
        var catalog=new DocumentCatalog(session.ProjectId,originPath);OpenProjectContext context;
        try{context=new(session,catalog,destination.Path);}catch{catalog.Dispose();throw;}
        foreach(var feature in catalog.Features)session.AddFeature(feature.Value,Path.GetFileName(feature.Value),feature.Key);
        context.Monitor.Changed+=()=>Changed?.Invoke();
        Active?.CancelWork();projects.Add(session.ProjectId,context);Active=context;
        recentStore?.Upsert(session.DisplayName,originPath,destination.Path);
        Changed?.Invoke();return context;
    }
    public OpenProjectContext ReopenRecent(Guid recentId)
    {
        var entry=recentStore?.GetById(recentId) ?? throw new InvalidOperationException("RecentProjectNotFound");
        return OpenSelected(entry.OriginPath,entry.OutputPath);
    }
    public bool RemoveRecent(Guid recentId)
    {
        var removed=recentStore?.Remove(recentId) ?? false;
        if(removed)Changed?.Invoke();
        return removed;
    }
    public OpenProjectContext RequireProject(Guid id)=>projects.TryGetValue(id,out var project)&&project.Session.IsOwnedBy(localSession.SessionId)?project:throw new KeyNotFoundException("Project does not belong to the local session.");
    public void SelectProject(Guid id){var next=RequireProject(id);if(Active!=next)Active?.CancelWork();Active=next;Changed?.Invoke();}
    public async Task ChangeOutputDirectoryAsync(Guid id)
    {
        if(Interlocked.CompareExchange(ref folderSelectionActive,1,0)!=0)throw new InvalidOperationException("FolderSelectionInProgress");
        try{var output=await picker.PickAsync("Selecionar RELATORIOS (novo destino externo)");if(output is not null)ChangeOutputSelected(id,output);}
        finally{Volatile.Write(ref folderSelectionActive,0);}
    }
    internal void ChangeOutputSelected(Guid id,string output)
    {
        var context=RequireProject(id);var destination=ValidateOutput(context.Catalog.FinalOriginPath,output);
        context.Session.ChangeOutputIdentity(destination.Identity);context.OutputDisplayPath=destination.Path;
        recentStore?.Upsert(context.Session.DisplayName,context.Catalog.FinalOriginPath,destination.Path);
        Changed?.Invoke();
    }
    // A prepared operation must retain this version and validate it at each boundary.
    // Actual Prepare/Execute/Save workflows belong to subsequent tasks.
    public DocumentVersion CaptureDocumentVersion(Guid projectId)
    {
        var project=RequireProject(projectId);
        var version=new DocumentVersion(projectId,project.Catalog.Generation,project.Catalog.ChangeRevision,project.WorkToken);
        EnsureCurrent(version);return version;
    }
    public void EnsureCurrent(DocumentVersion version)
    {
        var project=RequireProject(version.ProjectId);
        if(project!=Active || version.Cancellation.IsCancellationRequested || project.Catalog.HasPendingChanges ||
            version.Generation!=project.Catalog.Generation || version.Revision!=project.Catalog.ChangeRevision)
            throw new DocumentAccessException("Changed");
        // Revalidate actual inputs as well: watcher delivery is asynchronous.
        foreach(var entry in project.Catalog.Entries)
            new DocumentBroker(project.Catalog).Read(version.ProjectId,entry.DocumentId,version.Generation);
        if(version.Cancellation.IsCancellationRequested || project.Catalog.HasPendingChanges ||
            version.Generation!=project.Catalog.Generation || version.Revision!=project.Catalog.ChangeRevision)
            throw new DocumentAccessException("Changed");
    }
    public DocumentSnapshot ReadDocument(Guid projectId,Guid documentId)
    {var project=RequireProject(projectId);return new DocumentBroker(project.Catalog).Read(projectId,documentId,project.Catalog.Generation);}
    public DocumentPreviewSession OpenPreview(Guid projectId, Guid documentId)
    {
        var project = RequireProject(projectId);
        var lifetime = project.WorkToken;
        if (project != Active) throw new DocumentAccessException("OutOfScope");
        return new DocumentBroker(project.Catalog).OpenPreview(documentId, lifetime);
    }

    public void Refresh(Guid projectId)
    {
        var project=RequireProject(projectId);
        try{project.CancelWork();project.Catalog.Refresh();project.Session.SynchronizeFeatures(project.Catalog.Features);project.Session.Invalidate();project.LastError=null;}
        catch(DocumentAccessException error){project.LastError=error.Code;throw;}
        finally{Changed?.Invoke();}
    }
    private static (string Path,string Identity) ValidateOutput(string origin,string candidate)
    {
        using var handle=WindowsReadHandle.Open(Path.GetFullPath(candidate),true);var path=WindowsReadHandle.FinalPath(handle);
        if(string.Equals(origin,path,StringComparison.OrdinalIgnoreCase)||WindowsReadHandle.Contains(origin,path))throw new DocumentAccessException("DestinationInsideOrigin");
        return(path,WindowsReadHandle.Info(handle).Identity);
    }
    public void Dispose(){foreach(var project in projects.Values)project.Dispose();projects.Clear();}
}
