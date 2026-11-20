using SpecAudit.Domain;

namespace SpecAudit.Application;

public sealed record AvailabilityDecision(
    CapabilityAvailability Availability,
    string Reason,
    ProtectedCapability Capability);

public sealed record PrepareVerificationResult(
    bool Created,
    CapabilityAvailability Availability,
    string? Reason,
    VerificationRun? Run)
{
    public static PrepareVerificationResult Denied(CapabilityAvailability availability, string reason) =>
        new(false, availability, reason, null);

    public static PrepareVerificationResult Accepted(VerificationRun run) =>
        new(true, CapabilityAvailability.Available, run.Reason, run);
}

public sealed class VerificationService(ProjectService projectService, CapabilityRegistry capabilityRegistry)
{
    private readonly Dictionary<Guid, VerificationRun> runsById = [];
    private readonly Dictionary<Guid, List<Guid>> runIdsByProject = [];
    private readonly Dictionary<VerificationType, string> lockedAdapters = [];

    public event Action? Changed;

    public void SelectVerification(Guid projectId, VerificationType type)
    {
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        var context = projectService.RequireProject(projectId);
        context.Session.SelectVerification(type);
        Changed?.Invoke();
    }

    public VerificationType GetSelectedVerification(Guid projectId) =>
        projectService.RequireProject(projectId).Session.SelectedVerification;

    public bool IsAdapterLocked(VerificationType type) => lockedAdapters.ContainsKey(type);

