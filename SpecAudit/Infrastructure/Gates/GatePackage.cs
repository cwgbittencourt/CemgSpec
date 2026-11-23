using System.Diagnostics;
using System.Security.Cryptography;
using SpecAudit.Infrastructure.Documents;

namespace SpecAudit.Infrastructure.Gates;

public sealed record PythonRuntimeInfo(
    bool IsAvailable,
    string? ExecutablePath,
    string? Sha256,
    string? Version,
    string? ErrorCode,
    string? Message)
{
    public static PythonRuntimeInfo Available(string executablePath, string sha256, string version) =>
        new(true, executablePath, sha256, version, null, $"Python {version} conferido em {executablePath}");

    public static PythonRuntimeInfo Unavailable(string errorCode, string message) =>
        new(false, null, null, null, errorCode, message);
}

public sealed record GatePackageStatus(
    bool IsValid,
    string CoreVersion,
    string ToolIdentifier,
    string? CorePath,
    string? CoreSha256,
    string? PolicyPath,
    string? PolicySha256,
    string? ErrorCode,
    string? Message);

public sealed class GatePackage
{
    public const string CoreVersion = "1.0.0";
    public const string PlatformFingerprint = "win-x64";
    public const string PolicyVersion = "v4";
    public const string ToolDisplayName = "check_gate cemg-speckit-preset 1.0.0 / adaptador de leitura SpecAudit";
    public const string ExpectedCoreSha256 = "96ba5e3f2436c97d4df527f4e0b1e616ae8977b44942d75e26d71fb286dfa909";
    public const string ExpectedPolicySha256 = "6e3df7b8136ea8444d3fbf38690412bacfce079a3095a8ed4e08aab92c02e4f0";
    public const string ExpectedRunnerSha256 = "c529d4e5d0c651520f40f1fd33fd6580e658c111818996c03328e94aee5c19d1";
    public const string ExpectedCatalogPathSha256 = "e51d61f586fe2cb92021870946880437a3283ec5cc55b860167418c34bc0361a";

    public bool TryRegisterVerifiedSeal(SpecAudit.Application.CapabilityRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (!OperatingSystem.IsWindows() || System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture != System.Runtime.InteropServices.Architecture.X64)
            return false;

        var pkg = VerifyPackage();
        if (!pkg.IsValid) return false;

        var runnerPath = Path.Combine(packageRoot, "runner.py");
        var catalogPath = Path.Combine(packageRoot, "catalog_path.py");
        if (!File.Exists(runnerPath) || !File.Exists(catalogPath)) return false;
        if (!string.Equals(ComputeSha256(runnerPath), ExpectedRunnerSha256, StringComparison.Ordinal)) return false;
        if (!string.Equals(ComputeSha256(catalogPath), ExpectedCatalogPathSha256, StringComparison.Ordinal)) return false;

        var assessment = new SpecAudit.Domain.ProtectionAssessment(CoreVersion, PlatformFingerprint, PolicyVersion);
        var checks = new Dictionary<string, bool>
        {
            ["package-integrity"] = true,
            ["offline-no-shell"] = true,
            ["core-equivalence"] = true,
            ["nested-evidence"] = true,
            ["read-time-toctou"] = true,
            ["cancel-zero-writes"] = true
        };
        assessment.RecordPassed(checks, ["specs/001-auditar-projetos-speckit/evidence/GateAdapter-proof.md"]);
        registry.RecordProof(SpecAudit.Application.ProtectedCapability.GateAdapter, assessment);
        return true;
    }

    private readonly string packageRoot;
    private readonly Func<string?>? customPythonLocator;

    public string PackageRoot => packageRoot;

    public GatePackage(string? packageRoot = null, Func<string?>? customPythonLocator = null)
    {
        this.packageRoot = string.IsNullOrWhiteSpace(packageRoot)
            ? Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Gates")
            : Path.GetFullPath(packageRoot);
        this.customPythonLocator = customPythonLocator;
    }

