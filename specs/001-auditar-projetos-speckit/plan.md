# Plano de implementação: Auditoria documental de projetos SpecKit — V1

**Branch real**: main | **Revisão**: 2026-11-16 | **Spec**: [spec.md](spec.md)
**Estado**: atualizado à constituição 4.0.0; implementação retomada com controles de aplicação. Provas funcionais por história e integração continuam obrigatórias.
**Preset**: cemg-speckit 3.0.0 | **Template**: plan-template | **Composição**: replace.
**Fonte**: .specify/presets/cemg-speckit/templates/plan-template.md.
**Resolvedor**: .specify/scripts/powershell/resolve-template.ps1.
**Constituição**: 4.0.0; core + adendo append. Base Git da revisão: 79d9c3d; candidato identificado por hash dos documentos atuais.

## Resumo

Blazor local Windows com leitura exclusiva de documentos/registros gerados pelo SpecKit, dashboard de progresso declarado, ficha documental, preview Markdown e resultados em memória até Salvar externo. A combo fechada apresenta somente `speckit-analyze — Consistência documental` e `check_gate — Validar gates`, em Verificações e na ficha. Selecionar não executa; somente clicar Executar. Sem campo livre, shell, terceira opção, hooks da origem ou encadeamento de etapas.

Análise de código, comparação requisito–implementação, Roslyn/Razor, converge/checklist/clarify e consulta de vulnerabilidades da solução são futuras. C# permanece a linguagem de implementação do SpecAudit. Monitoramento documental e gate independem de IA. Codex/Antigravity somente para analyze, habilitados individualmente após prova técnica.

O antigo B2-WFP e suas duas provas falhas são históricos; a V1 NÃO depende de instalar/consertar o mediador privilegiado. A redução não aprova segurança automaticamente: a fronteira documental será implementada e demonstrada antes de habilitação. Essa prova é dependência das tarefas funcionais, não pré-condição para escrever tarefas. Sem nova elevação, instalação ou autorização inferida. Planejamento anterior completo: verification/archive/before-documentary-v1/plan.md; evidências históricas e manifestos permanecem imutáveis.

## Contexto técnico

- Plataforma: Windows x64, execução local no navegador, Blazor Interactive Server existente, ASP.NET Core 10 e C# estável compatível; sem previews. Versões do inventário anterior são históricas, a conferir antes de implementação.
- Dependências candidatas: parser Markdown, sanitizador e Mermaid locais. Fixar versões estáveis e consultar vulnerabilidades antes de adicioná-las. Nenhum pacote alterado nesta revisão. Não adicionar Roslyn, compilador Razor analítico, MSBuildWorkspace ou leitor de manifests do projeto auditado.
- Armazenamento: estado e resultados oficiais em memória; gravação só por Salvar no destino externo escolhido ao carregar. Caches/conversas dos agentes permitidos apenas fora da origem, com locais informados. Sem autosave ou logs próprios com conteúdo.
- Ferramenta gate: core check_gate.py/política CEMG 3.0.0 empacotados e pinados por hash, runtime Python confiável absoluto com -I -B; runner do SpecAudit invoca validate/validate_impossibility com Path de capacidade do catálogo. Nenhum caminho real da origem é entregue ao core/runner. A mesma semântica do core é verificada por equivalência em fixtures. Python ausente torna só o gate indisponível, sem instalação/fallback automático.
- Gate: subprocesso comum não elevado, runtime absoluto conferido com -I -B, cwd externo e runner/core/política confiáveis. Somente funções de validação, sem --project/main/hooks da origem. Catálogo virtual e broker continuam validando cada leitura; não há garantia de firewall/sandbox OS.
- Ferramenta analyze: adaptação documental com snapshots selecionados, ferramentas/configuração restritas e cwd/estado externos. Executor gerenciado comum, sem shell de comandos livres, carregamento de configuração da origem ou argumentos vindos de documentos. Compatibilidade e limites comprovados por adaptador; ausência de contenção OS é informada antes de executar.
- Rede: leitura/gate funcionam sem rede; analyze pode acessar o provedor selecionado após informação ao usuário. O SpecAudit não oferece ferramentas para bancos/APIs/serviços da solução. Controles são de aplicação/adaptador, sem promessa de bloqueio de rede pelo kernel.
- Limites técnicos: configuração interna versionada e aplicada por broker/runner a entrada, saída, grafo, duração e cancelamento. A tarefa de implementação calibra valores em fixture antes de habilitar; excesso gera LimitExceeded/Inconclusive explícito, nunca truncamento/READY parcial. Não são metas de desempenho de produto inventadas.

