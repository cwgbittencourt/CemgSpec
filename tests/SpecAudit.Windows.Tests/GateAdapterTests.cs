using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SpecAudit.Application;
using SpecAudit.Domain;
using SpecAudit.Infrastructure.Documents;
using SpecAudit.Infrastructure.Gates;
using SpecAudit.Windows.Tests.Fixtures;

namespace SpecAudit.Windows.Tests;

public sealed class GateAdapterTests
{
    private sealed class UnusedPicker : IFolderPicker
    {
        public Task<string?> PickAsync(string title) => Task.FromResult<string?>(null);
    }

    [Fact]
    public void S06DomainAndPackageExposeClosedStagesAndPinnedCemgCore()
    {
        var stageNames = Enum.GetNames<GateStage>();
        Assert.Equal(6, stageNames.Length);
        Assert.Contains(nameof(GateStage.Spec), stageNames);
        Assert.Contains(nameof(GateStage.Plan), stageNames);
        Assert.Contains(nameof(GateStage.Tasks), stageNames);
        Assert.Contains(nameof(GateStage.Pr), stageNames);
        Assert.Contains(nameof(GateStage.Acceptance), stageNames);
        Assert.Contains(nameof(GateStage.DevImpossibility), stageNames);

        Assert.Equal(
            new[] { nameof(VerificationType.AnalyzeDocuments), nameof(VerificationType.CheckGate) },
            Enum.GetNames<VerificationType>());
        Assert.False(Enum.TryParse<RunState>("Unavailable", out _));

        var package = new GatePackage();
        var status = package.VerifyPackage();
        Assert.True(status.IsValid, status.Message);
        Assert.Equal(GatePackage.ExpectedCoreSha256, status.CoreSha256);
        Assert.Equal(GatePackage.ExpectedPolicySha256, status.PolicySha256);
    }

