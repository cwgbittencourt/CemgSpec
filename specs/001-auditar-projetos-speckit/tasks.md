# Tasks: Auditoria documental de projetos SpecKit — V1

**Data**: 2026-11-16 | **Branch real**: main | **Base**: 79d9c3d.
**Preset**: cemg-speckit 3.0.0 | **Template**: tasks-template | **Composição**: replace.
**Fonte**: .specify/presets/cemg-speckit/templates/tasks-template.md; resolvido por setup-tasks.ps1.
**Entradas**: spec.md, plan.md, research.md (R18 vigente), data-model.md, contracts/application.md, contracts/verification.md, contracts/persistence.md e quickstart.md; constituição 4.0.0.
**Estado de execução**: migração e T001–T020 (PR-A) concluídas tecnicamente. T021–T052 (PR-B: US2 Documentos, US3 Dashboard, US4 Ficha documental, US8 Navegação/Temas) concluídas tecnicamente em `ac53fd2` (**95/95 testes aprovados**, zero impeditivos técnicos abertos); descrição em [verification/prs/PR-B.md](verification/prs/PR-B.md). Próxima T053 (Fase 8 — US5 Verificações permitidas / PR-C: combo fechada e `check_gate` offline). Aprovação/integração de PRs e execução/aceite humanos pendentes; nenhuma User Story aceita.

## Checkpoint histórico — constituição 4.0.0

A garantia de sandbox OS foi retirada por decisão explícita; SEC-001 permanece histórica. Somente leitura da aplicação, dois comandos, testes reais/roteiros por história e aceite do DEV permanecem obrigatórios. T015 resolveu os dois achados; T016 conferiu o roteiro reproduzível na mesma fonte f2b29a2, sem mudar código/testes. Próximo passo T017, verificação do incremento PR-A; aprovação de PR, execução e aceite humano pendentes. Evidências anteriores permanecem históricas.

## Referência visual obrigatória das interfaces

Consultar [guia dos mockups aprovados](design/mockups/README.md) antes de implementar/validar UI. Três imagens originais versionadas: [dashboard/ficha](design/mockups/dashboard-requisito.png), [documentos/sugestões](design/mockups/documentos-sugestoes.png) e [verificações/pasta externa](design/mockups/verificacoes-pasta-externa.png). Preservar visual/estrutura aprovados; spec/contratos e guia V1 substituem comandos, análise de código, testes, cópia isolada e salvamento automático históricos nas imagens. O vínculo não adiciona tarefa, muda dependência ou conclui checkbox. Provas de proteção continuam antes de cada habilitação.

## Regras de geração e execução

- Origem auditada integralmente somente leitura: não criar/alterar/renomear/excluir, nem temporariamente com restore, nem mudar ACL do usuário ou copiar a solução. Só documentos/registros SpecKit; nenhuma análise de código/manifests/configurações da origem.
- Combo fechada apenas speckit-analyze/check_gate; sem shell, hooks/scripts/plugins/configuração da origem ou execução automática. Provas usam dados sintéticos do desenvolvimento do SpecAudit, não projeto real.
- ReaderBoundary, GateAdapter, AnalyzeCodex, AnalyzeAntigravity e ExternalWriter só habilitam após prova real Passed da versão/política/ambiente. Falha ou NÃO EXECUTADO bloqueia tarefa dependente; não bloqueia escrever tarefas nem autoriza contornar controle.
- Sem instalação/elevação automática. Se implementação revelar necessidade de direito/escopo adicional, parar ação dependente e solicitar autorização concreta; autorizações antigas consumidas não são reutilizáveis.
- Conteúdo/relatórios oficiais em memória até Salvar externo. Conversas/caches dos agentes exclusivamente externos são informados e separados do histórico oficial.
- Cada checkbox abaixo é trabalho técnico do agente, nunca aceite/aprovação humana. Todas começam vazias. Teste escrito, compilação, mock ou presença de arquivo não comprova comportamento real.
- Usar contratos por task abaixo e templates CEMG de validação/review/DEV/PR. Correções e mudança de base exigem renovar evidências afetadas. Não iniciar integração insegura porque etapas anteriores foram apenas descritas.
- Paths SpecAudit/ e tests/ são relativos ao repositório; evidence/, validation/, acceptance/, verification/ e quickstart.md são relativos a esta feature. Paths propostos de implementação não são arquivos criados por este comando. Evidência técnica pode persistir na feature; não confundir com relatório oficial do produto antes de Salvar.
- US7/FR-017 adiados e sem tasks. Ordem prioriza dependências: US8 P2 antecede US5 P1 para concluir o shell/PR-B requerido pelo PR-C; prioridade funcional original não foi alterada.
- Preparação de testes anterior à implementação deve observar falha por regra ausente; falha de build/ambiente quebrado não comprova cenário. Reutilizar teste adequado quando existir; não escrever teste trivial só por checkbox.


## Fase 1 — Preparação

**Objetivo**: Preparar desenvolvimento/testes próprios do SpecAudit; nenhuma análise real habilitada.
**Dependências**: Nenhuma; ler entradas e gates READY.
**Teste independente**: Inventário e smoke do scaffold local, sem carregar solução real.

- [x] T001 Conferir SDK .NET10/C# estáveis, Windows e ferramentas existentes; fixar framework estável de testes e auditar dependências diretas/transitivas antes de introduzi-las em tests/SpecAudit.Tests/SpecAudit.Tests.csproj, tests/SpecAudit.Windows.Tests/SpecAudit.Windows.Tests.csproj e tests/SpecAudit.Ui.Tests/SpecAudit.Ui.Tests.csproj; registrar evidence/setup-validation.md.
- [x] T002 Preparar gerador e instrumentação de fixtures em tests/SpecAudit.Windows.Tests/Fixtures/DocumentaryFixture.cs e tests/SpecAudit.Ui.Tests/Fixtures/LocalHostFixture.cs a partir da receita de quickstart.md; sem inserir dados de usuário.
- [x] T003 Configurar sessão local, loopback, validação Host/Origin e antiforgery em SpecAudit/Program.cs e SpecAudit/Application/LocalSessionService.cs; controles de verificações/Save iniciam indisponíveis.

## Fase 2 — Fundação compartilhada

**Objetivo**: Definir capacidades, identidade e disponibilidade antes de integrar origem.
**Dependências**: T001–T003; sem expor funcionalidades não provadas.
**Teste independente**: Testes de contrato de estados/IDs e launcher sintético, sem agente real.

- [x] T004 Escrever testes de identidade/estado/contratos em tests/SpecAudit.Tests/SessionContractTests.cs e tests/SpecAudit.Windows.Tests/ExecutionContractTests.cs antes dos modelos/launcher.
- [x] T005 Definir ProjectSession/Feature, catálogo/snapshot, ProtectionAssessment e estados compartilhados em SpecAudit/Domain/ProjectSession.cs, SpecAudit/Domain/DocumentSnapshot.cs e SpecAudit/Domain/ProtectionAssessment.cs, observando as citações do modelo abaixo.
- [x] T006 Implementar executor gerenciado fechado, sem shell livre, com executável conferido, ArgumentList, cwd externo, stdin/saída limitada e cancelamento de árvore própria em SpecAudit/Infrastructure/Execution/TrustedCommandRunner.cs; sem sandbox OS.
- [x] T007 Implementar registro de capacidades e limites do contrato de aplicação v4 em SpecAudit/Application/CapabilityRegistry.cs e SpecAudit/Infrastructure/Execution/ExecutionLimits.cs; documentar evidence/capability-contract.md.

## Fase 3 — US1: Carregar e preservar (P1)

**Objetivo**: Selecionar origem/destino com catálogo protegido e invalidação, sem escrita.
**Dependências**: Fundação; ReaderBoundary deve passar antes de habilitar carregamento para executor.
**Teste independente**: US1-S01–S04 em fixture Windows; usuário continua podendo editar.

- [x] T008 Escrever testes US1-S01–S04 do broker/seleção/destinos e somente leitura da aplicação em tests/SpecAudit.Windows.Tests/ReaderBoundaryTests.cs e tests/SpecAudit.Ui.Tests/ProjectSelectionTests.cs; sem provas de contenção OS.
- [x] T009 [US1] Implementar catálogo e broker por IDs/handles somente leitura em SpecAudit/Infrastructure/Documents/DocumentCatalog.cs e SpecAudit/Infrastructure/Documents/DocumentBroker.cs; revalidar contenção no ponto real de leitura.
- [x] T010 Executar prova funcional ReaderBoundary v4 da aplicação/broker em evidence/ReaderBoundary-v4-proof.md e registrar selo da política atual somente se passar; falhas históricas de sandbox permanecem arquivadas.
- [x] T011 [US1] Implementar picker nativo e seleção/carregamento/alteração de destino em ProjectService.cs e Projects.razor após validar ReaderBoundary v4 da aplicação; preservar sessão, origem readonly e histórico externo.
- [x] T012 [US1] Implementar geração/invalidação documental em SpecAudit/Infrastructure/Documents/DocumentChangeMonitor.cs e integração de ProjectService.cs, preservando edição externa.
- [x] T013 [US1] Executar os cenários US1-S01–S04 / US1-AC01–AC04 no host/harness real e regressão relacionada, comparando resultado e efeitos; registrar versão, comandos e saídas em evidence/US1-validation.md.
- [x] T014 [US1] Revisar diff, critérios, contratos, segurança e resultados da versão de US1 em evidence/US1-review.md, declarando autorrevisão.
- [x] T015 [US1] Resolver achados de US1 e repetir checks afetados em evidence/US1-validation.md e evidence/US1-review.md; se nenhum, registrar referência à revisão real.
- [x] T016 [US1] Consolidar e conferir roteiro reproduzível em validation/US1-dev.md e registrar estado técnico em acceptance/US1.md, mantendo decisão do DEV pendente.
- [x] T017 [US1] Executar a verificação do incremento PR-A e regressão na base declarada, incluindo comportamento real e estado intermediário seguro, em evidence/PR-A-validation.md.
- [x] T018 [US1] Revisar o incremento completo PR-A em evidence/PR-A-review.md com diff incremental, limites e achados por arquivo/linha.
- [x] T019 [US1] Resolver e revalidar achados do PR-A em evidence/PR-A-validation.md e evidence/PR-A-review.md; registrar nenhum achado somente com review real.
- [x] T020 [US1] Preparar verification/prs/PR-A.md pelo pr-template.md CEMG com escopo final, tasks, base/predecessor, evidências e roteiro; não abrir/integrar PR ou marcar aprovação humana.

## Fase 4 — US2: Documentos (P1)

**Objetivo**: Preview/fonte/lado a lado e Mermaid seguros e próximos da referência padrão VS Code.
**Dependências**: PR-A técnico validado; proteção ReaderBoundary válida.
**Teste independente**: US2-S01–S04 com corpus offline/remoto controlado; scripts nunca executados.

