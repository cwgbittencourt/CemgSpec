using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SpecAudit.Application;
using SpecAudit.Domain;
using SpecAudit.Infrastructure.Reports;
using SpecAudit.Windows.Tests.Fixtures;

namespace SpecAudit.Windows.Tests;

public sealed class ExternalWriterTests
{
    private static readonly Assembly ProductAssembly = typeof(ProjectSession).Assembly;

    private sealed class UnusedPicker : IFolderPicker
    {
        public Task<string?> PickAsync(string title) => Task.FromResult<string?>(null);
    }

    [Fact]
    public void S01AndS02ExternalWriterWritesCreateNewManifestLastWithSafeCsvAndNeverTouchesOrigin()
    {
        var fixture = DocumentaryFixture.Create();
        try
        {
            using var projectService = CreateProjectService();
            var ctx = projectService.OpenSelected(fixture.Origin, fixture.Output);

            var finding = new Finding(
                Guid.NewGuid(),
                null,
                "SpecGap",
                FindingSeverity.High,
                "=SUM(A1:A2) fórmula perigosa",
                [],
                "+cmd|' /C calc'!A0",
                "fp-001");
            var decision = new HumanDecision(
                finding.FindingId,
                DecisionClassification.AcceptedRisk,
                "-nota inicial",
                "@revisor",
                finding.ContextFingerprint);

            var payload = new ReportExportPayload(
                SavedRecordId: Guid.NewGuid(),
                ProjectId: ctx.Session.ProjectId,
                ProjectDisplayName: ctx.Session.DisplayName,
                Scope: "Projeto completo",
                CreatedUtc: new DateTimeOffset(2026, 11, 20, 18, 0, 0, TimeSpan.Zero),
                IsStale: false,
                SelectedFormats: ExportFormat.Markdown | ExportFormat.Json | ExportFormat.Csv,
                DestinationIdentity: ctx.Session.OutputDirectoryIdentity,
                SourceHashes: new Dictionary<string, string> { ["specs/001/spec.md"] = "abc123" },
                ToolVersions: ["SpecAudit 1.0.0"],
                Limitations: ["Auditoria exclusivamente documental; não lê código-fonte."],
                Findings: [finding],
                Decisions: [decision],
                Indicators: null);

            var writeOrder = new List<string>();
            var writer = new ExternalReportWriter();
            var result = writer.Save(ctx, payload, beforeFileWrite: name => writeOrder.Add(name));

            Assert.Equal(SaveState.Saved, result.State);
            Assert.NotNull(result.Record);
            Assert.NotNull(result.RecordDirectoryPath);
            Assert.True(Directory.Exists(result.RecordDirectoryPath));

            // manifest.json MUST be written last
            Assert.Equal("manifest.json", writeOrder[^1]);
            Assert.Equal(["report.md", "report.json", "findings.csv", "indicators.csv", "manifest.json"], writeOrder);

            // Verify manifest schema_version=1 and state=Complete
            var manifestJson = File.ReadAllText(Path.Combine(result.RecordDirectoryPath!, "manifest.json"));
            using var manifestDoc = JsonDocument.Parse(manifestJson);
            Assert.Equal(1, manifestDoc.RootElement.GetProperty("schema_version").GetInt32());
            Assert.Equal("Complete", manifestDoc.RootElement.GetProperty("state").GetString());

            // Verify CSV formula neutralization
            var csv = File.ReadAllText(Path.Combine(result.RecordDirectoryPath!, "findings.csv"));
            Assert.Contains("'=SUM(A1:A2) fórmula perigosa", csv);
            Assert.Contains("'+cmd|' /C calc'!A0", csv);
            Assert.Contains("'-nota inicial", csv);
            Assert.Contains("'@revisor", csv);

            // Second save with identical SavedRecordId MUST fail (create-new, no overwrite)
            var duplicate = writer.Save(ctx, payload);
            Assert.Equal(SaveState.SaveFailed, duplicate.State);
            Assert.Equal("RecordAlreadyExists", duplicate.ErrorCode);

            // Origin MUST remain completely untouched
            Assert.True(fixture.IsUnchanged());
        }
        finally
        {
            Directory.Delete(fixture.Root, recursive: true);
        }
    }

