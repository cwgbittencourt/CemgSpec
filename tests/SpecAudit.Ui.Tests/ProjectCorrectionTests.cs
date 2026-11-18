using System.Reflection;
using Microsoft.AspNetCore.Http;
using SpecAudit.Application;
using SpecAudit.Components.Pages;
using SpecAudit.Infrastructure.Documents;
using SpecAudit.Windows.Tests.Fixtures;

namespace SpecAudit.Ui.Tests;

public sealed class ProjectCorrectionTests
{
    private sealed class Picker(params string[] selections) : IFolderPicker
    {
        private readonly Queue<string> paths = new(selections);
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

    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    private static Projects Page(ProjectService service)
    {
        var page = new Projects();
        typeof(Projects).GetProperty("ProjectStore", PrivateInstance)!.SetValue(page, service);
        return page;
    }

    private static async Task Open(Projects page)
    {
        foreach(var method in new[]{"Open","ChooseOrigin","ChooseOutput","ConfirmOpen"})
            await (Task)typeof(Projects).GetMethod(method,PrivateInstance)!.Invoke(page,null)!;
    }
    private static void Refresh(Projects page) => typeof(Projects).GetMethod("Refresh", PrivateInstance)!.Invoke(page, null);
    private static string? Error(Projects page) => (string?)typeof(Projects).GetField("error", PrivateInstance)!.GetValue(page);

    private static void ExceedLimit(DocumentaryFixture fixture, string dimension)
    {
        if (dimension == "document-bytes")
            File.WriteAllText(Path.Combine(fixture.Feature, "spec.md"), new string('x', 4 * 1024 * 1024 + 1));
        else
        {
            var folder = Path.Combine(fixture.Feature, "evidence");
            Directory.CreateDirectory(folder);
            for (var index = 0; index < 257; index++)
                File.WriteAllText(Path.Combine(folder, $"proof-{index}.md"), "# Synthetic evidence");
        }
    }

    private static async Task WaitForWatchEvents(OpenProjectContext project)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        var last = project.Catalog.ChangeRevision;
        var quiet = 0;
        while (quiet < 3 && DateTime.UtcNow < deadline)
        {
            await Task.Delay(100);
            var current = project.Catalog.ChangeRevision;
            quiet = current == last ? quiet + 1 : 0;
            last = current;
        }
        Assert.Equal(3, quiet);
    }

