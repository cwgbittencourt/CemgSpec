param([switch]$PrepareOnly, [ValidateRange(1024,65535)][int]$Port = 5187)
$ErrorActionPreference = 'Stop'
$manualUrl = "http://127.0.0.1:$Port"
$manualRoot = Join-Path ([IO.Path]::GetTempPath()) ('SpecAudit-Manual-' + [guid]::NewGuid().ToString('N'))
foreach ($projectName in @('Projeto-A','Projeto-B')) {
    $originPath = Join-Path $manualRoot $projectName
    New-Item -ItemType Directory -Force (Join-Path $originPath '.specify/memory') | Out-Null
    Set-Content -LiteralPath (Join-Path $originPath '.specify/memory/constitution.md') -Value '# Regras do projeto ficticio' -Encoding utf8
    foreach ($featureName in @('001-exemplo','002-segunda')) {
        $featurePath = Join-Path $originPath "specs/$featureName"
        New-Item -ItemType Directory -Force $featurePath | Out-Null
        Set-Content -LiteralPath (Join-Path $featurePath 'spec.md') -Value '# Especificacao ficticia' -Encoding utf8
        Set-Content -LiteralPath (Join-Path $featurePath 'plan.md') -Value '# Plano ficticio' -Encoding utf8
        if ($featureName -eq '001-exemplo') {
            Set-Content -LiteralPath (Join-Path $featurePath 'tasks.md') -Value '- [ ] T001 Tarefa ficticia' -Encoding utf8
        }
    }
}
foreach ($folder in @('Relatorios-A','Relatorios-A-novo','Relatorios-B')) {
    New-Item -ItemType Directory -Force (Join-Path $manualRoot $folder) | Out-Null
}
# Synthetic pre-existing history; created by this setup, never by the product.
Set-Content -LiteralPath (Join-Path $manualRoot 'Relatorios-A/historico-exemplo.txt') -Value 'Historico ficticio preexistente; preservar.' -Encoding utf8
$baselineRows = @(Get-ChildItem -LiteralPath $manualRoot -File -Recurse | Get-FileHash -Algorithm SHA256 | Select-Object Path,Hash)
$baselineRows | Export-Csv -LiteralPath (Join-Path $manualRoot 'baseline.csv') -NoTypeInformation -Encoding utf8
Write-Host "`nPASTA DO TESTE: $manualRoot"
Write-Host "Projeto A: $(Join-Path $manualRoot 'Projeto-A')"
Write-Host "Destino A: $(Join-Path $manualRoot 'Relatorios-A')"
Write-Host "Projeto B: $(Join-Path $manualRoot 'Projeto-B')"
Write-Host "Destino B: $(Join-Path $manualRoot 'Relatorios-B')"
Write-Host "Novo destino A: $(Join-Path $manualRoot 'Relatorios-A-novo')"
Write-Host "`nAbra $manualUrl quando aparecer 'Now listening on'."
Write-Host 'Mantenha este terminal aberto. Para encerrar a aplicacao: Ctrl+C.'
if ($PrepareOnly) { return }
$repoPath = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
Push-Location $repoPath
try {
    dotnet restore SpecAudit.slnx --locked-mode
    if ($LASTEXITCODE -ne 0) { throw 'Restore falhou. Preserve a mensagem e informe o problema.' }
    dotnet run --project SpecAudit/SpecAudit.csproj --no-restore --no-launch-profile -- --environment Development --urls $manualUrl
    if ($LASTEXITCODE -ne 0) { throw 'Aplicacao encerrou com erro. Preserve a mensagem; nao encerre processos de terceiros.' }
} finally { Pop-Location }
