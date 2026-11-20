using SpecAudit.Infrastructure.Gates;
using SpecAudit.Windows.Tests.Fixtures;

namespace SpecAudit.Windows.Tests;

public sealed class GatePackageTests
{
    [Fact]
    public void PackagedCoreAndPolicyMatchInstalledCemg300PresetByteForByte()
    {
        var package = new GatePackage();
        var status = package.VerifyPackage();

        Assert.True(status.IsValid, status.Message);
        Assert.Equal(GatePackage.CoreVersion, status.CoreVersion);
        Assert.Equal(GatePackage.ToolDisplayName, status.ToolIdentifier);
        Assert.Equal(GatePackage.ExpectedCoreSha256, status.CoreSha256);
        Assert.Equal(GatePackage.ExpectedPolicySha256, status.PolicySha256);
        Assert.NotNull(status.CorePath);
        Assert.NotNull(status.PolicyPath);

        // Verify byte-for-byte identity against the installed repository preset (without altering it).
        var repoRoot = FindRepoRoot();
        var presetCore = Path.Combine(repoRoot, ".specify", "presets", "cemg-speckit", "tools", "check_gate.py");
        var presetPolicy = Path.Combine(repoRoot, ".specify", "presets", "cemg-speckit", "policies", "gate-requirements.json");
        Assert.Equal(File.ReadAllBytes(presetCore), File.ReadAllBytes(status.CorePath!));
        Assert.Equal(File.ReadAllBytes(presetPolicy), File.ReadAllBytes(status.PolicyPath!));
    }

    [Fact]
    public void RejectsTamperedCoreOrPolicyAndMissingOrOriginPythonRuntime()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "SpecAudit-GatePkg-" + Guid.NewGuid().ToString("N"));
        var fixture = DocumentaryFixture.Create();
        try
        {
            var realPkg = new GatePackage();
            var realStatus = realPkg.VerifyPackage();
            Assert.True(realStatus.IsValid);

            var tamperedDir = Path.Combine(tempRoot, "pkg");
            Directory.CreateDirectory(Path.Combine(tamperedDir, "Core", "policies"));
            File.Copy(realStatus.CorePath!, Path.Combine(tamperedDir, "Core", "check_gate.py"));
            File.Copy(realStatus.PolicyPath!, Path.Combine(tamperedDir, "Core", "policies", "gate-requirements.json"));

            // Tamper check_gate.py -> rejected.
            File.AppendAllText(Path.Combine(tamperedDir, "Core", "check_gate.py"), "\n# tampered\n");
            var tamperedCore = new GatePackage(tamperedDir).VerifyPackage();
            Assert.False(tamperedCore.IsValid);
            Assert.Equal("CoreIntegrityMismatch", tamperedCore.ErrorCode);

            // Restore check_gate.py and tamper gate-requirements.json -> rejected.
            File.Copy(realStatus.CorePath!, Path.Combine(tamperedDir, "Core", "check_gate.py"), overwrite: true);
            File.AppendAllText(Path.Combine(tamperedDir, "Core", "policies", "gate-requirements.json"), " ");
            var tamperedPolicy = new GatePackage(tamperedDir).VerifyPackage();
            Assert.False(tamperedPolicy.IsValid);
            Assert.Equal("PolicyIntegrityMismatch", tamperedPolicy.ErrorCode);

            // Missing Python reports Unavailable without installing anything.
            var missingPython = new GatePackage(customPythonLocator: () => null).ResolvePythonRuntime(fixture.Origin);
            Assert.False(missingPython.IsAvailable);
            Assert.Equal("PythonNotFound", missingPython.ErrorCode);

            // Python inside audited origin is rejected.
            var fakeOriginPython = Path.Combine(fixture.Origin, "python.exe");
            File.WriteAllText(fakeOriginPython, "not a trusted runtime");
            var insideOrigin = new GatePackage(customPythonLocator: () => fakeOriginPython).ResolvePythonRuntime(fixture.Origin);
            Assert.False(insideOrigin.IsAvailable);
            Assert.Equal("RuntimeInsideOrigin", insideOrigin.ErrorCode);
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
            Directory.Delete(fixture.Root, recursive: true);
        }
    }

    [Fact]
    public void ResolvesVerifiedAbsolutePythonRuntimeOutsideOrigin()
    {
        var fixture = DocumentaryFixture.Create();
        try
        {
            var package = new GatePackage();
            var runtime = package.ResolvePythonRuntime(fixture.Origin);
            Assert.True(runtime.IsAvailable, runtime.Message);
            Assert.True(Path.IsPathFullyQualified(runtime.ExecutablePath!));
            Assert.NotNull(runtime.Sha256);
            Assert.Equal(64, runtime.Sha256!.Length);
            Assert.NotNull(runtime.Version);
            Assert.True(package.VerifyRuntimeUnchanged(runtime, fixture.Origin));
            Assert.True(fixture.IsUnchanged());
        }
        finally
        {
            Directory.Delete(fixture.Root, recursive: true);
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "SpecAudit.slnx")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new DirectoryNotFoundException("SpecAudit.slnx not found.");
    }
}
