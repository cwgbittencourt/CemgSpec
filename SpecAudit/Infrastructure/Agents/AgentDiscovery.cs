using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using SpecAudit.Domain;
using SpecAudit.Infrastructure.Documents;

namespace SpecAudit.Infrastructure.Agents;

public sealed record AgentCandidatePaths(
    IReadOnlyList<string> ExecutableCandidates,
    IReadOnlyList<string> VersionFiles,
    IReadOnlyList<string> ExternalStateLocations);

public sealed class AgentDiscovery
{
    private readonly IReadOnlyDictionary<AgentProvider, AgentCandidatePaths>? customPaths;
    private readonly Dictionary<AgentProvider, AgentInstallation> discoveredByProvider = [];

    public AgentDiscovery(IReadOnlyDictionary<AgentProvider, AgentCandidatePaths>? customPaths = null)
    {
        this.customPaths = customPaths;
    }

    public IReadOnlyList<AgentInstallation> DiscoverAll(string? auditedOriginPath = null)
    {
        string? finalOrigin = ResolveFinalOrigin(auditedOriginPath);
        var results = new List<AgentInstallation>(4);

        foreach (var provider in new[]
        {
            AgentProvider.Codex,
            AgentProvider.Antigravity,
            AgentProvider.ClaudeCode,
            AgentProvider.Kiro
        })
        {
            var installation = DiscoverProvider(provider, finalOrigin);
            discoveredByProvider[provider] = installation;
            results.Add(installation);
        }

        return results;
    }

