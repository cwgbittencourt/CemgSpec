using CemgSpec.Domain;

namespace CemgSpec.Infrastructure.Documents;

public enum PreviewLinkStatus { Authorized, Missing, MissingAnchor, OutOfScope, External }

public sealed record PreviewLink(PreviewLinkStatus Status, Guid? DocumentId = null, string? ElementId = null);

public sealed class DocumentPreviewSession
{
    private readonly DocumentCatalog catalog;
    private readonly DocumentBroker broker;
    private readonly CatalogEntry source;
    private readonly CancellationToken projectLifetime;
    private readonly MarkdownRenderer renderer = new();

    public MarkdownPreview Preview { get; }

    internal DocumentPreviewSession(DocumentCatalog catalog, DocumentBroker broker, Guid documentId,
        CancellationToken projectLifetime)
    {
        this.catalog = catalog;
        this.broker = broker;
        this.projectLifetime = projectLifetime;
        source = catalog.Entries.SingleOrDefault(entry => entry.DocumentId == documentId && entry.Kind != CatalogKind.PresentationAsset)
            ?? throw new DocumentAccessException("OutOfScope");
        var snapshot = Read(documentId);

        var initial = renderer.Render(snapshot);
        long remainingImageBytes = 8 * 1024 * 1024;
        var localImages = new Dictionary<string, PreviewRasterImage>(StringComparer.Ordinal);
        var resolvedImages = new Dictionary<Guid, PreviewRasterImage?>();
        foreach (var reference in initial.References.Where(reference => reference.Kind == PreviewReferenceKind.LocalImage)
                     .DistinctBy(reference => reference.Target))
        {
            var (entry, _) = FindTarget(reference.Target, image: true);
            if (entry is null) continue;
            if (!resolvedImages.TryGetValue(entry.DocumentId, out var image))
            {
                if (entry.Length > remainingImageBytes) continue;
                remainingImageBytes -= entry.Length;
                image = PreviewRasterImage.FromSnapshot(Read(entry.DocumentId));
                resolvedImages.Add(entry.DocumentId, image);
            }
            if (image is not null) localImages.Add(reference.Target, image);
        }
        Preview = localImages.Count == 0 ? initial : renderer.Render(snapshot, localImages);
        EnsureCurrent();
    }

    public PreviewLink ResolveLink(int index)
    {
        EnsureCurrent();
        var reference = GetReference(index);
        if (reference.Kind == PreviewReferenceKind.ExternalLink)
            return new(PreviewLinkStatus.External);
        if (reference.Kind != PreviewReferenceKind.DocumentLink)
            return new(PreviewLinkStatus.OutOfScope);

        var (entry, status) = FindTarget(reference.Target, image: false);
        if (entry is null) return new(status);

        var snapshot = Read(entry.DocumentId);
        var fragmentIndex = reference.Target.IndexOf('#');
        if (fragmentIndex < 0 || fragmentIndex == reference.Target.Length - 1)
            return new(PreviewLinkStatus.Authorized, entry.DocumentId);

        var fragment = Uri.UnescapeDataString(reference.Target[(fragmentIndex + 1)..]);
        var headings = entry.DocumentId == source.DocumentId ? Preview.Headings : renderer.Render(snapshot).Headings;
        var heading = headings.FirstOrDefault(candidate => candidate.Anchor == fragment);
        return heading is null
            ? new(PreviewLinkStatus.MissingAnchor, entry.DocumentId)
            : new(PreviewLinkStatus.Authorized, entry.DocumentId, heading.ElementId);
    }

    public Task<PreviewRasterImage> AuthorizeRemoteImageAsync(int index, CancellationToken cancellationToken = default) =>
        AuthorizeRemoteImageAsync(index, RemotePreviewImages.Default, cancellationToken);

    internal async Task<PreviewRasterImage> AuthorizeRemoteImageAsync(int index, RemotePreviewImages loader,
        CancellationToken cancellationToken = default)
    {
        EnsureCurrent();
        var reference = GetReference(index);
        if (reference.Kind != PreviewReferenceKind.RemoteImage)
            throw new DocumentAccessException("OutOfScope");

        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(projectLifetime, cancellationToken);
        var image = await loader.LoadAsync(new Uri(reference.Target), lifetime.Token);
        EnsureCurrent();
        return image;
    }

    private PreviewReference GetReference(int index) => index >= 0 && index < Preview.References.Count
        ? Preview.References[index]
        : throw new DocumentAccessException("OutOfScope");

    private DocumentSnapshot Read(Guid id)
    {
        if (projectLifetime.IsCancellationRequested)
            throw new DocumentAccessException("Changed");
        var snapshot = broker.Read(catalog.ProjectId, id, source.CatalogGeneration);
        if (projectLifetime.IsCancellationRequested)
            throw new DocumentAccessException("Changed");
        return snapshot;
    }

    private void EnsureCurrent() => Read(source.DocumentId);

    private (CatalogEntry? Entry, PreviewLinkStatus Status) FindTarget(string target, bool image)
    {
        var fragment = target.IndexOf('#');
        var rawPath = fragment < 0 ? target : target[..fragment];
        if (rawPath.Length == 0)
            return image ? (null, PreviewLinkStatus.OutOfScope) : (source, PreviewLinkStatus.Authorized);
        if (rawPath.Contains('?')) return (null, PreviewLinkStatus.OutOfScope);

        var parent = Path.GetDirectoryName(source.RelativePath);
        var segments = parent is null ? new List<string>() : parent.Split(Path.DirectorySeparatorChar).ToList();
        foreach (var encoded in rawPath.Split('/'))
        {
            var segment = Uri.UnescapeDataString(encoded);
            if (segment is "." or "") continue;
            if (segment == "..")
            {
                if (segments.Count == 0) return (null, PreviewLinkStatus.OutOfScope);
                segments.RemoveAt(segments.Count - 1);
                continue;
            }
            if (segment.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                segment.EndsWith('.') || segment.EndsWith(' '))
                return (null, PreviewLinkStatus.OutOfScope);
            segments.Add(segment);
        }

        if (segments.Count == 0) return (null, PreviewLinkStatus.OutOfScope);
        var extension = Path.GetExtension(segments[^1]).ToLowerInvariant();
        if (image ? extension is not (".png" or ".jpg" or ".jpeg" or ".webp")
                  : extension is not (".md" or ".json" or ".txt" or ".trx"))
            return (null, PreviewLinkStatus.OutOfScope);

        var path = string.Join(Path.DirectorySeparatorChar, segments);
        var entry = catalog.Entries.FirstOrDefault(candidate =>
            candidate.CatalogGeneration == source.CatalogGeneration &&
            (candidate.Kind == CatalogKind.PresentationAsset) == image &&
            candidate.RelativePath.Equals(path, StringComparison.OrdinalIgnoreCase));
        return entry is null ? (null, PreviewLinkStatus.Missing) : (entry, PreviewLinkStatus.Authorized);
    }
}
