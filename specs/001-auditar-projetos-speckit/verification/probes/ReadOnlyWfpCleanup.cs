using System;
using System.Runtime.InteropServices;
public static class ReadOnlyWfpCleanup {
 [StructLayout(LayoutKind.Sequential)]struct D{public IntPtr name,description;}
 [StructLayout(LayoutKind.Sequential)]struct S{public Guid key;public D display;public uint flags,wait,pid;public IntPtr sid,user;public int kernel;}
 [DllImport("fwpuclnt.dll",CharSet=CharSet.Unicode)]static extern uint FwpmEngineOpen0(string server,uint auth,IntPtr identity,ref S session,out IntPtr engine);
 [DllImport("fwpuclnt.dll")]static extern uint FwpmEngineClose0(IntPtr engine);
 [DllImport("fwpuclnt.dll")]static extern uint FwpmFilterGetByKey0(IntPtr engine,ref Guid key,out IntPtr value);
 [DllImport("fwpuclnt.dll")]static extern uint FwpmSubLayerGetByKey0(IntPtr engine,ref Guid key,out IntPtr value);
 [DllImport("fwpuclnt.dll")]static extern void FwpmFreeMemory0(ref IntPtr value);
 public static uint Query(Guid key,bool sublayer){var session=new S();IntPtr engine;uint result=FwpmEngineOpen0(null,10,IntPtr.Zero,ref session,out engine);if(result!=0)return result;IntPtr value=IntPtr.Zero;try{return sublayer?FwpmSubLayerGetByKey0(engine,ref key,out value):FwpmFilterGetByKey0(engine,ref key,out value);}finally{if(value!=IntPtr.Zero)FwpmFreeMemory0(ref value);FwpmEngineClose0(engine);}}
}