using System.Collections.Concurrent;
using System.Text;
using Microsoft.Playwright;
using SpecAudit.Domain;
using SpecAudit.Infrastructure.Documents;
using SpecAudit.Ui.Tests.Fixtures;
using static Microsoft.Playwright.Assertions;

namespace SpecAudit.Ui.Tests;

public sealed class PreviewAssetsTests(LocalHostFixture host) : IClassFixture<LocalHostFixture>
{
    [Fact]
    public async Task RendersOnlyMermaidWithLocalAssetsAndKeepsFallbackSource()
    {
        const string source = """
            # Corpus

            ```csharp
            int example = 42;
            ```

            ```mermaid
            flowchart LR
            A[Início] --> B[Fim]
            ```

            ```mermaid
            sequenceDiagram
            Alice->>Bob: Mensagem
            ```

            ```mermaid
            flowchart LR
            A[
            ```

            ```plantuml
            A -> B
            ```

            ```graphviz
            digraph { A -> B }
            ```

            <script>window.fixtureCanary = true;</script>
            <form><input name="fixture"></form>

            ![Local](design/fixture.png)

            ![Remota](https://preview-fixture.invalid/controlled.png)
            """;
        var png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAADElEQVR4nGPw7/kPAAMIAdtZLTAmAAAAAElFTkSuQmCC");
        var local = PreviewRasterImage.FromSnapshot(DocumentSnapshot.Create(Guid.NewGuid(), png, 0));
        Assert.NotNull(local);
        var preview = new MarkdownRenderer().Render(DocumentSnapshot.Create(
            Guid.NewGuid(), Encoding.UTF8.GetBytes(source), 0),
            new Dictionary<string, PreviewRasterImage> { ["design/fixture.png"] = local });
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Channel = "msedge", Headless = true });
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = 1440, Height = 1000 } });
        var external = new ConcurrentQueue<string>();
        await page.RouteAsync("**/*", async route =>
        {
            if (host.Address.IsBaseOf(new Uri(route.Request.Url)))
                await route.ContinueAsync();
            else
            {
                external.Enqueue(route.Request.Url);
                await route.AbortAsync();
            }
        });

        await page.GotoAsync(host.Address.ToString());
        await page.SetContentAsync("<link rel=\"stylesheet\" href=\"/lib/preview/preview.css\">"
            + "<main class=\"markdown-preview\">" + preview.Html + "</main>");
        await page.EvaluateAsync("""
            async () => {
                const preview = await import('/lib/preview/preview.js');
                await preview.enhancePreview(document.querySelector('main'));
            }
            """);

        await Expect(page.Locator("code.hljs span").First).ToBeVisibleAsync();
        var diagrams = page.GetByRole(AriaRole.Region, new() { Name = "Diagrama Mermaid", Exact = true });
        await Expect(diagrams).ToHaveCountAsync(2);
        var first = diagrams.First;
        var frame = first.Locator("iframe");
        await Expect(frame).ToHaveAttributeAsync("sandbox", "allow-scripts");
        await Expect(first.FrameLocator("iframe").Locator("svg")).ToContainTextAsync("Início");
        await Expect(first.FrameLocator("iframe").Locator("svg")).ToContainTextAsync("Fim");
        await Expect(diagrams.Last.FrameLocator("iframe").Locator("svg")).ToContainTextAsync("Mensagem");
        var width = (await frame.BoundingBoxAsync())!.Width;
        await first.GetByRole(AriaRole.Button, new() { Name = "Aumentar zoom" }).ClickAsync();
        Assert.True((await frame.BoundingBoxAsync())!.Width > width);
        await first.GetByRole(AriaRole.Button, new() { Name = "Ver fonte" }).ClickAsync();
        await Expect(first.Locator("code")).ToContainTextAsync("flowchart LR");

        var invalid = page.GetByRole(AriaRole.Region, new() { Name = "Mermaid inválido", Exact = true });
        await Expect(invalid.Locator("code")).ToContainTextAsync("A[");
        await Expect(invalid.GetByRole(AriaRole.Status)).ToContainTextAsync("inválido");
        foreach (var (format, code) in new[] { ("PlantUML", "A -> B"), ("Graphviz", "digraph { A -> B }") })
        {
            var fallback = page.GetByRole(AriaRole.Region, new() { Name = format, Exact = true });
            await Expect(fallback.Locator("code")).ToContainTextAsync(code);
            await Expect(fallback.GetByRole(AriaRole.Status)).ToContainTextAsync("não suportado");
            await Expect(fallback.Locator("iframe, svg")).ToHaveCountAsync(0);
        }
        await Expect(page.Locator("main script, main form")).ToHaveCountAsync(0);
        await Expect(page.GetByRole(AriaRole.Img, new() { Name = "Local", Exact = true }))
            .ToHaveJSPropertyAsync("naturalWidth", 1);
        await Expect(page.GetByRole(AriaRole.Img, new() { Name = "Remota", Exact = true })).ToHaveCountAsync(0);
        Assert.False(await page.EvaluateAsync<bool>("() => window.fixtureCanary === true"));
        Assert.Empty(external);
    }
}
