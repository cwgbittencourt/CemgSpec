using System.Reflection;
using Microsoft.AspNetCore.Http;
using SpecAudit.Application;
using SpecAudit.Components.Pages;
using SpecAudit.Windows.Tests.Fixtures;
namespace SpecAudit.Ui.Tests;
public sealed class ExplicitSelectionFlowTests
{
    private sealed class Picker(params string?[] paths):IFolderPicker {private Queue<string?> pending=new(paths);public Task<string?> PickAsync(string title)=>Task.FromResult(pending.Dequeue());}
    private const BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
    private static Task Act(Projects page,string name)=>(Task)typeof(Projects).GetMethod(name,Flags)!.Invoke(page,null)!;
    private static object? Field(Projects page,string name)=>typeof(Projects).GetField(name,Flags)!.GetValue(page);
    [Fact]
    public async Task RejectedDestinationRetainsActualSelectionsAndRetryLoadsSeparateContexts()
    {
        var a=DocumentaryFixture.Create();var b=DocumentaryFixture.Create();var registry=new LocalSessionRegistry();var issued=registry.Issue();var http=new DefaultHttpContext();http.Items[typeof(LocalSessionService)]=issued.Id;
        using var service=new ProjectService(new LocalSessionService(new HttpContextAccessor{HttpContext=http},registry),new Picker(a.Origin,a.Origin,a.Output,b.Origin,b.Output));
        var page=new Projects();typeof(Projects).GetProperty("ProjectStore",Flags)!.SetValue(page,service);
        await Act(page,"Open");await Act(page,"ChooseOrigin");Assert.Equal(a.Origin,Field(page,"pendingOrigin"));Assert.Null(Field(page,"pendingOutput"));Assert.Empty(service.Projects);
        await Act(page,"ChooseOutput");await Act(page,"ConfirmOpen");
        Assert.NotNull(Field(page,"error"));Assert.Equal(a.Origin,Field(page,"pendingOrigin"));Assert.Equal(a.Origin,Field(page,"pendingOutput"));Assert.True((bool)Field(page,"opening")!);Assert.Empty(service.Projects);
        await Act(page,"ChooseOutput");await Act(page,"ConfirmOpen");var first=service.Active!;Assert.Equal(a.Origin,first.Session.OriginDisplayPath);Assert.Equal(a.Output,first.OutputDisplayPath);
        await Act(page,"Open");await Act(page,"ChooseOrigin");await Act(page,"ChooseOutput");await Act(page,"ConfirmOpen");var second=service.Active!;
        Assert.Equal(b.Origin,second.Session.OriginDisplayPath);Assert.Equal(b.Output,second.OutputDisplayPath);Assert.Equal(2,service.Projects.Count);
        service.SelectProject(first.Session.ProjectId);Assert.Same(first,service.Active);Assert.Equal(a.Output,service.Active.OutputDisplayPath);service.SelectProject(second.Session.ProjectId);Assert.Same(second,service.Active);
        Assert.True(a.IsUnchanged());Assert.True(b.IsUnchanged());
    }
}
