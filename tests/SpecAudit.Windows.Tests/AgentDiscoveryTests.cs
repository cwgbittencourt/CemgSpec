using System.Reflection;
using SpecAudit.Application;
using SpecAudit.Domain;
using SpecAudit.Windows.Tests.Fixtures;

namespace SpecAudit.Windows.Tests;

public sealed class AgentDiscoveryTests
{
    private static readonly Assembly ProductAssembly = typeof(ProjectSession).Assembly;

    [Fact]
    public void S01DiscoversProvidersSeparatesDiscoveryFromCompatibilityAndRestrictsManualFallback()
    {
        var providerType = RequireType("SpecAudit.Domain.AgentProvider");
        var discoveryMethodType = RequireType("SpecAudit.Domain.AgentDiscoveryMethod");
        var compatibilityStateType = RequireType("SpecAudit.Domain.AgentCompatibilityState");
        var installationType = RequireType("SpecAudit.Domain.AgentInstallation");
        var discoveryServiceType = RequireType("SpecAudit.Infrastructure.Agents.AgentDiscovery");

        Assert.NotNull(installationType);
        Assert.NotNull(discoveryServiceType);

        var providers = Enum.GetNames(providerType);
        Assert.Equal(new[] { "Codex", "Antigravity", "ClaudeCode", "Kiro" }, providers);

        var methods = Enum.GetNames(discoveryMethodType);
        Assert.Contains("Automatic", methods);
        Assert.Contains("ManualFallback", methods);

        var states = Enum.GetNames(compatibilityStateType);
        Assert.Contains("Discovered", states);
        Assert.Contains("NotVerified", states);
        Assert.Contains("Compatible", states);
        Assert.Contains("Incompatible", states);
        Assert.Contains("Unsupported", states);
        Assert.Contains("Stale", states);
    }

    [Fact]
    public void S01ProjectSelectionAndCheckGateRemainIndependentFromAgentAvailability()
    {
        var fixture = DocumentaryFixture.Create();
        try
        {
            _ = RequireType("SpecAudit.Infrastructure.Agents.AgentDiscovery");
            Assert.True(fixture.IsUnchanged());
            Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Output));
        }
        finally
        {
            Directory.Delete(fixture.Root, recursive: true);
        }
    }

    private static Type RequireType(string fullName)
    {
        var type = ProductAssembly.GetType(fullName, throwOnError: false);
        Assert.True(type is not null, $"Expected contract type '{fullName}' to be implemented in T064.");
        return type!;
    }
}
