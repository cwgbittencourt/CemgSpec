using System.Net;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using CemgSpec.Domain;

namespace CemgSpec.Infrastructure.Documents;

public enum PreviewReferenceKind
{
    DocumentLink,
    ExternalLink,
    LocalImage,
    RemoteImage,
    Blocked
}

public sealed record PreviewReference(int Index, PreviewReferenceKind Kind, string Target, string Label);
public sealed record PreviewHeading(string Anchor, string ElementId, string Text, int Level);
public sealed record MarkdownPreview(string Source, string Html, IReadOnlyList<PreviewHeading> Headings,
    IReadOnlyList<PreviewReference> References);

public sealed class PreviewRasterImage
{
    internal string DataUri { get; }

    private PreviewRasterImage(string mediaType, byte[] bytes)
    {
        DataUri = $"data:{mediaType};base64,{Convert.ToBase64String(bytes)}";
    }

    public static PreviewRasterImage? FromSnapshot(DocumentSnapshot snapshot)
    {
        var bytes = snapshot.CopyBytes();
        var data = bytes.AsSpan();
        if (data.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            return new("image/png", bytes);
        if (data.StartsWith(new byte[] { 255, 216, 255 }))
            return new("image/jpeg", bytes);
        if (data.Length >= 12 && data.StartsWith("RIFF"u8) && data[8..].StartsWith("WEBP"u8))
            return new("image/webp", bytes);
        return null;
    }
}

public sealed class MarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
        .UsePipeTables()
        .UseTaskLists()
        .Build();

    private static readonly UTF8Encoding Utf8 = new(false, throwOnInvalidBytes: true);
    private static readonly HashSet<string> AllowedElements = new(StringComparer.Ordinal)
    {
        "article", "blockquote", "br", "caption", "code", "dd", "del", "details", "div", "dl",
        "dt", "em", "h1", "h2", "h3", "h4", "h5", "h6", "hr", "kbd", "li", "mark", "ol",
        "p", "pre", "s", "samp", "section", "small", "span", "strong", "sub", "summary", "sup",
        "table", "tbody", "td", "tfoot", "th", "thead", "tr", "ul", "var"
    };

    private static readonly HashSet<string> RemovedElements = new(StringComparer.Ordinal)
    {
        "audio", "embed", "form", "frame", "frameset", "iframe", "noscript", "object", "script",
        "select", "style", "template", "textarea", "video"
    };

    public MarkdownPreview Render(DocumentSnapshot snapshot,
        IReadOnlyDictionary<string, PreviewRasterImage>? localImages = null)
    {
        var bytes = snapshot.CopyBytes().AsSpan();
        if (bytes.StartsWith(Encoding.UTF8.Preamble))
            bytes = bytes[Encoding.UTF8.Preamble.Length..];

        string source;
        try
        {
            source = Utf8.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            throw new DocumentAccessException("InvalidEncoding");
        }

        // Parse the generated HTML in memory and write fresh markup from the allowlist.
        // AngleSharp has no resource loader here. The original nodes are never serialized.
        using var document = new HtmlParser().ParseDocument(Markdown.ToHtml(source, Pipeline));
        var output = new SanitizedMarkup(snapshot.DocumentId, localImages);
        foreach (var node in document.Body!.ChildNodes)
            output.Write(node, 0);
        return new(source, output.Html.ToString(), output.Headings.AsReadOnly(), output.References.AsReadOnly());
    }

    private sealed class SanitizedMarkup(Guid documentId, IReadOnlyDictionary<string, PreviewRasterImage>? localImages)
    {
        private const string HtmlNamespace = "http://www.w3.org/1999/xhtml";
        private const int MaxNesting = 128;
        private int remainingRasterCharacters = 12 * 1024 * 1024;
        private readonly HashSet<string> anchors = new(StringComparer.Ordinal);
        public StringBuilder Html { get; } = new();
        public List<PreviewHeading> Headings { get; } = [];
        public List<PreviewReference> References { get; } = [];

