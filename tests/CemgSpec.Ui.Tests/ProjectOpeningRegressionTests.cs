using Microsoft.AspNetCore.Http;
using CemgSpec.Application;
using CemgSpec.Windows.Tests.Fixtures;
namespace CemgSpec.Ui.Tests;
public sealed class ProjectOpeningRegressionTests
{
    private sealed class Picker : IFolderPicker
    {
        public int Calls;
        public TaskCompletionSource<string?> First=new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task<string?> PickAsync(string title)=>++Calls==1?First.Task:Task.FromResult<string?>(null);
    }
    private static ProjectService Service(IFolderPicker picker)
    {
        var registry=new LocalSessionRegistry();var issued=registry.Issue();var http=new DefaultHttpContext();http.Items[typeof(LocalSessionService)]=issued.Id;
        return new(new LocalSessionService(new HttpContextAccessor{HttpContext=http},registry),picker);
    }
    [Fact]
    public void ReopeningSameOriginSelectsExistingContextWithoutDuplicateAndKeepsOtherProject()
    {
        var a=DocumentaryFixture.Create();var b=DocumentaryFixture.Create();using var service=Service(new Picker());
        var first=service.OpenSelected(a.Origin,a.Output);var feature=first.Session.Features.Last();first.Session.SelectFeature(feature.FeatureId);
        var second=service.OpenSelected(b.Origin,b.Output);
        var reopened=service.OpenSelected(Path.Combine(a.Origin,"."),a.Evidence);
        Assert.Same(first,reopened);Assert.Same(first,service.Active);Assert.Equal(2,service.Projects.Count);
        Assert.Equal(feature.FeatureId,first.Session.ActiveFeatureId);Assert.Equal(a.Evidence,first.OutputDisplayPath);Assert.Equal(b.Output,second.OutputDisplayPath);
        Assert.True(a.IsUnchanged());Assert.True(b.IsUnchanged());
    }
    [Fact]
    public async Task PendingFolderFlowRejectsSecondOpeningAndResetsAfterCancel()
    {
        var picker=new Picker();using var service=Service(picker);var pending=service.OpenProjectAsync();
        try { await Assert.ThrowsAsync<InvalidOperationException>(()=>service.OpenProjectAsync());Assert.Equal(1,picker.Calls); }
        finally {picker.First.TrySetResult(null);await pending;}
        Assert.Null(await service.OpenProjectAsync());Assert.Empty(service.Projects);
    }
}
