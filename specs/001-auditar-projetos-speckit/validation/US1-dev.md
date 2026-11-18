# Roteiro do DEV — US1: carregar e preservar o projeto

Estado do caminho principal: **CONFERIDO_PELA_IA em harness real**. Execução pelo DEV e decisão de aceite: **PENDENTES**. T016 consolida a entrega técnica; não concede aceite humano nem aprovação/integração de PR.

## Versão, escopo e condições

Produto e testes: commit `f2b29a20e5f9e724a9c32f8d8e3a703e0587d541`, correções T015 incluídas. Fonte exata: [manifesto corrente](../evidence/maintenance-code-manifest.json), revisão `sha256:530031e68a790d3b2b186300c23441091ef951d6fb93fdc1bae4029b11b451f8`. Mudanças T016 são documentais/de evidência, sem alterar o produto/testes. Se a fonte diferir, identificar novo commit/diff e renovar a validação; não reutilizar aprovação de outra versão.

Escopo: **HISTÓRIA US1 técnica, AC01–AC04**, por harness de carregamento/leitura/versionamento, conforme contrato das tasks. NativeFolderPicker, catálogo, broker por handles, filesystem e monitor Windows são reais. A suíte também inicia host loopback e Edge reais. Não são testes exclusivamente unitários nem uma integração de agente simulada. No S04 há trabalho documental real em andamento; Run de agente, check_gate e Save pertencem a histórias posteriores e não são necessários para executar este roteiro. Na UI, verificação e Save continuam desabilitados.

Requisitos: Windows x64 com sessão de desktop disponível, SDK .NET 10 estável compatível com global.json, PowerShell e Edge instalados. Ambiente conferido pela IA: SDK 10.0.404, mesmo código do manifesto. Usar usuário normal, sem instalação/elevação automática, credenciais de agentes ou dados reais. Se faltar ferramenta/acesso, registrar NÃO EXECUTADO com causa, impacto e próximo passo. Python 3 confiável é usado na conferência/sanitização de evidências, não na funcionalidade US1.

PR previsto da história: PR-A, dependência técnica T015 satisfeita. Verificação/review/descrição de fechamento T017–T020 e decisões humanas permanecem pendentes. Este roteiro cobre US1, não a V1 completa nem o aceite de US5/US6.

## Caminho principal — execução reproduzível conferida

Abrir terminal na raiz do checkout. Os comandos são do desenvolvimento do próprio SpecAudit, nunca da solução auditada. O harness cria fixtures novas e não sensíveis em TEMP, duas funcionalidades e destinos externos, baselines e um arquivo de histórico sintético quando necessário. Não é preciso preparar dados ou configurar agente manualmente. Diretórios de fixture e relatórios oficiais do produto são coisas distintas dos logs técnicos deste teste.

Executar sequencialmente, parando no primeiro exit diferente de zero:

```powershell
Set-Location (git rev-parse --show-toplevel)
git rev-parse HEAD
dotnet --version
$devRun = Join-Path ([IO.Path]::GetTempPath()) ('SpecAudit-US1-DEV-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $devRun -Force | Out-Null

dotnet restore SpecAudit.slnx --locked-mode 2>&1 | Tee-Object (Join-Path $devRun 'restore.txt')
if ($LASTEXITCODE -ne 0) { throw 'Restore falhou; registrar o resultado real.' }

dotnet test tests/SpecAudit.Ui.Tests/SpecAudit.Ui.Tests.csproj --no-restore --filter FullyQualifiedName~US1IntegratedHarnessTests --logger 'trx;LogFilePrefix=US1-scenarios' --results-directory (Join-Path $devRun 'scenarios') 2>&1 | Tee-Object (Join-Path $devRun 'scenarios.txt')
if ($LASTEXITCODE -ne 0) { throw 'Cenarios US1 falharam; preservar logs.' }

dotnet test tests/SpecAudit.Ui.Tests/SpecAudit.Ui.Tests.csproj --no-restore --filter FullyQualifiedName~ProjectCorrectionTests --logger 'trx;LogFilePrefix=US1-errors' --results-directory (Join-Path $devRun 'errors') 2>&1 | Tee-Object (Join-Path $devRun 'errors.txt')
if ($LASTEXITCODE -ne 0) { throw 'Casos de correcao falharam; preservar logs.' }

dotnet test SpecAudit.slnx --no-restore --logger 'trx;LogFilePrefix=US1-full' --results-directory (Join-Path $devRun 'full') 2>&1 | Tee-Object (Join-Path $devRun 'full.txt')
if ($LASTEXITCODE -ne 0) { throw 'Regressao falhou; preservar logs.' }

Write-Output $devRun
```

