using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SpecAudit.Application;
using SpecAudit.Components.Pages;
using SpecAudit.Windows.Tests.Fixtures;

namespace SpecAudit.Ui.Tests;

// Review-only reproductions. Assertions express desired behavior, not the bug.
public sealed class T014ReviewProbe
{
    private sealed class Picker(params string[] values) : IFolderPicker
    {
        private readonly Queue<string> paths = new(values);
        public Task<string?> PickAsync(string title) => Task.FromResult<string?>(paths.Dequeue());
    }

    private static ProjectService Service(IFolderPicker picker)
    {
        var registry = new LocalSessionRegistry();
        var issued = registry.Issue();
        var context = new DefaultHttpContext();
        context.Items[typeof(LocalSessionService)] = issued.Id;
        return new(new LocalSessionService(new HttpContextAccessor { HttpContext = context }, registry), picker);
    }

    private static void Observe(string name, object observation)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "SpecAudit.slnx"))) directory = directory.Parent;
        var path = Path.Combine(directory!.FullName, "specs", "001-auditar-projetos-speckit", "evidence", "logs", "T014-review", name + ".json");
        File.WriteAllText(path, JsonSerializer.Serialize(observation, new JsonSerializerOptions { WriteIndented = true }));
    }

    [Fact]
    public async Task UnchangedDocumentsMustSurviveFeatureDirectoryMetadataEvent()
    {
        var fixture = DocumentaryFixture.Create();
        using var service = Service(new Picker(fixture.Origin, fixture.Output));
        var project = (await service.OpenProjectAsync())!;
        var version = service.CaptureDocumentVersion(project.Session.ProjectId);
        var events = new ConcurrentQueue<string>();
        using var observer = new FileSystemWatcher(fixture.Origin)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.LastWrite
        };
        observer.Changed += (_, args) => events.Enqueue(Path.GetRelativePath(fixture.Origin, args.FullPath));
        observer.EnableRaisingEvents = true;
        var changed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        project.Monitor.Changed += () => changed.TrySetResult();
        // A directory's LastWrite event is also possible when uncatalogued names
        // change. No document content or path changes in this deterministic probe.
        Directory.SetLastWriteTimeUtc(fixture.Feature, DateTime.UtcNow.AddSeconds(2));
        await Task.WhenAny(changed.Task, Task.Delay(TimeSpan.FromSeconds(2)));
        Assert.True(fixture.IsUnchanged());
        Observe("directory-metadata", new
        {
            originContentUnchanged = true,
            project.Catalog.HasPendingChanges,
            cancelled = version.Cancellation.IsCancellationRequested,
            directoryEvents = events.ToArray(),
            reports = Directory.GetFiles(fixture.Output).Length
        });
        Assert.False(project.Catalog.HasPendingChanges);
        Assert.False(version.Cancellation.IsCancellationRequested);
    }

    [Theory]
    [InlineData("document-bytes")]
    [InlineData("evidence-graph")]
    public async Task ProjectPageMustDisplayLimitsWithoutPropagatingException(string dimension)
    {
        var fixture = DocumentaryFixture.Create();
        if (dimension == "document-bytes")
            File.WriteAllText(Path.Combine(fixture.Feature, "spec.md"), new string('x', 4 * 1024 * 1024 + 1));
        else
        {
            var folder = Path.Combine(fixture.Feature, "evidence");
            Directory.CreateDirectory(folder);
            for (var index = 0; index < 257; index++) File.WriteAllText(Path.Combine(folder, $"proof-{index}.md"), "# Synthetic proof");
        }
        fixture.CaptureBaseline();
        using var service = Service(new Picker(fixture.Origin, fixture.Output));
        var page = new Projects();
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(Projects).GetProperty("ProjectStore", flags)!.SetValue(page, service);
        var open = typeof(Projects).GetMethod("Open", flags)!;
        var failure = await Record.ExceptionAsync(async () => await (Task)open.Invoke(page, null)!);
        Observe("page-limit-" + dimension, new
        {
            dimension,
            exception = failure?.GetType().Name,
            message = failure?.Message,
            displayedError = typeof(Projects).GetField("error", flags)!.GetValue(page),
            projects = service.Projects.Count,
            originContentUnchanged = fixture.IsUnchanged(),
            reports = Directory.GetFiles(fixture.Output).Length
        });
        Assert.True(fixture.IsUnchanged());
        Assert.Null(failure);
    }
}
