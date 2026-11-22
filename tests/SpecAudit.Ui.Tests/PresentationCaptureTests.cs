using Microsoft.Playwright;
using SpecAudit.Ui.Tests.Fixtures;
using static Microsoft.Playwright.Assertions;

namespace SpecAudit.Ui.Tests;

[Collection("US1 native dialogs")]
public sealed class PresentationCaptureTests(LocalHostFixture host) : IClassFixture<LocalHostFixture>
{
    [Fact]
    public async Task CaptureHighResPresentationScreenshotsWhenRequested()
    {
        if (Environment.GetEnvironmentVariable("SPECAUDIT_CAPTURE_PRESENTATION") != "1")
            return;

        var origin = @"C:\Projetos\Inicial";
        var output = @"C:\Projetos\Relatorios\Teste";
        var outDir = @"d:\Codex_Workspace\Projetos\SpecAudit\docs\prints";
        Directory.CreateDirectory(outDir);
        Directory.CreateDirectory(output);

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Channel = "msedge", Headless = true });
        await using var context = await browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1440, Height = 900 },
            DeviceScaleFactor = 2,
            ServiceWorkers = ServiceWorkerPolicy.Block
        });

        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(15000);

        // 01. Projetos (inicial com Projetos recentes)
        await host.OpenProjectPanelAsync(page);
        await page.GetByRole(AriaRole.Button, new() { Name = "Cancelar carregamento" }).ClickAsync();
        await page.WaitForTimeoutAsync(400);
        await page.ScreenshotAsync(new() { Path = Path.Combine(outDir, "01.png") });

        // Carregar C:\Projetos\Inicial + C:\Projetos\Relatorios\Teste
        await page.GetByRole(AriaRole.Button, new() { Name = "Carregar projeto" }).ClickAsync();
        await PickAsync(page, "Escolher projeto", "Selecionar PROJETO (origem somente leitura)", origin, "selected-origin");
        await PickAsync(page, "Escolher destino", "Selecionar RELATORIOS (destino externo)", output, "selected-output");
        await page.GetByRole(AriaRole.Button, new() { Name = "Confirmar carregamento" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Combobox, new() { Name = "Funcionalidade" })).ToBeVisibleAsync();
        await page.WaitForTimeoutAsync(400);

        // 02. Projeto carregado
        await page.ScreenshotAsync(new() { Path = Path.Combine(outDir, "02.png") });

        var nav = page.GetByRole(AriaRole.Navigation, new() { Name = "Navegação principal" });

        // 03. Dashboard — Visão Técnica
        await nav.GetByRole(AriaRole.Link, new() { Name = "Dashboard", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard", Exact = true })).ToBeVisibleAsync();
        await page.WaitForTimeoutAsync(400);
        await page.ScreenshotAsync(new() { Path = Path.Combine(outDir, "03.png") });

        // 04. Dashboard — Visão de Negócio (BA)
        await page.GetByRole(AriaRole.Button, new() { Name = "Visão de Negócio (BA)" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Main)).ToContainTextAsync("Entregas por História de Usuário");
        await page.WaitForTimeoutAsync(400);
        await page.ScreenshotAsync(new() { Path = Path.Combine(outDir, "04.png") });

        // 05. Glossário de Indicadores
        await page.GetByRole(AriaRole.Button, new() { Name = "Glossário" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Dialog, new() { Name = "Glossário de Indicadores" })).ToBeVisibleAsync();
        await page.WaitForTimeoutAsync(300);
        await page.ScreenshotAsync(new() { Path = Path.Combine(outDir, "05.png") });
        await page.GetByRole(AriaRole.Button, new() { Name = "Fechar" }).ClickAsync();

        // 06. Requisitos — Resumo (RF-002)
        await nav.GetByRole(AriaRole.Link, new() { Name = "Requisitos", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Requisitos", Exact = true })).ToBeVisibleAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "RF-002" }).ClickAsync();
        await page.WaitForTimeoutAsync(400);
        await page.ScreenshotAsync(new() { Path = Path.Combine(outDir, "06.png") });

        // 07. Requisitos — Aba Vínculos
        await page.GetByRole(AriaRole.Tab, new() { Name = "Vínculos" }).ClickAsync();
        await page.WaitForTimeoutAsync(300);
        await page.ScreenshotAsync(new() { Path = Path.Combine(outDir, "07.png") });

        // 08. Documentos — Leitor Seguro
        await nav.GetByRole(AriaRole.Link, new() { Name = "Documentos", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Documentos", Exact = true })).ToBeVisibleAsync();
        await page.WaitForTimeoutAsync(500);
        await page.ScreenshotAsync(new() { Path = Path.Combine(outDir, "08.png") });

        // 09. Sugestões e Histórico Salvo
        await nav.GetByRole(AriaRole.Link, new() { Name = "Sugestões", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Sugestões", Exact = true })).ToBeVisibleAsync();
        await page.WaitForTimeoutAsync(400);
        await page.ScreenshotAsync(new() { Path = Path.Combine(outDir, "09.png") });
    }

    private async Task PickAsync(IPage page, string button, string title, string folder, string pathTestId)
    {
        await page.GetByRole(AriaRole.Button, new() { Name = button, Exact = true }).ClickAsync();
        await US1IntegratedHarnessTests.AutomatedNativePicker.SelectInBrowserHostAsync(host.ProcessId, title, folder);
        await Expect(page.GetByTestId(pathTestId)).ToHaveTextAsync(folder, new() { IgnoreCase = true });
    }
}
