namespace CemgSpec.Domain;

public enum AgentProvider
{
    Codex,
    Antigravity,
    ClaudeCode,
    Kiro
}

public enum AgentDiscoveryMethod
{
    Automatic,
    ManualFallback
}

public enum AgentCompatibilityState
{
    Discovered,
    NotVerified,
    Compatible,
    Incompatible,
    Unsupported,
    Stale
}

public sealed class AgentInstallation
{
    public Guid AgentId { get; }
    public AgentProvider Provider { get; }
    public string DisplayName { get; }
    public string? ExecutablePath { get; }
    public string? ExecutableIdentity { get; }
    public string? ExecutableSha256 { get; }
    public string? Version { get; }
    public AgentDiscoveryMethod DiscoveryMethod { get; }
    public IReadOnlyList<string> ExternalStateLocations { get; }
    public AgentCompatibilityState CompatibilityState { get; private set; }
    public string StatusReason { get; private set; }

    public bool IsFound =>
        !string.IsNullOrWhiteSpace(ExecutablePath) &&
        !string.IsNullOrWhiteSpace(ExecutableIdentity);

    public bool CanManualLocate => !IsFound;

    public AgentInstallation(
        AgentProvider provider,
        string? executablePath,
        string? executableIdentity,
        string? executableSha256,
        string? version,
        AgentDiscoveryMethod discoveryMethod,
        IReadOnlyList<string> externalStateLocations,
        AgentCompatibilityState initialState,
        string statusReason,
        Guid? agentId = null)
    {
        if (!Enum.IsDefined(provider)) throw new ArgumentOutOfRangeException(nameof(provider));
        if (!Enum.IsDefined(discoveryMethod)) throw new ArgumentOutOfRangeException(nameof(discoveryMethod));
        if (!Enum.IsDefined(initialState)) throw new ArgumentOutOfRangeException(nameof(initialState));
        ArgumentNullException.ThrowIfNull(externalStateLocations);
        ArgumentException.ThrowIfNullOrWhiteSpace(statusReason);

        if (provider is AgentProvider.ClaudeCode or AgentProvider.Kiro &&
            initialState != AgentCompatibilityState.Unsupported)
        {
            throw new ArgumentException("ClaudeCode and Kiro must remain Unsupported in V1.", nameof(initialState));
        }

        if (initialState == AgentCompatibilityState.Compatible)
        {
            throw new ArgumentException("Discovery cannot directly construct a Compatible AgentInstallation without protection proof.", nameof(initialState));
        }

        AgentId = agentId is { } id && id != Guid.Empty ? id : Guid.NewGuid();
        Provider = provider;
        DisplayName = FormatProviderName(provider);
        ExecutablePath = string.IsNullOrWhiteSpace(executablePath) ? null : executablePath.Trim();
        ExecutableIdentity = string.IsNullOrWhiteSpace(executableIdentity) ? null : executableIdentity.Trim();
        ExecutableSha256 = string.IsNullOrWhiteSpace(executableSha256) ? null : executableSha256.Trim();
        Version = string.IsNullOrWhiteSpace(version) ? null : version.Trim();
        DiscoveryMethod = discoveryMethod;
        ExternalStateLocations = externalStateLocations
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        CompatibilityState = initialState;
        StatusReason = statusReason.Trim();
    }

    public void BeginVerification(string? reason = null)
    {
        if (Provider is AgentProvider.ClaudeCode or AgentProvider.Kiro)
            throw new InvalidOperationException($"{DisplayName} is Unsupported in V1.");
        if (!IsFound)
            throw new InvalidOperationException($"Cannot verify absent installation for {DisplayName}.");

        CompatibilityState = AgentCompatibilityState.NotVerified;
        if (!string.IsNullOrWhiteSpace(reason))
            StatusReason = reason.Trim();
    }

    public void MarkCompatible(string reason)
    {
        if (Provider is AgentProvider.ClaudeCode or AgentProvider.Kiro)
            throw new InvalidOperationException($"{DisplayName} cannot be enabled in V1.");
        if (!IsFound)
            throw new InvalidOperationException($"Absent {DisplayName} installation cannot be marked Compatible.");
        if (CompatibilityState is not (AgentCompatibilityState.Discovered or AgentCompatibilityState.NotVerified))
            throw new InvalidOperationException($"Must transition through Discovered/NotVerified before Compatible (current: {CompatibilityState}).");
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        CompatibilityState = AgentCompatibilityState.Compatible;
        StatusReason = reason.Trim();
    }

    public void MarkIncompatible(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        CompatibilityState = Provider is AgentProvider.ClaudeCode or AgentProvider.Kiro
            ? AgentCompatibilityState.Unsupported
            : AgentCompatibilityState.Incompatible;
        StatusReason = reason.Trim();
    }

    public void MarkStale(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        CompatibilityState = Provider is AgentProvider.ClaudeCode or AgentProvider.Kiro
            ? AgentCompatibilityState.Unsupported
            : AgentCompatibilityState.Stale;
        StatusReason = reason.Trim();
    }

    public static string FormatProviderName(AgentProvider provider) => provider switch
    {
        AgentProvider.Codex => "Codex",
        AgentProvider.Antigravity => "Antigravity",
        AgentProvider.ClaudeCode => "Claude Code",
        AgentProvider.Kiro => "Kiro",
        _ => provider.ToString()
    };
}
