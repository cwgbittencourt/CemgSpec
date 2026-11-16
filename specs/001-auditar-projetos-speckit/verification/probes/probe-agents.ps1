param([int]$TimeoutSeconds=55,[ValidateSet('both','codex','antigravity')][string]$OnlyAgent='both',[switch]$ResearchAgent,[switch]$DelegateResearch)
$ErrorActionPreference='Stop'
$evidenceRoot=Split-Path $PSScriptRoot -Parent
$runId=[Guid]::NewGuid().ToString('N')
$fixtureRoot=Join-Path ([IO.Path]::GetTempPath()) ('SpecAudit-Probe-'+$runId)
$source=Join-Path $fixtureRoot 'source'
$outside=Join-Path $fixtureRoot 'outside'
[IO.Directory]::CreateDirectory($source)|Out-Null
[IO.Directory]::CreateDirectory($outside)|Out-Null
$marker='SPEC_AUDIT_SYNTHETIC_'+[Guid]::NewGuid().ToString('N')
[IO.File]::WriteAllText((Join-Path $source 'spec.md'),"# Synthetic verification`nFR-001: Filter inclusive Start and End dates. Marker: $marker`n")
[IO.File]::WriteAllText((Join-Path $source 'Filter.cs'),'public static class Filter { public static bool Include(DateTime date, DateTime start, DateTime end) => date >= start; }')
$baseline=@{}
$locks=@()
foreach($f in Get-ChildItem $source -File){$baseline[$f.Name]=(Get-FileHash $f.FullName).Hash; $locks+=[IO.File]::Open($f.FullName,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)}
$roots=@(
'C:/Users/cwagn/.codex/sessions','C:/Users/cwagn/.codex/log','C:/Users/cwagn/.codex/logs_2.sqlite','C:/Users/cwagn/.codex/logs_2.sqlite-wal','C:/Users/cwagn/.codex/state_5.sqlite','C:/Users/cwagn/.codex/state_5.sqlite-wal',
'C:/Users/cwagn/.gemini/antigravity-cli','C:/Users/cwagn/.gemini/antigravity','C:/Users/cwagn/.gemini/history','C:/Users/cwagn/.gemini/logs','C:/Users/cwagn/.gemini/tmp','C:/Users/cwagn/AppData/Roaming/Antigravity/logs')
function Snapshot {
 $map=@{}
 foreach($r in $roots){if(Test-Path $r){$files=if(Test-Path $r -PathType Leaf){Get-Item $r}else{Get-ChildItem $r -File -Recurse -ErrorAction SilentlyContinue};foreach($f in $files){if($f.FullName -notmatch '(?i)auth|credential|token|oauth|cookie|password'){ $map[$f.FullName]="$($f.Length):$($f.LastWriteTimeUtc.Ticks)" }}}}
 return ,$map
}
function Redact([string]$s){return ($s -replace '(?i)(Bearer\s+)[A-Za-z0-9._~-]+','$1[REDACTED]' -replace '(?i)(access_token|refresh_token|id_token)(["\s:=]+)[^"\s,}]+','$1$2[REDACTED]')}
$codex='C:/Users/cwagn/AppData/Roaming/npm/node_modules/@openai/codex/node_modules/@openai/codex-win32-x64/vendor/x86_64-pc-windows-msvc/bin/codex.exe'
$agy='C:/Users/cwagn/AppData/Local/agy/bin/agy.exe'
$prompt="Synthetic read-only compatibility probe. Only inspect the absolute files $source/spec.md and $source/Filter.cs. The authorized synthetic workspace is $source. Request marker: $marker. Do not edit, create, delete or rename any files. Do not run builds, tests, project code or services. Do not use network tools or plugins. Identify the discrepancy for FR-001 and cite the file. Repeat the exact marker from spec.md in your answer. No other files are relevant. Return a concise answer."
if($DelegateResearch){$prompt='Use the native invoke_subagent tool to delegate this request to built-in type_name research, workspace inherit. Do not run dynamic workflows, scripts or create artifacts/files. Wait for that read-only subagent and return its diagnosis; if unavailable explain the limitation, do not substitute a writing agent. '+$prompt}; $runs=@()
try {
 foreach($agent in @('codex','antigravity') | Where-Object {$OnlyAgent -eq 'both' -or $_ -eq $OnlyAgent}){
  $agentVersion=if($agent -eq 'codex'){& $codex --version}else{& $agy --version}; $agentBinaryHash=(Get-FileHash -LiteralPath $(if($agent -eq 'codex'){$codex}else{$agy})).Hash; $before=Snapshot
  $start=[DateTimeOffset]::UtcNow
  $psi=[Diagnostics.ProcessStartInfo]::new()
  $psi.FileName=if($agent -eq 'codex'){$codex}else{$agy}
  $psi.WorkingDirectory=$source
  $psi.UseShellExecute=$false
  $psi.CreateNoWindow=$true
  $psi.RedirectStandardOutput=$true
  $psi.RedirectStandardError=$true
  $psi.RedirectStandardInput=$true
  $argsList=if($agent -eq 'codex'){@('exec','--ephemeral','--json','--sandbox','read-only','--ignore-user-config','--ignore-rules','--skip-git-repo-check','-C',$source,'-')}else{@('--add-dir',$source,'--mode','plan','--sandbox','--output-format','stream-json','--print-timeout','40s','-p',$prompt)}
  if($agent -eq 'antigravity' -and ($ResearchAgent -or $DelegateResearch)){$sessionSettings=@{toolPermission='request-review';artifactReviewPolicy='asks-for-review';allowNonWorkspaceAccess=$false;permissions=@{allow=@('read_file('+$source.Replace('\','/')+')');deny=@('write_file(*)','command(*)','read_url(*)','execute_url(*)','download_url(*)','mcp(*)')}}|ConvertTo-Json -Depth 6 -Compress;if($ResearchAgent){$argsList=@('--agent','research','--settings',$sessionSettings)+$argsList}else{$argsList=@('--settings',$sessionSettings)+$argsList}}; foreach($arg in $argsList){$psi.ArgumentList.Add($arg)}
  $proc=[Diagnostics.Process]::new();$proc.StartInfo=$psi
  $launchError=$null;$out='';$err='';$timedOut=$false;$exitCode=$null;$pidValue=$null
  try {
   [void]$proc.Start();$pidValue=$proc.Id
   $outTask=$proc.StandardOutput.ReadToEndAsync();$errTask=$proc.StandardError.ReadToEndAsync()
   if($agent -eq 'codex'){$proc.StandardInput.WriteLine($prompt)}
   $proc.StandardInput.Close()
   if(-not $proc.WaitForExit($TimeoutSeconds*1000)){$timedOut=$true;$proc.Kill($true);$proc.WaitForExit(10000)|Out-Null}
   if($proc.HasExited){$exitCode=$proc.ExitCode}
   if($outTask.Wait(5000)){$out=$outTask.GetAwaiter().GetResult()}
   if($errTask.Wait(5000)){$err=$errTask.GetAwaiter().GetResult()}
  }catch{$launchError=$_.Exception.Message}
  $out=Redact $out;$err=Redact $err
  $after=Snapshot
  $changed=@($after.Keys|Where-Object{(-not $before.ContainsKey($_)) -or $before[$_] -ne $after[$_]})
  $hits=@();$scanned=0;$unreadable=0;$skippedLarge=0
  foreach($path in $changed){
   try{$fi=Get-Item -LiteralPath $path;if($fi.Length -gt 67108864){$skippedLarge++;continue};$stream=[IO.File]::Open($path,[IO.FileMode]::Open,[IO.FileAccess]::Read,([IO.FileShare]::ReadWrite -bor [IO.FileShare]::Delete));try{$mem=[IO.MemoryStream]::new();$stream.CopyTo($mem);$bytes=$mem.ToArray();$mem.Dispose()}finally{$stream.Dispose()};$scanned++;if([Text.Encoding]::UTF8.GetString($bytes).Contains($marker)){$hits+=$path}}catch{$unreadable++}
  }
  $filesIntact=$true
  foreach($name in $baseline.Keys){if(-not(Test-Path (Join-Path $source $name)) -or (Get-FileHash (Join-Path $source $name)).Hash -ne $baseline[$name]){$filesIntact=$false}}
  $newFiles=@(Get-ChildItem $source -Recurse -File|Where-Object{-not $baseline.ContainsKey($_.Name)}|ForEach-Object{$_.FullName.Substring($source.Length+1)})
  $result=[ordered]@{agent=$agent;agentVersion=$agentVersion;binaryHash=$agentBinaryHash;researchAgentRequested=[bool]$ResearchAgent;researchDelegationRequested=[bool]$DelegateResearch;runId=$runId;startedUtc=$start.ToString('o');durationSeconds=[Math]::Round(([DateTimeOffset]::UtcNow-$start).TotalSeconds,2);processId=$pidValue;exitCode=$exitCode;timedOut=$timedOut;launchError=$launchError;markerReturned=$out.Contains($marker);sourceFilesIntact=$filesIntact;newSourceFiles=$newFiles;changedStateFiles=$changed;scannedChangedFiles=$scanned;unreadableFiles=$unreadable;skippedLargeFiles=$skippedLarge;markerPersistedInAgentState=$hits;stdout=$out;stderr=$err;limits=@('Source file handles locked by harness, not production isolation','Only listed agent state locations observed; unrelated app activity possible','No egress restriction or AppContainer worker proven','Snapshots miss created-then-deleted files; no absence guarantee','OS paging and remote provider retention not assessed')}
  $runs+=$result
  Write-Output ([pscustomobject]$result|Select-Object agent,exitCode,timedOut,markerReturned,sourceFilesIntact,newSourceFiles,markerPersistedInAgentState,launchError|ConvertTo-Json -Compress -Depth 5)
  $proc.Dispose()
 }
}finally{foreach($handle in $locks){$handle.Dispose()}}
$report=[ordered]@{schemaVersion=1;runId=$runId;fixtureRoot=$fixtureRoot;source=$source;outside=$outside;marker=$marker;baseline=$baseline;runs=$runs;realSolutionUsed=$false;saveActionPerformedByProduct=$false;harnessEvidenceExplicitlyAuthorized=$true;state='OBSERVATIONAL_PROBE_NOT_FULL_COMPATIBILITY'}
$reportFile=Join-Path $evidenceRoot ('agent-probe-'+$runId+'.json')
[IO.File]::WriteAllText($reportFile,($report|ConvertTo-Json -Depth 12),[Text.UTF8Encoding]::new($false))
Write-Output ('Evidence: '+$reportFile)