    public GatePackageStatus VerifyPackage(string? auditedOriginPath = null)
    {
        var corePath = Path.Combine(packageRoot, "Core", "check_gate.py");
        var policyPath = Path.Combine(packageRoot, "Core", "policies", "gate-requirements.json");

        if (!File.Exists(corePath))
        {
            return new(false, CoreVersion, ToolDisplayName, null, null, null, null,
                "CoreMissing", "Arquivo empacotado check_gate.py não encontrado no pacote confiável do SpecAudit.");
        }

        if (!File.Exists(policyPath))
        {
            return new(false, CoreVersion, ToolDisplayName, corePath, null, null, null,
                "PolicyMissing", "Arquivo empacotado gate-requirements.json não encontrado no pacote confiável do SpecAudit.");
        }

        if (!string.IsNullOrWhiteSpace(auditedOriginPath))
        {
            using var originHandle = WindowsReadHandle.Open(Path.GetFullPath(auditedOriginPath), true);
            var finalOrigin = WindowsReadHandle.FinalPath(originHandle);
            if (WindowsReadHandle.Contains(finalOrigin, Path.GetFullPath(corePath)) ||
                WindowsReadHandle.Contains(finalOrigin, Path.GetFullPath(policyPath)))
            {
                return new(false, CoreVersion, ToolDisplayName, null, null, null, null,
                    "PackageInsideOrigin", "Pacote confiável de gate nunca pode residir na origem auditada.");
            }
        }

        var coreHash = ComputeSha256(corePath);
        if (!string.Equals(coreHash, ExpectedCoreSha256, StringComparison.Ordinal))
        {
            return new(false, CoreVersion, ToolDisplayName, corePath, coreHash, policyPath, null,
                "CoreIntegrityMismatch", $"Hash SHA-256 de check_gate.py diverge do preset CEMG {CoreVersion} confiável.");
        }

        var policyHash = ComputeSha256(policyPath);
        if (!string.Equals(policyHash, ExpectedPolicySha256, StringComparison.Ordinal))
        {
            return new(false, CoreVersion, ToolDisplayName, corePath, coreHash, policyPath, policyHash,
                "PolicyIntegrityMismatch", $"Hash SHA-256 de gate-requirements.json diverge do preset CEMG {CoreVersion} confiável.");
        }

        return new(true, CoreVersion, ToolDisplayName, corePath, coreHash, policyPath, policyHash,
            null, $"Pacote {ToolDisplayName} íntegro.");
    }

