# Plano de implementação: Auditoria de projetos SpecKit

**Branch Git real**: main | **Contexto lógico retornado pelo setup**: 001-auditar-projetos-speckit
**Data**: 2026-11-15 | **Spec**: [spec.md](spec.md) | **Estado**: BLOQUEADO, pesquisa Phase 0 incompleta
**Preset**: cemg-speckit 3.0.0 | **Template**: plan-template | **Composição**: replace
**Fonte**: .specify/presets/cemg-speckit/templates/plan-template.md
**Resolvedor**: .specify/scripts/powershell/resolve-template.ps1
**Base**: 460af9e3e38d4cb200517c45424c7569b377a322; constituição 2.0.0.

## Resumo

Planejar um aplicativo Blazor local para consultar documentos SpecKit e evidências no código, mantendo a origem intacta e resultados transitórios até Salvar. Este rascunho registra pesquisa e planejamento independente. Não autoriza implementação, não declara compatibilidade dos agentes e não altera a especificação para contornar restrições.

B1 foi encerrado por emenda explícita do usuário: conversas/caches internos dos agentes podem persistir fora da origem. A prova confirmou esse comportamento no Antigravity; ele não reprova a política nova. B2 permanece: executor Windows, agentes, descendentes e rede precisam de comprovação conjunta sem alterar permissões do usuário ou copiar a solução. Phase 1 permanece pendente.

## Contexto técnico

- Linguagem/versão: C# 14, net10.0, sem previews; manter patches estáveis e auditados. Ambiente encontrado: SDK 10.0.404, ASP.NET Core 10.0.15, win-x64.
- Dependências: scaffold Blazor Interactive Server existente. Candidatos a pesquisar/fixar após gates: Roslyn para sintaxe, parser Razor compatível, Markdig, sanitizador, Mermaid local. Nenhum pacote adicionado.
- Armazenamento: estado de análise em memória; pacotes externos somente após Salvar; sem banco/telemetria de conteúdo próprio do SpecAudit antes da ação; conversas/caches internos dos agentes são permitidos exclusivamente fora da origem.
- Ferramentas de teste propostas: testes de domínio, integração Windows real e navegação automatizada de navegador; framework e versões devem ser fixados/auditados após resolver B2. Nenhuma execução funcional nesta etapa.
- Plataforma/tipo: aplicativo local Windows x64, Blazor Interactive Server; descoberta automática de agentes instalados; picker de pastas nativo mediado pelo aplicativo.
- Desempenho/escala: nenhuma meta numérica foi aprovada; não inventar throughput/latência. Cancelamento, limites de memória/saída/tempo e mensagens de limite precisam ser detalhados após experimento dos agentes sem modificar aceite silenciosamente.
- Rede: somente serviços do agente escolhido, fontes de avisos e recursos remotos explicitamente solicitados no preview; serviços da solução permanecem bloqueados.
- Pendências técnicas: B2 em research.md. Prova requer agentes reais, autenticação e observação de filesystem/processos/rede; ajuda CLI não é prova funcional.

## Verificação da constituição

| Princípio | Antes do design | Situação após pesquisa |
|---|---|---|
| I — origem intacta | Obrigatório; sem write/restore/cópia | B2: mecanismo candidato ainda não comprovado |
| II — externo e Salvar | Relatórios oficiais só por Salvar; estado interno externo permitido | B1 encerrado por emenda; verificar caminhos reais das gravações do agente como parte de B2 |
| III — evidências | Caminho/linhas/hash, inferência explícita | Planejado; não implementado |
| IV — indicadores/validação | Sem execução da origem ou falsa aprovação | Preservado no rascunho e matriz |
| V — agentes | Ambos obrigatórios e descoberta automática | Descobertos; execução protegida não comprovada |
| VI — segurança/privacidade | Acesso mínimo, conteúdo não confiável | B2 inclui autenticação/rede/descendentes |
| VII — vulnerabilidades | Consultar sem afirmar ausência absoluta | Consulta de pacotes executada; componentes futuros ainda não selecionados |
| VIII — decisões humanas | Rastreáveis, sem aceite inferido | Planejado; sem decisões humanas forjadas |
| IX — desenvolvimento validado | IA executa e DEV reproduz produto | Procedimentos futuros; não executados nesta fase |

Não há exceção concedida. O gate constitucional NÃO está aprovado para design final dependente. Reavaliar integralmente após provas e design Phase 1.

## Estrutura e artefatos

Existente: SpecAudit.slnx; SpecAudit/ como host Blazor; Components/ com scaffold. Não alterar fontes nesta etapa.

Fronteiras candidatas, sem impor estrutura final antes da pesquisa: host/UI local, domínio de análise, leitor restrito, executor isolado, adaptadores por agente e writer externo acionado por Salvar. A interface deve trabalhar por IDs de projeto, sem endpoints de leitura por caminho arbitrário. Roslyn opera sobre bytes, sem MSBuildWorkspace/targets/analyzers do projeto.

Artefatos desta fase: plan.md, research.md, verification/plan-review.md e verification/plan-stage.json. Phase 1 ainda não gerada: data-model.md, contracts/ e quickstart.md, dependentes de pesquisa resolvida. Não gerar tasks nesta invocação.

