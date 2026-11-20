using SpecAudit.Domain;
using SpecAudit.Infrastructure.Agents;

namespace SpecAudit.Application;

public sealed class AgentCompatibilityService(
    ProjectService projectService,
    CapabilityRegistry capabilityRegistry,
    AgentDiscovery agentDiscovery,
    VerificationService verificationService)
{
    private readonly Dictionary<Guid, List<AgentInstallation>> installationsByProject = [];

    public event Action? Changed;

    public IReadOnlyList<AgentInstallation> GetInstallations(Guid projectId, bool forceRefresh = false)
    {
        var context = projectService.RequireProject(projectId);
        if (forceRefresh || !installationsByProject.TryGetValue(projectId, out var list))
        {
            list = agentDiscovery.DiscoverAll(context.Catalog.FinalOriginPath).ToList();
            installationsByProject[projectId] = list;
        }

        foreach (var inst in list)
            RefreshCompatibilityState(inst, context.Catalog.FinalOriginPath);

        return list;
    }

    public AgentInstallation? GetSelectedAgent(Guid projectId)
    {
        var context = projectService.RequireProject(projectId);
        var list = GetInstallations(projectId);
        if (context.Session.SelectedAgentId is not { } selectedId)
            return null;
        return list.FirstOrDefault(i => i.AgentId == selectedId);
    }

    public void SelectAgent(Guid projectId, Guid? agentId)
    {
        var context = projectService.RequireProject(projectId);
        if (agentId is { } id)
        {
            var list = GetInstallations(projectId);
            if (list.All(i => i.AgentId != id))
                throw new KeyNotFoundException("AgentId does not belong to the project's discovered installations.");
        }
        context.Session.SelectAgent(agentId);
        Changed?.Invoke();
    }

    public AgentInstallation LocateManually(Guid projectId, AgentProvider provider, string executablePath)
    {
        var context = projectService.RequireProject(projectId);
        var list = GetInstallations(projectId).ToList();
        var existingIdx = list.FindIndex(i => i.Provider == provider);
        if (existingIdx >= 0 && list[existingIdx].IsFound && list[existingIdx].DiscoveryMethod == AgentDiscoveryMethod.Automatic)
        {
            throw new InvalidOperationException(
                $"Localização manual não permitida: {list[existingIdx].DisplayName} já foi descoberto automaticamente.");
        }

        var located = agentDiscovery.LocateManually(provider, executablePath, context.Catalog.FinalOriginPath);
        RefreshCompatibilityState(located, context.Catalog.FinalOriginPath);

        if (existingIdx >= 0)
            list[existingIdx] = located;
        else
            list.Add(located);

        installationsByProject[projectId] = list;
        context.Session.SelectAgent(located.AgentId);
        Changed?.Invoke();
        return located;
    }

    public AvailabilityDecision EvaluateSelectedAgentAvailability(Guid projectId)
    {
        var context = projectService.RequireProject(projectId);
        var selected = GetSelectedAgent(projectId);
        if (selected is null)
        {
            return new(
                CapabilityAvailability.PendingValidation,
                "Selecione um agente compatível para habilitar a execução de speckit-analyze.",
                ProtectedCapability.AnalyzeCodex);
        }

        RefreshCompatibilityState(selected, context.Catalog.FinalOriginPath);
        return EvaluateAgentAvailability(selected);
    }

    public AvailabilityDecision EvaluateAgentAvailability(AgentInstallation installation)
    {
        ArgumentNullException.ThrowIfNull(installation);

        if (installation.Provider is AgentProvider.ClaudeCode or AgentProvider.Kiro)
        {
            return new(
                CapabilityAvailability.Unsupported,
                $"{installation.DisplayName} permanece Unsupported para execução na V1 (suportados após prova: Codex e Antigravity).",
                ProtectedCapability.AnalyzeCodex);
        }

        var capability = installation.Provider == AgentProvider.Codex
            ? ProtectedCapability.AnalyzeCodex
            : ProtectedCapability.AnalyzeAntigravity;

        if (!installation.IsFound)
        {
            return new(
                CapabilityAvailability.Unavailable,
                $"{installation.DisplayName} não encontrado nos caminhos padrão; utilize a localização manual fora da origem.",
                capability);
        }

        if (installation.CompatibilityState == AgentCompatibilityState.Stale)
        {
            return new(
                CapabilityAvailability.PendingValidation,
                installation.StatusReason,
                capability);
        }

        if (installation.CompatibilityState != AgentCompatibilityState.Compatible)
        {
            return new(
                CapabilityAvailability.PendingValidation,
                installation.StatusReason,
                capability);
        }

        return verificationService.EvaluateAvailability(
            VerificationType.AnalyzeDocuments,
            capability,
            installation.Version ?? "",
            CodexAnalyzeAdapter.PlatformFingerprint,
            CodexAnalyzeAdapter.PolicyVersion,
            runtimeAvailable: true);
    }

    public void InvalidateProviderSeal(AgentProvider provider)
    {
        if (provider == AgentProvider.Codex)
            capabilityRegistry.Invalidate(ProtectedCapability.AnalyzeCodex);
        else if (provider == AgentProvider.Antigravity)
            capabilityRegistry.Invalidate(ProtectedCapability.AnalyzeAntigravity);

        foreach (var list in installationsByProject.Values)
        {
            foreach (var inst in list.Where(i => i.Provider == provider && i.IsFound))
                inst.MarkStale($"Selo de proteção de {inst.DisplayName} invalidado ou versão/política alterada.");
        }
        Changed?.Invoke();
    }

    private void RefreshCompatibilityState(AgentInstallation inst, string originPath)
    {
        if (inst.Provider is AgentProvider.ClaudeCode or AgentProvider.Kiro)
            return;
        if (!inst.IsFound)
            return;

        if (!agentDiscovery.VerifyUnchanged(inst, originPath))
            return;

        if (inst.Provider == AgentProvider.Codex)
        {
            if (!string.Equals(inst.Version, CodexAnalyzeAdapter.VerifiedCodexVersion, StringComparison.Ordinal) ||
                !string.Equals(inst.ExecutableSha256, CodexAnalyzeAdapter.VerifiedCodexSha256, StringComparison.Ordinal))
            {
                inst.BeginVerification($"Versão/binário do Codex ({inst.Version}) diverge da versão provada ({CodexAnalyzeAdapter.VerifiedCodexVersion}).");
                return;
            }

            var avail = verificationService.EvaluateAvailability(
                VerificationType.AnalyzeDocuments,
                ProtectedCapability.AnalyzeCodex,
                inst.Version ?? "",
                CodexAnalyzeAdapter.PlatformFingerprint,
                CodexAnalyzeAdapter.PolicyVersion);

            if (avail.Availability == CapabilityAvailability.Available)
            {
                if (inst.CompatibilityState is AgentCompatibilityState.Discovered or AgentCompatibilityState.NotVerified)
                {
                    inst.BeginVerification();
                    inst.MarkCompatible($"Compatibilidade comprovada pelo selo AnalyzeCodex (Codex {inst.Version}, somente leitura documental).");
                }
            }
            else
            {
                if (inst.CompatibilityState == AgentCompatibilityState.Compatible)
                    inst.MarkStale(avail.Reason);
                else
                    inst.BeginVerification(avail.Reason);
            }
        }
        else if (inst.Provider == AgentProvider.Antigravity)
        {
            var avail = verificationService.EvaluateAvailability(
                VerificationType.AnalyzeDocuments,
                ProtectedCapability.AnalyzeAntigravity,
                inst.Version ?? "",
                CodexAnalyzeAdapter.PlatformFingerprint,
                CodexAnalyzeAdapter.PolicyVersion);

            if (avail.Availability != CapabilityAvailability.Available)
            {
                if (inst.CompatibilityState == AgentCompatibilityState.Compatible)
                    inst.MarkStale(avail.Reason);
                else
                    inst.BeginVerification("Antigravity descoberto; aguarda prova real individual AnalyzeAntigravity antes de habilitar.");
            }
        }
    }
}
