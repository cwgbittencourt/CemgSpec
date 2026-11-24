using System.Runtime.InteropServices;
using System.Text;
using CemgSpec.Application;
namespace CemgSpec.Infrastructure.Documents;
public sealed class NativeFolderPicker : IFolderPicker
{
    public Task<string?> PickAsync(string title)
    {
        if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();
        var result=new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread=new Thread(()=>
        {
            IntPtr display=IntPtr.Zero,selection=IntPtr.Zero;
            try
            {
                display=Marshal.AllocCoTaskMem(520);
                var info=new BrowseInfo{DisplayName=display,Title=title,Flags=0x1|0x10|0x40|0x200};
                selection=SHBrowseForFolder(ref info);
                if(selection==IntPtr.Zero){result.SetResult(null);return;}
                var path=new StringBuilder(32768);
                if(!SHGetPathFromIDListEx(selection,path,(uint)path.Capacity,0))throw new InvalidOperationException("A filesystem folder is required.");
                result.SetResult(path.ToString());
            }
            catch(Exception error){result.SetException(error);}
            finally{if(selection!=IntPtr.Zero)Marshal.FreeCoTaskMem(selection);if(display!=IntPtr.Zero)Marshal.FreeCoTaskMem(display);}
        }){IsBackground=true,Name="CemgSpec folder selection"};
        thread.SetApartmentState(ApartmentState.STA);thread.Start();return result.Task;
    }
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]private struct BrowseInfo
    {public IntPtr Owner,Root,DisplayName;public string Title;public uint Flags;public IntPtr Callback,Parameter;public int Image;}
    [DllImport("shell32.dll",EntryPoint="SHBrowseForFolderW",CharSet=CharSet.Unicode)]private static extern IntPtr SHBrowseForFolder(ref BrowseInfo info);
    [DllImport("shell32.dll",EntryPoint="SHGetPathFromIDListEx",CharSet=CharSet.Unicode)]private static extern bool SHGetPathFromIDListEx(IntPtr item,StringBuilder path,uint length,uint flags);
}