Artefatos futuros por história: evidence/USn-validation.md, evidence/USn-review.md, validation/USn-dev.md, acceptance/USn.md; cada execução referencia versão/base e resultados reais. Evidências de desenvolvimento ficam na feature; isso não autoriza persistir análises do produto antes de Salvar.

## Matriz de validação

Todos os testes abaixo estão PROPOSTOS/NÃO EXECUTADOS. Os cenários são da spec. Fixture significa dados sintéticos de desenvolvimento, nunca solução real do usuário. Plan ainda não é READY: procedimentos executáveis finais serão consolidados no quickstart após B2.

| História | Critério/cenário | Nível de teste | Teste existente ou a criar | Execução | Resultado esperado | Evidência |
|---|---|---|---|---|---|---|| US1 | US1-AC01 / US1-S01 | E2E | A criar: LoadProject | Selecionar fixture com duas funcionalidades e destino externo | Mostrar artefatos presentes/ausentes sem escrita | evidence/US1-validation.md |
| US1 | US1-AC02 / US1-S02 | Integração Windows | A criar: ReadOnlyBoundary | Tentar create/write/rename/delete na origem, por filho e via junction | Todas negadas; hashes intactos; editor continua gravando | evidence/US1-validation.md |
| US1 | US1-AC03 / US1-S03 | E2E | A criar: ProjectIsolation | Alternar projetos A/B e trocar pasta externa de A | Sem mistura, movimento ou exclusão automática | evidence/US1-validation.md |
| US1 | US1-AC04 / US1-S04 | Integração/E2E | A criar: SourceInvalidation | Editar fonte pelo usuário durante leitura e antes de salvar | Edição preservada; resultado afetado desatualizado | evidence/US1-validation.md |
| US2 | US2-AC01 / US2-S01 | E2E | A criar: DocumentNavigation | Pesquisar e seguir links relativos/âncoras válidos, quebrados e externos ao escopo | Abrir só autorizado; explicar ausências | evidence/US2-validation.md |
| US2 | US2-AC02 / US2-S02 | E2E visual | A criar: MarkdownModes | Abrir corpus com tabela/lista/código/imagem e alternar modos | Equivalência de conteúdo; sem controles de edição | evidence/US2-validation.md |
| US2 | US2-AC03 / US2-S03 | Integração/E2E | A criar: MermaidOffline | Abrir Mermaid válido/inválido e PlantUML sem internet | Somente Mermaid válido renderiza, fonte acessível nos demais | evidence/US2-validation.md |
| US2 | US2-AC04 / US2-S04 | Segurança E2E | A criar: PreviewSafety | Abrir HTML/script/imagem remota e depois autorizar imagem | Nenhum script/rede automática; somente ação explícita sem credenciais | evidence/US2-validation.md |
| US3 | US3-AC01 / US3-S01 | Unidade/E2E | A criar: ProgressDenominators | Carregar 30/40 tarefas e 12/20 requisitos mais escopo vazio | 75%,60%, contagens corretas; vazio sem divisão | evidence/US3-validation.md |
| US3 | US3-AC02 / US3-S02 | Unidade/E2E | A criar: ValidationProvenance | Comparar aprovação documental e arquivo de teste não executado | Procedência visível; nenhum teste inventado | evidence/US3-validation.md |
| US3 | US3-AC03 / US3-S03 | Integração/E2E | A criar: DocumentFindings | Analisar contradição, referência inválida e requisito sem tarefa | Ocorrências com fontes, sem remediação | evidence/US3-validation.md |
| US3 | US3-AC04 / US3-S04 | Integração/E2E | A criar: DocumentRefresh | Editar documento, esperar pausa, editar código e filtrar | Documento atualiza; código pede análise; filtros mantêm escopo | evidence/US3-validation.md |
| US4 | US4-AC01 / US4-S01 | E2E | A criar: RequirementTrace | Abrir requisito com vínculo explícito e sugerido | Ambos rotulados e fontes navegáveis | evidence/US4-validation.md |
| US4 | US4-AC02 / US4-S02 | Integração real | A criar: ImplementationEvidence | Analisar filtro com data inicial mas sem final e placeholder isolado | Parcial com evidência; sem ausência total inventada | evidence/US4-validation.md |
| US4 | US4-AC03 / US4-S03 | Unidade/E2E | A criar: AcceptanceAggregation | Consolidar implementação completa e um critério sem validação | Evidenciada com validação parcial, nunca aprovação integral | evidence/US4-validation.md |
| US4 | US4-AC04 / US4-S04 | Integração real | A criar: RazorFlow | Relacionar rota/componente/entrada/regra C#; comparar outra linguagem | Fluxo rastreável; limites e suporte parcial visíveis | evidence/US4-validation.md |
| US5 | US5-AC01 / US5-S01 | Integração/E2E | A criar: AgentDiscovery | Detectar instalações reais e simular ausência de um agente | Encontrados automáticos; fallback manual só para ausente | evidence/US5-validation.md |
| US5 | US5-AC02 / US5-S02 | Integração real Windows | A criar: AgentCapability | Executar agentes autenticados sob restrições com rastreamento | Automação/saída/cancelamento e estado interno externo e zero relatório oficial sem Salvar comprovados | evidence/US5-validation.md |
| US5 | US5-AC03 / US5-S03 | Integração real/E2E | A criar: VerificationSelector | Selecionar cada tipo/escopo e executar com fonte adaptada visível | Resultados e estado sem editar artefatos | evidence/US5-validation.md |
| US5 | US5-AC04 / US5-S04 | Segurança real | A criar: UntrustedInputAndPersistence | Fornecer instruções maliciosas e tokens sintéticos e observar arquivos/rede | Sem scripts/serviços/segredos herdados ou sessões gravadas | evidence/US5-validation.md |
| US5 | US5-AC05 / US5-S05 | Integração/E2E | A criar: AgentFailures | Desautenticar fixture ou cancelar execução; ler documentos | Estado real e descendentes encerrados; consultas locais disponíveis | evidence/US5-validation.md |
| US6 | US6-AC01 / US6-S01 | Integração/E2E | A criar: ExplicitSave | Gerar conteúdo-marcador; fechar sem Salvar; repetir e Salvar | Nenhum registro oficial antes; estado do agente externo; novo pacote externo depois | evidence/US6-validation.md |
| US6 | US6-AC02 / US6-S02 | Integração/E2E | A criar: ExportFormats | Salvar amostra em Markdown,JSON,CSV e conferir manifesto | Conteúdo/procedência completos; origem intacta | evidence/US6-validation.md |
| US6 | US6-AC03 / US6-S03 | Unidade/E2E | A criar: FindingDecisions | Classificar risco/falso positivo sem e com motivo; mudar fonte | Motivo obrigatório e revisão reaberta; estado técnico preservado | evidence/US6-validation.md |
| US6 | US6-AC04 / US6-S04 | Integração/E2E | A criar: SavedHistory | Comparar dois registros e excluir um selecionado | Novas/persistentes/resolvidas corretas; outros intactos | evidence/US6-validation.md |
| US7 | US7-AC01 / US7-S01 | Integração real | A criar: AdvisoryCoverage | Ler inventário e grafo incompleto; consultar fonte real | Aviso com versão e cobertura incompleta explícita | evidence/US7-validation.md |
| US7 | US7-AC02 / US7-S02 | Integração/E2E | A criar: AdvisoryFreshness | Abrir registro antigo, mudar dependências e atualizar manualmente | Consulta devida sem autosave; sem registro não presumir consulta prévia | evidence/US7-validation.md |
| US7 | US7-AC03 / US7-S03 | Integração/E2E | A criar: AdvisoryOutage | Indisponibilizar fonte após consulta e comparar alertas | Última data/limitação; não declarar seguro nem repetir aviso continuamente | evidence/US7-validation.md |
| US8 | US8-AC01 / US8-S01 | E2E visual | A criar: AppNavigation | Percorrer cinco áreas com projeto carregado | Projeto/destino/somente leitura sempre identificáveis | evidence/US8-validation.md |
| US8 | US8-AC02 / US8-S02 | E2E visual | A criar: ThemeSwitch | Alternar escuro/claro com ficha e ocorrência selecionadas | Conteúdo/seleção preservados; estados com texto/ícone | evidence/US8-validation.md |

