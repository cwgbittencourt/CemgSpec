# Plano de implementação: Auditoria documental de projetos SpecKit — V1

**Branch real**: main | **Revisão**: 2026-11-16 | **Spec**: [spec.md](spec.md)
**Estado**: BLOCKED para tasks/design final; revisão de escopo documental concluída.
**Preset**: cemg-speckit 3.0.0 | **Template**: plan-template | **Composição**: replace.
**Fonte**: .specify/presets/cemg-speckit/templates/plan-template.md.
**Resolvedor**: .specify/scripts/powershell/resolve-template.ps1.
**Constituição**: 3.0.0; core + adendo append. Base Git histórica 460af9e3e38d4cb200517c45424c7569b377a322; candidato identificado por hash dos documentos atuais.

## Resumo

Blazor local Windows com leitura exclusiva de documentos/registros gerados pelo SpecKit, dashboard de progresso declarado, ficha documental, preview Markdown e resultados em memória até Salvar externo. A combo fechada apresenta somente `speckit-analyze — Consistência documental` e `check_gate — Validar gates`, em Verificações e na ficha. Selecionar não executa; somente clicar Executar. Sem campo livre, shell, terceira opção, hooks da origem ou encadeamento de etapas.

Análise de código, comparação requisito–implementação, Roslyn/Razor, converge/checklist/clarify e consulta de vulnerabilidades da solução são futuras. C# permanece a linguagem de implementação do SpecAudit. Monitoramento documental e gate independem de IA. Codex/Antigravity somente para analyze, habilitados individualmente após prova técnica.

O antigo B2-WFP e suas duas provas falhas são históricos; a V1 NÃO depende de instalar/consertar o mediador privilegiado. A redução não aprova segurança automaticamente: a fronteira documental ainda precisa ser demonstrada. Sem nova elevação, instalação ou autorização inferida. Planejamento anterior completo: verification/archive/before-documentary-v1/plan.md; evidências históricas e manifestos permanecem imutáveis.

## Contexto técnico

- Plataforma: Windows x64, execução local no navegador, Blazor Interactive Server existente, ASP.NET Core 10 e C# estável compatível; sem previews. Versões do inventário anterior são históricas, a conferir antes de implementação.
- Dependências candidatas: parser Markdown, sanitizador e Mermaid locais. Fixar versões estáveis e consultar vulnerabilidades antes de adicioná-las. Nenhum pacote alterado nesta revisão. Não adicionar Roslyn, compilador Razor analítico, MSBuildWorkspace ou leitor de manifests do projeto auditado.
- Armazenamento: estado e resultados oficiais em memória; gravação só por Salvar no destino externo escolhido ao carregar. Caches/conversas dos agentes permitidos apenas fora da origem, com locais informados. Sem autosave ou logs próprios com conteúdo.
- Ferramenta gate: cópia empacotada, íntegra e conferida do check_gate.py CEMG 3.0.0 e sua política gate-requirements.json, não a cópia da solução carregada. Python confiável e conferido; resolver sua disponibilidade sem instalar automaticamente. Python ausente torna só o gate indisponível, sem fallback para executar código da origem.
- Execução gate proposta: Python absoluto com `-I -B`, sem shell e cwd externo; caminho do script/política pertencente ao SpecAudit; argumentos estruturados e validados. NÃO passar `--project`: essa opção executa `specify preset resolve` via subprocesso. Nenhum subprocesso adicional de verificação permitido; processos técnicos inevitáveis devem ser identificados e restritos.
- Ferramenta analyze: adaptação declarada de speckit-analyze para documentos autorizados. O agente não recebe raiz navegável irrestrita nem ferramentas de shell/escrita/leitura arbitrária, plugins/configurações da solução. Entrega de conteúdo documental por broker/canal restrito ainda exige protótipo/prova, não apenas prompt. Ausência de proteção mantém adaptador desabilitado.
- Rede: gate offline; preview local offline e recursos remotos somente por ação explícita já prevista. analyze pode usar serviço do agente selecionado após informação ao usuário e compatibilidade comprovada. Não acessar serviços da solução. A fronteira de acesso/rede do adaptador documental precisa ser definida/provada sem tornar WFP elevado pré-condição automática.
- Desempenho: limites de entrada/saída/tempo/cancelamento ainda precisam ser detalhados com evidências técnicas; não inventar metas numéricas de produto.

