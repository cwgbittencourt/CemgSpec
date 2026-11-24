using Microsoft.Playwright;
using CemgSpec.Ui.Tests.Fixtures;
using CemgSpec.Windows.Tests.Fixtures;
using static Microsoft.Playwright.Assertions;

namespace CemgSpec.Ui.Tests;

[Collection("US1 native dialogs")]
public sealed class SaveHistoryTests(LocalHostFixture host) : IClassFixture<LocalHostFixture>
{
    [Fact]
    public Task S01AndS02SuggestionsStayInMemoryUntilExplicitSaveAndExportSelectedFormatsExternally() =>
        WithLoadedProjectAsync(async (page, fixture) =>
    {
        var nav = page.GetByRole(AriaRole.Navigation, new() { Name = "Navegação principal" });
        await nav.GetByRole(AriaRole.Link, new() { Name = "Sugestões", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Sugestões", Exact = true })).ToBeVisibleAsync();

        // Nothing saved automatically before explicit Save confirmation.
        Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Output));

        var saveButton = page.GetByRole(AriaRole.Button, new() { Name = "Salvar resultados" });
        await Expect(saveButton).ToBeEnabledAsync();
        await saveButton.ClickAsync();

        var dialog = page.GetByRole(AriaRole.Dialog, new() { Name = "Confirmar salvamento externo" });
        await Expect(dialog).ToBeVisibleAsync();
        await Expect(dialog).ToContainTextAsync(fixture.Output);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Confirmar e salvar" }).ClickAsync();

        await Expect(page.GetByRole(AriaRole.Region, new() { Name = "Histórico salvo" })).ToContainTextAsync("CemgSpec-");
        Assert.NotEmpty(Directory.EnumerateDirectories(fixture.Output, "CemgSpec-*"));
        Assert.True(fixture.IsUnchanged());
        await CaptureIfRequestedAsync(page, "suggestions-saved-dark.png");
    });

    [Fact]
    public Task S03AndS04ClassifiesFindingsRequiresReasonComparesAndDeletesSelectedRecord() =>
        WithLoadedProjectAsync(async (page, fixture) =>
    {
        var nav = page.GetByRole(AriaRole.Navigation, new() { Name = "Navegação principal" });
        await nav.GetByRole(AriaRole.Link, new() { Name = "Sugestões", Exact = true }).ClickAsync();

        var classification = page.GetByRole(AriaRole.Combobox, new() { Name = "Classificação da ocorrência" }).First;
        await Expect(classification).ToBeVisibleAsync();
        await classification.SelectOptionAsync(new SelectOptionValue { Value = "FalsePositive" });
        await page.GetByRole(AriaRole.Button, new() { Name = "Registrar decisão" }).First.ClickAsync();
        await Expect(page.GetByRole(AriaRole.Alert)).ToContainTextAsync("justificativa");

        await page.GetByRole(AriaRole.Textbox, new() { Name = "Justificativa" }).First.FillAsync("Falso positivo conferido manualmente");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Responsável" }).First.FillAsync("dev-humano");
        await page.GetByRole(AriaRole.Button, new() { Name = "Registrar decisão" }).First.ClickAsync();
        await Expect(page.GetByRole(AriaRole.Main)).ToContainTextAsync("Falso positivo");

        // Save twice to create two distinct records, then delete only one.
        for (var i = 0; i < 2; i++)
        {
            await page.GetByRole(AriaRole.Button, new() { Name = "Salvar resultados" }).ClickAsync();
            var dialog = page.GetByRole(AriaRole.Dialog, new() { Name = "Confirmar salvamento externo" });
            await dialog.GetByRole(AriaRole.Button, new() { Name = "Confirmar e salvar" }).ClickAsync();
        }

        Assert.Equal(2, Directory.GetDirectories(fixture.Output, "CemgSpec-*").Length);
        var history = page.GetByRole(AriaRole.Region, new() { Name = "Histórico salvo" });
        await history.GetByRole(AriaRole.Button, new() { Name = "Excluir registro" }).First.ClickAsync();
        await Expect(history).ToContainTextAsync("Histórico salvo (1)");
        Assert.Single(Directory.GetDirectories(fixture.Output, "CemgSpec-*"));
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

    private static async Task CaptureIfRequestedAsync(IPage page, string fileName)
    {
        var directory = Environment.GetEnvironmentVariable("CEMGSPEC_CAPTURE_DIR");
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
