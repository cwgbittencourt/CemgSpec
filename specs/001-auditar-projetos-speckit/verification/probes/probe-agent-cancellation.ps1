$ErrorActionPreference='Stop'
Add-Type -Path (Join-Path $PSScriptRoot 'BoundaryProbe.cs')
$fixture=Join-Path ([IO.Path]::GetTempPath()) ('SpecAudit-AgentCancel-'+[Guid]::NewGuid().ToString('N'));[IO.Directory]::CreateDirectory($fixture)|Out-Null
[IO.File]::WriteAllText((Join-Path $fixture 'spec.md'),'Synthetic FR-001: start and end inclusive.')
[IO.File]::WriteAllText((Join-Path $fixture 'Filter.cs'),'bool Include(int d,int s,int e)=>d>=s;')
$baseline=@{};foreach($f in Get-ChildItem $fixture -File){$baseline[$f.Name]=(Get-FileHash $f.FullName).Hash}
$codex='C:/Users/cwagn/AppData/Roaming/npm/node_modules/@openai/codex/node_modules/@openai/codex-win32-x64/vendor/x86_64-pc-windows-msvc/bin/codex.exe';$agy='C:/Users/cwagn/AppData/Local/agy/bin/agy.exe'
$results=@()
foreach($agent in @('codex','antigravity')){
 $exe=if($agent -eq 'codex'){$codex}else{$agy};$timeout=if($agent -eq 'codex'){100}else{1500}
 $arguments=if($agent -eq 'codex'){@('exec','--ephemeral','--json','--sandbox','read-only','--ignore-user-config','--ignore-rules','--skip-git-repo-check','-C',$fixture,'Only read spec.md and Filter.cs. Diagnose the missing filter. Do not modify files or execute project code.')}else{@('--mode','plan','--sandbox','--add-dir',$fixture,'--output-format','stream-json','-p',('Only read '+$fixture+'/spec.md and '+$fixture+'/Filter.cs. Diagnose without modifying files, running project code or accessing services.'))}
 $result=[BoundaryProbe]::Run($exe,$arguments,$fixture,$timeout)
 $survivors=@();foreach($id in $result.JobProcessIds){if(Get-Process -Id ([int]$id) -ErrorAction SilentlyContinue){$survivors+=$id}}
 $results+=@{agent=$agent;version=(& $exe --version);timeoutMs=$timeout;result=$result;survivingObservedPids=$survivors}
}
$intact=$true;foreach($name in $baseline.Keys){if((Get-FileHash (Join-Path $fixture $name)).Hash -ne $baseline[$name]){$intact=$false}}
$report=@{timestampUtc=[DateTimeOffset]::UtcNow.ToString('o');fixture=$fixture;sourceIntact=$intact;agents=$results;limits=@('Cancellation during early startup; does not prove long-running analysis cancellation','Processes in pre-existing external services are not members of this new job')}
$path=Join-Path (Split-Path $PSScriptRoot -Parent) 'agent-cancellation-probe.json';[IO.File]::WriteAllText($path,($report|ConvertTo-Json -Depth 10),[Text.UTF8Encoding]::new($false))
foreach($entry in $results){[pscustomobject]@{agent=$entry.agent;timedOut=$entry.result.TimedOut;before=$entry.result.ActiveBeforeClose;after=$entry.result.ActiveAfterTerminate;survivors=$entry.survivingObservedPids;error=$entry.result.Error}|ConvertTo-Json -Compress}