using Microsoft.AspNetCore.Http;
using SpecAudit.Application;
using SpecAudit.Infrastructure.Documents;
using SpecAudit.Windows.Tests.Fixtures;
namespace SpecAudit.Ui.Tests;
public sealed class ProjectSelectionTests
{
    private sealed class Picker(params string?[] folders) : IFolderPicker
    {private readonly Queue<string?> values=new(folders);public Task<string?> PickAsync(string title)=>Task.FromResult(values.Dequeue());}
    private static ProjectService Service(IFolderPicker picker)
    {
        var registry=new LocalSessionRegistry();var issued=registry.Issue();var context=new DefaultHttpContext();context.Items[typeof(LocalSessionService)]=issued.Id;
        return new(new LocalSessionService(new HttpContextAccessor{HttpContext=context},registry),picker);
    }
    [Fact]
    public async Task SelectedFoldersLoadTwoFeaturesAndCancelDoesNotCreateProject()
    {
        var fixture=DocumentaryFixture.Create();using var service=Service(new Picker(fixture.Origin,fixture.Output,null));
        var project=await service.OpenProjectAsync();Assert.NotNull(project);Assert.Equal(2,project.Session.Features.Count);
        var spec=Assert.Single(project.Catalog.Entries,e=>e.RelativePath.Replace('\\','/')=="specs/001-demo/spec.md");
        Assert.Contains("Demo feature",System.Text.Encoding.UTF8.GetString(service.ReadDocument(project.Session.ProjectId,spec.DocumentId).CopyBytes()));
        Assert.Null(await service.OpenProjectAsync());Assert.Single(service.Projects);Assert.True(fixture.IsUnchanged());Assert.Empty(Directory.GetFiles(fixture.Output));
    }
    [Fact]
    public void InternalAndJunctionDestinationsAreRejectedWithoutWritingOrigin()
    {
        var fixture=DocumentaryFixture.Create();var junction=Path.Combine(fixture.Root,"inside-alias");FixtureJunction.Create(fixture.Root,junction,fixture.Feature);
        using var service=Service(new Picker());
        foreach(var destination in new[]{fixture.Origin,fixture.Feature,junction})
            Assert.Equal("DestinationInsideOrigin",Assert.Throws<DocumentAccessException>(()=>service.OpenSelected(fixture.Origin,destination)).Code);
        Assert.Empty(service.Projects);Assert.True(fixture.IsUnchanged());
    }
    [Fact]
    public void ContextAndExistingHistoryStaySeparatedWhenChangingOutput()
    {
        var a=DocumentaryFixture.Create();var b=DocumentaryFixture.Create();File.WriteAllText(Path.Combine(a.Output,"existing.md"),"saved history");
        using var service=Service(new Picker());var first=service.OpenSelected(a.Origin,a.Output);var second=service.OpenSelected(b.Origin,b.Output);
        service.SelectProject(first.Session.ProjectId);service.ChangeOutputSelected(first.Session.ProjectId,a.Evidence);
        Assert.Same(first,service.Active);Assert.Equal(a.Evidence,first.OutputDisplayPath);Assert.Equal(b.Output,second.OutputDisplayPath);
        Assert.Equal("saved history",File.ReadAllText(Path.Combine(a.Output,"existing.md")));
        using var other=Service(new Picker());Assert.Throws<KeyNotFoundException>(()=>other.RequireProject(first.Session.ProjectId));
    }
    [Fact]
    public async Task ExternalDocumentEditInvalidatesSnapshotAndRefreshPreservesDocumentIdentity()
    {
        var fixture=DocumentaryFixture.Create();using var service=Service(new Picker());var project=service.OpenSelected(fixture.Origin,fixture.Output);
        var spec=Assert.Single(project.Catalog.Entries,e=>e.RelativePath.Replace('\\','/')=="specs/001-demo/spec.md");
        var changed=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);project.Monitor.Changed+=()=>changed.TrySetResult();
        File.AppendAllText(Path.Combine(fixture.Feature,"spec.md"),"\nExternal user edit");await changed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(project.Catalog.HasPendingChanges);Assert.Equal("Changed",Assert.Throws<DocumentAccessException>(()=>service.ReadDocument(project.Session.ProjectId,spec.DocumentId)).Code);
        service.Refresh(project.Session.ProjectId);Assert.Contains("External user edit",System.Text.Encoding.UTF8.GetString(service.ReadDocument(project.Session.ProjectId,spec.DocumentId).CopyBytes()));Assert.Equal(1,project.Catalog.Generation);
    }
    [Fact]
    public async Task UncataloguedMarkdownAndCodeDoNotInvalidateDocuments()
    {
        var fixture=DocumentaryFixture.Create();
        var ignored=Path.Combine(fixture.Feature,"notes.md");File.WriteAllText(ignored,"outside catalog");
        using var service=Service(new Picker());var project=service.OpenSelected(fixture.Origin,fixture.Output);
        File.AppendAllText(ignored," external edit");File.WriteAllText(Path.Combine(fixture.Feature,"source.cs"),"// code");
        await Task.Delay(500);
        Assert.False(project.Catalog.HasPendingChanges);
    }
    [Fact]
    public async Task CapturedVersionIsCancelledByEditAndCannotBeReusedAfterRefresh()
    {
        var fixture=DocumentaryFixture.Create();using var service=Service(new Picker());var project=service.OpenSelected(fixture.Origin,fixture.Output);
        var version=service.CaptureDocumentVersion(project.Session.ProjectId);
        var changed=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);project.Monitor.Changed+=()=>changed.TrySetResult();
        File.AppendAllText(Path.Combine(fixture.Feature,"spec.md"),"\nT012 external edit");await changed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(version.Cancellation.IsCancellationRequested);
        Assert.Equal("Changed",Assert.Throws<DocumentAccessException>(()=>service.EnsureCurrent(version)).Code);
        service.Refresh(project.Session.ProjectId);
        Assert.Equal("Changed",Assert.Throws<DocumentAccessException>(()=>service.EnsureCurrent(version)).Code);
        service.EnsureCurrent(service.CaptureDocumentVersion(project.Session.ProjectId));
        Assert.Contains("T012 external edit",File.ReadAllText(Path.Combine(fixture.Feature,"spec.md")));Assert.Empty(Directory.GetFiles(fixture.Output));
    }
    [Fact]
    public void SwitchingProjectCancelsOnlyItsOwnWorkWithoutSaving()
    {
        var a=DocumentaryFixture.Create();var b=DocumentaryFixture.Create();using var service=Service(new Picker());
        var first=service.OpenSelected(a.Origin,a.Output);var old=service.CaptureDocumentVersion(first.Session.ProjectId);
        var second=service.OpenSelected(b.Origin,b.Output);var current=service.CaptureDocumentVersion(second.Session.ProjectId);
        Assert.True(old.Cancellation.IsCancellationRequested);Assert.False(current.Cancellation.IsCancellationRequested);
        Assert.Equal("Changed",Assert.Throws<DocumentAccessException>(()=>service.EnsureCurrent(old)).Code);
        service.EnsureCurrent(current);service.SelectProject(first.Session.ProjectId);
        Assert.True(current.Cancellation.IsCancellationRequested);service.EnsureCurrent(service.CaptureDocumentVersion(first.Session.ProjectId));
        Assert.True(a.IsUnchanged());Assert.True(b.IsUnchanged());Assert.Empty(Directory.GetFiles(a.Output));Assert.Empty(Directory.GetFiles(b.Output));
    }
    [Fact]
    public async Task RenamingRecognizedDocumentOutOfCatalogInvalidatesVersion()
    {
        var fixture=DocumentaryFixture.Create();using var service=Service(new Picker());var project=service.OpenSelected(fixture.Origin,fixture.Output);
        var version=service.CaptureDocumentVersion(project.Session.ProjectId);
        var changed=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);project.Monitor.Changed+=()=>changed.TrySetResult();
        File.Move(Path.Combine(fixture.Feature,"spec.md"),Path.Combine(fixture.Feature,"notes.md"));
        await changed.Task.WaitAsync(TimeSpan.FromSeconds(5));Assert.True(project.Catalog.HasPendingChanges);
        Assert.Equal("Changed",Assert.Throws<DocumentAccessException>(()=>service.EnsureCurrent(version)).Code);
        Assert.True(File.Exists(Path.Combine(fixture.Feature,"notes.md")));Assert.False(File.Exists(Path.Combine(fixture.Feature,"spec.md")));
    }
    [Fact]
    public async Task DocumentaryDirectoriesCreatedAfterOpeningAreObserved()
    {
        var fixture=DocumentaryFixture.Create();var origin=Path.Combine(fixture.Root,"empty-origin");Directory.CreateDirectory(origin);
        using var service=Service(new Picker());var project=service.OpenSelected(origin,fixture.Output);
        var changed=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);project.Monitor.Changed+=()=>changed.TrySetResult();
        var feature=Path.Combine(origin,"specs","003-new");Directory.CreateDirectory(feature);File.WriteAllText(Path.Combine(feature,"spec.md"),"new spec");
        await changed.Task.WaitAsync(TimeSpan.FromSeconds(5));Assert.True(project.Catalog.HasPendingChanges);
    }
    [Fact]
    public void DisposingServiceCancelsPendingWorkWithoutSaving()
    {
        var fixture=DocumentaryFixture.Create();var service=Service(new Picker());var project=service.OpenSelected(fixture.Origin,fixture.Output);
        var version=service.CaptureDocumentVersion(project.Session.ProjectId);service.Dispose();service.Dispose();
        Assert.True(version.Cancellation.IsCancellationRequested);Assert.True(fixture.IsUnchanged());Assert.Empty(Directory.GetFiles(fixture.Output));
    }
    [Fact]
    public async Task RemovingDocumentDirectoryWithDotInNameInvalidatesCatalog()
    {
        var fixture=DocumentaryFixture.Create();var folder=Path.Combine(fixture.Feature,"evidence","round.1");Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"proof.md"),"proof");
        using var service=Service(new Picker());var project=service.OpenSelected(fixture.Origin,fixture.Output);
        var changed=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);project.Monitor.Changed+=()=>changed.TrySetResult();
        Directory.Move(folder,Path.Combine(fixture.Root,"parked-proof"));await changed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(project.Catalog.HasPendingChanges);
    }
    [Fact]
    public async Task RefreshSynchronizesAddedFeaturesAndPreservesExistingIdentities()
    {
        var fixture=DocumentaryFixture.Create();using var service=Service(new Picker());var project=service.OpenSelected(fixture.Origin,fixture.Output);
        var original=project.Session.Features.ToArray();var spec=project.Catalog.Entries.Single(e=>e.RelativePath.Replace('\\','/')=="specs/001-demo/spec.md");
        var changed=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);project.Monitor.Changed+=()=>changed.TrySetResult();
        var added=Path.Combine(fixture.Origin,"specs","003-added");Directory.CreateDirectory(added);File.WriteAllText(Path.Combine(added,"spec.md"),"added");
        await changed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        // Concurrent watcher delivery must reject a refresh, never commit a mixed catalog.
        // Retry the explicit refresh only after allowing the external write batch to settle.
        var deadline=DateTime.UtcNow.AddSeconds(5);
        while(true)
        {
            try{service.Refresh(project.Session.ProjectId);break;}
            catch(DocumentAccessException error) when(error.Code=="Changed" && DateTime.UtcNow<deadline)
            {Assert.True(project.Catalog.HasPendingChanges);await Task.Delay(100);}
        }
        Assert.Equal(3,project.Session.Features.Count);Assert.All(original,f=>Assert.Contains(f,project.Session.Features));
        Assert.Contains(project.Catalog.Entries,e=>e.DocumentId==spec.DocumentId);
        Assert.Contains(project.Session.Features,f=>f.SpecKitRelativeDirectory.Replace('\\','/')=="specs/003-added");
    }
}
