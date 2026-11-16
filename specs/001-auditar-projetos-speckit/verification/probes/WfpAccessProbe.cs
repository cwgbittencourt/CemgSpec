using System;
using System.Runtime.InteropServices;
public static class WfpAccessProbe {
 [StructLayout(LayoutKind.Sequential)]struct Blob {public uint size;public IntPtr data;}
 [StructLayout(LayoutKind.Sequential)]struct Sublayer {public Guid key;public Display display;public uint flags;public IntPtr provider;public Blob data;public ushort weight;}
 [DllImport("fwpuclnt.dll")]static extern uint FwpmSubLayerAdd0(IntPtr engine,ref Sublayer sublayer,IntPtr security);
 [StructLayout(LayoutKind.Sequential)] struct Display {public IntPtr name,description;}
 [StructLayout(LayoutKind.Sequential)] struct Session {public Guid key;public Display display;public uint flags,wait,pid;public IntPtr sid,username;public int kernel;}
 [DllImport("fwpuclnt.dll",CharSet=CharSet.Unicode)]static extern uint FwpmEngineOpen0(string server,uint auth,IntPtr identity,ref Session session,out IntPtr engine);
 [DllImport("fwpuclnt.dll")]static extern uint FwpmEngineClose0(IntPtr engine);
 public static uint AddEmptyDynamicSublayer(){IntPtr engine=IntPtr.Zero;var session=new Session{key=Guid.NewGuid(),flags=1};uint status=FwpmEngineOpen0(null,10,IntPtr.Zero,ref session,out engine);if(status!=0)return status;IntPtr name=Marshal.StringToHGlobalUni("SpecAudit synthetic permission probe "+Guid.NewGuid().ToString("N"));try{var layer=new Sublayer{key=Guid.NewGuid(),display=new Display{name=name},weight=0};return FwpmSubLayerAdd0(engine,ref layer,IntPtr.Zero);}finally{Marshal.FreeHGlobal(name);FwpmEngineClose0(engine);}}
 public static uint OpenDynamic(){IntPtr engine=IntPtr.Zero;var session=new Session{key=Guid.NewGuid(),flags=1};uint status=FwpmEngineOpen0(null,10,IntPtr.Zero,ref session,out engine);if(engine!=IntPtr.Zero)FwpmEngineClose0(engine);return status;}
}