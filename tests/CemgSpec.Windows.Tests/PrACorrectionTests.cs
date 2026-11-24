using CemgSpec.Domain;
using CemgSpec.Infrastructure.Documents;
using CemgSpec.Windows.Tests.Fixtures;
namespace CemgSpec.Windows.Tests;
public sealed class PrACorrectionTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConstitutionIsCataloguedHasHashAndDetectsMetadataPreservingEdit(bool withSpecs)
    {
        var fixture=DocumentaryFixture.Create();
        if(!withSpecs)Directory.Move(Path.Combine(fixture.Origin,"specs"),Path.Combine(fixture.Root,"parked-specs"));
        fixture.CaptureBaseline();
        using var catalog=new DocumentCatalog(Guid.NewGuid(),fixture.Origin);
        var entry=Assert.Single(catalog.Entries,e=>e.Kind==CatalogKind.Constitution);
        var path=Path.Combine(fixture.Origin,".specify","memory","constitution.md");
        var snapshot=new DocumentBroker(catalog).Read(catalog.ProjectId,entry.DocumentId,catalog.Generation);
        Assert.Equal(DocumentaryFixture.Hash(path),entry.ContentHash);
        Assert.Equal(entry.ContentHash,snapshot.ContentHash);
        Assert.Equal(Path.Combine(".specify","memory","constitution.md"),entry.RelativePath);
        Assert.True(fixture.IsUnchanged());Assert.Empty(Directory.GetFiles(fixture.Output));
        var stamp=File.GetLastWriteTimeUtc(path);var bytes=File.ReadAllBytes(path);bytes[0]^=1;
        File.WriteAllBytes(path,bytes);File.SetLastWriteTimeUtc(path,stamp);
        Assert.Equal("Changed",Assert.Throws<DocumentAccessException>(()=>new DocumentBroker(catalog).Read(catalog.ProjectId,entry.DocumentId,catalog.Generation)).Code);
        Assert.Equal(bytes,File.ReadAllBytes(path));
        Assert.NotEqual(snapshot.ContentHash,DocumentaryFixture.Hash(path));
    }
}
