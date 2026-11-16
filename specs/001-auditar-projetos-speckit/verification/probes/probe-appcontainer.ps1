$ErrorActionPreference='Stop'
Add-Type -Path (Join-Path $PSScriptRoot 'BoundaryProbe.cs')
$root=Join-Path ([IO.Path]::GetTempPath()) ('SpecAudit-AppContainer-'+[Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($root)|Out-Null
$path=Join-Path $root 'source.txt';[IO.File]::WriteAllText($path,'synthetic-source')
$baseline=(Get-FileHash $path).Hash
$listener=[Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback,0);$listener.Start();$port=$listener.LocalEndpoint.Port
try{
 $control=[Net.Sockets.TcpClient]::new();$control.Connect('127.0.0.1',$port);$accepted=$listener.AcceptTcpClient();$accepted.Dispose();$control.Dispose()
 $command="`$ProgressPreference='SilentlyContinue';`$r=@{networkConnected=`$false;networkError='';sourceReadable=`$false;writeDenied=`$false};try{`$c=[Net.Sockets.TcpClient]::new();`$t=`$c.ConnectAsync('127.0.0.1',$port);if(`$t.Wait(3000)){`$r.networkConnected=`$c.Connected};`$c.Dispose()}catch{`$r.networkError=`$_.Exception.Message};try{`$r.sourceReadable=[IO.File]::ReadAllText('$path') -eq 'synthetic-source'}catch{};try{[IO.File]::WriteAllText('$path','SHOULD_NOT_WRITE')}catch{`$r.writeDenied=`$true};`$r|ConvertTo-Json -Compress"
 $encoded=[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($command))
 $system=[BoundaryProbe]::RunWithContainer('C:/Windows/System32/WindowsPowerShell/v1.0/powershell.exe',@('-NoProfile','-NonInteractive','-EncodedCommand',$encoded),'C:/Windows/System32',12000,$true)
}finally{$listener.Stop()}
$agents=@()
foreach($entry in @(@('codex','C:/Users/cwagn/AppData/Roaming/npm/node_modules/@openai/codex/node_modules/@openai/codex-win32-x64/vendor/x86_64-pc-windows-msvc/bin/codex.exe'),@('antigravity','C:/Users/cwagn/AppData/Local/agy/bin/agy.exe'))){
 $result=[BoundaryProbe]::RunWithContainer($entry[1],@('--version'),'C:/Windows/System32',12000,$true)
 $agents+=@{agent=$entry[0];binaryHash=(Get-FileHash $entry[1]).Hash.ToLowerInvariant();versionOutsideContainer=(& $entry[1] --version);result=$result}
}
$report=@{timestampUtc=[DateTimeOffset]::UtcNow.ToString('o');fixture=$root;system=$system;agents=$agents;controlNetworkSucceeded=$true;sourceIntact=((Get-FileHash $path).Hash -eq $baseline);sourceAclsChanged=$false;firewallChanged=$false;capabilitiesGranted=@();limits=@('No provider egress capability granted: not a usable AI network policy','No source read broker implemented in this trial','Agent --version is startup only, not authenticated analysis')}
$evidence=Join-Path (Split-Path $PSScriptRoot -Parent) 'appcontainer-probe.json'
[IO.File]::WriteAllText($evidence,($report|ConvertTo-Json -Depth 10),[Text.UTF8Encoding]::new($false))
$report|ConvertTo-Json -Depth 8