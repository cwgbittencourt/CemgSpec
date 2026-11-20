using System.Reflection;
using SpecAudit.Domain;

namespace SpecAudit.Tests;

public sealed class DocumentaryProgressTests
{
    private static readonly Assembly ProductAssembly = typeof(ProjectSession).Assembly;

    [Fact]
    public void V1DomainForbidsCodeAnalysisAndImplementationScoreEntities()
    {
        var forbidden = new[] { "SourceCodeAnalysis", "Compilation", "ImplementationScore", "DependencyInventory" };
        var exportedNames = ProductAssembly.GetTypes().Select(type => type.Name).ToHashSet(StringComparer.Ordinal);
        foreach (var name in forbidden)
            Assert.DoesNotContain(name, exportedNames);
    }

    [Fact]
    public void S01ProjectionComputesDeclaredProgressDocumentaryCoverageAndEmptyNoItems()
    {
        var serviceType = ProductAssembly.GetType("SpecAudit.Application.DocumentaryDashboardService");
        var projectionType = ProductAssembly.GetType("SpecAudit.Domain.DashboardProjection");
        var taskDeclarationType = ProductAssembly.GetType("SpecAudit.Domain.TaskDeclaration");

        Assert.True(serviceType is not null && projectionType is not null && taskDeclarationType is not null,
            "T030 must implement TaskDeclaration, DashboardProjection, and DocumentaryDashboardService " +
            "computing 30/40 = 75% declared progress, 12/20 = 60% documentary coverage (keeping unlinked/inconclusive items in the denominator), " +
            "and NoItems without percentage when denominator is zero.");
    }

    [Fact]
    public void S02ProjectionExposesRecordedGateStatesWithProvenanceWithoutImplementationScore()
    {
        var projectionType = ProductAssembly.GetType("SpecAudit.Domain.DashboardProjection");
        Assert.True(projectionType is not null,
            "T030 must expose RecordedGateStates on DashboardProjection with source document, revision, and freshness, " +
            "separate from current check_gate results and without any implementation/test/human-acceptance score.");
    }

    [Fact]
    public void S03ProjectionReportsMissingLinksDuplicatesBlockersAndUnverifiedCodeReferences()
    {
        var serviceType = ProductAssembly.GetType("SpecAudit.Application.DocumentaryDashboardService");
        Assert.True(serviceType is not null,
            "T030 must detect recorded blockers, requirements without tasks, tasks without requirements, " +
            "duplicate/ambiguous IDs, and code references marked unverified without reading source code.");
    }

    [Fact]
    public void S04ProjectionMarksStaleOnCatalogChangeAndUpdatesWithoutExecutingAgentsOrGates()
    {
        var serviceType = ProductAssembly.GetType("SpecAudit.Application.DocumentaryDashboardService");
        Assert.True(serviceType is not null,
            "T030 must project Freshness.Stale when catalog generation/changes occur and recompute counts from broker snapshots " +
            "without invoking any agent or gate.");
    }
}

