using Microsoft.Playwright;
namespace CemgSpec.Ui.Tests;
public sealed class HostBaselineTests
{
    [Fact]
    public async Task BrowserAutomationRuntimeCanInitialize()
    {
        using var playwright = await Playwright.CreateAsync();
        Assert.NotNull(playwright.Chromium);
    }
}