## Verificação da constituição

| Princípio | Aplicação na V1 | Estado |
|---|---|---|
| I | Origem sem escrita, restauração, cópia ou ACL alterada; descendentes incluídos | Desenho definido em verification.md; prova de habilitação na implementação |
| II | Destino externo por projeto; relatório oficial só por Salvar; sessão interna externa | Preservado; fluxo a implementar/provar |
| III | Apenas documentos/registros SpecKit; referências a código não seguidas | Catálogo/Path de capacidade definidos; prova na implementação |
| IV | Progresso declarado/cobertura documental e gates; nenhuma implementação calculada | Critérios US3/US4 revisados |
| V | Agente só para analyze; gate e leitor sem IA | Critérios de habilitação individuais definidos; provas na implementação |
| VI | Sem scripts/hooks/configuração/segredos da origem; gate sem rede | Desenho V1-A/B validado documentalmente; execução futura condiciona habilitação |
| VII | Auditar dependências do próprio SpecAudit; inventário da origem adiado | Nenhuma dependência nova nesta revisão |
| VIII | Estado técnico não equivale a aceite humano autenticado | Preservado nos resultados/gates |
| IX | IA exercita produto, DEV reproduz e aceita por versão | Planejado; nenhuma funcionalidade demonstrada aqui |

Sem exceção à somente leitura. Alterar o escopo não converte provas falhas em aprovadas. Spec pronta significa requisitos documentados, não produto implementado. Desenho comparado aos princípios I–IX sem exceção; isto cumpre a validação de mecanismo no planejamento, sem alegar comprovação operacional. Execução/habilitação final continua condicionada aos testes da implementação.

## Estrutura e artefatos

Host existente SpecAudit/ e SpecAudit.slnx preservados. Fronteiras definidas: UI Blazor, domínio documental, catálogo/broker de documentos autorizados, adaptadores analyze, executor determinístico gate e writer externo acionado por Salvar. IDs de projeto/funcionalidade/documento, sem endpoint de caminho arbitrário.

O catálogo aceita somente documentos/registros SpecKit identificados e recursos de apresentação vinculados autorizados. Extensão .md/.json sozinha não autoriza qualquer arquivo. Não varrer código/manifests/configuração/Git para inferir implementação. Recusar reparse points que escapem ao escopo; evitar troca de destino entre validação e abertura. Links de código mostram texto não verificado e não abrem o destino.

Preflight + broker de leitura no ponto de acesso do gate validam etapa, registro, revisão e TODO o grafo de caminhos que o script possa abrir: evidence, stdout/stderr de test-run, artifacts de product-execution e fontes de decisões/impossibilidade. Não seguir .cs/.razor, configuração, conteúdo externo ao catálogo ou ciclos/referências inválidas. Se necessário para o gate, apresentar BLOCKED/escopo não suportado, sem ler o arquivo e sem falsificar READY. Revisão deve ser selecionada/confirmada pelo usuário; repetir o valor do registro não comprova que corresponde à versão atual. Hash válido comprova integridade registrada, não execução de teste nem autenticidade humana. Não reescrever registros da origem.

A combo seleciona enum fechado AnalyzeDocuments ou CheckGate. A escolha é mantida por projeto entre Verificações/ficha. Analyze: escopo da funcionalidade com requisito apenas como foco; exige spec/plan/tasks completos. Gate: funcionalidade + etapa/registro/revisão, com alcance explícito também na ficha. Desabilitar controles de agente para gate. Ausência de registros/pré-requisitos produz mensagem; não criar documentos para suprir ausência. Cancelamento não deixa processo da execução ativo.

Artefatos atuais: spec, plan, research, data-model.md, contracts/application.md, contracts/verification.md, contracts/persistence.md e quickstart.md, checklist/reviews/gates. Phase 1 concluída como desenho/roteiro. Não gerar tasks ou implementação nesta alteração; próximo comando poderá gerar tarefas com as dependências de proteção explícitas. Evidências por história futuras: evidence/USn-validation.md, evidence/USn-review.md, validation/USn-dev.md, acceptance/USn.md. Evidência técnica de desenvolvimento pode ser persistida no FEATURE_DIR; não contorna a regra do produto de Salvar.

