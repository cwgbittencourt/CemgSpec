[CmdletBinding()]
param([switch]$ExecuteSyntheticProof,[string]$ExpectedManifestSha256)
$ErrorActionPreference='Stop'
$feature=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$manifestPath=Join-Path $feature 'verification/mediator-prototype-manifest.json'
$dotnet='C:\Program Files\dotnet\dotnet.exe'
$binaryRoot=Join-Path $PSScriptRoot 'bin/Release/net10.0'
$dll=Join-Path $binaryRoot 'NetworkMediator.dll'
if(-not $ExecuteSyntheticProof){& $dotnet $dll --self-test;exit $LASTEXITCODE}
$isAdmin=([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if(-not $isAdmin){throw 'Proof requires a separately authorized elevated console. This script never requests elevation.'}
if($ExpectedManifestSha256 -notmatch '^[0-9a-fA-F]{64}$'){throw 'Specify the reviewed full SHA256 of the manifest.'}
if((Get-FileHash -LiteralPath $manifestPath -Algorithm SHA256).Hash -ne $ExpectedManifestSha256){throw 'Manifest changed: review and authorization must be renewed.'}
$manifest=Get-Content -LiteralPath $manifestPath -Raw|ConvertFrom-Json
foreach($required in @('NetworkMediator.dll','NetworkMediator.deps.json','NetworkMediator.runtimeconfig.json')){
 $entry=@($manifest.binaries|Where-Object name -eq $required)
 if($entry.Count -ne 1){throw "Missing/duplicate manifest binary: $required"}
 $binary=Join-Path $binaryRoot $required
 if((Get-FileHash -LiteralPath $binary -Algorithm SHA256).Hash -ne $entry[0].sha256){throw "Binary changed: $required"}
}
foreach($name in @('DOTNET_STARTUP_HOOKS','DOTNET_ADDITIONAL_DEPS','DOTNET_SHARED_STORE','CORECLR_ENABLE_PROFILING','CORECLR_PROFILER','CORECLR_PROFILER_PATH','COR_ENABLE_PROFILING','COR_PROFILER','COR_PROFILER_PATH')){
 if([Environment]::GetEnvironmentVariable($name,'Process')){throw "Runtime extension variable present: $name. Use a reviewed clean console; this script will not alter global environment."}
}
& $dotnet $dll --run-wfp-proof --ack-synthetic-only
exit $LASTEXITCODE