## Verificação da constituição

| Princípio | Aplicação na V1 | Estado |
|---|---|---|
| I | Origem sem escrita, restauração, cópia ou ACL alterada; descendentes incluídos | V1-A: prova técnica documental pendente |
| II | Destino externo por projeto; relatório oficial só por Salvar; sessão interna externa | Preservado; fluxo a implementar/provar |
| III | Apenas documentos/registros SpecKit; referências a código não seguidas | Escopo explícito; leitor/broker a provar |
| IV | Progresso declarado/cobertura documental e gates; nenhuma implementação calculada | Critérios US3/US4 revisados |
| V | Agente só para analyze; gate e leitor sem IA | Prova individual de Codex/Antigravity pendente |
| VI | Sem scripts/hooks/configuração/segredos da origem; gate sem rede | V1-B e V1-A pendentes |
| VII | Auditar dependências do próprio SpecAudit; inventário da origem adiado | Nenhuma dependência nova nesta revisão |
| VIII | Estado técnico não equivale a aceite humano autenticado | Preservado nos resultados/gates |
| IX | IA exercita produto, DEV reproduz e aceita por versão | Planejado; nenhuma funcionalidade demonstrada aqui |

Sem exceção à somente leitura. Alterar o escopo não converte provas falhas em aprovadas. Spec pronta significa requisitos documentados, não produto implementado. Gate constitucional de execução final ainda não aprovado.

## Estrutura e artefatos

Host existente SpecAudit/ e SpecAudit.slnx preservados. Fronteiras propostas: UI Blazor, domínio documental, catálogo/broker de documentos autorizados, adaptadores analyze, executor determinístico gate e writer externo acionado por Salvar. IDs de projeto/funcionalidade/documento, sem endpoint de caminho arbitrário.

O catálogo aceita somente documentos/registros SpecKit identificados e recursos de apresentação vinculados autorizados. Extensão .md/.json sozinha não autoriza qualquer arquivo. Não varrer código/manifests/configuração/Git para inferir implementação. Recusar reparse points que escapem ao escopo; evitar troca de destino entre validação e abertura. Links de código mostram texto não verificado e não abrem o destino.

Preflight do gate valida etapa, registro, revisão e TODO o grafo de caminhos que o script possa abrir: evidence, stdout/stderr de test-run, artifacts de product-execution e fontes de decisões/impossibilidade. Não seguir .cs/.razor, configuração, conteúdo externo ao catálogo ou ciclos/referências inválidas. Se necessário para o gate, apresentar BLOCKED/escopo não suportado, sem ler o arquivo e sem falsificar READY. Revisão deve ser selecionada/confirmada pelo usuário; repetir o valor do registro não comprova que corresponde à versão atual. Hash válido comprova integridade registrada, não execução de teste nem autenticidade humana. Não reescrever registros da origem.

A combo seleciona enum fechado AnalyzeDocuments ou CheckGate. A escolha é mantida por projeto entre Verificações/ficha. Analyze: escopo da funcionalidade com requisito apenas como foco; exige spec/plan/tasks completos. Gate: funcionalidade + etapa/registro/revisão, com alcance explícito também na ficha. Desabilitar controles de agente para gate. Ausência de registros/pré-requisitos produz mensagem; não criar documentos para suprir ausência. Cancelamento não deixa processo da execução ativo.

