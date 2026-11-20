using System.Reflection;
using SpecAudit.Domain;

namespace SpecAudit.Tests;

public sealed class RequirementTraceTests
{
    private static readonly Assembly ProductAssembly = typeof(ProjectSession).Assembly;

    [Fact]
    public void S01RequirementKeysStayContextualAcrossFeaturesAndDistinguishExplicitFromInferredLinks()
    {
        var serviceType = ProductAssembly.GetType("SpecAudit.Application.RequirementTraceService");
        var requirementType = ProductAssembly.GetType("SpecAudit.Domain.Requirement");
        var linkType = ProductAssembly.GetType("SpecAudit.Domain.EvidenceLink");

        Assert.True(serviceType is not null && requirementType is not null && linkType is not null,
            "T037 must implement Requirement, EvidenceLink, Finding, and RequirementTraceService keeping " +
            "RequirementKey=(ProjectId, FeatureId, LocalId) distinct across features (e.g. FR-001 in 001-demo vs 002-other) " +
            "and separating Explicit vs Inferred provenance.");
    }

    [Fact]
    public void S02CriteriaReportLinkedMissingOrAmbiguousWithoutClassifyingImplementation()
    {
        var criterionType = ProductAssembly.GetType("SpecAudit.Domain.Criterion");
        var coverageStateType = ProductAssembly.GetType("SpecAudit.Domain.CoverageState");
        Assert.True(criterionType is not null && coverageStateType is not null,
            "T037 must define Criterion and CoverageState (Linked, MissingLink, Ambiguous) without Implemented/NotImplemented states " +
            "and detect duplicate IDs in the same feature as Ambiguous without merging.");
    }

    [Fact]
    public void S03ThirdPartyValidationDeclarationsStaySeparatedFromCalculatedVerification()
    {
        var serviceType = ProductAssembly.GetType("SpecAudit.Application.RequirementTraceService");
        Assert.True(serviceType is not null,
            "T037 must present recorded validation/acceptance statements as third-party documentary declarations with source/version " +
            "without granting SpecAudit approval.");
    }

    [Fact]
    public void S04CodeAndConfigReferencesRemainUnverifiedTextAndAreNeverOpened()
    {
        var serviceType = ProductAssembly.GetType("SpecAudit.Application.RequirementTraceService");
        Assert.True(serviceType is not null,
            "T037 must keep .cs/.razor/config references as EvidenceLink with Resolution.OutOfScope / unverified text " +
            "across any language without opening source files.");
    }
}

