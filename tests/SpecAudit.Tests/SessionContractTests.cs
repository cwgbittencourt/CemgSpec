using SpecAudit.Domain;
namespace SpecAudit.Tests;
public sealed class SessionContractTests
{
    [Fact]
    public void SessionAndFeatureIdsCannotCrossProjects()
    {
        var owner=Guid.NewGuid();var first=new ProjectSession(owner,"A","origin-A","display-A","output-A");
        var second=new ProjectSession(Guid.NewGuid(),"B","origin-B","display-B","output-B");
        var feature=first.AddFeature("specs/001-demo","Demo");
        Assert.True(first.IsOwnedBy(owner)); Assert.False(second.IsOwnedBy(owner));
        Assert.Equal(feature,first.RequireFeature(feature.FeatureId));
        Assert.Throws<KeyNotFoundException>(()=>second.RequireFeature(feature.FeatureId));
        Assert.Throws<ArgumentException>(()=>first.AddFeature("../escape","Invalid"));
    }
    [Fact]
    public void SelectionDoesNotCreateRunAndRejectsUnknownVerification()
    {
        var project=new ProjectSession(Guid.NewGuid(),"A","origin-A","A","output-A");
        Assert.Equal(VerificationType.AnalyzeDocuments,project.SelectedVerification);
        project.SelectVerification(VerificationType.CheckGate);
        Assert.Equal(VerificationType.CheckGate,project.SelectedVerification);
        Assert.Throws<ArgumentOutOfRangeException>(()=>project.SelectVerification((VerificationType)42));
        Assert.Equal(0,project.Generation); Assert.Equal(1,project.Invalidate());
    }
    [Fact]
    public void SnapshotsOwnTheirBytesAndHashReadContent()
    {
        byte[] bytes=[1,2,3];var doc=Guid.NewGuid();
        var snapshot=DocumentSnapshot.Create(doc,bytes,7);
        bytes[0]=9;
        Assert.Equal(new byte[]{1,2,3},snapshot.CopyBytes()); Assert.Equal(7,snapshot.SourceGeneration);
        Assert.Equal(Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(new byte[]{1,2,3})),snapshot.ContentHash);
        var copy=snapshot.CopyBytes();copy[0]=8;Assert.Equal(1,snapshot.CopyBytes()[0]);
    }
    [Fact]
    public void AssessmentRequiresActualChecksAndMatchingVersionPolicyEnvironment()
    {
        var assessment=new ProtectionAssessment("v1","windows-A","policy-A");
        Assert.False(assessment.Allows("v1","windows-A","policy-A"));
        Assert.Throws<InvalidOperationException>(()=>assessment.RecordPassed(new Dictionary<string,bool>(),[]));
        assessment.RecordPassed(new Dictionary<string,bool>{{"required-check",true}},new[]{"evidence/real.md"});
        Assert.True(assessment.Allows("v1","windows-A","policy-A"));
        Assert.False(assessment.Allows("v2","windows-A","policy-A"));
        Assert.False(assessment.Allows("v1","windows-B","policy-A"));
        Assert.False(assessment.Allows("v1","windows-A","policy-B"));
        assessment.Invalidate();Assert.Equal(AssessmentState.Stale,assessment.State);Assert.False(assessment.Allows("v1","windows-A","policy-A"));
    }
}