        public void Write(INode node, int depth)
        {
            if (depth > MaxNesting)
                throw new DocumentAccessException("PreviewNestingLimit");
            if (node is IText text)
            {
                Html.Append(WebUtility.HtmlEncode(text.Data));
                return;
            }
            if (node is not IElement element || element.NamespaceUri != HtmlNamespace)
                return;

            var tag = element.LocalName;
            if (RemovedElements.Contains(tag))
                return;

            switch (tag)
            {
                case "a":
                    WriteReference(element, depth, image: false);
                    return;
                case "img":
                    WriteReference(element, depth, image: true);
                    return;
                case "input":
                    if (element.GetAttribute("type")?.Equals("checkbox", StringComparison.OrdinalIgnoreCase) == true
                        && element.ParentElement?.Closest("li") is not null && element.Closest("button") is null)
                    {
                        Html.Append(element.HasAttribute("checked")
                            ? "<input type=\"checkbox\" disabled checked>"
                            : "<input type=\"checkbox\" disabled>");
                    }
                    return;
            }

            if (!AllowedElements.Contains(tag))
            {
                WriteChildren(element, depth);
                return;
            }

            Html.Append('<').Append(tag);
            if (tag.Length == 2 && tag[0] == 'h' && tag[1] is >= '1' and <= '6')
                WriteHeading(element, tag[1] - '0');
            if (tag == "ol" && int.TryParse(element.GetAttribute("start"), out var start) && start != 1)
                Html.Append(" start=\"").Append(start.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append('"');
            if (tag == "code" && element.ParentElement?.LocalName == "pre")
                WriteCodeLanguage(element);
            Html.Append('>');

            if (tag is "br" or "hr")
                return;
            WriteChildren(element, depth);
            Html.Append("</").Append(tag).Append('>');
        }

        private void WriteChildren(IElement element, int depth)
        {
            foreach (var child in element.ChildNodes)
                Write(child, depth + 1);
        }

        private void WriteHeading(IElement element, int level)
        {
            var anchor = element.Id;
            if (string.IsNullOrEmpty(anchor) || anchor.Length > 256 ||
                !anchor.All(c => char.IsLetterOrDigit(c) || c is '-' or '_' or '.') || !anchors.Add(anchor))
                return;

            var id = $"preview-{documentId:N}-{anchor}";
            Html.Append(" id=\"").Append(WebUtility.HtmlEncode(id)).Append('"');
            Headings.Add(new(anchor, id, element.TextContent, level));
        }

        private void WriteCodeLanguage(IElement element)
        {
            const string prefix = "language-";
            var language = element.ClassList.FirstOrDefault(value =>
                value.StartsWith(prefix, StringComparison.Ordinal))?[prefix.Length..];
            if (string.IsNullOrEmpty(language) || language.Length > 32 ||
                !language.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '+' or '#'))
                return;

            Html.Append(" class=\"language-").Append(WebUtility.HtmlEncode(language.ToLowerInvariant())).Append('"');
        }

        private void WriteReference(IElement element, int depth, bool image)
        {
            var target = element.GetAttribute(image ? "src" : "href")?.Trim();
            var label = image ? element.GetAttribute("alt") ?? "" : element.TextContent;
            if (string.IsNullOrEmpty(target))
            {
                if (image)
                    Html.Append(WebUtility.HtmlEncode(label));
                else
                    WriteChildren(element, depth);
                return;
            }

            var kind = Classify(target, image);
            var index = References.Count;
            References.Add(new(index, kind, target, label));

            if (kind == PreviewReferenceKind.LocalImage && localImages is not null &&
                localImages.TryGetValue(target, out var raster) && raster.DataUri.Length <= remainingRasterCharacters)
            {
                remainingRasterCharacters -= raster.DataUri.Length;
                Html.Append("<img src=\"").Append(raster.DataUri).Append("\" alt=\"")
                    .Append(WebUtility.HtmlEncode(label)).Append("\">");
                return;
            }

            Html.Append("<span data-preview-reference=\"").Append(index).Append("\">");
            if (image)
                Html.Append(WebUtility.HtmlEncode(label));
            else
                WriteChildren(element, depth);
            Html.Append("</span>");
        }

        private static PreviewReferenceKind Classify(string target, bool image)
        {
            if (target.Any(char.IsControl) || target.StartsWith('/') || target.Contains('\\'))
                return PreviewReferenceKind.Blocked;
            if (Uri.TryCreate(target, UriKind.Absolute, out var uri))
            {
                if (uri.Scheme is not ("http" or "https") || string.IsNullOrEmpty(uri.Host)
                    || uri.UserInfo.Length != 0 || !uri.IsWellFormedOriginalString())
                    return PreviewReferenceKind.Blocked;
                return image ? PreviewReferenceKind.RemoteImage : PreviewReferenceKind.ExternalLink;
            }

            if (target.Contains(':') || (image && target.StartsWith('#')))
                return PreviewReferenceKind.Blocked;
            return image ? PreviewReferenceKind.LocalImage : PreviewReferenceKind.DocumentLink;
        }
    }
}