## Matriz de validação

Todos os cenários abaixo são PROPOSTOS/NÃO EXECUTADOS no produto. Reutilizar testes adequados existentes, sem atribuir resultados do worker antigo à V1. Dados sempre sintéticos e externos a solução real. A matriz lista uma entrada por critério ativo; US7 está adiada.

| História | Critério/cenário | Nível e teste a criar/reutilizar | Execução e asserção esperada | Evidência |
|---|---|---|---|---|
| US1 | US1-AC01 / US1-S01 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US1-S01 | Preparar Projeto com duas funcionalidades; Carregar com destino externo; observar: Mostrar artefatos e escopos, sem escrita na origem. Ver quickstart.md. | evidence/US1-validation.md |
| US1 | US1-AC02 / US1-S02 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US1-S02 | Preparar Link que resolve para origem e APIs somente leitura; Escolher destino ou tentar escrita; observar: Recusar e preservar conteúdo. Ver quickstart.md. | evidence/US1-validation.md |
| US1 | US1-AC03 / US1-S03 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US1-S03 | Preparar Dois projetos e histórico salvo; Alternar projeto e mudar destino; observar: Manter contextos e arquivos anteriores. Ver quickstart.md. | evidence/US1-validation.md |
| US1 | US1-AC04 / US1-S04 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US1-S04 | Preparar Análise em andamento; Usuário editar documento monitorado; observar: Preservar edição e indicar perda de atualidade. Ver quickstart.md. | evidence/US1-validation.md |
| US2 | US2-AC01 / US2-S01 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US2-S01 | Preparar Links válidos e quebrados; Buscar/navegar; observar: Abrir destino autorizado ou explicar ausência. Ver quickstart.md. | evidence/US2-validation.md |
| US2 | US2-AC02 / US2-S02 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US2-S02 | Preparar Corpus conhecido; Alternar modos; observar: Preservar equivalência do conteúdo e origem. Ver quickstart.md. | evidence/US2-validation.md |
| US2 | US2-AC03 / US2-S03 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US2-S03 | Preparar Diagramas válidos/inválidos e não suportados; Abrir offline; observar: Renderizar suportado e explicar demais. Ver quickstart.md. | evidence/US2-validation.md |
| US2 | US2-AC04 / US2-S04 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US2-S04 | Preparar Script e imagem remota; Abrir e depois autorizar imagem; observar: Nenhum script; rede só após ação. Ver quickstart.md. | evidence/US2-validation.md |
| US3 | US3-AC01 / US3-S01 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US3-S01 | Preparar 30/40 tarefas marcadas e 12/20 requisitos vinculados; Abrir painel e escopo vazio; observar: Mostrar 75% declarado e 60% de cobertura documental; vazio sem percentual. Ver quickstart.md. | evidence/US3-validation.md |
| US3 | US3-AC02 / US3-S02 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US3-S02 | Preparar Registro READY e declaração humana em documento; Consultar painel; observar: Exibir procedência, sem implementação/teste/aceite comprovados. Ver quickstart.md. | evidence/US3-validation.md |
| US3 | US3-AC03 / US3-S03 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US3-S03 | Preparar Contradição, vínculo ausente e referência a código; Abrir ocorrências; observar: Mostrar fontes e limite documental, sem ler código. Ver quickstart.md. | evidence/US3-validation.md |
| US3 | US3-AC04 / US3-S04 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US3-S04 | Preparar Resultado anterior e documento alterado; Atualizar documento e filtrar; observar: Atualizar contagens e marcar verificação desatualizada, sem executar agente/gate. Ver quickstart.md. | evidence/US3-validation.md |
| US4 | US4-AC01 / US4-S01 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US4-S01 | Preparar Requisito com vínculos explícitos/inferidos; Abrir ficha; observar: Diferenciar fontes e mostrar registros/sugestões. Ver quickstart.md. | evidence/US4-validation.md |
| US4 | US4-AC02 / US4-S02 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US4-S02 | Preparar Critério sem tarefa e outro ambíguo; Consultar critérios; observar: Informar lacunas documentais sem classificar implementação. Ver quickstart.md. | evidence/US4-validation.md |
| US4 | US4-AC03 / US4-S03 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US4-S03 | Preparar Documento declara validação sem prova acessível; Consolidar; observar: Mostrar declaração e limite, sem aprovação pelo SpecAudit. Ver quickstart.md. | evidence/US4-validation.md |
| US4 | US4-AC04 / US4-S04 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US4-S04 | Preparar Documentos de projetos em linguagens diferentes e link .cs; Navegar; observar: Mesma análise documental; link .cs não lido. Ver quickstart.md. | evidence/US4-validation.md |
| US5 | US5-AC01 / US5-S01 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US5-S01 | Preparar Agente encontrado e outro ausente; Abrir seleção e escolher gate; observar: Descoberta automática, fallback restrito; gate não pede agente. Ver quickstart.md. | evidence/US5-validation.md |
| US5 | US5-AC02 / US5-S02 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US5-S02 | Preparar Dois agentes reais e integração incompatível; Executar analyze; observar: Demonstrar contrato ou bloquear apenas o adaptador com motivo. Ver quickstart.md. | evidence/US5-validation.md |
| US5 | US5-AC03 / US5-S03 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US5-S03 | Preparar Verificações e ficha abertas; Alternar combo, tentar terceira opção e clicar Executar; observar: Só duas opções, seleção coerente; execução apenas por clique. Ver quickstart.md. | evidence/US5-validation.md |
| US5 | US5-AC04 / US5-S04 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US5-S04 | Preparar Documentos com instruções maliciosas e tasks ausente; Analisar; observar: Não executar instruções/ler código; informar pré-requisito sem criar arquivos. Ver quickstart.md. | evidence/US5-validation.md |
| US5 | US5-AC05 / US5-S05 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US5-S05 | Preparar Agente sem autenticação ou em execução; Cancelar/consultar leitor e gate; observar: Mostrar estado real, encerrar descendentes e manter funções locais. Ver quickstart.md. | evidence/US5-validation.md |
| US5 | US5-AC06 / US5-S06 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US5-S06 | Preparar Registros conhecidos com READY, BLOCKED, hash adulterado e revisão divergente; Selecionar etapa/registro/revisão e executar gate; observar: Resultado correto e delimitado, sem inventar aprovação humana/implementação. Ver quickstart.md. | evidence/US5-validation.md |
| US5 | US5-AC07 / US5-S07 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US5-S07 | Preparar Rede desativada e evidências documentais, .cs, link externo e alteração concorrente; Executar gate; observar: Documental funciona offline; demais bloqueiam ou invalidam sem ler destinos proibidos. Ver quickstart.md. | evidence/US5-validation.md |
| US6 | US6-AC01 / US6-S01 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US6-S01 | Preparar Resultado em sessão; Fechar sem salvar e repetir/salvar; observar: Primeiro não gera registro oficial; segundo fica no destino externo; estado interno do agente permanece externo. Ver quickstart.md. | evidence/US6-validation.md |
| US6 | US6-AC02 / US6-S02 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US6-S02 | Preparar Resultado revisado; Exportar três formatos; observar: Conteúdo rastreável e origem intacta. Ver quickstart.md. | evidence/US6-validation.md |
| US6 | US6-AC03 / US6-S03 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US6-S03 | Preparar Ocorrência; Classificar sem/com justificativa e mudar contexto; observar: Exigir justificativa e nova revisão quando afetada. Ver quickstart.md. | evidence/US6-validation.md |
| US6 | US6-AC04 / US6-S04 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US6-S04 | Preparar Duas análises salvas; Comparar/excluir uma; observar: Exibir diferenças e preservar demais registros. Ver quickstart.md. | evidence/US6-validation.md |
| US8 | US8-AC01 / US8-S01 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US8-S01 | Preparar Projeto carregado; Percorrer cinco áreas; observar: Manter contexto e proteção identificáveis. Ver quickstart.md. | evidence/US8-validation.md |
| US8 | US8-AC02 / US8-S02 | Integração Windows/E2E quando há IO; domínio/UI nos demais; caso US8-S02 | Preparar Ficha com ocorrência selecionada; Trocar tema; observar: Preservar seleção e entendimento dos estados. Ver quickstart.md. | evidence/US8-validation.md |