Artefatos atuais: spec, plan, research, checklist/reviews e gates. Phase 1 (data-model.md, contracts/, quickstart.md) permanece pendente de V1-A/B; não gerar tasks ou implementação nesta alteração. Evidências por história futuras: evidence/USn-validation.md, evidence/USn-review.md, validation/USn-dev.md, acceptance/USn.md. Evidência técnica de desenvolvimento pode ser persistida no FEATURE_DIR; não contorna a regra do produto de Salvar.

## Matriz de validação

Todos os cenários abaixo são PROPOSTOS/NÃO EXECUTADOS no produto. Reutilizar testes adequados existentes, sem atribuir resultados do worker antigo à V1. Dados sempre sintéticos e externos a solução real. A matriz lista uma entrada por critério ativo; US7 está adiada.

| História | Critério/cenário | Nível e teste a criar/reutilizar | Execução e asserção esperada | Evidência |
|---|---|---|---|---|
| US1 | US1-AC01 / US1-S01 | Integração Windows + E2E; caso US1-S01 | Preparar Projeto com duas funcionalidades; Carregar com destino externo; observar: Mostrar artefatos e escopos, sem escrita na origem. | evidence/US1-validation.md |
| US1 | US1-AC02 / US1-S02 | Integração Windows + E2E; caso US1-S02 | Preparar Link que resolve para origem e executor restrito; Escolher destino ou tentar escrita; observar: Recusar e preservar conteÃºdo. | evidence/US1-validation.md |
| US1 | US1-AC03 / US1-S03 | Integração Windows + E2E; caso US1-S03 | Preparar Dois projetos e histÃ³rico salvo; Alternar projeto e mudar destino; observar: Manter contextos e arquivos anteriores. | evidence/US1-validation.md |
| US1 | US1-AC04 / US1-S04 | Integração Windows + E2E; caso US1-S04 | Preparar AnÃ¡lise em andamento; UsuÃ¡rio editar documento monitorado; observar: Preservar ediÃ§Ã£o e indicar perda de atualidade. | evidence/US1-validation.md |
| US2 | US2-AC01 / US2-S01 | Domínio + E2E visual/documental; caso US2-S01 | Preparar Links vÃ¡lidos e quebrados; Buscar/navegar; observar: Abrir destino autorizado ou explicar ausÃªncia. | evidence/US2-validation.md |
| US2 | US2-AC02 / US2-S02 | Domínio + E2E visual/documental; caso US2-S02 | Preparar Corpus conhecido; Alternar modos; observar: Preservar equivalÃªncia do conteÃºdo e origem. | evidence/US2-validation.md |
| US2 | US2-AC03 / US2-S03 | Domínio + E2E visual/documental; caso US2-S03 | Preparar Diagramas vÃ¡lidos/invÃ¡lidos e nÃ£o suportados; Abrir offline; observar: Renderizar suportado e explicar demais. | evidence/US2-validation.md |
| US2 | US2-AC04 / US2-S04 | Domínio + E2E visual/documental; caso US2-S04 | Preparar Script e imagem remota; Abrir e depois autorizar imagem; observar: Nenhum script; rede sÃ³ apÃ³s aÃ§Ã£o. | evidence/US2-validation.md |
| US3 | US3-AC01 / US3-S01 | Domínio + E2E visual/documental; caso US3-S01 | Preparar 30/40 tarefas marcadas e 12/20 requisitos vinculados; Abrir painel e escopo vazio; observar: Mostrar 75% declarado e 60% de cobertura documental; vazio sem percentual. | evidence/US3-validation.md |
| US3 | US3-AC02 / US3-S02 | Domínio + E2E visual/documental; caso US3-S02 | Preparar Registro READY e declaraÃ§Ã£o humana em documento; Consultar painel; observar: Exibir procedÃªncia, sem implementaÃ§Ã£o/teste/aceite comprovados. | evidence/US3-validation.md |
| US3 | US3-AC03 / US3-S03 | Domínio + E2E visual/documental; caso US3-S03 | Preparar ContradiÃ§Ã£o, vÃ­nculo ausente e referÃªncia a cÃ³digo; Abrir ocorrÃªncias; observar: Mostrar fontes e limite documental, sem ler cÃ³digo. | evidence/US3-validation.md |
| US3 | US3-AC04 / US3-S04 | Domínio + E2E visual/documental; caso US3-S04 | Preparar Resultado anterior e documento alterado; Atualizar documento e filtrar; observar: Atualizar contagens e marcar verificaÃ§Ã£o desatualizada, sem executar agente/gate. | evidence/US3-validation.md |
| US4 | US4-AC01 / US4-S01 | Domínio + E2E visual/documental; caso US4-S01 | Preparar Requisito com vÃ­nculos explÃ­citos/inferidos; Abrir ficha; observar: Diferenciar fontes e mostrar registros/sugestÃµes. | evidence/US4-validation.md |
| US4 | US4-AC02 / US4-S02 | Domínio + E2E visual/documental; caso US4-S02 | Preparar CritÃ©rio sem tarefa e outro ambÃ­guo; Consultar critÃ©rios; observar: Informar lacunas documentais sem classificar implementaÃ§Ã£o. | evidence/US4-validation.md |
| US4 | US4-AC03 / US4-S03 | Domínio + E2E visual/documental; caso US4-S03 | Preparar Documento declara validaÃ§Ã£o sem prova acessÃ­vel; Consolidar; observar: Mostrar declaraÃ§Ã£o e limite, sem aprovaÃ§Ã£o pelo SpecAudit. | evidence/US4-validation.md |
| US4 | US4-AC04 / US4-S04 | Domínio + E2E visual/documental; caso US4-S04 | Preparar Documentos de projetos em linguagens diferentes e link .cs; Navegar; observar: Mesma anÃ¡lise documental; link .cs nÃ£o lido. | evidence/US4-validation.md |
| US5 | US5-AC01 / US5-S01 | Integração Windows + E2E; caso US5-S01 | Preparar Agente encontrado e outro ausente; Abrir seleÃ§Ã£o e escolher gate; observar: Descoberta automÃ¡tica, fallback restrito; gate nÃ£o pede agente. | evidence/US5-validation.md |
| US5 | US5-AC02 / US5-S02 | Integração Windows + E2E; caso US5-S02 | Preparar Dois agentes reais e integraÃ§Ã£o incompatÃ­vel; Executar analyze; observar: Demonstrar contrato ou bloquear apenas o adaptador com motivo. | evidence/US5-validation.md |
| US5 | US5-AC03 / US5-S03 | Integração Windows + E2E; caso US5-S03 | Preparar VerificaÃ§Ãµes e ficha abertas; Alternar combo, tentar terceira opÃ§Ã£o e clicar Executar; observar: SÃ³ duas opÃ§Ãµes, seleÃ§Ã£o coerente; execuÃ§Ã£o apenas por clique. | evidence/US5-validation.md |
| US5 | US5-AC04 / US5-S04 | Integração Windows + E2E; caso US5-S04 | Preparar Documentos com instruÃ§Ãµes maliciosas e tasks ausente; Analisar; observar: NÃ£o executar instruÃ§Ãµes/ler cÃ³digo; informar prÃ©-requisito sem criar arquivos. | evidence/US5-validation.md |
| US5 | US5-AC05 / US5-S05 | Integração Windows + E2E; caso US5-S05 | Preparar Agente sem autenticaÃ§Ã£o ou em execuÃ§Ã£o; Cancelar/consultar leitor e gate; observar: Mostrar estado real, encerrar descendentes e manter funÃ§Ãµes locais. | evidence/US5-validation.md |
| US5 | US5-AC06 / US5-S06 | Integração Windows + E2E; caso US5-S06 | Preparar Registros conhecidos com READY, BLOCKED, hash adulterado e revisÃ£o divergente; Selecionar etapa/registro/revisÃ£o e executar gate; observar: Resultado correto e delimitado, sem inventar aprovaÃ§Ã£o humana/implementaÃ§Ã£o. | evidence/US5-validation.md |
| US5 | US5-AC07 / US5-S07 | Integração Windows + E2E; caso US5-S07 | Preparar Rede desativada e evidÃªncias documentais, .cs, link externo e alteraÃ§Ã£o concorrente; Executar gate; observar: Documental funciona offline; demais bloqueiam ou invalidam sem ler destinos proibidos. | evidence/US5-validation.md |
| US6 | US6-AC01 / US6-S01 | Integração Windows + E2E; caso US6-S01 | Preparar Resultado em sessÃ£o; Fechar sem salvar e repetir/salvar; observar: Primeiro nÃ£o gera registro oficial; segundo fica no destino externo; estado interno do agente permanece externo. | evidence/US6-validation.md |
| US6 | US6-AC02 / US6-S02 | Integração Windows + E2E; caso US6-S02 | Preparar Resultado revisado; Exportar trÃªs formatos; observar: ConteÃºdo rastreÃ¡vel e origem intacta. | evidence/US6-validation.md |
| US6 | US6-AC03 / US6-S03 | Integração Windows + E2E; caso US6-S03 | Preparar OcorrÃªncia; Classificar sem/com justificativa e mudar contexto; observar: Exigir justificativa e nova revisÃ£o quando afetada. | evidence/US6-validation.md |
| US6 | US6-AC04 / US6-S04 | Integração Windows + E2E; caso US6-S04 | Preparar Duas anÃ¡lises salvas; Comparar/excluir uma; observar: Exibir diferenÃ§as e preservar demais registros. | evidence/US6-validation.md |
| US8 | US8-AC01 / US8-S01 | Domínio + E2E visual/documental; caso US8-S01 | Preparar Projeto carregado; Percorrer cinco Ã¡reas; observar: Manter contexto e proteÃ§Ã£o identificÃ¡veis. | evidence/US8-validation.md |
| US8 | US8-AC02 / US8-S02 | Domínio + E2E visual/documental; caso US8-S02 | Preparar Ficha com ocorrÃªncia selecionada; Trocar tema; observar: Preservar seleÃ§Ã£o e entendimento dos estados. | evidence/US8-validation.md |

