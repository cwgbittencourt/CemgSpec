param(
    [switch]$InstallLocally
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $PSScriptRoot "dist\SpecAudit-win-x64"
$zipPath = Join-Path $PSScriptRoot "dist\SpecAudit-Installer-win-x64.zip"

if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null

Write-Host "1/3 Publicando SpecAudit (win-x64, Self-Contained, Release)..." -ForegroundColor Cyan
dotnet publish (Join-Path $repoRoot "SpecAudit\SpecAudit.csproj") `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -o $publishDir `
    --nologo

# Copy docs guide into the distribution
$docsOut = Join-Path $publishDir "docs"
New-Item -ItemType Directory -Force -Path $docsOut | Out-Null
foreach ($doc in @("GUIA_DE_USO_TELAS.md", "apresentacao-specaudit.html", "apresentacao-specaudit.pdf")) {
    $srcDoc = Join-Path $repoRoot "docs\$doc"
    if (Test-Path $srcDoc) { Copy-Item $srcDoc $docsOut -Force }
}

# Create Install-SpecAudit.ps1 inside the package
$installPs1 = @'
$ErrorActionPreference = "Stop"
$targetDir = Join-Path $env:LOCALAPPDATA "Programs\SpecAudit"
Write-Host "Instalando SpecAudit em $targetDir ..." -ForegroundColor Cyan

Get-Process -Name "SpecAudit" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 400

New-Item -ItemType Directory -Force -Path $targetDir | Out-Null
Copy-Item -Path (Join-Path $PSScriptRoot "*") -Destination $targetDir -Recurse -Force

$exePath = Join-Path $targetDir "SpecAudit.exe"
$wsh = New-Object -ComObject WScript.Shell

$desktopLnk = Join-Path ([Environment]::GetFolderPath("Desktop")) "SpecAudit.lnk"
$sc1 = $wsh.CreateShortcut($desktopLnk)
$sc1.TargetPath = $exePath
$sc1.Arguments = "--open-browser"
$sc1.WorkingDirectory = $targetDir
$sc1.Description = "SpecAudit - Auditoria Documental SpecKit"
$sc1.Save()

$startMenuDir = Join-Path ([Environment]::GetFolderPath("Programs")) "SpecAudit"
New-Item -ItemType Directory -Force -Path $startMenuDir | Out-Null
$startLnk = Join-Path $startMenuDir "SpecAudit.lnk"
$sc2 = $wsh.CreateShortcut($startLnk)
$sc2.TargetPath = $exePath
$sc2.Arguments = "--open-browser"
$sc2.WorkingDirectory = $targetDir
$sc2.Description = "SpecAudit - Auditoria Documental SpecKit"
$sc2.Save()

Write-Host ""
Write-Host "SpecAudit instalado com sucesso!" -ForegroundColor Green
Write-Host "- Pasta instalada: $targetDir"
Write-Host "- Atalho criado na Area de Trabalho: $desktopLnk"
Write-Host "- Atalho criado no Menu Iniciar: $startLnk"
'@
[System.IO.File]::WriteAllText((Join-Path $publishDir "Install-SpecAudit.ps1"), $installPs1, (New-Object System.Text.UTF8Encoding($true)))

$installCmd = "@echo off`r`npowershell -NoProfile -ExecutionPolicy Bypass -File `"%~dp0Install-SpecAudit.ps1`"`r`npause`r`n"
[System.IO.File]::WriteAllText((Join-Path $publishDir "Instalar-SpecAudit.cmd"), $installCmd, (New-Object System.Text.UTF8Encoding($false)))

$uninstallPs1 = @'
$ErrorActionPreference = "Stop"
Get-Process -Name "SpecAudit" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 400
$targetDir = Join-Path $env:LOCALAPPDATA "Programs\SpecAudit"
$desktopLnk = Join-Path ([Environment]::GetFolderPath("Desktop")) "SpecAudit.lnk"
$startMenuDir = Join-Path ([Environment]::GetFolderPath("Programs")) "SpecAudit"
if (Test-Path $desktopLnk) { Remove-Item $desktopLnk -Force }
if (Test-Path $startMenuDir) { Remove-Item $startMenuDir -Recurse -Force }
if (Test-Path $targetDir) { Remove-Item $targetDir -Recurse -Force }
Write-Host "SpecAudit desinstalado com sucesso." -ForegroundColor Yellow
'@
[System.IO.File]::WriteAllText((Join-Path $publishDir "Uninstall-SpecAudit.ps1"), $uninstallPs1, (New-Object System.Text.UTF8Encoding($false)))

$uninstallCmd = "@echo off`r`npowershell -NoProfile -ExecutionPolicy Bypass -File `"%~dp0Uninstall-SpecAudit.ps1`"`r`npause`r`n"
[System.IO.File]::WriteAllText((Join-Path $publishDir "Desinstalar-SpecAudit.cmd"), $uninstallCmd, (New-Object System.Text.UTF8Encoding($false)))

Write-Host "2/3 Gerando pacote ZIP distribuível ($zipPath)..." -ForegroundColor Cyan
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -CompressionLevel Optimal

if ($InstallLocally) {
    Write-Host "3/3 Executando instalacao local..." -ForegroundColor Cyan
    & (Join-Path $publishDir "Install-SpecAudit.ps1")
}

Write-Host "Concluido! Pacote pronto em: $zipPath" -ForegroundColor Green
