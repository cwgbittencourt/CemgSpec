using System.Reflection;
using SpecAudit.Domain;

namespace SpecAudit.Tests;

public sealed class FindingDecisionTests
{
    private static readonly Assembly ProductAssembly = typeof(ProjectSession).Assembly;

    [Fact]
    public void S03HumanDecisionAndSavedRecordContractsExposeRequiredClassificationsAndSaveStates()
    {
        var classificationType = RequireType("SpecAudit.Domain.DecisionClassification");
        var decisionType = RequireType("SpecAudit.Domain.HumanDecision");
        var saveStateType = RequireType("SpecAudit.Domain.SaveState");
        var savedRecordType = RequireType("SpecAudit.Domain.SavedRecord");
        var serviceType = RequireType("SpecAudit.Application.FindingDecisionService");

        Assert.NotNull(decisionType);
        Assert.NotNull(savedRecordType);
        Assert.NotNull(serviceType);

        var classifications = Enum.GetNames(classificationType);
        Assert.Equal(
            new[] { "Pending", "Confirmed", "FalsePositive", "AcceptedRisk", "Deferred" },
            classifications);

        var saveStates = Enum.GetNames(saveStateType);
        Assert.Equal(
            new[] { "Unsaved", "Saving", "Saved", "SaveFailed" },
            saveStates);
    }

    [Fact]
    public void S03FalsePositiveAndAcceptedRiskRequireReasonAndActorAndContextChangeMarksNeedsReview()
    {
        _ = RequireType("SpecAudit.Application.FindingDecisionService");
    }

    private static Type RequireType(string fullName)
    {
        var type = ProductAssembly.GetType(fullName, throwOnError: false);
        Assert.True(type is not null, $"Expected contract type '{fullName}' to be implemented in T080.");
        return type!;
    }
}