Para US5-S06, usar registros locais CEMG conhecidos: READY com hash correto; BLOCKED com bloqueio declarado; estado READY com evidence hash adulterado; revisão divergente; registro ausente. Comparar saída/exit do script empacotado, captura da UI e ausência de qualquer escrita. Para US5-S07, incluir referência .cs, junção para fora e referência aninhada em logs/artifacts; instrumentar abertura real e comprovar que o destino proibido não foi lido. Gate nunca recebe `--project`.

Para US1-S02/US5-S02/S04, ensaiar tentativas reais de escrita/acesso indevido em fixture por aplicação, adaptador e descendentes, observar conteúdo/hashes e permitir edição normal pelo usuário. Mocks não comprovam fronteira Windows nem integração real. Provar que um payload documental não libera ferramenta ou configura agente. Simulação de erro serve apenas à apresentação.

## Ambiente e dados

Preparação proposta: usuário normal Windows, navegador local, corpus sintético com duas funcionalidades, 40 tarefas (30 marcadas), 20 requisitos (12 vinculados), spec/plan/tasks, gates READY/BLOCKED, links válidos/quebrados e Markdown/Mermaid malicioso/ inválido. Incluir arquivo .cs canário que não deve ser lido; seu conteúdo não entra em corpus/análise. O teste de desenvolvimento pode instrumentar a fixture, o produto não a executa. Criar destino externo e link inválido para origem. Baseline e observação em diretório técnico externo, sem restaurar a origem para ocultar alterações.

