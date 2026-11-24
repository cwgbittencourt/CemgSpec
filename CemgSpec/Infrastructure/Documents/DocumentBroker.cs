using CemgSpec.Domain;
namespace CemgSpec.Infrastructure.Documents;
public sealed class DocumentBroker(DocumentCatalog catalog)
{
    internal Action? BeforeRead {get;init;}
    internal Action<string,string,string>? AccessObserved {get;init;}
    public DocumentSnapshot Read(Guid projectId,Guid documentId,long generation)
    {
        lock(catalog.SyncRoot)return ReadLocked(projectId,documentId,generation);
    }
    public DocumentPreviewSession OpenPreview(Guid documentId, CancellationToken projectLifetime = default)
        => new(catalog, this, documentId, projectLifetime);
    private DocumentSnapshot ReadLocked(Guid projectId,Guid documentId,long generation)
    {
        if(projectId!=catalog.ProjectId || !catalog.Index.TryGetValue(documentId,out var entry))throw new DocumentAccessException("OutOfScope");
        if(catalog.HasPendingChanges || generation!=catalog.Generation || entry.CatalogGeneration!=generation)throw new DocumentAccessException("Changed");
        catalog.ConfirmOrigin();
        var expected=Path.Combine(catalog.FinalOriginPath,entry.RelativePath);
        using var handle=WindowsReadHandle.Open(expected,false);
        var final=WindowsReadHandle.FinalPath(handle);var initial=WindowsReadHandle.Info(handle);
        if(!WindowsReadHandle.Contains(catalog.FinalOriginPath,final) || !string.Equals(final,expected,StringComparison.OrdinalIgnoreCase) || (initial.Attributes&0x400)!=0 || initial.Links!=1)
        {AccessObserved?.Invoke(expected,"open","OutOfScope");throw new DocumentAccessException("OutOfScope");}
        if(initial.Identity!=entry.FileIdentity || initial.Length!=entry.Length || DateTimeOffset.FromFileTime(initial.WriteStamp)!=entry.LastObservedUtc)throw new DocumentAccessException("Changed");
        catalog.Limits.CheckDocument(initial.Length);
        BeforeRead?.Invoke();
        if(!string.Equals(WindowsReadHandle.FinalPath(handle),expected,StringComparison.OrdinalIgnoreCase))throw new DocumentAccessException("Changed");
        var bytes=new byte[checked((int)initial.Length)];
        try
        {
            using var stream=new FileStream(handle,FileAccess.Read);
            stream.ReadExactly(bytes);
            var after=WindowsReadHandle.Info(handle);
            using var current=WindowsReadHandle.Open(expected,false);
            if(after.WriteStamp!=initial.WriteStamp || after.Length!=initial.Length || WindowsReadHandle.Info(current).Identity!=initial.Identity || catalog.HasPendingChanges || generation!=catalog.Generation)throw new DocumentAccessException("Changed");
            var snapshot=DocumentSnapshot.Create(documentId,bytes,generation);
            if(entry.ContentHash.Length!=0 && entry.ContentHash!=snapshot.ContentHash)throw new DocumentAccessException("Changed");
            AccessObserved?.Invoke(expected,"read","Authorized");return snapshot;
        }
        catch(EndOfStreamException){throw new DocumentAccessException("Changed");}
    }
}