- [x] T021 [P] [US2] Escrever testes de contrato/preview US2-S01–S04 em tests/SpecAudit.Ui.Tests/MarkdownPreviewTests.cs e tests/SpecAudit.Tests/DocumentNavigationTests.cs. Incluir comparação visual com design/mockups/README.md e os mockups da tela, exigindo as substituições de comportamento da V1.
- [x] T022 [US2] Selecionar/fixar parser Markdown, sanitizador e Mermaid locais auditados e implementar renderização em SpecAudit/Infrastructure/Documents/MarkdownRenderer.cs e SpecAudit/wwwroot/lib/preview/; nenhum CDN.
- [x] T023 [US2] Validar segurança e navegação do preview antes de conectá-lo à UI em evidence/preview-safety.md, usando assets via broker.
- [x] T024 [US2] Integrar árvore/busca/âncoras e Preview/Fonte/Lado a lado em SpecAudit/Components/Pages/Documents.razor e SpecAudit/Components/Documents/MarkdownPreview.razor após validação de segurança. Preservar o visual de design/mockups/documentos-sugestoes.png e aplicar as adaptações obrigatórias de design/mockups/README.md; os comportamentos são exclusivamente os da V1 atual.
- [x] T025 [US2] Executar os cenários US2-S01–S04 / US2-AC01–AC04 no host/harness real e regressão relacionada, comparando resultado e efeitos; registrar versão, comandos e saídas em evidence/US2-validation.md.
- [x] T026 [US2] Revisar diff, critérios, contratos, segurança e resultados da versão de US2 em evidence/US2-review.md, declarando autorrevisão.
- [x] T027 [US2] Resolver achados de US2 e repetir checks afetados em evidence/US2-validation.md e evidence/US2-review.md; se nenhum, registrar referência à revisão real.
- [x] T028 [US2] Consolidar e conferir roteiro reproduzível em validation/US2-dev.md e registrar estado técnico em acceptance/US2.md, mantendo decisão do DEV pendente.

## Fase 5 — US3: Dashboard documental (P1)

**Objetivo**: Contagens/filtros/procedência honestos e atualização documental sem executar verificações.
**Dependências**: PR-A; pode desenvolver domínio em paralelo ao renderer quando arquivos/deps independentes.
**Teste independente**: US3-S01–S04: 75% declarado, 60% cobertura documental e Sem itens.

- [x] T029 [P] [US3] Escrever testes US3-S01–S04 em tests/SpecAudit.Tests/DocumentaryProgressTests.cs e tests/SpecAudit.Ui.Tests/DashboardTests.cs. Incluir comparação visual com design/mockups/README.md e os mockups da tela, exigindo as substituições de comportamento da V1.
- [x] T030 [US3] Definir TaskDeclaration/DashboardProjection e projeções em SpecAudit/Domain/TaskDeclaration.cs, SpecAudit/Domain/DashboardProjection.cs e SpecAudit/Application/DocumentaryDashboardService.cs; observar constraints literais abaixo.
- [x] T031 [US3] Implementar indicadores/ocorrências/filtros e fonte/atualidade em SpecAudit/Components/Pages/Dashboard.razor, preservando procedência documental. Preservar o visual de design/mockups/dashboard-requisito.png e aplicar as adaptações obrigatórias de design/mockups/README.md; os comportamentos são exclusivamente os da V1 atual.
- [x] T032 [US3] Executar os cenários US3-S01–S04 / US3-AC01–AC04 no host/harness real e regressão relacionada, comparando resultado e efeitos; registrar versão, comandos e saídas em evidence/US3-validation.md.
- [x] T033 [US3] Revisar diff, critérios, contratos, segurança e resultados da versão de US3 em evidence/US3-review.md, declarando autorrevisão.
- [x] T034 [US3] Resolver achados de US3 e repetir checks afetados em evidence/US3-validation.md e evidence/US3-review.md; se nenhum, registrar referência à revisão real.
- [x] T035 [US3] Consolidar e conferir roteiro reproduzível em validation/US3-dev.md e registrar estado técnico em acceptance/US3.md, mantendo decisão do DEV pendente.

## Fase 6 — US4: Ficha documental (P1)

**Objetivo**: Reunir critérios/vínculos/fontes e limites sem avaliar implementação.
**Dependências**: US3 de domínio/UI pronto; leitor US2 pronto para abrir fontes.
**Teste independente**: US4-S01–S04: explícito/inferido, ausente/ambíguo, declarações externas e .cs não aberto.

- [x] T036 [US4] Escrever testes US4-S01–S04 em tests/SpecAudit.Tests/RequirementTraceTests.cs e tests/SpecAudit.Ui.Tests/RequirementSheetTests.cs. Incluir comparação visual com design/mockups/README.md e os mockups da tela, exigindo as substituições de comportamento da V1.
- [x] T037 [US4] Definir Requirement/Criterion/EvidenceLink/Finding e serviço de rastreabilidade em SpecAudit/Domain/Requirement.cs, SpecAudit/Domain/EvidenceLink.cs, SpecAudit/Domain/Finding.cs e SpecAudit/Application/RequirementTraceService.cs com as constraints literais abaixo.
- [x] T038 [US4] Implementar ficha em SpecAudit/Components/Pages/Requirements.razor e SpecAudit/Components/Requirements/RequirementSheet.razor com descrição/critério/plano/tarefas/verificações/sugestões documentais. Preservar o visual de design/mockups/dashboard-requisito.png e aplicar as adaptações obrigatórias de design/mockups/README.md; os comportamentos são exclusivamente os da V1 atual.
- [x] T039 [US4] Executar os cenários US4-S01–S04 / US4-AC01–AC04 no host/harness real e regressão relacionada, comparando resultado e efeitos; registrar versão, comandos e saídas em evidence/US4-validation.md.
- [x] T040 [US4] Revisar diff, critérios, contratos, segurança e resultados da versão de US4 em evidence/US4-review.md, declarando autorrevisão.
- [x] T041 [US4] Resolver achados de US4 e repetir checks afetados em evidence/US4-validation.md e evidence/US4-review.md; se nenhum, registrar referência à revisão real.
- [x] T042 [US4] Consolidar e conferir roteiro reproduzível em validation/US4-dev.md e registrar estado técnico em acceptance/US4.md, mantendo decisão do DEV pendente.

## Fase 7 — US8: Navegação e temas (P2; dependência de PR-B)

**Objetivo**: Concluir shell das cinco áreas e temas antes da integração de verificações PR-C.
**Dependências**: US2/3/4 no PR-B. Prioridade de negócio P2 preservada; antecipação é dependência de layout/PR, não mudança de prioridade.
**Teste independente**: US8-S01–S02: mesmas cinco áreas/seleções e estados em escuro/claro.

- [x] T043 [US8] Escrever testes US8-S01/S02 em tests/SpecAudit.Ui.Tests/NavigationThemeTests.cs com estados indisponíveis de verificações e Salvar. Incluir comparação visual com design/mockups/README.md e os mockups da tela, exigindo as substituições de comportamento da V1.
- [x] T044 [US8] Implementar navegação/tema em SpecAudit/Components/Layout/MainLayout.razor e SpecAudit/wwwroot/app.css; azul/lilás/branco/cinza, escuro inicial e claro opcional, preferência só na sessão. Criar apenas placeholders indisponíveis das áreas Verificações/Sugestões nesses arquivos para que a navegação não dependa de PR-C/E futuros. Preservar o visual de design/mockups/dashboard-requisito.png e aplicar as adaptações obrigatórias de design/mockups/README.md; os comportamentos são exclusivamente os da V1 atual.
- [x] T045 [US8] Executar os cenários US8-S01/S02 / US8-AC01/02 no host/harness real e regressão relacionada, comparando resultado e efeitos; registrar versão, comandos e saídas em evidence/US8-validation.md.
- [x] T046 [US8] Revisar diff, critérios, contratos, segurança e resultados da versão de US8 em evidence/US8-review.md, declarando autorrevisão.
- [x] T047 [US8] Resolver achados de US8 e repetir checks afetados em evidence/US8-validation.md e evidence/US8-review.md; se nenhum, registrar referência à revisão real.
- [x] T048 [US8] Consolidar e conferir roteiro reproduzível em validation/US8-dev.md e registrar estado técnico em acceptance/US8.md, mantendo decisão do DEV pendente.
- [x] T049 [US8] Executar a verificação do incremento PR-B e regressão na base declarada, incluindo comportamento real e estado intermediário seguro, em evidence/PR-B-validation.md.
- [x] T050 [US8] Revisar o incremento completo PR-B em evidence/PR-B-review.md com diff incremental, limites e achados por arquivo/linha.
- [x] T051 [US8] Resolver e revalidar achados do PR-B em evidence/PR-B-validation.md e evidence/PR-B-review.md; registrar nenhum achado somente com review real.
- [x] T052 [US8] Preparar verification/prs/PR-B.md pelo pr-template.md CEMG com escopo final, tasks, base/predecessor, evidências e roteiro; não abrir/integrar PR ou marcar aprovação humana.

## Fase 8 — US5: Verificações permitidas (P1)

**Objetivo**: Combo fechada e gate offline, depois dois adaptadores documentais com provas individuais.
**Dependências**: PR-A e PR-B técnicos prontos; as tarefas PR-C e PR-D são incrementos distintos da mesma história.
**Teste independente**: US5-S01–S07; cada integração exige seu selo antes de Executar ficar habilitado.