Inicialização futura do aplicativo a partir da raiz de desenvolvimento:
`dotnet run --project SpecAudit/SpecAudit.csproj --no-launch-profile -- --urls http://127.0.0.1:5187`.
Comando do próprio SpecAudit, não da solução auditada. Nenhuma demonstração funcional executada nesta revisão. Antes de disponibilizar, definir proteção local de sessão, Host/origem, antiforgery, picker de pasta e contratos; loopback somente, sem exposição LAN.

Gate de desenvolvimento documental nesta feature usa Python existente `__USER_HOME__/anaconda3_11/python.exe` e `.specify/presets/cemg-speckit/tools/check_gate.py`, com --feature/--record/--stage/--revision, SEM --project. Isso valida os registros de planejamento deste repositório; não é prova do executor empacotado ou da UI futura.

Agentes: usar autenticação existente exclusivamente pela integração validada; não ler/copiar tokens para relatórios. Gate não precisa de agente ou credencial. Registrar versões e locais externos reais de cache/sessão antes de prova do analyze. Nenhuma instalação/elevação automática autorizada. IA e DEV precisam dos mesmos dados/entrada/resultado/efeitos; roteiro final no quickstart após pesquisa. Limpeza: encerrar host e descendentes próprios; excluir somente fixtures previamente resolvidas dentro de raiz sintética expressamente identificada; nunca origem real ou arquivos do usuário.

