# Roteiro do DEV — testar Ficha documental do requisito (US4)

**Estado do roteiro:** caminho automatizado **CONFERIDO_PELA_IA** sobre o commit `6d1ebf9a3a40017d04eeef7f87eb47626387a873`; percurso manual interativo no navegador com **CONFERENCIA_PARCIAL** (interface exercitada pelo harness Edge/Playwright real; cliques manuais livres pelo DEV pendentes). Execução e aceite do DEV **PENDENTES**. Este roteiro não aprova nem integra PR.

## 1. Pré-requisitos e ambiente

- Windows x64 com desktop, PowerShell 7 (`pwsh`), SDK .NET 10 compatível com `global.json` (validado em `10.0.404` / runtime `10.0.15`) e Microsoft Edge.
- Nenhum agente de IA, credencial, acesso à internet ou projeto real é necessário. Todos os dados são sintéticos e temporários.

## 2. Repetir a validação automatizada (caminho principal conferido pela IA)

Abra o PowerShell na raiz do repositório `CemgSpec` e execute o bloco abaixo:

```powershell
$devRun = Join-Path "$env:LOCALAPPDATA\Temp" ('CemgSpec-US4-DEV-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $devRun -Force | Out-Null
$env:TEMP = "$env:LOCALAPPDATA\Temp"
$env:TMP = $env:TEMP
git rev-parse HEAD
dotnet restore CemgSpec.slnx --locked-mode
if ($LASTEXITCODE -ne 0) { throw 'Restore falhou; preserve a mensagem.' }

# RUN-01: domínio, chaves contextuais, duplicidades e observador do broker (esperado: 4/4)
dotnet test tests/CemgSpec.Tests/CemgSpec.Tests.csproj --no-restore `
    --filter 'FullyQualifiedName~RequirementTraceTests' `
    --logger 'trx;LogFilePrefix=US4-domain' --results-directory $devRun 2>&1 | Tee-Object (Join-Path $devRun 'run01.txt')
if ($LASTEXITCODE -ne 0) { throw 'RUN-01 falhou.' }

# RUN-02: US4-S01..S04 no host loopback + Edge real (esperado: 4/4)
dotnet test tests/CemgSpec.Ui.Tests/CemgSpec.Ui.Tests.csproj --no-restore `
    --filter 'FullyQualifiedName~RequirementSheetTests' `
    --logger 'trx;LogFilePrefix=US4-e2e' --results-directory $devRun 2>&1 | Tee-Object (Join-Path $devRun 'run02.txt')
if ($LASTEXITCODE -ne 0) { throw 'RUN-02 falhou.' }

# RUN-03: regressão selecionada (esperado: 93/93 — 20 domínio, 16 Windows, 57 UI)
$filtro = 'FullyQualifiedName!~RealBrowserRendersLocalProjectSelectionWithoutCircuitErrors&FullyQualifiedName!~US1IntegratedHarnessTests'
dotnet test CemgSpec.slnx --no-build --no-restore --filter $filtro `
    --logger 'trx;LogFilePrefix=US4-regression' --results-directory $devRun 2>&1 | Tee-Object (Join-Path $devRun 'run03.txt')
if ($LASTEXITCODE -ne 0) { throw 'RUN-03 falhou.' }

Write-Output "Resultados preservados em: $devRun"
```

Para gerar também as capturas (`requirements-fr001.png` e `requirements-other-fr001.png`), defina `$env:CEMGSPEC_CAPTURE_DIR = Join-Path $devRun 'captures'` antes do `RUN-02`.

| Cenário automatizado | Entrada / ação | Resultado esperado |
|---|---|---|
| US4-S01 / AC01 | `FR-001` em `001-demo` e `002-other` com vínculos explícitos e inferidos | Chaves contextuais independentes sem mesclagem; badges `Explícito` vs `Inferido`; ausência de `Evidências da implementação`, `npm test` e `Cópia isolada`. |
| US4-S02 / AC02 | Critério sem tarefa (`FR-013`) e duplicidade na mesma funcionalidade | `CoverageState` restrito a `Linked`/`MissingLink`/`Ambiguous`; `Evidência documental insuficiente` sem `Não implementado`; duplicidades preservadas como `Ambíguo`. |
| US4-S03 / AC03 | Registros de gate/validação em `verification/*.json` | Exibidos como `registro de terceiros` com fonte, revisão e aviso de que o CemgSpec não aprova requisito, teste, PR ou história. |
| US4-S04 / AC04 | Referência documental a `../../DoNotRead.cs` em `plan.md` | Exibida como `Não verificada (fora do catálogo)` com nota de `capacidade futura`; observador do broker comprova que `DoNotRead.cs` nunca é aberto. |

## 3. Conferência manual opcional no navegador

1. Inicie a aplicação com `& .\specs\001-auditar-projetos-speckit\validation\Start-US1Manual.ps1` (ou `dotnet run --project CemgSpec/CemgSpec.csproj --no-launch-profile -- --environment Development --urls http://127.0.0.1:5187`).
2. Abra `http://127.0.0.1:5187`, carregue um projeto e clique em **Requisitos** na sidebar.
3. Alterne funcionalidades e requisitos, navegue pelas abas (`Resumo`, `Vínculos`, `Verificações`, `Sugestões`) e compare com a metade inferior de [dashboard-requisito.png](../design/mockups/dashboard-requisito.png) e [README dos mockups](../design/mockups/README.md) no tema escuro.
4. Encerre o host com `Ctrl+C`. Para limpeza opcional, remova apenas a pasta `$devRun`.

## Versão e evidências de referência

- Commit validado pela IA: `6d1ebf9a3a40017d04eeef7f87eb47626387a873`.
- Evidências: [US4-validation.md](../evidence/US4-validation.md), [US4-review.md](../evidence/US4-review.md) e [T036-requirement-tests.md](../evidence/T036-requirement-tests.md).
- Estado técnico e aceite: [acceptance/US4.md](../acceptance/US4.md).

## Registro humano e aceite

### Registro da execução humana

Preenchimento reservado ao DEV. Os campos abaixo permanecem em branco até execução humana real.

| Cenário / critério | Resultado | Resultado observado / motivo da não execução | Evidências (links) | Commit testado / diff ou manifesto | Responsável | Data/hora e fuso | Defeito / observações |
|---|---|---|---|---|---|---|---|
| US4-S01 / US4-AC01 | | | | | | | |
| US4-S02 / US4-AC02 | | | | | | | |
| US4-S03 / US4-AC03 | | | | | | | |
| US4-S04 / US4-AC04 | | | | | | | |

### Retestes e novas rodadas

| Rodada | Cenário / critério | Resultado | Resultado observado / motivo da não execução | Evidências (links) | Commit testado / diff ou manifesto | Responsável | Data/hora e fuso | Defeito / observações |
|---|---|---|---|---|---|---|---|---|
| | | | | | | | | |

### Decisão de aceite

Preenchimento reservado ao DEV. Selecionar explicitamente **ACEITO**, **REJEITADO** ou **ACEITO_COM_RESSALVAS**. Campos em branco significam decisão pendente.

| Campo | Registro do DEV |
|---|---|
| Decisão explícita | |
| Responsável pela decisão | |
| Data/hora e fuso | |
| Commit efetivamente validado / diff ou manifesto | |
| Rodada e referência à execução humana / evidências | |
| Justificativa e observações | |
| Escopo aceito ou rejeitado | |
| Ressalvas, critérios não atendidos e riscos, se aplicável | |
| Ações posteriores e responsáveis, se aplicável | |
