using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.AccessControl;
using System.Security.Principal;

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
            if(mode=="diagnose")return DiagnoseWorker();
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
        string stagedWorker=StageWorker(root);
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
            BoundaryProbe.BeforeContainerLaunch=sid=>{GrantWorkerReadExecute(stagedWorker,sid);mediator.Activate(sid,allowPort);lifetime.Install();}; BoundaryProbe.BeforeResume=()=>{Control(denied,IPAddress.Loopback);controlUnaffected=true;lifetime.Resume();};
            // Policy is installed before the suspended native worker is resumed.
            worker=BoundaryProbe.RunWithContainer(stagedWorker,["--prove",BoundaryProbe.BrokerReadHandle.ToInt64().ToString(System.Globalization.CultureInfo.InvariantCulture),source,allowPort.ToString(),denyPort.ToString(),deny6Port.ToString(),udpPort.ToString(),"root"],Environment.GetFolderPath(Environment.SpecialFolder.System),20000,true);
            
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
        bool workerAssertions=rows.Count==2 && rows.Select(r=>r.GetProperty("Role").GetString()).Order().SequenceEqual(new[]{"child","root"}) && rows.All(r=>r.GetProperty("WsaStartupError").GetInt32()==0 && r.GetProperty("IsAppContainer").GetBoolean() && !r.GetProperty("AdminGroupEnabled").GetBoolean() && r.GetProperty("ReadViaBroker").GetBoolean()&&r.GetProperty("WriteDenied").GetBoolean()&&r.GetProperty("CreateDenied").GetBoolean()&&r.GetProperty("RenameDenied").GetBoolean()&&r.GetProperty("DeleteDenied").GetBoolean()&&r.GetProperty("AllowedConnected").GetBoolean()&&!r.GetProperty("DeniedConnected").GetBoolean()&&!r.GetProperty("Ipv6Connected").GetBoolean());
        bool passed=rollbackPassed && controlUnaffected && failure is null && worker?.ExitCode==0 && worker.TokenIsAppContainer==1 && worker.AdminGroupEnabled==false && worker.ProfileDeleteResult==0 && workerAssertions && before==Hash(source) && allowedCount==2 && deniedCount==0 && denied6Count==0 && udpBytes==0 && cleanup;
        var report=new {state=passed?"PASSED_SYNTHETIC_SCOPE":"FAILED_OR_INCOMPLETE",timestampUtc=DateTimeOffset.UtcNow,fixture=root,sourceIntact=before==Hash(source),policySublayer=mediator.SublayerKey,filterKeys=mediator.FilterKeys,allowedCount,deniedCount,denied6Count,udpBytes,cleanupVerified=cleanup,controlUnaffectedDuringPolicy=controlUnaffected,workerAssertions,workerExitStatusHex=worker is null?null:$"0x{unchecked((uint)worker.ExitCode.GetValueOrDefault()):X8}",bootstrapReached=worker?.Output?.Contains("WorkerReady")==true,rollbackPassed,rollbackEvidence,worker,rows,error=failure?.ToString(),globalFirewallProfilesChanged=false,persistentFiltersCreated=false,agentsExecuted=false,limits=new[]{"Not a service or production authenticated IPC implementation","No real provider/agent authentication tested","No proof of hard-crash fail-closed ordering","Only synthetic TCP/UDP/IPv6 and orderly worker termination covered"}};
        string output=Path.Combine(root,"proof-result.json");File.WriteAllText(output,JsonSerializer.Serialize(report,Json));Console.WriteLine(JsonSerializer.Serialize(new{report.state,report.sourceIntact,report.cleanupVerified,resultFile=output},Json));return passed?0:1;
    }
    private sealed record RollbackResult(bool Passed,bool CleanupVerified,BoundaryResult Worker);
    private static RollbackResult RunRollback(int port)
    {
        using var policy=new MediatorSession();
        BoundaryProbe.BeforeContainerLaunch=sid=>policy.Activate(sid,port,true);
        BoundaryProbe.BeforeResume=()=>throw new InvalidOperationException("Rollback must prevent resume.");
        BoundaryResult result;
        try {result=BoundaryProbe.RunWithContainer(WorkerPath(),["--bootstrap-check"],Environment.GetFolderPath(Environment.SpecialFolder.System),3000,true);}
        finally {BoundaryProbe.BeforeContainerLaunch=null;BoundaryProbe.BeforeResume=null;}
        policy.Dispose();bool removed=policy.VerifyRemoved();
        return new(result.ProcessId==0 && result.Error?.Contains("Synthetic transaction rollback")==true && removed && result.ProfileDeleteResult==0,removed,result);
    }
    private static string WorkerPath(){string path=Path.Combine(AppContext.BaseDirectory,"SyntheticWorker.exe");if(!File.Exists(path))throw new FileNotFoundException("Build the reviewed native synthetic worker first.",path);return path;}
    private static string StageWorker(string root){string directory=Path.Combine(root,"worker");Directory.CreateDirectory(directory);string staged=Path.Combine(directory,"SyntheticWorker.exe");File.Copy(WorkerPath(),staged,false);if(Hash(staged)!=Hash(WorkerPath()))throw new IOException("Worker staging hash mismatch.");return staged;}
    private static void GrantWorkerReadExecute(string staged,IntPtr sid){var directory=new DirectoryInfo(Path.GetDirectoryName(staged)!);var acl=directory.GetAccessControl();acl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(sid),FileSystemRights.ReadAndExecute,InheritanceFlags.ContainerInherit|InheritanceFlags.ObjectInherit,PropagationFlags.None,AccessControlType.Allow));directory.SetAccessControl(acl);}
    private static int DiagnoseWorker()
    {
        string root=Path.Combine(Path.GetTempPath(),"SpecAudit-WorkerBootstrap-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);string staged=StageWorker(root);
        BoundaryProbe.BeforeContainerLaunch=sid=>GrantWorkerReadExecute(staged,sid);
        BoundaryResult result;try{result=BoundaryProbe.RunWithContainer(staged,["--bootstrap-tree"],Environment.GetFolderPath(Environment.SpecialFolder.System),10000,true);}finally{BoundaryProbe.BeforeContainerLaunch=null;}

        int ready=0;foreach(string line in (result.Output??"").Split('\n')){try{using var doc=JsonDocument.Parse(line);var row=doc.RootElement;bool valid=row.TryGetProperty("Bootstrap",out var bootstrap) && bootstrap.GetString()=="READY" && row.GetProperty("TokenKnown").GetBoolean() && row.GetProperty("IsAppContainer").GetBoolean() && !row.GetProperty("AdminGroupEnabled").GetBoolean();if(valid)ready++;}catch(JsonException){} }
        bool passed=ready==2 && result.ExitCode==0 && result.TokenIsAppContainer==1 && result.AdminGroupEnabled==false && result.ActiveAfterTerminate==0 && result.ProfileDeleteResult==0;
        Console.WriteLine(JsonSerializer.Serialize(new{state=passed?"PASSED_BOOTSTRAP_ONLY":"FAILED_BOOTSTRAP",timestampUtc=DateTimeOffset.UtcNow,privilegedOperationsExecuted=0,networkConnections=0,result},Json));return passed?0:1;
    }
}


