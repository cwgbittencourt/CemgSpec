# Plano Técnico: Visão do Analista de Negócio (Dashboard BA)

- **Feature**: `002-visao-analista-negocio`
- **Base técnica**: .NET 10 / C# Blazor Interactive Server (`SpecAudit`)
- **Referência visual**: `docs/mockups/dashboard-visao-negocio-ba.jpg`

---

## Arquitetura e Componentes

### 1. Domínio (`SpecAudit/Domain/BusinessDashboardProjection.cs`)
- `UserStoryStatusRow(string StoryId, string Title, bool IsDeferred, int CompletedTasks, int TotalTasks, string DevValidationState, string PoAcceptanceState, Guid? ValidationDocId, Guid? AcceptanceDocId)`
- `EvolutionDelta(string BaselineLabel, double PreviousProgressPct, double CurrentProgressPct, int PreviousBlockers, int CurrentBlockers, int PreviousReadyGates, int CurrentReadyGates)`
- `BusinessDashboardProjection(int ReadyStories, int ActiveStories, int PendingDevCount, int PendingPoCount, IReadOnlyList<UserStoryStatusRow> Stories, EvolutionDelta? Evolution)`

### 2. Aplicação (`SpecAudit/Application/BusinessDashboardService.cs`)
- Lê exclusivamente via `DocumentBroker` (com cache por `(ProjectId, Catalog.Generation)`):
  - `spec.md`: extrai histórias `USx`, títulos e marcação `ADIADA`.
  - `tasks.md`: calcula `CompletedTasks / TotalTasks` por `USx` (incluindo herança do título de fase `## Fase N — USx`).
  - `validation/USx-dev.md` e `acceptance/USx.md`: identifica se a decisão humana está `Pendente` ou `Registrada` (nunca marca aprovação automaticamente).
  - `SavedHistoryService`: compara o pacote salvo válido mais recente no destino externo contra o estado atual para compor `EvolutionDelta`.

### 3. Exportação Executiva (`SpecAudit/Infrastructure/Reports/ExecutiveSummarySerializer.cs`)
- Gera HTML autocontido (limpo, pronto para impressão/PDF via navegador) contendo status geral, tabela de histórias, escopo entregue/adiado, impedimentos ativos e próximas ações humanas.
- Grava exclusivamente no destino externo via `ExternalReportWriter` (revalidando handle/identidade Windows e proibindo escrita na origem).

### 4. Interface (`SpecAudit/Components/Pages/Dashboard.razor` + `SpecAudit/Components/Dashboard/`)
- Controle segmentado no cabeçalho: `Visão Técnica` | `Visão de Negócio (BA)` + botão `? Glossário`.
- Novo componente `BusinessDashboardView.razor` (3 KPIs gerenciais, tabela `Entregas por História de Usuário`, card `Evolução (Antes × Agora)` e card `Resumo Executivo` com `Exportar Resumo Executivo`).
- Novo componente `GlossaryDialog.razor` (modal acessível com explicações objetivas para Analistas de Negócio).
- Scroll independente por tópico (`.topic-scroll`) preservado nos dois temas (Escuro e Claro).

---

## Estratégia de Testes

- **Unitários (`SpecAudit.Tests/BusinessDashboardServiceTests.cs`)**: agregação por `USx`, detecção de histórias adiadas, leitura de status `Pendente` em `validation/` e `acceptance/`, cálculo de `EvolutionDelta` e geração HTML do `ExecutiveSummarySerializer`.
- **UI (`SpecAudit.Ui.Tests/BusinessDashboardTests.cs`)**: alternância `Visão Técnica` ↔ `Visão de Negócio (BA)`, abertura do `? Glossário`, conferência dos 3 KPIs e tabela `US1–US8`, e exportação explícita do Resumo Executivo no destino externo com origem intacta.