## Ambiente e dados

Estado encontrado: Windows x64, SDK 10.0.404, runtime ASP.NET Core 10.0.15, Codex 0.147.0 e agy 1.1.10. Usar versões estáveis; nenhuma dependência instalada neste plan.

Procedimento proposto de preparação: criar corpus sintético fora de qualquer solução real, com duas funcionalidades, 40 tarefas (30 concluídas), 20 requisitos (12 evidenciados), links válidos/inválidos e amostras C#/Razor completas/parciais. Criar destino externo vazio e registrar hashes baseline. Tokens são marcadores sintéticos, nunca credenciais reais. Bloquear serviços da fixture por construção.

Acesso dos agentes usa autenticação existente delimitada; não ler/copiar seus tokens para evidências. Não carregar regras/configurações da fixture. Para B2, executar prompt sintético identificável e monitorar filesystem, processos, descendentes e egress; verificar caches/sessões/logs em locais realmente usados pelos CLIs, inclusive fora do destino escolhido. Redirecionar TEMP/HOME não basta. Cancelar e repetir; restaurar apenas dados sintéticos por ação de limpeza, nunca origem real.

Inicialização futura do próprio produto, a partir da raiz: dotnet run --project SpecAudit/SpecAudit.csproj --no-launch-profile -- --urls http://127.0.0.1:5187. Este comando identifica o caminho de desenvolvimento existente, NÃO foi executado como comprovação das funcionalidades planejadas. Antes da demonstração funcional o plan final deve resolver proteção local da sessão, Host/origem, antiforgery e seletores de pasta. Acesso pelo DEV ao mesmo loopback; sem exposição na rede.

A IA deve observar cada resultado esperado da matriz e entregar os mesmos passos ao DEV; ambos identificam commit/revisão, ambiente, dados, inicialização, entrada, resposta e efeitos. Evidência de desenvolvimento pode ser persistida no FEATURE_DIR; não confundir com resultados do produto antes de Salvar.

