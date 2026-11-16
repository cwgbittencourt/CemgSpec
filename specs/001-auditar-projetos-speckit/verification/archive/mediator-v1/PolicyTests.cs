using System.Runtime.InteropServices;
namespace SpecAudit.NetworkProof;
internal static class ExecutionGate
{
    internal static string Mode(string[] args,bool administrator)
    {
        if(args.Length==0 || args.SequenceEqual(new[]{"--self-test"}))return "self-test";
        if(args.SequenceEqual(new[]{"--describe"}))return "describe";
        if(!args.SequenceEqual(new[]{"--run-wfp-proof","--ack-synthetic-only"}))throw new ArgumentException("Only documented modes are accepted; no SID, executable, endpoint or firewall-rule arguments are allowed.");
        if(!administrator)throw new UnauthorizedAccessException("An explicitly authorized elevated console is required for the synthetic WFP proof. No elevation will be requested automatically.");
        return "proof";
    }
}
internal sealed class PolicyLifetime
{
    internal enum Phase {Suspended,Installed,Running,Stopped,Released}
    internal Phase State {get;private set;}=Phase.Suspended;
    internal void Install(){if(State!=Phase.Suspended)throw new InvalidOperationException();State=Phase.Installed;}
    internal void Resume(){if(State!=Phase.Installed)throw new InvalidOperationException();State=Phase.Running;}
    internal void Stop(){if(State==Phase.Released)throw new InvalidOperationException();State=Phase.Stopped;}
    internal void Release(){if(State!=Phase.Stopped)throw new InvalidOperationException("Stop worker/job before releasing filters.");State=Phase.Released;}
}
internal static class SelfTests
{
    internal static object Run()
    {
        var checks=new List<string>();
        void Assert(bool ok,string name){if(!ok)throw new InvalidOperationException("Self-test failed: "+name);checks.Add(name);}
        bool Reject(System.Action action){try{action();return false;}catch(ArgumentException){return true;}catch(UnauthorizedAccessException){return true;}catch(InvalidOperationException){return true;}}
        Assert(ExecutionGate.Mode([],false)=="self-test","No arguments cannot mutate policy");
        Assert(ExecutionGate.Mode(["--describe"],false)=="describe","Describe is unprivileged");
        Assert(Reject(()=>ExecutionGate.Mode(["--run-wfp-proof"],true)),"Missing acknowledgement refused");
        Assert(Reject(()=>ExecutionGate.Mode(["--run-wfp-proof","--ack-synthetic-only"],false)),"Non-admin proof refused without UAC");
        Assert(Reject(()=>ExecutionGate.Mode(["--run-wfp-proof","--ack-synthetic-only","--target","other.exe"],true)),"Arbitrary target refused");
        Assert(ExecutionGate.Mode(["--run-wfp-proof","--ack-synthetic-only"],true)=="proof","Exact authorized proof mode accepted");
        var life=new PolicyLifetime();Assert(Reject(life.Resume),"Cannot resume before policy");life.Install();life.Resume();Assert(Reject(life.Release),"Cannot remove filters while worker runs");life.Stop();life.Release();Assert(life.State==PolicyLifetime.Phase.Released,"Orderly lifetime completes");
        Assert(Reject(()=>MediatorSession.SidCondition(IntPtr.Zero)),"No unscoped SID condition");
        Assert(Marshal.SizeOf<WfpNative.Value>()==16,"FWP_VALUE0 x64 layout");
        Assert(Marshal.SizeOf<WfpNative.Condition>()==40,"FWPM_FILTER_CONDITION0 x64 layout");
        Assert(Marshal.SizeOf<WfpNative.Filter>()==200,"FWPM_FILTER0 x64 layout");
        Assert(Marshal.OffsetOf<WfpNative.Filter>(nameof(WfpNative.Filter.Conditions)).ToInt32()==120,"Condition pointer offset x64");
        Assert(Marshal.SizeOf<WfpNative.Sublayer>()==72,"FWPM_SUBLAYER0 x64 layout");
        return new {state="PASSED",checks=checks.Count,items=checks,privilegedOperationsExecuted=0,networkConnections=0};
    }
}