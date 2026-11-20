using Microsoft.AspNetCore.Http;
using SpecAudit.Application;
using SpecAudit.Domain;

namespace SpecAudit.Tests;

public sealed class FindingDecisionTests
{
    private sealed class UnusedPicker : IFolderPicker
    {
        public Task<string?> PickAsync(string title) => Task.FromResult<string?>(null);
    }

    [Fact]
    public void S03HumanDecisionAndSavedRecordContractsExposeRequiredClassificationsAndSaveStates()
    {
        var classifications = Enum.GetNames<DecisionClassification>();
        Assert.Equal(
            ["Pending", "Confirmed", "FalsePositive", "AcceptedRisk", "Deferred"],
            classifications);

        var saveStates = Enum.GetNames<SaveState>();
        Assert.Equal(
            ["Unsaved", "Saving", "Saved", "SaveFailed"],
            saveStates);
    }

    [Fact]
    public void S03FalsePositiveAndAcceptedRiskRequireReasonAndActorAndContextChangeMarksNeedsReview()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "SpecAudit-T080-" + Guid.NewGuid().ToString("N"));
        var origin = Path.Combine(tempRoot, "origin");
        var output = Path.Combine(tempRoot, "output");
        Directory.CreateDirectory(origin);
        Directory.CreateDirectory(output);
        File.WriteAllText(Path.Combine(origin, "README.md"), "# SpecAudit Test\n");

        try
        {
            using var projectService = CreateProjectService();
            var ctx = projectService.OpenSelected(origin, output);
            var projectId = ctx.Session.ProjectId;

            var service = new FindingDecisionService(projectService);
            var finding = new Finding(
                Guid.NewGuid(),
                null,
                "SpecGap",
                FindingSeverity.Medium,
                "FR-001 sem critério de aceitação explícito.",
                [],
                "Adicionar critério verificável em spec.md.",
                "fp-v1");

            // FalsePositive and AcceptedRisk MUST require both Reason and Actor
            Assert.Throws<ArgumentException>(() =>
                service.Classify(projectId, finding, DecisionClassification.FalsePositive, "", "Ana"));
            Assert.Throws<ArgumentException>(() =>
                service.Classify(projectId, finding, DecisionClassification.FalsePositive, "Coberto na seção 4", ""));
            Assert.Throws<ArgumentException>(() =>
                service.Classify(projectId, finding, DecisionClassification.AcceptedRisk, "   ", "Ana"));
            Assert.Throws<ArgumentException>(() =>
                service.Classify(projectId, finding, DecisionClassification.AcceptedRisk, "Aceito para V1", " "));

            // Valid FalsePositive classification preserves technical finding and records metadata
            var decision = service.Classify(
                projectId,
                finding,
                DecisionClassification.FalsePositive,
                "Coberto em requisitos não-funcionais.",
                "Ana Silva");

            Assert.Equal(finding.FindingId, decision.FindingId);
            Assert.Equal(DecisionClassification.FalsePositive, decision.Classification);
            Assert.Equal("Coberto em requisitos não-funcionais.", decision.Reason);
            Assert.Equal("Ana Silva", decision.Actor);
            Assert.Equal("fp-v1", decision.ContextFingerprint);
            Assert.False(decision.NeedsReview);

            // Context fingerprint change marks decision as NeedsReview without erasing the technical finding
            var updatedFinding = finding with { ContextFingerprint = "fp-v2" };
            service.ReconcileContextChanges(projectId, [updatedFinding]);

            var refreshed = service.GetDecision(projectId, finding.FindingId);
            Assert.NotNull(refreshed);
            Assert.True(refreshed!.NeedsReview);
            Assert.Equal(DecisionClassification.FalsePositive, refreshed.Classification);
            Assert.Equal("FR-001 sem critério de aceitação explícito.", updatedFinding.Explanation);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
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