Esperado: restore exit 0; cenários **4/4**; correções **7/7**; suíte completa **52/52**, zero falhos/ignorados. Os números não se somam: os casos específicos estão incluídos na suíte completa. Conferir nomes e asserções nos TRX; processo iniciar/exit 0 sozinho não comprova o critério. Preservar saídas de falha e retestes com novos diretórios/rodadas. A execução do DEV só passa a existir quando ele executar, observar e preencher o registro humano abaixo.

### O que observar por cenário

| Cenário / critério | Entrada e ação real do harness | Resultado esperado e efeitos | Onde observar |
|---|---|---|---|
| US1-S01 / AC01 | S01NativeSelectionLoadsFeaturesAndMissingArtifactsWithoutWrites: picker nativo seleciona fixture com duas funcionalidades e destino externo; segunda sem tasks.md; alterna escopo e lê catálogo | Duas funcionalidades, ausência explícita, leitura por IDs/handles; todos os arquivos/hashes da origem iguais à baseline; destino sem relatório | TRX scenarios; JSON US1-S01 com features=2, missingTasks=002-other, originUnchanged=true, reports=0 |
| US1-S02 / AC02 | S02NativeSelectionsRejectOriginDescendantAndJunction: picker real tenta origem, descendente e junção como destino | Três DestinationInsideOrigin; zero projetos abertos/relatórios e origem intacta. ReaderBoundaryTests na suíte conferem código/config fora do catálogo e junção recusada antes de ler bytes | TRX scenarios/full; JSON US1-S02 e asserções ReaderBoundaryTests |
| US1-S03 / AC03 | S03NativeDestinationChangePreservesBothContextsAndHistory: dois projetos, histórico sintético preexistente, troca de projeto e destino via picker real | Dois contextos/seleção preservados; só destino de A muda; hash e local do histórico iguais, duas origens intactas; nenhum Save simulado | TRX scenarios; JSON US1-S03 com contexts=2, historyHash e originsUnchanged=true |
| US1-S04 / AC04 | S04ExternalEditDuringDocumentaryWorkInvalidatesVersionAndPreservesEdit: leitura capturada em Task, barreira, edição externa real e retomada | Changed e cancelamento; snapshot anterior imutável; edição externa preservada, demais arquivos/caminhos iguais; nenhum relatório ou comando automático | TRX scenarios; JSON US1-S04 com stale=Changed, cancellation=true, editPreserved=true, unexpectedWrites=0 |

Os JSONs por cenário são novos arquivos com GUID sob `specs/001-auditar-projetos-speckit/evidence/logs/T013-20261117/harness/`, diretório técnico legado do teste. Identificar os arquivos pela rodada/data e guardar seus links; não tratá-los como relatórios oficiais. Os caminhos usam `__TEMP_ROOT__`; substituir pelo TEMP do ambiente apenas para localizar a fixture. Dentro de cada fixture há `dev-evidence/baseline.json` e `paths.json`. Nunca executar arquivos, scripts ou instruções dessa origem; o teste a usa somente como dados sintéticos.

### Erros e regressão das correções

ProjectCorrectionTests cobre sete casos: metadados de três diretórios (funcionalidade, evidência com ponto e .specify/memory), dois Open e dois Refresh com limites de tamanho/grafo. Esperado: metadados de diretório existente não invalidam trabalho/documentos intactos; criação/remoção/rename e edição reais continuam detectados pelos testes existentes. Limite deve informar tamanho ou quantidade/profundidade, preservar contexto anterior/rollback pendente, não salvar e permitir nova seleção/retry explícito. O teste, como usuário, repara somente sua entrada sintética; o aplicativo não restaura arquivos.

