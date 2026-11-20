# Roteiro do DEV — testar Documentos e preview Markdown (US2)

**Estado do roteiro:** caminho automatizado **CONFERIDO_PELA_IA** sobre o commit `62c6e3ca264220ab939ca6a056e9d59ddae8ef71`; percurso manual interativo no navegador com **CONFERENCIA_PARCIAL** (interface exercitada pelo harness Edge/Playwright real; cliques manuais livres pelo DEV pendentes). Execução e aceite do DEV **PENDENTES**. Este roteiro não aprova nem integra PR.

## 1. Pré-requisitos e ambiente

- Windows x64 com desktop, PowerShell 7 (`pwsh`), SDK .NET 10 compatível com `global.json` (validado em `10.0.404` / runtime `10.0.15`) e Microsoft Edge.
- Nenhum agente de IA, credencial, acesso à internet ou projeto real é necessário. Todos os dados são sintéticos e temporários.
- Não executar scripts, builds ou testes dentro de uma solução auditada; os comandos abaixo são exclusivamente do desenvolvimento do próprio `SpecAudit`.

## 2. Repetir a validação automatizada (caminho principal conferido pela IA)

Abra o PowerShell na raiz do repositório `SpecAudit` e execute o bloco abaixo (interrompe em caso de falha):

```powershell
$devRun = Join-Path "$env:LOCALAPPDATA\Temp" ('SpecAudit-US2-DEV-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $devRun -Force | Out-Null
$env:TEMP = "$env:LOCALAPPDATA\Temp"
$env:TMP = $env:TEMP
git rev-parse HEAD
dotnet restore SpecAudit.slnx --locked-mode
if ($LASTEXITCODE -ne 0) { throw 'Restore falhou; preserve a mensagem.' }

# RUN-01: domínio, sanitização e navegação do broker (esperado: 4/4)
dotnet test tests/SpecAudit.Tests/SpecAudit.Tests.csproj --no-restore `
    --filter 'FullyQualifiedName~DocumentNavigationTests|FullyQualifiedName~MarkdownRendererTests' `
    --logger 'trx;LogFilePrefix=US2-domain' --results-directory $devRun 2>&1 | Tee-Object (Join-Path $devRun 'run01.txt')
if ($LASTEXITCODE -ne 0) { throw 'RUN-01 falhou.' }

# RUN-02: US2-S01..S04 no host loopback + Edge real + transporte controlado de imagem remota (esperado: 15/15)
dotnet test tests/SpecAudit.Ui.Tests/SpecAudit.Ui.Tests.csproj --no-restore `
    --filter 'FullyQualifiedName~MarkdownPreviewTests|FullyQualifiedName~PreviewAssetsTests' `
    --logger 'trx;LogFilePrefix=US2-e2e' --results-directory $devRun 2>&1 | Tee-Object (Join-Path $devRun 'run02.txt')
if ($LASTEXITCODE -ne 0) { throw 'RUN-02 falhou.' }

# RUN-03: regressão selecionada (esperado: 75/75 — 11 domínio, 16 Windows, 48 UI)
$filtro = 'FullyQualifiedName!~RealBrowserRendersLocalProjectSelectionWithoutCircuitErrors&FullyQualifiedName!~US1IntegratedHarnessTests'
dotnet test SpecAudit.slnx --no-build --no-restore --filter $filtro `
    --logger 'trx;LogFilePrefix=US2-regression' --results-directory $devRun 2>&1 | Tee-Object (Join-Path $devRun 'run03.txt')
if ($LASTEXITCODE -ne 0) { throw 'RUN-03 falhou.' }

