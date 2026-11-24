# Roteiro do DEV — testar Dashboard documental (US3)

**Estado do roteiro:** caminho automatizado **CONFERIDO_PELA_IA** sobre o commit `7b52ea290056c25317dc036670734e3086f5588c`; percurso manual interativo no navegador com **CONFERENCIA_PARCIAL** (interface exercitada pelo harness Edge/Playwright real; cliques manuais livres pelo DEV pendentes). Execução e aceite do DEV **PENDENTES**. Este roteiro não aprova nem integra PR.

## 1. Pré-requisitos e ambiente

- Windows x64 com desktop, PowerShell 7 (`pwsh`), SDK .NET 10 compatível com `global.json` (validado em `10.0.404` / runtime `10.0.15`) e Microsoft Edge.
- Nenhum agente de IA, credencial, acesso à internet ou projeto real é necessário. Todos os dados são sintéticos e temporários.
- Não executar scripts, builds ou testes dentro de uma solução auditada; os comandos abaixo são exclusivamente do desenvolvimento do próprio `CemgSpec`.

## 2. Repetir a validação automatizada (caminho principal conferido pela IA)

Abra o PowerShell na raiz do repositório `CemgSpec` e execute o bloco abaixo:

```powershell
$devRun = Join-Path "$env:LOCALAPPDATA\Temp" ('CemgSpec-US3-DEV-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $devRun -Force | Out-Null
$env:TEMP = "$env:LOCALAPPDATA\Temp"
$env:TMP = $env:TEMP
git rev-parse HEAD
dotnet restore CemgSpec.slnx --locked-mode
if ($LASTEXITCODE -ne 0) { throw 'Restore falhou; preserve a mensagem.' }

# RUN-01: domínio, fórmulas, duplicidades e observador do broker (esperado: 5/5)
dotnet test tests/CemgSpec.Tests/CemgSpec.Tests.csproj --no-restore `
    --filter 'FullyQualifiedName~DocumentaryProgressTests' `
    --logger 'trx;LogFilePrefix=US3-domain' --results-directory $devRun 2>&1 | Tee-Object (Join-Path $devRun 'run01.txt')
if ($LASTEXITCODE -ne 0) { throw 'RUN-01 falhou.' }

# RUN-02: US3-S01..S04 no host loopback + Edge real (esperado: 4/4)
dotnet test tests/CemgSpec.Ui.Tests/CemgSpec.Ui.Tests.csproj --no-restore `
    --filter 'FullyQualifiedName~DashboardTests' `
    --logger 'trx;LogFilePrefix=US3-e2e' --results-directory $devRun 2>&1 | Tee-Object (Join-Path $devRun 'run02.txt')
if ($LASTEXITCODE -ne 0) { throw 'RUN-02 falhou.' }

# RUN-03: regressão selecionada (esperado: 84/84 — 16 domínio, 16 Windows, 52 UI)
$filtro = 'FullyQualifiedName!~RealBrowserRendersLocalProjectSelectionWithoutCircuitErrors&FullyQualifiedName!~US1IntegratedHarnessTests'
dotnet test CemgSpec.slnx --no-build --no-restore --filter $filtro `
    --logger 'trx;LogFilePrefix=US3-regression' --results-directory $devRun 2>&1 | Tee-Object (Join-Path $devRun 'run03.txt')
if ($LASTEXITCODE -ne 0) { throw 'RUN-03 falhou.' }

Write-Output "Resultados preservados em: $devRun"
```

Para gerar também as capturas (`dashboard-demo.png` e `dashboard-empty.png`), defina `$env:CEMGSPEC_CAPTURE_DIR = Join-Path $devRun 'captures'` antes do `RUN-02`.

| Cenário automatizado | Entrada / ação | Resultado esperado |
|---|---|---|
| US3-S01 / AC01 | `001-demo` (30/40 tarefas, 12/20 requisitos vinculados) e escopo vazio `003-empty` | `75%` (`30 / 40`) declarado e `60%` (`12 / 20`) de cobertura documental; `003-empty` exibe `Sem itens` sem percentual; ausência de `Implementação evidenciada` e `Validação aprovada`. |
| US3-S02 / AC02 | Registros `ready.json`, `blocked.json`, `bad-hash.json`, `wrong-revision.json`, `code-reference.json` | Estados e revisão exibidos com fonte e aviso explícito de que `READY` não comprova funcionamento, testes ou aceite humano. |
| US3-S03 / AC03 | Impedimentos registrados, requisitos sem tarefa (`FR-013`–`FR-020`), tarefas sem requisito (`T013`–`T040`), duplicidade e referência a `DoNotRead.cs` | Ocorrências listadas com fonte e gravidade; referência a `.cs` marcada `não verificada nesta versão` e observador do broker comprova que `DoNotRead.cs` nunca é aberto. |
| US3-S04 / AC04 | Filtro por `Gravidade` (`Alta`/`Todas`) e edição externa em `tasks.md` marcando `T031` | Indicadores anteriores (`30 / 40`) permanecem visíveis marcados como `Desatualizado` até clicar em `Atualizar documentos`; após o clique, atualiza para `31 / 40` sem executar agente ou gate, preservando a edição externa e mantendo a pasta de relatórios vazia. |

## 3. Conferência manual opcional no navegador

1. Inicie a aplicação com `& .\specs\001-auditar-projetos-speckit\validation\Start-US1Manual.ps1` (ou `dotnet run --project CemgSpec/CemgSpec.csproj --no-launch-profile -- --environment Development --urls http://127.0.0.1:5187`).
2. Abra `http://127.0.0.1:5187`, carregue **Projeto-A** e **Relatorios-A** e clique em **Dashboard** na sidebar.
3. Confira os cards de indicadores, filtros, tabela de requisitos, ocorrências documentais, estados de gate registrados e limites da V1 contra [dashboard-requisito.png](../design/mockups/dashboard-requisito.png) e [README dos mockups](../design/mockups/README.md) no tema escuro (tema claro será entregue na US8/T044).
4. Encerre o host com `Ctrl+C`. Para limpeza opcional, remova apenas a pasta `$devRun`.

## Versão e evidências de referência

- Commit validado pela IA: `7b52ea290056c25317dc036670734e3086f5588c`.
- Evidências: [US3-validation.md](../evidence/US3-validation.md), [US3-review.md](../evidence/US3-review.md) e [T029-dashboard-tests.md](../evidence/T029-dashboard-tests.md).
- Estado técnico e aceite: [acceptance/US3.md](../acceptance/US3.md).

## Registro humano e aceite

### Registro da execução humana

Preenchimento reservado ao DEV. Os campos abaixo permanecem em branco até execução humana real.

| Cenário / critério | Resultado | Resultado observado / motivo da não execução | Evidências (links) | Commit testado / diff ou manifesto | Responsável | Data/hora e fuso | Defeito / observações |
|---|---|---|---|---|---|---|---|
| US3-S01 / US3-AC01 | | | | | | | |
| US3-S02 / US3-AC02 | | | | | | | |
| US3-S03 / US3-AC03 | | | | | | | |
| US3-S04 / US3-AC04 | | | | | | | |

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