Para US5-S06, usar registros locais CEMG conhecidos: READY com hash correto; BLOCKED com bloqueio declarado; estado READY com evidence hash adulterado; revisão divergente; registro ausente. Comparar saída/exit do script empacotado, captura da UI e ausência de qualquer escrita. Para US5-S07, incluir referência .cs, junção para fora e referência aninhada em logs/artifacts; instrumentar abertura real e comprovar que o destino proibido não foi lido. Gate nunca recebe `--project`.

Para US1-S02/US5-S02/S04, testar as APIs reais da aplicação, rejeição de destinos/junções e IDs alheios, preservação dos hashes, seleção documental e configuração/cwd/estado externos dos adaptadores. Código/configuração não entram no catálogo. As antigas provas de negação de syscall em processos maliciosos são históricas, não requisitos ativos. Testes reais das integrações continuam obrigatórios; mocks não as comprovam.

## Ambiente e dados

Preparação proposta: usuário normal Windows, navegador local, corpus sintético com duas funcionalidades, 40 tarefas (30 marcadas), 20 requisitos (12 vinculados), spec/plan/tasks, gates READY/BLOCKED, links válidos/quebrados e Markdown/Mermaid malicioso/ inválido. Incluir arquivo .cs canário que não deve ser lido; seu conteúdo não entra em corpus/análise. O teste de desenvolvimento pode instrumentar a fixture, o produto não a executa. Criar destino externo e link inválido para origem. Baseline e observação em diretório técnico externo, sem restaurar a origem para ocultar alterações.