    [Theory]
    [InlineData("specs/001-demo")]
    [InlineData("specs/001-demo/evidence/round.1")]
    [InlineData(".specify/memory")]
    public async Task DirectoryMetadataKeepsUnchangedDocumentsAndWorkCurrent(string relativeDirectory)
    {
        var fixture = DocumentaryFixture.Create();
        var directory = Path.GetFullPath(Path.Combine(fixture.Origin, relativeDirectory));
        Directory.CreateDirectory(directory);
        using var service = Service(new Picker());
        var project = service.OpenSelected(fixture.Origin, fixture.Output);
        var version = service.CaptureDocumentVersion(project.Session.ProjectId);
        var observed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var observer = new FileSystemWatcher(fixture.Origin)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.DirectoryName
        };
        observer.Changed += (_, args) =>
        {
            if (string.Equals(args.FullPath, directory, StringComparison.OrdinalIgnoreCase)) observed.TrySetResult();
        };
        observer.EnableRaisingEvents = true;
        var invalidated = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        project.Monitor.Changed += () => invalidated.TrySetResult();
        Directory.SetLastWriteTimeUtc(directory, DateTime.UtcNow.AddSeconds(2));
        await observed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        // Observe a real OS event and allow a forbidden invalidation to arrive.
        await Task.WhenAny(invalidated.Task, Task.Delay(400));
        Assert.True(fixture.IsUnchanged());
        Assert.False(project.Catalog.HasPendingChanges);
        Assert.False(version.Cancellation.IsCancellationRequested);
        service.EnsureCurrent(version);
        Assert.Empty(Directory.GetFiles(fixture.Output));
    }

    [Theory]
    [InlineData("document-bytes")]
    [InlineData("evidence-graph")]
    public async Task OpenLimitDisplaysErrorPreservesPreviousContextAndAllowsNextSelection(string dimension)
    {
        var original = DocumentaryFixture.Create();
        var oversized = DocumentaryFixture.Create();
        var next = DocumentaryFixture.Create();
        ExceedLimit(oversized, dimension);
        oversized.CaptureBaseline();
        using var service = Service(new Picker(oversized.Origin, oversized.Output, next.Origin, next.Output));
        var previous = service.OpenSelected(original.Origin, original.Output);
        var version = service.CaptureDocumentVersion(previous.Session.ProjectId);
        var page = Page(service);
        Assert.Null(await Record.ExceptionAsync(() => Open(page)));
        Assert.Contains(dimension == "document-bytes" ? "tamanho" : "quantidade", Error(page)!, StringComparison.OrdinalIgnoreCase);
        Assert.Same(previous, service.Active);
        Assert.Single(service.Projects);
        Assert.False(version.Cancellation.IsCancellationRequested);
        service.EnsureCurrent(version);
        Assert.True(original.IsUnchanged());
        Assert.True(oversized.IsUnchanged());
        Assert.Empty(Directory.GetFiles(oversized.Output));
        await Open(page);
        Assert.Null(Error(page));
        Assert.Equal(next.Origin, service.Active!.Session.OriginDisplayPath);
        Assert.Equal(2, service.Projects.Count);
        Assert.True(next.IsUnchanged());
        Assert.Empty(Directory.GetFiles(original.Output));
        Assert.Empty(Directory.GetFiles(next.Output));
    }

    [Theory]
    [InlineData("document-bytes")]
    [InlineData("evidence-graph")]
    public async Task RefreshLimitDisplaysErrorPreservesPendingContextAndAllowsExplicitRetry(string dimension)
    {
        var fixture = DocumentaryFixture.Create();
        using var service = Service(new Picker());
        var project = service.OpenSelected(fixture.Origin, fixture.Output);
        var previousEntries = project.Catalog.Entries.ToArray();
        var version = service.CaptureDocumentVersion(project.Session.ProjectId);
        var page = Page(service);
        ExceedLimit(fixture, dimension);
        fixture.CaptureBaseline();
        await WaitForWatchEvents(project);
        Assert.Null(Record.Exception(() => Refresh(page)));
        Assert.Contains(dimension == "document-bytes" ? "tamanho" : "quantidade", Error(page)!, StringComparison.OrdinalIgnoreCase);
        Assert.Same(project, service.Active);
        Assert.True(project.Catalog.HasPendingChanges);
        Assert.Equal(previousEntries, project.Catalog.Entries);
        Assert.True(version.Cancellation.IsCancellationRequested);
        Assert.True(fixture.IsUnchanged());
        Assert.Empty(Directory.GetFiles(fixture.Output));

        // The test user deliberately repairs synthetic input; the product never
        // restores documents or deletes data to hide a failed operation.
        if (dimension == "document-bytes")
            File.WriteAllText(Path.Combine(fixture.Feature, "spec.md"), "# Corrected synthetic spec");
        else
            Directory.Move(Path.Combine(fixture.Feature, "evidence"), Path.Combine(fixture.Root, "parked-evidence"));
        fixture.CaptureBaseline();
        await WaitForWatchEvents(project);
        Refresh(page);
        Assert.Null(Error(page));
        Assert.False(project.Catalog.HasPendingChanges);
        Assert.Same(project, service.Active);
        service.EnsureCurrent(service.CaptureDocumentVersion(project.Session.ProjectId));
        Assert.True(fixture.IsUnchanged());
        Assert.Empty(Directory.GetFiles(fixture.Output));
    }
}
