using System.Collections.Concurrent;
using System.Text;
using System.Net;
using System.Net.Sockets;
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
            click A "https://preview-fixture.invalid/diagram"
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
            <svg><image href="https://preview-fixture.invalid/svg.png"></image></svg>
            <div style="background-image:url(https://preview-fixture.invalid/css.png)">Texto</div>

            ![Local](design/fixture.png)

            ![Remota](http://preview-fixture.invalid/controlled.png)
            ![Vetorial](design/fixture.svg)
            ![Fora](../../outside.png)

            [Externo](https://preview-fixture.invalid/guide)
            """;
        var png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAADElEQVR4nGPw7/kPAAMIAdtZLTAmAAAAAElFTkSuQmCC");
        var origin = Directory.CreateTempSubdirectory("SpecAudit-T023-");
        try
        {
            await CheckPreviewAsync(origin, source, png);
        }
        finally
        {
            origin.Delete(recursive: true);
        }
    }

    private async Task CheckPreviewAsync(DirectoryInfo origin, string source, byte[] png)
    {
        var feature = Directory.CreateDirectory(Path.Combine(origin.FullName, "specs", "001-preview"));
        var design = Directory.CreateDirectory(Path.Combine(feature.FullName, "design"));
        var markdown = Path.Combine(feature.FullName, "spec.md");
        var raster = Path.Combine(design.FullName, "fixture.png");
        File.WriteAllText(markdown, source);
        File.WriteAllBytes(raster, png);
        File.WriteAllText(Path.Combine(design.FullName, "fixture.svg"), "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>");
        File.WriteAllBytes(Path.Combine(origin.FullName, "outside.png"), png);

        using var catalog = new DocumentCatalog(Guid.NewGuid(), origin.FullName);
        var reads = new List<string>();
        var broker = new DocumentBroker(catalog)
        {
            AccessObserved = (path, operation, result) =>
            {
                if (operation == "read" && result == "Authorized") reads.Add(path);
            }
        };
        var session = broker.OpenPreview(Assert.Single(catalog.Entries, entry => entry.Kind == CatalogKind.Spec).DocumentId);
        var preview = session.Preview;
        Assert.Contains(raster, reads);
        Assert.All(reads, path => Assert.Contains(path, new[] { markdown, raster }));
        var remote = Assert.Single(preview.References, reference => reference.Kind == PreviewReferenceKind.RemoteImage);
        var externalLink = Assert.Single(preview.References, reference => reference.Kind == PreviewReferenceKind.ExternalLink);
        Assert.Equal(PreviewLinkStatus.External, session.ResolveLink(externalLink.Index).Status);

        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        var connections = 0;
        var loader = new RemotePreviewImages(async (destination, cancellationToken) =>
        {
            Assert.Equal("preview-fixture.invalid", destination.Host);
            Assert.Equal(80, destination.Port);
            Interlocked.Increment(ref connections);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await socket.ConnectAsync(endpoint, cancellationToken);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        });

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

        await page.Context.AddCookiesAsync([new() { Name = "session_fixture", Value = "synthetic", Url = remote.Target }]);
        await page.ExposeFunctionAsync("authorizeFixtureImage", async () =>
            (await session.AuthorizeRemoteImageAsync(remote.Index, loader)).DataUri);
        await page.GotoAsync(host.Address.ToString());
        await page.SetContentAsync("<link rel=\"stylesheet\" href=\"/lib/preview/preview.css\">"
            + "<main class=\"markdown-preview\">" + preview.Html + "</main><button type=\"button\">Carregar nesta sessão</button>");
        await page.EvaluateAsync("""
            async () => {
                const preview = await import('/lib/preview/preview.js');
                await preview.enhancePreview(document.querySelector('main'));
                document.querySelector('body > button').addEventListener('click', async () => {
                    const image = document.createElement('img');
                    image.alt = 'Remota autorizada';
                    image.src = await window.authorizeFixtureImage();
                    document.querySelector('main').append(image);
                }, { once: true });
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
        await Expect(first.FrameLocator("iframe").Locator("svg a")).ToHaveCountAsync(0);
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
        await Expect(page.Locator("main svg, main [style]:not(iframe):not(.preview-diagram-viewport div)")).ToHaveCountAsync(0);
        Assert.Empty(external);
        Assert.Equal(0, Volatile.Read(ref connections));
        Assert.False(listener.Pending());

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var request = RespondWithImageAsync(listener, png, timeout.Token);
        await page.GetByRole(AriaRole.Button, new() { Name = "Carregar nesta sessão", Exact = true }).ClickAsync();
        var headers = await request;
        await Expect(page.GetByRole(AriaRole.Img, new() { Name = "Remota autorizada" }))
            .ToHaveJSPropertyAsync("naturalWidth", 1);
        Assert.Equal(1, Volatile.Read(ref connections));
        Assert.Equal("GET /controlled.png HTTP/1.1", headers[0]);
        Assert.DoesNotContain(headers, header => new[] { "Cookie:", "Authorization:", "Proxy-Authorization:", "Referer:" }
            .Any(name => header.StartsWith(name, StringComparison.OrdinalIgnoreCase)));
        Assert.Empty(external);
        var redirect = RespondWithImageAsync(listener, [], timeout.Token, redirect: true);
        var refused = await Assert.ThrowsAsync<DocumentAccessException>(() =>
            session.AuthorizeRemoteImageAsync(remote.Index, loader, timeout.Token));
        Assert.Equal("RemoteImageUnavailable", refused.Code);
        Assert.Equal("GET /controlled.png HTTP/1.1", (await redirect)[0]);
        Assert.Equal(2, Volatile.Read(ref connections));
        Assert.False(listener.Pending());
        Assert.Equal(source, File.ReadAllText(markdown));
        Assert.Equal(png, File.ReadAllBytes(raster));
    }

    [Theory]
    [InlineData("127.0.0.1", false)]
    [InlineData("10.0.0.1", false)]
    [InlineData("169.254.169.254", false)]
    [InlineData("192.0.2.1", false)]
    [InlineData("::ffff:127.0.0.1", false)]
    [InlineData("fe80::1", false)]
    [InlineData("2001:db8::1", false)]
    [InlineData("8.8.8.8", true)]
    [InlineData("2606:4700::1111", true)]
    public void RemoteTransportOnlyDialsPublicAddresses(string address, bool expected)
    {
        Assert.Equal(expected, RemotePreviewImages.IsPublic(IPAddress.Parse(address)));
    }

    private static async Task<List<string>> RespondWithImageAsync(TcpListener listener, byte[] png, CancellationToken cancellationToken, bool redirect = false)
    {
        using var peer = await listener.AcceptTcpClientAsync(cancellationToken);
        using var stream = peer.GetStream();
        using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
        var headers = new List<string>();
        while (await reader.ReadLineAsync(cancellationToken) is { Length: > 0 } header)
            headers.Add(header);
        var response = redirect
            ? Encoding.ASCII.GetBytes("HTTP/1.1 302 Found\r\nLocation: http://preview-fixture.invalid/redirect.png\r\nContent-Length: 0\r\nConnection: close\r\n\r\n")
            : Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: image/png\r\nContent-Length: {png.Length}\r\nConnection: close\r\n\r\n");
        await stream.WriteAsync(response, cancellationToken);
        await stream.WriteAsync(png, cancellationToken);
        return headers;
    }
}