    [Fact]
    public async Task S06ValidatesReadyBlockedDevImpossibilityAndMatchesOfficialCliEquivalence()
    {
        var fixture = DocumentaryFixture.Create();
        try
        {
            var specHash = DocumentaryFixture.Hash(Path.Combine(fixture.Feature, "spec.md"));
            var revision = "sha256:" + specHash;

            // Add synthetic PR, Acceptance, and DevImpossibility records with nested test-run, product-execution, and human decision evidence.
            var stdoutPath = WriteFeatureFile(fixture, "evidence/logs/test-stdout.txt", "Passed: 10/10\n");
            var stderrPath = WriteFeatureFile(fixture, "evidence/logs/test-stderr.txt", "");
            var artifactPath = WriteFeatureFile(fixture, "evidence/captures/ui-check.txt", "Synthetic UI capture evidence\n");
            var prApprovalSource = WriteFeatureFile(fixture, "evidence/pr-approval-source.md", "PR approved by human reviewer.\n");
            var acceptSource = WriteFeatureFile(fixture, "evidence/acceptance-source.md", "Story accepted by DEV.\n");
            var impossSource = WriteFeatureFile(fixture, "evidence/dev-impossibility-source.md", "Confirmed by DEV.\n");

            var testRunPath = WriteFeatureFile(fixture, "evidence/test-run.json", JsonSerializer.Serialize(new
            {
                revision,
                result = "PASSED",
                exit_code = 0,
                command = "dotnet test --synthetic",
                timestamp = "2026-11-20T12:00:00Z",
                stdout = new { path = "evidence/logs/test-stdout.txt", sha256 = DocumentaryFixture.Hash(stdoutPath) },
                stderr = new { path = "evidence/logs/test-stderr.txt", sha256 = DocumentaryFixture.Hash(stderrPath) }
            }));

            var aiExecPath = WriteFeatureFile(fixture, "evidence/ai-exec.json", JsonSerializer.Serialize(new
            {
                revision,
                state = "OBSERVED",
                executor = "AI",
                medium = "UI",
                startup = "dotnet run",
                input = "fixture",
                expected = "READY",
                observed = "READY",
                effects_checked = "origin unchanged",
                scenarios = new[] { "US5-S06" },
                artifacts = new[] { new { path = "evidence/captures/ui-check.txt", sha256 = DocumentaryFixture.Hash(artifactPath) } }
            }));

            var devExecPath = WriteFeatureFile(fixture, "evidence/dev-exec.json", JsonSerializer.Serialize(new
            {
                revision,
                state = "OBSERVED",
                executor = "DEV",
                medium = "UI",
                startup = "dotnet run",
                input = "fixture",
                expected = "READY",
                observed = "READY",
                effects_checked = "origin unchanged",
                scenarios = new[] { "US5-S06" },
                artifacts = new[] { new { path = "evidence/captures/ui-check.txt", sha256 = DocumentaryFixture.Hash(artifactPath) } }
            }));

            WriteFeatureFile(fixture, "verification/pr-ready.json", JsonSerializer.Serialize(new
            {
                schema_version = 1,
                stage = "pr",
                revision,
                state = "READY",
                blockers = Array.Empty<string>(),
                checks = new Dictionary<string, bool>
                {
                    ["applicable_tests_passed"] = true,
                    ["review_complete"] = true,
                    ["blocking_findings_resolved"] = true,
                    ["safe_intermediate_state"] = true,
                    ["dependencies_satisfied"] = true,
                    ["human_pr_approved"] = true,
                    ["ai_product_execution_observed"] = true
                },
                evidence = new object[]
                {
                    new { path = "evidence/test-run.json", kind = "test-run", sha256 = DocumentaryFixture.Hash(testRunPath) },
                    new { path = "evidence/ai-exec.json", kind = "product-execution", sha256 = DocumentaryFixture.Hash(aiExecPath) },
                    new { path = "evidence/pr-approval-source.md", kind = "document", sha256 = DocumentaryFixture.Hash(prApprovalSource) }
                },
                human_decisions = new object[]
                {
                    new { kind = "pr-approval", decision = "APPROVED", revision, actor = "reviewer-synthetic", timestamp = "2026-11-20T12:00:00Z", source = "evidence/pr-approval-source.md" }
                }
            }));

            WriteFeatureFile(fixture, "verification/acceptance-ready.json", JsonSerializer.Serialize(new
            {
                schema_version = 1,
                stage = "acceptance",
                revision,
                state = "READY",
                blockers = Array.Empty<string>(),
                checks = new Dictionary<string, bool>
                {
                    ["integrated_tests_passed"] = true,
                    ["review_current"] = true,
                    ["dev_script_checked"] = true,
                    ["dev_executed"] = true,
                    ["human_acceptance_explicit"] = true,
                    ["ai_product_execution_observed"] = true,
                    ["dev_product_execution_observed"] = true
                },
                evidence = new object[]
                {
                    new { path = "evidence/test-run.json", kind = "test-run", sha256 = DocumentaryFixture.Hash(testRunPath) },
                    new { path = "evidence/ai-exec.json", kind = "product-execution", sha256 = DocumentaryFixture.Hash(aiExecPath) },
                    new { path = "evidence/dev-exec.json", kind = "product-execution", sha256 = DocumentaryFixture.Hash(devExecPath) },
                    new { path = "evidence/acceptance-source.md", kind = "document", sha256 = DocumentaryFixture.Hash(acceptSource) }
                },
                human_decisions = new object[]
                {
                    new { kind = "story-acceptance", decision = "APPROVED", revision, actor = "dev-synthetic", timestamp = "2026-11-20T12:00:00Z", source = "evidence/acceptance-source.md" }
                }
            }));

            WriteFeatureFile(fixture, "verification/impossibility-ready.json", JsonSerializer.Serialize(new
            {
                schema_version = 1,
                stage = "dev-impossibility",
                revision,
                state = "CONFIRMED_BY_DEV",
                story = "US5",
                scenarios = new[] { "US5-S06" },
                reason = "Synthetic impossibility",
                attempts = new[] { "Attempt 1" },
                unverified = new[] { "External host" },
                risk = "Low",
                scope = "Synthetic",
                next_action = "Wait",
                next_owner = "DEV",
                decision = "WAIT",
                evidence = new object[]
                {
                    new { path = "evidence/dev-impossibility-source.md", kind = "document", sha256 = DocumentaryFixture.Hash(impossSource) }
                },
                human_confirmation = new { actor = "dev-synthetic", timestamp = "2026-11-20T12:00:00Z", source = "evidence/dev-impossibility-source.md" }
            }));

            fixture.CaptureBaseline();

            using var projects = CreateProjectService();
            var ctx = projects.OpenSelected(fixture.Origin, fixture.Output);
            var registry = CreatePassedGateRegistry();
            var service = new VerificationService(projects, registry);
            var package = new GatePackage();
            var executor = new GateExecutor(package);
            var runtime = package.ResolvePythonRuntime(fixture.Origin);
            Assert.True(runtime.IsAvailable, runtime.Message);

            var feature = ctx.Session.Features.First(f => f.SpecKitRelativeDirectory.Contains("001-demo"));
            var availability = service.EvaluateAvailability(VerificationType.CheckGate, ProtectedCapability.GateAdapter, GatePackage.CoreVersion, "win-x64", "v4");

            var cases = new (string RecordRel, GateStage Stage, GateSemanticResult ExpectedSemantic, int ExpectedExit)[]
            {
                ("verification/ready.json", GateStage.Spec, GateSemanticResult.Ready, 0),
                ("verification/blocked.json", GateStage.Spec, GateSemanticResult.Blocked, 1),
                ("verification/bad-hash.json", GateStage.Spec, GateSemanticResult.Blocked, 1),
                ("verification/wrong-revision.json", GateStage.Spec, GateSemanticResult.Blocked, 1),
                ("verification/pr-ready.json", GateStage.Pr, GateSemanticResult.Ready, 0),
                ("verification/acceptance-ready.json", GateStage.Acceptance, GateSemanticResult.Ready, 0),
                ("verification/impossibility-ready.json", GateStage.DevImpossibility, GateSemanticResult.DevImpossibilityRecorded, 0)
            };

            var officialCore = Path.Combine(FindRepoRoot(), ".specify", "presets", "cemg-speckit-preset", "tools", "check_gate.py");

            foreach (var c in cases)
            {
                var recordEntry = ctx.Catalog.Entries.First(e =>
                    e.FeatureId == feature.FeatureId &&
                    e.RelativePath.Replace('\\', '/').EndsWith("/" + c.RecordRel, StringComparison.OrdinalIgnoreCase));

                var candidate = new GateCandidate(
                    ctx.Session.ProjectId,
                    feature.FeatureId,
                    c.Stage,
                    recordEntry.DocumentId,
                    revision,
                    RevisionSource.ArtifactSha256,
                    [],
                    ctx.Catalog.Generation);

                var prepared = service.PrepareGateRun(ctx.Session.ProjectId, candidate, availability);
                Assert.True(prepared.Created);
                var run = prepared.Run!;

                var meta = await executor.ExecuteAsync(ctx, candidate, run);
                Assert.NotNull(meta);
                Assert.Equal(RunState.Completed, run.State);
                Assert.Equal(c.ExpectedSemantic, run.SemanticResult!.GateResult);
                Assert.Equal(c.ExpectedExit, run.ExitCode);
                Assert.True(meta!.BrokerReadCount >= 1);
                Assert.Contains("check_gate cemg-speckit-preset 1.0.0", run.ToolIdentifier);

                // Compare against official cemg-speckit-preset 1.0.0 CLI on the exact same synthetic fixture (offline, no --project).
                var official = RunOfficialCli(runtime.ExecutablePath!, officialCore, fixture.Feature, c.RecordRel, c.Stage.ToCliName(), revision);
                Assert.Equal(official.ExitCode, run.ExitCode);
                Assert.Equal(official.Result, JsonDocument.Parse(run.Output).RootElement.GetProperty("result").GetString());
                Assert.Equal(official.Errors, ReadErrors(run.Output));
            }

            Assert.True(fixture.IsUnchanged());
            Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Output));
        }
        finally
        {
            Directory.Delete(fixture.Root, recursive: true);
        }
    }

    [Fact]
    public async Task S05AndS06SeparateAvailabilityCoreResultErrorAndCancellation()
    {
        var fixture = DocumentaryFixture.Create();
        try
        {
            using var projects = CreateProjectService();
            var ctx = projects.OpenSelected(fixture.Origin, fixture.Output);
            var unverifiedRegistry = new CapabilityRegistry();
            var unverifiedService = new VerificationService(projects, unverifiedRegistry);

            var feature = ctx.Session.Features.First(f => f.SpecKitRelativeDirectory.Contains("001-demo"));
            var record = ctx.Catalog.Entries.First(e => e.RelativePath.EndsWith("ready.json", StringComparison.OrdinalIgnoreCase));
            var candidate = new GateCandidate(
                ctx.Session.ProjectId,
                feature.FeatureId,
                GateStage.Spec,
                record.DocumentId,
                "sha256:" + DocumentaryFixture.Hash(Path.Combine(fixture.Feature, "spec.md")),
                RevisionSource.ArtifactSha256,
                [],
                ctx.Catalog.Generation);

            // PendingValidation must not create a run.
            var pending = unverifiedService.EvaluateAvailability(VerificationType.CheckGate, ProtectedCapability.GateAdapter, GatePackage.CoreVersion, "win-x64", "v4");
            Assert.Equal(CapabilityAvailability.PendingValidation, pending.Availability);
            Assert.False(unverifiedService.PrepareGateRun(ctx.Session.ProjectId, candidate, pending).Created);

            // Missing Python must report Unavailable and not create a run.
            var missingPkg = new GatePackage(customPythonLocator: () => null);
            var missingRuntime = missingPkg.ResolvePythonRuntime(fixture.Origin);
            Assert.False(missingRuntime.IsAvailable);
            var passedRegistry = CreatePassedGateRegistry();
            var service = new VerificationService(projects, passedRegistry);
            var unavailable = service.EvaluateAvailability(VerificationType.CheckGate, ProtectedCapability.GateAdapter, GatePackage.CoreVersion, "win-x64", "v4", runtimeAvailable: false, runtimeUnavailableReason: missingRuntime.Message);
            Assert.Equal(CapabilityAvailability.Unavailable, unavailable.Availability);
            Assert.False(service.PrepareGateRun(ctx.Session.ProjectId, candidate, unavailable).Created);

            // Deterministic cancellation during broker read terminates child process and marks Cancelled with null SemanticResult.
            var available = service.EvaluateAvailability(VerificationType.CheckGate, ProtectedCapability.GateAdapter, GatePackage.CoreVersion, "win-x64", "v4");
            var prepared = service.PrepareGateRun(ctx.Session.ProjectId, candidate, available);
            var run = prepared.Run!;

            using var cts = new CancellationTokenSource();
            var executor = new GateExecutor();
            await executor.ExecuteAsync(ctx, candidate, run, cts.Token, beforeBrokerRead: _ => cts.Cancel());

            Assert.Equal(RunState.Cancelled, run.State);
            Assert.True(run.CancelRequested);
            Assert.True(run.TerminationConfirmed);
            Assert.Null(run.SemanticResult);
            Assert.True(fixture.IsUnchanged());
            Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Output));
        }
        finally
        {
            Directory.Delete(fixture.Root, recursive: true);
        }
    }

    [Fact]
    public async Task S07DeniesCodeReferenceNestedLogsOutsideArtifactsAndJunctionToctouOffline()
    {
        var fixture = DocumentaryFixture.Create();
        try
        {
            var specHash = DocumentaryFixture.Hash(Path.Combine(fixture.Feature, "spec.md"));
            var revision = "sha256:" + specHash;

            // Nested test-run referencing ../../DoNotRead.cs in stdout.
            var badTestRunPath = WriteFeatureFile(fixture, "evidence/bad-test-run.json", JsonSerializer.Serialize(new
            {
                revision,
                result = "PASSED",
                exit_code = 0,
                command = "synthetic",
                timestamp = "2026-11-20T12:00:00Z",
                stdout = new { path = "../../DoNotRead.cs", sha256 = new string('0', 64) },
                stderr = new { path = "spec.md", sha256 = specHash }
            }));

            WriteFeatureFile(fixture, "verification/nested-code-log.json", JsonSerializer.Serialize(new
            {
                schema_version = 1,
                stage = "spec",
                revision,
                state = "READY",
                blockers = Array.Empty<string>(),
                checks = new Dictionary<string, bool>
                {
                    ["criteria_observable"] = true,
                    ["scenarios_defined"] = true,
                    ["scope_defined"] = true,
                    ["dependencies_identified"] = true,
                    ["material_decisions_resolved"] = true
                },
                evidence = new object[]
                {
                    new { path = "evidence/bad-test-run.json", kind = "test-run", sha256 = DocumentaryFixture.Hash(badTestRunPath) }
                }
            }));

            // Nested product-execution referencing ../../../outside.md in artifacts.
            var badExecPath = WriteFeatureFile(fixture, "evidence/bad-exec.json", JsonSerializer.Serialize(new
            {
                revision,
                state = "OBSERVED",
                executor = "AI",
                medium = "UI",
                startup = "dotnet run",
                input = "x",
                expected = "y",
                observed = "y",
                effects_checked = "z",
                scenarios = new[] { "US5-S07" },
                artifacts = new[] { new { path = "../../../outside.md", sha256 = new string('0', 64) } }
            }));

            WriteFeatureFile(fixture, "verification/nested-outside-artifact.json", JsonSerializer.Serialize(new
            {
                schema_version = 1,
                stage = "spec",
                revision,
                state = "READY",
                blockers = Array.Empty<string>(),
                checks = new Dictionary<string, bool>
                {
                    ["criteria_observable"] = true,
                    ["scenarios_defined"] = true,
                    ["scope_defined"] = true,
                    ["dependencies_identified"] = true,
                    ["material_decisions_resolved"] = true
                },
                evidence = new object[]
                {
                    new { path = "evidence/bad-exec.json", kind = "product-execution", sha256 = DocumentaryFixture.Hash(badExecPath) }
                }
            }));

            // Junction inside evidence/ pointing outside the feature.
            var outsideDir = Path.Combine(fixture.Root, "outside-junction-target");
            Directory.CreateDirectory(outsideDir);
            File.WriteAllText(Path.Combine(outsideDir, "secret.md"), "Outside secret");
            var junctionDir = Path.Combine(fixture.Feature, "evidence", "linked");
            Directory.CreateDirectory(Path.Combine(fixture.Feature, "evidence"));
            CreateJunction(junctionDir, outsideDir);

            WriteFeatureFile(fixture, "verification/junction-ref.json", JsonSerializer.Serialize(new
            {
                schema_version = 1,
                stage = "spec",
                revision,
                state = "READY",
                blockers = Array.Empty<string>(),
                checks = new Dictionary<string, bool>
                {
                    ["criteria_observable"] = true,
                    ["scenarios_defined"] = true,
                    ["scope_defined"] = true,
                    ["dependencies_identified"] = true,
                    ["material_decisions_resolved"] = true
                },
                evidence = new object[]
                {
                    new { path = "evidence/linked/secret.md", kind = "document", sha256 = new string('0', 64) }
                }
            }));

            fixture.CaptureBaseline();

            using var projects = CreateProjectService();
            var ctx = projects.OpenSelected(fixture.Origin, fixture.Output);
            var registry = CreatePassedGateRegistry();
            var service = new VerificationService(projects, registry);
            var executor = new GateExecutor();
            var feature = ctx.Session.Features.First(f => f.SpecKitRelativeDirectory.Contains("001-demo"));
            var availability = service.EvaluateAvailability(VerificationType.CheckGate, ProtectedCapability.GateAdapter, GatePackage.CoreVersion, "win-x64", "v4");

            var observedDocIds = new List<Guid>();
            foreach (var blockedRel in new[]
            {
                "verification/code-reference.json",
                "verification/nested-code-log.json",
                "verification/nested-outside-artifact.json",
                "verification/junction-ref.json"
            })
            {
                var recEntry = ctx.Catalog.Entries.First(e =>
                    e.FeatureId == feature.FeatureId &&
                    e.RelativePath.Replace('\\', '/').EndsWith("/" + blockedRel, StringComparison.OrdinalIgnoreCase));
                var candidate = new GateCandidate(ctx.Session.ProjectId, feature.FeatureId, GateStage.Spec, recEntry.DocumentId, revision, RevisionSource.ArtifactSha256, [], ctx.Catalog.Generation);
                var prepared = service.PrepareGateRun(ctx.Session.ProjectId, candidate, availability);
                var run = prepared.Run!;
                await executor.ExecuteAsync(ctx, candidate, run, beforeBrokerRead: id => observedDocIds.Add(id));

                Assert.Equal(RunState.Blocked, run.State);
                Assert.Null(run.SemanticResult);
            }

            // DoNotRead.cs and outside secret.md are never in the catalog and Python runner was never even invoked for blocked preflights.
            Assert.Empty(observedDocIds);
            Assert.DoesNotContain(ctx.Catalog.Entries, e => e.RelativePath.Contains("DoNotRead.cs", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(ctx.Catalog.Entries, e => e.RelativePath.Contains("secret.md", StringComparison.OrdinalIgnoreCase));
            Assert.True(fixture.IsUnchanged());

            // Read-time TOCTOU: mutate spec.md right before broker serves spec.md to runner.py -> must NOT return READY and must mark Stale!
            var readyEntry = ctx.Catalog.Entries.First(e => e.RelativePath.EndsWith("ready.json", StringComparison.OrdinalIgnoreCase));
            var specEntry = ctx.Catalog.Entries.First(e => e.FeatureId == feature.FeatureId && e.RelativePath.EndsWith("spec.md", StringComparison.OrdinalIgnoreCase));
            var toctouCandidate = new GateCandidate(ctx.Session.ProjectId, feature.FeatureId, GateStage.Spec, readyEntry.DocumentId, revision, RevisionSource.ArtifactSha256, [], ctx.Catalog.Generation);
            var toctouPrep = service.PrepareGateRun(ctx.Session.ProjectId, toctouCandidate, availability);
            var toctouRun = toctouPrep.Run!;

            var mutated = false;
            await executor.ExecuteAsync(ctx, toctouCandidate, toctouRun, beforeBrokerRead: docId =>
            {
                if (!mutated && docId == specEntry.DocumentId)
                {
                    mutated = true;
                    File.AppendAllText(Path.Combine(fixture.Feature, "spec.md"), "\n- FR-999: TOCTOU concurrent mutation\n");
                }
            });

            Assert.True(mutated);
            Assert.NotEqual(RunState.Completed, toctouRun.State);
            Assert.Null(toctouRun.SemanticResult);
            Assert.Equal(Freshness.Stale, toctouRun.Freshness);

            Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Output));
        }
        finally
        {
            var junction = Path.Combine(fixture.Feature, "evidence", "linked");
            if (Directory.Exists(junction)) Directory.Delete(junction, recursive: false);
            Directory.Delete(fixture.Root, recursive: true);
        }
    }

    private static string WriteFeatureFile(DocumentaryFixture fixture, string featureRelative, string content)
    {
        var full = Path.Combine(fixture.Feature, featureRelative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, content);
        return full;
    }

    private static void CreateJunction(string junctionPath, string targetPath)
    {
        var start = new ProcessStartInfo("cmd.exe", $"/c mklink /J \"{junctionPath}\" \"{targetPath}\"")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using var process = Process.Start(start)!;
        process.WaitForExit(5000);
        Assert.Equal(0, process.ExitCode);
    }

    private static (int ExitCode, string Result, string[] Errors) RunOfficialCli(
        string pythonExe,
        string checkGatePy,
        string featureDir,
        string recordRel,
        string stage,
        string revision)
    {
        var start = new ProcessStartInfo(pythonExe)
        {
            WorkingDirectory = Path.GetTempPath(),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        start.ArgumentList.Add("-I");
        start.ArgumentList.Add("-B");
        start.ArgumentList.Add(checkGatePy);
        start.ArgumentList.Add("--feature");
        start.ArgumentList.Add(featureDir);
        start.ArgumentList.Add("--record");
        start.ArgumentList.Add(recordRel);
        start.ArgumentList.Add("--stage");
        start.ArgumentList.Add(stage);
        start.ArgumentList.Add("--revision");
        start.ArgumentList.Add(revision);

        using var process = Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEnd();
        process.WaitForExit(10000);
        using var doc = JsonDocument.Parse(stdout);
        var result = doc.RootElement.GetProperty("result").GetString()!;
        var errors = ReadErrors(stdout);
        return (process.ExitCode, result, errors);
    }

    private static string[] ReadErrors(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("errors").EnumerateArray().Select(e => e.GetString()!).ToArray();
    }

    private static ProjectService CreateProjectService()
    {
        var registry = new LocalSessionRegistry();
        var issued = registry.Issue();
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        accessor.HttpContext.Items[typeof(LocalSessionService)] = issued.Id;
        return new ProjectService(new LocalSessionService(accessor, registry), new UnusedPicker());
    }

    private static CapabilityRegistry CreatePassedGateRegistry()
    {
        var registry = new CapabilityRegistry();
        var proof = new ProtectionAssessment(GatePackage.CoreVersion, "win-x64", "v4");
        var checks = new[]
        {
            "package-integrity",
            "offline-no-shell",
            "core-equivalence",
            "nested-evidence",
            "read-time-toctou",
            "cancel-zero-writes"
        }.ToDictionary(c => c, _ => true);
        proof.RecordPassed(checks, ["synthetic-gate-test"]);
        registry.RecordProof(ProtectedCapability.GateAdapter, proof);
        return registry;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "SpecAudit.slnx")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new DirectoryNotFoundException("SpecAudit.slnx not found.");
    }
}
