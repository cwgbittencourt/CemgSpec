using System.Diagnostics;
if(args.Length==0)return 64;
if(args[0]=="--echo"){Console.Write(await Console.In.ReadToEndAsync());Console.Error.Write("fixture-stderr");return 0;}
if(args[0]=="--arguments"){Console.Write(System.Text.Json.JsonSerializer.Serialize(args.Skip(1)));return 0;}
if(args[0]=="--profile")
{
 var folders = new[] { "USERPROFILE", "APPDATA", "LOCALAPPDATA", "TEMP", "TMP" };
 Console.Write(System.Text.Json.JsonSerializer.Serialize(folders.Select(Environment.GetEnvironmentVariable)));
 return 0;
}
if(args[0]=="--flood"){Console.Write(new string('x',100000));await Task.Delay(30000);return 0;}
if(args[0]=="--wait" || args[0]=="--child-wait")
{
 if(args[0]=="--wait")
 {
  var start=new ProcessStartInfo(Environment.ProcessPath!){UseShellExecute=false,CreateNoWindow=true};
  if(Path.GetFileNameWithoutExtension(Environment.ProcessPath)=="dotnet")start.ArgumentList.Add(typeof(Program).Assembly.Location);
  start.ArgumentList.Add("--child-wait");var child=Process.Start(start)!;Console.WriteLine("CHILD_PID="+child.Id);Console.Out.Flush();
 }
 await Task.Delay(30000);return 0;
}
return 64;
