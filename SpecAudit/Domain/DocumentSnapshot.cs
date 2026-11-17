using System.Security.Cryptography;
namespace SpecAudit.Domain;
public sealed record CatalogEntry(Guid DocumentId,Guid ProjectId,Guid? FeatureId,string RelativePath,CatalogKind Kind,string FileIdentity,string ContentHash,long Length,DateTimeOffset LastObservedUtc,long CatalogGeneration);
public sealed record DocumentReference(string TargetText, Guid? TargetDocumentId);
public sealed class DocumentSnapshot
{
    private readonly byte[] bytes;
    public Guid DocumentId { get; }
    public Guid SnapshotId { get; }=Guid.NewGuid();
    public string ContentHash { get; }
    public string Encoding { get; }="utf-8";
    public long SourceGeneration { get; }
    public DateTimeOffset ReadUtc { get; }=DateTimeOffset.UtcNow;
    public IReadOnlyList<DocumentReference> References { get; } = Array.Empty<DocumentReference>();
    private DocumentSnapshot(Guid id,byte[] input,long generation)
    {
        if(id==Guid.Empty)throw new ArgumentException("Document identity is required.",nameof(id));
        if(generation<0)throw new ArgumentOutOfRangeException(nameof(generation));
        DocumentId=id;bytes=(byte[])input.Clone();SourceGeneration=generation;
        ContentHash=Convert.ToHexStringLower(SHA256.HashData(bytes));
    }
    public static DocumentSnapshot Create(Guid id,byte[] bytes,long generation)=>new(id,bytes,generation);
    public byte[] CopyBytes()=>(byte[])bytes.Clone();
}