Limpeza futura: encerrar host/descendentes, remover apenas fixtures e saídas de teste explicitamente identificadas sob raiz sintética; nunca comandos de exclusão sobre origem real. Mocks cobrem domínio/erros, não integram agentes, isolamento ou fonte real de avisos.

## Viabilidade por história

| História | IA consegue executar? | DEV consegue reproduzir? | Impedimento atual | trabalho para resolver |
|---|---|---|---|---|
| US1 | Parcial, pesquisa; produto não existe | Após implementação/prova | B2 | Provar broker/executor e reparse paths sem ACL da origem |
| US2 | Parcial, corpus e parser propostos | Após implementação | Proteção de leitura e preview pendentes | Resolver B2 e implementar leitor seguro |
| US3 | Fórmulas verificáveis; fluxo não existe | Após implementação | Domínio/UI pendentes | Implementar fixtures e asserções da matriz |
| US4 | Não no fluxo real atual | Após US5 e implementação | B2 e extração Razor | Provar agentes, definir parser e preservar limites |
| US5 | Análise sintética real executada por ambos | Harness reproduzível; produto final pendente | B2 parcial; persistência externa autorizada | Ensaiar agentes sob fronteira completa e validar locais externos |
| US6 | Não no produto atual | Após implementação | Writer e prova de Salvar explícito | Demonstrar relatório oficial em memória até Salvar e estado de agente externo |
| US7 | Consulta de pacotes própria executada; produto não | Após implementação | Inventário/fonte e UI pendentes | Implementar leitura sem restore e integração real |
| US8 | Não no produto atual | Após implementação | Telas são scaffold | Implementar e demonstrar dois temas |

Nenhuma linha concede aceite ou declara capacidade implementada. Instrumentação de arquivos/rede ainda precisa ser preparada; ausência de ferramenta mantém prova pendente.

## Estratégia de revisão

Comparar diff ao commit base e registrar revisão candidata por hashes enquanto não houver novo commit. Revisar origem intacta, caminhos reais, entrada não confiável, logs/persistência, rede e descendentes, procedência das conclusões e regras de denominador. Revisão atual é autorrevisão documental; pesquisa por agentes não equivale a revisão independente de implementação.

Futuros achados devem registrar arquivo/linha, efeito, prioridade, evidência e resolução. São impeditivos: qualquer escrita na origem, relatório oficial do SpecAudit persistido antes de Salvar, execução de solução/serviços, acesso indevido, teste exigido falho, integração obrigatória sem prova e evidência desatualizada. Correção ou mudança de base exige renovar checks afetados; aprovação humana por PR e aceite pelo DEV permanecem separados.

## Dependências e sequência de entrega

1. Resolver B2 por prova técnica controlada e reavaliar a constituição.
2. Somente com pesquisa completa: finalizar Phase 1 (modelo, contratos de UI/brokers/adaptadores/salvamento e quickstart executável).
3. Revalidar plan e então gerar tasks; nenhuma task ou implementação é criada aqui.
4. Implementar fronteiras de segurança antes de expor leitura/agentes; leitor/indicadores independem de análise interpretativa.
5. Integrar agentes após proteção demonstrada; consolidar ficha, salvamento, histórico, consultas e temas; validar cada incremento e fluxo final.

## Complexidade e exceções

O isolamento por processo e broker separado é justificado pela exigência de proteger a origem sem alterar permissões do usuário. Job Object não substitui fronteira de filesystem. MSBuildWorkspace foi descartado para evitar execução de targets do projeto. A emenda 2.0.0 autoriza estado interno externo dos agentes; não flexibiliza origem intacta nem Salvar dos relatórios oficiais. Não prometer ausência física de resíduos em pagefile/hibernação/dumps do Windows: requisitos de não persistência são implementados nos componentes controlados e limitações de máquina precisam ser discutidas caso alterem o aceite.

## Estratégia de entrega em PRs

Divisão candidata por comportamento e risco, não por quantidade de linhas ou duração. IDs de tasks serão definidos somente em tasks. Todas as linhas exigem estado intermediário seguro; não habilitar adaptador sem prova.

| PR previsto | Objetivo e escopo | US e critérios parciais ou finais | Tasks previstas | Base e dependências | Testes e evidências | Revisor e modalidade | Condição de integração | Completa a história? |
|---|---|---|---|---|---|---|---|---|
| PR-A | Fronteiras, seleção segura e invalidação | US1 | Áreas de broker, caminhos e fixtures | Base main; B2 resolvidos | Casos US1; evidence/US1-validation.md | IA autorrevisão + humano | Sem impeditivos e aprovação humana | US1 ao cobrir todos critérios |
| PR-B | Documentos, dashboard e navegação inicial | US2/US3; US8 parcial | Leitor, domínio, preview e shell | PR-A integrado | US2/US3 e regressão US1 | IA + humano | Proteção/preview e fórmulas verificadas | US2/US3; US8 pendente |
| PR-C | Dois adaptadores e verificação por escopo | US5; US4 parcial | Descoberta, execução, captura e cancelamento | PR-A/B e provas reais | US5 com CLIs reais e segurança | IA + humano | Nenhum relatório oficial sem Salvar; sessões/caches externos permitidos | US5 somente com ambos comprovados |
| PR-D | Ficha, decisões e persistência externa | US4/US6 | Evidências, agregação, writer/exportações/histórico | PR-C e contratos finais | US4/US6; regressão de proteção | IA + humano | Rastreabilidade e Salvar íntegros | US4/US6 |
| PR-E | Avisos, temas e fechamento integrado | US7/US8 e regressão completa | Inventário, fonte, alertas, temas e roteiro DEV | PR-D | US7/US8; fluxo completo, IA e DEV | IA + humano | Checks atuais, roteiro e aceite separado | Fluxo final das histórias |

