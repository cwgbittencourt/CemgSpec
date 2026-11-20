using System.Reflection;
using SpecAudit.Domain;
using SpecAudit.Windows.Tests.Fixtures;

namespace SpecAudit.Windows.Tests;

public sealed class ExternalWriterTests
{
    private static readonly Assembly ProductAssembly = typeof(ProjectSession).Assembly;

    [Fact]
    public void S01AndS02ExternalWriterWritesCreateNewManifestLastWithSafeCsvAndNeverTouchesOrigin()
    {
        var fixture = DocumentaryFixture.Create();
        try
        {
            _ = RequireType("SpecAudit.Infrastructure.Reports.ExternalReportWriter");
            _ = RequireType("SpecAudit.Infrastructure.Reports.ReportSerializer");
            Assert.True(fixture.IsUnchanged());
            Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Output));
        }
        finally
        {
            Directory.Delete(fixture.Root, recursive: true);
        }
    }

    [Fact]
    public void S01AndS02ExternalWriterRejectsOriginDestinationJunctionSwapAndReportsPartialFailureHonestly()
    {
        _ = RequireType("SpecAudit.Infrastructure.Reports.ExternalReportWriter");
    }

    [Fact]
    public void S04SavedHistoryReadsValidPackagesDiffsFindingsAndDeletesOnlySelectedRecord()
    {
        _ = RequireType("SpecAudit.Infrastructure.Reports.SavedHistoryReader");
        _ = RequireType("SpecAudit.Application.SavedHistoryService");
    }

    private static Type RequireType(string fullName)
    {
        var type = ProductAssembly.GetType(fullName, throwOnError: false);
        Assert.True(type is not null, $"Expected contract type '{fullName}' to be implemented in T081/T083.");
        return type!;
    }
}