- [x] T053 [US5] Escrever testes de contrato gate US5-S06/S07 e disponibilidade/seleção US5-S03/S05 em tests/SpecAudit.Windows.Tests/GateAdapterTests.cs e tests/SpecAudit.Ui.Tests/VerificationSelectionTests.cs. Incluir comparação visual com design/mockups/README.md e os mockups da tela, exigindo as substituições de comportamento da V1.
- [x] T054 [US5] Definir GateCandidate/VerificationRun e máquina de estados em SpecAudit/Domain/GateCandidate.cs, SpecAudit/Domain/VerificationRun.cs e SpecAudit/Application/VerificationService.cs; conservar enum de só duas opções.
- [ ] T055 [US5] Empacotar core check_gate.py e gate-requirements.json CEMG 3.0.0 com hashes e runtime Python absoluto conferido em SpecAudit/Infrastructure/Gates/Core/ e SpecAudit/Infrastructure/Gates/GatePackage.cs; não importar script da origem.
- [ ] T056 [US5] Implementar runner confiável Path de capacidade e cliente broker em SpecAudit/Infrastructure/Gates/runner.py, SpecAudit/Infrastructure/Gates/catalog_path.py e SpecAudit/Infrastructure/Gates/GateExecutor.cs, apenas validate/validate_impossibility.
- [ ] T057 [US5] Executar prova GateAdapter em evidence/GateAdapter-proof.md antes de habilitar gate, comparando core oficial em fixtures versus runner de capacidade; manter apenas dados sintéticos.
- [ ] T058 [US5] Integrar combo compartilhada e habilitar check_gate somente com selo Passed em SpecAudit/Components/Verifications/VerificationSelector.razor e SpecAudit/Components/Pages/Verifications.razor, reutilizando-o na ficha. Preservar o visual de design/mockups/verificacoes-pasta-externa.png e aplicar as adaptações obrigatórias de design/mockups/README.md; os comportamentos são exclusivamente os da V1 atual.
- [ ] T059 [US5] Executar a verificação do incremento PR-C e regressão na base declarada, incluindo comportamento real e estado intermediário seguro, em evidence/PR-C-validation.md.
- [ ] T060 [US5] Revisar o incremento completo PR-C em evidence/PR-C-review.md com diff incremental, limites e achados por arquivo/linha.
- [ ] T061 [US5] Resolver e revalidar achados do PR-C em evidence/PR-C-validation.md e evidence/PR-C-review.md; registrar nenhum achado somente com review real.
- [ ] T062 [US5] Preparar verification/prs/PR-C.md pelo pr-template.md CEMG com escopo final, tasks, base/predecessor, evidências e roteiro; não abrir/integrar PR ou marcar aprovação humana.
- [ ] T063 [US5] Escrever contratos de descoberta/execução documental US5-S01/S02/S04/S05 em tests/SpecAudit.Windows.Tests/AgentDiscoveryTests.cs e tests/SpecAudit.Windows.Tests/AnalyzeProtectionTests.cs antes dos adaptadores.
- [ ] T064 [US5] Definir AgentInstallation e descoberta em SpecAudit/Domain/AgentInstallation.cs e SpecAudit/Infrastructure/Agents/AgentDiscovery.cs para Codex/Antigravity/Claude Code/Kiro; fallback manual só para desejado não encontrado.
- [ ] T065 [US5] Implementar adaptador Codex documental em SpecAudit/Infrastructure/Agents/CodexAnalyzeAdapter.cs, ainda desabilitado, usando snapshots/canal restrito e estado externo conhecido.
- [ ] T066 [US5] Executar prova real individual AnalyzeCodex em evidence/AnalyzeCodex-proof.md antes de habilitar, incluindo análise sintética correta, contrato documental, ferramentas configuradas, finalidades de rede/segredos, estado externo e descendentes.
- [ ] T067 [US5] Habilitar seleção/Execução Codex somente com Assessment Passed em SpecAudit/Application/AgentCompatibilityService.cs e SpecAudit/Components/Verifications/AgentSelector.razor. Preservar o visual de design/mockups/verificacoes-pasta-externa.png e aplicar as adaptações obrigatórias de design/mockups/README.md; os comportamentos são exclusivamente os da V1 atual.
- [ ] T068 [P] [US5] Implementar adaptador Antigravity documental em SpecAudit/Infrastructure/Agents/AntigravityAnalyzeAdapter.cs, ainda desabilitado, com conversa/cache apenas externos e sem importá-los como histórico oficial.
- [ ] T069 [US5] Executar prova real individual AnalyzeAntigravity em evidence/AnalyzeAntigravity-proof.md antes de habilitar, com a mesma matriz de proteção e escopo do Codex.
- [ ] T070 [US5] Habilitar Antigravity só com Assessment Passed em SpecAudit/Application/AgentCompatibilityService.cs e SpecAudit/Components/Verifications/AgentSelector.razor, sem afetar Codex/gate. Preservar o visual de design/mockups/verificacoes-pasta-externa.png e aplicar as adaptações obrigatórias de design/mockups/README.md; os comportamentos são exclusivamente os da V1 atual.
- [ ] T071 [US5] Executar os cenários US5-S01–S07 / US5-AC01–AC07 no host/harness real e regressão relacionada, comparando resultado e efeitos; registrar versão, comandos e saídas em evidence/US5-validation.md.
- [ ] T072 [US5] Revisar diff, critérios, contratos, segurança e resultados da versão de US5 em evidence/US5-review.md, declarando autorrevisão.
- [ ] T073 [US5] Resolver achados de US5 e repetir checks afetados em evidence/US5-validation.md e evidence/US5-review.md; se nenhum, registrar referência à revisão real.
- [ ] T074 [US5] Consolidar e conferir roteiro reproduzível em validation/US5-dev.md e registrar estado técnico em acceptance/US5.md, mantendo decisão do DEV pendente.
- [ ] T075 [US5] Executar a verificação do incremento PR-D e regressão na base declarada, incluindo comportamento real e estado intermediário seguro, em evidence/PR-D-validation.md.
- [ ] T076 [US5] Revisar o incremento completo PR-D em evidence/PR-D-review.md com diff incremental, limites e achados por arquivo/linha.
- [ ] T077 [US5] Resolver e revalidar achados do PR-D em evidence/PR-D-validation.md e evidence/PR-D-review.md; registrar nenhum achado somente com review real.
- [ ] T078 [US5] Preparar verification/prs/PR-D.md pelo pr-template.md CEMG com escopo final, tasks, base/predecessor, evidências e roteiro; não abrir/integrar PR ou marcar aprovação humana.

## Fase 9 — US6: Sugestões, Save e histórico (P2)

**Objetivo**: Resultados oficiais em memória até Salvar, novo pacote externo e decisões atribuíveis.
**Dependências**: PR-A/B/C/D; ExternalWriter deve passar antes de habilitar Save.
**Teste independente**: US6-S01–S04: fechar sem salvar, exportar três formatos, revisar decisões e comparar/excluir só registro escolhido.

- [ ] T079 [US6] Escrever contratos US6-S01–S04 em tests/SpecAudit.Windows.Tests/ExternalWriterTests.cs, tests/SpecAudit.Tests/FindingDecisionTests.cs e tests/SpecAudit.Ui.Tests/SaveHistoryTests.cs antes de gravar relatórios. Incluir comparação visual com design/mockups/README.md e os mockups da tela, exigindo as substituições de comportamento da V1.
- [ ] T080 [US6] Definir HumanDecision/SavedRecord e classificação em SpecAudit/Domain/HumanDecision.cs, SpecAudit/Domain/SavedRecord.cs e SpecAudit/Application/FindingDecisionService.cs com constraints literais abaixo.
- [ ] T081 [US6] Implementar writer e serialização em SpecAudit/Infrastructure/Reports/ExternalReportWriter.cs e SpecAudit/Infrastructure/Reports/ReportSerializer.cs, ainda sem botão Save habilitado.
- [ ] T082 [US6] Executar prova ExternalWriter real em evidence/ExternalWriter-proof.md antes de habilitar Save, observando conteúdo/caminhos no disco e alterações concorrentes.
- [ ] T083 [US6] Implementar listagem/validação/comparação e exclusão explícita de histórico em SpecAudit/Infrastructure/Reports/SavedHistoryReader.cs e SpecAudit/Application/SavedHistoryService.cs, usando IDs e destino externo resolvido.
- [ ] T084 [US6] Habilitar Save/formatos/histórico e decisões em SpecAudit/Components/Pages/Suggestions.razor e SpecAudit/Components/Reports/SaveDialog.razor somente com ExternalWriter Passed. Preservar o visual de design/mockups/documentos-sugestoes.png e aplicar as adaptações obrigatórias de design/mockups/README.md; os comportamentos são exclusivamente os da V1 atual.
- [ ] T085 [US6] Executar os cenários US6-S01–S04 / US6-AC01–AC04 no host/harness real e regressão relacionada, comparando resultado e efeitos; registrar versão, comandos e saídas em evidence/US6-validation.md.
- [ ] T086 [US6] Revisar diff, critérios, contratos, segurança e resultados da versão de US6 em evidence/US6-review.md, declarando autorrevisão.
- [ ] T087 [US6] Resolver achados de US6 e repetir checks afetados em evidence/US6-validation.md e evidence/US6-review.md; se nenhum, registrar referência à revisão real.
- [ ] T088 [US6] Consolidar e conferir roteiro reproduzível em validation/US6-dev.md e registrar estado técnico em acceptance/US6.md, mantendo decisão do DEV pendente.

## Fase 10 — Regressão e fechamento integrado

**Objetivo**: Demonstrar conjunto no navegador/harness, consolidar evidências e descrição final do PR-E sem aceite humano automático.
**Dependências**: Todas as histórias técnicas prontas; US7 permanece fora da V1.
**Teste independente**: Todos os 29 cenários, selos individuais, gate offline e ausência de código/escrita na origem.

- [ ] T089 Executar regressão integrada dos 29 cenários e auditoria final das dependências do próprio SpecAudit em evidence/feature-validation.md e revisar quickstart.md para refletir caminho real entregue. Comparar todas as telas com design/mockups/README.md nos dois temas, preservando aparência e comprovando substituições de comportamento da V1.
- [ ] T090 Executar a verificação do incremento PR-E e regressão na base declarada, incluindo comportamento real e estado intermediário seguro, em evidence/PR-E-validation.md.
- [ ] T091 Revisar o incremento completo PR-E em evidence/PR-E-review.md com diff incremental, limites e achados por arquivo/linha.
- [ ] T092 Resolver e revalidar achados do PR-E em evidence/PR-E-validation.md e evidence/PR-E-review.md; registrar nenhum achado somente com review real.
- [ ] T093 Preparar verification/prs/PR-E.md pelo pr-template.md CEMG com escopo final, tasks, base/predecessor, evidências e roteiro; não abrir/integrar PR ou marcar aprovação humana.
- [ ] T094 Consolidar roteiro integrado e estado técnico em validation/feature-dev.md, acceptance/feature.md e verification/implementation-stage.json; deixar reprodução/aceite humano pendentes até fonte real.

## Contrato de cada task

Cada linha herda ambiente/fixture/limpeza de quickstart.md, sem exigir teste futuro para comprovar a entrega atual. Tasks de escrita de teste registram red significativo; a prova funcional ocorre na task de execução posterior, nunca antecipadamente. IDs de dependência são técnicos; integrar PR depende ainda de aprovação humana real.

