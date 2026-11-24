using Microsoft.Playwright;
using CemgSpec.Ui.Tests.Fixtures;
using CemgSpec.Windows.Tests.Fixtures;
using static Microsoft.Playwright.Assertions;

namespace CemgSpec.Ui.Tests;

[Collection("US1 native dialogs")]
public sealed class BusinessDashboardTests(LocalHostFixture host) : IClassFixture<LocalHostFixture>
{
    [Fact]
    public Task FR001ToFR006BusinessViewToggleGlossaryStoriesAndExecutiveExportWorkWithoutTouchingOrigin() =>
        WithLoadedProjectAsync(async (page, fixture) =>
    {
        var nav = page.GetByRole(AriaRole.Navigation, new() { Name = "Navegação principal" });
        await nav.GetByRole(AriaRole.Link, new() { Name = "Dashboard", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard", Exact = true })).ToBeVisibleAsync();

        // FR-001: Switch to Business View (BA)
        await page.GetByRole(AriaRole.Button, new() { Name = "Visão de Negócio (BA)" }).ClickAsync();

        // FR-002 & FR-003: Business KPIs and User Story table
        await Expect(page.GetByRole(AriaRole.Main)).ToContainTextAsync("Histórias prontas");
        await Expect(page.GetByRole(AriaRole.Main)).ToContainTextAsync("Aguardando validação DEV");
        await Expect(page.GetByRole(AriaRole.Main)).ToContainTextAsync("Aguardando aceite PO");
        await Expect(page.GetByRole(AriaRole.Main)).ToContainTextAsync("Entregas por História de Usuário");

        // FR-005: Glossary dialog
        await page.GetByRole(AriaRole.Button, new() { Name = "Glossário" }).ClickAsync();
        var glossary = page.GetByRole(AriaRole.Dialog, new() { Name = "Glossário de Indicadores" });
        await Expect(glossary).ToBeVisibleAsync();
        await glossary.GetByRole(AriaRole.Button, new() { Name = "Fechar" }).ClickAsync();
        await Expect(glossary).ToBeHiddenAsync();

        // FR-006: Explicit executive summary export to external destination
        await page.GetByRole(AriaRole.Button, new() { Name = "Exportar Resumo Executivo" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Main)).ToContainTextAsync("Resumo Executivo exportado");
        Assert.NotEmpty(Directory.EnumerateFiles(fixture.Output, "CemgSpec-Resumo-Executivo-*.html"));
        Assert.True(fixture.IsUnchanged());
    });

    private async Task WithLoadedProjectAsync(Func<IPage, DocumentaryFixture, Task> verify)
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

            await verify(page, fixture);
        }
        finally
        {
            await context.CloseAsync();
            for (var attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    if (Directory.Exists(fixture.Root))
                        Directory.Delete(fixture.Root, recursive: true);
                    break;
                }
                catch (IOException) when (attempt < 4)
                {
                    await Task.Delay(200);
                }
            }
        }
    }

    private async Task PickAsync(IPage page, string button, string title, string folder, string pathTestId)
    {
        await page.GetByRole(AriaRole.Button, new() { Name = button, Exact = true }).ClickAsync();
        await US1IntegratedHarnessTests.AutomatedNativePicker.SelectInBrowserHostAsync(host.ProcessId, title, folder);
        await Expect(page.GetByTestId(pathTestId)).ToHaveTextAsync(folder, new() { IgnoreCase = true });
    }
}
