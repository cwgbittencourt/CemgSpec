using Microsoft.Playwright;
using CemgSpec.Ui.Tests.Fixtures;
using CemgSpec.Windows.Tests.Fixtures;
using static Microsoft.Playwright.Assertions;

namespace CemgSpec.Ui.Tests;

[Collection("US1 native dialogs")]
public sealed class RequirementSheetTests(LocalHostFixture host) : IClassFixture<LocalHostFixture>
{
    [Fact]
    public Task S01RequirementSheetSeparatesSameLocalIdAcrossFeaturesAndDistinguishesExplicitVsInferred() =>
        WithRequirementsFixtureAsync(async (page, _) =>
    {
        var main = page.GetByRole(AriaRole.Main);
        await Expect(main.GetByRole(AriaRole.Heading, new() { Name = "Requisitos", Exact = true })).ToBeVisibleAsync();

        // Select FR-001 in 001-demo and verify explicit vs inferred distinction and V1 substitutions.
        var sheet = page.GetByRole(AriaRole.Region, new() { Name = "Ficha documental do requisito" });
        await Expect(sheet).ToBeVisibleAsync();
        await Expect(sheet).ToContainTextAsync("FR-001");
        await Expect(sheet).ToContainTextAsync("Explícito");
        await Expect(sheet).ToContainTextAsync("001-demo");
        await Expect(sheet).Not.ToContainTextAsync("Evidências da implementação");
        await Expect(sheet).Not.ToContainTextAsync("npm test");
        await Expect(sheet).Not.ToContainTextAsync("Cópia isolada");
        await CaptureIfRequestedAsync(page, "requirements-fr001.png");

        // Switch feature to 002-other: FR-001 must show its own independent description, never merged with 001-demo.
        await page.GetByRole(AriaRole.Combobox, new() { Name = "Funcionalidade" })
            .SelectOptionAsync(new SelectOptionValue { Label = "002-other" });
        await Expect(sheet).ToContainTextAsync("Independent identity");
        await Expect(sheet).Not.ToContainTextAsync("Documentary requirement 1");
        await CaptureIfRequestedAsync(page, "requirements-other-fr001.png");
    });

    [Fact]
    public Task S02RequirementSheetShowsMissingAndAmbiguousCriteriaWithoutImplementationVerdict() =>
        WithRequirementsFixtureAsync(async (page, _) =>
    {
        var sheet = page.GetByRole(AriaRole.Region, new() { Name = "Ficha documental do requisito" });
        await page.GetByRole(AriaRole.Button, new() { Name = "FR-013" }).ClickAsync();
        await Expect(sheet).ToContainTextAsync("Evidência documental insuficiente");
        await Expect(sheet).Not.ToContainTextAsync("Não implementado");
    });

    [Fact]
    public Task S03RequirementSheetShowsRecordedDeclarationsWithLimitsWithoutCemgSpecApproval() =>
        WithRequirementsFixtureAsync(async (page, _) =>
    {
        var sheet = page.GetByRole(AriaRole.Region, new() { Name = "Ficha documental do requisito" });
        await Expect(sheet).ToContainTextAsync("registro de terceiros");
        await Expect(sheet).ToContainTextAsync("não aprova");
    });

    [Fact]
    public Task S04RequirementSheetKeepsCodeLinksUnverifiedAndInformsFutureCodeAnalysis() =>
        WithRequirementsFixtureAsync(async (page, _) =>
    {
        var sheet = page.GetByRole(AriaRole.Region, new() { Name = "Ficha documental do requisito" });
        await Expect(sheet).ToContainTextAsync("DoNotRead.cs");
        await Expect(sheet).ToContainTextAsync("não verificada");
        await Expect(sheet).ToContainTextAsync("capacidade futura");
    });

    private async Task WithRequirementsFixtureAsync(Func<IPage, DocumentaryFixture, Task> verify)
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
        var response = await page.GotoAsync(new Uri(host.Address, "/requisitos").ToString());
        Assert.True(response?.Ok == true, $"The Requisitos route must be available. HTTP {response?.Status}.");

        var fixture = DocumentaryFixture.Create();
        try
        {
            // Add a documentary mention of DoNotRead.cs in plan.md to test S04 unverified code reference.
            File.WriteAllText(Path.Combine(fixture.Feature, "plan.md"),
                "# Demo plan\n\n- FR-001: Referência documental ao plano e menção a ../../DoNotRead.cs.\n");
            fixture.CaptureBaseline();

            await host.OpenProjectPanelAsync(page);
            await PickAsync(page, "Escolher projeto", "Selecionar PROJETO (origem somente leitura)", fixture.Origin, "selected-origin");
            await PickAsync(page, "Escolher destino", "Selecionar RELATORIOS (destino externo)", fixture.Output, "selected-output");
            await page.GetByRole(AriaRole.Button, new() { Name = "Confirmar carregamento" }).ClickAsync();
            await Expect(page.GetByRole(AriaRole.Combobox, new() { Name = "Funcionalidade" })).ToBeVisibleAsync();
            await page.GetByRole(AriaRole.Link, new() { Name = "Requisitos", Exact = true }).ClickAsync();

            await verify(page, fixture);
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

