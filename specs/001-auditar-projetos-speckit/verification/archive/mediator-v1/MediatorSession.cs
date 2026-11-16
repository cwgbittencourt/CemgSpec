using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SpecAudit.NetworkProof;

internal static class WfpNative
{
    internal static readonly Guid Connect4 = new("c38d57d1-05a7-4c33-904f-7fbceee60e82");
    internal static readonly Guid Connect6 = new("4a72393b-319f-44bc-84c3-ba54dcb3b6b4");
    internal static readonly Guid PackageId = new("71bc78fa-f17c-4997-a602-6abb261f351c");
    internal static readonly Guid RemoteAddress = new("b235ae9a-1d64-49b8-a44c-5ff3d9095045");
    internal static readonly Guid RemotePort = new("c35a604d-d22b-4e1a-91b4-68f674ee674b");
    internal static readonly Guid Protocol = new("3971ef2b-623e-4f9a-8cb1-6e79b806b9a7");
    internal const uint NotFound = 0x80320003;

    [StructLayout(LayoutKind.Sequential)] internal struct Display { public IntPtr Name, Description; }
    [StructLayout(LayoutKind.Sequential)] internal struct Blob { public uint Size; public IntPtr Data; }
    [StructLayout(LayoutKind.Sequential)] internal struct Session { public Guid Key; public Display Display; public uint Flags, Wait, Pid; public IntPtr Sid, Username; public int Kernel; }
    [StructLayout(LayoutKind.Sequential)] internal struct Sublayer { public Guid Key; public Display Display; public uint Flags; public IntPtr Provider; public Blob Data; public ushort Weight; }
    [StructLayout(LayoutKind.Explicit, Size=8)] internal struct Data { [FieldOffset(0)] public byte Byte; [FieldOffset(0)] public ushort UShort; [FieldOffset(0)] public uint UInt; [FieldOffset(0)] public IntPtr Pointer; }
    [StructLayout(LayoutKind.Sequential)] internal struct Value { public uint Type; public Data Data; }
    [StructLayout(LayoutKind.Sequential)] internal struct Condition { public Guid Field; public uint Match; public Value Value; }
    [StructLayout(LayoutKind.Sequential)] internal struct Action { public uint Type; public Guid TypeId; }
    [StructLayout(LayoutKind.Explicit,Size=16)] internal struct Context { [FieldOffset(0)] public ulong Raw; [FieldOffset(0)] public Guid Provider; }
    [StructLayout(LayoutKind.Sequential)] internal struct Filter { public Guid Key; public Display Display; public uint Flags; public IntPtr Provider; public Blob ProviderData; public Guid Layer, Sublayer; public Value Weight; public uint Count; public IntPtr Conditions; public Action Action; public Context Context; public IntPtr Reserved; public ulong Id; public Value EffectiveWeight; }

    [DllImport("fwpuclnt.dll",CharSet=CharSet.Unicode)] internal static extern uint FwpmEngineOpen0(string? server,uint auth,IntPtr identity,ref Session session,out IntPtr engine);
    [DllImport("fwpuclnt.dll")] internal static extern uint FwpmEngineClose0(IntPtr engine);
    [DllImport("fwpuclnt.dll")] internal static extern uint FwpmTransactionBegin0(IntPtr engine,uint flags);
    [DllImport("fwpuclnt.dll")] internal static extern uint FwpmTransactionCommit0(IntPtr engine);
    [DllImport("fwpuclnt.dll")] internal static extern uint FwpmTransactionAbort0(IntPtr engine);
    [DllImport("fwpuclnt.dll")] internal static extern uint FwpmSubLayerAdd0(IntPtr engine,ref Sublayer sublayer,IntPtr security);
    [DllImport("fwpuclnt.dll")] internal static extern uint FwpmFilterAdd0(IntPtr engine,ref Filter filter,IntPtr security,out ulong id);
    [DllImport("fwpuclnt.dll")] internal static extern uint FwpmFilterGetByKey0(IntPtr engine,ref Guid key,out IntPtr filter);
    [DllImport("fwpuclnt.dll")] internal static extern uint FwpmSubLayerGetByKey0(IntPtr engine,ref Guid key,out IntPtr sublayer);
    [DllImport("fwpuclnt.dll")] internal static extern void FwpmFreeMemory0(ref IntPtr memory);
    internal static void Check(uint status,string operation) { if(status!=0) throw new Win32Exception(unchecked((int)status),$"{operation}: 0x{status:x8}"); }
}