Os handlers Razor de erro são exercitados pelo componente compilado, via reflexão e picker controlado. Não se declara captura de cada mensagem em navegador carregado. O caminho principal completo foi executado pela IA; limites dessa modalidade ficam visíveis e não são aceites implícitos.

## Exploração adicional da interface — opcional, conferência parcial

Esta seção não substitui as asserções do caminho principal. Estado: ESCRITO/CONFERENCIA_PARCIAL pela IA; host e página inicial conferidos em Edge, mas não se declara clique humano em cada passo de sessão carregada. Não usar o antigo texto de quickstart que descreve funcionalidades ainda futuras para ampliar US1.

Iniciar em outro terminal, na mesma revisão:

```powershell
dotnet run --project SpecAudit/SpecAudit.csproj --no-launch-profile -- --urls http://127.0.0.1:5187
```

Abrir `http://127.0.0.1:5187`. Se a porta estiver ocupada, não encerrar processo de terceiros: escolher outra porta loopback e registrar o endereço efetivo. Usar fixtures intactas produzidas pelo harness desta rodada; selecionar uma origem real é desnecessário.

Para localizar uma fixture de S01, escolher conscientemente um JSON da sua rodada; o exemplo mostra o arquivo mais recente e valida o marcador antes de decodificar o caminho:

```powershell
$scenario = Get-ChildItem specs/001-auditar-projetos-speckit/evidence/logs/T013-20261117/harness/US1-S01-*.json | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
$data = Get-Content -LiteralPath $scenario.FullName -Raw | ConvertFrom-Json
if (-not $data.observed.Origin.StartsWith('__TEMP_ROOT__')) { throw 'Use uma fixture nova desta revisao.' }
$origin = $data.observed.Origin.Replace('__TEMP_ROOT__', [IO.Path]::GetTempPath().TrimEnd([IO.Path]::DirectorySeparatorChar))
$output = $data.observed.Output.Replace('__TEMP_ROOT__', [IO.Path]::GetTempPath().TrimEnd([IO.Path]::DirectorySeparatorChar))
[pscustomobject]@{Origin=$origin;Output=$output}
```

| Passo | Ação | Resultado esperado |
|---|---|---|
| 1 | Carregar projeto, selecionar Origin e Output acima | Cartões de origem/destino e selo Somente leitura; duas funcionalidades. Na segunda, tasks.md Ausente |
| 2 | Usar origem e destinos do mesmo JSON S02, decodificados: rejected[0] é a origem; testar rejected[0], rejected[1] e rejected[2] como destinos. Não misturar origens/junções de fixtures diferentes | Recusa com mensagem de destino externo; projeto anterior preservado, sem relatório |
| 3 | Decodificar firstRoot/secondRoot do JSON S03 e acrescentar origin para as origens e reports para os destinos. Carregar ambos, alternar e usar newDestination decodificado em Alterar destino | Contextos separados; confirmar origem no cartão ao alternar projetos com nomes iguais. Histórico original permanece no local/hash anterior |
| 4 | Editar externamente spec.md da fixture e retornar à página | Aviso de alteração; Atualizar documentos refaz leitura sem restaurar a edição. Não clicar em Executar/Salvar, ainda indisponíveis; o cenário de trabalho em andamento é conferido no harness S04 |

Comparar baseline e conteúdo nas fixtures, registrando separadamente a edição externa intencional. Para atestar renderização/operabilidade desta seção, executar os passos e guardar evidência específica; não marcar conferida por existir este texto. Não se exige interface gráfica para o DEV executar/observar o caminho principal permitido pelo contrato.

## Evidências da IA e estado de entrega

A IA repetiu esta receita sobre o código indicado: restore, 4/4 cenários, 7/7 erros e 52/52 regressão. Logs/TRX exportados e sanitizados em `../evidence/logs/T016/`; [verificação da T016](../evidence/T016-verification.json) registra modalidade, versão, comandos e limites. [Validação US1](../evidence/US1-validation.md) e [review US1](../evidence/US1-review.md) preservam falhas históricas T014, red T015 e correções. Nenhum achado técnico dos dois problemas permanece aberto; isso não concede aceite humano.