Sequência de dependência é explícita; atualizar base real por PR e renovar evidências após correções/rebase. Justificativa: segurança precisa estar íntegra antes de qualquer integração; leitor/dashboard são demonstráveis sem agente; execução e persistência possuem riscos distintos e exigem provas próprias. O último incremento inclui roteiro consolidado e aceite humano, sem PR adicional só por contagem.

## Preservação do planejamento técnico

research.md preserva decisões, motivos, alternativas e fontes. data-model.md, contracts/ e quickstart.md NÃO foram gerados nesta rodada porque a Phase 1 requer pesquisa completa. Não inventar design final para encobrir B2. Os caminhos de código e frameworks de teste finais dependem das provas; a matriz atual não substitui os contratos ou procedimentos finais.

## Condição de saída da etapa plan

BLOQUEADO. Spec revalidada READY, mas pesquisa ainda não resolve agentes/isolamento, ambiente de prova e procedimentos finais. Registrar checks reais e blockers em verification/plan-stage.json; executar check_gate.py sem forjar sucesso. Não iniciar tasks/implementação dependente. Hooks pré/pós ausentes nesta revisão; reavaliar se configuração mudar.

## Plano de execução do produto

Para cada PR futuro: iniciar o próprio SpecAudit por loopback, carregar fixture sintética, realizar ações reais da matriz, observar resultados/efeitos, comparar a baseline da origem e registros externos e fornecer o mesmo roteiro ao DEV. Para agentes, resposta simulada não comprova integração; para segurança, hash final não substitui bloqueio de escrita durante execução.

Produto final NÃO EXECUTADO: o ensaio posterior executou CLIs e um processo Windows restrito, não a aplicação funcional planejada. Causa: B2 e Phase 1 pendente. Impacto: sem prontidão para tasks e sem aceite funcional. Próximo passo: ensaio técnico sintético e revisão da pesquisa; se inviável, apresentar alternativas ao usuário sem alterar constituição/spec por conta própria.

## Atualização após prova sintética autorizada

Ver [verification/agent-probe-results.md](verification/agent-probe-results.md). Codex e Antigravity detectaram a divergência intencional por leitura, com arquivos sintéticos intactos. Antigravity persistiu marcador em sessões externas antes de Salvar; esse comportamento é permitido pela constitution 2.0.0, desde que exclusivamente externo. Codex efêmero não apresentou marcador nos locais lidos, mas observação é incompleta e não concede compatibilidade integral.

Token Windows restrito/integridade baixa leu fixture e teve escrita negada, sem ACL alterada ou elevação. Prova isolada não comprova agentes nesse token, egress ou descendentes. B2 permanece parcial. Nenhum código do aplicativo ou arquivo de solução real foi modificado.

Research nativo, políticas e artefatos estão em investigação adicional por solicitação do usuário. Worktree branch não é candidato no escopo atual sem cópia isolada. Sem mudança automática de constituição/spec ou liberação de tasks.

### Avaliação das alternativas Antigravity

research nativo foi invocado com sucesso pelo agente principal; chamadas de leitura observadas. --agent research direto foi recusado. Políticas temporárias deny não impediram persistência de conteúdo nas sessões pai/filho, agora permitida somente fora da origem. Candidato de adaptador deve observar conclusão da filha, pois CLI principal pode retornar antes do diagnóstico. Worktree branch não atende fluxo aprovado e não foi criado. B1 foi encerrado por emenda, não por comprovação de modo efêmero; request-review não substitui B2. Ver relatório de prova para evidências e fontes.


## Emenda de persistência autorizada — constituição 2.0.0

Conversas, transcrições e caches internos dos agentes podem existir antes de Salvar exclusivamente fora da solução verificada. Não precisam usar a pasta dos relatórios, mas seus locais efetivos devem ser identificados e informados. Relatórios, sugestões, decisões e histórico oficial do SpecAudit permanecem em memória até Salvar e não são reconstruídos/importados automaticamente de sessões do agente. Não apagar caches para fingir efemeridade.

B1 encerrado por decisão humana nesta conversa, sem reclassificar a evidência histórica como teste de não persistência aprovado. B2 permanece impeditivo: origem, subprocessos, egress, IPC para serviços existentes e instruções não confiáveis precisam de prova conjunta. O ensaio de token isolado não concede aprovação integral. Plan continua BLOCKED, sem liberação de tasks ou implementação dependente.

## Atualização técnica de isolamento e rede — 2026-11-16

