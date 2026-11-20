# PR-B — Leitor documental, Dashboard, Ficha do requisito e Navegação/Temas (US2, US3, US4, US8)

- **Estado de aprovação humana**: **PENDENTE** (este arquivo prepara a descrição revisável do PR; não abre, publica ou integra PR).
- **Predecessor**: PR-A ([PR-A.md](PR-A.md), base `ddf8f31` / `73e06b9`, aprovação humana pendente).
- **Revisão candidata**: `ac53fd27d52ddc0b9cd4ab4fbc7c2e02462eae53`.
- **Tasks cobertas**: T021–T052 (US2, US3, US4 e US8).

## Escopo final entregue

1. **US2 — Documentos e preview Markdown seguro** (`Documents.razor`, `MarkdownPreview.razor`, `MarkdownRenderer.cs`, `DocumentPreviewSession.cs`, `RemotePreviewImages.cs`, bundles locais `preview.js`/`mermaid.js`/`highlight.js`): árvore do catálogo, busca por caminho/conteúdo, âncoras, modos Preview/Fonte/Lado a lado somente leitura, Mermaid offline em iframe opaco com zoom/fonte, sanitização estrita, imagem remota sob clique sem credenciais herdadas e link externo em nova aba.
2. **US3 — Dashboard documental** (`TaskDeclaration.cs`, `DashboardProjection.cs`, `DocumentaryDashboardService.cs`, `Dashboard.razor`): progresso declarado (`[x]`/`total`), cobertura documental (`vínculos explícitos`/`total`), `Sem itens` em escopo vazio, gates registrados com procedência, ocorrências documentais sem leitura de código, filtros e preservação `Desatualizado` até `Atualizar documentos`.
3. **US4 — Ficha documental do requisito** (`Requirement.cs`, `EvidenceLink.cs`, `Finding.cs`, `RequirementTraceService.cs`, `Requirements.razor`, `RequirementSheet.razor`): chaves contextuais sem mesclagem entre funcionalidades, `Explícito` vs `Inferido`, `CoverageState` documental (`Linked`/`MissingLink`/`Ambiguous`), declarações de terceiros e referências a código não verificadas.
4. **US8 — Navegação das 5 áreas e Temas escuro/claro** (`MainLayout.razor`, `app.css`): 5 áreas + `Projetos` na sidebar, tema escuro inicial e claro opcional na sessão preservando seleção/estados, e placeholders indisponíveis para `Verificações` e `Sugestões`.
5. **Compatibilidade de transporte local** (`Program.cs`, `LocalSessionHttpTests.cs`): liberação dos endpoints internos de transporte fallback SignalR (`/_blazor`, `/_blazor/negotiate`, `/_blazor/disconnect`) da validação de antiforgery de formulário HTML, preservando IP loopback, `Host`/`Origin` exatos e cookie de sessão `HttpOnly; SameSite=Strict`.

## Evidências e roteiros

- Validação do incremento: [PR-B-validation.md](../../evidence/PR-B-validation.md) (**95/95 testes aprovados**, exit code 0).
- Review do incremento: [PR-B-review.md](../../evidence/PR-B-review.md) (autorrevisão técnica sem impeditivos abertos; revisão independente e aprovação humana pendentes).
- Roteiros reproduzíveis do DEV e estados técnicos (decisão do DEV pendente em todas as histórias):
  - US2: [US2-dev.md](../../validation/US2-dev.md) · [acceptance/US2.md](../../acceptance/US2.md)
  - US3: [US3-dev.md](../../validation/US3-dev.md) · [acceptance/US3.md](../../acceptance/US3.md)
  - US4: [US4-dev.md](../../validation/US4-dev.md) · [acceptance/US4.md](../../acceptance/US4.md)
  - US8: [US8-dev.md](../../validation/US8-dev.md) · [acceptance/US8.md](../../acceptance/US8.md)

## Decisão humana (reservada ao revisor/DEV)

- Aprovação de PR: PENDENTE
- Integração: PENDENTE
- Aceite funcional das histórias (US2, US3, US4, US8): PENDENTE
