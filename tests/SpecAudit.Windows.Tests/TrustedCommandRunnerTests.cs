using System.Diagnostics;
using System.Security.Cryptography;
using SpecAudit.Domain;
using SpecAudit.Infrastructure.Execution;
using SpecAudit.Windows.Tests.Fixtures;
namespace SpecAudit.Windows.Tests;
public sealed class TrustedCommandRunnerTests
{
    private static TrustedCommandDefinition Definition(string mode,params string[] args)
    {
        var root=new DirectoryInfo(AppContext.BaseDirectory);
        while(root is not null && !File.Exists(Path.Combine(root.FullName,"SpecAudit.slnx")))root=root.Parent;
        var executable=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"dotnet","dotnet.exe");
        var dll=Path.Combine(root!.FullName,"tests","SpecAudit.CommandFixture","bin","Debug","net10.0","SpecAudit.CommandFixture.dll");
        return new(VerificationType.AnalyzeDocuments,executable,Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(executable))),new[]{dll,mode}.Concat(args).ToArray());
    }
    [Fact]
    public async Task SendsSelectedInputAndCapturesActualOutputWithoutOriginChanges()
    {
        var fixture=DocumentaryFixture.Create();
        var result=await new TrustedCommandRunner().RunAsync(Definition("--echo"),"synthetic selected documents",fixture.Evidence,fixture.Origin,TimeSpan.FromSeconds(15));
        Assert.Equal(0,result.ExitCode);Assert.Equal("synthetic selected documents",result.Output);Assert.Equal("fixture-stderr",result.Error);
        Assert.False(result.Cancelled);Assert.False(result.LimitExceeded);Assert.True(fixture.IsUnchanged());
    }
    [Fact]
    public async Task ArgumentsAreDataAndUnknownOperationOrUntrustedLocationsAreRejected()
    {
        var fixture=DocumentaryFixture.Create();var runner=new TrustedCommandRunner();
        var definition=Definition("--arguments","text with spaces","& echo unexpected","$(not-a-command)");
        var result=await runner.RunAsync(definition,"",fixture.Evidence,fixture.Origin,TimeSpan.FromSeconds(15));
        Assert.Equal(new[]{"text with spaces","& echo unexpected","$(not-a-command)"},System.Text.Json.JsonSerializer.Deserialize<string[]>(result.Output));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(()=>runner.RunAsync(definition with{Operation=(VerificationType)99},"",fixture.Evidence,fixture.Origin,TimeSpan.FromSeconds(15)));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>runner.RunAsync(definition,"",fixture.Origin,fixture.Origin,TimeSpan.FromSeconds(15)));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>runner.RunAsync(definition with{Sha256=new string('0',64)},"",fixture.Evidence,fixture.Origin,TimeSpan.FromSeconds(15)));
    }
    [Fact]
    public async Task ExcessOutputIsExplicitFailureAndTimeoutTerminatesOwnProcessTree()
    {
        var fixture=DocumentaryFixture.Create();var runner=new TrustedCommandRunner();
        var flood=await runner.RunAsync(Definition("--flood"),"",fixture.Evidence,fixture.Origin,TimeSpan.FromSeconds(15));
        Assert.True(flood.LimitExceeded);Assert.True(flood.RootTerminationConfirmed);Assert.True(flood.Output.Length<=65536);
        var timeout=await runner.RunAsync(Definition("--wait"),"",fixture.Evidence,fixture.Origin,TimeSpan.FromSeconds(2));
        Assert.True(timeout.TimedOut);Assert.True(timeout.RootTerminationConfirmed);
        var childLine=Assert.Single(timeout.Output.Split('\n'),l=>l.StartsWith("CHILD_PID="));var childId=int.Parse(childLine[10..].Trim());
        try{using var child=Process.GetProcessById(childId);Assert.True(child.HasExited);}catch(ArgumentException){}
        Assert.True(fixture.IsUnchanged());
    }
}
