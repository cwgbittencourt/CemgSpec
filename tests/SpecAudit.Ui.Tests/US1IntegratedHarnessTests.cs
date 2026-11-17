using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SpecAudit.Application;
using SpecAudit.Infrastructure.Documents;
using SpecAudit.Windows.Tests.Fixtures;

namespace SpecAudit.Ui.Tests;

// Real Windows dialog, service, catalog, broker and watcher. No agent/Save simulation.
[Collection("US1 native dialogs")]
public sealed class US1IntegratedHarnessTests
{
    private sealed class AutomatedNativePicker(params string[] paths) : IFolderPicker
    {
        private readonly Queue<string> pending = new(paths);
        public async Task<string?> PickAsync(string title)
        {
            var expected = pending.Dequeue();
            var uniqueTitle = title + " — T013-" + Guid.NewGuid().ToString("N");
            var selection = new NativeFolderPicker().PickAsync(uniqueTitle);
            IntPtr dialog = IntPtr.Zero;
            var deadline = DateTime.UtcNow.AddSeconds(10);
            while (dialog == IntPtr.Zero && DateTime.UtcNow < deadline)
            {
                EnumWindows((window, _) =>
                {
                    GetWindowThreadProcessId(window, out var processId);
                    if (processId != Environment.ProcessId) return true;
                    EnumChildWindows(window, (child, _) =>
                    {
                        var text = new StringBuilder(1024);
                        GetWindowText(child, text, text.Capacity);
                        if (text.ToString() == uniqueTitle) dialog = window;
                        return true;
                    }, IntPtr.Zero);
                    return dialog == IntPtr.Zero;
                }, IntPtr.Zero);
                if (dialog == IntPtr.Zero) await Task.Delay(25);
            }
            Assert.NotEqual(IntPtr.Zero, dialog);
            ShowWindow(dialog, 0);
            try
            {
                // SHBrowseForFolder's documented BFFM_SETSELECTIONW message selects
                // an existing synthetic directory in the actual native dialog.
                SendMessage(dialog, 0x467, new IntPtr(1), expected);
                SendMessage(dialog, 0x111, new IntPtr(1), IntPtr.Zero);
                var actual = await selection.WaitAsync(TimeSpan.FromSeconds(10));
                Assert.Equal(Path.GetFullPath(expected), actual, ignoreCase: true);
                return actual;
            }
            finally
            {
                if (!selection.IsCompleted) SendMessage(dialog, 0x111, new IntPtr(2), IntPtr.Zero);
            }
        }
        private delegate bool EnumWindow(IntPtr window, IntPtr parameter);
        [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindow callback, IntPtr parameter);
        [DllImport("user32.dll")] private static extern bool EnumChildWindows(IntPtr window, EnumWindow callback, IntPtr parameter);
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr window, StringBuilder text, int length);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr window, int command);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr SendMessage(IntPtr window, uint message, IntPtr parameter, string value);
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr window, uint message, IntPtr parameter, IntPtr value);
    }

    private static ProjectService Service(IFolderPicker picker)
    {
        var registry = new LocalSessionRegistry();
        var issued = registry.Issue();
        var context = new DefaultHttpContext();
        context.Items[typeof(LocalSessionService)] = issued.Id;
        return new(new LocalSessionService(new HttpContextAccessor { HttpContext = context }, registry), picker);
    }

    private static void Record(string scenario, object observed)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "SpecAudit.slnx"))) directory = directory.Parent;
        var output = Path.Combine(directory!.FullName, "specs", "001-auditar-projetos-speckit", "evidence", "logs", "T013-20261117", "harness");
        Directory.CreateDirectory(output);
        // Each run is a new artifact; prior rounds are never overwritten.
        var json = JsonSerializer.Serialize(new { scenario, timestamp = DateTimeOffset.UtcNow, result = "PASSOU", observed }, new JsonSerializerOptions { WriteIndented = true });
        var temporaryRoot = JsonSerializer.Serialize(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar))[1..^1];
        json = json.Replace(temporaryRoot, "__TEMP_ROOT__", StringComparison.OrdinalIgnoreCase);
        File.WriteAllText(Path.Combine(output, scenario + "-" + Guid.NewGuid().ToString("N") + ".json"), json);
    }

    [Fact]
    public async Task S01NativeSelectionLoadsFeaturesAndMissingArtifactsWithoutWrites()
    {
        var fixture = DocumentaryFixture.Create();
        File.Delete(Path.Combine(fixture.Origin, "specs", "002-other", "tasks.md"));
        fixture.CaptureBaseline();
        using var service = Service(new AutomatedNativePicker(fixture.Origin, fixture.Output));
        var project = (await service.OpenProjectAsync())!;
        Assert.Equal(2, project.Session.Features.Count);
        foreach (var feature in project.Session.Features)
        {
            project.Session.SelectFeature(feature.FeatureId);
            var entries = project.Catalog.Entries.Where(e => e.FeatureId == feature.FeatureId).ToArray();
            Assert.Contains(entries, e => Path.GetFileName(e.RelativePath) == "spec.md");
            Assert.Equal(feature.SpecKitRelativeDirectory.Replace('\\', '/').EndsWith("001-demo"), entries.Any(e => Path.GetFileName(e.RelativePath) == "tasks.md"));
            foreach (var entry in entries) Assert.NotEmpty(service.ReadDocument(project.Session.ProjectId, entry.DocumentId).CopyBytes());
        }
        Assert.True(fixture.IsUnchanged());
        Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Output));
        Record("US1-S01", new { fixture.Root, fixture.Origin, fixture.Output, features = 2, missingTasks = "002-other", originUnchanged = true, reports = 0, nativePicker = true });
    }

    [Fact]
    public async Task S02NativeSelectionsRejectOriginDescendantAndJunction()
    {
        var fixture = DocumentaryFixture.Create();
        var junction = Path.Combine(fixture.Root, "origin-alias");
        FixtureJunction.Create(fixture.Root, junction, fixture.Feature);
        using var service = Service(new AutomatedNativePicker(fixture.Origin, fixture.Origin, fixture.Origin, fixture.Feature, fixture.Origin, junction));
        for (var attempt = 0; attempt < 3; attempt++)
            Assert.Equal("DestinationInsideOrigin", (await Assert.ThrowsAsync<DocumentAccessException>(() => service.OpenProjectAsync())).Code);
        Assert.Empty(service.Projects);
        Assert.True(fixture.IsUnchanged());
        Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Output));
        Record("US1-S02", new { fixture.Root, rejected = new[] { fixture.Origin, fixture.Feature, junction }, originUnchanged = true, projects = 0, nativePicker = true });
    }

    [Fact]
    public async Task S03NativeDestinationChangePreservesBothContextsAndHistory()
    {
        var a = DocumentaryFixture.Create();
        var b = DocumentaryFixture.Create();
        var history = Path.Combine(a.Output, "existing.md");
        File.WriteAllText(history, "synthetic pre-existing saved history");
        var historyHash = DocumentaryFixture.Hash(history);
        using var service = Service(new AutomatedNativePicker(a.Origin, a.Output, b.Origin, b.Output, a.Evidence));
        var first = (await service.OpenProjectAsync())!;
        var firstFeature = first.Session.Features.Last().FeatureId;
        first.Session.SelectFeature(firstFeature);
        var second = (await service.OpenProjectAsync())!;
        service.SelectProject(first.Session.ProjectId);
        await service.ChangeOutputDirectoryAsync(first.Session.ProjectId);
        Assert.Equal(firstFeature, first.Session.ActiveFeatureId);
        Assert.Equal(a.Evidence, first.OutputDisplayPath);
        Assert.Equal(b.Output, second.OutputDisplayPath);
        Assert.Equal(historyHash, DocumentaryFixture.Hash(history));
        Assert.Single(Directory.EnumerateFileSystemEntries(a.Output));
        Assert.Empty(Directory.EnumerateFileSystemEntries(b.Output));
        Assert.Equal(2, service.Projects.Count);
        Assert.True(a.IsUnchanged()); Assert.True(b.IsUnchanged());
        Record("US1-S03", new { firstRoot = a.Root, secondRoot = b.Root, history, historyHash, newDestination = a.Evidence, contexts = 2, originsUnchanged = true, nativePicker = true, historyPreparedByHarness = true });
    }

    [Fact]
    public async Task S04ExternalEditDuringDocumentaryWorkInvalidatesVersionAndPreservesEdit()
    {
        var fixture = DocumentaryFixture.Create();
        using var service = Service(new AutomatedNativePicker(fixture.Origin, fixture.Output));
        var project = (await service.OpenProjectAsync())!;
        var version = service.CaptureDocumentVersion(project.Session.ProjectId);
        var entry = project.Catalog.Entries.Single(e => e.RelativePath.Replace('\\', '/') == "specs/001-demo/spec.md");
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var changed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        project.Monitor.Changed += () => changed.TrySetResult();
        var work = Task.Run(async () =>
        {
            var snapshot = service.ReadDocument(project.Session.ProjectId, entry.DocumentId);
            started.SetResult();
            await resume.Task;
            Assert.Equal("Changed", Assert.Throws<DocumentAccessException>(() => service.EnsureCurrent(version)).Code);
            Assert.DoesNotContain("T013 external user edit", Encoding.UTF8.GetString(snapshot.CopyBytes()));
        });
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var path = Path.Combine(fixture.Feature, "spec.md");
        File.AppendAllText(path, "\nT013 external user edit");
        await changed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        resume.SetResult(); await work.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(version.Cancellation.IsCancellationRequested);
        Assert.True(project.Catalog.HasPendingChanges);
        Assert.Contains("T013 external user edit", File.ReadAllText(path));
        // The external edit is the only permitted difference in the entire origin.
        var current = Directory.EnumerateFiles(fixture.Origin, "*", SearchOption.AllDirectories).ToDictionary(p => Path.GetRelativePath(fixture.Origin, p), DocumentaryFixture.Hash);
        Assert.Equal(fixture.Baseline.Keys.Order(), current.Keys.Order());
        foreach (var pair in fixture.Baseline.Where(p => p.Key != Path.GetRelativePath(fixture.Origin, path))) Assert.Equal(pair.Value, current[pair.Key]);
        Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Output));
        Record("US1-S04", new { fixture.Root, fixture.Origin, fixture.Output, version.Generation, stale = "Changed", cancellation = true, editPreserved = true, unexpectedWrites = 0, medium = "public document/version API harness; no agent or Save workflow" });
    }
}
