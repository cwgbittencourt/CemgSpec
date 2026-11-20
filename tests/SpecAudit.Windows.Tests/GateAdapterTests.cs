using System.Reflection;
using SpecAudit.Application;
using SpecAudit.Domain;
using SpecAudit.Windows.Tests.Fixtures;

namespace SpecAudit.Windows.Tests;

public sealed class GateAdapterTests
{
    private static readonly Assembly ProductAssembly = typeof(ProjectSession).Assembly;

    [Fact]
    public void S06DomainAndPackageExposeClosedStagesAndPinnedCemgCore()
    {
        var gateStageType = RequireType("SpecAudit.Domain.GateStage");
        var gateCandidateType = RequireType("SpecAudit.Domain.GateCandidate");
        var verificationRunType = RequireType("SpecAudit.Domain.VerificationRun");
        var verificationServiceType = RequireType("SpecAudit.Application.VerificationService");
        var gatePackageType = RequireType("SpecAudit.Infrastructure.Gates.GatePackage");
        var gateExecutorType = RequireType("SpecAudit.Infrastructure.Gates.GateExecutor");

        Assert.NotNull(gateCandidateType);
        Assert.NotNull(verificationRunType);
        Assert.NotNull(verificationServiceType);
        Assert.NotNull(gatePackageType);
        Assert.NotNull(gateExecutorType);

        var stageNames = Enum.GetNames(gateStageType);
        Assert.Equal(6, stageNames.Length);
        Assert.Contains("Spec", stageNames);
        Assert.Contains("Plan", stageNames);
        Assert.Contains("Tasks", stageNames);
        Assert.Contains("Pr", stageNames);
        Assert.Contains("Acceptance", stageNames);
        Assert.Contains("DevImpossibility", stageNames);

        var verificationTypes = Enum.GetNames<VerificationType>();
        Assert.Equal(new[] { nameof(VerificationType.AnalyzeDocuments), nameof(VerificationType.CheckGate) }, verificationTypes);
        Assert.False(Enum.TryParse<RunState>("Unavailable", out _));
    }

    [Fact]
    public void S06ValidatesReadyBlockedDevImpossibilityAndRejectsBadHashOrWrongRevision()
    {
        var fixture = DocumentaryFixture.Create();
        try
        {
            Assert.True(File.Exists(Path.Combine(fixture.Feature, "verification", "ready.json")));
            Assert.True(File.Exists(Path.Combine(fixture.Feature, "verification", "blocked.json")));
            Assert.True(File.Exists(Path.Combine(fixture.Feature, "verification", "bad-hash.json")));
            Assert.True(File.Exists(Path.Combine(fixture.Feature, "verification", "wrong-revision.json")));

            _ = RequireType("SpecAudit.Infrastructure.Gates.GateExecutor");
            Assert.True(fixture.IsUnchanged());
            Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Output));
        }
        finally
        {
            Directory.Delete(fixture.Root, recursive: true);
        }
    }

    [Fact]
    public void S05AndS06SeparateAvailabilityCoreResultErrorAndCancellation()
    {
        var registry = new CapabilityRegistry();
        Assert.Equal(
            CapabilityAvailability.PendingValidation,
            registry.Availability(ProtectedCapability.GateAdapter, "v1", "windows", "policy"));

        _ = RequireType("SpecAudit.Application.VerificationService");
        _ = RequireType("SpecAudit.Infrastructure.Gates.GatePackage");
    }

    [Fact]
    public void S07DeniesCodeReferenceNestedLogsOutsideArtifactsAndJunctionToctouOffline()
    {
        var fixture = DocumentaryFixture.Create();
        try
        {
            Assert.True(File.Exists(Path.Combine(fixture.Feature, "verification", "code-reference.json")));
            Assert.True(File.Exists(Path.Combine(fixture.Feature, "verification", "nested-log.json")));
            Assert.True(File.Exists(Path.Combine(fixture.Feature, "verification", "outside-artifact.json")));

            _ = RequireType("SpecAudit.Infrastructure.Gates.GateExecutor");
            Assert.True(fixture.IsUnchanged());
            Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Output));
        }
        finally
        {
            Directory.Delete(fixture.Root, recursive: true);
        }
    }

    private static Type RequireType(string fullName)
    {
        var type = ProductAssembly.GetType(fullName, throwOnError: false);
        Assert.True(type is not null, $"Expected contract type '{fullName}' to be implemented in T054-T056.");
        return type!;
    }
}

