using Microsoft.AspNetCore.Http;
using SpecAudit.Application;
using SpecAudit.Domain;
using SpecAudit.Infrastructure.Agents;
using SpecAudit.Infrastructure.Gates;
using SpecAudit.Windows.Tests.Fixtures;

namespace SpecAudit.Windows.Tests;

public sealed class AgentDiscoveryTests
{
    private sealed class UnusedPicker : IFolderPicker
    {
        public Task<string?> PickAsync(string title) => Task.FromResult<string?>(null);
    }

    [Fact]
    public void S01DiscoversProvidersSeparatesDiscoveryFromCompatibilityAndRestrictsManualFallback()
    {
        var fixture = DocumentaryFixture.Create();
        var fakeBinsDir = Path.Combine(Path.GetTempPath(), "SpecAudit-FakeAgents-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(fakeBinsDir);
        try
        {
            // 1. Test real discovery on this workstation: must discover 4 providers, keep ClaudeCode/Kiro Unsupported,
            // and never mark any discovered provider as Compatible automatically.
            var realDiscovery = new AgentDiscovery();
            var realInstallations = realDiscovery.DiscoverAll(fixture.Origin);
            Assert.Equal(4, realInstallations.Count);
            Assert.All(realInstallations, inst =>
            {
                Assert.NotEqual(AgentCompatibilityState.Compatible, inst.CompatibilityState);
                Assert.NotEmpty(inst.ExternalStateLocations);
                if (inst.Provider is AgentProvider.ClaudeCode or AgentProvider.Kiro)
                    Assert.Equal(AgentCompatibilityState.Unsupported, inst.CompatibilityState);
            });

            // Verify real Codex and Antigravity installations present on this machine are discovered with version and hash.
            var realCodex = realInstallations.Single(i => i.Provider == AgentProvider.Codex);
            var realAntigravity = realInstallations.Single(i => i.Provider == AgentProvider.Antigravity);
            Assert.True(realCodex.IsFound, realCodex.StatusReason);
            Assert.Equal(AgentCompatibilityState.Discovered, realCodex.CompatibilityState);
            Assert.False(string.IsNullOrWhiteSpace(realCodex.Version));
            Assert.True(realDiscovery.VerifyUnchanged(realCodex, fixture.Origin));

            Assert.True(realAntigravity.IsFound, realAntigravity.StatusReason);
            Assert.Equal(AgentCompatibilityState.Discovered, realAntigravity.CompatibilityState);
            Assert.False(string.IsNullOrWhiteSpace(realAntigravity.Version));
            Assert.True(realDiscovery.VerifyUnchanged(realAntigravity, fixture.Origin));

            // Manual fallback on an already automatically discovered provider MUST be rejected.
            var dummyExe = Path.Combine(fakeBinsDir, "manual-codex.exe");
            File.WriteAllText(dummyExe, "MZ-synthetic-binary");
            Assert.Throws<InvalidOperationException>(() =>
                realDiscovery.LocateManually(AgentProvider.Codex, dummyExe, fixture.Origin));

            // 2. Test controlled fixture where Codex is found and Antigravity is absent:
            var foundCodexExe = Path.Combine(fakeBinsDir, "codex.exe");
            var manualAntigravityExe = Path.Combine(fakeBinsDir, "Antigravity.exe");
            var originMaliciousExe = Path.Combine(fixture.Origin, "evil-agent.exe");
            File.WriteAllText(foundCodexExe, "MZ-found-codex");
            File.WriteAllText(manualAntigravityExe, "MZ-manual-antigravity");
            File.WriteAllText(originMaliciousExe, "MZ-inside-origin");
            fixture.CaptureBaseline();

            var controlledPaths = new Dictionary<AgentProvider, AgentCandidatePaths>
            {
                [AgentProvider.Codex] = new([foundCodexExe], [], [Path.Combine(fakeBinsDir, ".codex")]),
                [AgentProvider.Antigravity] = new([Path.Combine(fakeBinsDir, "missing-antigravity.exe")], [], [Path.Combine(fakeBinsDir, ".antigravity")]),
                [AgentProvider.ClaudeCode] = new([], [], [Path.Combine(fakeBinsDir, ".claude")]),
                [AgentProvider.Kiro] = new([], [], [Path.Combine(fakeBinsDir, ".kiro")])
            };

            var controlled = new AgentDiscovery(controlledPaths);
            var list = controlled.DiscoverAll(fixture.Origin);
            var codex = list.Single(i => i.Provider == AgentProvider.Codex);
            var agy = list.Single(i => i.Provider == AgentProvider.Antigravity);

            Assert.True(codex.IsFound);
            Assert.False(codex.CanManualLocate);
            Assert.False(agy.IsFound);
            Assert.True(agy.CanManualLocate);
            Assert.Equal(AgentCompatibilityState.NotVerified, agy.CompatibilityState);

            // Manual location inside audited origin MUST be rejected.
            Assert.Throws<InvalidOperationException>(() =>
                controlled.LocateManually(AgentProvider.Antigravity, originMaliciousExe, fixture.Origin));

            // Manual location outside origin succeeds for absent provider, stays Discovered (not Compatible!).
            var locatedAgy = controlled.LocateManually(AgentProvider.Antigravity, manualAntigravityExe, fixture.Origin);
            Assert.True(locatedAgy.IsFound);
            Assert.Equal(AgentDiscoveryMethod.ManualFallback, locatedAgy.DiscoveryMethod);
            Assert.Equal(AgentCompatibilityState.Discovered, locatedAgy.CompatibilityState);

            // Tampering with executable invalidates installation to Stale.
            File.AppendAllText(manualAntigravityExe, "-tampered");
            Assert.False(controlled.VerifyUnchanged(locatedAgy, fixture.Origin));
            Assert.Equal(AgentCompatibilityState.Stale, locatedAgy.CompatibilityState);

            Assert.True(fixture.IsUnchanged());
            Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Output));
        }
        finally
        {
            Directory.Delete(fakeBinsDir, recursive: true);
            Directory.Delete(fixture.Root, recursive: true);
        }
    }

    [Fact]
    public async Task S01ProjectSelectionAndCheckGateRemainIndependentFromAgentAvailability()
    {
        var fixture1 = DocumentaryFixture.Create();
        var fixture2 = DocumentaryFixture.Create();
        try
        {
            using var projects = CreateProjectService();
            var ctx1 = projects.OpenSelected(fixture1.Origin, fixture1.Output);
            var ctx2 = projects.OpenSelected(fixture2.Origin, fixture2.Output);

            var agentA = Guid.NewGuid();
            ctx1.Session.SelectAgent(agentA);
            Assert.Equal(agentA, ctx1.Session.SelectedAgentId);
            Assert.Null(ctx2.Session.SelectedAgentId);

            // Even with zero agents discovered / all agents missing, check_gate and documentary reader work offline!
            projects.SelectProject(ctx1.Session.ProjectId);
            var registry = new CapabilityRegistry();
            var gatePkg = new GatePackage();
            Assert.True(gatePkg.TryRegisterVerifiedSeal(registry));

            var verifications = new VerificationService(projects, registry);
            var feature = ctx1.Session.Features.First(f => f.SpecKitRelativeDirectory.Contains("001-demo"));
            var record = ctx1.Catalog.Entries.First(e => e.RelativePath.EndsWith("ready.json", StringComparison.OrdinalIgnoreCase));
            var specHash = DocumentaryFixture.Hash(Path.Combine(fixture1.Feature, "spec.md"));
            var candidate = new GateCandidate(
                ctx1.Session.ProjectId,
                feature.FeatureId,
                GateStage.Spec,
                record.DocumentId,
                "sha256:" + specHash,
                RevisionSource.ArtifactSha256,
                [],
                ctx1.Catalog.Generation);

            var availability = verifications.EvaluateAvailability(
                VerificationType.CheckGate,
                ProtectedCapability.GateAdapter,
                GatePackage.CoreVersion,
                GatePackage.PlatformFingerprint,
                GatePackage.PolicyVersion);
            Assert.Equal(CapabilityAvailability.Available, availability.Availability);

            var prepared = verifications.PrepareGateRun(ctx1.Session.ProjectId, candidate, availability);
            Assert.True(prepared.Created);
            var executor = new GateExecutor(gatePkg);
            await executor.ExecuteAsync(ctx1, candidate, prepared.Run!);
            Assert.Equal(RunState.Completed, prepared.Run!.State);
            Assert.Equal(GateSemanticResult.Ready, prepared.Run.SemanticResult!.GateResult);

            Assert.True(fixture1.IsUnchanged());
            Assert.True(fixture2.IsUnchanged());
        }
        finally
        {
            Directory.Delete(fixture1.Root, recursive: true);
            Directory.Delete(fixture2.Root, recursive: true);
        }
    }

    [Fact]
    public void S02AgentCompatibilityEnablesCodexOnlyWithValidSealAndInvalidationDoesNotAffectCheckGate()
    {
        var fixture = DocumentaryFixture.Create();
        try
        {
            using var projects = CreateProjectService();
            var ctx = projects.OpenSelected(fixture.Origin, fixture.Output);
            var discovery = new AgentDiscovery();
            var codexAdapter = new CodexAnalyzeAdapter(discovery);
            var gatePkg = new GatePackage();
            var registry = new CapabilityRegistry();
            var verifications = new VerificationService(projects, registry);
            var compatibility = new AgentCompatibilityService(projects, registry, discovery, verifications);

            // 1. Without AnalyzeCodex seal registered, Codex is NotVerified and unavailable.
            var initialInstallations = compatibility.GetInstallations(ctx.Session.ProjectId);
            var codex = initialInstallations.Single(i => i.Provider == AgentProvider.Codex);
            compatibility.SelectAgent(ctx.Session.ProjectId, codex.AgentId);
            Assert.Equal(AgentCompatibilityState.NotVerified, codex.CompatibilityState);
            Assert.Equal(CapabilityAvailability.PendingValidation, compatibility.EvaluateSelectedAgentAvailability(ctx.Session.ProjectId).Availability);

            // 2. Registering verified seal enables Codex (Compatible -> Available), while Antigravity/ClaudeCode/Kiro stay disabled.
            Assert.True(gatePkg.TryRegisterVerifiedSeal(registry));
            Assert.True(codexAdapter.TryRegisterVerifiedSeal(registry, fixture.Origin));

            var refreshed = compatibility.GetInstallations(ctx.Session.ProjectId);
            codex = refreshed.Single(i => i.Provider == AgentProvider.Codex);
            var agy = refreshed.Single(i => i.Provider == AgentProvider.Antigravity);
            var claude = refreshed.Single(i => i.Provider == AgentProvider.ClaudeCode);
            var kiro = refreshed.Single(i => i.Provider == AgentProvider.Kiro);

            Assert.Equal(AgentCompatibilityState.Compatible, codex.CompatibilityState);
            Assert.Equal(CapabilityAvailability.Available, compatibility.EvaluateSelectedAgentAvailability(ctx.Session.ProjectId).Availability);
            Assert.NotEqual(AgentCompatibilityState.Compatible, agy.CompatibilityState);
            Assert.Equal(AgentCompatibilityState.Unsupported, claude.CompatibilityState);
            Assert.Equal(AgentCompatibilityState.Unsupported, kiro.CompatibilityState);

            // 3. Registering AnalyzeAntigravity seal enables Antigravity independently; invalidating Codex leaves Antigravity & GateAdapter Available.
            var agyAdapter = new AntigravityAnalyzeAdapter();
            Assert.True(agyAdapter.TryRegisterVerifiedSeal(registry, fixture.Origin));
            refreshed = compatibility.GetInstallations(ctx.Session.ProjectId);
            agy = refreshed.Single(i => i.Provider == AgentProvider.Antigravity);
            Assert.Equal(AgentCompatibilityState.Compatible, agy.CompatibilityState);

            compatibility.InvalidateProviderSeal(AgentProvider.Codex);
            Assert.Equal(AgentCompatibilityState.Stale, codex.CompatibilityState);
            Assert.Equal(CapabilityAvailability.PendingValidation, compatibility.EvaluateSelectedAgentAvailability(ctx.Session.ProjectId).Availability);

            compatibility.SelectAgent(ctx.Session.ProjectId, agy.AgentId);
            Assert.Equal(AgentCompatibilityState.Compatible, agy.CompatibilityState);
            Assert.Equal(CapabilityAvailability.Available, compatibility.EvaluateSelectedAgentAvailability(ctx.Session.ProjectId).Availability);

            var gateAvail = verifications.EvaluateAvailability(
                VerificationType.CheckGate,
                ProtectedCapability.GateAdapter,
                GatePackage.CoreVersion,
                GatePackage.PlatformFingerprint,
                GatePackage.PolicyVersion);
            Assert.Equal(CapabilityAvailability.Available, gateAvail.Availability);
        }
        finally
        {
            Directory.Delete(fixture.Root, recursive: true);
        }
    }

    private static ProjectService CreateProjectService()
    {
        var registry = new LocalSessionRegistry();
        var issued = registry.Issue();
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        accessor.HttpContext.Items[typeof(LocalSessionService)] = issued.Id;
        return new ProjectService(new LocalSessionService(accessor, registry), new UnusedPicker());
    }
}