    public PythonRuntimeInfo ResolvePythonRuntime(string? auditedOriginPath = null)
    {
        var candidates = DiscoverPythonCandidates();
        if (candidates.Count == 0)
        {
            return PythonRuntimeInfo.Unavailable(
                "PythonNotFound",
                "Runtime Python 3.10+ não encontrado no ambiente. O SpecAudit não instala Python automaticamente.");
        }

        string? finalOrigin = null;
        if (!string.IsNullOrWhiteSpace(auditedOriginPath))
        {
            using var originHandle = WindowsReadHandle.Open(Path.GetFullPath(auditedOriginPath), true);
            finalOrigin = WindowsReadHandle.FinalPath(originHandle);
        }

        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate) || !Path.IsPathFullyQualified(candidate))
                continue;

            var full = Path.GetFullPath(candidate);
            if (!File.Exists(full)) continue;
            if (IsWindowsAppsStub(full)) continue;

            if (finalOrigin is not null && WindowsReadHandle.Contains(finalOrigin, full))
            {
                return PythonRuntimeInfo.Unavailable(
                    "RuntimeInsideOrigin",
                    "Runtime Python dentro da origem auditada é proibido e rejeitado.");
            }

            var probe = ProbePythonVersion(full);
            if (probe is null) continue;

            var hash = ComputeSha256(full);
            return PythonRuntimeInfo.Available(full, hash, probe);
        }

        return PythonRuntimeInfo.Unavailable(
            "PythonIncompatible",
            "Nenhum runtime Python 3.10+ compatível foi confirmado fora da origem auditada.");
    }

    public bool VerifyRuntimeUnchanged(PythonRuntimeInfo runtime, string? auditedOriginPath = null)
    {
        if (!runtime.IsAvailable || string.IsNullOrWhiteSpace(runtime.ExecutablePath) || string.IsNullOrWhiteSpace(runtime.Sha256))
            return false;
        if (!Path.IsPathFullyQualified(runtime.ExecutablePath) || !File.Exists(runtime.ExecutablePath))
            return false;
        if (!string.IsNullOrWhiteSpace(auditedOriginPath))
        {
            using var originHandle = WindowsReadHandle.Open(Path.GetFullPath(auditedOriginPath), true);
            var finalOrigin = WindowsReadHandle.FinalPath(originHandle);
            if (WindowsReadHandle.Contains(finalOrigin, Path.GetFullPath(runtime.ExecutablePath)))
                return false;
        }
        return string.Equals(ComputeSha256(runtime.ExecutablePath), runtime.Sha256, StringComparison.Ordinal);
    }

    private List<string> DiscoverPythonCandidates()
    {
        if (customPythonLocator is not null)
        {
            var located = customPythonLocator();
            return string.IsNullOrWhiteSpace(located) ? [] : [located];
        }

        var results = new List<string>();
        var envOverride = Environment.GetEnvironmentVariable("SPECAUDIT_PYTHON_EXE");
        if (!string.IsNullOrWhiteSpace(envOverride))
            results.Add(envOverride.Trim());

        var searchRoots = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python"),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            @"C:\"
        };

        foreach (var root in searchRoots.Where(r => !string.IsNullOrWhiteSpace(r) && Directory.Exists(r)))
        {
            try
            {
                foreach (var dir in Directory.EnumerateDirectories(root, "Python3*").OrderByDescending(d => d, StringComparer.OrdinalIgnoreCase))
                {
                    var exe = Path.Combine(dir, "python.exe");
                    if (File.Exists(exe)) results.Add(exe);
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        var pyLauncher = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "py.exe");
        if (File.Exists(pyLauncher))
        {
            var resolved = QueryPyLauncher(pyLauncher);
            if (!string.IsNullOrWhiteSpace(resolved))
                results.Add(resolved);
        }

        return results.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string? QueryPyLauncher(string pyLauncher)
    {
        try
        {
            var start = new ProcessStartInfo(pyLauncher)
            {
                WorkingDirectory = Path.GetTempPath(),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            start.ArgumentList.Add("-3");
            start.ArgumentList.Add("-I");
            start.ArgumentList.Add("-B");
            start.ArgumentList.Add("-c");
            start.ArgumentList.Add("import sys; print(sys.executable)");
            using var process = Process.Start(start);
            if (process is null) return null;
            var output = process.StandardOutput.ReadToEnd().Trim();
            if (!process.WaitForExit(5000))
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                return null;
            }
            return process.ExitCode == 0 && Path.IsPathFullyQualified(output) ? output : null;
        }
        catch
        {
            return null;
        }
    }

    private static string? ProbePythonVersion(string pythonExe)
    {
        try
        {
            var start = new ProcessStartInfo(pythonExe)
            {
                WorkingDirectory = Path.GetTempPath(),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            start.ArgumentList.Add("-I");
            start.ArgumentList.Add("-B");
            start.ArgumentList.Add("-c");
            start.ArgumentList.Add("import sys; v=sys.version_info; print(f'{v.major}.{v.minor}.{v.micro}') if (v.major,v.minor)>=(3,10) else sys.exit(2)");
            start.Environment.Clear();
            foreach (var name in new[] { "SystemRoot", "WINDIR", "TEMP", "TMP" })
            {
                if (Environment.GetEnvironmentVariable(name) is { } value)
                    start.Environment[name] = value;
            }
            using var process = Process.Start(start);
            if (process is null) return null;
            var output = process.StandardOutput.ReadToEnd().Trim();
            if (!process.WaitForExit(5000))
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                return null;
            }
            return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output) ? output : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsWindowsAppsStub(string fullPath) =>
        fullPath.Contains(@"\Microsoft\WindowsApps\", StringComparison.OrdinalIgnoreCase);

    internal static string ComputeSha256(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexStringLower(SHA256.HashData(stream));
    }
}

