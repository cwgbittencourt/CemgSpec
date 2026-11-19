using System.Text;
using SpecAudit.Domain;
using SpecAudit.Infrastructure.Documents;

namespace SpecAudit.Tests;

public sealed class MarkdownRendererTests
{
    [Fact]
    public void RendersDocumentStructureAndPreservesMarkdownSource()
    {
        const string source = """
            # Título
            ## Entrega

            - Item
            - [x] Feita
            - [ ] Pendente

            | Cabeçalho | Segundo |
            | --- | --- |
            | Célula | Valor |

            > Uma citação

            **Destaque** e <em>HTML básico</em>.

            ![Figura](design/figura.png)

            ```csharp
            int example = 42;
            ```
            """;
        var raster = PreviewRasterImage.FromSnapshot(DocumentSnapshot.Create(Guid.NewGuid(),
            Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAADElEQVR4nGPw7/kPAAMIAdtZLTAmAAAAAElFTkSuQmCC"), 0));
        Assert.NotNull(raster);
        var preview = new MarkdownRenderer().Render(Snapshot(source),
            new Dictionary<string, PreviewRasterImage> { ["design/figura.png"] = raster });

        Assert.Equal(source, preview.Source);
        Assert.Equal(new[] { 1, 2 }, preview.Headings.Select(heading => heading.Level));
        var heading = preview.Headings[1];
        Assert.Equal("entrega", heading.Anchor);
        Assert.Contains($"""<h2 id="{heading.ElementId}">Entrega</h2>""", preview.Html);
        Assert.Contains("<li>Item</li>", preview.Html);
        Assert.Contains("<input type=\"checkbox\" disabled checked>", preview.Html);
        Assert.Contains("<input type=\"checkbox\" disabled>", preview.Html);
        Assert.Contains("<td>Valor</td>", preview.Html);
        Assert.Contains("<blockquote>", preview.Html);
        Assert.Contains("<strong>Destaque</strong>", preview.Html);
        Assert.Contains("<em>HTML b&#225;sico</em>", preview.Html);
        Assert.Contains("<img src=\"data:image/png;base64,", preview.Html);
        Assert.Contains("alt=\"Figura\"", preview.Html);
        Assert.Contains("<code class=\"language-csharp\">int example = 42;", preview.Html);
    }

    [Fact]
    public void DropsActiveMarkupAndKeepsDestinationsInert()
    {
        const string source = """
            # Referências

            [Seção](#referências)
            [Plano](plan.md)
            [Guia](https://example.invalid/guide)
            ![Local](design/photo.png)
            ![Remota](https://example.invalid/photo.png)

            <div onclick="ignored()" style="color: red"><strong>Visível</strong></div>
            <script>window.fixtureCanary = true;</script>
            <form><input name="fixture"><button>Enviar</button></form>
            <iframe src="https://example.invalid/frame"></iframe>
            <svg><a href="https://example.invalid/svg">SVG</a></svg>
            <a href="javascript:void(0)">Ação bloqueada</a>
            <img src="//example.invalid/automatic.png" alt="Automática">
            """;
        var preview = new MarkdownRenderer().Render(Snapshot(source));

        foreach (var removed in new[] { "<script", "<form", "<iframe", "<svg", "<img", "onclick=", "style=", "href=", "src=", "example.invalid" })
            Assert.DoesNotContain(removed, preview.Html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<strong>Vis&#237;vel</strong>", preview.Html);
        Assert.Contains("data-preview-reference=", preview.Html);
        Assert.Collection(preview.References,
            item => Assert.Equal(PreviewReferenceKind.DocumentLink, item.Kind),
            item => Assert.Equal(PreviewReferenceKind.DocumentLink, item.Kind),
            item => Assert.Equal(PreviewReferenceKind.ExternalLink, item.Kind),
            item => Assert.Equal(PreviewReferenceKind.LocalImage, item.Kind),
            item => Assert.Equal(PreviewReferenceKind.RemoteImage, item.Kind),
            item => Assert.Equal(PreviewReferenceKind.Blocked, item.Kind),
            item => Assert.Equal(PreviewReferenceKind.Blocked, item.Kind));
        Assert.Null(PreviewRasterImage.FromSnapshot(Snapshot("<svg></svg>")));
    }

    [Fact]
    public void ReportsInvalidUtf8()
    {
        var snapshot = DocumentSnapshot.Create(Guid.NewGuid(), [0xC3, 0x28], 0);
        Assert.Equal("InvalidEncoding", Assert.Throws<DocumentAccessException>(() =>
            new MarkdownRenderer().Render(snapshot)).Code);
    }

    private static DocumentSnapshot Snapshot(string source) =>
        DocumentSnapshot.Create(Guid.NewGuid(), Encoding.UTF8.GetBytes(source), 0);
}