| ID | Origem | Depende de | Entrega / arquivos | Como verificar | Resultado esperado |
|---|---|---|---|---|---|
| T001 | plan contexto; quickstart; constituição VII | Nenhuma | tests/SpecAudit.Tests/SpecAudit.Tests.csproj; tests/SpecAudit.Windows.Tests/SpecAudit.Windows.Tests.csproj; tests/SpecAudit.Ui.Tests/SpecAudit.Ui.Tests.csproj; evidence/setup-validation.md | Reutilizar testes adequados se existirem; dotnet --info, dotnet restore/test dos projetos próprios e dotnet list package --vulnerable --include-transitive; registrar falha de consulta, versões e smoke. | Projetos de teste próprios executáveis e dependências auditadas; nada instalado/elevado automaticamente ou executado na solução auditada. |
| T002 | quickstart receita e observação | T001 | tests/SpecAudit.Windows.Tests/Fixtures/DocumentaryFixture.cs; tests/SpecAudit.Ui.Tests/Fixtures/LocalHostFixture.cs | Gerar duas raízes TEMP exclusivas, 30/40 tarefas e 12/20 requisitos, gates positivos/negativos, .cs canário e reparse/TOCTOU com barreira de sincronização; instrumentar path/operação/resultado e validar conjunto/hashes. | Fixture e logs técnicos externos reproduzíveis; leitura indevida detectável antes de conteúdo; limpeza só sob raiz sintética resolvida e autorizada. |
| T003 | contracts/application.md sessão; constituição VI | T001 | SpecAudit/Program.cs; SpecAudit/Application/LocalSessionService.cs | Iniciar host próprio http://127.0.0.1:5187; testar host/origin indevidos, IDs de outra sessão e ações mutantes sem validação; inspecionar logs sem conteúdo. | Só sessão/loopback válidos; nenhuma execução ou persistência automática; estado inseguro não exposto. |
| T004 | data-model identidade/estados; contracts/verification.md | T002, T003 | tests/SpecAudit.Tests/SessionContractTests.cs; tests/SpecAudit.Windows.Tests/ExecutionContractTests.cs | Exercitar IDs cruzados, CapabilityAvailability separado de RunState, ausência de selo, canal de outro RunId, cancelamento e geração; observar falha por regra ausente, não ambiente quebrado. | Testes detectam bypass de sessão/capacidade e transições inválidas; modelo ainda não implementado não é prova funcional. |
| T005 | data-model entidades e estados | T004 | SpecAudit/Domain/ProjectSession.cs; SpecAudit/Domain/DocumentSnapshot.cs; SpecAudit/Domain/ProtectionAssessment.cs | Rodar SessionContractTests com chaves contextualizadas, hashes/geração e selos NotVerified/Passed/Failed/Stale da versão/política; assegurar ausência de caminho livre na API. | Estado em memória; sem disponibilidade não se cria Run; selo de versão diferente não habilita. |
| T006 | constituição 4.0.0; plan | T005 | TrustedCommandRunner.cs | Executar harness confiável com argumentos separados, stdin, saída/erro, timeout e cancelamento; rejeitar cwd dentro da origem e operação desconhecida. | Processo comum não elevado e configuração controlada; limites de confiança explícitos. |
| T007 | constituição 4.0.0; plan | T006 | CapabilityRegistry.cs; ExecutionLimits.cs | Testar prova ausente/incompleta, política/versão/plataforma alteradas, tamanho/profundidade/saída/duração. | Sem habilitação por default; excesso é Inconclusive/LimitExceeded. |
| T008 | constituição 4.0.0; plan | T007 | ReaderBoundaryTests.cs; ProjectSelectionTests.cs | Dados sintéticos: IDs cruzados, duas funcionalidades, destino interno/junção, ACL ampla e edição externa; observar hashes e respostas reais das APIs. | A aplicação só lê documentos autorizados, não grava na origem nem confunde sessões. |
| T009 | US1-AC01/02; contracts/verification.md ReadDocumentCapability | T008 | SpecAudit/Infrastructure/Documents/DocumentCatalog.cs; SpecAudit/Infrastructure/Documents/DocumentBroker.cs | Executar testes Windows de abertura permitida, .cs/.razor/config e arquivo fora do catálogo negados antes de ler; junction trocada e arquivo substituído geram OutOfScope/Changed. | Somente docs/registros reconhecidos, não só extensão; sem cópia da solução, write/delete/ACL ou pré-checagem isolada vulnerável a TOCTOU. |
| T010 | constituição 4.0.0; plan | T009 | evidence/ReaderBoundary-v4-proof.md | Executar ReaderBoundaryTests/ProjectSelectionTests, ler via broker, negar código/config/IDs/junções, comparar origem e preservar edição normal do usuário. | Controles de aplicação demonstrados; nenhuma promessa de negação pelo kernel a agente arbitrário. |
| T011 | US1-AC01/02/03; constituição 4.0.0 | T010, T098 | ProjectService.cs; NativeFolderPicker.cs; Projects.razor | Exercitar seleção controlada com filesystem real, destinos internos/junção, troca de projetos/destino e página em navegador real | APIs readonly e contextos separados; diálogo nativo continua sujeito à validação humana integrada da US1 |
| T012 | US1-AC04 | T011 | SpecAudit/Infrastructure/Documents/DocumentChangeMonitor.cs; SpecAudit/Application/ProjectService.cs | Editar documento durante leitura, entre Prepare/Execute e antes de Save; verificar Stale/Changed e falta de monitoramento de conteúdo de código. | Não mistura snapshots, não restaura, não dispara agente/gate; troca de projeto cancela trabalho próprio sem salvar. |
| T013 | US1-S01–S04 / US1-AC01–AC04 | T012 | evidence/US1-validation.md | Usar receita/host de quickstart.md; executar testes aplicáveis e ações reais dos cenários; conferir acessos, origem e relatório oficial antes de Salvar. | Asserções dos cenários observadas; falha/ausência fica explícita e bloqueia habilitação dependente; mocks não substituem integração. |
| T014 | US1-S01–S04 / US1-AC01–AC04 | T013 | evidence/US1-review.md | Comparar diff à base do PR, cada critério à asserção real e evidências; listar arquivo/linha/efeito/prioridade e limites. | Review atual com achados rastreáveis; nenhum aceite humano preenchido. |
| T015 | US1-S01–S04 / US1-AC01–AC04 | T014 | evidence/US1-validation.md; evidence/US1-review.md | Corrigir somente escopo autorizado, executar novamente cenários/regressão afetados e renovar versão/hashes/review. | Sem impeditivo aberto para fechamento normal; se inviável, manter bloqueado, sem converter falha em aprovação. |
| T016 | US1-S01–S04 / US1-AC01–AC04 | T015 | validation/US1-dev.md; acceptance/US1.md | Iniciar incremento e repetir passos IA, fornecer mesmos dados/entrada/resultado ao DEV; se impossível, rascunhar dev-impossibility-template com causa/risco sem confirmar pelo DEV. | Roteiro distingue ESCRITO/conferido/executado pelo DEV; nenhuma checkbox de aprovação humana; aceite só por decisão atribuível. |
| T017 | US1; fundação e proteção de leitura | T016 | evidence/PR-A-validation.md | Rodar suítes aplicáveis descritas nas tarefas precedentes e host/harness de quickstart.md; coletar comando/cwd/versão/entrada/esperado/observado/efeitos. | Testes e comportamento do escopo deste PR comprovados ou bloqueio registrado; não exigir funcionalidade de PR futuro. |
| T018 | US1; fundação e proteção de leitura | T017 | evidence/PR-A-review.md | Inspecionar diff e evidências da revisão candidata; verificar integração dos componentes e invariantes; declarar autorrevisão. | Review rastreável; não inferir revisão humana ou aprovação de PR. |
| T019 | US1; fundação e proteção de leitura | T018 | evidence/PR-A-validation.md; evidence/PR-A-review.md | Após correção ou mudança de base, repetir checks afetados e renovar revisão/hashes; sem possibilidade, registrar causa e dependentes bloqueados. | Nenhum impeditivo aberto para integração normal; resultados reais preservados. |
| T020 | US1; fundação e proteção de leitura | T019 | verification/prs/PR-A.md | Conferir descrição contra diff final e template instalado; indicar aprovação humana PENDENTE e condições técnicas reais. | PR revisável com validação própria, descrição atual e decisão humana separada; sem publicação automática. |
| T021 | US2-AC01/02/03/04 | T020 | tests/SpecAudit.Ui.Tests/MarkdownPreviewTests.cs; tests/SpecAudit.Tests/DocumentNavigationTests.cs | Cobrir todos os recursos do corpus, links quebrados/fora do catálogo, modos e payloads ativos/remotos; observar falha esperada antes de implementar. Preparar comparação com design/mockups/README.md, sem validar comportamentos antigos presentes nos pixels. | Asserções detectam conteúdo omitido/ativo e navegação indevida; sem fonte/code scan. |
| T022 | US2-AC02/03/04; constituição VII | T021 | SpecAudit/Infrastructure/Documents/MarkdownRenderer.cs; SpecAudit/wwwroot/lib/preview/ | Auditar componentes diretos/transitivos, licenças/versões e hashes antes de introduzir; rodar corpus offline, Mermaid inválido/PlantUML fonte e HTML ativo. | Títulos/listas/tarefas não editáveis/tabelas/citações/imagens/código em documento corretos; só Mermaid renderizado; scripts bloqueados. |
| T023 | US2-AC01/03/04 | T022 | evidence/preview-safety.md | Rodar DocumentNavigationTests e testes reais de renderer; verificar scripts/formulários, SVG/HTML/Mermaid não confiáveis, rede automática zero, links/âncoras/catalog e imagens remotas somente por ação explícita sem credenciais. | Qualquer payload ativo ou leitura fora do catálogo impede habilitação do preview; todos os efeitos observados registrados. |
| T024 | US2-AC01/02/03/04 | T023 | SpecAudit/Components/Pages/Documents.razor; SpecAudit/Components/Documents/MarkdownPreview.razor | Rodar MarkdownPreviewTests no host e comparar corpus ao preview padrão VS Code sem extensões; autorizar imagem na sessão e navegar links externos por clique. Comparar a tela real nos temas aplicáveis ao mockup design/mockups/documentos-sugestoes.png e ao guia design/mockups/README.md, registrando diferenças intencionais e validando regras V1. | Sem edição, scripts ou rede silenciosa; equivalência de conteúdo e fontes/limitações visíveis. |
| T025 | US2-S01–S04 / US2-AC01–AC04 | T024 | evidence/US2-validation.md | Usar receita/host de quickstart.md; executar testes aplicáveis e ações reais dos cenários; conferir acessos, origem e relatório oficial antes de Salvar. | Asserções dos cenários observadas; falha/ausência fica explícita e bloqueia habilitação dependente; mocks não substituem integração. |
| T026 | US2-S01–S04 / US2-AC01–AC04 | T025 | evidence/US2-review.md | Comparar diff à base do PR, cada critério à asserção real e evidências; listar arquivo/linha/efeito/prioridade e limites. | Review atual com achados rastreáveis; nenhum aceite humano preenchido. |
| T027 | US2-S01–S04 / US2-AC01–AC04 | T026 | evidence/US2-validation.md; evidence/US2-review.md | Corrigir somente escopo autorizado, executar novamente cenários/regressão afetados e renovar versão/hashes/review. | Sem impeditivo aberto para fechamento normal; se inviável, manter bloqueado, sem converter falha em aprovação. |
| T028 | US2-S01–S04 / US2-AC01–AC04 | T027 | validation/US2-dev.md; acceptance/US2.md | Iniciar incremento e repetir passos IA, fornecer mesmos dados/entrada/resultado ao DEV; se impossível, rascunhar dev-impossibility-template com causa/risco sem confirmar pelo DEV. | Roteiro distingue ESCRITO/conferido/executado pelo DEV; nenhuma checkbox de aprovação humana; aceite só por decisão atribuível. |
| T029 | US3-AC01/02/03/04 | T020 | tests/SpecAudit.Tests/DocumentaryProgressTests.cs; tests/SpecAudit.Ui.Tests/DashboardTests.cs | Usar fixture 30/40 e 12/20, escopo vazio, links ambíguos, READY documental e documento alterado; testar fórmula/procedência sem código. Preparar comparação com design/mockups/README.md, sem validar comportamentos antigos presentes nos pixels. | Teste falha se checkbox/hash virar implementação, denominador omitir inconclusivos ou atualização executar agente. |
| T030 | US3-AC01/02/03/04; data-model | T029 | SpecAudit/Domain/TaskDeclaration.cs; SpecAudit/Domain/DashboardProjection.cs; SpecAudit/Application/DocumentaryDashboardService.cs | Rodar DocumentaryProgressTests com contagens, duplicidades e vínculos explícitos versus inferidos; usar apenas snapshots do broker; marcar Stale por geração. | 75% declarado e 60% documental, vazio NoItems; nenhum score de implementação ou leitura de código; update sem agente/gate. |
| T031 | US3-AC01/02/03/04 | T030 | SpecAudit/Components/Pages/Dashboard.razor | Rodar DashboardTests no host; seguir cada número/ocorrência até fonte autorizada, aplicar filtros e editar documento enquanto aberto. Comparar a tela real nos temas aplicáveis ao mockup design/mockups/dashboard-requisito.png e ao guia design/mockups/README.md, registrando diferenças intencionais e validando regras V1. | Contagens e regra explícitas; fonte e revisão visíveis; registro READY não é teste/aceite; alteração invalida sem execução automática. |
| T032 | US3-S01–S04 / US3-AC01–AC04 | T031 | evidence/US3-validation.md | Usar receita/host de quickstart.md; executar testes aplicáveis e ações reais dos cenários; conferir acessos, origem e relatório oficial antes de Salvar. | Asserções dos cenários observadas; falha/ausência fica explícita e bloqueia habilitação dependente; mocks não substituem integração. |
| T033 | US3-S01–S04 / US3-AC01–AC04 | T032 | evidence/US3-review.md | Comparar diff à base do PR, cada critério à asserção real e evidências; listar arquivo/linha/efeito/prioridade e limites. | Review atual com achados rastreáveis; nenhum aceite humano preenchido. |
| T034 | US3-S01–S04 / US3-AC01–AC04 | T033 | evidence/US3-validation.md; evidence/US3-review.md | Corrigir somente escopo autorizado, executar novamente cenários/regressão afetados e renovar versão/hashes/review. | Sem impeditivo aberto para fechamento normal; se inviável, manter bloqueado, sem converter falha em aprovação. |
| T035 | US3-S01–S04 / US3-AC01–AC04 | T034 | validation/US3-dev.md; acceptance/US3.md | Iniciar incremento e repetir passos IA, fornecer mesmos dados/entrada/resultado ao DEV; se impossível, rascunhar dev-impossibility-template com causa/risco sem confirmar pelo DEV. | Roteiro distingue ESCRITO/conferido/executado pelo DEV; nenhuma checkbox de aprovação humana; aceite só por decisão atribuível. |
| T036 | US4-AC01/02/03/04 | T031, T024 | tests/SpecAudit.Tests/RequirementTraceTests.cs; tests/SpecAudit.Ui.Tests/RequirementSheetTests.cs | Testar IDs iguais em funcionalidades diferentes, duplicados na mesma, vínculo inferido, ausência/ambiguidade e referências a código/configuração. Preparar comparação com design/mockups/README.md, sem validar comportamentos antigos presentes nos pixels. | Sem merge indevido de IDs, conclusão de implementação ou leitura de destino proibido. |
| T037 | US4-AC01/02/03/04; data-model | T036 | SpecAudit/Domain/Requirement.cs; SpecAudit/Domain/EvidenceLink.cs; SpecAudit/Domain/Finding.cs; SpecAudit/Application/RequirementTraceService.cs | Rodar RequirementTraceTests em documentos de linguagens diferentes; navegar só DocumentIds autorizados; validar fontes/hash e evidência insuficiente. | CoverageState documental e proveniência explícita/inferida; referências a fonte viram OutOfScope e não código aberto. |
| T038 | US4-AC01/02/03/04 | T037 | SpecAudit/Components/Pages/Requirements.razor; SpecAudit/Components/Requirements/RequirementSheet.razor | Rodar RequirementSheetTests; abrir vínculos e declarações externas, focar requisito e conferir limite de implementação futura; sem criar arquivo de ficha. Comparar a tela real nos temas aplicáveis ao mockup design/mockups/dashboard-requisito.png e ao guia design/mockups/README.md, registrando diferenças intencionais e validando regras V1. | Mesma consulta documental independente de linguagem; sem Implemented/NotImplemented ou aceite pela IA; pontos para combo posterior sem executar nada. |
| T039 | US4-S01–S04 / US4-AC01–AC04 | T038 | evidence/US4-validation.md | Usar receita/host de quickstart.md; executar testes aplicáveis e ações reais dos cenários; conferir acessos, origem e relatório oficial antes de Salvar. | Asserções dos cenários observadas; falha/ausência fica explícita e bloqueia habilitação dependente; mocks não substituem integração. |
| T040 | US4-S01–S04 / US4-AC01–AC04 | T039 | evidence/US4-review.md | Comparar diff à base do PR, cada critério à asserção real e evidências; listar arquivo/linha/efeito/prioridade e limites. | Review atual com achados rastreáveis; nenhum aceite humano preenchido. |
| T041 | US4-S01–S04 / US4-AC01–AC04 | T040 | evidence/US4-validation.md; evidence/US4-review.md | Corrigir somente escopo autorizado, executar novamente cenários/regressão afetados e renovar versão/hashes/review. | Sem impeditivo aberto para fechamento normal; se inviável, manter bloqueado, sem converter falha em aprovação. |
| T042 | US4-S01–S04 / US4-AC01–AC04 | T041 | validation/US4-dev.md; acceptance/US4.md | Iniciar incremento e repetir passos IA, fornecer mesmos dados/entrada/resultado ao DEV; se impossível, rascunhar dev-impossibility-template com causa/risco sem confirmar pelo DEV. | Roteiro distingue ESCRITO/conferido/executado pelo DEV; nenhuma checkbox de aprovação humana; aceite só por decisão atribuível. |
| T043 | US8-AC01/02 | T038 | tests/SpecAudit.Ui.Tests/NavigationThemeTests.cs | Percorrer cinco áreas em sessão; trocar tema com ocorrência selecionada; verificar projeto/origem/destino e texto/ícone independente de cor. Preparar comparação com design/mockups/README.md, sem validar comportamentos antigos presentes nos pixels. | Nada executa/grava ao navegar; seleção/conteúdo se mantém e incompleto não parece habilitado. |
| T044 | US8-AC01/02 | T043 | SpecAudit/Components/Layout/MainLayout.razor; SpecAudit/wwwroot/app.css; SpecAudit/Components/Pages/Verifications.razor; SpecAudit/Components/Pages/Suggestions.razor | Rodar NavigationThemeTests em host real; verificar foco/legibilidade/texto/ícone e contexto ao alternar modos/áreas. Comparar a tela real nos temas aplicáveis ao mockup design/mockups/dashboard-requisito.png e ao guia design/mockups/README.md, registrando diferenças intencionais e validando regras V1. | Cinco áreas presentes e sem perda de estado; sem autosave de conteúdo ou funcionalidade sem selo. |
| T045 | US8-S01/S02 / US8-AC01/02 | T044 | evidence/US8-validation.md | Usar receita/host de quickstart.md; executar testes aplicáveis e ações reais dos cenários; conferir acessos, origem e relatório oficial antes de Salvar. | Asserções dos cenários observadas; falha/ausência fica explícita e bloqueia habilitação dependente; mocks não substituem integração. |
| T046 | US8-S01/S02 / US8-AC01/02 | T045 | evidence/US8-review.md | Comparar diff à base do PR, cada critério à asserção real e evidências; listar arquivo/linha/efeito/prioridade e limites. | Review atual com achados rastreáveis; nenhum aceite humano preenchido. |
| T047 | US8-S01/S02 / US8-AC01/02 | T046 | evidence/US8-validation.md; evidence/US8-review.md | Corrigir somente escopo autorizado, executar novamente cenários/regressão afetados e renovar versão/hashes/review. | Sem impeditivo aberto para fechamento normal; se inviável, manter bloqueado, sem converter falha em aprovação. |
| T048 | US8-S01/S02 / US8-AC01/02 | T047 | validation/US8-dev.md; acceptance/US8.md | Iniciar incremento e repetir passos IA, fornecer mesmos dados/entrada/resultado ao DEV; se impossível, rascunhar dev-impossibility-template com causa/risco sem confirmar pelo DEV. | Roteiro distingue ESCRITO/conferido/executado pelo DEV; nenhuma checkbox de aprovação humana; aceite só por decisão atribuível. |
| T049 | US2/US3/US4/US8; ReaderBoundary preservado | T048, T028, T035, T042 | evidence/PR-B-validation.md | Rodar suítes aplicáveis descritas nas tarefas precedentes e host/harness de quickstart.md; coletar comando/cwd/versão/entrada/esperado/observado/efeitos. | Testes e comportamento do escopo deste PR comprovados ou bloqueio registrado; não exigir funcionalidade de PR futuro. |
| T050 | US2/US3/US4/US8; ReaderBoundary preservado | T049 | evidence/PR-B-review.md | Inspecionar diff e evidências da revisão candidata; verificar integração dos componentes e invariantes; declarar autorrevisão. | Review rastreável; não inferir revisão humana ou aprovação de PR. |
| T051 | US2/US3/US4/US8; ReaderBoundary preservado | T050 | evidence/PR-B-validation.md; evidence/PR-B-review.md | Após correção ou mudança de base, repetir checks afetados e renovar revisão/hashes; sem possibilidade, registrar causa e dependentes bloqueados. | Nenhum impeditivo aberto para integração normal; resultados reais preservados. |
| T052 | US2/US3/US4/US8; ReaderBoundary preservado | T051 | verification/prs/PR-B.md | Conferir descrição contra diff final e template instalado; indicar aprovação humana PENDENTE e condições técnicas reais. | PR revisável com validação própria, descrição atual e decisão humana separada; sem publicação automática. |
| T053 | US5-AC03/05/06/07 | T052 | tests/SpecAudit.Windows.Tests/GateAdapterTests.cs; tests/SpecAudit.Ui.Tests/VerificationSelectionTests.cs | Cobrir READY/BLOCKED/DEV_IMPOSSIBILITY_RECORDED, hash/revisão inválidos, logs/artifacts aninhados, .cs/junção/TOCTOU, ausência Python/selos e nenhum agente/rede. Preparar comparação com design/mockups/README.md, sem validar comportamentos antigos presentes nos pixels. | Teste distingue resultado core de erro/cancelamento; detecta leitura indevida e terceira opção/execução ao selecionar. |
| T054 | US5-AC03/05/06/07; data-model | T053 | SpecAudit/Domain/GateCandidate.cs; SpecAudit/Domain/VerificationRun.cs; SpecAudit/Application/VerificationService.cs | Rodar testes de estados com Availability separado, preparação vencida, cancelamento sem término e gate de funcionalidade na ficha; validar ownership de IDs. | Sem disponibilidade não cria Run; Preparing pode Blocked; falha não é requisito reprovado; sem approval de IA. |
| T055 | US5-AC06/07; contracts/verification.md CheckGate | T054 | SpecAudit/Infrastructure/Gates/Core/check_gate.py; SpecAudit/Infrastructure/Gates/Core/policies/gate-requirements.json; SpecAudit/Infrastructure/Gates/GatePackage.cs | Comparar bytes/hashes ao preset confiável instalado; rejeitar core/política/runtime alterados; auditar componentes; ausência de Python comunica indisponibilidade sem instalar. | Pacote confiável versionado, adaptação identificada, sem --project/main/active e sem alteração do preset original. |
| T056 | US5-AC05/06/07; contrato CheckGate | T055 | SpecAudit/Infrastructure/Gates/runner.py; SpecAudit/Infrastructure/Gates/catalog_path.py; SpecAudit/Infrastructure/Gates/GateExecutor.cs | Executar GateAdapterTests em fixture com -I -B/cwd externo, sem shell/rede; todas as operações resolve/join/read_bytes/read_text do core usam catálogo/broker, só política do pacote é exceção; testar grafo completo e TOCTOU no ponto da leitura. | Não entrega path real da origem ao Python/core, não cria cópia da solução e não abre fonte/configuração; limite/preflight bloqueia sem READY parcial. |
| T057 | US5-AC05/06/07; selo GateAdapter | T056 | evidence/GateAdapter-proof.md | Rodar GateAdapterTests reais: equivalência de resultados/exit, todos os stages suportados inclusive pr/acceptance/impossibility com fontes atribuídas, offline, cancelamento/descendentes, paths aninhados e .cs/junção negados antes de ler. | Equivalência e restrições observadas; falso READY/ausência de prova mantém selo Failed/NotVerified e bloqueia a próxima tarefa. |
| T058 | US5-AC03/05/06/07 | T057 | SpecAudit/Components/Verifications/VerificationSelector.razor; SpecAudit/Components/Pages/Verifications.razor; SpecAudit/Components/Requirements/RequirementSheet.razor | Rodar VerificationSelectionTests no host: exatamente analyze/check_gate, seleção por projeto sincronizada, sem executar ao selecionar; confirmar funcionalidade/etapa/registro/revisão esperada antes do clique Executar. Comparar a tela real nos temas aplicáveis ao mockup design/mockups/verificacoes-pasta-externa.png e ao guia design/mockups/README.md, registrando diferenças intencionais e validando regras V1. | Gate funciona sem agente/rede; analyze ainda indisponível; botão do gate bloqueado com selo ausente/versão alterada; alcance real visível na ficha. |
| T059 | US5-S03/S05/S06/S07, parcialmente; analyze permanece desabilitado | T058 | evidence/PR-C-validation.md | Rodar suítes aplicáveis descritas nas tarefas precedentes e host/harness de quickstart.md; coletar comando/cwd/versão/entrada/esperado/observado/efeitos. | Testes e comportamento do escopo deste PR comprovados ou bloqueio registrado; não exigir funcionalidade de PR futuro. |
| T060 | US5-S03/S05/S06/S07, parcialmente; analyze permanece desabilitado | T059 | evidence/PR-C-review.md | Inspecionar diff e evidências da revisão candidata; verificar integração dos componentes e invariantes; declarar autorrevisão. | Review rastreável; não inferir revisão humana ou aprovação de PR. |
| T061 | US5-S03/S05/S06/S07, parcialmente; analyze permanece desabilitado | T060 | evidence/PR-C-validation.md; evidence/PR-C-review.md | Após correção ou mudança de base, repetir checks afetados e renovar revisão/hashes; sem possibilidade, registrar causa e dependentes bloqueados. | Nenhum impeditivo aberto para integração normal; resultados reais preservados. |
| T062 | US5-S03/S05/S06/S07, parcialmente; analyze permanece desabilitado | T061 | verification/prs/PR-C.md | Conferir descrição contra diff final e template instalado; indicar aprovação humana PENDENTE e condições técnicas reais. | PR revisável com validação própria, descrição atual e decisão humana separada; sem publicação automática. |
| T063 | US5-AC01/02/04/05 | T062 | tests/SpecAudit.Windows.Tests/AgentDiscoveryTests.cs; tests/SpecAudit.Windows.Tests/AnalyzeProtectionTests.cs | Cobrir encontrado/ausente/incompatível, scripts/config maliciosos, segredo sintético, código fora do catálogo, autenticação ausente, cache interno externo e cancelamento pai/filho; mocks só domínio, provas reais posteriores. | Falhas do contrato são observáveis e bloqueiam somente adaptador pertinente; gate/leitor não dependem de autenticação. |
| T064 | US5-AC01; data-model | T063 | SpecAudit/Domain/AgentInstallation.cs; SpecAudit/Infrastructure/Agents/AgentDiscovery.cs | Rodar AgentDiscoveryTests com instalações reais quando disponíveis e fixtures de ausência; conferir identidade/versão sem usar executável/configuração do projeto; Unsupported para demais integrações. | Descoberta não é compatibilidade; seleção por projeto e locais externos informados, sem analisar nada na descoberta. |
| T065 | US5-AC02/04/05; AnalyzeDocuments | T064 | SpecAudit/Infrastructure/Agents/CodexAnalyzeAdapter.cs | Exercitar contrato com harness e erro de autenticação, schema inválido/invenção de DocumentId, ausência de spec/plan/tasks e cancelamento; nenhuma tool de shell/escrita/leitura arbitrária ou configuração da origem. | Não lê código, não cria relatório oficial nem arquivo de pré-requisito; saída inválida é Inconclusive/Failed; só prompt não habilita. |
| T066 | US5-AC02/04/05; selo AnalyzeCodex | T065 | evidence/AnalyzeCodex-proof.md | Rodar AnalyzeProtectionTests com Codex real autenticado, RunId/canal e duas funcionalidades; observar diagnóstico com fonte/hash, tentativas indevidas, canais de rede só necessários, cache/transcrição externos, cancelamento até job zerado. Filtrar explicitamente Provider=Codex; não exigir adaptador Antigravity ainda não implementado. | Tudo aplicável passa na versão/binário/política avaliada; incompatibilidade ou falta de ferramenta/acesso mantém selo pendente/falho e tarefa dependente bloqueada, sem ampliar direitos. |
| T067 | US5-AC01/02/03/04/05 | T066 | SpecAudit/Application/AgentCompatibilityService.cs; SpecAudit/Components/Verifications/AgentSelector.razor | No host repetir análise por clique, foco do requisito e cancelamento; adulterar/invalidar versão/política de selo e verificar indisponibilidade; mostrar envio externo e adaptação. Comparar a tela real nos temas aplicáveis ao mockup design/mockups/verificacoes-pasta-externa.png e ao guia design/mockups/README.md, registrando diferenças intencionais e validando regras V1. | Codex indisponível sem prova, nunca permissões ampliadas; gate e leitor continuam independentes. |
| T068 | US5-AC02/04/05; AnalyzeDocuments | T064 | SpecAudit/Infrastructure/Agents/AntigravityAnalyzeAdapter.cs | Exercitar contrato/harness, saídas e erro de autenticação; não carregar rules/plugins/config da origem; canal só documental e recursos fechados. | Nenhuma escrita na origem ou relatório oficial antes de Salvar; existência de transcrição externa informada não é relatório salvo. |
| T069 | US5-AC02/04/05; selo AnalyzeAntigravity | T068 | evidence/AnalyzeAntigravity-proof.md | Rodar AnalyzeProtectionTests com Antigravity real, fonte/hash e respostas; provar restrições OS/código/ferramentas/rede, caches externos e término de descendentes; não inferir sucesso de comando recusado antes do teste. Filtrar explicitamente Provider=Antigravity; provas não dependem de habilitação futura. | Compatibilidade comprovada por versão/ambiente ou indisponível com motivo; nenhuma prova antiga reaproveitada como atual. |
| T070 | US5-AC01/02/03/04/05 | T069, T067 | SpecAudit/Application/AgentCompatibilityService.cs; SpecAudit/Components/Verifications/AgentSelector.razor | No host executar/cancelar Antigravity, testar selo inválido e trocar agente/projeto; conferir estado interno informado e nenhuma persistência oficial. Comparar a tela real nos temas aplicáveis ao mockup design/mockups/verificacoes-pasta-externa.png e ao guia design/mockups/README.md, registrando diferenças intencionais e validando regras V1. | Cada agente possui selo/versão próprios; falha de um não habilita o outro nem bloqueia leitura/gate. |
| T071 | US5-S01–S07 / US5-AC01–AC07 | T070 | evidence/US5-validation.md | Usar receita/host de quickstart.md; executar testes aplicáveis e ações reais dos cenários; conferir acessos, origem e relatório oficial antes de Salvar. | Asserções dos cenários observadas; falha/ausência fica explícita e bloqueia habilitação dependente; mocks não substituem integração. |
| T072 | US5-S01–S07 / US5-AC01–AC07 | T071 | evidence/US5-review.md | Comparar diff à base do PR, cada critério à asserção real e evidências; listar arquivo/linha/efeito/prioridade e limites. | Review atual com achados rastreáveis; nenhum aceite humano preenchido. |
| T073 | US5-S01–S07 / US5-AC01–AC07 | T072 | evidence/US5-validation.md; evidence/US5-review.md | Corrigir somente escopo autorizado, executar novamente cenários/regressão afetados e renovar versão/hashes/review. | Sem impeditivo aberto para fechamento normal; se inviável, manter bloqueado, sem converter falha em aprovação. |
| T074 | US5-S01–S07 / US5-AC01–AC07 | T073 | validation/US5-dev.md; acceptance/US5.md | Iniciar incremento e repetir passos IA, fornecer mesmos dados/entrada/resultado ao DEV; se impossível, rascunhar dev-impossibility-template com causa/risco sem confirmar pelo DEV. | Roteiro distingue ESCRITO/conferido/executado pelo DEV; nenhuma checkbox de aprovação humana; aceite só por decisão atribuível. |
| T075 | US5 completo; regressão gate offline e ficha/seleção | T074 | evidence/PR-D-validation.md | Rodar suítes aplicáveis descritas nas tarefas precedentes e host/harness de quickstart.md; coletar comando/cwd/versão/entrada/esperado/observado/efeitos. | Testes e comportamento do escopo deste PR comprovados ou bloqueio registrado; não exigir funcionalidade de PR futuro. |
| T076 | US5 completo; regressão gate offline e ficha/seleção | T075 | evidence/PR-D-review.md | Inspecionar diff e evidências da revisão candidata; verificar integração dos componentes e invariantes; declarar autorrevisão. | Review rastreável; não inferir revisão humana ou aprovação de PR. |
| T077 | US5 completo; regressão gate offline e ficha/seleção | T076 | evidence/PR-D-validation.md; evidence/PR-D-review.md | Após correção ou mudança de base, repetir checks afetados e renovar revisão/hashes; sem possibilidade, registrar causa e dependentes bloqueados. | Nenhum impeditivo aberto para integração normal; resultados reais preservados. |
| T078 | US5 completo; regressão gate offline e ficha/seleção | T077 | verification/prs/PR-D.md | Conferir descrição contra diff final e template instalado; indicar aprovação humana PENDENTE e condições técnicas reais. | PR revisável com validação própria, descrição atual e decisão humana separada; sem publicação automática. |
| T079 | US6-AC01/02/03/04 | T078 | tests/SpecAudit.Windows.Tests/ExternalWriterTests.cs; tests/SpecAudit.Tests/FindingDecisionTests.cs; tests/SpecAudit.Ui.Tests/SaveHistoryTests.cs | Testar resultados em memória, falsa classificação sem motivo, fingerprint alterado, destino interno/reparse/TOCTOU, falha parcial, CSV fórmula e exclusão selecionada. Preparar comparação com design/mockups/README.md, sem validar comportamentos antigos presentes nos pixels. | Detecta relatório oficial antes de Save, sobrescrita, perda de decisão e origem como fallback; não importa cache de agente. |
| T080 | US6-AC01/03/04; data-model | T079 | SpecAudit/Domain/HumanDecision.cs; SpecAudit/Domain/SavedRecord.cs; SpecAudit/Application/FindingDecisionService.cs | Rodar FindingDecisionTests com Pending/Confirmed/FalsePositive/AcceptedRisk/Deferred, justificativa/data/ator, contexto alterado e risco sem alterar falha técnica. | Estados em memória até Save, revisão de decisão afetada sem apagar original; nenhuma aprovação humana inventada. |
| T081 | US6-AC01/02; contracts/persistence.md | T080 | SpecAudit/Infrastructure/Reports/ExternalReportWriter.cs; SpecAudit/Infrastructure/Reports/ReportSerializer.cs | Rodar ExternalWriterTests em fixture: manifesto SchemaVersion=1 por último, create-new, MD/JSON/CSV escolhidos, metadados completos, CSV seguro e erro parcial honesto; revalidar destino por handle no acesso. | Somente destino externo autorizado; sem overwrite/restore/autosave; incomplete não vira Saved; não excluir resíduos automaticamente. |
| T082 | US6-AC01/02; selo ExternalWriter | T081 | evidence/ExternalWriter-proof.md | Rodar ExternalWriterTests, fechar sessão sem Save, simular destino interno/junction trocada/permissão/disco cheio e conferir arquivos parciais externos; instrumentar acessos e origem intacta. | Zero relatório oficial antes de Save; todo gravado após ação fica externo; falha mantém Save indisponível e evidencia efeitos sem mascarar. |
| T083 | US6-AC04; contracts/persistence.md Histórico | T082 | SpecAudit/Infrastructure/Reports/SavedHistoryReader.cs; SpecAudit/Application/SavedHistoryService.cs | Rodar testes com pacotes válidos/incompletos/adulterados, mesmas chaves em projetos diferentes, amostra incompleta, troca de destino e seleção de uma exclusão. | Só histórico oficial salvo; não infere resolvido de ausência em amostra incompleta; sem retenção/exclusão automática ou acesso à origem. |
| T084 | US6-AC01/02/03/04 | T083 | SpecAudit/Components/Pages/Suggestions.razor; SpecAudit/Components/Reports/SaveDialog.razor | Rodar SaveHistoryTests no host, mostrar conteúdo/destino e atualidade antes da confirmação; exportar formatos, comparar/excluir; teste com selo ausente/adulterado. Comparar a tela real nos temas aplicáveis ao mockup design/mockups/documentos-sugestoes.png e ao guia design/mockups/README.md, registrando diferenças intencionais e validando regras V1. | Sem selo Save fica indisponível; não há salvamento por navegação/fechamento/erro; CSV e decisões rastreáveis, origem intacta. |
| T085 | US6-S01–S04 / US6-AC01–AC04 | T084 | evidence/US6-validation.md | Usar receita/host de quickstart.md; executar testes aplicáveis e ações reais dos cenários; conferir acessos, origem e relatório oficial antes de Salvar. | Asserções dos cenários observadas; falha/ausência fica explícita e bloqueia habilitação dependente; mocks não substituem integração. |
| T086 | US6-S01–S04 / US6-AC01–AC04 | T085 | evidence/US6-review.md | Comparar diff à base do PR, cada critério à asserção real e evidências; listar arquivo/linha/efeito/prioridade e limites. | Review atual com achados rastreáveis; nenhum aceite humano preenchido. |
| T087 | US6-S01–S04 / US6-AC01–AC04 | T086 | evidence/US6-validation.md; evidence/US6-review.md | Corrigir somente escopo autorizado, executar novamente cenários/regressão afetados e renovar versão/hashes/review. | Sem impeditivo aberto para fechamento normal; se inviável, manter bloqueado, sem converter falha em aprovação. |
| T088 | US6-S01–S04 / US6-AC01–AC04 | T087 | validation/US6-dev.md; acceptance/US6.md | Iniciar incremento e repetir passos IA, fornecer mesmos dados/entrada/resultado ao DEV; se impossível, rascunhar dev-impossibility-template com causa/risco sem confirmar pelo DEV. | Roteiro distingue ESCRITO/conferido/executado pelo DEV; nenhuma checkbox de aprovação humana; aceite só por decisão atribuível. |
| T089 | SC-001–008; plan/quickstart; constituição VII/IX | T088, T020, T052, T062, T078 | evidence/feature-validation.md; quickstart.md | Rodar suites próprias domínio/Windows/UI e host real com ambas integrações, fluxo leitura→combo→execução→cancelamento→Save; verificar todos os selos, versões, arquivos/acessos/rede e auditoria direta/transitiva; não executar solução auditada. Registrar comparação visual e diferenças intencionais segundo design/mockups/README.md, sem tratar mockup como execução. | Fluxo real observado por versão, sem leitura de código, escrita/autosave/segredos; falha/não execução bloqueia entrega dependente; achados de vulnerabilidade comunicados com origem/data/versão. |
| T090 | US6 + fechamento integrado US1/US2/US3/US4/US5/US8 e SC-001–008 | T089 | evidence/PR-E-validation.md | Rodar suítes aplicáveis descritas nas tarefas precedentes e host/harness de quickstart.md; coletar comando/cwd/versão/entrada/esperado/observado/efeitos. | Testes e comportamento do escopo deste PR comprovados ou bloqueio registrado; não exigir funcionalidade de PR futuro. |
| T091 | US6 + fechamento integrado US1/US2/US3/US4/US5/US8 e SC-001–008 | T090 | evidence/PR-E-review.md | Inspecionar diff e evidências da revisão candidata; verificar integração dos componentes e invariantes; declarar autorrevisão. | Review rastreável; não inferir revisão humana ou aprovação de PR. |
| T092 | US6 + fechamento integrado US1/US2/US3/US4/US5/US8 e SC-001–008 | T091 | evidence/PR-E-validation.md; evidence/PR-E-review.md | Após correção ou mudança de base, repetir checks afetados e renovar revisão/hashes; sem possibilidade, registrar causa e dependentes bloqueados. | Nenhum impeditivo aberto para integração normal; resultados reais preservados. |
| T093 | US6 + fechamento integrado US1/US2/US3/US4/US5/US8 e SC-001–008 | T092 | verification/prs/PR-E.md | Conferir descrição contra diff final e template instalado; indicar aprovação humana PENDENTE e condições técnicas reais. | PR revisável com validação própria, descrição atual e decisão humana separada; sem publicação automática. |
| T094 | constituição IX; PASSAGENS; execução reproduzível | T093 | validation/feature-dev.md; acceptance/feature.md; verification/implementation-stage.json | Conferir mesmas versões/dados/host/passos/resultados para DEV; vincular testes/reviews/hashes; se impossibilidade preparar rascunho e manter NÃO EXECUTADO sem decidir pelo DEV. | Pacote técnico rastreável; não marca PR aprovado/história aceita/merge/deploy por concluir tasks; nenhum commit/push/publicação automático. |

