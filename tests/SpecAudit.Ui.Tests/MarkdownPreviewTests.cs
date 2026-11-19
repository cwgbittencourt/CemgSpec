using System.Collections.Concurrent;
using Microsoft.Playwright;
using SpecAudit.Ui.Tests.Fixtures;
using SpecAudit.Windows.Tests.Fixtures;
using static Microsoft.Playwright.Assertions;

namespace SpecAudit.Ui.Tests;

[Collection("US1 native dialogs")]
public sealed class MarkdownPreviewTests(LocalHostFixture host) : IClassFixture<LocalHostFixture>
{
    private const string RemoteImage = "https://preview-fixture.test/controlled.png";
    private const string ExternalLink = "https://docs.preview-fixture.test/guide";
    private const string Corpus = """
        # Corpus de preview

        ## Detalhes

        - Item da lista
        - [x] Tarefa concluída
        - [ ] Tarefa pendente

        | Coluna A | Coluna B |
        | --- | --- |
        | Valor A | Valor B |

        > Citação documental

        ![Imagem local](design/sample.png)

        ```csharp
        int example = 42;
        ```

        [Plano autorizado](plan.md#entrega)

        [Âncora local](#detalhes)

        [Referência ausente](absent.md)

        [Referência fora do catálogo](../../DoNotRead.cs)

        ```mermaid
        flowchart LR
        A[Início] --> B[Fim]
        ```

        ```mermaid
        flowchart LR
        A[
        ```

        ```plantuml
        A -> B
        ```

        ```dot
        digraph { A -> B }
        ```

        <strong>HTML básico preservado</strong>
        <script>window.__previewCanary = true;</script>
        <form><input name="preview-canary"><button>Enviar formulário</button></form>

        ![Imagem remota](https://preview-fixture.test/controlled.png)

        [Guia externo](https://docs.preview-fixture.test/guide)
        """;
    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAADElEQVR4nGPw7/kPAAMIAdtZLTAmAAAAAElFTkSuQmCC");

