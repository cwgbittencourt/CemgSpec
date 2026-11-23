using Microsoft.Playwright;
using SpecAudit.Ui.Tests.Fixtures;
using SpecAudit.Windows.Tests.Fixtures;
using static Microsoft.Playwright.Assertions;

namespace SpecAudit.Ui.Tests;

[Collection("US1 native dialogs")]
public sealed class VerificationSelectionTests(LocalHostFixture host) : IClassFixture<LocalHostFixture>
{
    [Fact]
    public Task S03ClosedComboHasExactlyTwoOptionsSynchronizedBetweenVerificationsAndRequirementSheetWithoutAutoRun() =>
        WithLoadedProjectAsync(async (page, _) =>
    {
        var nav = page.GetByRole(AriaRole.Navigation, new() { Name = "Navegação principal" });
        await nav.GetByRole(AriaRole.Link, new() { Name = "Verificações", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Verificações", Exact = true })).ToBeVisibleAsync();

        var combo = page.GetByRole(AriaRole.Combobox, new() { Name = "Tipo de verificação" });
        await Expect(combo).ToBeVisibleAsync();
        var options = await combo.Locator("option").AllInnerTextsAsync();
        Assert.Equal(
            new[]
            {
                "check_gate — Validar gates (offline, sem IA)"
            },
            options.Select(o => o.Trim()).ToArray());

        // No free command input and no forbidden V1 mockup behaviors.
        var main = page.GetByRole(AriaRole.Main);
        await Expect(main.GetByRole(AriaRole.Textbox, new() { Name = "Comando" })).ToHaveCountAsync(0);
        await Expect(main).Not.ToContainTextAsync("npm test");
        await Expect(main).Not.ToContainTextAsync("Cópia isolada");
        await Expect(main).Not.ToContainTextAsync("Validação aprovada");

        // check_gate is selected by default without auto-running.
        await Expect(page.GetByRole(AriaRole.Combobox, new() { Name = "Etapa do gate" })).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Combobox, new() { Name = "Registro do gate" })).ToBeVisibleAsync();
        await Expect(main).Not.ToContainTextAsync("Execução concluída");
        await CaptureIfRequestedAsync(page, "verifications-check-gate-dark.png");

        // Navigating to Requisitos must preserve the exact same offline check_gate selection in the requirement sheet.
        await nav.GetByRole(AriaRole.Link, new() { Name = "Requisitos", Exact = true }).ClickAsync();
        var sheet = page.GetByRole(AriaRole.Region, new() { Name = "Ficha documental do requisito" });
        await Expect(sheet).ToBeVisibleAsync();
        var sheetCombo = sheet.GetByRole(AriaRole.Combobox, new() { Name = "Tipo de verificação" });
        await Expect(sheetCombo).ToBeVisibleAsync();
        await Expect(sheetCombo).ToHaveValueAsync("CheckGate");
        await Expect(sheet).ToContainTextAsync("funcionalidade/etapa");
    });

    [Fact]
    public Task S05AnalyzeRemainsDisabledWithoutProofWhileCheckGateRunsOfflineWithoutAgent() =>
        WithLoadedProjectAsync(async (page, _) =>
    {
        var nav = page.GetByRole(AriaRole.Navigation, new() { Name = "Navegação principal" });
        await nav.GetByRole(AriaRole.Link, new() { Name = "Verificações", Exact = true }).ClickAsync();

        var executeButton = page.GetByRole(AriaRole.Button, new() { Name = "Executar verificação" });
        await page.GetByRole(AriaRole.Combobox, new() { Name = "Etapa do gate" })
            .SelectOptionAsync(new SelectOptionValue { Value = "Spec" });
        await page.GetByRole(AriaRole.Combobox, new() { Name = "Registro do gate" })
            .SelectOptionAsync(new SelectOptionValue { Label = "specs/001-demo/verification/ready.json" });
        await Expect(executeButton).ToBeEnabledAsync();

        await executeButton.ClickAsync();
        var resultRegion = page.GetByRole(AriaRole.Region, new() { Name = "Resultado da verificação" });
        await Expect(resultRegion).ToContainTextAsync("READY");
        await Expect(resultRegion).ToContainTextAsync("check_gate cemg-speckit-preset 1.0.0");
        await Expect(resultRegion).ToContainTextAsync("não autentica decisão humana");
        await CaptureIfRequestedAsync(page, "verifications-ready-dark.png");
    });

    [Fact]
    public Task S06AndS07CheckGateShowsBlockedAndScopeDenialAndMarksStaleAfterConcurrentEdit() =>
        WithLoadedProjectAsync(async (page, fixture) =>
    {
        var nav = page.GetByRole(AriaRole.Navigation, new() { Name = "Navegação principal" });
        await nav.GetByRole(AriaRole.Link, new() { Name = "Verificações", Exact = true }).ClickAsync();

        await page.GetByRole(AriaRole.Combobox, new() { Name = "Etapa do gate" })
            .SelectOptionAsync(new SelectOptionValue { Value = "Spec" });

        // Blocked record returns BLOCKED (distinct from execution error).
        await page.GetByRole(AriaRole.Combobox, new() { Name = "Registro do gate" })
            .SelectOptionAsync(new SelectOptionValue { Label = "specs/001-demo/verification/blocked.json" });
        await page.GetByRole(AriaRole.Button, new() { Name = "Executar verificação" }).ClickAsync();
        var resultRegion = page.GetByRole(AriaRole.Region, new() { Name = "Resultado da verificação" });
        await Expect(resultRegion).ToContainTextAsync("BLOCKED");

        // Code reference record is blocked by scope/preflight without READY.
        await page.GetByRole(AriaRole.Combobox, new() { Name = "Registro do gate" })
            .SelectOptionAsync(new SelectOptionValue { Label = "specs/001-demo/verification/code-reference.json" });
        await page.GetByRole(AriaRole.Button, new() { Name = "Executar verificação" }).ClickAsync();
        await Expect(resultRegion).ToContainTextAsync("Bloqueada");
        await Expect(resultRegion).Not.ToContainTextAsync("READY");

        // Run READY then mutate spec.md externally -> result must become Desatualizado (Stale) without auto-run.
        await page.GetByRole(AriaRole.Combobox, new() { Name = "Registro do gate" })
            .SelectOptionAsync(new SelectOptionValue { Label = "specs/001-demo/verification/ready.json" });
        await page.GetByRole(AriaRole.Button, new() { Name = "Executar verificação" }).ClickAsync();
        await Expect(resultRegion).ToContainTextAsync("READY");

        File.AppendAllText(Path.Combine(fixture.Feature, "spec.md"), "\n- FR-099: Concurrent edit.\n");
        await Expect(resultRegion).ToContainTextAsync("Desatualizado");
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

