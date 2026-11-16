$ErrorActionPreference='Stop'
$root=Join-Path ([IO.Path]::GetTempPath()) ('SpecAudit-TokenProbe-'+[Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($root)|Out-Null
$source=Join-Path $root 'source.txt'
$canary=Join-Path $root 'outside-canary.txt'
[IO.File]::WriteAllText($source,'synthetic-read-only')
[IO.File]::WriteAllText($canary,'original-canary')
$baseline=(Get-FileHash $source).Hash
$canaryBaseline=(Get-FileHash $canary).Hash
$native=@"
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
public static class SpecAuditLowTokenProbe {
 [StructLayout(LayoutKind.Sequential)] struct SIDATTR {public IntPtr Sid;public uint Attributes;}
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] struct SI {public int cb;public string reserved;public string desktop;public string title;public int x,y,xs,ys,xc,yc,fill,flags;public short show,cb2;public IntPtr res2,input,output,error;}
 [StructLayout(LayoutKind.Sequential)] struct PI {public IntPtr process,thread;public uint pid,tid;}
 [DllImport("kernel32.dll")] static extern IntPtr GetCurrentProcess();
 [DllImport("advapi32.dll",SetLastError=true)] static extern bool OpenProcessToken(IntPtr p,uint access,out IntPtr token);
 [DllImport("advapi32.dll",SetLastError=true)] static extern bool CreateRestrictedToken(IntPtr old,uint flags,uint disable,IntPtr sids,uint del,IntPtr privileges,uint restricted,IntPtr rsids,out IntPtr token);
 [DllImport("advapi32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern bool ConvertStringSidToSid(string sid,out IntPtr result);
 [DllImport("advapi32.dll",SetLastError=true)] static extern bool SetTokenInformation(IntPtr token,int kind,IntPtr data,int size);
 [DllImport("advapi32.dll")] static extern uint GetLengthSid(IntPtr sid);
 [DllImport("advapi32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern bool CreateProcessAsUser(IntPtr token,string app,StringBuilder command,IntPtr pa,IntPtr ta,bool inherit,uint flags,IntPtr env,string cwd,ref SI si,out PI pi);
 [DllImport("kernel32.dll")] static extern uint WaitForSingleObject(IntPtr h,uint ms);
 [DllImport("kernel32.dll")] static extern bool GetExitCodeProcess(IntPtr h,out uint code);
 [DllImport("kernel32.dll")] static extern bool TerminateProcess(IntPtr h,uint code);
 [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr h);
 [DllImport("kernel32.dll")] static extern IntPtr LocalFree(IntPtr h);
 public static string Run(string exe,string arguments,string cwd) {
  IntPtr token=IntPtr.Zero,restricted=IntPtr.Zero,sid=IntPtr.Zero,label=IntPtr.Zero;PI pi=new PI();
  string phase="OpenProcessToken";
  try {
   if(!OpenProcessToken(GetCurrentProcess(),0xF01FF,out token))throw new Win32Exception();
   phase="CreateRestrictedToken";if(!CreateRestrictedToken(token,1,0,IntPtr.Zero,0,IntPtr.Zero,0,IntPtr.Zero,out restricted))throw new Win32Exception();
   phase="ConvertLowSid";if(!ConvertStringSidToSid("S-1-16-4096",out sid))throw new Win32Exception();
   SIDATTR value=new SIDATTR{Sid=sid,Attributes=0x20};label=Marshal.AllocHGlobal(Marshal.SizeOf<SIDATTR>());Marshal.StructureToPtr(value,label,false);
   phase="SetIntegrity";if(!SetTokenInformation(restricted,25,label,Marshal.SizeOf<SIDATTR>()+(int)GetLengthSid(sid)))throw new Win32Exception();
   SI si=new SI();si.cb=Marshal.SizeOf<SI>();phase="CreateProcessAsUser";
   if(!CreateProcessAsUser(restricted,exe,new StringBuilder("\""+exe+"\" "+arguments),IntPtr.Zero,IntPtr.Zero,false,0x08000000,IntPtr.Zero,cwd,ref si,out pi))throw new Win32Exception();
   phase="Wait";if(WaitForSingleObject(pi.process,20000)!=0){TerminateProcess(pi.process,124);return "TIMEOUT";}
   uint code;GetExitCodeProcess(pi.process,out code);return "EXIT:"+code;
  } catch(Win32Exception ex){return "BLOCKED:"+phase+":"+ex.NativeErrorCode+":"+ex.Message;}
  finally{if(pi.thread!=IntPtr.Zero)CloseHandle(pi.thread);if(pi.process!=IntPtr.Zero)CloseHandle(pi.process);if(label!=IntPtr.Zero)Marshal.FreeHGlobal(label);if(sid!=IntPtr.Zero)LocalFree(sid);if(restricted!=IntPtr.Zero)CloseHandle(restricted);if(token!=IntPtr.Zero)CloseHandle(token);}
 }
}
"@
Add-Type -TypeDefinition $native
# Child only touches synthetic files. Exit 0 means read succeeded and write was denied.
$child="try { if([IO.File]::ReadAllText('$source') -ne 'synthetic-read-only'){exit 31} } catch {exit 32}; try { [IO.File]::WriteAllText('$canary','unexpected-write'); exit 33 } catch [System.UnauthorizedAccessException] {exit 0} catch {exit 34}"
$encoded=[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($child))
$result=[SpecAuditLowTokenProbe]::Run('C:/Windows/System32/WindowsPowerShell/v1.0/powershell.exe',('-NoProfile -NonInteractive -EncodedCommand '+$encoded),$root)
$report=[ordered]@{state=$result;fixtureRoot=$root;sourceIntact=((Get-FileHash $source).Hash -eq $baseline);canaryIntact=((Get-FileHash $canary).Hash -eq $canaryBaseline);sourceAclsModified=$false;adminElevationRequested=$false;mechanism='Restricted token with low mandatory integrity';limits=@('Does not prove agents work under token','Does not restrict network','No Job Object or descendant cancellation demonstrated','Not full AppContainer isolation')}
$path=Join-Path (Split-Path $PSScriptRoot -Parent) 'windows-token-probe.json'
[IO.File]::WriteAllText($path,($report|ConvertTo-Json -Depth 5),[Text.UTF8Encoding]::new($false))
$report|ConvertTo-Json -Depth 5