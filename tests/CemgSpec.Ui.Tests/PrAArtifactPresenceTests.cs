using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CemgSpec.Application;
using CemgSpec.Components.Pages;
using CemgSpec.Windows.Tests.Fixtures;
namespace CemgSpec.Ui.Tests;
public sealed class PrAArtifactPresenceTests
{
    private sealed class Picker : IFolderPicker { public Task<string?> PickAsync(string title)=>Task.FromResult<string?>(null); }
    private static ProjectService Service()
    {
        var registry=new LocalSessionRegistry();var issued=registry.Issue();var context=new DefaultHttpContext();context.Items[typeof(LocalSessionService)]=issued.Id;
        return new(new LocalSessionService(new HttpContextAccessor{HttpContext=context},registry),new Picker());
    }
    private static async Task<string> Render(ProjectService service)
    {
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton(service);
        await using var provider=services.BuildServiceProvider();
        await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async()=> (await renderer.RenderComponentAsync<Projects>()).ToHtmlString());
    }
    [Theory]
    [InlineData("spec.md")]
    [InlineData("plan.md")]
    [InlineData("tasks.md")]
    public async Task RenderedArtifactStatusUsesFeatureRootAndWindowsCaseRules(string name)
    {
        var missing=DocumentaryFixture.Create();
        File.Move(Path.Combine(missing.Feature,name),Path.Combine(missing.Root,"parked-"+name));
        var nested=Path.Combine(missing.Feature,"evidence","round.1");Directory.CreateDirectory(nested);File.WriteAllText(Path.Combine(nested,name),"historical artifact");missing.CaptureBaseline();
        using var service=Service();service.OpenSelected(missing.Origin,missing.Output);
        var html=await Render(service);
        Assert.Matches("<td>"+System.Text.RegularExpressions.Regex.Escape(name)+"</td><td><span class=\"missing-state\">Ausente</span>",html);
        Assert.True(missing.IsUnchanged());Assert.Empty(Directory.GetFiles(missing.Output));
        var present=DocumentaryFixture.Create();
        var original=Path.Combine(present.Feature,name);var parked=Path.Combine(present.Root,"renamed-"+name);File.Move(original,parked);File.Move(parked,Path.Combine(present.Feature,name.ToUpperInvariant()));present.CaptureBaseline();
        service.OpenSelected(present.Origin,present.Output);
        html=await Render(service);
        Assert.Matches("<td>"+System.Text.RegularExpressions.Regex.Escape(name)+"</td><td><span class=\"present-state\">Presente</span>",html);
        Assert.True(present.IsUnchanged());Assert.Empty(Directory.GetFiles(present.Output));
    }
}
