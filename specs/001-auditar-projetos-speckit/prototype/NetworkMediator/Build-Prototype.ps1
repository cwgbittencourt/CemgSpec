[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
# Uses existing build tools only; never installs or elevates.
$toolRoot='C:\Program Files\Microsoft Visual Studio\18\Community\VC\Tools\MSVC\14.51.36231'
$sdkRoot='C:\Program Files (x86)\Windows Kits\10'
$sdkVersion='10.0.26100.0'
$compiler=Join-Path $toolRoot 'bin\Hostx64\x64\cl.exe'
$output=Join-Path $PSScriptRoot 'bin\Release\net10.0'
$objects=Join-Path $PSScriptRoot 'obj\native'
New-Item -ItemType Directory -Force $output,$objects | Out-Null
& 'C:\Program Files\dotnet\dotnet.exe' build (Join-Path $PSScriptRoot 'NetworkMediator.csproj') -c Release --nologo
if($LASTEXITCODE -ne 0){throw 'Managed build failed.'}
$includes=@((Join-Path $toolRoot 'include'))
foreach($part in @('shared','um','ucrt')){$includes+=Join-Path $sdkRoot "Include\$sdkVersion\$part"}
$arguments=@('/nologo','/W4','/WX','/O2','/MT','/DUNICODE','/D_UNICODE','/guard:cf')
foreach($include in $includes){$arguments+="/I$include"}
$arguments+=@((Join-Path $PSScriptRoot 'SyntheticWorker.c'),"/Fo$(Join-Path $objects 'SyntheticWorker.obj')","/Fe$(Join-Path $output 'SyntheticWorker.exe')",'/link',"/LIBPATH:$(Join-Path $toolRoot 'lib\x64')","/LIBPATH:$(Join-Path $sdkRoot "Lib\$sdkVersion\um\x64")","/LIBPATH:$(Join-Path $sdkRoot "Lib\$sdkVersion\ucrt\x64")",'ws2_32.lib','advapi32.lib','/DYNAMICBASE','/NXCOMPAT','/GUARD:CF')
& $compiler @arguments
if($LASTEXITCODE -ne 0){throw 'Native build failed.'}
& (Join-Path $toolRoot 'bin\Hostx64\x64\dumpbin.exe') /dependents (Join-Path $output 'SyntheticWorker.exe')
if($LASTEXITCODE -ne 0){throw 'Import inspection failed.'}
