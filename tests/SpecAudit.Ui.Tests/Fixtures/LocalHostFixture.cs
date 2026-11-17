using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
namespace SpecAudit.Ui.Tests.Fixtures;
public sealed class LocalHostFixture : IAsyncLifetime
{
    private Process? process;
    public Uri Address { get; private set; } = null!;
    public string Root { get; private set; } = "";
    public string Output { get; private set; } = "";
    public async Task InitializeAsync()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while(current is not null && !File.Exists(Path.Combine(current.FullName,"SpecAudit.slnx"))) current=current.Parent;
        Root=current?.FullName ?? throw new InvalidOperationException("Development checkout not found.");
        using(var listener=new TcpListener(IPAddress.Loopback,0)) { listener.Start(); Address=new Uri($"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}"); }
        var start=new ProcessStartInfo("dotnet") { WorkingDirectory=Path.Combine(Root,"SpecAudit"),UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true };
        start.ArgumentList.Add(Path.Combine(Root,"SpecAudit","bin","Debug","net10.0","SpecAudit.dll"));
        start.ArgumentList.Add("--urls");start.ArgumentList.Add(Address.ToString());
        start.Environment["ASPNETCORE_ENVIRONMENT"]="Development";
        start.Environment["DOTNET_NOLOGO"]="1";
        process=new Process{StartInfo=start};
        process.OutputDataReceived+=(_,e)=>{if(e.Data is not null) Output+=e.Data+"\n";};
        process.ErrorDataReceived+=(_,e)=>{if(e.Data is not null) Output+=e.Data+"\n";};
        process.Start();process.BeginOutputReadLine();process.BeginErrorReadLine();
        using var probe=new HttpClient{Timeout=TimeSpan.FromSeconds(1)};
        for(var attempt=0;attempt<100;attempt++)
        {
            if(process.HasExited) throw new InvalidOperationException("Host startup failed: "+Output);
            try { using var response=await probe.GetAsync(Address); return; }
            catch(HttpRequestException){} catch(TaskCanceledException){}
            await Task.Delay(100);
        }
        await DisposeAsync();throw new TimeoutException("Own loopback host failed to start.");
    }
    public HttpClient CreateClient(bool cookies=true) => new(new HttpClientHandler{UseCookies=cookies,AllowAutoRedirect=false}) { BaseAddress=Address,Timeout=TimeSpan.FromSeconds(10) };
    public async Task DisposeAsync()
    {
        if(process is null)return;
        if(!process.HasExited){process.Kill(entireProcessTree:true);await process.WaitForExitAsync();}
        process.Dispose();process=null;
    }
}
