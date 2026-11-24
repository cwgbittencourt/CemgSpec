using System.Text;
using System.Text.Json;
using CemgSpec.Domain;

namespace CemgSpec.Infrastructure.Projects;

public sealed class RecentProjectStore
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly object gate = new();
    private readonly string storeFilePath;
    private readonly List<RecentProjectEntry> entries = [];

    public RecentProjectStore(string? customFilePath = null)
    {
        var envOverride = Environment.GetEnvironmentVariable("CEMGSPEC_RECENT_STORE_PATH");
        storeFilePath = !string.IsNullOrWhiteSpace(customFilePath)
            ? customFilePath
            : !string.IsNullOrWhiteSpace(envOverride)
                ? envOverride
                : Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "CemgSpec",
                    "recent-projects.json");

        Load();
    }

    public IReadOnlyList<RecentProjectEntry> List()
    {
        lock (gate)
        {
            return entries
                .OrderByDescending(e => e.LastOpenedUtc)
                .ToArray();
        }
    }

    public RecentProjectEntry? GetById(Guid id)
    {
        lock (gate)
        {
            return entries.FirstOrDefault(e => e.Id == id);
        }
    }

    public RecentProjectEntry Upsert(string displayName, string originPath, string outputPath)
    {
        var normalizedOrigin = Path.GetFullPath(originPath).TrimEnd(Path.DirectorySeparatorChar);
        var normalizedOutput = Path.GetFullPath(outputPath).TrimEnd(Path.DirectorySeparatorChar);

        lock (gate)
        {
            var existingIndex = entries.FindIndex(e =>
                string.Equals(e.OriginPath, normalizedOrigin, StringComparison.OrdinalIgnoreCase));

            var updated = new RecentProjectEntry(
                existingIndex >= 0 ? entries[existingIndex].Id : Guid.NewGuid(),
                displayName,
                normalizedOrigin,
                normalizedOutput,
                DateTimeOffset.UtcNow);

            if (existingIndex >= 0)
                entries[existingIndex] = updated;
            else
                entries.Add(updated);

            SaveLocked();
            return updated;
        }
    }

    public bool Remove(Guid id)
    {
        lock (gate)
        {
            var removed = entries.RemoveAll(e => e.Id == id) > 0;
            if (removed)
                SaveLocked();
            return removed;
        }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(storeFilePath)) return;
            var json = File.ReadAllText(storeFilePath, Utf8NoBom);
            var loaded = JsonSerializer.Deserialize<List<RecentProjectEntry>>(json, JsonOptions);
            if (loaded is not null)
                entries.AddRange(loaded);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            // Ignore unreadable recent list; start empty without blocking app startup.
        }
    }

    private void SaveLocked()
    {
        try
        {
            var dir = Path.GetDirectoryName(storeFilePath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var tempPath = storeFilePath + ".tmp";
            var json = JsonSerializer.Serialize(entries, JsonOptions);
            File.WriteAllText(tempPath, json, Utf8NoBom);
            File.Move(tempPath, storeFilePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Never fail project opening if local preference storage is unavailable.
        }
    }
}
