using Microsoft.Playwright;
using SpecAudit.Ui.Tests.Fixtures;
namespace SpecAudit.Ui.Tests;
public sealed class ProjectPageBrowserTests : IClassFixture<LocalHostFixture>
{
    private readonly LocalHostFixture host;
    public ProjectPageBrowserTests(LocalHostFixture host)=>this.host=host;
    [Fact]
    public async Task RealBrowserRendersLocalProjectSelectionWithoutCircuitErrors()
    {
        using var playwright=await Playwright.CreateAsync();
        await using var browser=await playwright.Chromium.LaunchAsync(new(){Channel="msedge",Headless=true});
        var page=await browser.NewPageAsync(new(){ViewportSize=new(){Width=1440,Height=1000}});
        var errors=new List<string>();page.PageError+=(_,error)=>errors.Add(error);
        await page.GotoAsync(host.Address.ToString());
        await page.GetByRole(AriaRole.Button,new(){Name="Carregar projeto",Exact=true}).WaitForAsync();
        await page.WaitForTimeoutAsync(1000);
        Assert.Equal("Projetos — CEMG Spec",await page.TitleAsync());
        Assert.True(await page.GetByRole(AriaRole.Heading,new(){Name="Escolha um projeto para começar"}).IsVisibleAsync());
        Assert.Empty(errors);
        Assert.NotEqual("block",await page.Locator("#blazor-error-ui").EvaluateAsync<string>("element => getComputedStyle(element).display"));
        var screenshot=Path.Combine(host.Root,"specs","001-auditar-projetos-speckit","evidence","logs","T011-project-page.png");
        await page.ScreenshotAsync(new(){Path=screenshot,FullPage=true});
    }
    [Fact]
    public async Task BrowserShowsExplicitSelectionStagesAndCancelsWithoutLoading()
    {
        using var playwright=await Playwright.CreateAsync();
        await using var browser=await playwright.Chromium.LaunchAsync(new(){Channel="msedge",Headless=true});
        var page=await browser.NewPageAsync();var errors=new List<string>();page.PageError+=(_,error)=>errors.Add(error);
        var panel = await host.OpenProjectPanelAsync(page);
        Assert.Equal("Nenhuma pasta selecionada",await page.GetByTestId("selected-origin").InnerTextAsync());
        Assert.Equal("Nenhuma pasta selecionada",await page.GetByTestId("selected-output").InnerTextAsync());
        Assert.False(await page.GetByRole(AriaRole.Button,new(){Name="Escolher destino",Exact=true}).IsEnabledAsync());
        Assert.False(await page.GetByRole(AriaRole.Button,new(){Name="Confirmar carregamento",Exact=true}).IsEnabledAsync());
        await page.GetByRole(AriaRole.Button,new(){Name="Cancelar carregamento",Exact=true}).ClickAsync();
        await panel.WaitForAsync(new(){State=WaitForSelectorState.Detached});
        Assert.Equal(0,await panel.CountAsync());Assert.Empty(errors);
    }
}