internal sealed class MediatorSession : IDisposable
{
    private IntPtr engine;
    private readonly List<Guid> keys = [];
    internal Guid SublayerKey { get; } = Guid.NewGuid();
    internal bool Activated { get; private set; }
    internal bool Closed { get; private set; }
    internal IReadOnlyList<Guid> FilterKeys => keys;
    internal static WfpNative.Condition SidCondition(IntPtr sid)
    {
        if(sid==IntPtr.Zero) throw new ArgumentException("A dedicated AppContainer SID is required.");
        return new() { Field=WfpNative.PackageId, Value=new() { Type=13, Data=new() { Pointer=sid } } };
    }
    internal static WfpNative.Condition Scalar(Guid field,uint type,uint value) => new() { Field=field, Value=new() {Type=type,Data=new(){UInt=value}} };
    internal void Activate(IntPtr sid,int port,bool injectRollback=false)
    {
        if(Activated||Closed) throw new InvalidOperationException("One session per execution.");
        if(port<1||port>65535) throw new ArgumentOutOfRangeException(nameof(port));
        _=SidCondition(sid);
        var session=new WfpNative.Session {Key=Guid.NewGuid(),Flags=1};
        WfpNative.Check(WfpNative.FwpmEngineOpen0(null,10,IntPtr.Zero,ref session,out engine),"open dynamic engine");
        bool transaction=false;
        try
        {
            WfpNative.Check(WfpNative.FwpmTransactionBegin0(engine,0),"begin transaction"); transaction=true;
            IntPtr name=Marshal.StringToHGlobalUni("SpecAudit synthetic proof "+SublayerKey);
            try { var sublayer=new WfpNative.Sublayer {Key=SublayerKey,Display=new(){Name=name},Weight=0x100}; WfpNative.Check(WfpNative.FwpmSubLayerAdd0(engine,ref sublayer,IntPtr.Zero),"add private sublayer"); }
            finally {Marshal.FreeHGlobal(name);}
            // One permit and two deny filters, all scoped to this execution's package SID.
            // Soft permit: third-party blocking policies remain effective. No CLEAR_ACTION_RIGHT.
            Add(WfpNative.Connect4,0x1002,15,[SidCondition(sid),Scalar(WfpNative.RemoteAddress,3,0x7f000001),Scalar(WfpNative.RemotePort,2,(uint)port),Scalar(WfpNative.Protocol,1,6)]);
            if(injectRollback)throw new InvalidOperationException("Synthetic transaction rollback before commit.");
            Add(WfpNative.Connect4,0x1001,0,[SidCondition(sid)]);
            Add(WfpNative.Connect6,0x1001,0,[SidCondition(sid)]);
            WfpNative.Check(WfpNative.FwpmTransactionCommit0(engine),"commit filters");transaction=false;Activated=true;
        }
        catch {if(transaction)_=WfpNative.FwpmTransactionAbort0(engine);Dispose();throw;}
    }
    private void Add(Guid layer,uint action,byte weight,WfpNative.Condition[] conditions)
    {
        if(conditions.Length==0||conditions[0].Field!=WfpNative.PackageId||conditions[0].Value.Data.Pointer==IntPtr.Zero) throw new InvalidOperationException("Unscoped filters are forbidden.");
        int size=Marshal.SizeOf<WfpNative.Condition>(); IntPtr array=Marshal.AllocHGlobal(size*conditions.Length);IntPtr name=Marshal.StringToHGlobalUni("SpecAudit execution-scoped proof");
        try
        {
            for(int i=0;i<conditions.Length;i++)Marshal.StructureToPtr(conditions[i],array+i*size,false);
            var filter=new WfpNative.Filter {Key=Guid.NewGuid(),Display=new(){Name=name},Layer=layer,Sublayer=SublayerKey,Weight=new(){Type=1,Data=new(){Byte=weight}},Count=(uint)conditions.Length,Conditions=array,Action=new(){Type=action}};
            WfpNative.Check(WfpNative.FwpmFilterAdd0(engine,ref filter,IntPtr.Zero,out _),"add SID-scoped filter");keys.Add(filter.Key);
        }
        finally {Marshal.FreeHGlobal(name);Marshal.FreeHGlobal(array);}
    }
    public void Dispose()
    {
        if(engine!=IntPtr.Zero){uint status=WfpNative.FwpmEngineClose0(engine);if(status!=0)throw new Win32Exception(unchecked((int)status),"Dynamic session close failed.");engine=IntPtr.Zero;}
        Closed=true;
    }
    internal bool VerifyRemoved()
    {
        if(!Closed)throw new InvalidOperationException("Terminate worker and close its policy first.");
        var session=new WfpNative.Session();WfpNative.Check(WfpNative.FwpmEngineOpen0(null,10,IntPtr.Zero,ref session,out var observer),"open cleanup observer");
        try
        {
            foreach(var keyValue in keys){var key=keyValue;uint s=WfpNative.FwpmFilterGetByKey0(observer,ref key,out var value);if(value!=IntPtr.Zero)WfpNative.FwpmFreeMemory0(ref value);if(s!=WfpNative.NotFound)return false;}
            var subkey=SublayerKey;uint sub=WfpNative.FwpmSubLayerGetByKey0(observer,ref subkey,out var pointer);if(pointer!=IntPtr.Zero)WfpNative.FwpmFreeMemory0(ref pointer);return sub==0x80320007;
        }
        finally {_=WfpNative.FwpmEngineClose0(observer);}
    }
}