Tabela vigente: [verification/isolation-network-status.md](verification/isolation-network-status.md). Asserções positivas: negar create/write/rename/delete em processo/filho, origem intacta, cancelamento dos CLIs durante startup com zero processos ativos, broker por handle somente leitura e inicialização --version em AppContainer real. Essas provas são parciais e não constituem aprovação do produto.

Resultado negativo: token baixo e AppContainer candidato permitiram o canário TCP; não são uma política de egress comprovada. Codex falhou ao inicializar estado sob token baixo; Antigravity concluiu diagnóstico com erros de logs/caches. Falta estado externo gravável seguro e análise autenticada conjunta no isolamento final.

WFP: abrir sessão dinâmica passou; criar subcamada dinâmica vazia falhou com acesso negado (5), sem adicionar filtros. Nenhum firewall global, ACL da origem, UAC ou serviço foi alterado. Sandboxes nativos recusaram o comando TCP antes de executar, com zero conexões observadas; classificar NÃO EXECUTADO, nunca bloqueio de tráfego aprovado. Não tentar contornar a recusa por bypass ou alteração de comando.

Decisão técnica aprovada pelo usuário: adotar mediador privilegiado mínimo para filtros dinâmicos exclusivos da identidade de execução, mantendo executor sem elevação; sem instalação ou elevação autorizada nesta etapa. Proxy allowlist sem negação de conexões diretas pelo OS não atende. Proposta, limites e matriz de testes adicionais estão no relatório, sem instalação autorizada. B2 permanece BLOCKED; não iniciar tasks nem design dependente como se a rede estivesse resolvida.

## Arquitetura aprovada: mediador mínimo de rede — 2026-11-16

**Decisão humana**: usuário aprovou um mediador privilegiado mínimo para filtros temporários por execução, mantendo executor sem privilégios e solução somente leitura. A aprovação é de arquitetura; NÃO autoriza instalação, elevação, criação de serviço ou alteração de firewall nesta etapa. Não constitui teste de rede aprovado.

### Responsabilidades e fronteiras

| Componente | Permissão e responsabilidade | Operações proibidas |
|---|---|---|
| Host/UI local | Solicitar análise e mostrar estados; acionar Salvar | Elevar executor; aceitar regras arbitrárias de agente/documento |
| Broker de leitura | Abrir fontes autorizadas com acesso somente leitura e fornecer handles/conteúdo | Gravar origem, executar projeto ou mudar ACLs |
| Executor e agentes | Identidade restrita por execução, Job Object sem breakaway; estado interno apenas externo | Elevação, acesso direto a serviços não autorizados ou escrita na origem |
| Mediador privilegiado mínimo | Autorizar identidade de execução e instalar/remover exclusivamente filtros WFP temporários dessa identidade | Ler solução/credenciais, executar comandos arbitrários, alterar perfis globais, regras persistentes ou filtros de terceiros |
| Proxy de saída | Encaminhar apenas destinos/portas aprovados para o perfil do agente e fontes de avisos | Proxy aberto, wildcards amplos, destino arbitrário por prompt ou fallback direto |
| Writer de relatórios | Gravar no destino externo validado apenas após Salvar | Importar sessões como relatório salvo ou aplicar correções na origem |

O mediador não é uma elevação do executor. Sua identidade/privilégios de serviço mínimos e os direitos WFP necessários devem ser determinados e testados; NÃO presumir que LocalSystem irrestrito seja necessário. Instalação/configuração inicial será uma etapa separada, com autorização específica. Não mudar a constituição: a proibição de elevar o executor e a preservação da origem continuam integrais.

### Interface de controle delimitada

Canal IPC local autenticado, com ACL para o host autorizado e validação da identidade do cliente. Não expor listener remoto. Operações conceituais: abrir política de execução, consultar estado e encerrar política. Abrir recebe identificador emitido pelo host, perfil de agente previamente aprovado e identidade de execução vinculada ao processo suspenso/job; não recebe scripts, caminhos de solução, URLs ou regras WFP livres. Consultas de vulnerabilidades usam perfil separado, sem ampliar o perfil do agente.

O mediador deve verificar propriedade, token, identidade dedicada, instante de criação e vínculo ao job antes de retomar execução. Não confiar somente no PID (risco de reutilização) ou em caminho de executável compartilhado com instâncias do usuário. Descendentes devem permanecer sob a mesma política. Recusar invocações de processos, sessões ou perfis não registrados. O formato final do contrato e os códigos de erro pertencem ao design após a prova técnica.

### Ciclo de vida fail-closed

1. Criar identidade dedicada e processo suspenso sem privilégios; manter Job Object e broker de leitura.
2. Validar destinos externos do estado interno e preparar proxy autenticado por execução, sem registrar segredos.
3. Mediador valida a solicitação e cria sessão WFP dinâmica; instala filtros antes de qualquer execução do agente.
4. Negar conexões diretas IPv4/IPv6 por padrão; permitir somente canal do proxy e canais internos do agente demonstrados como necessários. Não liberar todo loopback.
5. Confirmar política efetiva; somente então retomar processo. Se não puder impor filtro, não iniciar análise nem fazer fallback sem isolamento.
6. No término, cancelamento, falha do host ou expiração, encerrar job/descendentes antes de retirar a proteção. Se o mediador/proxy cair, executar encerramento fail-closed por supervisor/lease independente; uma sessão dinâmica que desapareça não pode deixar agentes vivos com rede livre.
7. Remover somente objetos criados para a execução; verificar ausência de processos ativos e de filtros residuais, preservando regras de terceiros e firewall global.