Inicialização futura do aplicativo a partir da raiz de desenvolvimento:
`dotnet run --project SpecAudit/SpecAudit.csproj --no-launch-profile -- --urls http://127.0.0.1:5187`.
Comando do próprio SpecAudit, não da solução auditada. Nenhuma demonstração funcional executada nesta revisão. Antes de disponibilizar, implementar e provar sessão local emitida pelo host, validação Host/Origin/antiforgery e picker nativo com IDs opacos, conforme contracts/application.md; loopback somente, sem exposição LAN.

Gate de desenvolvimento documental nesta feature usa Python existente `C:/Users/cwagn/anaconda3_11/python.exe` e `.specify/presets/cemg-speckit/tools/check_gate.py`, com --feature/--record/--stage/--revision, SEM --project. Isso valida os registros de planejamento deste repositório; não é prova do executor empacotado ou da UI futura.

Agentes: usar autenticação existente exclusivamente pela integração validada; não ler/copiar tokens para relatórios. Gate não precisa de agente ou credencial. Registrar versões e locais externos reais de cache/sessão antes de prova do analyze. Nenhuma instalação/elevação automática autorizada. IA e DEV precisam dos mesmos dados/entrada/resultado/efeitos; roteiro definido em quickstart.md; fluxo a executar na implementação. Limpeza: encerrar host e descendentes próprios; excluir somente fixtures previamente resolvidas dentro de raiz sintética expressamente identificada; nunca origem real ou arquivos do usuário.

## Viabilidade por história

| História | IA executa agora? | DEV reproduz agora? | Impedimento | Próximo trabalho |
|---|---|---|---|---|
| US1 | Somente inspeção documental | Não no produto | ReaderBoundary ainda não implementado/provado | Provar preservação e escopo sem ACL na solução |
| US2 | Corpus/expectativas documentados | Após implementação | Leitor/catálogo/preview | Provar sanitização e caminhos |
| US3 | Fórmulas inspecionadas | Após implementação | Domínio/UI | Demonstrar contagens e atualidade |
| US4 | Requisitos documentados | Após implementação | Fontes/vínculos/UI | Demonstrar ficha sem código |
| US5 | check_gate de desenvolvimento executável; integração final não | Não no produto | GateAdapter/Analyze ainda não implementados/provados | Provar analyze restrito e gate empacotado/offline |
| US6 | Não no produto | Após implementação | Writer/fluxo Salvar | Provar memória até clique e histórico externo |
| US7 | Fora da V1 | Fora da V1 | Escopo adiado | Nova revisão futura |
| US8 | Não no produto | Após implementação | Telas ainda scaffold | Demonstrar cinco áreas/dois temas |

## Estratégia de revisão

Autorrevisão documental nesta revisão; aprovação humana de PR e aceite de história separados. Conferir diff, origem intacta, catálogo, reparse/TOCTOU, referências aninhadas do gate, ausência de shell/hooks, rede/credenciais do agente, persistência externa e rótulos honestos. Achados com arquivo/linha/efeito/evidência. Impeditivos: qualquer acesso a código na V1, escrita na origem, relatório oficial antes de Salvar, agente sem proteção comprovada habilitado, caminho arbitrário, execução de código/hooks da origem, READY falso ou resultado sem limites. Revalidar testes/hashes afetados após mudança; não conceder aceite por hash ou documentação.

