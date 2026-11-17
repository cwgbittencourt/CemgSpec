namespace SpecAudit.Tests;
public sealed class RuntimeBaselineTests
{
    [Fact]
    public void DevelopmentUsesSupportedStableRuntime()
    {
        Assert.Equal(10, Environment.Version.Major);
        Assert.DoesNotContain("preview", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription, StringComparison.OrdinalIgnoreCase);
    }
}
