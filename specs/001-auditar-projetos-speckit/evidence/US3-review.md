# Code review — US3 (T033)

Estado do review: **BLOQUEADA para aprovação normal — ALTERAÇÕES SOLICITADAS** (2 achados P2 abertos para T034; 1 achado P3 registrado).
Execução da task T033: **CONCLUÍDA**; executar a revisão e aprovar a implementação são estados distintos.
Modalidade: **AUTORREVISAO**, Antigravity (Gemini 4 Neon); nenhuma revisão independente ou humana atribuída.
Data/hora e fuso: 2026-11-19 23:20 America/Sao_Paulo.
Versão revisada: código em `517aea5e4547c02e69dcd12221d779b156d0c379` (validação T032 em `4fe3dde`).
PR previsto: PR-B (base do incremento US3 em `d2a34bc`; sem PR aberto ou integrado).

## Pré-condições e conformidade

- Checklist `checklists/requirements.md`: 19/19, zero pendentes.
- `.specify/extensions.yml`: ausente, sem hooks registrados.
- Dependências: nenhuma dependência NuGet ou npm adicionada na US3 (`git diff --name-only d2a34bc..517aea5 -- *.csproj *packages.lock.json package*.json` vazio).
- Evidências anteriores conferidas: [T029-dashboard-tests.md](T029-dashboard-tests.md) e [US3-validation.md](US3-validation.md) (5/5 domínio, 4/4 US3 E2E, 84/84 regressão).

## Análise dos critérios

| Critério | Código relevante | Evidência consultada | Conclusão e limites |
|---|---|---|---|
| US3-AC01 | `TaskDeclaration.cs:1-16`; `DashboardProjection.cs:1-102`; `DocumentaryDashboardService.cs:145-189`; `Dashboard.razor:104-152` | `RUN-01` (`S01`) e `RUN-02` (`S01`) + capturas `captures/T032/*` | `75%` (`30/40`), `60%` (`12/20`), `Sem itens` em `003-empty`, duplicidades mantidas no denominador (`11/21`) e rótulos V1 conferidos. |
| US3-AC02 | `DocumentaryDashboardService.cs:294-396`; `Dashboard.razor:138-151,244-273` | `RUN-01` (`S02`) e `RUN-02` (`S02`) | Estados e gates registrados exibem fonte, revisão, atualidade e aviso explícito de que `READY` não comprova funcionamento/teste/aceite humano. |
| US3-AC03 | `DocumentaryDashboardService.cs:64-143,343-373,418-433`; `Dashboard.razor:154-240` | `RUN-01` (`S03`, observador do broker) e `RUN-02` (`S03`) | Impedimentos, lacunas, duplicidades e referências a `.cs` (`não verificada nesta versão`) exibidos sem abrir código. Tabela de requisitos trunca silenciosamente em 25 itens (**REV-US3-01**). |
| US3-AC04 | `Dashboard.razor:55-102,283-366`; `DocumentaryDashboardService.cs:145-210` | `RUN-01` (`S04`) e `RUN-02` (`S04`) | Filtros e atualização explícita sem executar agente/gate funcionam, mas há dois defeitos P2: filtro invisível ao trocar de funcionalidade (**REV-US3-02**) e projeção zerada para `Sem itens` durante `HasPendingChanges` (**REV-US3-03**). |

## Achados confirmados

### REV-US3-03 — P2 — Alteração externa zera a projeção exibida para `Sem itens` em vez de manter os indicadores anteriores marcados como `Desatualizado`