    public AvailabilityDecision EvaluateAvailability(
        VerificationType type,
        ProtectedCapability capability,
        string version,
        string platform,
        string policy,
        bool runtimeAvailable = true,
        string? runtimeUnavailableReason = null)
    {
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));

        if (lockedAdapters.TryGetValue(type, out var lockReason))
            return new(CapabilityAvailability.Unavailable, lockReason, capability);

        if (!runtimeAvailable)
        {
            var reason = string.IsNullOrWhiteSpace(runtimeUnavailableReason)
                ? "Runtime confiável indisponível no ambiente."
                : runtimeUnavailableReason.Trim();
            return new(CapabilityAvailability.Unavailable, reason, capability);
        }

        var availability = capabilityRegistry.Availability(capability, version, platform, policy);
        var message = availability == CapabilityAvailability.Available
            ? "Capacidade habilitada com selo de proteção válido."
            : $"Selo de proteção {capability} pendente ou inválido para esta versão/política.";
        return new(availability, message, capability);
    }

    public PrepareVerificationResult PrepareGateRun(
        Guid projectId,
        GateCandidate candidate,
        AvailabilityDecision availability,
        RequirementKey? focusRequirement = null)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(availability);

        var context = projectService.RequireProject(projectId);
        if (candidate.ProjectId != projectId)
            throw new InvalidOperationException("GateCandidate does not belong to the requested project.");

        context.Session.RequireFeature(candidate.FeatureId);
        var recordEntry = context.Catalog.Entries.FirstOrDefault(e => e.DocumentId == candidate.RecordDocumentId)
            ?? throw new KeyNotFoundException("RecordDocumentId does not belong to the project catalog.");
        if (recordEntry.FeatureId != candidate.FeatureId)
            throw new InvalidOperationException("RecordDocumentId does not belong to the requested feature.");

        foreach (var evidenceId in candidate.EvidenceDocumentIds)
        {
            var evidenceEntry = context.Catalog.Entries.FirstOrDefault(e => e.DocumentId == evidenceId)
                ?? throw new KeyNotFoundException("EvidenceDocumentId does not belong to the project catalog.");
            if (evidenceEntry.FeatureId != candidate.FeatureId)
                throw new InvalidOperationException("EvidenceDocumentId does not belong to the requested feature.");
        }

        if (focusRequirement is not null &&
            (focusRequirement.ProjectId != projectId || focusRequirement.FeatureId != candidate.FeatureId))
        {
            throw new InvalidOperationException("FocusRequirementKey does not belong to the requested project and feature.");
        }

        // Without Available capability, never create a VerificationRun.
        if (availability.Availability != CapabilityAvailability.Available)
            return PrepareVerificationResult.Denied(availability.Availability, availability.Reason);

        var run = new VerificationRun(
            projectId,
            candidate.FeatureId,
            VerificationType.CheckGate,
            candidate.CatalogGeneration,
            candidateId: candidate.CandidateId,
            agentId: null,
            focusRequirementKey: focusRequirement);

        var snapshotIds = new List<Guid> { candidate.RecordDocumentId };
        snapshotIds.AddRange(candidate.EvidenceDocumentIds);
        run.BeginPreparing(snapshotIds);

        RegisterRun(run);

        if (context.Catalog.HasPendingChanges || context.Catalog.Generation != candidate.CatalogGeneration)
        {
            run.BlockPreparation(
                "Preparação vencida: os documentos do projeto foram alterados antes da execução.",
                ["Atualize o catálogo e confirme novamente o registro e a revisão esperada antes de executar."]);
            run.MarkStale();
        }

        Changed?.Invoke();
        return PrepareVerificationResult.Accepted(run);
    }

    public PrepareVerificationResult PrepareAnalyzeRun(
        Guid projectId,
        Guid featureId,
        AgentInstallation agent,
        AvailabilityDecision availability,
        RequirementKey? focusRequirement = null)
    {
        ArgumentNullException.ThrowIfNull(agent);
        ArgumentNullException.ThrowIfNull(availability);

        var context = projectService.RequireProject(projectId);
        context.Session.RequireFeature(featureId);

        if (focusRequirement is not null &&
            (focusRequirement.ProjectId != projectId || focusRequirement.FeatureId != featureId))
        {
            throw new InvalidOperationException("FocusRequirementKey does not belong to the requested project and feature.");
        }

        // Without Available capability and found installation, never create a VerificationRun.
        if (!agent.IsFound || availability.Availability != CapabilityAvailability.Available)
            return PrepareVerificationResult.Denied(availability.Availability, availability.Reason);

        var constitution = context.Catalog.Entries.FirstOrDefault(e => e.Kind == CatalogKind.Constitution);
        var spec = context.Catalog.Entries.FirstOrDefault(e => e.FeatureId == featureId && e.Kind == CatalogKind.Spec);
        var plan = context.Catalog.Entries.FirstOrDefault(e => e.FeatureId == featureId && e.Kind == CatalogKind.Plan);
        var tasks = context.Catalog.Entries.FirstOrDefault(e => e.FeatureId == featureId && e.Kind == CatalogKind.Tasks);

        var presentIds = new[] { constitution, spec, plan, tasks }
            .Where(e => e is not null)
            .Select(e => e!.DocumentId)
            .ToArray();

        var run = new VerificationRun(
            projectId,
            featureId,
            VerificationType.AnalyzeDocuments,
            context.Catalog.Generation,
            candidateId: null,
            agentId: agent.AgentId,
            focusRequirementKey: focusRequirement);

        run.BeginPreparing(presentIds);
        RegisterRun(run);

        var missing = new List<string>();
        if (constitution is null) missing.Add(".specify/memory/constitution.md");
        if (spec is null) missing.Add("spec.md");
        if (plan is null) missing.Add("plan.md");
        if (tasks is null) missing.Add("tasks.md");

        if (missing.Count > 0)
        {
            run.BlockPreparation(
                $"Pré-requisitos documentais ausentes para speckit-analyze: {string.Join(", ", missing)}.",
                [
                    "O SpecAudit opera em modo somente leitura e nunca cria arquivos de pré-requisito ausentes na origem.",
                    "Crie ou restaure os documentos obrigatórios na solução antes de executar speckit-analyze."
                ]);
        }
        else if (context.Catalog.HasPendingChanges)
        {
            run.BlockPreparation(
                "Preparação vencida: os documentos do projeto foram alterados antes da execução.",
                ["Atualize o catálogo documental antes de iniciar speckit-analyze."]);
            run.MarkStale();
        }

        Changed?.Invoke();
        return PrepareVerificationResult.Accepted(run);
    }

    public VerificationRun RequireRun(Guid projectId, Guid runId)
    {
        projectService.RequireProject(projectId);
        if (!runsById.TryGetValue(runId, out var run) || run.ProjectId != projectId)
            throw new KeyNotFoundException("VerificationRun does not belong to the requested project.");
        return run;
    }

    public void RequestCancel(Guid projectId, Guid runId)
    {
        var run = RequireRun(projectId, runId);
        run.RequestCancel();
        Changed?.Invoke();
    }

    public void CompleteCancel(
        Guid projectId,
        Guid runId,
        bool terminationConfirmed,
        string reason,
        int? exitCode = null,
        string output = "",
        string errorOutput = "")
    {
        var run = RequireRun(projectId, runId);
        run.FinishCancellation(terminationConfirmed, reason, exitCode, output, errorOutput);
        if (!terminationConfirmed)
        {
            lockedAdapters[run.Type] =
                $"Encerramento inconclusivo na execução {run.RunId}: novas execuções de {run.Type} bloqueadas.";
        }
        Changed?.Invoke();
    }

    public void InvalidateProjectRuns(Guid projectId)
    {
        if (!runIdsByProject.TryGetValue(projectId, out var ids)) return;
        var changed = false;
        foreach (var id in ids)
        {
            if (runsById.TryGetValue(id, out var run) && run.Freshness != Freshness.Stale)
            {
                run.MarkStale();
                changed = true;
            }
        }
        if (changed) Changed?.Invoke();
    }

    public IReadOnlyList<VerificationRun> GetRuns(Guid projectId)
    {
        projectService.RequireProject(projectId);
        if (!runIdsByProject.TryGetValue(projectId, out var ids)) return [];
        return ids.Select(id => runsById[id]).Reverse().ToArray();
    }

    public VerificationRun? GetLatestRun(Guid projectId)
    {
        projectService.RequireProject(projectId);
        if (!runIdsByProject.TryGetValue(projectId, out var ids) || ids.Count == 0) return null;
        return runsById[ids[^1]];
    }

    private void RegisterRun(VerificationRun run)
    {
        runsById[run.RunId] = run;
        if (!runIdsByProject.TryGetValue(run.ProjectId, out var list))
        {
            list = [];
            runIdsByProject[run.ProjectId] = list;
        }
        list.Add(run.RunId);
    }
}

