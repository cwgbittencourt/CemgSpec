using System.Security.Cryptography;
using SpecAudit.Application;
using SpecAudit.Domain;

namespace SpecAudit.Tests;

public sealed class VerificationStateMachineTests
{
    private sealed class UnusedPicker : IFolderPicker
    {
        public Task<string?> PickAsync(string title) => Task.FromResult<string?>(null);
    }

    private sealed class MiniFixture : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "SpecAudit-T054-" + Guid.NewGuid().ToString("N"));
        public string Origin => Path.Combine(Root, "origin");
        public string Output => Path.Combine(Root, "reports");
        public string Feature => Path.Combine(Origin, "specs", "001-demo");

        public MiniFixture()
        {
            Directory.CreateDirectory(Output);
            Write(".specify/memory/constitution.md", "# Constitution");
            Write("specs/001-demo/spec.md", "# Demo\n- FR-001: First");
            Write("specs/001-demo/verification/ready.json", "{\"schema_version\":1,\"stage\":\"spec\",\"state\":\"READY\"}");
            Write("specs/002-other/spec.md", "# Other\n- FR-001: Second");
        }

        private void Write(string relative, string content)
        {
            var path = Path.Combine(Origin, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
        }

        public static string Hash(string path) => Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)));

        public void Dispose() => Directory.Delete(Root, recursive: true);
    }

    [Fact]
    public void UnavailableOrPendingCapabilityNeverCreatesVerificationRun()
    {
        using var fixture = new MiniFixture();
        using var projects = CreateProjectService();
        var ctx = projects.OpenSelected(fixture.Origin, fixture.Output);
        var registry = new CapabilityRegistry();
        var service = new VerificationService(projects, registry);

        var feature = ctx.Session.Features.First(f => f.SpecKitRelativeDirectory.Contains("001-demo"));
        var record = ctx.Catalog.Entries.First(e => e.RelativePath.EndsWith("ready.json", StringComparison.OrdinalIgnoreCase));
        var candidate = new GateCandidate(
            ctx.Session.ProjectId,
            feature.FeatureId,
            GateStage.Spec,
            record.DocumentId,
            "sha256:" + MiniFixture.Hash(Path.Combine(fixture.Feature, "spec.md")),
            RevisionSource.ArtifactSha256,
            [],
            ctx.Catalog.Generation);

        var pending = service.EvaluateAvailability(VerificationType.CheckGate, ProtectedCapability.GateAdapter, "3.0.0", "win-x64", "v4");
        Assert.Equal(CapabilityAvailability.PendingValidation, pending.Availability);

        var denied = service.PrepareGateRun(ctx.Session.ProjectId, candidate, pending);
        Assert.False(denied.Created);
        Assert.Null(denied.Run);
        Assert.Equal(CapabilityAvailability.PendingValidation, denied.Availability);
        Assert.Empty(service.GetRuns(ctx.Session.ProjectId));

        var noRuntime = service.EvaluateAvailability(VerificationType.CheckGate, ProtectedCapability.GateAdapter, "3.0.0", "win-x64", "v4", runtimeAvailable: false, runtimeUnavailableReason: "Python ausente");
        Assert.Equal(CapabilityAvailability.Unavailable, noRuntime.Availability);
        var deniedRuntime = service.PrepareGateRun(ctx.Session.ProjectId, candidate, noRuntime);
        Assert.False(deniedRuntime.Created);
        Assert.Null(deniedRuntime.Run);
        Assert.Empty(service.GetRuns(ctx.Session.ProjectId));
    }

    [Fact]
    public void StalePreparationEndsBlockedAndUnconfirmedCancelLocksAdapterAsFailed()
    {
        using var fixture = new MiniFixture();
        using var projects = CreateProjectService();
        var ctx = projects.OpenSelected(fixture.Origin, fixture.Output);
        var registry = CreatePassedGateRegistry("3.0.0", "win-x64", "v4");
        var service = new VerificationService(projects, registry);

        var feature = ctx.Session.Features.First(f => f.SpecKitRelativeDirectory.Contains("001-demo"));
        var record = ctx.Catalog.Entries.First(e => e.RelativePath.EndsWith("ready.json", StringComparison.OrdinalIgnoreCase));
        var availability = service.EvaluateAvailability(VerificationType.CheckGate, ProtectedCapability.GateAdapter, "3.0.0", "win-x64", "v4");
        Assert.Equal(CapabilityAvailability.Available, availability.Availability);

        var expiredCandidate = new GateCandidate(
            ctx.Session.ProjectId,
            feature.FeatureId,
            GateStage.Spec,
            record.DocumentId,
            "sha256:abc",
            RevisionSource.ArtifactSha256,
            [],
            ctx.Catalog.Generation + 99);
        var blockedPrep = service.PrepareGateRun(ctx.Session.ProjectId, expiredCandidate, availability);
        Assert.True(blockedPrep.Created);
        Assert.NotNull(blockedPrep.Run);
        Assert.Equal(RunState.Blocked, blockedPrep.Run!.State);
        Assert.Equal(Freshness.Stale, blockedPrep.Run.Freshness);
        Assert.Null(blockedPrep.Run.SemanticResult);

        var validCandidate = new GateCandidate(
            ctx.Session.ProjectId,
            feature.FeatureId,
            GateStage.Spec,
            record.DocumentId,
            "sha256:abc",
            RevisionSource.ArtifactSha256,
            [],
            ctx.Catalog.Generation);
        var focus = new RequirementKey(ctx.Session.ProjectId, feature.FeatureId, "FR-001");
        var prepared = service.PrepareGateRun(ctx.Session.ProjectId, validCandidate, availability, focus);
        Assert.Equal(RunState.Preparing, prepared.Run!.State);
        Assert.Equal(focus, prepared.Run.FocusRequirementKey);
        prepared.Run.BeginRunning("check_gate CEMG 3.0.0 / adaptador SpecAudit");
        Assert.Equal(RunState.Running, prepared.Run.State);
        service.RequestCancel(ctx.Session.ProjectId, prepared.Run.RunId);
        Assert.True(prepared.Run.CancelRequested);
        Assert.Equal(RunState.Running, prepared.Run.State);
        service.CompleteCancel(ctx.Session.ProjectId, prepared.Run.RunId, terminationConfirmed: true, "Cancelado pelo usuário");
        Assert.Equal(RunState.Cancelled, prepared.Run.State);
        Assert.True(prepared.Run.TerminationConfirmed);
        Assert.Null(prepared.Run.SemanticResult);
        Assert.False(service.IsAdapterLocked(VerificationType.CheckGate));

        var second = service.PrepareGateRun(ctx.Session.ProjectId, validCandidate, availability);
        second.Run!.BeginRunning("check_gate CEMG 3.0.0 / adaptador SpecAudit");
        service.RequestCancel(ctx.Session.ProjectId, second.Run.RunId);
        service.CompleteCancel(ctx.Session.ProjectId, second.Run.RunId, terminationConfirmed: false, "Timeout ao encerrar");
        Assert.Equal(RunState.Failed, second.Run.State);
        Assert.False(second.Run.TerminationConfirmed);
        Assert.Null(second.Run.SemanticResult);
        Assert.True(service.IsAdapterLocked(VerificationType.CheckGate));

        var lockedAvailability = service.EvaluateAvailability(VerificationType.CheckGate, ProtectedCapability.GateAdapter, "3.0.0", "win-x64", "v4");
        Assert.Equal(CapabilityAvailability.Unavailable, lockedAvailability.Availability);
    }

    [Fact]
    public void EnforcesProjectFeatureAndDocumentOwnershipAndSeparatesFailureFromRequirementOutcome()
    {
        using var fixture = new MiniFixture();
        using var projects = CreateProjectService();
        var ctx = projects.OpenSelected(fixture.Origin, fixture.Output);
        var registry = CreatePassedGateRegistry("3.0.0", "win-x64", "v4");
        var service = new VerificationService(projects, registry);

        var demo = ctx.Session.Features.First(f => f.SpecKitRelativeDirectory.Contains("001-demo"));
        var other = ctx.Session.Features.First(f => f.SpecKitRelativeDirectory.Contains("002-other"));
        var demoRecord = ctx.Catalog.Entries.First(e => e.RelativePath.EndsWith("ready.json", StringComparison.OrdinalIgnoreCase));
        var availability = service.EvaluateAvailability(VerificationType.CheckGate, ProtectedCapability.GateAdapter, "3.0.0", "win-x64", "v4");

        var wrongFeatureCandidate = new GateCandidate(
            ctx.Session.ProjectId,
            other.FeatureId,
            GateStage.Spec,
            demoRecord.DocumentId,
            "sha256:abc",
            RevisionSource.ArtifactSha256,
            [],
            ctx.Catalog.Generation);
        Assert.Throws<InvalidOperationException>(() => service.PrepareGateRun(ctx.Session.ProjectId, wrongFeatureCandidate, availability));

        var validCandidate = new GateCandidate(
            ctx.Session.ProjectId,
            demo.FeatureId,
            GateStage.Spec,
            demoRecord.DocumentId,
            "sha256:abc",
            RevisionSource.ArtifactSha256,
            [],
            ctx.Catalog.Generation);
        var wrongFocus = new RequirementKey(ctx.Session.ProjectId, other.FeatureId, "FR-001");
        Assert.Throws<InvalidOperationException>(() => service.PrepareGateRun(ctx.Session.ProjectId, validCandidate, availability, wrongFocus));

        var prepared = service.PrepareGateRun(ctx.Session.ProjectId, validCandidate, availability);
        prepared.Run!.BeginRunning("check_gate CEMG 3.0.0 / adaptador SpecAudit");
        prepared.Run.MarkFailed("Erro de schema na saída do runner", exitCode: 2);
        Assert.Equal(RunState.Failed, prepared.Run.State);
        Assert.Null(prepared.Run.SemanticResult);

        service.InvalidateProjectRuns(ctx.Session.ProjectId);
        Assert.Equal(Freshness.Stale, prepared.Run.Freshness);
        Assert.Equal(RunState.Failed, prepared.Run.State);

        Assert.Equal(6, Enum.GetNames<GateStage>().Length);
        Assert.Equal(new[] { nameof(VerificationType.AnalyzeDocuments), nameof(VerificationType.CheckGate) }, Enum.GetNames<VerificationType>());
        Assert.False(Enum.TryParse<RunState>("Unavailable", out _));
    }

    private static ProjectService CreateProjectService()
    {
        var registry = new LocalSessionRegistry();
        var issued = registry.Issue();
        var accessor = new Microsoft.AspNetCore.Http.HttpContextAccessor
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };
        accessor.HttpContext.Items[typeof(LocalSessionService)] = issued.Id;
        var session = new LocalSessionService(accessor, registry);
        return new ProjectService(session, new UnusedPicker());
    }

    private static CapabilityRegistry CreatePassedGateRegistry(string version, string platform, string policy)
    {
        var registry = new CapabilityRegistry();
        var proof = new ProtectionAssessment(version, platform, policy);
        var checks = new[]
        {
            "package-integrity",
            "offline-no-shell",
            "core-equivalence",
            "nested-evidence",
            "read-time-toctou",
            "cancel-zero-writes"
        }.ToDictionary(c => c, _ => true);
        proof.RecordPassed(checks, ["synthetic-unit-test"]);
        registry.RecordProof(ProtectedCapability.GateAdapter, proof);
        return registry;
    }
}
