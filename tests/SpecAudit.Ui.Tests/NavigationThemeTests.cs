using Microsoft.Playwright;
using SpecAudit.Ui.Tests.Fixtures;
using SpecAudit.Windows.Tests.Fixtures;
using static Microsoft.Playwright.Assertions;

namespace SpecAudit.Ui.Tests;

[Collection("US1 native dialogs")]
public sealed class NavigationThemeTests(LocalHostFixture host) : IClassFixture<LocalHostFixture>
{
    [Fact]
    public Task S01FiveAreasKeepProjectContextReadOnlyBadgeAndUnavailablePlaceholders() =>
        WithLoadedProjectAsync(async page =>
    {
        var nav = page.GetByRole(AriaRole.Navigation, new() { Name = "Navegação principal" });
        foreach (var area in new[] { "Dashboard", "Requisitos", "Documentos", "Verificações", "Sugestões" })
            await Expect(nav.GetByRole(AriaRole.Link, new() { Name = area, Exact = true })).ToBeVisibleAsync();

        // Traverse all five areas and confirm project context + read-only protection stay visible.
        await nav.GetByRole(AriaRole.Link, new() { Name = "Dashboard", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard", Exact = true })).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Main)).ToContainTextAsync("Somente leitura");

        await nav.GetByRole(AriaRole.Link, new() { Name = "Requisitos", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Requisitos", Exact = true })).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Main)).ToContainTextAsync("Somente leitura");

        await nav.GetByRole(AriaRole.Link, new() { Name = "Documentos", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Documentos", Exact = true })).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Main)).ToContainTextAsync("Somente leitura");

        await nav.GetByRole(AriaRole.Link, new() { Name = "Verificações", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Verificações", Exact = true })).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Executar verificação" })).ToBeDisabledAsync();

        await nav.GetByRole(AriaRole.Link, new() { Name = "Sugestões", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Sugestões", Exact = true })).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Salvar resultados" })).ToBeEnabledAsync();
    });

    [Fact]
    public Task S02ThemeToggleSwitchesDarkAndLightPreservingSelectionAndStates() =>
        WithLoadedProjectAsync(async page =>
    {
        var nav = page.GetByRole(AriaRole.Navigation, new() { Name = "Navegação principal" });
        await nav.GetByRole(AriaRole.Link, new() { Name = "Requisitos", Exact = true }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "FR-013" }).ClickAsync();
        var sheet = page.GetByRole(AriaRole.Region, new() { Name = "Ficha documental do requisito" });
        await Expect(sheet).ToContainTextAsync("FR-013");
        await Expect(sheet).ToContainTextAsync("Evidência documental insuficiente");

        var shell = page.Locator(".app-shell");
        await Expect(shell).ToHaveAttributeAsync("data-theme", "dark");
        var darkButton = page.GetByRole(AriaRole.Button, new() { Name = "Escuro", Exact = true });
        var lightButton = page.GetByRole(AriaRole.Button, new() { Name = "Claro", Exact = true });
        await Expect(darkButton).ToHaveAttributeAsync("aria-pressed", "true");
        await Expect(lightButton).ToHaveAttributeAsync("aria-pressed", "false");
        await CaptureIfRequestedAsync(page, "theme-dark-requirements.png");

        await lightButton.ClickAsync();
        await Expect(shell).ToHaveAttributeAsync("data-theme", "light");
        await Expect(lightButton).ToHaveAttributeAsync("aria-pressed", "true");
        await Expect(darkButton).ToHaveAttributeAsync("aria-pressed", "false");
        // Selection and textual states must be preserved across theme toggle.
        await Expect(sheet).ToContainTextAsync("FR-013");
        await Expect(sheet).ToContainTextAsync("Evidência documental insuficiente");
        var bg = await shell.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        Assert.Equal("rgb(247, 248, 250)", bg);
        await CaptureIfRequestedAsync(page, "theme-light-requirements.png");

        // Also capture Dashboard in light theme when requested.
        await nav.GetByRole(AriaRole.Link, new() { Name = "Dashboard", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard", Exact = true })).ToBeVisibleAsync();
        await Expect(shell).ToHaveAttributeAsync("data-theme", "light");
        await CaptureIfRequestedAsync(page, "theme-light-dashboard.png");

        // Switch back to dark and verify persistence in session.
        await darkButton.ClickAsync();
        await Expect(shell).ToHaveAttributeAsync("data-theme", "dark");
    });

    private async Task WithLoadedProjectAsync(Func<IPage, Task> verify)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Channel = "msedge", Headless = true });
        await using var context = await browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1440, Height = 1000 },
            ServiceWorkers = ServiceWorkerPolicy.Block
        });

        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(10000);
        var fixture = DocumentaryFixture.Create();
        try
        {
            await host.OpenProjectPanelAsync(page);
            await PickAsync(page, "Escolher projeto", "Selecionar PROJETO (origem somente leitura)", fixture.Origin, "selected-origin");
            await PickAsync(page, "Escolher destino", "Selecionar RELATORIOS (destino externo)", fixture.Output, "selected-output");
            await page.GetByRole(AriaRole.Button, new() { Name = "Confirmar carregamento" }).ClickAsync();
            await Expect(page.GetByRole(AriaRole.Combobox, new() { Name = "Funcionalidade" })).ToBeVisibleAsync();

            await verify(page);
            Assert.True(fixture.IsUnchanged());
            Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Output));
        }
        finally
        {
            await context.CloseAsync();
            Directory.Delete(fixture.Root, recursive: true);
        }
    }

    private static async Task CaptureIfRequestedAsync(IPage page, string fileName)
    {
        var directory = Environment.GetEnvironmentVariable("SPECAUDIT_CAPTURE_DIR");
        if (string.IsNullOrWhiteSpace(directory)) return;
        Directory.CreateDirectory(directory);
        await page.ScreenshotAsync(new() { Path = Path.Combine(directory, fileName), FullPage = true });
    }

    private async Task PickAsync(IPage page, string button, string title, string folder, string pathTestId)
    {
        await page.GetByRole(AriaRole.Button, new() { Name = button, Exact = true }).ClickAsync();
        await US1IntegratedHarnessTests.AutomatedNativePicker.SelectInBrowserHostAsync(host.ProcessId, title, folder);
        await Expect(page.GetByTestId(pathTestId)).ToHaveTextAsync(folder, new() { IgnoreCase = true });
    }
}