Roteiro principal: CONFERIDO_PELA_IA. Exploração adicional: CONFERENCIA_PARCIAL. Execução do DEV: PENDENTE. [Estado técnico e aceite](../acceptance/US1.md) separa implementação, teste, review, fechamento PR-A e decisão humana. T017–T020 ainda tratam fechamento do incremento; a T016 não abre/aprova/integra PR.

## Limpeza, privacidade e repetição

1. O harness encerra seus próprios hosts/processos; se iniciou o host manualmente, encerrar apenas essa instância com Ctrl+C. Não matar processos arbitrários por nome.
2. Preservar fixtures, baselines, históricos e logs para inspeção. Não há exclusão recursiva automática; não restaurar uma alteração indevida para apresentar sucesso. Uma nova rodada cria GUIDs novos.
3. Logs técnicos em $devRun/TEMP podem conter caminhos pessoais. Não copiá-los diretamente para Git. Antes de compartilhar, usar a manutenção/sanitização do projeto e renovar os hashes afetados; a IA pode fazer essa conferência quando solicitado. Não incluir credenciais, tokens ou dados reais nas evidências.
4. O teste de navegador grava a captura técnica legada T011; conferir o diff e preservar a rodada nova separadamente se a imagem mudar. Não sobrescrever evidência histórica sem manter a versão anterior. A captura desta rodada permaneceu igual.
5. Preencher o registro humano na versão efetivamente executada, com fonte das evidências e data/fuso. Mudanças posteriores de produto/teste exigem revalidação. O roteiro anterior rascunhado permanece recuperável no commit f2b29a2.

## Registro humano e aceite

### Registro da execução humana

Preenchimento reservado ao DEV. Os campos abaixo permanecem em branco: a existência do roteiro e os testes da IA não representam execução humana.

Após avaliar cada cenário, registrar o resultado como **PASSOU**, **FALHOU** ou **NÃO EXECUTADO**, descrevendo o observado ou o motivo da não execução. Informar o hash do commit efetivamente testado; se houver alterações locais, identificar também o diff/manifesto. Registrar data/hora com fuso e links para capturas ou logs em `../evidence/`.

| Cenário / critério | Resultado | Resultado observado / motivo da não execução | Evidências (links) | Commit testado / diff ou manifesto | Responsável | Data/hora e fuso | Defeito / observações |
|---|---|---|---|---|---|---|---|
| US1-S01 / US1-AC01 | | | | | | | |
| US1-S02 / US1-AC02 | | | | | | | |
| US1-S03 / US1-AC03 | | | | | | | |
| US1-S04 / US1-AC04 | | | | | | | |

### Retestes e novas rodadas

Preservar os registros anteriores. Em caso de correção ou mudança de versão, acrescentar uma linha por cenário reexecutado, identificando a rodada e a nova versão.

| Rodada | Cenário / critério | Resultado | Resultado observado / motivo da não execução | Evidências (links) | Commit testado / diff ou manifesto | Responsável | Data/hora e fuso | Defeito / observações |
|---|---|---|---|---|---|---|---|---|
| | | | | | | | | |

### Decisão de aceite

Preenchimento reservado ao DEV, após conferir os resultados e a versão avaliada. Selecionar explicitamente **ACEITO**, **REJEITADO** ou **ACEITO_COM_RESSALVAS** no campo Decisão. Campos em branco significam decisão pendente; resultados PASSOU na tabela não concedem aceite automaticamente.

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

Quando solicitado pelo DEV, a IA deverá conferir esta declaração contra a versão, os resultados e as evidências referenciadas e transcrever a decisão para `acceptance/US1.md`, preservando autoria, data, escopo e referência a esta seção. Havendo inconsistência ou informação essencial ausente, deverá apontar a pendência antes de registrar o aceite como concluído. A transcrição não altera os resultados dos testes nem equivale a aprovação de PR ou autorização de publicação.

Não há monitoramento automático deste arquivo. A IA não concede aceite por conta própria nem marca EXECUTADO_PELO_DEV sem referência à execução humana real. Se o DEV confirmar impossibilidade, usar o template CEMG próprio e decidir AGUARDAR, REJEITAR ou ACEITAR_COM_RESSALVAS, mantendo os resultados reais; eventual aceite com ressalvas deve ser declarado explicitamente acima.