A ordem acima é um requisito de projeto, não uma propriedade já demonstrada do código de prova.

### Política de destinos

Perfis versionados por agente distinguem inferência, autenticação/renovação necessária e controle interno. Consultas de avisos têm perfil próprio. Domínios/portas exatos serão obtidos da documentação e de tráfego de teste autorizado; não inventar lista completa ou liberar *.googleapis.com/*.openai.com como atalho. Telemetria/atualizador não recebe acesso por ser parte do binário: qualquer exceção exige finalidade aprovada.

O proxy valida destino efetivo, DNS, redirecionamentos, endereços privados e uso de IP literal. A autenticação do agente mantém seus segredos fora de logs. Não interceptar TLS nem instalar certificados nesta etapa. Uma allowlist por domínio de um CONNECT proxy controla destino/porta, não garante quais caminhos/operações HTTPS serão utilizados; não prometer inspeção que não existe.

Serviços já existentes fora do job não são protegidos automaticamente por filtros dos descendentes. A prova deve impedir delegação indevida via IPC ou estabelecer quais canais internos pertencem ao próprio agente, sem conceder acesso aos serviços da solução. Estado externo gravável/autenticação devem ser integrados sem alterar ACLs da origem ou copiar credenciais por conveniência.

### Provas exigidas antes de liberar B2

| Prova | Resultado necessário | Estado atual |
|---|---|---|
| Inferência dos dois agentes e consulta de aviso via perfil aprovado | Resposta real com destinos autorizados e origem intacta | NÃO EXECUTADO |
| Canário local e serviço privado não autorizado | Negação confirmada no socket e no listener, não mera recusa do agente | NÃO EXECUTADO com novo mediador |
| Saída direta, IPv6, UDP/QUIC, DNS e redirect fora da lista | Nenhum bypass do proxy/política | NÃO EXECUTADO |
| Descendentes e outra instância do mesmo agente | Filhos restritos; instância do usuário não afetada | NÃO EXECUTADO com filtros |
| Falha do mediador/proxy/host e cancelamento | Nenhuma janela de rede livre; job encerrado e filtros próprios removidos | NÃO EXECUTADO |
| IPC malformado, PID reutilizado e perfil não autorizado | Pedido recusado sem alterar política de outro processo | NÃO EXECUTADO |
| Estado externo e relatórios | Caches externos permitidos; relatório oficial somente por Salvar | NÃO EXECUTADO em integração final |

### Sequência de entrega ajustada

Inserir antes do PR-A um incremento técnico de prova do mediador/proxy e rollback. Seu código/ensaios devem usar apenas fixtures, validar falhas e documentar instalação/remoção; não disponibilizar análise real enquanto B2 não passar. O incremento exige aprovação humana de PR e autorização operacional separada para qualquer instalação ou elevação. Demais PRs mantêm dependência explícita dessa prova. Esta atualização não gera tasks nem implementa o serviço.

A decisão arquitetural está resolvida. O bloqueio remanescente é técnico: capacidade WFP, política efetiva, integração dos agentes e comportamento fail-closed ainda precisam ser demonstrados. Plan continua BLOCKED até pesquisa/design e gates completos; não converter aprovação da arquitetura em aprovação de execução.
## Protótipo preparado, sem execução privilegiada — 2026-11-16

Projeto revisável separado: prototype/NetworkMediator. Código de sessão WFP dinâmica com SID obrigatório, transação/rollback, permit TCP local restrito e deny IPv4/IPv6; worker suspenso e token restrito sem grupo Administradores habilitado; Job Object e broker por handle somente leitura. CLI fechado, sem parâmetros de alvo arbitrário ou instalação. Não é serviço/IPC de produção nem proxy de domínios.

Build Release passou sem avisos/erros; 15 autotestes sem privilégios passaram. Não houve instalação, elevação ou filtros aplicados. Manifesto e hashes em verification/mediator-prototype-manifest.json; escopo/comando que ainda precisam de autorização em verification/mediator-authorization-request.md. Modo privilegiado exige console elevada já autorizada e confirmação exata; wrapper verifica os binários. Auditoria de pacotes sem achados reportados, sem PackageReference, não cobre runtime/SDK.

Provas preparadas: rollback antes de worker, leitura/negação de quatro mutações em raiz/filho, TCP permitido, TCP IPv4/IPv6 e UDP negados, processo-controle fora do SID, token restrito, origem intacta e limpeza dos filtros/perfis. Nada disso foi declarado aprovado por compilação/autotestes. Crash abrupto/fail-closed de produção, autenticação/proxy/domínios e agentes continuam pendentes. Plan permanece BLOCKED até comprovação.


## Única prova elevada realizada — 2026-11-16

Manifesto d0fd1c46893c69d51dd2aba717834b8aed30a09423ff4b53b9bb5fc458eec115 conferido; uma execução UAC autorizada, sem retry. Ver verification/mediator-elevated-summary.md e tabela vigente em verification/isolation-network-status.md.

WFP dinâmico passou a instalação por SID e rollback de transação. O controle fora da identidade permaneceu conectado. Worker foi criado em AppContainer com grupo Administradores desabilitado, mas terminou com STATUS_DLL_INIT_FAILED (0xC0000142), sem saída de asserções. Portanto TCP/UDP/IPv6 e operações de arquivo dessa fase são NÃO EXECUTADOS, não bloqueio de rede aprovado. Não foi determinada a DLL ou a causa específica; eventos Application consultados não localizaram ocorrência no intervalo.

Fonte sintética intacta; job sem processos ativos e perfis temporários removidos. Consulta independente de leitura confirmou os três filtros e subcamada ausentes, manifesto inalterado e PID observado encerrado. Nenhum serviço, perfil global de firewall, agente real, credencial ou solução real foi acessado/alterado.

Plan permanece BLOCKED pelo worker e provas funcionais/integradas pendentes. A autorização única está consumida. Uma correção requer nova revisão/hash e outra autorização antes de nova prova elevada. Nesta etapa não foram alterados código do aplicativo, código/binários do protótipo, constituição ou especificação.

## Revisão 2 do worker — preparação sem elevação, 2026-11-16

A autorização anterior permanece consumida. Correções e mitigação documentadas em verification/worker-0xc0000142-investigation.md: escape dos argumentos Windows corrigido, PowerShell substituído apenas no worker sintético por Win32 mínimo sem CLR/USER32 diretos. Aplicativo e mediador continuam C#. Filho inicialmente recebeu ERROR_ACCESS_DENIED ao carregar o binário; staging só do executável em TEMP, hash conferido e ReadAndExecute por SID exclusivo resolveram esse bootstrap. Não se copia solução nem se modifica sua ACL. Nenhuma instalação/elevação/WFP aplicada nesta revisão.

Build sem avisos/erros, 23 autotestes passaram. Worker e filho iniciaram sob AppContainer sem Administradores habilitado, exit=0, job zerado e perfil removido. Não abriram rede no diagnóstico. A causa exata do 0xC0000142 anterior permanece inconclusiva; sucesso sem elevação não comprova correção sob WFP elevado.

A revisão 1 e seu manifesto foram arquivados. Novo manifesto vigente em verification/mediator-prototype-manifest.json e autorização concreta em verification/mediator-authorization-request-v2.md, vinculados a quatro binários, inclusive SyntheticWorker.exe. Nenhuma segunda prova elevada executada. Próximo passo depende de autorização explícita de uma execução, sem retry automático; depois verificar tráfego permitido/negado, leitura por broker, recusa de direitos de mutação, controle externo e limpeza. RenameDenied nesta versão mede recusa de DELETE necessário à renomeação, não uma chamada real de rename. Não generalizar esse teste para todas as operações de arquivos.

Plan permanece BLOCKED: B2-prova-elevada-rev2, rede/integridade da fixture na nova revisão, agentes/autenticação/proxy/IPC final e hard-crash fail-closed. Fases finais de design/contratos/tasks seguem bloqueadas. Consulta NuGet sem achados e sem PackageReference; não cobre Windows/CRT/SDK/runtime ou auditoria completa nativa. Código do aplicativo, constituição e especificação não foram alterados.


## Resultado da única execução elevada da revisão 2 — 2026-11-16

Manifesto 3f0839d1f20f77ae77f872e0337753fd7adcf55622b7659f417300520f7b4cc7 conferido; uma execução elevada explicitamente autorizada, sem retry, exit=1. Código/binários/manifesto não recompilados nem alterados. Sem instalação, perfis globais, solução real, agentes ou credenciais. Relatório completo: verification/mediator-v2-elevated-summary.md.

Rollback WFP antes do worker, política por SID exclusivo e controle externo passaram. Worker criado em AppContainer sem Administradores habilitado, mas voltou a falhar com 0xC0000142 antes de WorkerReady (bootstrapReached=false, saída vazia). Filho, leitura/negações e TCP/UDP/IPv6 NÃO EXECUTADOS; contagens zero não demonstram restrição efetiva de tráfego. Remover PowerShell/CLR do worker não resolveu o problema nesse contexto; não há identificação da DLL causadora.

Fonte sintética intacta, job zerado, perfil removido. Consulta independente sem elevação confirmou três filtros e subcamada ausentes, PID encerrado, manifesto inalterado e 20 arquivos monitorados do aplicativo/solução/constituição/spec com hashes preservados. Evidências JSON específicas da revisão 2 mantêm intactos os resultados anteriores.

Autorização v2 consumida. Não repetir nem instalar/elevar para diagnosticar sem autorização distinta. Próximo incremento: investigar contexto de inicialização sem relaxar isolamento e preparar nova revisão somente se necessária. Plan permanece BLOCKED por B2-worker e provas funcionais de rede/fixture, integração real dos agentes/autenticação/proxy/IPC e hard-crash fail-closed. Sucesso de bootstrap sem elevação não fecha esses bloqueios. Nenhum aceite humano/Phase 1 final/tasks liberado.