## Restrições literais do modelo por task

Aplicar campos da respectiva linha em data-model.md e preservar literalmente as restrições abaixo. Não criar tamanhos, retenção, metas ou estados de implementação adicionais. Testes da task correspondente devem cobrir estas regras.

| Task | Entidade / campos de origem | Restrição literal |
|---|---|---|
| T005 | ProjectSession: ProjectId, DisplayName, OriginHandleIdentity, OriginDisplayPath, OutputDirectoryIdentity, CreatedUtc, ActiveFeatureId, SelectedVerification, SelectedAgentId, Generation | “Origin/destino não podem coincidir nem se conter de forma que destino grave na origem; validar junções/links; contexto nunca é reaproveitado entre projetos” |
| T005 | Feature: FeatureId, ProjectId, SpecKitRelativeDirectory, Title, DocumentIds, CapabilitySummary | “Caminho relativo só dentro do catálogo autorizado; artefato ausente é estado explícito” |
| T005 | CatalogEntry: DocumentId, ProjectId, FeatureId opcional, RelativePath, Kind, FileIdentity, ContentHash, Length, LastObservedUtc, CatalogGeneration | “Kind: Constitution, Spec, Plan, Tasks, Checklist, Research, Design, GateRecord, EvidenceDocument, PresentationAsset; origem reconhecida SpecKit, não só extensão” |
| T005 | DocumentSnapshot: DocumentId, SnapshotId, ContentHash, Bytes em memória, Encoding, SourceGeneration, ReadUtc, References | “Obtido por handle somente leitura; não copia árvore da solução; não escreve arquivo temporário de conteúdo” |
| T037 | Requirement: RequirementKey=(ProjectId,FeatureId,LocalId), SourceDocumentId, Description, StoryRefs, CriteriaIds | “IDs duplicados na mesma funcionalidade geram ocorrência/ambiguidade; não mesclar” |
| T037 | Criterion: CriterionKey, RequirementKey, Text, DocumentLocation, TaskLinks, CoverageState | “CoverageState: Linked, MissingLink, Ambiguous; nunca Implemented/NotImplemented” |
| T030 | TaskDeclaration: TaskKey contextual, Text, MarkedComplete, SourceLocation, ExplicitRequirementRefs, Phase | “Checkbox é declaração documental; não comprova implementação” |
| T037 | EvidenceLink: LinkId, SourceLocation, TargetDocumentId opcional, TargetText, Relation, Provenance, Resolution | “Provenance Explicit/Inferred; Resolution Authorized, Missing, Ambiguous, OutOfScope; referência a .cs/.razor/config não é aberta” |
| T054 | GateCandidate: CandidateId, FeatureId, Stage, RecordDocumentId, ExpectedRevision, RevisionSource, EvidenceDocumentIds | “Stage fechado: spec, plan, tasks, pr, acceptance, dev-impossibility; revisão confirmada explicitamente, não aceita só por repetir registro” |
| T064 | AgentInstallation: AgentId, Provider, ExecutableIdentity, Version, DiscoveryMethod, ExternalStateLocations, CompatibilityState | “Provider Codex/Antigravity/ClaudeCode/Kiro; manual apenas se não encontrado; encontrado não significa habilitado” |
| T005 | ProtectionAssessment: AssessmentId, ComponentVersion, PlatformFingerprint, CapabilityDigest, Checks, EvidenceRefs, State | “State NotVerified, Passed, Failed, Stale; só Passed da versão/configuração aplicável permite habilitar integração” |
| T054 | VerificationRun: RunId, ProjectId, FeatureId, FocusRequirementKey opcional, Type, CandidateId/AgentId, InputSnapshotIds, State, Freshness, StartedUtc, EndedUtc, ExitCode, Output, Findings, Limitations | “Type somente AnalyzeDocuments/CheckGate; estado de execução separado do resultado semântico e atualidade” |
| T037 | Finding: FindingId, RunId opcional, Category, Severity, Explanation, SourceLocations, Suggestion, ContextFingerprint | “Gravidade justificada; conclusão de IA não vira aceite humano” |
| T080 | HumanDecision: DecisionId, FindingId, Classification, Reason, Actor, TimestampUtc, ContextFingerprint, NeedsReview | “Pending/Confirmed/FalsePositive/AcceptedRisk/Deferred; FalsePositive/AcceptedRisk exigem justificativa e responsável; decisão técnica original preservada” |
| T030 | DashboardProjection: ProjectId, FeatureId, CompletedTaskCount, TaskCount, ExplicitlyLinkedRequirementCount, RequirementCount, RecordedGateStates, Freshness | “Percentual=100*n/d se d>0; d=0 é NoItems; sem percentual de implementação” |
| T080 | SavedRecord: SavedRecordId, ProjectId, Scope, CreatedUtc, SchemaVersion, SourceHashes, ToolVersions, ContentManifest, SelectedFormats, DestinationIdentity | “Criado somente em Save bem-sucedido; novo diretório/arquivo por registro; nunca sobrescrever existente” |

