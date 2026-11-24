# Roteiro reproduzível de validação — CemgSpec V1

**Revisão**: 2026-11-16 | cemg-speckit 3.0.0 | constituição 3.0.0.
**Estado**: ESCRITO e conferido documentalmente; fluxos de produto/proteção NÃO EXECUTADOS nesta etapa. Aplicação ainda é scaffold. Comandos de testes abaixo descrevem projetos/casos a entregar nas tasks, não testes já existentes.

## Escopo e condições

Somente documentos SpecKit, analyze/check_gate na combo, origem sem alterações, Save externo. Testar em fixtures novas não sensíveis, nunca solução real. Não instalar ferramentas ou elevar por este roteiro. Executor e host com usuário normal. Caso dependência falte, informar indisponibilidade e implementar pré-requisito permitido; instalação/elevação exige autorização própria. Provas antigas 0xC0000142 não são aprovação da V1.

Requisitos de desenvolvimento: Windows x64; .NET 10 SDK estável suportado; PowerShell 7 para receita abaixo; navegador local; Python 3 confiável compatível com o core CEMG (inspecionar versão/runtime na implementação); Codex e Antigravity instalados/autenticados apenas para demonstração analyze. Conferir versões e vulnerabilidades antes de fixar novas dependências; nenhum pacote novo nesta revisão. Bibliotecas Markdown/Mermaid/sanitizador empacotadas localmente, sem CDN; suas versões/hashes e auditoria acompanham PR de implementação.

## Inicialização do próprio CemgSpec

No PowerShell, raiz de desenvolvimento:

```powershell
Set-Location '__REPO_ROOT__'
dotnet --info
dotnet restore .\CemgSpec\CemgSpec.csproj
dotnet list .\CemgSpec\CemgSpec.csproj package --vulnerable --include-transitive
dotnet run --project .\CemgSpec\CemgSpec.csproj --no-launch-profile -- --urls http://127.0.0.1:5187
```

Esses comandos são do desenvolvimento do CemgSpec, não da solução carregada. Registrar ferramentas/versões, data e resultado da consulta; sem achados não prova segurança integral, falha de consulta não significa sem vulnerabilidades. Não executar agora como se telas planejadas existissem. Após implementação abrir `http://127.0.0.1:5187`; host/Origin/antiforgery e sessão local precisam passar antes de usar dados. Não expor LAN. Se porta ocupada, encerrar apenas sua instância ou registrar outra porta loopback em todas as evidências.

## Preparar dados sintéticos — ação do DEV, fora da origem real

Executar receita em PowerShell 7 apenas para criar fixtures descartáveis. Cada rodada cria raiz TEMP exclusiva. O produto nunca executa esta receita nem scripts da fixture. O setup escreve a fixture antes da análise; durante análise a origem deve permanecer intacta.

