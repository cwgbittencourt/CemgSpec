using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using SpecAudit.Domain;
using SpecAudit.Infrastructure.Documents;
namespace SpecAudit.Infrastructure.Execution;

// Created only by trusted adapters. No UI/agent/document can supply an executable or args.
internal sealed record TrustedCommandDefinition(VerificationType Operation,string Executable,string Sha256,IReadOnlyList<string> Arguments);
internal sealed record CommandExecutionResult(int? ExitCode,string Output,string Error,bool Cancelled,bool TimedOut,bool LimitExceeded,bool RootTerminationConfirmed);
internal sealed class TrustedCommandRunner
{
    public async Task<CommandExecutionResult> RunAsync(TrustedCommandDefinition definition,string input,string workingDirectory,string originDirectory,TimeSpan timeout,CancellationToken cancellation=default)
    {
        if(!Enum.IsDefined(definition.Operation))throw new ArgumentOutOfRangeException(nameof(definition.Operation));
        if(!Path.IsPathFullyQualified(definition.Executable))throw new ArgumentException("Trusted executable must be absolute.");
        if(timeout<=TimeSpan.Zero || timeout>TimeSpan.FromMinutes(5))throw new ArgumentOutOfRangeException(nameof(timeout));
        if(Encoding.UTF8.GetByteCount(input)>4*1024*1024)throw new LimitExceededException("input-bytes");
        using var origin=WindowsReadHandle.Open(Path.GetFullPath(originDirectory),true);
        using var work=WindowsReadHandle.Open(Path.GetFullPath(workingDirectory),true);
        var originPath=WindowsReadHandle.FinalPath(origin);var workPath=WindowsReadHandle.FinalPath(work);
        if(string.Equals(originPath,workPath,StringComparison.OrdinalIgnoreCase) || WindowsReadHandle.Contains(originPath,workPath))throw new InvalidOperationException("Working directory must be external to audited origin.");
        var executable=Path.GetFullPath(definition.Executable);
        if(WindowsReadHandle.Contains(originPath,executable))throw new InvalidOperationException("Never execute a tool from audited origin.");
        using var image=new FileStream(executable,FileMode.Open,FileAccess.Read,FileShare.Read);
        if(Convert.ToHexStringLower(SHA256.HashData(image))!=definition.Sha256)throw new InvalidOperationException("Trusted executable hash mismatch.");
        var start=new ProcessStartInfo(executable){WorkingDirectory=workPath,UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardOutput=true,RedirectStandardError=true};
        foreach(var argument in definition.Arguments)start.ArgumentList.Add(argument);
        start.Environment.Clear();
        foreach(var name in new[]{"SystemRoot","WINDIR","USERPROFILE","APPDATA","LOCALAPPDATA","TEMP","TMP"})
            if(Environment.GetEnvironmentVariable(name) is { } value)start.Environment[name]=value;
        start.Environment["PATH"]=Environment.SystemDirectory;
        start.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"]="1";
        using var deadline=new CancellationTokenSource(timeout);using var limit=new CancellationTokenSource();
        using var lifetime=CancellationTokenSource.CreateLinkedTokenSource(cancellation,deadline.Token,limit.Token);
        using var process=new Process{StartInfo=start};
        cancellation.ThrowIfCancellationRequested();
        if(!process.Start())throw new InvalidOperationException("Could not start trusted adapter.");
        int characters=0;
        async Task<string> ReadBounded(StreamReader reader)
        {
            var collected=new StringBuilder();var buffer=new char[4096];
            while(true)
            {
                int count;
                try{count=await reader.ReadAsync(buffer.AsMemory(),lifetime.Token);}
                catch(OperationCanceledException){return collected.ToString();}
                if(count==0)return collected.ToString();
                var total=Interlocked.Add(ref characters,count);
                var allowed=Math.Max(0,Math.Min(count,65536-(total-count)));
                collected.Append(buffer,0,allowed);
                if(total>65536){await limit.CancelAsync();return collected.ToString();}
            }
        }
        var stdout=ReadBounded(process.StandardOutput);var stderr=ReadBounded(process.StandardError);
        try
        {
            await process.StandardInput.WriteAsync(input.AsMemory(),lifetime.Token);process.StandardInput.Close();
            await process.WaitForExitAsync(lifetime.Token);
            await Task.WhenAll(stdout,stderr).WaitAsync(lifetime.Token);
        }
        catch(OperationCanceledException) when(lifetime.IsCancellationRequested)
        {
            if(!process.HasExited)process.Kill(entireProcessTree:true);
            using var termination=new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await process.WaitForExitAsync(termination.Token);
        }
        catch
        {
            if(!process.HasExited)process.Kill(entireProcessTree:true);
            using var termination=new CancellationTokenSource(TimeSpan.FromSeconds(10));await process.WaitForExitAsync(termination.Token);throw;
        }
        return new(process.HasExited?process.ExitCode:null,await stdout,await stderr,cancellation.IsCancellationRequested,deadline.IsCancellationRequested && !cancellation.IsCancellationRequested,limit.IsCancellationRequested,process.HasExited);
    }
}