## Viabilidade por história

| História | IA executa agora? | DEV reproduz agora? | Impedimento | Próximo trabalho |
|---|---|---|---|---|
| US1 | Somente inspeção documental | Não no produto | V1-A, fronteira/catálogo | Provar preservação e escopo sem ACL na solução |
| US2 | Corpus/expectativas documentados | Após implementação | Leitor/catálogo/preview | Provar sanitização e caminhos |
| US3 | Fórmulas inspecionadas | Após implementação | Domínio/UI | Demonstrar contagens e atualidade |
| US4 | Requisitos documentados | Após implementação | Fontes/vínculos/UI | Demonstrar ficha sem código |
| US5 | check_gate de desenvolvimento executável; integração final não | Não no produto | V1-A/B e adaptadores | Provar analyze restrito e gate empacotado/offline |
| US6 | Não no produto | Após implementação | Writer/fluxo Salvar | Provar memória até clique e histórico externo |
| US7 | Fora da V1 | Fora da V1 | Escopo adiado | Nova revisão futura |
| US8 | Não no produto | Após implementação | Telas ainda scaffold | Demonstrar cinco áreas/dois temas |

## Estratégia de revisão

Autorrevisão documental nesta revisão; aprovação humana de PR e aceite de história separados. Conferir diff, origem intacta, catálogo, reparse/TOCTOU, referências aninhadas do gate, ausência de shell/hooks, rede/credenciais do agente, persistência externa e rótulos honestos. Achados com arquivo/linha/efeito/evidência. Impeditivos: qualquer acesso a código na V1, escrita na origem, relatório oficial antes de Salvar, agente sem proteção comprovada habilitado, caminho arbitrário, execução de código/hooks da origem, READY falso ou resultado sem limites. Revalidar testes/hashes afetados após mudança; não conceder aceite por hash ou documentação.

## Dependências e sequência de entrega

1. Fechar V1-A: catálogo/broker somente leitura e adaptador documental sem ferramentas arbitrárias; prova individual Codex/Antigravity, com rede/estado externo/cancelamento e descendentes delimitados. Não depende de repetir WFP elevado; se o mecanismo escolhido exigir elevação, nova decisão/autorização é necessária.
2. Fechar V1-B: gate confiável empacotado, runtime/argumentos e leitura do grafo exclusivamente documental, offline e sem --project/subprocessos externos; comprovar registros positivos/negativos.
3. Finalizar Phase 1, contratos UI/combos/gate/agentes/Salvar e quickstart reproduzível; reavaliar constituição e gate plan. Só depois gerar tasks.
4. Entregar leitor/contagens/temas com fronteira comprovada; gate local pode ser demonstrado independentemente de disponibilidade do agente. Analyze permanece desabilitado enquanto sua integração não passar.
5. Integrar analyze e concluir ficha/sugestões/Salvar/histórico e demonstração integrada pela IA/DEV. Não incluir análise de código ou consulta de dependências da solução.