```powershell
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ('CemgSpec-V1-' + [guid]::NewGuid().ToString('N'))
$origin = Join-Path $testRoot 'origin'
$featureDir = Join-Path $origin 'specs/001-demo'
$outputDir = Join-Path $testRoot 'reports'
$evidenceDir = Join-Path $testRoot 'dev-evidence'
$utf8 = [Text.UTF8Encoding]::new($false)
New-Item -ItemType Directory -Force $featureDir,$outputDir,$evidenceDir,(Join-Path $origin '.specify/memory'),(Join-Path $featureDir 'verification') | Out-Null
[IO.File]::WriteAllText((Join-Path $origin '.specify/memory/constitution.md'), '# Demo constitution: documentation only; source is read-only.', $utf8)
$requirements = 1..20 | ForEach-Object { '- FR-{0:D3}: Documentary requirement {0}.' -f $_ }
[IO.File]::WriteAllText((Join-Path $featureDir 'spec.md'), ('# Demo feature' + "`n`n" + ($requirements -join "`n")), $utf8)
[IO.File]::WriteAllText((Join-Path $featureDir 'plan.md'), '# Demo plan: documentary links only.', $utf8)
$taskLines = 1..40 | ForEach-Object { $check = if($_ -le 30){'x'}else{' '}; $link = if($_ -le 12){'FR-{0:D3}' -f $_}else{'unlinked'}; '- [{0}] T{1:D3} Demo task {1} {2}' -f $check,$_,$link }
[IO.File]::WriteAllText((Join-Path $featureDir 'tasks.md'), ('# Tasks' + "`n" + ($taskLines -join "`n")), $utf8)
$specHash = (Get-FileHash -LiteralPath (Join-Path $featureDir 'spec.md') -Algorithm SHA256).Hash.ToLowerInvariant()
$record = [ordered]@{schema_version=1;stage='spec';revision=('sha256:'+$specHash);state='READY';checks=@{criteria_observable=$true;scenarios_defined=$true;scope_defined=$true;dependencies_identified=$true;material_decisions_resolved=$true};blockers=@();human_decisions=@();evidence=@(@{path='spec.md';kind='document';sha256=$specHash})}
[IO.File]::WriteAllText((Join-Path $featureDir 'verification/spec-stage.json'), ($record | ConvertTo-Json -Depth 8), $utf8)
[IO.File]::WriteAllText((Join-Path $origin 'DoNotRead.cs'), '// synthetic source canary; product must not read this file', $utf8)
[IO.File]::WriteAllText((Join-Path $featureDir 'research.md'), "# Preview corpus`n`n## Anchor`n`n- [ ] Read-only task`n`n| A | B |`n|---|---|`n| one | two |`n`n> Quote`n`n[Plan](plan.md)`n`n[Forbidden code](../../DoNotRead.cs)`n`n<script>window.__unsafe=1</script>", $utf8)
$baseline = Get-ChildItem -LiteralPath $origin -Recurse -File | ForEach-Object { [pscustomobject]@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash.ToLowerInvariant()} }
[IO.File]::WriteAllText((Join-Path $evidenceDir 'baseline.json'), ($baseline | ConvertTo-Json -Depth 5), $utf8)
[pscustomobject]@{testRoot=$testRoot;origin=$origin;feature=$featureDir;output=$outputDir;evidence=$evidenceDir;expectedRevision=('sha256:'+$specHash)} | ConvertTo-Json
```

Guardar o objeto de caminhos em evidência técnica. Criar segundo projeto repetindo receita com novo GUID; copiar somente dados sintéticos de preparação não é requisito de cópia isolada de solução do produto. Adicionar manualmente ao corpus Mermaid válido/inválido, PlantUML como fonte, imagem local documental e URL de imagem remota controlada; conteúdo nunca contém segredos reais. Sem endpoint controlado disponível, manter cenário remoto NÃO EXECUTADO, não usar serviço da solução. Preparação não concede execução de scripts pelo produto.

Para cenários negativos gate, antes da análise preparar registros adicionais: cópia com state=BLOCKED + blocker; cópia com hash de evidence incorreto; cópia com revision diferente; cópia com evidence path para DoNotRead.cs; documento test-run cujo stdout aponte para .cs; registro product-execution com artifact fora do catálogo. JSON de fixture deve ser escrito pelo DEV, nunca pelo aplicativo para corrigir pré-requisito. O runner precisa tratar estruturas incompletas como erro explícito, não crash/sucesso.

## Ordem de implementação e habilitação

1. Implementar catálogo/broker e harness Windows do contrato [verification.md](contracts/verification.md). Com origem sintética, tentar acesso/escrita não permitidos por executor/filho, junction trocada, entrada maliciosa e cancelamento. Observar falha real de abertura antes de conteúdo proibido ser lido; hash inalterado sozinho não prova que código não foi lido. Instrumentação de desenvolvimento deve registrar somente caminho/operação/resultado, nunca credenciais. Resultado esperado ReaderBoundary Passed; senão não habilitar origem para executor.
2. Implementar runner gate com Path de capacidade e core/política pinados. Provar equivalência com core oficial em registros controlados, offline, sem --project, sem acesso a código/subprocessos arbitrários; comprovar TOCTOU por barreira de sincronização de teste. GateAdapter só passa se todas as provas aplicáveis passarem. Sem runtime íntegro, botão indisponível.
3. Implementar adaptadores Codex/Antigravity e executar, individualmente, o mesmo corpus via canal restrito. Provar análise documental real, saída/erros, estado exclusivamente externo, ferramentas/rede/segredos bloqueados e término de descendentes. A versão que falhar fica indisponível; ausência de prova não bloqueia planejamento/tarefas, mas bloqueia habilitação/PR dependente e aceite de US5 completo.
4. Implementar writer com provas de Save/destino externo e falhas. Só depois habilitar Save. Sem prova, resultados continuam em memória; não usar origem como fallback.

Essas etapas devem aparecer ANTES das tarefas de habilitação funcional dependentes. Não há alteração constitucional, dispensa de teste, aceite humano ou autorização elevada implícita.

## Execução funcional pelo meio real de uso — após implementar

Abrir host, escolher Origin e Output mostrados pela receita. Validar primeiro destino inválido interno/junção; carregamento não cria arquivo. Navegar nas cinco áreas, três modos de documentos e dois temas. Painel esperado: 30/40 tarefas = 75% declarado, 12/20 requisitos = 60% cobertura documental; sem percentual de implementação. Seguir cada ocorrência/fonte; referência a .cs permanece texto não verificado. Carregar escopo vazio e observar Sem itens.

Verificações e ficha: confirmar exatamente duas opções. Alternar seleção em uma tela e conferir a outra; selecionar não inicia execução. Check_gate não pede agente: selecionar etapa spec, registro verification/spec-stage.json, confirmar expectedRevision da receita e clicar Executar. READY/exit 0 para registro íntegro, BLOCKED/exit 1 para bloqueio/hash/revisão inválidos; referência fora do escopo é bloqueada antes de chamar core/leitura. Gate offline continua funcionando. Rótulo não afirma teste ou decisão humana autenticados.

Analyze: selecionar cada agente com selo Passed, confirmar alcance da funcionalidade e envio externo, clicar Executar. Introduzir contradição documental intencional (por edição do DEV) e verificar achado rastreável; não exigir saída textual idêntica de IA. Foco do requisito não elimina documentos necessários. Remover tasks (por preparação do DEV) e comprovar bloqueio sem criação automática. Cancelar durante execução e observar encerramento de toda a árvore antes de Cancelled; ler documento/gate sem depender de IA. Agente incompatível permanece visível com motivo e indisponível.

Após uma execução, editar documento pelo usuário e conferir Stale + preservação da edição; não executar agente/gate automaticamente. Alteração concorrente no ponto de leitura deve ser ensaiada pelo harness. Não modificar código como forma de disparar análise V1.

Fechar sem Salvar e conferir que reports não contém relatório oficial novo. Estado externo do agente pode permanecer conforme informado; não o importar como histórico. Repetir execução e clicar Salvar, conferir conteúdo/destino, Markdown/JSON/CSV e manifesto create-new. Classificar falso positivo/risco sem motivo deve ser recusado; com motivo mantém estado técnico original. Comparar dois registros e excluir somente o selecionado por ação humana. Simular permissão revogada/disco cheio e verificar SaveFailed e parcial externo identificado. Não há limpeza automática de conteúdo para simular zero efeitos.

## Matriz dos 29 cenários

Todos começam NÃO EXECUTADO até implementação. Cada linha abaixo exige fonte/hash, versão do aplicativo/adaptador e captura/resultado da asserção; não é teste executado por existir esta tabela. Procedimento comum acima + entrada/ação/expectativa da linha determinam o caso. Evidência futura em evidence/USn-validation.md, review em evidence/USn-review.md, roteiro DEV em validation/USn-dev.md e decisão acceptance/USn.md.

| Cenário / critério | Entrada e ação | Resultado observável |
|---|---|---|
| US1-S01 / US1-AC01 | Projeto com duas funcionalidades; Carregar com destino externo | Mostrar artefatos e escopos, sem escrita na origem |
| US1-S02 / US1-AC02 | Link que resolve para origem e executor restrito; Escolher destino ou tentar escrita | Recusar e preservar conteúdo |
| US1-S03 / US1-AC03 | Dois projetos e histórico salvo; Alternar projeto e mudar destino | Manter contextos e arquivos anteriores |
| US1-S04 / US1-AC04 | Análise em andamento; Usuário editar documento monitorado | Preservar edição e indicar perda de atualidade |
| US2-S01 / US2-AC01 | Links válidos e quebrados; Buscar/navegar | Abrir destino autorizado ou explicar ausência |
| US2-S02 / US2-AC02 | Corpus conhecido; Alternar modos | Preservar equivalência do conteúdo e origem |
| US2-S03 / US2-AC03 | Diagramas válidos/inválidos e não suportados; Abrir offline | Renderizar suportado e explicar demais |
| US2-S04 / US2-AC04 | Script e imagem remota; Abrir e depois autorizar imagem | Nenhum script; rede só após ação |
| US3-S01 / US3-AC01 | 30/40 tarefas marcadas e 12/20 requisitos vinculados; Abrir painel e escopo vazio | Mostrar 75% declarado e 60% de cobertura documental; vazio sem percentual |
| US3-S02 / US3-AC02 | Registro READY e declaração humana em documento; Consultar painel | Exibir procedência, sem implementação/teste/aceite comprovados |
| US3-S03 / US3-AC03 | Contradição, vínculo ausente e referência a código; Abrir ocorrências | Mostrar fontes e limite documental, sem ler código |
| US3-S04 / US3-AC04 | Resultado anterior e documento alterado; Atualizar documento e filtrar | Atualizar contagens e marcar verificação desatualizada, sem executar agente/gate |
| US4-S01 / US4-AC01 | Requisito com vínculos explícitos/inferidos; Abrir ficha | Diferenciar fontes e mostrar registros/sugestões |
| US4-S02 / US4-AC02 | Critério sem tarefa e outro ambíguo; Consultar critérios | Informar lacunas documentais sem classificar implementação |
| US4-S03 / US4-AC03 | Documento declara validação sem prova acessível; Consolidar | Mostrar declaração e limite, sem aprovação pelo CemgSpec |
| US4-S04 / US4-AC04 | Documentos de projetos em linguagens diferentes e link .cs; Navegar | Mesma análise documental; link .cs não lido |
| US5-S01 / US5-AC01 | Agente encontrado e outro ausente; Abrir seleção e escolher gate | Descoberta automática, fallback restrito; gate não pede agente |
| US5-S02 / US5-AC02 | Dois agentes reais e integração incompatível; Executar analyze | Demonstrar contrato ou bloquear apenas o adaptador com motivo |
| US5-S03 / US5-AC03 | Verificações e ficha abertas; Alternar combo, tentar terceira opção e clicar Executar | Só duas opções, seleção coerente; execução apenas por clique |
| US5-S04 / US5-AC04 | Documentos com instruções maliciosas e tasks ausente; Analisar | Não executar instruções/ler código; informar pré-requisito sem criar arquivos |
| US5-S05 / US5-AC05 | Agente sem autenticação ou em execução; Cancelar/consultar leitor e gate | Mostrar estado real, encerrar descendentes e manter funções locais |
| US5-S06 / US5-AC06 | Registros conhecidos com READY, BLOCKED, hash adulterado e revisão divergente; Selecionar etapa/registro/revisão e executar gate | Resultado correto e delimitado, sem inventar aprovação humana/implementação |
| US5-S07 / US5-AC07 | Rede desativada e evidências documentais, .cs, link externo e alteração concorrente; Executar gate | Documental funciona offline; demais bloqueiam ou invalidam sem ler destinos proibidos |
| US6-S01 / US6-AC01 | Resultado em sessão; Fechar sem salvar e repetir/salvar | Primeiro não gera registro oficial; segundo fica no destino externo; estado interno do agente permanece externo |
| US6-S02 / US6-AC02 | Resultado revisado; Exportar três formatos | Conteúdo rastreável e origem intacta |
| US6-S03 / US6-AC03 | Ocorrência; Classificar sem/com justificativa e mudar contexto | Exigir justificativa e nova revisão quando afetada |
| US6-S04 / US6-AC04 | Duas análises salvas; Comparar/excluir uma | Exibir diferenças e preservar demais registros |
| US8-S01 / US8-AC01 | Projeto carregado; Percorrer cinco áreas | Manter contexto e proteção identificáveis |
| US8-S02 / US8-AC02 | Ficha com ocorrência selecionada; Trocar tema | Preservar seleção e entendimento dos estados |

## Automação a entregar nas tasks

Projetos de testes planejados: tests/CemgSpec.Tests (domínio/modelo/contagens), tests/CemgSpec.Windows.Tests (broker, gate runner, agentes/cancelamento/TOCTOU e writer) e tests/CemgSpec.Ui.Tests (host real e navegação de navegador). Reutilizar framework adequado existente se encontrado; neste scaffold não há suíte existente que comprove estes fluxos. A implementação seleciona versões estáveis auditadas e entrega testes/testdata junto do incremento. Scripts/testes são do desenvolvimento do CemgSpec e não são terceira opção na combo do produto.

Comandos, após os projetos existirem, a partir da raiz de desenvolvimento:

```powershell
dotnet test .\tests\CemgSpec.Tests\CemgSpec.Tests.csproj
dotnet test .\tests\CemgSpec.Windows.Tests\CemgSpec.Windows.Tests.csproj
dotnet test .\tests\CemgSpec.Ui.Tests\CemgSpec.Ui.Tests.csproj
```

Os testes de integração reais devem identificar previamente agentes/autenticação, fixtureRoot, diretório externo técnico, runtime/core conferidos e host loopback; dados ausentes geram NÃO EXECUTADO com motivo e bloqueiam habilitação correspondente. O runner UI deve iniciar/encerrar seu próprio host em loopback; não depender de instância arbitrária ou solução auditada. Contrato de seleção de dados: fixtureRoot externo criado pela receita; nunca caminho real de usuário como default. Registros identificam comando completo efetivo, cwd, entradas, versões, saída, resultado esperado/observado, efeitos e limites. Comandos sem variáveis finais/credenciais não são prova executada; nenhum segredo entra em log.

## Conferência e encerramento

Comparar baseline: os arquivos da origem devem ter mesmos hashes e conjunto de caminhos, exceto edições externas intencionais do DEV registradas antes/depois. Tentativa indevida bem-sucedida em fixture é falha de proteção, nunca restaurar e apresentar aprovado. Hash não substitui observação de acesso/leitura negada. Registrar evidências em diretório técnico externo/FEATURE_DIR do desenvolvimento; isto não autoriza persistência de relatórios do produto antes de Save.

Encerrar somente host/Job Object/processos próprios; conferir que nenhum descendente da execução resta. Preservar fixture/evidência para inspeção; excluir apenas por decisão explícita do DEV, após resolver caminho absoluto sob a raiz testRoot escolhida e confirmar que não é origem real. Não fornecer comando de exclusão recursiva baseado em path não validado.

Entregar ao DEV mesma revisão, versões, receita/caminhos, comando de inicialização, endereço, entradas, passos, resultados e efeitos. Registrar estados ESCRITO, CONFERIDO_PELA_IA (fluxo realmente executado) e EXECUTADO_PELO_DEV separadamente. Aceite normal só após reprodução e decisão humana atribuível. Se impossibilidade, IA prepara rascunho com causa/risco/ação; só DEV confirma e escolhe AGUARDAR/REJEITAR/ACEITAR_COM_RESSALVAS. Não marcar teste não executado aprovado.

## Registro de testes e decisão do DEV — emenda 4.1.0

Esta regra rege o desenvolvimento do CemgSpec, sem ampliar as verificações do
produto sobre a origem auditada. Todos os roteiros `validation/USn-dev.md` e de
incrementos devem conter tabela por cenário/critério com resultado, observado ou
motivo de não execução, evidências, commit/diff testado, responsável e data/fuso;
campos humanos em branco na entrega. Retestes acrescentam rodadas sem apagar histórico.
A declaração explícita no roteiro registra ACEITO, REJEITADO ou ACEITO_COM_RESSALVAS,
com versão, autoria, justificativa, escopo e riscos/ações quando aplicável. Quando
solicitado pelo DEV, a IA confere e transcreve para `acceptance/USn.md` com referência
à fonte; inconsistências mantêm pendência. PASSOU não concede aceite; incremento
não aceita história completa. Não há monitoramento automático configurado.
Templates futuros: overrides locais `dev-validation-template`, `acceptance-template`,
`plan-template` e `tasks-template`, sob `.specify/templates/overrides/`.
