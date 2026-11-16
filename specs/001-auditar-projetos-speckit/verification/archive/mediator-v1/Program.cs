using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using SpecAudit.NetworkProof;

internal static class Program
{
    private static readonly JsonSerializerOptions Json=new(){WriteIndented=true,IncludeFields=true};
    public static int Main(string[] args)
    {
        try
        {
            bool admin=OperatingSystem.IsWindows() && new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            string mode=ExecutionGate.Mode(args,admin);
            if(mode=="self-test"){Console.WriteLine(JsonSerializer.Serialize(SelfTests.Run(),Json));return 0;}
            if(mode=="describe")
            {
                Console.WriteLine("Prototype only. Default/self-test performs no native policy operations. Explicit proof mode requires administrator and acknowledgement. It creates only synthetic TEMP data, transient AppContainers and SID-scoped dynamic WFP filters. No agents, credentials, service installation, global firewall profiles or persistent rules. Hard-crash fail-closed and provider integration remain unproven.");return 0;
            }
            return RunProof();
        }
        catch(Exception error){Console.Error.WriteLine(error.GetType().Name+": "+error.Message);return 2;}
    }
    private static string Hash(string path)=>Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)));
    private static int Drain(TcpListener server){int count=0;while(server.Pending()){using var connection=server.AcceptTcpClient();count++;}return count;}
    private static void Control(TcpListener server,IPAddress ip){using var client=new TcpClient(ip.AddressFamily);client.Connect(ip,((IPEndPoint)server.LocalEndpoint).Port);using var accepted=server.AcceptTcpClient();}
    private static int RunProof()
    {
        if(!OperatingSystem.IsWindows() || !Environment.Is64BitProcess)throw new PlatformNotSupportedException("Windows x64 required.");
        string root=Path.Combine(Path.GetTempPath(),"SpecAudit-MediatorProof-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        string source=Path.Combine(root,"source.txt");File.WriteAllText(source,"synthetic-read-only-source");string before=Hash(source);
        var allowed=new TcpListener(IPAddress.Loopback,0);var denied=new TcpListener(IPAddress.Loopback,0);var denied6=new TcpListener(IPAddress.IPv6Loopback,0);
        allowed.Start();denied.Start();denied6.Start();using var udp=new UdpClient(new IPEndPoint(IPAddress.Loopback,0));
        int allowPort=((IPEndPoint)allowed.LocalEndpoint).Port;int denyPort=((IPEndPoint)denied.LocalEndpoint).Port;int deny6Port=((IPEndPoint)denied6.LocalEndpoint).Port;int udpPort=((IPEndPoint)udp.Client.LocalEndPoint!).Port;
        var lifetime=new PolicyLifetime();using var mediator=new MediatorSession();BoundaryResult? worker=null;bool cleanup=false,controlUnaffected=false,rollbackPassed=false;object? rollbackEvidence=null;int allowedCount=0,deniedCount=0,denied6Count=0,udpBytes=0;Exception? failure=null;
        try
        {
            Control(allowed,IPAddress.Loopback);Control(denied,IPAddress.Loopback);Control(denied6,IPAddress.IPv6Loopback);
            using(var controlUdp=new UdpClient()){controlUdp.Send([0x42],1,new IPEndPoint(IPAddress.Loopback,udpPort));var endpoint=new IPEndPoint(IPAddress.Any,0);udp.Client.ReceiveTimeout=2000;_=udp.Receive(ref endpoint);}
            var rollback=RunRollback(allowPort);rollbackPassed=rollback.Passed;rollbackEvidence=rollback;if(!rollbackPassed)throw new InvalidOperationException("Rollback test failed; normal proof not started.");
            using var stream=File.Open(source,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);
            BoundaryProbe.BrokerReadHandle=stream.SafeFileHandle.DangerousGetHandle();if(!BoundaryProbe.SetBrokerHandleInheritance(true))throw new InvalidOperationException("Cannot inherit read handle.");
            BoundaryProbe.BeforeContainerLaunch=sid=>{mediator.Activate(sid,allowPort);lifetime.Install();}; BoundaryProbe.BeforeResume=()=>{Control(denied,IPAddress.Loopback);controlUnaffected=true;lifetime.Resume();};
            string io=BuildWorker(BoundaryProbe.BrokerReadHandle.ToInt64(),source,allowPort,denyPort,deny6Port,udpPort);
            string child=Convert.ToBase64String(Encoding.Unicode.GetBytes(io.Replace("'root'","'child'")));
            string code=io+"; $p=Start-Process -FilePath '@TRUSTED_PS@' -ArgumentList '-NoProfile','-NonInteractive','-EncodedCommand','"+child+"' -NoNewWindow -PassThru; $p.WaitForExit(); exit $p.ExitCode";
            code=code.Replace("@TRUSTED_PS@",Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),"WindowsPowerShell/v1.0/powershell.exe").Replace("'","''"));
            // Callback installs policy before NativeWorker resumes its suspended low-privilege process.
            worker=BoundaryProbe.RunWithContainer(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),"WindowsPowerShell/v1.0/powershell.exe"),["-NoProfile","-NonInteractive","-EncodedCommand",Convert.ToBase64String(Encoding.Unicode.GetBytes(code))],Environment.GetFolderPath(Environment.SpecialFolder.System),15000,true);
            
            if(worker.ActiveAfterTerminate!=0)throw new InvalidOperationException("Worker job still active: retain policy until operator cleanup.");
            lifetime.Stop();mediator.Dispose();lifetime.Release();cleanup=mediator.VerifyRemoved();
            allowedCount=Drain(allowed);deniedCount=Drain(denied);denied6Count=Drain(denied6);udpBytes=udp.Available;
            // Caller outside the dedicated AppContainer remains unaffected while that policy ran.
            Control(denied,IPAddress.Loopback);
        }
        catch(Exception error){failure=error;}
        finally
        {
            BoundaryProbe.BeforeContainerLaunch=null;BoundaryProbe.BeforeResume=null;_=BoundaryProbe.SetBrokerHandleInheritance(false);BoundaryProbe.BrokerReadHandle=IntPtr.Zero;
            allowed.Stop();denied.Stop();denied6.Stop();
            // NativeWorker closes its job before returning, including on normal exceptions/timeouts.
            if((worker is null || worker.ActiveAfterTerminate==0) && lifetime.State!=PolicyLifetime.Phase.Released){lifetime.Stop();mediator.Dispose();}
        }
        var rows=new List<JsonElement>();if(worker?.Output is not null)foreach(string line in worker.Output.Split('\n')){try{using var parsed=JsonDocument.Parse(line);if(parsed.RootElement.TryGetProperty("Role",out _))rows.Add(parsed.RootElement.Clone());}catch(JsonException){}}
        bool workerAssertions=rows.Count==2 && rows.All(r=>r.GetProperty("ReadViaBroker").GetBoolean()&&r.GetProperty("WriteDenied").GetBoolean()&&r.GetProperty("CreateDenied").GetBoolean()&&r.GetProperty("RenameDenied").GetBoolean()&&r.GetProperty("DeleteDenied").GetBoolean()&&r.GetProperty("AllowedConnected").GetBoolean()&&!r.GetProperty("DeniedConnected").GetBoolean()&&!r.GetProperty("Ipv6Connected").GetBoolean());
        bool passed=rollbackPassed && controlUnaffected && failure is null && worker?.ExitCode==0 && worker.TokenIsAppContainer==1 && worker.AdminGroupEnabled==false && worker.ProfileDeleteResult==0 && workerAssertions && before==Hash(source) && allowedCount==2 && deniedCount==0 && denied6Count==0 && udpBytes==0 && cleanup;
        var report=new {state=passed?"PASSED_SYNTHETIC_SCOPE":"FAILED_OR_INCOMPLETE",timestampUtc=DateTimeOffset.UtcNow,fixture=root,sourceIntact=before==Hash(source),policySublayer=mediator.SublayerKey,filterKeys=mediator.FilterKeys,allowedCount,deniedCount,denied6Count,udpBytes,cleanupVerified=cleanup,controlUnaffectedDuringPolicy=controlUnaffected,workerAssertions,rollbackPassed,rollbackEvidence,worker,rows,error=failure?.ToString(),globalFirewallProfilesChanged=false,persistentFiltersCreated=false,agentsExecuted=false,limits=new[]{"Not a service or production authenticated IPC implementation","No real provider/agent authentication tested","No proof of hard-crash fail-closed ordering","Only synthetic TCP/UDP/IPv6 and orderly worker termination covered"}};
        string output=Path.Combine(root,"proof-result.json");File.WriteAllText(output,JsonSerializer.Serialize(report,Json));Console.WriteLine(JsonSerializer.Serialize(new{report.state,report.sourceIntact,report.cleanupVerified,resultFile=output},Json));return passed?0:1;
    }
    private sealed record RollbackResult(bool Passed,bool CleanupVerified,BoundaryResult Worker);
    private static RollbackResult RunRollback(int port)
    {
        using var policy=new MediatorSession();
        BoundaryProbe.BeforeContainerLaunch=sid=>policy.Activate(sid,port,true);
        BoundaryProbe.BeforeResume=()=>throw new InvalidOperationException("Rollback must prevent resume.");
        BoundaryResult result;
        try {result=BoundaryProbe.RunWithContainer(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),"WindowsPowerShell/v1.0/powershell.exe"),["-NoProfile","-Command","exit 99"],Environment.GetFolderPath(Environment.SpecialFolder.System),3000,true);}
        finally {BoundaryProbe.BeforeContainerLaunch=null;BoundaryProbe.BeforeResume=null;}
        policy.Dispose();bool removed=policy.VerifyRemoved();
        return new(result.ProcessId==0 && result.Error?.Contains("Synthetic transaction rollback")==true && removed && result.ProfileDeleteResult==0,removed,result);
    }
    private static string BuildWorker(long handle,string source,int allowed,int denied,int denied6,int udp)=>"""
    $ProgressPreference='SilentlyContinue';function Tcp([string]$hostName,[int]$port){try{$c=[Net.Sockets.TcpClient]::new();$task=$c.ConnectAsync($hostName,$port);if($task.Wait(1500)){return $c.Connected};return $false}catch{return $false}finally{if($c){$c.Dispose()}}};$r=@{Role='root';ReadViaBroker=$false;WriteDenied=$false;CreateDenied=$false;RenameDenied=$false;DeleteDenied=$false;AllowedConnected=$false;DeniedConnected=$false;Ipv6Connected=$false};$h=[Microsoft.Win32.SafeHandles.SafeFileHandle]::new([IntPtr]@HANDLE@,$false);try{$s=[IO.FileStream]::new($h,[IO.FileAccess]::Read);$s.Position=0;$reader=[IO.StreamReader]::new($s);$r.ReadViaBroker=$reader.ReadToEnd() -eq 'synthetic-read-only-source'}catch{};try{[IO.File]::WriteAllText('@SOURCE@','SHOULD_NOT_WRITE')}catch{$r.WriteDenied=$true};try{[IO.File]::WriteAllText('@SOURCE@.new','SHOULD_NOT_CREATE')}catch{$r.CreateDenied=$true};try{[IO.File]::Move('@SOURCE@','@SOURCE@.renamed')}catch{$r.RenameDenied=$true};try{[IO.File]::Delete('@SOURCE@')}catch{$r.DeleteDenied=$true};$r.AllowedConnected=Tcp '127.0.0.1' @ALLOWED@;$r.DeniedConnected=Tcp '127.0.0.1' @DENIED@;$r.Ipv6Connected=Tcp '::1' @DENIED6@;try{$u=[Net.Sockets.UdpClient]::new();$bytes=[Text.Encoding]::ASCII.GetBytes('SYNTHETIC');$u.Send($bytes,$bytes.Length,'127.0.0.1',@UDP@)|Out-Null}catch{}finally{if($u){$u.Dispose()}};$r|ConvertTo-Json -Compress
    """.Replace("@HANDLE@",handle.ToString()).Replace("@SOURCE@",source.Replace("'","''")).Replace("@ALLOWED@",allowed.ToString()).Replace("@DENIED@",denied.ToString()).Replace("@DENIED6@",denied6.ToString()).Replace("@UDP@",udp.ToString());
}