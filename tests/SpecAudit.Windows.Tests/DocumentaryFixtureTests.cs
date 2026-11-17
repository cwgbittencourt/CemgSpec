using SpecAudit.Windows.Tests.Fixtures;
using System.Text.Json;
namespace SpecAudit.Windows.Tests;
public sealed class DocumentaryFixtureTests
{
    [Fact]
    public void CreatesIndependentProjectsWithKnownCountsAndNegativeGateRecords()
    {
        var first=DocumentaryFixture.Create(); var second=DocumentaryFixture.Create();
        Assert.NotEqual(first.Root,second.Root);
        var tasks=File.ReadAllLines(Path.Combine(first.Feature,"tasks.md"));
        Assert.Equal(40,tasks.Count(l=>l.StartsWith("- [")));
        Assert.Equal(30,tasks.Count(l=>l.StartsWith("- [x]")));
        Assert.Equal(12,tasks.Count(l=>l.Contains(" FR-")));
        Assert.Equal(20,File.ReadAllLines(Path.Combine(first.Feature,"spec.md")).Count(l=>l.StartsWith("- FR-")));
        Assert.Equal(2,Directory.GetDirectories(Path.Combine(first.Origin,"specs")).Length);
        foreach(var variant in new[]{"ready","blocked","bad-hash","wrong-revision","code-reference"})
        { using var record=JsonDocument.Parse(File.ReadAllText(Path.Combine(first.Feature,"verification",variant+".json"))); Assert.Equal("spec",record.RootElement.GetProperty("stage").GetString()); }
        Assert.True(first.IsUnchanged());
        File.AppendAllText(Path.Combine(first.Feature,"spec.md"),"\nExternal user edit");
        Assert.False(first.IsUnchanged()); Assert.True(second.IsUnchanged());
        first.RecordAccess(Path.Combine(first.Origin,"DoNotRead.cs"),"open","denied-test-marker");
        Assert.Contains("denied-test-marker",File.ReadAllText(Path.Combine(first.Evidence,"access.jsonl")));
    }

    [Fact]
    public async Task JunctionSwapCanBeReproducedAtAnExplicitRaceBarrier()
    {
        var fixture=DocumentaryFixture.Create();
        var link=Path.Combine(fixture.Root,"junction");
        FixtureJunction.Create(fixture.Root,link,fixture.Origin);
        var initial=Directory.ResolveLinkTarget(link,true)!.FullName;
        var reader=Task.Run(()=>
        {
            if(!fixture.RaceBarrier.SignalAndWait(TimeSpan.FromSeconds(10)))throw new TimeoutException();
            if(!fixture.RaceBarrier.SignalAndWait(TimeSpan.FromSeconds(10)))throw new TimeoutException();
            return Directory.ResolveLinkTarget(link,true)!.FullName;
        });
        Assert.True(fixture.RaceBarrier.SignalAndWait(TimeSpan.FromSeconds(10)));
        FixtureJunction.Swap(fixture.Root,link,fixture.Output);
        Assert.True(fixture.RaceBarrier.SignalAndWait(TimeSpan.FromSeconds(10)));
        Assert.NotEqual(initial,await reader);
        Assert.Equal(fixture.Output,Directory.ResolveLinkTarget(link,true)!.FullName);
        Assert.True(fixture.IsUnchanged());
        fixture.RecordAccess(link,"resolve-after-swap","changed");
    }
}
