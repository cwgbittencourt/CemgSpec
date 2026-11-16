using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Microsoft.Win32.SafeHandles;
public sealed class BoundaryResult { public string Phase,Error,Output,ContainerProfile; public int? ProfileDeleteResult,TokenIsAppContainer; public int? ExitCode; public bool TimedOut,JobAssigned; public uint ProcessId,TotalProcesses,ActiveBeforeClose,ActiveAfterTerminate; public bool JobTerminationRequested; public long[] JobProcessIds; }
public static class BoundaryProbe {
 public static IntPtr BrokerReadHandle=IntPtr.Zero; public static bool SetBrokerHandleInheritance(bool inherit){return BrokerReadHandle!=IntPtr.Zero && SetHandleInformation(BrokerReadHandle,1,inherit?1u:0u);}
 [StructLayout(LayoutKind.Sequential)] struct SA {public int size;public IntPtr descriptor;[MarshalAs(UnmanagedType.Bool)]public bool inherit;}
 [StructLayout(LayoutKind.Sequential)] struct SIDATTR {public IntPtr sid;public uint attributes;}
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] struct SI {public int cb;public string reserved,desktop,title;public int x,y,xs,ys,xc,yc,fill,flags;public short show,cb2;public IntPtr res2,input,output,error;}
 [StructLayout(LayoutKind.Sequential)] struct PI {public IntPtr process,thread;public uint pid,tid;}
 [StructLayout(LayoutKind.Sequential)] struct BASIC {public long processTime,jobTime;public uint flags;public UIntPtr min,max;public uint active;public UIntPtr affinity;public uint priority,scheduling;}
 [StructLayout(LayoutKind.Sequential)] struct IO {public ulong a,b,c,d,e,f;}
 [StructLayout(LayoutKind.Sequential)] struct EXT {public BASIC basic;public IO io;public UIntPtr pm,jm,ppm,pjm;}
 [StructLayout(LayoutKind.Sequential)] struct ACCOUNT {public long a,b,c,d;public uint faults,total,active,terminated;}
 [DllImport("advapi32.dll",SetLastError=true)]static extern bool GetTokenInformation(IntPtr token,int kind,IntPtr value,int size,out int needed);
 [DllImport("kernel32.dll")]static extern IntPtr GetCurrentProcess();
 [DllImport("advapi32.dll",SetLastError=true)]static extern bool OpenProcessToken(IntPtr p,uint access,out IntPtr token);
 [DllImport("advapi32.dll",SetLastError=true)]static extern bool CreateRestrictedToken(IntPtr old,uint flags,uint a,IntPtr b,uint c,IntPtr d,uint e,IntPtr f,out IntPtr token);
 [DllImport("advapi32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern bool ConvertStringSidToSid(string s,out IntPtr sid);
 [DllImport("advapi32.dll")]static extern uint GetLengthSid(IntPtr sid);
 [DllImport("advapi32.dll",SetLastError=true)]static extern bool SetTokenInformation(IntPtr token,int kind,IntPtr data,int size);
 [DllImport("advapi32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern bool CreateProcessAsUser(IntPtr token,string app,StringBuilder cmd,IntPtr pa,IntPtr ta,bool inherit,uint flags,IntPtr env,string cwd,ref SI si,out PI pi);
 [DllImport("kernel32.dll",SetLastError=true)]static extern bool CreatePipe(out IntPtr read,out IntPtr write,ref SA sa,int size);
 [DllImport("kernel32.dll",SetLastError=true)]static extern bool SetHandleInformation(IntPtr h,uint mask,uint flags);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern IntPtr CreateJobObject(IntPtr sa,string name);
 [DllImport("kernel32.dll",SetLastError=true)]static extern bool SetInformationJobObject(IntPtr h,int kind,ref EXT value,int size);
 [DllImport("kernel32.dll",SetLastError=true)]static extern bool QueryInformationJobObject(IntPtr h,int kind,out ACCOUNT value,int size,IntPtr returned);
 [DllImport("kernel32.dll",SetLastError=true)]static extern bool AssignProcessToJobObject(IntPtr job,IntPtr process);
 [DllImport("kernel32.dll",SetLastError=true)]static extern bool TerminateJobObject(IntPtr job,uint code);
 [DllImport("kernel32.dll",SetLastError=true)]static extern bool QueryInformationJobObject(IntPtr job,int kind,IntPtr buffer,int size,IntPtr returned);
 [DllImport("kernel32.dll")]static extern uint ResumeThread(IntPtr h);
 [DllImport("kernel32.dll")]static extern uint WaitForSingleObject(IntPtr h,uint ms);
 [DllImport("kernel32.dll")]static extern bool GetExitCodeProcess(IntPtr h,out uint c);
 [DllImport("kernel32.dll")]static extern bool TerminateProcess(IntPtr h,uint code);
 [DllImport("kernel32.dll")]static extern bool CloseHandle(IntPtr h);
 [DllImport("kernel32.dll")]static extern IntPtr LocalFree(IntPtr h);
 [StructLayout(LayoutKind.Sequential)] struct SIEX {public SI startup;public IntPtr attributes;}
 [StructLayout(LayoutKind.Sequential)] struct CAPS {public IntPtr sid,capabilities;public uint count,reserved;}
 [DllImport("userenv.dll",CharSet=CharSet.Unicode)]static extern int CreateAppContainerProfile(string name,string display,string description,IntPtr capabilities,uint count,out IntPtr sid);
 [DllImport("userenv.dll",CharSet=CharSet.Unicode)]static extern int DeleteAppContainerProfile(string name);
 [DllImport("advapi32.dll")]static extern IntPtr FreeSid(IntPtr sid);
 [DllImport("kernel32.dll",SetLastError=true)]static extern bool InitializeProcThreadAttributeList(IntPtr list,int count,int flags,ref IntPtr size);
 [DllImport("kernel32.dll",SetLastError=true)]static extern bool UpdateProcThreadAttribute(IntPtr list,uint flags,IntPtr attribute,IntPtr value,IntPtr size,IntPtr previous,IntPtr returned);
 [DllImport("kernel32.dll")]static extern void DeleteProcThreadAttributeList(IntPtr list);
 [DllImport("advapi32.dll",EntryPoint="CreateProcessAsUserW",CharSet=CharSet.Unicode,SetLastError=true)]static extern bool CreateProcessAsUserEx(IntPtr token,string app,StringBuilder cmd,IntPtr pa,IntPtr ta,bool inherit,uint flags,IntPtr env,string cwd,ref SIEX si,out PI pi);
 public static string Quote(string s){return "\""+s.Replace("\\","\\").Replace("\"","\\\"")+"\"";}
 public static BoundaryResult Run(string exe,string[] args,string cwd,int timeoutMs){return RunWithContainer(exe,args,cwd,timeoutMs,false);}
 public static BoundaryResult RunWithContainer(string exe,string[] args,string cwd,int timeoutMs,bool appContainer){
  var r=new BoundaryResult();IntPtr token=IntPtr.Zero,low=IntPtr.Zero,sid=IntPtr.Zero,label=IntPtr.Zero,rd=IntPtr.Zero,wr=IntPtr.Zero,job=IntPtr.Zero; PI pi=new PI();StreamReader reader=null;IntPtr containerSid=IntPtr.Zero,attrs=IntPtr.Zero,caps=IntPtr.Zero,handleList=IntPtr.Zero;bool attrsReady=false;
  try {
   r.Phase="token";if(!OpenProcessToken(GetCurrentProcess(),0xF01FF,out token))throw new Win32Exception();if(!CreateRestrictedToken(token,1,0,IntPtr.Zero,0,IntPtr.Zero,0,IntPtr.Zero,out low))throw new Win32Exception();
   if(!ConvertStringSidToSid("S-1-16-4096",out sid))throw new Win32Exception();label=Marshal.AllocHGlobal(Marshal.SizeOf<SIDATTR>());Marshal.StructureToPtr(new SIDATTR{sid=sid,attributes=0x20},label,false);if(!SetTokenInformation(low,25,label,Marshal.SizeOf<SIDATTR>()+(int)GetLengthSid(sid)))throw new Win32Exception();
   r.Phase="pipe";SA sa=new SA{size=Marshal.SizeOf<SA>(),inherit=true};if(!CreatePipe(out rd,out wr,ref sa,0)||!SetHandleInformation(rd,1,0))throw new Win32Exception();
   r.Phase="job";job=CreateJobObject(IntPtr.Zero,null);if(job==IntPtr.Zero)throw new Win32Exception();EXT limits=new EXT();limits.basic.flags=0x2000;if(!SetInformationJobObject(job,9,ref limits,Marshal.SizeOf<EXT>()))throw new Win32Exception();
   SI si=new SI{cb=Marshal.SizeOf<SI>(),flags=0x100,output=wr,error=wr};var cmd=new StringBuilder(Quote(exe));foreach(var arg in args)cmd.Append(' ').Append(Quote(arg));
   r.Phase="launch";
if(appContainer){
 r.Phase="container-profile";r.ContainerProfile="specaudit-probe-"+Guid.NewGuid().ToString("N");int hr=CreateAppContainerProfile(r.ContainerProfile,r.ContainerProfile,"Temporary synthetic SpecAudit isolation probe",IntPtr.Zero,0,out containerSid);if(hr!=0)throw new Exception("CreateAppContainerProfile HRESULT "+hr);
 IntPtr size=IntPtr.Zero;InitializeProcThreadAttributeList(IntPtr.Zero,2,0,ref size);attrs=Marshal.AllocHGlobal(size);if(!InitializeProcThreadAttributeList(attrs,2,0,ref size))throw new Win32Exception();attrsReady=true;
 caps=Marshal.AllocHGlobal(Marshal.SizeOf<CAPS>());Marshal.StructureToPtr(new CAPS{sid=containerSid,count=0},caps,false);if(!UpdateProcThreadAttribute(attrs,0,new IntPtr(0x20009),caps,new IntPtr(Marshal.SizeOf<CAPS>()),IntPtr.Zero,IntPtr.Zero))throw new Win32Exception();
 int handleCount=BrokerReadHandle==IntPtr.Zero?1:2;handleList=Marshal.AllocHGlobal(IntPtr.Size*handleCount);Marshal.WriteIntPtr(handleList,0,wr);if(handleCount==2)Marshal.WriteIntPtr(handleList,IntPtr.Size,BrokerReadHandle);if(!UpdateProcThreadAttribute(attrs,0,new IntPtr(0x20002),handleList,new IntPtr(IntPtr.Size*handleCount),IntPtr.Zero,IntPtr.Zero))throw new Win32Exception(); SIEX extended=new SIEX{startup=si,attributes=attrs};extended.startup.cb=Marshal.SizeOf<SIEX>();r.Phase="container-launch";if(!CreateProcessAsUserEx(low,exe,cmd,IntPtr.Zero,IntPtr.Zero,true,0x08080004,IntPtr.Zero,cwd,ref extended,out pi))throw new Win32Exception();
}else{if(!CreateProcessAsUser(low,exe,cmd,IntPtr.Zero,IntPtr.Zero,true,0x08000004,IntPtr.Zero,cwd,ref si,out pi))throw new Win32Exception();}
r.ProcessId=pi.pid;IntPtr actualToken;if(OpenProcessToken(pi.process,8,out actualToken)){IntPtr tokenInfo=Marshal.AllocHGlobal(4);try{int needed;if(GetTokenInformation(actualToken,29,tokenInfo,4,out needed))r.TokenIsAppContainer=Marshal.ReadInt32(tokenInfo);}finally{Marshal.FreeHGlobal(tokenInfo);CloseHandle(actualToken);}}
   r.Phase="assign";if(!AssignProcessToJobObject(job,pi.process)){TerminateProcess(pi.process,125);throw new Win32Exception();}r.JobAssigned=true;
   CloseHandle(wr);wr=IntPtr.Zero;reader=new StreamReader(new FileStream(new SafeFileHandle(rd,true),FileAccess.Read),Encoding.UTF8);rd=IntPtr.Zero;var outputTask=reader.ReadToEndAsync();ResumeThread(pi.thread);
   r.Phase="wait";if(WaitForSingleObject(pi.process,(uint)timeoutMs)!=0)r.TimedOut=true;
   ACCOUNT account;if(QueryInformationJobObject(job,1,out account,Marshal.SizeOf<ACCOUNT>(),IntPtr.Zero)){r.TotalProcesses=account.total;r.ActiveBeforeClose=account.active;}
   IntPtr list=Marshal.AllocHGlobal(4096);try{if(QueryInformationJobObject(job,3,list,4096,IntPtr.Zero)){int count=Marshal.ReadInt32(list,4);r.JobProcessIds=new long[Math.Min(count,128)];for(int i=0;i<r.JobProcessIds.Length;i++)r.JobProcessIds[i]=Marshal.ReadIntPtr(list,8+i*IntPtr.Size).ToInt64();}}finally{Marshal.FreeHGlobal(list);} r.JobTerminationRequested=TerminateJobObject(job,124);for(int i=0;i<40;i++){ACCOUNT left;if(QueryInformationJobObject(job,1,out left,Marshal.SizeOf<ACCOUNT>(),IntPtr.Zero)){r.ActiveAfterTerminate=left.active;if(left.active==0)break;}System.Threading.Thread.Sleep(50);} CloseHandle(job);job=IntPtr.Zero;WaitForSingleObject(pi.process,5000);uint exit;if(GetExitCodeProcess(pi.process,out exit))r.ExitCode=unchecked((int)exit);
   if(outputTask.Wait(5000))r.Output=outputTask.Result;else r.Error="Output pipe did not close";r.Phase="finished";
  }catch(Exception e){r.Error=e.GetType().Name+": "+e.Message;}
  finally{if(job!=IntPtr.Zero)CloseHandle(job);if(pi.thread!=IntPtr.Zero)CloseHandle(pi.thread);if(pi.process!=IntPtr.Zero)CloseHandle(pi.process);if(reader!=null)reader.Dispose();if(rd!=IntPtr.Zero)CloseHandle(rd);if(wr!=IntPtr.Zero)CloseHandle(wr);if(label!=IntPtr.Zero)Marshal.FreeHGlobal(label);if(sid!=IntPtr.Zero)LocalFree(sid);if(low!=IntPtr.Zero)CloseHandle(low);if(token!=IntPtr.Zero)CloseHandle(token);if(attrsReady)DeleteProcThreadAttributeList(attrs);if(attrs!=IntPtr.Zero)Marshal.FreeHGlobal(attrs);if(caps!=IntPtr.Zero)Marshal.FreeHGlobal(caps);if(handleList!=IntPtr.Zero)Marshal.FreeHGlobal(handleList);if(containerSid!=IntPtr.Zero){FreeSid(containerSid);r.ProfileDeleteResult=DeleteAppContainerProfile(r.ContainerProfile);}}
  return r;
 }
}