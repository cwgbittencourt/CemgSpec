using SpecAudit.Domain;

namespace SpecAudit.Components.Documents;

public enum DocumentMatch { None, Path, Content }

public sealed record DocumentTreeFile(CatalogEntry Entry, string Name, string DisplayPath, bool ByContent);

public sealed class DocumentTreeFolder(string key, string name)
{
    public string Key { get; } = key;
    public string Name { get; } = name;
    public List<DocumentTreeFolder> Folders { get; } = [];
    public List<DocumentTreeFile> Files { get; } = [];
}

// Builds the navigation tree only from catalog entries. Presentation assets are reachable
// through documents that reference them, never as standalone tree items.
public static class DocumentTreeBuilder
{
    private static readonly string[] CanonicalOrder =
        ["spec.md", "plan.md", "tasks.md", "research.md", "data-model.md", "quickstart.md"];

    public static IReadOnlyList<DocumentTreeFolder> Build(IReadOnlyList<CatalogEntry> entries,
        IReadOnlyDictionary<Guid, string> features, Func<CatalogEntry, string, DocumentMatch> match)
    {
        var project = new DocumentTreeFolder("project", "Projeto");
        var groups = features
            .OrderBy(feature => feature.Value, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(feature => feature.Key,
                feature => (Root: new DocumentTreeFolder("feature:" + feature.Key.ToString("N"), Path.GetFileName(feature.Value)),
                    Directory: feature.Value));

        foreach (var entry in entries.Where(entry => entry.Kind != CatalogKind.PresentationAsset))
        {
            DocumentTreeFolder root;
            string display;
            if (entry.FeatureId is { } featureId && groups.TryGetValue(featureId, out var group))
            {
                root = group.Root;
                display = Path.GetRelativePath(group.Directory, entry.RelativePath);
            }
            else
            {
                root = project;
                display = entry.RelativePath;
            }

            var result = match(entry, display);
            if (result == DocumentMatch.None) continue;
            var segments = display.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            var folder = root;
            for (var i = 0; i < segments.Length - 1; i++)
            {
                var key = folder.Key + "/" + segments[i];
                var next = folder.Folders.FirstOrDefault(candidate => candidate.Key == key);
                if (next is null)
                {
                    next = new DocumentTreeFolder(key, segments[i]);
                    folder.Folders.Add(next);
                }
                folder = next;
            }
            folder.Files.Add(new(entry, segments[^1], display, result == DocumentMatch.Content));
        }

        var roots = new List<DocumentTreeFolder>();
        if (project.Files.Count > 0 || project.Folders.Count > 0) roots.Add(project);
        roots.AddRange(groups.Values.Select(group => group.Root).Where(root => root.Files.Count > 0 || root.Folders.Count > 0));
        foreach (var root in roots) Sort(root);
        return roots;
    }

    private static void Sort(DocumentTreeFolder folder)
    {
        folder.Files.Sort((left, right) =>
        {
            var order = Rank(left.Name).CompareTo(Rank(right.Name));
            return order != 0 ? order : StringComparer.OrdinalIgnoreCase.Compare(left.Name, right.Name);
        });
        folder.Folders.Sort((left, right) => StringComparer.OrdinalIgnoreCase.Compare(left.Name, right.Name));
        foreach (var child in folder.Folders) Sort(child);
    }

    private static int Rank(string name)
    {
        var index = Array.FindIndex(CanonicalOrder, candidate => candidate.Equals(name, StringComparison.OrdinalIgnoreCase));
        return index < 0 ? CanonicalOrder.Length : index;
    }
}

