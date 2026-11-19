using SpecAudit.Infrastructure.Documents;

namespace SpecAudit.Tests;

public sealed class DocumentNavigationTests
{
    [Fact]
    public void S01SnapshotReferencesOnlyAuthorizeCataloguedDestinations()
    {
        const string feature = "specs/001-navigation";
        const string sourceText = """
            # Origem
            ## Detalhes

            [Plano](plan.md#entrega)
            [Nesta página](#detalhes)
            [Ausente](absent.md)
            [Fora do catálogo](../../DoNotRead.cs)
            """;
        var origin = Directory.CreateTempSubdirectory("SpecAudit-T021-");
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

            var snapshot = new DocumentBroker(catalog).Read(projectId, specification.DocumentId, catalog.Generation);
            Assert.Equal(sourceText, System.Text.Encoding.UTF8.GetString(snapshot.CopyBytes()));
            Guid? TargetOf(string target) => Assert.Single(snapshot.References, reference => reference.TargetText == target).TargetDocumentId;
            Assert.Equal(plan.DocumentId, TargetOf("plan.md#entrega"));
            Assert.Equal(specification.DocumentId, TargetOf("#detalhes"));
            Assert.Null(TargetOf("absent.md"));
            Assert.Null(TargetOf("../../DoNotRead.cs"));
        }
        finally
        {
            origin.Delete(recursive: true);
        }
    }
}
