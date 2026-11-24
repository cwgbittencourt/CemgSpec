using CemgSpec.Infrastructure.Documents;

namespace CemgSpec.Tests;

public sealed class DocumentNavigationTests
{
    [Fact]
    public void S01PreviewReferencesOnlyAuthorizeCataloguedDestinations()
    {
        const string feature = "specs/001-navigation";
        const string sourceText = """
            # Origem
            ## Detalhes

            [Plano](plan.md#entrega)
            [Nesta página](#detalhes)
            [Ausente](absent.md)
            [Âncora ausente](plan.md#ausente)
            [Escapa da origem](../../../outside.md)
            [Separador codificado](contracts%2Fplan.md)
            [Fora do catálogo](../../DoNotRead.cs)
            """;
        var origin = Directory.CreateTempSubdirectory("CemgSpec-T021-");
        try
        {
            Directory.CreateDirectory(Path.Combine(origin.FullName, feature));
            File.WriteAllText(Path.Combine(origin.FullName, feature, "spec.md"), sourceText);
            File.WriteAllText(Path.Combine(origin.FullName, feature, "plan.md"), "# Plano\n\n## Entrega");
            File.WriteAllText(Path.Combine(origin.FullName, "DoNotRead.cs"), "// Outside the catalog");

            var projectId = Guid.NewGuid();
            using var catalog = new DocumentCatalog(projectId, origin.FullName);
            var specification = Assert.Single(catalog.Entries, entry => entry.Kind == Domain.CatalogKind.Spec);
            var plan = Assert.Single(catalog.Entries, entry => entry.Kind == Domain.CatalogKind.Plan);
            Assert.DoesNotContain(catalog.Entries, entry => Path.GetFileName(entry.RelativePath) == "DoNotRead.cs");

            var preview = new DocumentBroker(catalog).OpenPreview(specification.DocumentId);
            Assert.Equal(sourceText, preview.Preview.Source);
            int IndexOf(string target) => Assert.Single(preview.Preview.References,
                reference => reference.Target == target).Index;
            PreviewLink Resolve(string target) => preview.ResolveLink(IndexOf(target));

            Assert.Equal(new(PreviewLinkStatus.Authorized, plan.DocumentId, $"preview-{plan.DocumentId:N}-entrega"),
                Resolve("plan.md#entrega"));
            Assert.Equal(new(PreviewLinkStatus.Authorized, specification.DocumentId, $"preview-{specification.DocumentId:N}-detalhes"),
                Resolve("#detalhes"));
            Assert.Equal(PreviewLinkStatus.Missing, Resolve("absent.md").Status);
            Assert.Equal(PreviewLinkStatus.MissingAnchor, Resolve("plan.md#ausente").Status);
            Assert.Equal(PreviewLinkStatus.OutOfScope, Resolve("../../DoNotRead.cs").Status);
            Assert.Equal(PreviewLinkStatus.OutOfScope, Resolve("../../../outside.md").Status);
            Assert.Equal(PreviewLinkStatus.OutOfScope, Resolve("contracts%2Fplan.md").Status);
            catalog.MarkChanged();
            catalog.Refresh();
            Assert.Equal("Changed", Assert.Throws<DocumentAccessException>(() => Resolve("#detalhes")).Code);
        }
        finally
        {
            origin.Delete(recursive: true);
        }
    }
}
