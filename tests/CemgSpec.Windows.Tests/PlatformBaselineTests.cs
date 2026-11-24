namespace CemgSpec.Windows.Tests;
public sealed class PlatformBaselineTests
{
    [Fact]
    public void WindowsBoundarySuiteRunsOnWindowsX64()
    {
        Assert.True(OperatingSystem.IsWindows(), "Boundary tests require real Windows; no simulated pass.");
        Assert.Equal(System.Runtime.InteropServices.Architecture.X64, System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture);
    }
}
