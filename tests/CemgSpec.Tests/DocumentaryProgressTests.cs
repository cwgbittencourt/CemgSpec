using System.Reflection;
using CemgSpec.Application;
using CemgSpec.Domain;
using CemgSpec.Infrastructure.Documents;

namespace CemgSpec.Tests;

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
        using var fixture = TempDocumentaryOrigin.Create();
        using var catalog = new DocumentCatalog(Guid.NewGuid(), fixture.Origin);
        var features = catalog.Features.Select(pair => new Feature(pair.Key, catalog.ProjectId, pair.Value, Path.GetFileName(pair.Value))).ToArray();
        var demo = Assert.Single(features, feature => feature.Title == "001-demo");
        var empty = Assert.Single(features, feature => feature.Title == "003-empty");

        var service = new DocumentaryDashboardService();
        var projection = service.Build(catalog, new DocumentBroker(catalog), features, new DashboardFilter(FeatureId: demo.FeatureId));

        Assert.Equal(30, projection.CompletedTaskCount);
        Assert.Equal(40, projection.TaskCount);
        Assert.Equal(75.0, projection.DeclaredTaskProgress.Percentage);
        Assert.Equal("75%", projection.DeclaredTaskProgress.DisplayPercentage);
        Assert.Equal(12, projection.ExplicitlyLinkedRequirementCount);
        Assert.Equal(20, projection.RequirementCount);
        Assert.Equal(60.0, projection.DocumentaryCoverage.Percentage);
        Assert.Equal("60%", projection.DocumentaryCoverage.DisplayPercentage);

        var emptyProjection = service.Build(catalog, new DocumentBroker(catalog), features, new DashboardFilter(FeatureId: empty.FeatureId));
        Assert.Equal(MetricState.NoItems, emptyProjection.DeclaredTaskProgress.State);
        Assert.Null(emptyProjection.DeclaredTaskProgress.Percentage);
        Assert.Equal("Sem itens", emptyProjection.DeclaredTaskProgress.DisplayPercentage);
        Assert.Equal(MetricState.NoItems, emptyProjection.DocumentaryCoverage.State);
        Assert.Null(emptyProjection.DocumentaryCoverage.Percentage);
        Assert.Equal("Sem itens", emptyProjection.DocumentaryCoverage.DisplayPercentage);
    }

    [Fact]
    public void S02ProjectionExposesRecordedGateStatesWithProvenanceWithoutImplementationScore()
    {
        using var fixture = TempDocumentaryOrigin.Create();
        using var catalog = new DocumentCatalog(Guid.NewGuid(), fixture.Origin);
        var features = catalog.Features.Select(pair => new Feature(pair.Key, catalog.ProjectId, pair.Value, Path.GetFileName(pair.Value))).ToArray();
        var demo = Assert.Single(features, feature => feature.Title == "001-demo");

        var service = new DocumentaryDashboardService();
        var projection = service.Build(catalog, new DocumentBroker(catalog), features, new DashboardFilter(FeatureId: demo.FeatureId));

        var ready = Assert.Single(projection.RecordedGateStates, gate => gate.SourceRelativePath.EndsWith("ready.json", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("READY", ready.RecordedState);
        Assert.Equal("spec", ready.Stage);
        Assert.Contains("não comprova", ready.ProvenanceNote, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(Freshness.Current, ready.Freshness);

        var blocked = Assert.Single(projection.RecordedGateStates, gate => gate.SourceRelativePath.EndsWith("blocked.json", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("BLOCKED", blocked.RecordedState);
        Assert.Contains("Synthetic blocker", blocked.Blockers);
    }

    [Fact]
    public void S03ProjectionReportsMissingLinksDuplicatesBlockersAndUnverifiedCodeReferences()
    {
        using var fixture = TempDocumentaryOrigin.Create(includeDuplicateRequirement: true);
        using var catalog = new DocumentCatalog(Guid.NewGuid(), fixture.Origin);
        var observedReads = new List<string>();
        var broker = new DocumentBroker(catalog)
        {
            AccessObserved = (path, operation, result) =>
            {
                if (operation == "read") observedReads.Add(path);
            }
        };
        var features = catalog.Features.Select(pair => new Feature(pair.Key, catalog.ProjectId, pair.Value, Path.GetFileName(pair.Value))).ToArray();
        var demo = Assert.Single(features, feature => feature.Title == "001-demo");

        var service = new DocumentaryDashboardService();
        var projection = service.Build(catalog, broker, features, new DashboardFilter(FeatureId: demo.FeatureId));

        Assert.Contains(projection.Occurrences, occ => occ.Category == DocumentaryOccurrenceCategory.RecordedBlocker && occ.Title.Contains("Synthetic blocker"));
        Assert.Contains(projection.Occurrences, occ => occ.Category == DocumentaryOccurrenceCategory.RequirementWithoutTask && occ.Title.Contains("FR-013"));
        Assert.Contains(projection.Occurrences, occ => occ.Category == DocumentaryOccurrenceCategory.TaskWithoutRequirement && occ.Title.Contains("T013"));
        Assert.Contains(projection.Occurrences, occ => occ.Category == DocumentaryOccurrenceCategory.DuplicateRequirementId && occ.Title.Contains("FR-001"));
        Assert.Contains(projection.Occurrences, occ => occ.Category == DocumentaryOccurrenceCategory.UnverifiedCodeReference && occ.Title.Contains("DoNotRead.cs"));
        // Duplicate FR-001 stays in denominator (21 total) and is NOT counted as unambiguously linked (11/21).
        Assert.Equal(21, projection.RequirementCount);
        Assert.Equal(11, projection.ExplicitlyLinkedRequirementCount);
        // Canary .cs file is never read by the broker.
        Assert.DoesNotContain(observedReads, path => path.EndsWith("DoNotRead.cs", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void S04ProjectionMarksStaleOnCatalogChangeAndUpdatesWithoutExecutingAgentsOrGates()
    {
        using var fixture = TempDocumentaryOrigin.Create();
        using var catalog = new DocumentCatalog(Guid.NewGuid(), fixture.Origin);
        var features = catalog.Features.Select(pair => new Feature(pair.Key, catalog.ProjectId, pair.Value, Path.GetFileName(pair.Value))).ToArray();
        var demo = Assert.Single(features, feature => feature.Title == "001-demo");
        var service = new DocumentaryDashboardService();

        var initial = service.Build(catalog, new DocumentBroker(catalog), features, new DashboardFilter(FeatureId: demo.FeatureId));
        Assert.Equal(Freshness.Current, initial.Freshness);
        Assert.Equal(30, initial.CompletedTaskCount);

        var tasksPath = Path.Combine(fixture.Origin, "specs", "001-demo", "tasks.md");
        var updated = File.ReadAllText(tasksPath).Replace("- [ ] T031 ", "- [x] T031 ", StringComparison.Ordinal);
        File.WriteAllText(tasksPath, updated);
        catalog.MarkChanged();

        var stale = service.Build(catalog, new DocumentBroker(catalog), features, new DashboardFilter(FeatureId: demo.FeatureId));
        Assert.Equal(Freshness.Stale, stale.Freshness);

        catalog.Refresh();
        var refreshed = service.Build(catalog, new DocumentBroker(catalog), features, new DashboardFilter(FeatureId: demo.FeatureId));
        Assert.Equal(Freshness.Current, refreshed.Freshness);
        Assert.Equal(31, refreshed.CompletedTaskCount);
        Assert.Equal(updated, File.ReadAllText(tasksPath));
    }

    private sealed class TempDocumentaryOrigin : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "CemgSpec-T030-" + Guid.NewGuid().ToString("N"));
        public string Origin => Path.Combine(Root, "origin");

        public static TempDocumentaryOrigin Create(bool includeDuplicateRequirement = false)
        {
            var f = new TempDocumentaryOrigin();
            var requirements = Enumerable.Range(1, 20).Select(i => $"- FR-{i:D3}: Documentary requirement {i}.").ToList();
            if (includeDuplicateRequirement)
                requirements.Add("- FR-001: Duplicate requirement definition.");
            var tasks = Enumerable.Range(1, 40).Select(i => $"- [{(i <= 30 ? "x" : " ")}] T{i:D3} Demo task {i} {(i <= 12 ? $"FR-{i:D3}" : "unlinked")}");
            f.Write("specs/001-demo/spec.md", "# Demo feature\n" + string.Join('\n', requirements));
            f.Write("specs/001-demo/plan.md", "# Demo plan");
            f.Write("specs/001-demo/tasks.md", "# Tasks\n## Fase 1\n" + string.Join('\n', tasks));
            f.Write("specs/003-empty/spec.md", "# Empty feature\n");
            f.Write("specs/003-empty/tasks.md", "# Tasks\n");
            f.Write("DoNotRead.cs", "// synthetic canary; never read");
            f.Write("specs/001-demo/verification/ready.json", "{\"stage\":\"spec\",\"state\":\"READY\",\"revision\":\"sha256:abc\",\"blockers\":[]}");
            f.Write("specs/001-demo/verification/blocked.json", "{\"stage\":\"spec\",\"state\":\"BLOCKED\",\"revision\":\"sha256:abc\",\"blockers\":[\"Synthetic blocker\"]}");
            f.Write("specs/001-demo/verification/code-reference.json", "{\"stage\":\"spec\",\"state\":\"READY\",\"revision\":\"sha256:abc\",\"evidence\":[{\"path\":\"../../DoNotRead.cs\"}]}");
            return f;
        }

        private void Write(string relative, string content)
        {
            var path = Path.Combine(Origin, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
        }
    }
}