## Complexidade e exceções

Nenhuma exceção à segurança. A lista de dois comandos é restrição de produto, não sandbox. Prompt/allowlist nominal, detecção posterior por hash ou escrita seguida de restore não cumprem somente leitura. O mediador WFP é pesquisa histórica para cenário amplo/futuro, sem sucesso funcional declarado. Não remover logs/provas antigas nem reaproveitar autorização elevada consumida. Não prometer que limitar comandos comprova isolamento do agente.

## Estratégia de entrega em PRs

| PR previsto | Objetivo/escopo | US/critério | Tasks previstas | Base/dependência | Verificação/evidência | Revisão | Integração | Fecha história? |
|---|---|---|---|---|---|---|---|---|
| PR-A | Fronteira/catálogo, seleção e invalidação | US1 | Áreas; IDs somente em tasks | main; V1-A resolvido | Matriz US1 e acessos reais | IA autorrevisão + humano | Sem impeditivos, checks executados e aprovação humana | US1 |
| PR-B | Preview, contagens, ficha e temas | US2/3/4/8 | Leitor/domínio/UI | PR-A | Matriz correspondente, dois temas e zero leitura de código | IA + humano | Mesmas condições; analyze ainda desabilitado | Fluxos documentais |
| PR-C | Executor gate e combo de duas opções | US5-AC03/06/07 e parte AC05 | Gate/contrato/UI | PR-A/B; V1-B resolvido | Registros offline válidos/negativos e combo sincronizada | IA + humano | Sem IA obrigatória para gate; analyze protegido por indisponibilidade | Parcial US5 |
| PR-D | Descoberta/analyze real protegido | US5-AC01/02/04/05; integração AC03 | Adaptadores e cancelamento | PR-A/C; V1-A agentes comprovado | Ambos reais e estado externo | IA + humano | Não habilitar incompatível | US5 |
| PR-E | Salvar/exportar/histórico e fluxo final | US6 e regressão ativa | Writer/decisões/fechamento | PR-B/C/D | Matriz US6 + cenário integrado e roteiro DEV | IA + humano | Fluxo real pela IA; DEV reproduz/aceita separadamente | US6 e conjunto |

Divisão por escopo, risco e testabilidade; sem metas de linhas/duração. Cada PR preserva estado intermediário seguro; código/testes/evidências juntos. Mudança de base exige renovar verificações. Nenhum PR aberto ou implementação feita nesta revisão.

## Preservação do planejamento técnico

research.md registra decisão nova e mantém provas históricas. Modelo, contratos e quickstart finais aguardam V1-A/B e pesquisa completa. Não gerar esses artefatos como se isolamento já tivesse sido provado; não substituir pesquisa/design por matriz de testes.

## Condição de saída da etapa plan

BLOCKED para tasks: spec atual com critérios/cenários pode ser READY, mas faltam pesquisa da fronteira documental, acesso seguro do gate e design/roteiro executável finais. Atualizar verification/plan-stage.json e executar gate com hashes reais; não aprovar checks pendentes. A revisão de escopo documental está concluída, sem afirmar produto funcional.

## Plano de execução do produto

Após implementação, IA inicia host loopback e carrega corpus/destino externo, navega Documentos/Dashboard/Requisitos, alterna temas, seleciona cada uma das duas opções nas duas telas, confirma escopo do gate, executa, observa estados/fontes/efeitos/cancelamento; fecha sem Salvar e comprova ausência de registros oficiais; repete e clica Salvar para comprovar exportações/histórico. DEV repete mesma revisão/ambiente/dados, observa resultados e decide aceite separado. Sem execução real, manter NÃO EXECUTADO com causa e próximo passo; IA não confirma impossibilidade em nome do DEV.
