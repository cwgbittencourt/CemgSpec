using CemgSpec.Domain;
using CemgSpec.Infrastructure.Reports;

namespace CemgSpec.Application;

public enum FindingDiffStatus
{
    New,
    Persistent,
    Resolved,
    Inconclusive
}

public sealed record FindingDiffEntry(
    string ContextualKey,
    FindingDiffStatus Status,
    string Category,
    string Explanation,
    string? PreviousFingerprint,
    string? CurrentFingerprint);

public sealed record HistoryDiffResult(
    bool IsComparable,
    string? InconclusiveReason,
    IReadOnlyList<FindingDiffEntry> Entries);

public sealed class SavedHistoryService(ProjectService projectService, SavedHistoryReader reader)
{
    public event Action? Changed;

    public IReadOnlyList<SavedPackageSummary> ListSaved(Guid projectId)
    {
        var context = projectService.RequireProject(projectId);
        return reader.ListPackages(context);
    }

    public long GetTotalDiskBytes(Guid projectId)
        => ListSaved(projectId).Sum(p => p.TotalBytes);

    public DeleteSavedResult DeleteSaved(Guid projectId, Guid savedRecordId)
    {
        var context = projectService.RequireProject(projectId);
        var result = reader.DeletePackage(context, savedRecordId);
        if (result.Deleted)
            Changed?.Invoke();
        return result;
    }

    public HistoryDiffResult Compare(
        SavedPackageSummary baseline,
        IReadOnlyList<Finding> currentFindings,
        bool currentSampleIsComplete = true)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(currentFindings);

        if (!baseline.IsValid || baseline.IsIncomplete || baseline.IsTampered)
        {
            return new(
                IsComparable: false,
                InconclusiveReason: "Pacote salvo de referência está incompleto ou adulterado; comparação indisponível.",
                Entries: []);
        }

        if (!currentSampleIsComplete)
        {
            return new(
                IsComparable: false,
                InconclusiveReason: "Amostra documental atual está incompleta; não é permitido inferir achados como resolvidos.",
                Entries: []);
        }

        var baselineMap = baseline.Findings
            .GroupBy(f => $"{f.Category}|{f.ContextFingerprint}", StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        var currentMap = currentFindings
            .GroupBy(f => $"{f.Category}|{f.ContextFingerprint}", StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        var entries = new List<FindingDiffEntry>();

        foreach (var (key, current) in currentMap)
        {
            if (baselineMap.TryGetValue(key, out var prev))
            {
                entries.Add(new(key, FindingDiffStatus.Persistent, current.Category, current.Explanation, prev.ContextFingerprint, current.ContextFingerprint));
            }
            else
            {
                entries.Add(new(key, FindingDiffStatus.New, current.Category, current.Explanation, null, current.ContextFingerprint));
            }
        }

        foreach (var (key, prev) in baselineMap)
        {
            if (!currentMap.ContainsKey(key))
            {
                entries.Add(new(key, FindingDiffStatus.Resolved, prev.Category, prev.Explanation, prev.ContextFingerprint, null));
            }
        }

        return new(IsComparable: true, InconclusiveReason: null, Entries: entries);
    }
}
