using Microsoft.AspNetCore.Http;
using SpecAudit.Application;
using SpecAudit.Infrastructure.Projects;

namespace SpecAudit.Tests;

public sealed class RecentProjectStoreTests
{
    private sealed class UnusedPicker : IFolderPicker
    {
        public Task<string?> PickAsync(string title) => Task.FromResult<string?>(null);
    }

    [Fact]
    public void PersistsDeduplicatesUpdatesDestinationReopensAndRemovesRecentProjects()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "SpecAudit-Recent-" + Guid.NewGuid().ToString("N"));
        var origin = Path.Combine(tempRoot, "origin");
        var output1 = Path.Combine(tempRoot, "output1");
        var output2 = Path.Combine(tempRoot, "output2");
        var storeFile = Path.Combine(tempRoot, "store", "recent-projects.json");
        Directory.CreateDirectory(origin);
        Directory.CreateDirectory(output1);
        Directory.CreateDirectory(output2);
        File.WriteAllText(Path.Combine(origin, "README.md"), "# Recent Test\n");

        try
        {
            var store = new RecentProjectStore(storeFile);
            using (var service = CreateProjectService(store))
            {
                var ctx = service.OpenSelected(origin, output1);
                Assert.Single(service.RecentProjects);
                Assert.Equal(Path.GetFullPath(origin).TrimEnd(Path.DirectorySeparatorChar), service.RecentProjects[0].OriginPath);
                Assert.Equal(Path.GetFullPath(output1).TrimEnd(Path.DirectorySeparatorChar), service.RecentProjects[0].OutputPath);

                // Updating output updates the persisted recent entry without duplicating
                service.ChangeOutputSelected(ctx.Session.ProjectId, output2);
                Assert.Single(service.RecentProjects);
                Assert.Equal(Path.GetFullPath(output2).TrimEnd(Path.DirectorySeparatorChar), service.RecentProjects[0].OutputPath);
            }

            // Reload store from disk in a fresh service instance and reopen with 1 click
            var reloadedStore = new RecentProjectStore(storeFile);
            using (var freshService = CreateProjectService(reloadedStore))
            {
                Assert.Single(freshService.RecentProjects);
                var entry = freshService.RecentProjects[0];

                var reopened = freshService.ReopenRecent(entry.Id);
                Assert.Equal(entry.OutputPath, reopened.OutputDisplayPath.TrimEnd(Path.DirectorySeparatorChar));

                Assert.True(freshService.RemoveRecent(entry.Id));
                Assert.Empty(freshService.RecentProjects);
            }
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    private static ProjectService CreateProjectService(RecentProjectStore store)
    {
        var registry = new LocalSessionRegistry();
        var issued = registry.Issue();
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        accessor.HttpContext.Items[typeof(LocalSessionService)] = issued.Id;
        var session = new LocalSessionService(accessor, registry);
        return new ProjectService(session, new UnusedPicker(), store);
    }
}
