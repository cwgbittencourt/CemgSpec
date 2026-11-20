using SpecAudit.Application;
using SpecAudit.Domain;
using SpecAudit.Infrastructure.Documents;

namespace SpecAudit.Tests;

public sealed class RequirementTraceTests
{
    [Fact]
    public void S01RequirementKeysStayContextualAcrossFeaturesAndDistinguishExplicitFromInferredLinks()
    {
        using var fixture = TempTraceOrigin.Create();
        using var catalog = new DocumentCatalog(Guid.NewGuid(), fixture.Origin);
        var features = catalog.Features.Select(pair => new Feature(pair.Key, catalog.ProjectId, pair.Value, Path.GetFileName(pair.Value))).ToArray();

        var service = new RequirementTraceService();
        var sheets = service.ListSheets(catalog, new DocumentBroker(catalog), features);

        var frDemo = Assert.Single(sheets, sheet => sheet.FeatureTitle == "001-demo" && sheet.Requirement.Key.LocalId == "FR-001");
        var frOther = Assert.Single(sheets, sheet => sheet.FeatureTitle == "002-other" && sheet.Requirement.Key.LocalId == "FR-001");

        Assert.NotEqual(frDemo.Requirement.Key, frOther.Requirement.Key);
        Assert.Contains("Documentary requirement 1", frDemo.Requirement.Description);
        Assert.Contains("Independent identity", frOther.Requirement.Description);

        Assert.Contains(frDemo.Links, link => link.Provenance == LinkProvenance.Explicit && link.TargetText.Contains("T001"));
        Assert.Contains(frDemo.Links, link => link.Provenance == LinkProvenance.Inferred && link.TargetText.Contains("T002"));
    }

    [Fact]
    public void S02CriteriaReportLinkedMissingOrAmbiguousWithoutClassifyingImplementation()
    {
        var names = Enum.GetNames<CoverageState>();
        Assert.Equal(["Linked", "MissingLink", "Ambiguous"], names);

        using var fixture = TempTraceOrigin.Create(includeDuplicate: true);
        using var catalog = new DocumentCatalog(Guid.NewGuid(), fixture.Origin);
        var features = catalog.Features.Select(pair => new Feature(pair.Key, catalog.ProjectId, pair.Value, Path.GetFileName(pair.Value))).ToArray();

        var service = new RequirementTraceService();
        var sheets = service.ListSheets(catalog, new DocumentBroker(catalog), features);

        var duplicates = sheets.Where(sheet => sheet.FeatureTitle == "001-demo" && sheet.Requirement.Key.LocalId == "FR-001").ToArray();
        Assert.Equal(2, duplicates.Length);
        Assert.All(duplicates, dup =>
        {
            Assert.True(dup.Requirement.HasDuplicateDefinition);
            Assert.All(dup.Requirement.Criteria, c => Assert.Equal(CoverageState.Ambiguous, c.CoverageState));
            Assert.Contains(dup.Findings, f => f.Category == "Ambiguity");
        });

        var missing = Assert.Single(sheets, sheet => sheet.FeatureTitle == "001-demo" && sheet.Requirement.Key.LocalId == "FR-002");
        Assert.All(missing.Requirement.Criteria, c => Assert.Equal(CoverageState.MissingLink, c.CoverageState));
        Assert.Contains(missing.Findings, f => f.Explanation.Contains("Evidência documental insuficiente"));
    }

    [Fact]
    public void S03ThirdPartyValidationDeclarationsStaySeparatedFromCalculatedVerification()
    {
        using var fixture = TempTraceOrigin.Create();
        using var catalog = new DocumentCatalog(Guid.NewGuid(), fixture.Origin);
        var features = catalog.Features.Select(pair => new Feature(pair.Key, catalog.ProjectId, pair.Value, Path.GetFileName(pair.Value))).ToArray();

        var service = new RequirementTraceService();
        var sheets = service.ListSheets(catalog, new DocumentBroker(catalog), features);
        var frDemo = Assert.Single(sheets, sheet => sheet.FeatureTitle == "001-demo" && sheet.Requirement.Key.LocalId == "FR-001");

        var decl = Assert.Single(frDemo.RecordedDeclarations, d => d.SourceRelativePath.EndsWith("ready.json", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("READY", decl.DeclaredState);
        Assert.Contains("registro de terceiros", decl.ThirdPartyNotice, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("não aprova", decl.ThirdPartyNotice, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void S04CodeAndConfigReferencesRemainUnverifiedTextAndAreNeverOpened()
    {
        using var fixture = TempTraceOrigin.Create();
        using var catalog = new DocumentCatalog(Guid.NewGuid(), fixture.Origin);
        var reads = new List<string>();
        var broker = new DocumentBroker(catalog)
        {
            AccessObserved = (path, op, _) => { if (op == "read") reads.Add(path); }
        };
        var features = catalog.Features.Select(pair => new Feature(pair.Key, catalog.ProjectId, pair.Value, Path.GetFileName(pair.Value))).ToArray();

        var service = new RequirementTraceService();
        var sheets = service.ListSheets(catalog, broker, features);
        var frDemo = Assert.Single(sheets, sheet => sheet.FeatureTitle == "001-demo" && sheet.Requirement.Key.LocalId == "FR-001");

        Assert.Contains(frDemo.Links, link =>
            link.Resolution == LinkResolution.OutOfScope &&
            link.TargetText.Contains("DoNotRead.cs", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(frDemo.Limitations, l => l.Contains("capacidade futura", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(reads, path => path.EndsWith("DoNotRead.cs", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class TempTraceOrigin : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "SpecAudit-T037-" + Guid.NewGuid().ToString("N"));
        public string Origin => Path.Combine(Root, "origin");

        public static TempTraceOrigin Create(bool includeDuplicate = false)
        {
            var f = new TempTraceOrigin();
            var spec = "### US1 — História de exemplo\n- FR-001: Documentary requirement 1 (US1).\n- FR-002: Unlinked requirement.\n";
            if (includeDuplicate)
                spec += "- FR-001: Duplicate FR-001 in same feature.\n";
            f.Write("specs/001-demo/spec.md", spec);
            f.Write("specs/001-demo/plan.md", "# Plan\n- FR-001: Referência ao plano e ../../DoNotRead.cs\n");
            f.Write("specs/001-demo/tasks.md", "# Tasks\n- [x] T001 [US1] Explicit FR-001\n- [ ] T002 [US1] Inferred by story only\n");
            f.Write("specs/001-demo/verification/ready.json", "{\"stage\":\"spec\",\"state\":\"READY\",\"revision\":\"sha256:abc\"}");
            f.Write("specs/002-other/spec.md", "# Other\n- FR-001: Independent identity.\n");
            f.Write("specs/002-other/tasks.md", "- [ ] T001 FR-001\n");
            f.Write("DoNotRead.cs", "// synthetic canary; never read");
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