## Dependências e sequência de entrega

Planejamento define controles e dependências, implementação comprova. A autorização atual permite fechar desenho e gerar plano pronto para tasks; não é exceção à constituição ou autorização de elevação.

1. Gerar tarefas a partir de modelo/contratos/matriz/quickstart. Incluir infraestrutura de ReaderBoundary e provas Windows reais em fixture ANTES de qualquer tarefa que exponha origem ao executor; se falhar, bloquear dependente e manter UI indisponível. Não exige produto pronto para escrever tarefa.
2. Implementar gate confiável: core/política pinados + runner Path de capacidade/broker; provas de equivalência/offline/paths aninhados/TOCTOU/cancelamento. GateAdapter habilita apenas após passar. Sem --project, subprocesso de verificação adicional ou script da origem.
3. Implementar leitor/domínio/ficha/temas com catálogo seguro e teste do meio real de uso. Leitor/gate independem do analyze.
4. Implementar/provar adaptadores individuais Codex/Antigravity com canal documental, contrato documental, ferramentas configuradas e finalidades de rede e cache externo. Analyzer sem selo Passed permanece indisponível. Nenhum novo teste elevado ou instalação está autorizado por esta fase.
5. Implementar writer/Save/histórico com fronteira externa e provas; demonstrar fluxo final pela IA e roteiro pelo DEV. Revalidar após alterações; nenhum aceite ou aprovação humana inferido.

## Complexidade e exceções

Nenhuma exceção à segurança. A lista de dois comandos é restrição de produto, não sandbox. Prompt/allowlist nominal, detecção posterior por hash ou escrita seguida de restore não cumprem somente leitura. O mediador WFP é pesquisa histórica para cenário amplo/futuro, sem sucesso funcional declarado. Não remover logs/provas antigas nem reaproveitar autorização elevada consumida. Não prometer que limitar comandos comprova isolamento do agente.

## Estratégia de entrega em PRs

| PR previsto | Objetivo/escopo | US/critério | Tasks previstas | Base/dependência | Verificação/evidência | Revisão | Integração | Fecha história? |
|---|---|---|---|---|---|---|---|---|
| PR-A | Fronteira/catálogo, seleção e invalidação | US1 | Áreas; IDs somente em tasks | main; implementar/provar ReaderBoundary neste incremento antes de habilitar | Matriz US1 e acessos reais | IA autorrevisão + humano | Sem impeditivos, checks executados e aprovação humana | US1 |
| PR-B | Preview, contagens, ficha e temas | US2/3/4/8 | Leitor/domínio/UI | PR-A | Matriz correspondente, dois temas e zero leitura de código | IA + humano | Mesmas condições; analyze ainda desabilitado | Fluxos documentais |
| PR-C | Executor gate e combo de duas opções | US5-AC03/06/07 e parte AC05 | Gate/contrato/UI | PR-A/B; implementar/provar GateAdapter neste incremento antes de habilitar | Registros offline válidos/negativos e combo sincronizada | IA + humano | Sem IA obrigatória para gate; analyze protegido por indisponibilidade | Parcial US5 |
| PR-D | Descoberta/analyze real protegido | US5-AC01/02/04/05; integração AC03 | Adaptadores e cancelamento | PR-A/C; implementar/provar cada AnalyzeAdapter antes de habilitar | Ambos reais e estado externo | IA + humano | Não habilitar incompatível | US5 |
| PR-E | Salvar/exportar/histórico e fluxo final | US6 e regressão ativa | Writer/decisões/fechamento | PR-B/C/D | Matriz US6 + cenário integrado e roteiro DEV | IA + humano | Fluxo real pela IA; DEV reproduz/aceita separadamente | US6 e conjunto |

Divisão por escopo, risco e testabilidade; sem metas de linhas/duração. Cada PR preserva estado intermediário seguro; código/testes/evidências juntos. Mudança de base exige renovar verificações. Nenhum PR aberto ou implementação feita nesta revisão.

## Preservação do planejamento técnico

research.md R18 registra as decisões de mecanismo e sua revisão; data-model.md e os três contratos definem dados/estados, UI, fronteira e Save. quickstart.md define ambiente, receita de fixtures, comandos futuros, asserções e 29 cenários. Seu estado é ESCRITO/documentalmente conferido, não produto demonstrado. As evidências históricas permanecem íntegras e não são reutilizadas como execução atual.