    [Fact]
    public void S01AndS02ExternalWriterRejectsOriginDestinationJunctionSwapAndReportsPartialFailureHonestly()
    {
        var fixture = DocumentaryFixture.Create();
        try
        {
            using var projectService = CreateProjectService();
            var ctx = projectService.OpenSelected(fixture.Origin, fixture.Output);

            var writer = new ExternalReportWriter();
            var payload = new ReportExportPayload(
                SavedRecordId: Guid.NewGuid(),
                ProjectId: ctx.Session.ProjectId,
                ProjectDisplayName: ctx.Session.DisplayName,
                Scope: "Projeto completo",
                CreatedUtc: DateTimeOffset.UtcNow,
                IsStale: false,
                SelectedFormats: ExportFormat.Markdown | ExportFormat.Json,
                DestinationIdentity: ctx.Session.OutputDirectoryIdentity,
                SourceHashes: new Dictionary<string, string>(),
                ToolVersions: ["SpecAudit 1.0.0"],
                Limitations: ["Auditoria documental."],
                Findings: [],
                Decisions: [],
                Indicators: null);

            // 1. Partial write failure reports honest SaveFailed and does NOT delete partial directory
            var partial = writer.Save(ctx, payload, beforeFileWrite: name =>
            {
                if (name == "report.json")
                    throw new IOException("Simulated disk full during report.json write.");
            });

            Assert.Equal(SaveState.SaveFailed, partial.State);
            Assert.Equal("PartialWriteFailure", partial.ErrorCode);
            Assert.NotNull(partial.RecordDirectoryPath);
            Assert.True(Directory.Exists(partial.RecordDirectoryPath));
            Assert.True(File.Exists(Path.Combine(partial.RecordDirectoryPath!, "report.md")));
            Assert.False(File.Exists(Path.Combine(partial.RecordDirectoryPath!, "manifest.json")));

            // 2. Destination identity swap (recreate output directory) MUST be rejected
            var outputBackup = fixture.Output + "-old";
            Directory.Move(fixture.Output, outputBackup);
            Directory.CreateDirectory(fixture.Output);

            var swappedPayload = payload with { SavedRecordId = Guid.NewGuid() };
            var swappedResult = writer.Save(ctx, swappedPayload);
            Assert.Equal(SaveState.SaveFailed, swappedResult.State);
            Assert.Equal("DestinationIdentityChanged", swappedResult.ErrorCode);

            // Origin MUST remain completely untouched
            Assert.True(fixture.IsUnchanged());
        }
        finally
        {
            Directory.Delete(fixture.Root, recursive: true);
        }
    }