    [Fact]
    public Task S01SearchTreeAndFollowOnlyAuthorisedDocumentLinks() => WithCorpusAsync(async (page, _) =>
    {
        var tree = page.GetByRole(AriaRole.Tree, new() { Name = "Documentos" });
        await Expect(tree.GetByRole(AriaRole.Treeitem, new() { Name = "001-demo", Exact = true })).ToBeVisibleAsync();
        await Expect(tree.GetByRole(AriaRole.Treeitem, new() { Name = "002-other", Exact = true })).ToBeVisibleAsync();
        await Expect(tree.GetByText("DoNotRead.cs", new() { Exact = true })).ToHaveCountAsync(0);

        var search = page.GetByRole(AriaRole.Searchbox, new() { Name = "Buscar documentos" });
        await search.FillAsync("ready.json");
        await Expect(tree.GetByText("ready.json", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(tree.GetByText("research.md", new() { Exact = true })).ToHaveCountAsync(0);
        await search.FillAsync("research.md");
        await tree.GetByText("research.md", new() { Exact = true }).ClickAsync();
        await Mode(page, "Preview").ClickAsync();

        var preview = Preview(page);
        await preview.GetByRole(AriaRole.Link, new() { Name = "Âncora local" }).ClickAsync();
        await Expect(preview.Locator(":target")).ToHaveTextAsync("Detalhes");
        await preview.GetByText("Referência ausente", new() { Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Status).Filter(new() { HasText = "ausente" })).ToBeVisibleAsync();
        await preview.GetByText("Referência fora do catálogo", new() { Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Status).Filter(new() { HasText = "fora do catálogo" })).ToBeVisibleAsync();
        await preview.GetByRole(AriaRole.Link, new() { Name = "Plano autorizado" }).ClickAsync();
        await Expect(preview.GetByRole(AriaRole.Heading, new() { Name = "Plano da fixture" })).ToBeVisibleAsync();
        await Expect(preview.Locator(":target")).ToHaveTextAsync("Entrega");
    });

    [Fact]
    public Task S02PreviewSourceAndSplitUseTheSameReadOnlyDocument() => WithCorpusAsync(async (page, _) =>
    {
        await OpenCorpusAsync(page);
        var preview = Preview(page);
        var source = page.GetByRole(AriaRole.Region, new() { Name = "Fonte Markdown" });
        await Expect(source).ToBeHiddenAsync();
        await Expect(preview.GetByRole(AriaRole.Heading, new() { Name = "Corpus de preview", Level = 1 })).ToBeVisibleAsync();
        await Expect(preview.GetByRole(AriaRole.Listitem).Filter(new() { HasText = "Item da lista" })).ToBeVisibleAsync();
        var tasks = preview.GetByRole(AriaRole.Checkbox);
        await Expect(tasks).ToHaveCountAsync(2);
        await Expect(tasks.First).ToBeCheckedAsync();
        await Expect(tasks.Last).Not.ToBeCheckedAsync();
        await Expect(tasks.First).ToBeDisabledAsync();
        await Expect(tasks.Last).ToBeDisabledAsync();
        await Expect(preview.GetByRole(AriaRole.Cell, new() { Name = "Valor B" })).ToBeVisibleAsync();
        await Expect(preview.Locator("blockquote")).ToHaveTextAsync("Citação documental");
        await Expect(preview.Locator("pre code").Filter(new() { HasText = "int example = 42;" }).Locator("span").First).ToBeVisibleAsync();
        await Expect(preview.GetByRole(AriaRole.Img, new() { Name = "Imagem local" })).ToBeVisibleAsync();
        await Expect(preview.GetByRole(AriaRole.Img, new() { Name = "Imagem local" })).ToHaveJSPropertyAsync("complete", true);
        Assert.True(await preview.GetByRole(AriaRole.Img, new() { Name = "Imagem local" })
            .EvaluateAsync<bool>("image => image.naturalWidth > 0"));

        await Mode(page, "Fonte").ClickAsync();
        await Expect(preview).ToBeHiddenAsync();
        await ExpectSourceAsync(source);

        await Mode(page, "Lado a lado").ClickAsync();
        await Expect(preview.GetByRole(AriaRole.Heading, new() { Name = "Corpus de preview" })).ToBeVisibleAsync();
        await ExpectSourceAsync(source);
        var previewBox = await preview.BoundingBoxAsync();
        var sourceBox = await source.BoundingBoxAsync();
        Assert.NotNull(previewBox);
        Assert.NotNull(sourceBox);
        Assert.True(previewBox.X + previewBox.Width <= sourceBox.X || sourceBox.X + sourceBox.Width <= previewBox.X,
            "At the desktop viewport, both visible panes must be side by side.");
    });

    [Fact]
    public Task S03MermaidRendersOfflineAndOtherDiagramsShowSourceAndExplanation() =>
        WithCorpusAsync(async (page, remoteRequests) =>
    {
        await OpenCorpusAsync(page);
        var diagram = Preview(page).GetByRole(AriaRole.Region, new() { Name = "Diagrama Mermaid" });
        var drawing = diagram.Locator("svg");
        await Expect(drawing).ToBeVisibleAsync();
        await Expect(drawing).ToContainTextAsync("Início");
        await Expect(drawing).ToContainTextAsync("Fim");
        var width = (await drawing.BoundingBoxAsync())!.Width;
        await diagram.GetByRole(AriaRole.Button, new() { Name = "Aumentar zoom" }).ClickAsync();
        await page.WaitForFunctionAsync("([drawing, width]) => drawing.getBoundingClientRect().width > width",
            new object?[] { await drawing.ElementHandleAsync(), width });
        await diagram.GetByRole(AriaRole.Button, new() { Name = "Ver fonte" }).ClickAsync();
        await Expect(diagram.GetByText("flowchart LR", new() { Exact = false })).ToBeVisibleAsync();

        var preview = Preview(page);
        var invalid = preview.GetByRole(AriaRole.Region, new() { Name = "Mermaid inválido" });
        await Expect(invalid).ToContainTextAsync("A[");
        await Expect(invalid).ToContainTextAsync("inválido");
        foreach (var (format, text) in new[] { ("PlantUML", "A -> B"), ("Graphviz", "digraph { A -> B }") })
        {
            var unsupported = preview.GetByRole(AriaRole.Region, new() { Name = format });
            await Expect(unsupported).ToContainTextAsync(text);
            await Expect(unsupported).ToContainTextAsync("não suportado");
            await Expect(unsupported.Locator("svg")).ToHaveCountAsync(0);
        }

        Assert.Empty(remoteRequests);
    });

    [Fact]
    public Task S04ActiveContentIsInertAndExternalRequestsRequireAnAction() =>
        WithCorpusAsync(async (page, remoteRequests) =>
    {
        await page.Context.AddCookiesAsync([new() { Name = "preview_fixture_cookie", Value = "test", Url = RemoteImage }]);
        await OpenCorpusAsync(page);
        var preview = Preview(page);
        await Expect(preview.Locator("strong")).ToHaveTextAsync("HTML básico preservado");
        await Expect(preview.Locator("script, form, iframe, object, embed")).ToHaveCountAsync(0);
        Assert.False(await page.EvaluateAsync<bool>("() => window.__previewCanary === true"));

        var load = preview.GetByRole(AriaRole.Button, new() { Name = "Carregar nesta sessão" });
        await Expect(load).ToBeVisibleAsync();
        var external = preview.GetByRole(AriaRole.Link, new() { Name = "Guia externo" });
        await Expect(external).ToBeVisibleAsync();
        Assert.Empty(remoteRequests);

        var response = await page.RunAndWaitForResponseAsync(() => load.ClickAsync(), item => item.Url == RemoteImage);
        Assert.True(response.Ok);
        await Expect(preview.GetByRole(AriaRole.Img, new() { Name = "Imagem remota" })).ToBeVisibleAsync();
        var imageRequest = Assert.Single(remoteRequests);
        Assert.Equal(RemoteImage, imageRequest.Url);
        Assert.False(imageRequest.Headers.ContainsKey("cookie"));
        Assert.False(imageRequest.Headers.ContainsKey("authorization"));
        Assert.False(imageRequest.Headers.ContainsKey("referer"));

        var popup = await page.RunAndWaitForPopupAsync(() => external.ClickAsync());
        await popup.WaitForURLAsync(ExternalLink);
        Assert.Equal(host.Address.Authority, new Uri(page.Url).Authority);
        Assert.Equal(new[] { RemoteImage, ExternalLink }, remoteRequests.Select(request => request.Url));
    });

    [Fact]
    public Task BrowserCanPrepareCorpusWithNativeDialogs() =>
        WithCorpusAsync((_, _) => Task.CompletedTask, openDocuments: false);

    private async Task WithCorpusAsync(
        Func<IPage, ConcurrentQueue<(string Url, Dictionary<string, string> Headers)>, Task> verify, bool openDocuments = true)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Channel = "msedge", Headless = true });
        await using var context = await browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1440, Height = 1000 },
            ServiceWorkers = ServiceWorkerPolicy.Block
        });
        var requests = new ConcurrentQueue<(string Url, Dictionary<string, string> Headers)>();
        await context.RouteAsync("**/*", async route =>
        {
            var url = route.Request.Url;
            if (host.Address.IsBaseOf(new Uri(url)))
            {
                await route.ContinueAsync();
                return;
            }

            requests.Enqueue((url, await route.Request.AllHeadersAsync()));
            switch (url)
            {
                case RemoteImage:
                    await route.FulfillAsync(new() { ContentType = "image/png", BodyBytes = Png });
                    break;
                case ExternalLink:
                    await route.FulfillAsync(new() { ContentType = "text/html", Body = "<h1>Guia externo de teste</h1>" });
                    break;
                default:
                    await route.AbortAsync();
                    break;
            }
        });

        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(10000);
        if (openDocuments)
        {
            var response = await page.GotoAsync(new Uri(host.Address, "/documentos").ToString());
            Assert.True(response?.Ok == true, $"The Documentos route must be available. HTTP {response?.Status}.");
        }

        var fixture = DocumentaryFixture.Create();
        try
        {
            File.WriteAllText(Path.Combine(fixture.Feature, "research.md"), Corpus);
            File.WriteAllText(Path.Combine(fixture.Feature, "plan.md"), "# Plano da fixture\n\n## Entrega");
            var design = Directory.CreateDirectory(Path.Combine(fixture.Feature, "design"));
            File.WriteAllBytes(Path.Combine(design.FullName, "sample.png"), Png);
            fixture.CaptureBaseline();

            await host.OpenProjectPanelAsync(page);
            await PickAsync(page, "Escolher projeto", "Selecionar PROJETO (origem somente leitura)", fixture.Origin, "selected-origin");
            await PickAsync(page, "Escolher destino", "Selecionar RELATORIOS (destino externo)", fixture.Output, "selected-output");
            await page.GetByRole(AriaRole.Button, new() { Name = "Confirmar carregamento" }).ClickAsync();
            await Expect(page.GetByRole(AriaRole.Combobox, new() { Name = "Funcionalidade" })).ToBeVisibleAsync();
            if (openDocuments)
                await page.GetByRole(AriaRole.Link, new() { Name = "Documentos", Exact = true }).ClickAsync();

            await verify(page, requests);
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
        await Expect(page.GetByTestId(pathTestId)).ToHaveTextAsync(folder);
    }

    private static async Task OpenCorpusAsync(IPage page)
    {
        await page.GetByRole(AriaRole.Searchbox, new() { Name = "Buscar documentos" }).FillAsync("research.md");
        await page.GetByRole(AriaRole.Tree, new() { Name = "Documentos" })
            .GetByText("research.md", new() { Exact = true }).ClickAsync();
        await Mode(page, "Preview").ClickAsync();
    }

    private static ILocator Preview(IPage page) =>
        page.GetByRole(AriaRole.Region, new() { Name = "Preview Markdown" });

    private static ILocator Mode(IPage page, string name) =>
        page.GetByRole(AriaRole.Tab, new() { Name = name, Exact = true });

    private static async Task ExpectSourceAsync(ILocator source)
    {
        await Expect(source).ToBeVisibleAsync();
        Assert.Equal(Corpus.ReplaceLineEndings("\n"), (await source.Locator("pre").TextContentAsync())?.ReplaceLineEndings("\n"));
        await Expect(source.Locator("textarea, input, [contenteditable='true']")).ToHaveCountAsync(0);
    }
}