## Condição de saída da etapa plan

READY para geração de tarefas após revisão de cobertura/dados/acessos/PRs/roteiro e check_gate dos hashes atuais. Critérios do gate plan são condições de planejamento, não testes já aprovados. V1-A/B têm desenho e caminho de validação definidos; suas provas permanecem NÃO EXECUTADAS como obrigações da implementação/habilitação. tasks deve ordenar essas provas antes das tarefas dependentes. Sem operação segura demonstrada não há integração/PR/aceite normal; READY do plano não habilita agente ou gate nem altera a origem.

## Plano de execução do produto

Após implementação, IA inicia host loopback e carrega corpus/destino externo, navega Documentos/Dashboard/Requisitos, alterna temas, seleciona cada uma das duas opções nas duas telas, confirma escopo do gate, executa, observa estados/fontes/efeitos/cancelamento; fecha sem Salvar e comprova ausência de registros oficiais; repete e clica Salvar para comprovar exportações/histórico. DEV repete mesma revisão/ambiente/dados, observa resultados e decide aceite separado. Sem execução real, manter NÃO EXECUTADO com causa e próximo passo; IA não confirma impossibilidade em nome do DEV.

## Estrutura concreta prevista para tasks

- SpecAudit/Domain/: modelo documental/indicadores/ocorrências e estados de execução, sem dependência de filesystem de origem.
- SpecAudit/Application/: serviços tipados de sessão/catálogo/ficha/verificações/Save.
- SpecAudit/Infrastructure/Documents/: broker por IDs/handles Windows, catálogo e invalidação.
- SpecAudit/Infrastructure/Execution/: executor gerenciado fechado/cancelamento e capacidades por execução.
- SpecAudit/Infrastructure/Agents/: descoberta e adaptadores independentes Codex/Antigravity.
- SpecAudit/Infrastructure/Gates/: runner confiável, Path de capacidade e pacote do core/política CEMG.
- SpecAudit/Infrastructure/Reports/: writer externo create-new e leitura/comparação de pacotes salvos.
- SpecAudit/Components/: cinco áreas e componentes compartilhados, combo fechada e preview sanitizado.
- tests/SpecAudit.Tests, tests/SpecAudit.Windows.Tests e tests/SpecAudit.Ui.Tests: domínio, fronteiras reais e host/navegador. Framework/versões estáveis devem ser fixados e auditados antes de introdução; não são dependências adicionadas nesta revisão.

Detalhes de operações/erros em contracts/, propriedades e relações em data-model.md. Esta estrutura é plano, não código existente. APIs Blazor internas, sem REST público ou leitura por path arbitrário.


## Modelo de confiança vigente — constituição 4.0.0

A fronteira de somente leitura é imposta nas APIs/catálogo/broker e testada com filesystem real, inclusive identidade, junções, alteração concorrente, ID de outra sessão e destino externo. Manter abertura readonly e nenhuma operação de escrita/execução da origem. Os dois valores de VerificationType são o conjunto fechado de verificações.

Processos externos são confiados à configuração validada de cada adaptador: executável absoluto/hash conferido, ArgumentList sem shell, cwd externo, dados selecionados via stdin/canal, saída limitada e cancelamento da árvore própria. Não fornecer ferramenta genérica de filesystem/shell ao agente. Estado externo precisa ser identificável. Não executar scripts/presets/configuração do projeto. Antes de analyze informar explicitamente a ausência de contenção de agente comprometido pelo SO.

Retirar do produto AppContainer/LPAC, alteração de tokens, ajuste de memória, depuração de bootstrap e compilação de worker nativo. Preservar fontes/provas em verification/archive/constitution-v3-isolation/. SEC-001 permanece uma falha histórica da garantia retirada; não foi corrigida ou aprovada por esta emenda. Nenhum experimento bloqueado de privilégio será repetido para destravar a V1.

T095 sincroniza documentos; T096 substitui o candidato experimental por execução gerenciada; T097 adapta testes ao contrato aprovado sem contar testes retirados como aprovados; T098 executa análise/regressão e registra o checkpoint. Em seguida concluir T006–T010 pelo contrato atual e retomar T011. Os 29 critérios ativos e sete roteiros DEV permanecem; somente requisitos específicos de sandbox OS são substituídos.

## Registro de testes e decisão do DEV — emenda 4.1.0

Esta regra rege o desenvolvimento do SpecAudit, sem ampliar as verificações do
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
