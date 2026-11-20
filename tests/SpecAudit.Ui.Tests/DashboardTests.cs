using Microsoft.Playwright;
using SpecAudit.Ui.Tests.Fixtures;
using SpecAudit.Windows.Tests.Fixtures;
using static Microsoft.Playwright.Assertions;

namespace SpecAudit.Ui.Tests;

[Collection("US1 native dialogs")]
public sealed class DashboardTests(LocalHostFixture host) : IClassFixture<LocalHostFixture>
{
    [Fact]
    public Task S01DashboardShowsDeclaredProgressDocumentaryCoverageAndNoItemsForEmptyScope() =>
        WithDashboardFixtureAsync(async (page, _) =>
    {
        var main = page.GetByRole(AriaRole.Main);
        await Expect(main.GetByRole(AriaRole.Heading, new() { Name = "Dashboard", Exact = true })).ToBeVisibleAsync();

        // 30/40 tasks marked complete = 75% declared; 12/20 requirements explicitly linked = 60% documentary coverage.
        await Expect(main).ToContainTextAsync("75%");
        await Expect(main).ToContainTextAsync("30 / 40");
        await Expect(main).ToContainTextAsync("60%");
        await Expect(main).ToContainTextAsync("12 / 20");
        await Expect(main).ToContainTextAsync("Cobertura documental");

        // Mandatory V1 substitutions from design/mockups/README.md: never claim evidenced implementation or approved validation.
        await Expect(main).Not.ToContainTextAsync("Implementação evidenciada");
        await Expect(main).Not.ToContainTextAsync("Validação aprovada");

        // Empty feature scope (003-empty) must show "Sem itens" and never a percentage.
        var featureFilter = page.GetByRole(AriaRole.Combobox, new() { Name = "Funcionalidade" });
        await featureFilter.SelectOptionAsync(new SelectOptionValue { Label = "003-empty" });
        await Expect(main).ToContainTextAsync("Sem itens");
        await Expect(main).Not.ToContainTextAsync("0%");
        await Expect(main).Not.ToContainTextAsync("NaN%");
    });

    [Fact]
    public Task S02DashboardShowsRecordedGateStatesWithProvenanceWithoutClaimingImplementation() =>
        WithDashboardFixtureAsync(async (page, _) =>
    {
        var gates = page.GetByRole(AriaRole.Region, new() { Name = "Estados documentais e gates registrados" });
        await Expect(gates).ToBeVisibleAsync();
        await Expect(gates).ToContainTextAsync("READY");
        await Expect(gates).ToContainTextAsync("BLOCKED");
        await Expect(gates).ToContainTextAsync("ready.json");
        await Expect(gates).ToContainTextAsync("não comprova");
        await Expect(gates).Not.ToContainTextAsync("Implementação evidenciada");
    });

    [Fact]
    public Task S03DashboardListsDocumentaryFindingsAndLeavesCodeReferencesUnverified() =>
        WithDashboardFixtureAsync(async (page, _) =>
    {
        var findings = page.GetByRole(AriaRole.Region, new() { Name = "Ocorrências documentais" });
        await Expect(findings).ToBeVisibleAsync();
        await Expect(findings).ToContainTextAsync("Synthetic blocker");
        await Expect(findings).ToContainTextAsync("FR-013");
        await Expect(findings).ToContainTextAsync("T013");
        await Expect(findings).ToContainTextAsync("DoNotRead.cs");
        await Expect(findings).ToContainTextAsync("não verificada nesta versão");
    });

    [Fact]
    public Task S04DashboardFiltersAndRefreshesOnDocumentChangeWithoutRunningAgentsOrGates() =>
        WithDashboardFixtureAsync(async (page, fixture) =>
    {
        var severityFilter = page.GetByRole(AriaRole.Combobox, new() { Name = "Gravidade" });
        await Expect(severityFilter).ToBeVisibleAsync();

        // External edit marks a 31st task complete; dashboard must detect change and update counts on explicit refresh without running agents/gates.
        var tasksPath = Path.Combine(fixture.Feature, "tasks.md");
        var original = File.ReadAllText(tasksPath);
        var updated = original.Replace("- [ ] T031 ", "- [x] T031 ", StringComparison.Ordinal);
        File.WriteAllText(tasksPath, updated);

        try
        {
            var refreshButton = page.GetByRole(AriaRole.Button, new() { Name = "Atualizar documentos" });
            await Expect(refreshButton).ToBeVisibleAsync();
            await refreshButton.ClickAsync();
            await Expect(page.GetByRole(AriaRole.Main)).ToContainTextAsync("31 / 40");
            Assert.Equal(updated, File.ReadAllText(tasksPath));
            Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Output));
        }
        finally
        {
            File.WriteAllText(tasksPath, original);
        }
    });

    private async Task WithDashboardFixtureAsync(Func<IPage, DocumentaryFixture, Task> verify)
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
        var response = await page.GotoAsync(new Uri(host.Address, "/dashboard").ToString());
        Assert.True(response?.Ok == true, $"The Dashboard route must be available. HTTP {response?.Status}.");

        var fixture = DocumentaryFixture.Create();
        try
        {
            // Add an empty feature (003-empty) for the NoItems scenario and a duplicate requirement / code reference for S03.
            var emptyFeature = Directory.CreateDirectory(Path.Combine(fixture.Origin, "specs", "003-empty"));
            File.WriteAllText(Path.Combine(emptyFeature.FullName, "spec.md"), "# Empty feature\n");
            File.WriteAllText(Path.Combine(emptyFeature.FullName, "tasks.md"), "# Tasks\n");
            fixture.CaptureBaseline();

            await host.OpenProjectPanelAsync(page);
            await PickAsync(page, "Escolher projeto", "Selecionar PROJETO (origem somente leitura)", fixture.Origin, "selected-origin");
            await PickAsync(page, "Escolher destino", "Selecionar RELATORIOS (destino externo)", fixture.Output, "selected-output");
            await page.GetByRole(AriaRole.Button, new() { Name = "Confirmar carregamento" }).ClickAsync();
            await Expect(page.GetByRole(AriaRole.Combobox, new() { Name = "Funcionalidade" })).ToBeVisibleAsync();
            await page.GetByRole(AriaRole.Link, new() { Name = "Dashboard", Exact = true }).ClickAsync();

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

    private async Task PickAsync(IPage page, string button, string title, string folder, string pathTestId)
    {
        await page.GetByRole(AriaRole.Button, new() { Name = button, Exact = true }).ClickAsync();
        await US1IntegratedHarnessTests.AutomatedNativePicker.SelectInBrowserHostAsync(host.ProcessId, title, folder);
        await Expect(page.GetByTestId(pathTestId)).ToHaveTextAsync(folder, new() { IgnoreCase = true });
    }
}