Estados compartilhados de T005/T054: “Available, PendingValidation, Unsupported, Unavailable” pertencem a CapabilityAvailability; “Queued → Preparing → Running → Completed, Failed ou Cancelled; Preparing também pode terminar Blocked” pertence a VerificationRun.State. “Freshness: Current → Stale” não altera aprovação/estado histórico. Sem disponibilidade não se cria Run; cancelamento só é concluído depois de confirmar término dos próprios descendentes.

Estado de Save em T081: “Unsaved → Saving → Saved ou SaveFailed”. Decisão técnica e humana permanecem separadas; ator/data/justificativa são dados atribuídos e não autenticação humana provada pelo gate.

## Barreiras de habilitação obrigatórias

| Capacidade | Construção | Prova anterior | Habilitação dependente | Em falha |
|---|---|---|---|---|
| ReaderBoundary | T009 | T010 | T011 | Manter indisponível, registrar causa e bloquear dependente; não ampliar direitos |
| GateAdapter | T056 | T057 | T058 | Manter indisponível, registrar causa e bloquear dependente; não ampliar direitos |
| AnalyzeCodex | T065 | T066 | T067 | Manter indisponível, registrar causa e bloquear dependente; não ampliar direitos |
| AnalyzeAntigravity | T068 | T069 | T070 | Manter indisponível, registrar causa e bloquear dependente; não ampliar direitos |
| ExternalWriter | T081 | T082 | T084 | Manter indisponível, registrar causa e bloquear dependente; não ampliar direitos |

