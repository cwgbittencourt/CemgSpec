# Code review — US4 (T040)

Estado do review: **BLOQUEADA para aprovação normal — ALTERAÇÕES SOLICITADAS** (1 achado P2 aberto para T041; 1 achado P3 registrado).
Execução da task T040: **CONCLUÍDA**; executar a revisão e aprovar a implementação são estados distintos.
Modalidade: **AUTORREVISAO**, Antigravity (Gemini 4 Neon); nenhuma revisão independente ou humana atribuída.
Data/hora e fuso: 2026-11-20 08:28 America/Sao_Paulo.
Versão revisada: código em `fa38766474cbe338ffabc2bc8d53fc3c832cf440` (validação T039 em `4d6e587`).
PR previsto: PR-B (base do incremento US4 em `32170ad`; sem PR aberto ou integrado).

## Pré-condições e conformidade

- Checklist `checklists/requirements.md`: 19/19, zero pendentes.
- `.specify/extensions.yml`: ausente, sem hooks registrados.
- Dependências: nenhuma dependência NuGet ou npm adicionada na US4 (`git diff --name-only 32170ad..fa38766 -- *.csproj *packages.lock.json package*.json` vazio).
- Evidências anteriores conferidas: [T036-requirement-tests.md](T036-requirement-tests.md) e [US4-validation.md](US4-validation.md) (4/4 domínio, 4/4 US4 E2E, 93/93 regressão).

## Análise dos critérios

| Critério | Código relevante | Evidência consultada | Conclusão e limites |
|---|---|---|---|
| US4-AC01 | `Requirement.cs:1-32`; `EvidenceLink.cs:1-27`; `RequirementTraceService.cs:35-190`; `Requirements.razor:1-196`; `RequirementSheet.razor:1-302` | `RUN-01` (`S01`) e `RUN-02` (`S01`) + capturas `captures/T039/*` | Ficha em memória, separação de `FR-001` entre `001-demo` e `002-other`, `Explícito` vs `Inferido`, abas e substituições V1 conferidos. |
| US4-AC02 | `Requirement.cs:9-14`; `RequirementTraceService.cs:126-170`; `RequirementSheet.razor:86-112,213-224` | `RUN-01` (`S02`) e `RUN-02` (`S02`) | `CoverageState` restrito a `Linked`/`MissingLink`/`Ambiguous`; `Evidência documental insuficiente` sem `Não implementado`; duplicidades sem mesclar. Pequeno detalhe em duplicidades na troca de aba (**REV-US4-02**). |
| US4-AC03 | `RequirementTraceService.cs:258-291`; `RequirementSheet.razor:116-143` | `RUN-01` (`S03`) e `RUN-02` (`S03`) | Declarações registradas exibidas como `registro de terceiros` com fonte, revisão e aviso de que o SpecAudit não aprova. |
| US4-AC04 | `RequirementTraceService.cs:192-210`; `RequirementSheet.razor:61-82,150-159` | `RUN-01` (`S04`, observador do broker) e `RUN-02` (`S04`) | Referências a `.cs` permanecem `OutOfScope` (`Não verificada (fora do catálogo)`) sem leitura do arquivo-fonte e com nota de `capacidade futura`. |

## Achados confirmados

### REV-US4-01 — P2 — Alteração externa detectada pelo broker antes do watcher zera a lista de requisitos em vez de preservar como `Desatualizado`

- **Local**: `SpecAudit/Components/Pages/Requirements.razor:150-166`.
- **Descrição**: `Rebuild()` preserva `sheets` como `Freshness.Stale` apenas quando `active.Catalog.HasPendingChanges` já é verdadeiro. Se um documento for alterado no disco e o usuário trocar de funcionalidade ou disparar `Rebuild()` antes da entrega assíncrona do evento do `FileSystemWatcher`, `TraceService.ListSheets(...)` recebe `DocumentAccessException("Changed")` do broker e retorna uma lista vazia, apagando as fichas exibidas até o clique em `Atualizar documentos`. Além disso, a faixa de contexto em `Requirements.razor` não exibe o selo visual de atualidade (`Atual` / `Desatualizado`) quando `Freshness == Stale`.
- **Prioridade e impacto**: **P2 (impeditivo para fechamento normal)**. Mesma classe de corrida já corrigida no Dashboard (`REV-US3-03`); garante consistência visual e preservação do estado documental em alterações externas.
- **Correção proposta para T041**: Exibir selo `Atual` / `Desatualizado` na faixa de contexto de `Requirements.razor` e preservar `sheets` marcando `Freshness.Stale` também quando `TraceService.ListSheets` detectar mudança no broker antes do watcher (na mesma `CatalogGeneration`).
- **Situação**: **ABERTO** (correção e revalidação na T041).

### REV-US4-02 — P3 — Alternar entre definições duplicadas do mesmo ID não redefine a aba ativa em `RequirementSheet`

- **Local**: `SpecAudit/Components/Requirements/RequirementSheet.razor:213-224`.
- **Descrição**: `OnParametersSet` compara `lastKey != Sheet.Requirement.Key`. Quando dois requisitos duplicados na mesma funcionalidade compartilham o mesmo `RequirementKey`, alternar entre eles na lista mantém a aba anterior em vez de retornar para `Resumo`.
- **Prioridade e impacto**: **P3 (não impeditivo)**.
- **Correção proposta para T041**: Comparar `Sheet.Requirement.SourceLocation` (junto com `Key`) em `OnParametersSet`.
- **Situação**: **ABERTO** (pode ser resolvido junto na T041).

## Segurança, contratos e limites

- **Somente leitura e ausência de código**: `RequirementTraceService` lê exclusivamente via `DocumentBroker`; `DoNotRead.cs` não é aberto (comprovado em `S04` via `AccessObserved`).
- **Limites desta revisão**: autorrevisão técnica; tema claro pendente da US8 (T044); aceite humano pendente do DEV (T042).

## Conclusão e passagem

Passagem de revisão **T040** concluída com 1 achado **P2** (`REV-US4-01`) e 1 achado **P3** (`REV-US4-02`) abertos. O fechamento normal da US4 (T042) fica bloqueado até a resolução e revalidação na **T041**. Nenhum código de produção ou teste foi alterado nesta task.

