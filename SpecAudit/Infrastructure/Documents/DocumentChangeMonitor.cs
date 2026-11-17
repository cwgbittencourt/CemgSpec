namespace SpecAudit.Infrastructure.Documents;
public sealed class DocumentChangeMonitor : IDisposable
{
    private readonly FileSystemWatcher watcher;
    private readonly string origin;
    private readonly Func<string,bool> containsDocuments;
    private int disposed;
    public event Action? Changed;
    public DocumentChangeMonitor(string origin,Func<string,bool>? containsDocuments=null)
    {
        this.origin=Path.GetFullPath(origin);this.containsDocuments=containsDocuments??(_=>false);
        if(File.GetAttributes(this.origin).HasFlag(FileAttributes.ReparsePoint))throw new DocumentAccessException("OutOfScope");
        // Observe names/metadata only. Content is read exclusively by the broker.
        // Watching the root also detects documentary directories created after opening.
        watcher=new FileSystemWatcher(this.origin){IncludeSubdirectories=true,NotifyFilter=NotifyFilters.FileName|NotifyFilters.DirectoryName|NotifyFilters.LastWrite|NotifyFilters.Size};
        watcher.Changed+=Notify;watcher.Created+=Notify;watcher.Deleted+=Notify;
        watcher.Renamed+=(_,args)=>{if(Relevant(args.OldFullPath)||Relevant(args.FullPath))Signal();};
        watcher.Error+=(_,_)=>Signal();
        watcher.EnableRaisingEvents=true;
    }
    private bool Relevant(string path)
    {
        var parts=Path.GetRelativePath(origin,path).Replace('\\','/').Split('/');
        if(parts[0].Equals(".specify",StringComparison.OrdinalIgnoreCase))
            return parts.Length==1 || (parts[1].Equals("memory",StringComparison.OrdinalIgnoreCase) &&
                (parts.Length==2 || (parts.Length==3 && parts[2].Equals("constitution.md",StringComparison.OrdinalIgnoreCase))));
        if(!parts[0].Equals("specs",StringComparison.OrdinalIgnoreCase))return false;
        if(parts.Length==1)return true;
        if(!System.Text.RegularExpressions.Regex.IsMatch(parts[1],@"^\d{3}-[A-Za-z0-9_-]+$"))return false;
        if(parts.Length==2)return true;
        var name=parts[^1];
        if(parts.Length==3 && new[]{"spec.md","plan.md","tasks.md","research.md","data-model.md","quickstart.md"}.Contains(name,StringComparer.OrdinalIgnoreCase))return true;
        if(!new[]{"checklists","contracts","design","evidence","verification","validation","acceptance"}.Contains(parts[2],StringComparer.OrdinalIgnoreCase))return false;
        if(new[]{"package.json","appsettings.json","launchSettings.json"}.Contains(name,StringComparer.OrdinalIgnoreCase))return false;
        var extension=Path.GetExtension(name).ToLowerInvariant();
        // Extensionless directory events invalidate conservatively, including removals.
        return Directory.Exists(path) || containsDocuments(Path.GetRelativePath(origin,path)) || extension.Length==0 || extension is ".md" or ".json" or ".txt" or ".trx" ||
            (parts[2].Equals("design",StringComparison.OrdinalIgnoreCase) && extension is ".png" or ".jpg" or ".jpeg" or ".webp" or ".svg");
    }
    private void Notify(object sender,FileSystemEventArgs args){if(Relevant(args.FullPath))Signal();}
    private void Signal(){if(Volatile.Read(ref disposed)==0)Changed?.Invoke();}
    public void Dispose(){if(Interlocked.Exchange(ref disposed,1)==0)watcher.Dispose();}
}
