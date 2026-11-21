using System.Reflection;
using SpecAudit.Domain;

namespace SpecAudit.Tests;

public sealed class BusinessDashboardServiceTests
{
    private static readonly Assembly ProductAssembly = typeof(ProjectSession).Assembly;

    [Fact]
    public void FR002AndFR003BusinessDashboardContractsExistAndExposeRequiredProjections()
    {
        var rowType = RequireType("SpecAudit.Domain.UserStoryStatusRow");
        var deltaType = RequireType("SpecAudit.Domain.EvolutionDelta");
        var projectionType = RequireType("SpecAudit.Domain.BusinessDashboardProjection");
        var serviceType = RequireType("SpecAudit.Application.BusinessDashboardService");

        Assert.NotNull(rowType);
        Assert.NotNull(deltaType);
        Assert.NotNull(projectionType);
        Assert.NotNull(serviceType);
    }

    [Fact]
    public void FR006ExecutiveSummarySerializerContractExists()
    {
        var serializerType = RequireType("SpecAudit.Infrastructure.Reports.ExecutiveSummarySerializer");
        Assert.NotNull(serializerType);
    }

    private static Type RequireType(string fullName)
    {
        var type = ProductAssembly.GetType(fullName, throwOnError: false);
        Assert.True(type is not null, $"Expected contract type '{fullName}' to be implemented in 002 T002-T008.");
        return type!;
    }
}