## Mapeamento de tasks para PRs

Base atual main/79d9c3d e alterações documentais desta feature identificadas por hash. PRs podem ser preparados de forma dependente com diff incremental explícito; integração normal requer predecessor integrado/aprovado conforme política, checks atuais e aprovação humana real. Descrição de PR não autoriza abertura/merge/push.

| PR previsto | Tasks incluídas | Objetivo | Predecessor/base | Validação própria | Review | Fechamento |
|---|---|---|---|---|---|---|
| PR-A | T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012, T013, T014, T015, T016, T017, T018, T019, T020 | Fronteira, seleção e invalidação | main + documentação atual | T017; evidence/PR-A-validation.md | T018/T019; evidence/PR-A-review.md | US1; aprovação humana PENDENTE |
| PR-B | T021, T022, T023, T024, T025, T026, T027, T028, T029, T030, T031, T032, T033, T034, T035, T036, T037, T038, T039, T040, T041, T042, T043, T044, T045, T046, T047, T048, T049, T050, T051, T052 | Leitor, dashboard, ficha e temas | PR-A | T049; evidence/PR-B-validation.md | T050/T051; evidence/PR-B-review.md | US2/US3/US4/US8; aprovação humana PENDENTE |
| PR-C | T053, T054, T055, T056, T057, T058, T059, T060, T061, T062 | Gate offline e combo fechada | PR-A/PR-B | T059; evidence/PR-C-validation.md | T060/T061; evidence/PR-C-review.md | US5 parcial; analyze indisponível; aprovação humana PENDENTE |
| PR-D | T063, T064, T065, T066, T067, T068, T069, T070, T071, T072, T073, T074, T075, T076, T077, T078 | Descoberta e analyze real por adaptador | PR-C; ReaderBoundary preservado | T075; evidence/PR-D-validation.md | T076/T077; evidence/PR-D-review.md | US5 completo; aprovação humana PENDENTE |
| PR-E | T079, T080, T081, T082, T083, T084, T085, T086, T087, T088, T089, T090, T091, T092, T093, T094 | Save/histórico e fluxo final | PR-B/PR-C/PR-D | T090; evidence/PR-E-validation.md | T091/T092; evidence/PR-E-review.md | US6 + feature; aprovação humana PENDENTE |

