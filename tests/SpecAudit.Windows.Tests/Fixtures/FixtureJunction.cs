using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
namespace SpecAudit.Windows.Tests.Fixtures;
internal static class FixtureJunction
{
    public static void Create(string fixtureRoot,string link,string target)
    {
        var root=Path.GetFullPath(fixtureRoot)+Path.DirectorySeparatorChar;
        link=Path.GetFullPath(link);target=Path.GetFullPath(target);
        if(!link.StartsWith(root,StringComparison.OrdinalIgnoreCase) || !target.StartsWith(root,StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Junction fixture paths must remain inside its exclusive root.");
        if(!Directory.Exists(target))throw new DirectoryNotFoundException(target);
        Directory.CreateDirectory(link);
        var substitute="\\??\\"+target;
        var names=Encoding.Unicode.GetBytes(substitute+'\0'+target+'\0');
        var buffer=new byte[16+names.Length];
        BitConverter.GetBytes(0xA0000003u).CopyTo(buffer,0);
        BitConverter.GetBytes(checked((ushort)(8+names.Length))).CopyTo(buffer,4);
        BitConverter.GetBytes(checked((ushort)(substitute.Length*2))).CopyTo(buffer,10);
        BitConverter.GetBytes(checked((ushort)((substitute.Length+1)*2))).CopyTo(buffer,12);
        BitConverter.GetBytes(checked((ushort)(target.Length*2))).CopyTo(buffer,14);
        names.CopyTo(buffer,16);
        using var handle=CreateFile(link,0x40000000,0,IntPtr.Zero,3,0x02200000,IntPtr.Zero);
        if(handle.IsInvalid || !DeviceIoControl(handle,0x900A4,buffer,buffer.Length,IntPtr.Zero,0,out _,IntPtr.Zero))
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }
    public static void Swap(string fixtureRoot,string link,string target)
    {
        var absolute=Path.GetFullPath(link);
        var root=Path.GetFullPath(fixtureRoot)+Path.DirectorySeparatorChar;
        if(!absolute.StartsWith(root,StringComparison.OrdinalIgnoreCase) ||
           !File.GetAttributes(absolute).HasFlag(FileAttributes.ReparsePoint))throw new InvalidOperationException("Only a fixture junction can be removed.");
        // Remove only the reparse entry; never recurse into its target.
        Directory.Delete(absolute,false);
        Create(fixtureRoot,absolute,target);
    }
    [DllImport("kernel32.dll",EntryPoint="CreateFileW",CharSet=CharSet.Unicode,SetLastError=true)]
    private static extern SafeFileHandle CreateFile(string path,uint access,uint share,IntPtr security,uint disposition,uint flags,IntPtr template);
    [DllImport("kernel32.dll",SetLastError=true)]
    [return:MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeviceIoControl(SafeFileHandle handle,uint code,byte[] input,int inputSize,IntPtr output,int outputSize,out int returned,IntPtr overlapped);
}