Write-Output "Resultados preservados em: $devRun"
```

Para gerar também as capturas de tela (`documents-preview.png`, `documents-source.png`, `documents-split.png`), defina `$env:SPECAUDIT_CAPTURE_DIR = Join-Path $devRun 'captures'` antes do `RUN-02`.

| Cenário automatizado | Entrada / ação | Resultado esperado |
|---|---|---|
| US2-S01 / AC01 | Busca por `ready.json` e `research.md`; árvore com `001-demo` e `002-other` sem `DoNotRead.cs`; clique em âncora local, referência ausente, referência fora do catálogo (`../../DoNotRead.cs`) e link autorizado `plan.md#entrega` | Apenas artefatos do catálogo visíveis; `:target` destaca `Detalhes` e `Entrega`; avisos distintos para referência ausente e fora do catálogo; origem inalterada e pasta de relatórios vazia. |
| US2-S02 / AC02 | Corpus Markdown em `Preview`, `Fonte` e `Lado a lado` + ciclos repetidos `Fonte` → `Lado a lado` | Títulos, listas, checkboxes desabilitados, tabela, citação, código C# destacado e PNG local via broker; `Fonte` somente leitura (`<pre>` sem inputs/contenteditable) idêntica ao arquivo; `Lado a lado` lado a lado em desktop; âncora continua responsiva sem duplicação de listeners. |
| US2-S03 / AC03 | Mermaid válido (flowchart/sequência), Mermaid inválido, PlantUML e Graphviz offline | Mermaid renderizado em iframe `sandbox="allow-scripts"` com zoom (`Aumentar zoom`) e `Ver fonte`; Mermaid inválido, PlantUML e Graphviz exibem explicação e fonte sem `<svg>`/`<iframe>`; zero requisições externas. |
| US2-S04 / AC04 | HTML básico, `<script>`, `<form>`, imagem remota com cookie configurado no contexto e link externo | `<strong>` preservado; script/formulário removidos e canário inerte; zero requisições automáticas; clique em `Carregar nesta sessão` mediado por `RemotePreviewImages` sem cookies/auth/referer nem redirecionamentos (`PreviewAssetsTests` comprova entrega positiva PNG em servidor loopback controlado e recusa de HTTP 302); clique em `Guia externo` abre nova aba mantendo a aba principal no host local. |

## 3. Conferência manual opcional no navegador

1. Inicie o aplicativo em Development com projeto sintético usando `& .\specs\001-auditar-projetos-speckit\validation\Start-US1Manual.ps1` (ou `dotnet run --project SpecAudit/SpecAudit.csproj --no-launch-profile -- --environment Development --urls http://127.0.0.1:5187`).
2. Abra `http://127.0.0.1:5187`, verifique que **Projetos** aparece ativo na sidebar, carregue **Projeto-A** e **Relatorios-A** e clique em **Documentos** na sidebar.
3. Confira a faixa superior (nome do projeto, selo **Somente leitura** e destino externo), a árvore à esquerda, a busca no topo à direita, os modos **Preview / Fonte / Lado a lado**, o painel **Neste documento** e **Procedência documental**.
4. Compare visualmente com [documentos-sugestoes.png](../design/mockups/documentos-sugestoes.png) e [README dos mockups](../design/mockups/README.md) no tema escuro (tema claro será entregue na US8/T044).
5. Encerre o host com `Ctrl+C`. Para limpeza opcional dos resultados do passo 2, remova manualmente apenas a pasta `$devRun` exibida no terminal.

## Versão e evidências de referência

- Commit validado pela IA: `62c6e3ca264220ab939ca6a056e9d59ddae8ef71`.
- Evidências: [US2-validation.md](../evidence/US2-validation.md), [US2-review.md](../evidence/US2-review.md), [preview-safety.md](../evidence/preview-safety.md), [T022-local-preview.md](../evidence/T022-local-preview.md) e [T021-preview-tests.md](../evidence/T021-preview-tests.md).
- Estado técnico e aceite: [acceptance/US2.md](../acceptance/US2.md).

## Registro humano e aceite

### Registro da execução humana

Preenchimento reservado ao DEV. Os campos abaixo permanecem em branco até execução humana real.

| Cenário / critério | Resultado | Resultado observado / motivo da não execução | Evidências (links) | Commit testado / diff ou manifesto | Responsável | Data/hora e fuso | Defeito / observações |
|---|---|---|---|---|---|---|---|
| US2-S01 / US2-AC01 | | | | | | | |
| US2-S02 / US2-AC02 | | | | | | | |
| US2-S03 / US2-AC03 | | | | | | | |
| US2-S04 / US2-AC04 | | | | | | | |

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