## Rastreabilidade de critérios e cenários

A coluna de implementação refere-se a código futuro do SpecAudit, nunca a código da solução auditada. Validação é task futura, não cenário executado nesta geração.

| Critério | Cenário | Tasks de entrega/proteção | Task de validação da história |
|---|---|---|---|
| US1-AC01 | US1-S01 | T009, T011 | T013 |
| US1-AC02 | US1-S02 | T009, T010, T011 | T013 |
| US1-AC03 | US1-S03 | T011 | T013 |
| US1-AC04 | US1-S04 | T012 | T013 |
| US2-AC01 | US2-S01 | T023, T024 | T025 |
| US2-AC02 | US2-S02 | T022, T024 | T025 |
| US2-AC03 | US2-S03 | T022, T023, T024 | T025 |
| US2-AC04 | US2-S04 | T022, T023, T024 | T025 |
| US3-AC01 | US3-S01 | T030, T031 | T032 |
| US3-AC02 | US3-S02 | T030, T031 | T032 |
| US3-AC03 | US3-S03 | T030, T031 | T032 |
| US3-AC04 | US3-S04 | T030, T031 | T032 |
| US4-AC01 | US4-S01 | T037, T038 | T039 |
| US4-AC02 | US4-S02 | T037, T038 | T039 |
| US4-AC03 | US4-S03 | T037, T038 | T039 |
| US4-AC04 | US4-S04 | T037, T038 | T039 |
| US5-AC01 | US5-S01 | T064, T067, T070 | T071 |
| US5-AC02 | US5-S02 | T066, T067, T069, T070 | T071 |
| US5-AC03 | US5-S03 | T054, T058, T067, T070 | T071 |
| US5-AC04 | US5-S04 | T065, T066, T068, T069 | T071 |
| US5-AC05 | US5-S05 | T054, T057, T066, T069 | T071 |
| US5-AC06 | US5-S06 | T055, T056, T057, T058 | T071 |
| US5-AC07 | US5-S07 | T056, T057, T058 | T071 |
| US6-AC01 | US6-S01 | T080, T081, T082, T084 | T085 |
| US6-AC02 | US6-S02 | T081, T082, T084 | T085 |
| US6-AC03 | US6-S03 | T080, T084 | T085 |
| US6-AC04 | US6-S04 | T083, T084 | T085 |
| US8-AC01 | US8-S01 | T044 | T045 |
| US8-AC02 | US8-S02 | T044 | T045 |

## Dependências e paralelismo

Ordem de histórias: fundação → US1/PR-A → US2 + US3 → US4 → US8/PR-B → US5 gate/PR-C → US5 agentes/PR-D → US6/PR-E → fechamento. US2/US3 podem ser desenvolvidas em paralelo apenas quando seus pré-requisitos técnicos já estiverem concluídos; execução numérica é uma ordem topológica segura. Nenhuma verificação de uma task exige uma task futura.

Exemplos concretos de [P] (capacidade, não ordem para iniciar agentes agora):
- T021 e T029: após PR-A, arquivos de teste distintos, sem dependência entre si.
- T068 e T065: depois de T064, adaptadores distintos e ainda desabilitados; provas/habilitações continuam separadas.
- US1: não há [P] seguro entre broker/prova/habilitação; executar dependências em ordem.
- US4: trace precede ficha; não paralelizar arquivos compartilhados de modelo.
- US8: testes precedem shell; não paralelizar MainLayout/app.css com outra edição desses arquivos.
- US6: writer/prova/habilitação são sequenciais; classificação pode ser refinada só sem conflitar com modelos compartilhados.

## Estratégia de entrega e checkpoints

MVP técnico sugerido: fundação + US1/PR-A (carregar/preservar), ainda sem análise/Save inseguros. Próximo incremento utilizável: US2/US3/US4/US8 no PR-B para consultar documentos e progresso; depois gate offline antes de analyze. V1 completa exige os dois agentes protegidos e Save/histórico, com todas as provas aplicáveis.

Cada história termina com validação real, review/correções, roteiro IA/DEV e estado técnico. PR-C é parcial de US5, não recebe aceite de história completo. O fluxo integrado é consolidado em T089 e no ciclo PR-E, sem criar task de aprovação humana. Mocks e checks de geração não substituem esses passos.

## Caminhos de saída e condição de saída da etapa tasks

Este arquivo e verification/tasks-stage.json identificam a revisão por SHA256. verification/tasks-review.md e verification/tasks-validation.json registram inspeção documental/estrutural, não execução de produto. Aplicar PASSAGENS.md antes de declarar READY e antes de implementar: spec/plan atuais, origem/deps/contratos por task, grafo acíclico, cobertura dos 29 critérios, PRs resolvidos e análise de consistência sem impeditivos. Hooks pré/pós: conferir extensions.yml; ausente nesta geração, sem obrigação adicional identificada.

READY de tasks significa lista pronta para uma futura solicitação de implementação; não executa tarefa, não aprova proteção, PR ou história. Se testes/provas não puderem ocorrer no futuro, registrar NÃO EXECUTADO/causa/impacto/próximo passo e rascunho de impossibilidade para o DEV; somente ele confirma e decide. Nenhuma instalação, elevação, commit/push ou alteração de solução auditada é autorizada automaticamente por concluir a geração.

## Migração autorizada para constituição 4.0.0 (PR-A)

IDs preservados, com descrições/contratos redefinidos pela emenda explícita. Executar T095–T098 como migração antes de concluir T006–T010 e iniciar T011. As versões anteriores estão no arquivo histórico.

- [x] T095 Sincronizar constituição 4.0.0, spec, plan e contratos para isolamento flexível, preservando somente leitura e dois comandos; registrar evidence/isolation-v4-amendment.md.
- [x] T096 Arquivar código/testes experimentais de isolamento e substituir sua execução por TrustedCommandRunner em SpecAudit/Infrastructure/Execution/TrustedCommandRunner.cs, sem tokens, memória de processos ou worker nativo.
- [x] T097 Atualizar testes e registro de capacidades para o contrato v4 em tests/SpecAudit.Windows.Tests/ e CapabilityRegistry.cs, preservando a evidência histórica SEC-001 sem contá-la como teste aprovado.
- [x] T098 Executar análise de consistência e regressão da migração v4, registrar evidence/isolation-v4-revalidation.md e atualizar verification/implementation-stage.json antes de retomar T011.

| ID | Origem | Depende de | Entrega | Como verificar | Resultado esperado |
|---|---|---|---|---|---|
| T095 | constituição 4.0.0; plan | T005 | constitution.md; spec.md; plan.md; contracts/verification.md | Conferir autorização, versão, escopo documental, limitações e remoção das exigências ativas de sandbox. | Artefatos consistentes; falhas anteriores continuam históricas. |
| T096 | constituição 4.0.0; plan | T095 | verification/archive/constitution-v3-isolation/; TrustedCommandRunner.cs | Build sem código experimental ativo; executor gerenciado usa somente operações fechadas e cwd externo. | Nenhum experimento nativo executado automaticamente. |
| T097 | constituição 4.0.0; plan | T096 | tests/SpecAudit.Windows.Tests/; CapabilityRegistry.cs | Regressão de broker real e executor gerenciado; conferir lista dos testes retirados por emenda. | Cobertura dos controles atuais; retirada de escopo explicitamente rastreada. |
| T098 | constituição 4.0.0; plan | T097 | evidence/isolation-v4-revalidation.md; implementation-stage.json | Conferir contratos/código/testes, saídas reais e revisão atual; aprovação humana e histórias continuam pendentes. | Migração funcional validada, sem alegar correção da antiga garantia OS. |

T011 exige T098 (migração validada) e T010 (prova de aplicação v4). T095–T098 pertencem ao PR-A; não há dependência do antigo sandbox OS. Histórias e aceite humano preservados.

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
