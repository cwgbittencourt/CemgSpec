using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
namespace SpecAudit.Infrastructure.Documents;
internal static class WindowsReadHandle
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Information
    {
        public uint Attributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME Created,Accessed,Written;
        public uint Volume,SizeHigh,SizeLow,Links,IndexHigh,IndexLow;
        public readonly string Identity=>$"{Volume:x8}:{IndexHigh:x8}{IndexLow:x8}";
        public readonly long Length=>((long)SizeHigh<<32)|SizeLow;
        public readonly long WriteStamp=>((long)Written.dwHighDateTime<<32)|(uint)Written.dwLowDateTime;
    }
    public static SafeFileHandle Open(string path,bool directory)
    {
        if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();
        var handle=CreateFile(path,directory?0x80u:0x80000000u,7,IntPtr.Zero,3,directory?0x02000000u:0x00200000u,IntPtr.Zero);
        if(handle.IsInvalid){handle.Dispose();throw new DocumentAccessException("Unreadable");}
        return handle;
    }
    public static Information Info(SafeFileHandle handle)
    {
        if(!GetFileInformationByHandle(handle,out var info))throw new DocumentAccessException("Unreadable");return info;
    }
    public static string FinalPath(SafeFileHandle handle)
    {
        var buffer=new StringBuilder(32768);var count=GetFinalPathNameByHandle(handle,buffer,(uint)buffer.Capacity,0);
        if(count==0 || count>=buffer.Capacity)throw new DocumentAccessException("Unreadable");
        var value=buffer.ToString();return value.StartsWith("\\\\?\\UNC\\",StringComparison.OrdinalIgnoreCase) ? "\\\\"+value[8..] : value.StartsWith("\\\\?\\",StringComparison.Ordinal) ? value[4..] : value;
    }
    public static bool Contains(string root,string path)=>path.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase);
    [DllImport("kernel32.dll",EntryPoint="CreateFileW",CharSet=CharSet.Unicode,SetLastError=true)]
    private static extern SafeFileHandle CreateFile(string path,uint access,uint share,IntPtr security,uint disposition,uint flags,IntPtr template);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]
    [return:MarshalAs(UnmanagedType.Bool)]private static extern bool GetFileInformationByHandle(SafeFileHandle handle,out Information info);
    [DllImport("kernel32.dll",EntryPoint="GetFinalPathNameByHandleW",CharSet=CharSet.Unicode,SetLastError=true)]
    private static extern uint GetFinalPathNameByHandle(SafeFileHandle handle,StringBuilder path,uint capacity,uint flags);
}