    [Fact]
    public void S04SavedHistoryReadsValidPackagesDiffsFindingsAndDeletesOnlySelectedRecord()
    {
        var fixture = DocumentaryFixture.Create();
        try
        {
            using var projectService = CreateProjectService();
            var ctx = projectService.OpenSelected(fixture.Origin, fixture.Output);
            var projectId = ctx.Session.ProjectId;

            var writer = new ExternalReportWriter();
            var registry = new CapabilityRegistry();
            Assert.Equal(CapabilityAvailability.PendingValidation,
                registry.Availability(ProtectedCapability.ExternalWriter, ExternalReportWriter.WriterVersion, ExternalReportWriter.PlatformFingerprint, ExternalReportWriter.PolicyVersion));
            Assert.True(writer.TryRegisterVerifiedSeal(registry));
            Assert.Equal(CapabilityAvailability.Available,
                registry.Availability(ProtectedCapability.ExternalWriter, ExternalReportWriter.WriterVersion, ExternalReportWriter.PlatformFingerprint, ExternalReportWriter.PolicyVersion));

            var findingA = new Finding(Guid.NewGuid(), null, "SpecGap", FindingSeverity.High, "Gap A", [], null, "fp-a");
            var findingB = new Finding(Guid.NewGuid(), null, "TaskGap", FindingSeverity.Medium, "Gap B", [], null, "fp-b");
            var findingC = new Finding(Guid.NewGuid(), null, "PlanGap", FindingSeverity.Low, "Gap C", [], null, "fp-c");

            var rec1Id = Guid.NewGuid();
            var rec2Id = Guid.NewGuid();
            var save1 = writer.Save(ctx, new ReportExportPayload(
                rec1Id, projectId, ctx.Session.DisplayName, "Projeto completo",
                new DateTimeOffset(2026, 11, 20, 18, 0, 0, TimeSpan.Zero), false,
                ExportFormat.Markdown | ExportFormat.Json, ctx.Session.OutputDirectoryIdentity,
                new Dictionary<string, string>(), ["SpecAudit 1.0.0"], ["Documental."],
                [findingA, findingB], [], null));
            Assert.Equal(SaveState.Saved, save1.State);

            var save2 = writer.Save(ctx, new ReportExportPayload(
                rec2Id, projectId, ctx.Session.DisplayName, "Projeto completo",
                new DateTimeOffset(2026, 11, 20, 18, 5, 0, TimeSpan.Zero), false,
                ExportFormat.Markdown | ExportFormat.Json, ctx.Session.OutputDirectoryIdentity,
                new Dictionary<string, string>(), ["SpecAudit 1.0.0"], ["Documental."],
                [findingB, findingC], [], null));
            Assert.Equal(SaveState.Saved, save2.State);

            // Unrelated directory and incomplete SpecAudit directory
            var unrelatedDir = Path.Combine(fixture.Output, "other-notes");
            Directory.CreateDirectory(unrelatedDir);
            File.WriteAllText(Path.Combine(unrelatedDir, "note.txt"), "keep me");
            var incompleteDir = Path.Combine(fixture.Output, "SpecAudit-20261120T181000Z-incomplete");
            Directory.CreateDirectory(incompleteDir);
            File.WriteAllText(Path.Combine(incompleteDir, "report.md"), "partial");

            var history = new SavedHistoryService(projectService, new SavedHistoryReader());
            var listed = history.ListSaved(projectId);
            Assert.Equal(3, listed.Count);
            Assert.Equal(2, listed.Count(p => p.IsValid));
            Assert.Single(listed, p => p.IsIncomplete && !p.IsValid);
            Assert.True(history.GetTotalDiskBytes(projectId) > 0);

            // Compare baseline (save1: A, B) against current (B, C) -> A Resolved, B Persistent, C New
            var baseline = listed.Single(p => p.Record?.SavedRecordId == rec1Id);
            var diff = history.Compare(baseline, [findingB, findingC], currentSampleIsComplete: true);
            Assert.True(diff.IsComparable);
            Assert.Contains(diff.Entries, e => e.Status == FindingDiffStatus.Resolved && e.Category == "SpecGap");
            Assert.Contains(diff.Entries, e => e.Status == FindingDiffStatus.Persistent && e.Category == "TaskGap");
            Assert.Contains(diff.Entries, e => e.Status == FindingDiffStatus.New && e.Category == "PlanGap");

            // Incomplete current sample MUST NOT infer Resolved
            var inconclusive = history.Compare(baseline, [findingB], currentSampleIsComplete: false);
            Assert.False(inconclusive.IsComparable);
            Assert.Empty(inconclusive.Entries);

            // Tampering with save2 report.md marks it IsTampered
            File.AppendAllText(Path.Combine(save2.RecordDirectoryPath!, "report.md"), "\nTAMPERED");
            var afterTamper = history.ListSaved(projectId);
            Assert.Single(afterTamper, p => p.DirectoryName == save2.Record!.RecordDirectoryName && p.IsTampered && !p.IsValid);

            // Delete ONLY rec1; rec2, unrelatedDir and Origin MUST remain untouched
            var deleted = history.DeleteSaved(projectId, rec1Id);
            Assert.True(deleted.Deleted);
            Assert.False(Directory.Exists(save1.RecordDirectoryPath));
            Assert.True(Directory.Exists(save2.RecordDirectoryPath));
            Assert.True(Directory.Exists(unrelatedDir));
            Assert.True(fixture.IsUnchanged());
        }
        finally
        {
            Directory.Delete(fixture.Root, recursive: true);
        }
    }

    private static Type RequireType(string fullName)
    {
        var type = ProductAssembly.GetType(fullName, throwOnError: false);
        Assert.True(type is not null, $"Expected contract type '{fullName}' to be implemented in T081/T083.");
        return type!;
    }

    private static ProjectService CreateProjectService()
    {
        var registry = new LocalSessionRegistry();
        var issued = registry.Issue();
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        accessor.HttpContext.Items[typeof(LocalSessionService)] = issued.Id;
        var session = new LocalSessionService(accessor, registry);
        return new ProjectService(session, new UnusedPicker());
    }
}
