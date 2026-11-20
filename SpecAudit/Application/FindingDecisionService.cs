using SpecAudit.Domain;

namespace SpecAudit.Application;

public sealed class FindingDecisionService(ProjectService projectService)
{
    private readonly Dictionary<Guid, Dictionary<Guid, HumanDecision>> decisionsByProject = [];

    public event Action? Changed;

    public HumanDecision Classify(
        Guid projectId,
        Finding finding,
        DecisionClassification classification,
        string? reason,
        string? actor)
    {
        projectService.RequireProject(projectId);
        ArgumentNullException.ThrowIfNull(finding);

        var decision = new HumanDecision(
            finding.FindingId,
            classification,
            reason,
            actor,
            finding.ContextFingerprint);

        if (!decisionsByProject.TryGetValue(projectId, out var map))
        {
            map = [];
            decisionsByProject[projectId] = map;
        }
        map[finding.FindingId] = decision;
        Changed?.Invoke();
        return decision;
    }

    public HumanDecision? GetDecision(Guid projectId, Guid findingId)
    {
        projectService.RequireProject(projectId);
        return decisionsByProject.TryGetValue(projectId, out var map) && map.TryGetValue(findingId, out var decision)
            ? decision
            : null;
    }

    public IReadOnlyList<HumanDecision> GetDecisions(Guid projectId)
    {
        projectService.RequireProject(projectId);
        return decisionsByProject.TryGetValue(projectId, out var map)
            ? map.Values.OrderByDescending(d => d.TimestampUtc).ToArray()
            : [];
    }

    public void ReconcileContextChanges(Guid projectId, IReadOnlyList<Finding> currentFindings)
    {
        projectService.RequireProject(projectId);
        if (!decisionsByProject.TryGetValue(projectId, out var map)) return;

        var changed = false;
        foreach (var finding in currentFindings)
        {
            if (map.TryGetValue(finding.FindingId, out var decision))
            {
                if (decision.EvaluateContextFingerprint(finding.ContextFingerprint))
                    changed = true;
            }
        }
        if (changed) Changed?.Invoke();
    }
}