- **Local**: `SpecAudit/Components/Pages/Dashboard.razor:298-317` e `SpecAudit/Application/DocumentaryDashboardService.cs:192-210`.
- **Descrição**: Quando ocorre alteração externa em um documento monitorado (`active.Catalog.HasPendingChanges == true`), `OnProjectsChanged` chama `SyncAndRebuild()` → `DashboardService.Build(...)`. Como o `DocumentBroker` recusa leituras com `DocumentAccessException("Changed")` enquanto há mudanças pendentes, `TryReadUtf8` retorna `null` para todos os arquivos, reconstruindo uma `DashboardProjection` com `0 / 0` (`Sem itens`) em vez de preservar a última projeção válida marcada com `Freshness.Stale` (`Desatualizado`) até o usuário clicar em `Atualizar documentos`.
- **Prioridade e impacto**: **P2 (impeditivo para fechamento normal)**. Distorce os indicadores visíveis durante o estado `Stale` (`US3-AC04` / `data-model.md`).
- **Correção proposta para T034**: Em `Dashboard.razor`, quando `active.Catalog.HasPendingChanges` for verdadeiro e já existir uma `projection` da mesma sessão/projeto, preservar `projection` marcando `Freshness = Freshness.Stale` (e `RecordedGateStates` com `Freshness = Freshness.Stale`) em vez de substituí-la por uma projeção vazia; após `Refresh()`, recalcular normalmente com `Freshness.Current`. Cobrir com asserção em `DashboardTests.S04` verificando que `30 / 40` e `Desatualizado` continuam visíveis antes do clique em `Atualizar documentos`.
- **Situação**: **ABERTO** (correção e revalidação na T034).

### REV-US3-02 — P2 — Trocar `Funcionalidade` mantém `Fase` e `História` da funcionalidade anterior como filtros invisíveis

- **Local**: `SpecAudit/Components/Pages/Dashboard.razor:320-324`.
- **Descrição**: `OnFeatureChanged` atualiza apenas `selectedFeatureId` sem limpar `selectedPhase` e `selectedStory`. Se o usuário filtrar por uma fase/história de `001-demo` e trocar para outra funcionalidade sem aquela fase/história, o filtro continua ativo em memória apesar de não constar mais nas opções do `<select>`, podendo zerar indevidamente os indicadores da nova funcionalidade.
- **Prioridade e impacto**: **P2 (impeditivo para fechamento normal)**. Viola previsibilidade e integridade dos filtros de `US3-AC04`.
- **Correção proposta para T034**: Limpar `selectedPhase` e `selectedStory` em `OnFeatureChanged` (ou validar que ainda pertencem às listas disponíveis) e adicionar asserção de regressão em `DashboardTests.S04` filtrando por `Gravidade`/`Fase` e alternando de funcionalidade.
- **Situação**: **ABERTO** (correção e revalidação na T034).

### REV-US3-01 — P3 — Tabela `Requisitos no escopo` trunca silenciosamente em 25 linhas

- **Local**: `SpecAudit/Components/Pages/Dashboard.razor:191`.
- **Descrição**: `@foreach (var req in projection.Requirements.Take(25))` oculta silenciosamente requisitos além do 25º sem aviso ao usuário.
- **Prioridade e impacto**: **P3 (não impeditivo)**. Pode ocultar requisitos em escopos maiores (ex.: `Todas as funcionalidades`).
- **Correção proposta para T034**: Remover o `.Take(25)` para exibir todos os requisitos do escopo selecionado (ou oferecer paginação/expansão explícita).
- **Situação**: **ABERTO** (pode ser resolvido junto na T034).

## Segurança, contratos e limites

- **Somente leitura e ausência de código**: `DocumentaryDashboardService` lê exclusivamente via `DocumentBroker`; `DoNotRead.cs` não é aberto (comprovado em `S03` via `AccessObserved`); nenhuma entidade proibida na V1 (`SourceCodeAnalysis`, `Compilation`, `ImplementationScore`, `DependencyInventory`) existe no assembly.
- **Execução automática**: nenhuma chamada a agente ou `check_gate` existe na US3; atualização documental é local e explícita.
- **Limites desta revisão**: autorrevisão técnica; tema claro pendente da US8 (T044); aceite humano pendente do DEV (T035).

## Conclusão e passagem

Passagem de revisão **T033** concluída com 2 achados **P2** (`REV-US3-02`, `REV-US3-03`) e 1 achado **P3** (`REV-US3-01`) abertos. O fechamento normal da US3 (T035) fica bloqueado até a resolução e revalidação na **T034**. Nenhum código de produção ou teste foi alterado nesta task.

