# Tasks: Visão do Analista de Negócio (Dashboard BA)

- **Feature**: `002-visao-analista-negocio`
- **Regra obrigatória de rastreabilidade**: Toda tarefa de entrega cita explicitamente os códigos de requisito (`FR-xxx`) na própria linha do checkbox para garantir 100% de Cobertura Documental rastreável no CemgSpec.

---

## Fase 1 — Preparação e Contratos

- [x] T001 Criar contratos de teste unitário em `tests/CemgSpec.Tests/BusinessDashboardServiceTests.cs` e de UI em `tests/CemgSpec.Ui.Tests/BusinessDashboardTests.cs` cobrindo `FR-001` a `FR-006`.

---

## Fase 2 — US1: Visão por História de Usuário e Pendências Humanas (P1)

- [x] T002 [US1] (FR-002, FR-003) Criar modelos de domínio `UserStoryStatusRow`, `EvolutionDelta` e `BusinessDashboardProjection` em `CemgSpec/Domain/BusinessDashboardProjection.cs`.
- [x] T003 [US1] (FR-002, FR-003) Implementar `BusinessDashboardService` em `CemgSpec/Application/BusinessDashboardService.cs` lendo `spec.md`, `tasks.md`, `validation/*-dev.md` e `acceptance/*.md` via `DocumentBroker` com cache por geração e registro em `CemgSpec/Program.cs`.
- [x] T004 [US1] (FR-001, FR-002, FR-003) Implementar alternância `Visão Técnica` / `Visão de Negócio (BA)` em `CemgSpec/Components/Pages/Dashboard.razor` e criar `CemgSpec/Components/Dashboard/BusinessDashboardView.razor` (3 KPIs gerenciais + tabela `Entregas por História de Usuário` com scroll independente e links diretos para `/documentos`).

---

## Fase 3 — US2: Evolução entre Auditorias Salvas (Antes × Agora) (P2)

- [x] T005 [US2] (FR-004) Integrar `SavedHistoryService` em `CemgSpec/Application/BusinessDashboardService.cs` para calcular `EvolutionDelta` (delta de progresso, impedimentos e gates `READY` entre pacote salvo válido e estado atual, sem inferir resolução em amostra incompleta).
- [x] T006 [US2] (FR-004) Adicionar o card **Evolução (Antes × Agora)** com seletor de pacote salvo em `CemgSpec/Components/Dashboard/BusinessDashboardView.razor`.

---

## Fase 4 — US3: Glossário Contextual e Resumo Executivo Exportável (P2)

- [x] T007 [US3] (FR-005) Criar o componente modal `CemgSpec/Components/Dashboard/GlossaryDialog.razor` e conectar o botão `? Glossário` no cabeçalho de `CemgSpec/Components/Pages/Dashboard.razor`.
- [x] T008 [US3] (FR-006) Implementar `CemgSpec/Infrastructure/Reports/ExecutiveSummarySerializer.cs` (HTML autocontido pronto para impressão/PDF) e integrar exportação explícita no destino externo via `ExternalReportWriter` a partir do botão **Exportar Resumo Executivo** em `BusinessDashboardView.razor`.

---

## Fase 5 — Regressão e Fechamento

- [x] T009 Executar `BusinessDashboardServiceTests`, `BusinessDashboardTests` e regressão completa (`CemgSpec.Tests`, `CemgSpec.Windows.Tests`, `CemgSpec.Ui.Tests`), registrando evidências em `specs/002-visao-analista-negocio/evidence/feature-validation.md`.
