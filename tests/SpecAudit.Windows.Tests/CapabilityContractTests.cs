using SpecAudit.Application;
using SpecAudit.Domain;
using SpecAudit.Infrastructure.Execution;
namespace SpecAudit.Windows.Tests;
public sealed class CapabilityContractTests
{
    [Fact]
    public void MissingOrIncompleteProofCannotEnableCapability()
    {
        var registry=new CapabilityRegistry();Assert.Equal(CapabilityAvailability.PendingValidation,registry.Availability(ProtectedCapability.ReaderBoundary,"v1","windows","policy"));
        var proof=new ProtectionAssessment("v1","windows","policy");
        proof.RecordPassed(new Dictionary<string,bool>{{"only-one-check",true}},["synthetic-proof"]);
        Assert.Throws<InvalidOperationException>(()=>registry.RecordProof(ProtectedCapability.ReaderBoundary,proof));
        var names=new[]{"authorized-read","code-config-denied","junction-denied","application-read-only","user-edit-preserved","origin-intact"};
        proof.RecordPassed(names.ToDictionary(n=>n,_=>true),["synthetic-proof"]);registry.RecordProof(ProtectedCapability.ReaderBoundary,proof);
        Assert.Equal(CapabilityAvailability.Available,registry.Availability(ProtectedCapability.ReaderBoundary,"v1","windows","policy"));
        Assert.Equal(CapabilityAvailability.PendingValidation,registry.Availability(ProtectedCapability.ReaderBoundary,"changed","windows","policy"));
        Assert.Equal(CapabilityAvailability.PendingValidation,registry.Availability(ProtectedCapability.ReaderBoundary,"v1","changed","policy"));
        Assert.Equal(CapabilityAvailability.PendingValidation,registry.Availability(ProtectedCapability.ReaderBoundary,"v1","windows","changed"));
        registry.Invalidate(ProtectedCapability.ReaderBoundary);Assert.Equal(CapabilityAvailability.PendingValidation,registry.Availability(ProtectedCapability.ReaderBoundary,"v1","windows","policy"));
        Assert.False(Enum.TryParse<RunState>("Unavailable",out _));
    }
    [Fact]
    public void CalibratedBoundariesRejectExcessWithoutPartialSuccess()
    {
        var fixture=Fixtures.DocumentaryFixture.Create();var limits=new ExecutionLimits();
        var lengths=Directory.GetFiles(fixture.Origin,"*",SearchOption.AllDirectories).Select(f=>new FileInfo(f).Length).ToArray();
        foreach(var length in lengths)limits.CheckDocument(length);
        limits.CheckGraph(lengths.Length,4);limits.CheckDocument(limits.MaxDocumentBytes);limits.CheckOutput(limits.MaxOutputCharacters);limits.CheckDuration(limits.MaxDuration);
        Assert.Equal("Inconclusive",Assert.Throws<LimitExceededException>(()=>limits.CheckDocument(limits.MaxDocumentBytes+1)).Result);
        Assert.Throws<LimitExceededException>(()=>limits.CheckGraph(limits.MaxGraphNodes+1,0));Assert.Throws<LimitExceededException>(()=>limits.CheckGraph(1,limits.MaxGraphDepth+1));
        Assert.Throws<LimitExceededException>(()=>limits.CheckOutput(limits.MaxOutputCharacters+1));Assert.Throws<LimitExceededException>(()=>limits.CheckDuration(limits.MaxDuration+TimeSpan.FromMilliseconds(1)));
    }
}
