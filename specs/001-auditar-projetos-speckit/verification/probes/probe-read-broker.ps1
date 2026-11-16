$ErrorActionPreference='Stop'
Add-Type -Path (Join-Path $PSScriptRoot 'BoundaryProbe.cs')
$root=Join-Path ([IO.Path]::GetTempPath()) ('SpecAudit-Broker-'+[Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($root)|Out-Null
$path=Join-Path $root 'source.txt';[IO.File]::WriteAllText($path,'synthetic-broker-read')
$baseline=(Get-FileHash $path).Hash
$stream=[IO.File]::Open($path,[IO.FileMode]::Open,[IO.FileAccess]::Read,([IO.FileShare]::ReadWrite -bor [IO.FileShare]::Delete))
try{
 [BoundaryProbe]::BrokerReadHandle=$stream.SafeFileHandle.DangerousGetHandle();if(-not [BoundaryProbe]::SetBrokerHandleInheritance($true)){throw 'Handle inheritance denied'}
 $id=[BoundaryProbe]::BrokerReadHandle.ToInt64()
 $command="`$ProgressPreference='SilentlyContinue';`$r=@{readViaBroker=`$false;writeViaBrokerDenied=`$false;directPathReadDenied=`$false};`$h=[Microsoft.Win32.SafeHandles.SafeFileHandle]::new([IntPtr]$id,`$false);try{`$f=[IO.FileStream]::new(`$h,[IO.FileAccess]::Read);`$reader=[IO.StreamReader]::new(`$f);`$r.readViaBroker=`$reader.ReadToEnd() -eq 'synthetic-broker-read'}catch{};try{`$w=[IO.FileStream]::new(`$h,[IO.FileAccess]::Write);`$w.WriteByte(65);`$w.Flush()}catch{`$r.writeViaBrokerDenied=`$true}finally{if(`$w){try{`$w.Dispose()}catch{};[GC]::SuppressFinalize(`$w)}};try{[IO.File]::ReadAllText('$path')|Out-Null}catch{`$r.directPathReadDenied=`$true};`$r|ConvertTo-Json -Compress"
 $encoded=[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($command))
 $result=[BoundaryProbe]::RunWithContainer('C:/Windows/System32/WindowsPowerShell/v1.0/powershell.exe',@('-NoProfile','-NonInteractive','-EncodedCommand',$encoded),'C:/Windows/System32',15000,$true)
}finally{[BoundaryProbe]::SetBrokerHandleInheritance($false)|Out-Null;[BoundaryProbe]::BrokerReadHandle=[IntPtr]::Zero;$stream.Dispose()}
$report=@{timestampUtc=[DateTimeOffset]::UtcNow.ToString('o');fixture=$root;result=$result;sourceIntact=((Get-FileHash $path).Hash -eq $baseline);sourceAclsChanged=$false;sourceCopyCreated=$false;handlePolicy='Explicit inherited handle list: output pipe + one read-only source handle';limits=@('Agent integration with read broker not implemented','Network boundary still not validated')}
$destination=Join-Path (Split-Path $PSScriptRoot -Parent) 'read-broker-probe.json'
[IO.File]::WriteAllText($destination,($report|ConvertTo-Json -Depth 8),[Text.UTF8Encoding]::new($false));$report|ConvertTo-Json -Depth 7