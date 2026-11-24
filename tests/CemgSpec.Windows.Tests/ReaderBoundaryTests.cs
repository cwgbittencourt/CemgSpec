using CemgSpec.Domain;
using CemgSpec.Infrastructure.Documents;
using CemgSpec.Windows.Tests.Fixtures;
namespace CemgSpec.Windows.Tests;
public sealed class ReaderBoundaryTests
{
    [Fact]
    public void CatalogReadsOnlyRecognizedDocumentsAndRejectsForeignIds()
    {
        var fixture=DocumentaryFixture.Create();var project=Guid.NewGuid();
        using var catalog=new DocumentCatalog(project,fixture.Origin);
        var entry=Assert.Single(catalog.Entries,e=>e.RelativePath.Replace('\\','/')=="specs/001-demo/spec.md");
        Assert.DoesNotContain(catalog.Entries,e=>e.RelativePath.EndsWith(".cs"));
        var broker=new DocumentBroker(catalog);
        var snapshot=broker.Read(project,entry.DocumentId,0);
        Assert.Contains("Demo feature",System.Text.Encoding.UTF8.GetString(snapshot.CopyBytes()));
        Assert.Equal("OutOfScope",Assert.Throws<DocumentAccessException>(()=>broker.Read(Guid.NewGuid(),entry.DocumentId,0)).Code);
        Assert.Equal("OutOfScope",Assert.Throws<DocumentAccessException>(()=>broker.Read(project,Guid.NewGuid(),0)).Code);
        Assert.True(fixture.IsUnchanged());
    }
    [Fact]
    public void RejectsJunctionAtActualOpenBeforeReadingAnyBytes()
    {
        var fixture=DocumentaryFixture.Create();var project=Guid.NewGuid();
        using var catalog=new DocumentCatalog(project,fixture.Origin);
        var entry=Assert.Single(catalog.Entries,e=>e.RelativePath.Replace('\\','/')=="specs/001-demo/spec.md");
        var feature=fixture.Feature;var parked=Path.Combine(fixture.Root,"parked");
        Directory.Move(feature,parked);
        FixtureJunction.Create(fixture.Root,feature,fixture.Output);
        File.WriteAllText(Path.Combine(fixture.Output,"spec.md"),"unapproved target");
        var bytesRead=false;var broker=new DocumentBroker(catalog){BeforeRead=()=>bytesRead=true};
        var failure=Assert.Throws<DocumentAccessException>(()=>broker.Read(project,entry.DocumentId,0));
        Assert.Equal("OutOfScope",failure.Code);Assert.False(bytesRead);
    }
    [Fact]
    public void ReplacedFileAndChangedGenerationDoNotReturnSnapshot()
    {
        var fixture=DocumentaryFixture.Create();var project=Guid.NewGuid();
        using var catalog=new DocumentCatalog(project,fixture.Origin);
        var entry=Assert.Single(catalog.Entries,e=>e.RelativePath.Replace('\\','/')=="specs/001-demo/spec.md");
        var path=Path.Combine(fixture.Feature,"spec.md");
        File.Move(path,Path.Combine(fixture.Feature,"old.md"));File.WriteAllText(path,"replacement");
        var broker=new DocumentBroker(catalog);
        Assert.Equal("Changed",Assert.Throws<DocumentAccessException>(()=>broker.Read(project,entry.DocumentId,0)).Code);
        Assert.Equal("Changed",Assert.Throws<DocumentAccessException>(()=>broker.Read(project,entry.DocumentId,1)).Code);
    }

    [Fact]
    public void ApplicationBrokerRemainsReadOnlyWhenOriginHasBroadApplicationAcl()
    {
        if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();
        var fixture=DocumentaryFixture.Create();var directory=new DirectoryInfo(fixture.Origin);
        var acl=System.IO.FileSystemAclExtensions.GetAccessControl(directory);
        acl.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(new System.Security.Principal.SecurityIdentifier("S-1-15-2-1"),System.Security.AccessControl.FileSystemRights.ReadAndExecute,System.Security.AccessControl.InheritanceFlags.ContainerInherit|System.Security.AccessControl.InheritanceFlags.ObjectInherit,System.Security.AccessControl.PropagationFlags.None,System.Security.AccessControl.AccessControlType.Allow));
        System.IO.FileSystemAclExtensions.SetAccessControl(directory,acl);fixture.CaptureBaseline();
        using var catalog=new DocumentCatalog(Guid.NewGuid(),fixture.Origin);
        var entry=Assert.Single(catalog.Entries,e=>e.RelativePath.Replace('\\','/')=="specs/001-demo/spec.md");
        var snapshot=new DocumentBroker(catalog).Read(catalog.ProjectId,entry.DocumentId,0);
        Assert.NotEmpty(snapshot.CopyBytes());Assert.True(fixture.IsUnchanged());
        Assert.DoesNotContain(catalog.Entries,e=>e.RelativePath.EndsWith(".cs") || e.RelativePath.EndsWith("appsettings.json"));
    }
    [Fact]
    public void ExternalEditDuringReadRejectsMixedSnapshotAndPreservesEdit()
    {
        var fixture=DocumentaryFixture.Create();var project=Guid.NewGuid();using var catalog=new DocumentCatalog(project,fixture.Origin);
        var entry=Assert.Single(catalog.Entries,e=>e.RelativePath.Replace('\\','/')=="specs/001-demo/spec.md");
        var path=Path.Combine(fixture.Feature,"spec.md");var original=new DocumentBroker(catalog).Read(project,entry.DocumentId,0);
        var broker=new DocumentBroker(catalog){BeforeRead=()=>File.AppendAllText(path,"\nExternal concurrent edit")};
        Assert.Equal("Changed",Assert.Throws<DocumentAccessException>(()=>broker.Read(project,entry.DocumentId,0)).Code);
        Assert.Contains("External concurrent edit",File.ReadAllText(path));
        Assert.DoesNotContain("External concurrent edit",System.Text.Encoding.UTF8.GetString(original.CopyBytes()));Assert.Empty(Directory.GetFiles(fixture.Output));
    }
}
