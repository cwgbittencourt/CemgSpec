using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32.SafeHandles;
namespace SpecAudit.Infrastructure.Documents;
internal static class WindowsReadHandle
{
    private static readonly ConditionalWeakTable<SafeFileHandle, string> PosixPaths = new();

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
        if(!OperatingSystem.IsWindows())return OpenPosix(path,directory);
        var handle=CreateFile(path,directory?0x80u:0x80000000u,7,IntPtr.Zero,3,directory?0x02000000u:0x00200000u,IntPtr.Zero);
        if(handle.IsInvalid){handle.Dispose();throw new DocumentAccessException("Unreadable");}
        return handle;
    }
    public static Information Info(SafeFileHandle handle)
    {
        if(!OperatingSystem.IsWindows())return InfoPosix(handle);
        if(!GetFileInformationByHandle(handle,out var info))throw new DocumentAccessException("Unreadable");return info;
    }
    public static string FinalPath(SafeFileHandle handle)
    {
        if(!OperatingSystem.IsWindows())return PosixPaths.TryGetValue(handle,out var p)?p:throw new DocumentAccessException("Unreadable");
        var buffer=new StringBuilder(32768);var count=GetFinalPathNameByHandle(handle,buffer,(uint)buffer.Capacity,0);
        if(count==0 || count>=buffer.Capacity)throw new DocumentAccessException("Unreadable");
        var value=buffer.ToString();return value.StartsWith("\\\\?\\UNC\\",StringComparison.OrdinalIgnoreCase) ? "\\\\"+value[8..] : value.StartsWith("\\\\?\\",StringComparison.Ordinal) ? value[4..] : value;
    }
    public static bool Contains(string root,string path)=>path.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase);

    private static SafeFileHandle OpenPosix(string path,bool directory)
    {
        var full=Path.GetFullPath(path);
        if(directory)
        {
            var di=new DirectoryInfo(full);
            if(!di.Exists || di.LinkTarget is not null)throw new DocumentAccessException("Unreadable");
        }
        else
        {
            var fi=new FileInfo(full);
            if(!fi.Exists || fi.LinkTarget is not null)throw new DocumentAccessException("Unreadable");
        }
        var handle=new SafeFileHandle(new IntPtr(1),ownsHandle:false);
        PosixPaths.AddOrUpdate(handle,full);
        return handle;
    }
    private static Information InfoPosix(SafeFileHandle handle)
    {
        if(!PosixPaths.TryGetValue(handle,out var full))throw new DocumentAccessException("Unreadable");
        var isDir=Directory.Exists(full);
        if(!isDir && !File.Exists(full))throw new DocumentAccessException("Unreadable");
        var hash=SHA256.HashData(Encoding.UTF8.GetBytes(full));
        var len=isDir?0L:new FileInfo(full).Length;
        var ft=isDir?Directory.GetLastWriteTimeUtc(full).ToFileTimeUtc():File.GetLastWriteTimeUtc(full).ToFileTimeUtc();
        return new Information
        {
            Volume=BitConverter.ToUInt32(hash,0),
            IndexHigh=BitConverter.ToUInt32(hash,4),
            IndexLow=BitConverter.ToUInt32(hash,8),
            Links=1,
            SizeHigh=(uint)(len>>32),
            SizeLow=(uint)(len&0xFFFFFFFF),
            Written=new System.Runtime.InteropServices.ComTypes.FILETIME{dwHighDateTime=(int)(ft>>32),dwLowDateTime=(int)(ft&0xFFFFFFFF)}
        };
    }
    [DllImport("kernel32.dll",EntryPoint="CreateFileW",CharSet=CharSet.Unicode,SetLastError=true)]
    private static extern SafeFileHandle CreateFile(string path,uint access,uint share,IntPtr security,uint disposition,uint flags,IntPtr template);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]
    [return:MarshalAs(UnmanagedType.Bool)]private static extern bool GetFileInformationByHandle(SafeFileHandle handle,out Information info);
    [DllImport("kernel32.dll",EntryPoint="GetFinalPathNameByHandleW",CharSet=CharSet.Unicode,SetLastError=true)]
    private static extern uint GetFinalPathNameByHandle(SafeFileHandle handle,StringBuilder path,uint capacity,uint flags);
}