    public AgentInstallation LocateManually(
        AgentProvider provider,
        string candidateExecutablePath,
        string? auditedOriginPath = null)
    {
        if (!Enum.IsDefined(provider))
            throw new ArgumentOutOfRangeException(nameof(provider));
        ArgumentException.ThrowIfNullOrWhiteSpace(candidateExecutablePath);

        if (discoveredByProvider.TryGetValue(provider, out var existing) &&
            existing.IsFound &&
            existing.DiscoveryMethod == AgentDiscoveryMethod.Automatic)
        {
            throw new InvalidOperationException(
                $"Localização manual não permitida: {existing.DisplayName} já foi descoberto automaticamente.");
        }

        if (!Path.IsPathFullyQualified(candidateExecutablePath))
            throw new ArgumentException("O caminho manual do executável deve ser absoluto.", nameof(candidateExecutablePath));

        var fullPath = Path.GetFullPath(candidateExecutablePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Executável informado manualmente não foi encontrado.", fullPath);

        var ext = Path.GetExtension(fullPath);
        if (!string.Equals(ext, ".exe", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Localização manual aceita apenas executáveis nativos (.exe) verificáveis por handle.");

        var finalOrigin = ResolveFinalOrigin(auditedOriginPath);
        using var handle = WindowsReadHandle.Open(fullPath, directory: false);
        var resolvedPath = WindowsReadHandle.FinalPath(handle);
        if (finalOrigin is not null &&
            (string.Equals(finalOrigin, resolvedPath, StringComparison.OrdinalIgnoreCase) ||
             WindowsReadHandle.Contains(finalOrigin, resolvedPath)))
        {
            throw new InvalidOperationException(
                "Proibido utilizar executável ou ferramenta localizada dentro da origem auditada.");
        }

        var info = WindowsReadHandle.Info(handle);
        var sha256 = ComputeSha256(resolvedPath);
        var identity = $"{info.Identity}|sha256:{sha256}";
        var version = TryReadExeVersion(resolvedPath) ?? "manual-unversioned";
        var paths = GetCandidatePaths(provider);

        var initialState = provider is AgentProvider.ClaudeCode or AgentProvider.Kiro
            ? AgentCompatibilityState.Unsupported
            : AgentCompatibilityState.Discovered;
        var reason = provider is AgentProvider.ClaudeCode or AgentProvider.Kiro
            ? $"{AgentInstallation.FormatProviderName(provider)} localizado manualmente ({version}), porém permanece Unsupported na V1."
            : $"{AgentInstallation.FormatProviderName(provider)} localizado manualmente ({version}); requer prova de proteção Passed antes de habilitar execução.";

        var manualInstallation = new AgentInstallation(
            provider,
            resolvedPath,
            identity,
            sha256,
            version,
            AgentDiscoveryMethod.ManualFallback,
            paths.ExternalStateLocations,
            initialState,
            reason);

        discoveredByProvider[provider] = manualInstallation;
        return manualInstallation;
    }

    public bool VerifyUnchanged(AgentInstallation installation, string? auditedOriginPath = null)
    {
        ArgumentNullException.ThrowIfNull(installation);
        if (!installation.IsFound || string.IsNullOrWhiteSpace(installation.ExecutablePath))
            return false;

        if (!File.Exists(installation.ExecutablePath))
        {
            installation.MarkStale("Executável do agente não está mais disponível no caminho descoberto.");
            return false;
        }

        var finalOrigin = ResolveFinalOrigin(auditedOriginPath);
        using var handle = WindowsReadHandle.Open(installation.ExecutablePath, directory: false);
        var resolvedPath = WindowsReadHandle.FinalPath(handle);
        if (finalOrigin is not null &&
            (string.Equals(finalOrigin, resolvedPath, StringComparison.OrdinalIgnoreCase) ||
             WindowsReadHandle.Contains(finalOrigin, resolvedPath)))
        {
            installation.MarkStale("Executável do agente resolveu para dentro da origem auditada.");
            return false;
        }

        var info = WindowsReadHandle.Info(handle);
        var sha256 = ComputeSha256(resolvedPath);
        var currentIdentity = $"{info.Identity}|sha256:{sha256}";
        if (!string.Equals(currentIdentity, installation.ExecutableIdentity, StringComparison.Ordinal))
        {
            installation.MarkStale("Identidade ou hash SHA-256 do executável do agente foi alterado.");
            return false;
        }

        return true;
    }

    private AgentInstallation DiscoverProvider(AgentProvider provider, string? finalOrigin)
    {
        var paths = GetCandidatePaths(provider);
        foreach (var candidate in paths.ExecutableCandidates)
        {
            if (string.IsNullOrWhiteSpace(candidate) || !Path.IsPathFullyQualified(candidate) || !File.Exists(candidate))
                continue;

            try
            {
                using var handle = WindowsReadHandle.Open(Path.GetFullPath(candidate), directory: false);
                var resolved = WindowsReadHandle.FinalPath(handle);
                if (finalOrigin is not null &&
                    (string.Equals(finalOrigin, resolved, StringComparison.OrdinalIgnoreCase) ||
                     WindowsReadHandle.Contains(finalOrigin, resolved)))
                {
                    // Ignore any binary inside the audited origin.
                    continue;
                }

                var info = WindowsReadHandle.Info(handle);
                var sha256 = ComputeSha256(resolved);
                var identity = $"{info.Identity}|sha256:{sha256}";
                var version = ResolveVersion(paths.VersionFiles, resolved);

                if (provider is AgentProvider.ClaudeCode or AgentProvider.Kiro)
                {
                    return new AgentInstallation(
                        provider,
                        resolved,
                        identity,
                        sha256,
                        version,
                        AgentDiscoveryMethod.Automatic,
                        paths.ExternalStateLocations,
                        AgentCompatibilityState.Unsupported,
                        $"{AgentInstallation.FormatProviderName(provider)} ({version}) descoberto, porém Unsupported para execução na V1.");
                }

                return new AgentInstallation(
                    provider,
                    resolved,
                    identity,
                    sha256,
                    version,
                    AgentDiscoveryMethod.Automatic,
                    paths.ExternalStateLocations,
                    AgentCompatibilityState.Discovered,
                    $"{AgentInstallation.FormatProviderName(provider)} ({version}) descoberto automaticamente; aguarda verificação do selo de proteção antes de habilitar.");
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        var absentState = provider is AgentProvider.ClaudeCode or AgentProvider.Kiro
            ? AgentCompatibilityState.Unsupported
            : AgentCompatibilityState.NotVerified;
        var absentReason = provider is AgentProvider.ClaudeCode or AgentProvider.Kiro
            ? $"{AgentInstallation.FormatProviderName(provider)} não instalado e Unsupported na V1."
            : $"{AgentInstallation.FormatProviderName(provider)} não encontrado nos caminhos padrão; localização manual permitida fora da origem auditada.";

        return new AgentInstallation(
            provider,
            executablePath: null,
            executableIdentity: null,
            executableSha256: null,
            version: null,
            AgentDiscoveryMethod.Automatic,
            paths.ExternalStateLocations,
            absentState,
            absentReason);
    }

    private AgentCandidatePaths GetCandidatePaths(AgentProvider provider)
    {
        if (customPaths is not null && customPaths.TryGetValue(provider, out var overridePaths))
            return overridePaths;

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        return provider switch
        {
            AgentProvider.Codex => new AgentCandidatePaths(
                ExecutableCandidates:
                [
                    Path.Combine(appData, "npm", "node_modules", "@openai", "codex", "node_modules", "@openai", "codex-win32-x64", "vendor", "x86_64-pc-windows-msvc", "bin", "codex.exe"),
                    Path.Combine(appData, "npm", "node_modules", "@openai", "codex-win32-x64", "vendor", "x86_64-pc-windows-msvc", "bin", "codex.exe"),
                    Path.Combine(localAppData, "Programs", "codex", "codex.exe")
                ],
                VersionFiles:
                [
                    Path.Combine(appData, "npm", "node_modules", "@openai", "codex", "package.json")
                ],
                ExternalStateLocations:
                [
                    Path.Combine(userProfile, ".codex")
                ]),

            AgentProvider.Antigravity => new AgentCandidatePaths(
                ExecutableCandidates:
                [
                    Path.Combine(localAppData, "Programs", "antigravity", "Antigravity.exe"),
                    Path.Combine(localAppData, "Programs", "Antigravity IDE", "Antigravity IDE.exe")
                ],
                VersionFiles:
                [
                    Path.Combine(localAppData, "Programs", "antigravity", "version")
                ],
                ExternalStateLocations:
                [
                    Path.Combine(userProfile, ".gemini", "antigravity")
                ]),

            AgentProvider.ClaudeCode => new AgentCandidatePaths(
                ExecutableCandidates:
                [
                    Path.Combine(localAppData, "Programs", "claude", "claude.exe"),
                    Path.Combine(localAppData, "AnthropicClaude", "claude.exe")
                ],
                VersionFiles:
                [
                    Path.Combine(appData, "npm", "node_modules", "@anthropic-ai", "claude-code", "package.json")
                ],
                ExternalStateLocations:
                [
                    Path.Combine(userProfile, ".claude")
                ]),

            AgentProvider.Kiro => new AgentCandidatePaths(
                ExecutableCandidates:
                [
                    Path.Combine(localAppData, "Programs", "Kiro", "Kiro.exe"),
                    Path.Combine(localAppData, "Kiro", "Kiro.exe")
                ],
                VersionFiles: [],
                ExternalStateLocations:
                [
                    Path.Combine(userProfile, ".kiro")
                ]),

            _ => throw new ArgumentOutOfRangeException(nameof(provider))
        };
    }

    private static string ResolveVersion(IReadOnlyList<string> versionFiles, string executablePath)
    {
        foreach (var file in versionFiles)
        {
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file)) continue;
            try
            {
                if (file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    using var doc = JsonDocument.Parse(File.ReadAllBytes(file));
                    if (doc.RootElement.TryGetProperty("version", out var vProp) &&
                        vProp.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrWhiteSpace(vProp.GetString()))
                    {
                        return vProp.GetString()!.Trim();
                    }
                }
                else
                {
                    var text = File.ReadAllText(file).Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                        return text.Split('\r', '\n')[0].Trim();
                }
            }
            catch { }
        }

        return TryReadExeVersion(executablePath) ?? "unknown";
    }

    private static string? TryReadExeVersion(string executablePath)
    {
        try
        {
            var info = FileVersionInfo.GetVersionInfo(executablePath);
            if (!string.IsNullOrWhiteSpace(info.FileVersion))
                return info.FileVersion.Trim();
            if (!string.IsNullOrWhiteSpace(info.ProductVersion))
                return info.ProductVersion.Trim();
        }
        catch { }
        return null;
    }

    private static string? ResolveFinalOrigin(string? auditedOriginPath)
    {
        if (string.IsNullOrWhiteSpace(auditedOriginPath)) return null;
        using var handle = WindowsReadHandle.Open(Path.GetFullPath(auditedOriginPath), directory: true);
        return WindowsReadHandle.FinalPath(handle);
    }

    private static string ComputeSha256(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexStringLower(SHA256.HashData(stream));
    }
}
