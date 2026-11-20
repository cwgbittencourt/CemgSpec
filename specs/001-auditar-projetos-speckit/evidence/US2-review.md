# Code review — US2 (T026)

Estado do review: **BLOQUEADA para aprovação normal — ALTERAÇÕES SOLICITADAS** (1 achado P2 aberto para T027; 1 achado P3 registrado).
Execução da task T026: **CONCLUÍDA**; executar a revisão e aprovar a implementação são estados distintos.
Modalidade: **AUTORREVISAO**, Antigravity (Gemini 4 Neon); nenhuma revisão independente ou humana atribuída.
Data/hora e fuso: 2026-11-19 22:32 America/Sao_Paulo.
Versão revisada: código em `14865b7fb8aba1e95792ddca52d4286ef30edd38` (validação T025 em `656cb19`).
PR previsto: PR-B (base do incremento US2 em `ddf8f31` / `73e06b9`; sem PR aberto ou integrado).

## Pré-condições e conformidade

- Checklist `checklists/requirements.md`: 19/19, zero pendentes; nenhuma alteração nos marcadores.
- `.specify/extensions.yml`: ausente, sem hooks registrados.
- Dependências: nenhuma dependência NuGet ou npm adicionada após a auditoria da T022 (`git diff 3803ffb..656cb19 -- *.csproj *packages.lock.json package*.json` vazio).
- Evidências anteriores conferidas: [T021-preview-tests.md](T021-preview-tests.md), [T022-local-preview.md](T022-local-preview.md), [preview-safety.md](preview-safety.md) e [US2-validation.md](US2-validation.md) (4/4 domínio, 15/15 US2 E2E/assets, 75/75 regressão).

## Análise dos critérios

| Critério | Código relevante | Evidência consultada | Conclusão e limites |
|---|---|---|---|
| US2-AC01 | `DocumentTree.cs:20-87`; `DocumentTreeNode.razor:2-39`; `Documents.razor:49-71,277-325`; `DocumentPreviewSession.cs:51-74,109-147` | `RUN-01` (`DocumentNavigationTests`) e `RUN-02` (`S01`) | Árvore e busca cobrem apenas o catálogo; código e caminhos fora da origem recusados; âncora local/cruzada, ausência e fora do catálogo com estados distintos. |
| US2-AC02 | `MarkdownRenderer.cs:48-243`; `Documents.razor:74-127`; `MarkdownPreview.razor:1-128`; `MarkdownPreview.razor.css:1-121` | `RUN-01`, `RUN-02` (`S02`) e capturas `captures/T025/*` | Preview, Fonte e Lado a lado somente leitura; títulos, listas, tarefas desabilitadas, tabelas, citações, imagem local e destaque de código conferidos. Remontagem ao voltar de `Fonte` gera **REV-US2-01**. |
| US2-AC03 | `wwwroot/lib/preview/preview.js:21-223`; `MarkdownPreview.razor:29-46` | `RUN-02` (`S03` e `PreviewAssetsTests`) | Mermaid renderizado localmente em iframe `sandbox="allow-scripts"` com CSP `default-src 'none'`, zoom e fonte; Mermaid inválido, PlantUML e Graphviz exibem fonte e explicação. |
| US2-AC04 | `MarkdownRenderer.cs:57-242`; `RemotePreviewImages.cs:7-102`; `DocumentPreviewSession.cs:76-91`; `MarkdownPreview.razor:93-114`; `MarkdownPreview.razor.js:12-71` | `RUN-01`, `RUN-02` (`S04` e `PreviewAssetsTests`) e `preview-safety.md` | HTML ativo removido; scripts/formulários inertes; rede automática zero; imagem remota mediada pelo servidor sem cookies/auth/referer/redirecionamento e apenas em IPs públicos; link externo abre em nova aba com `noopener,noreferrer` somente após clique. |

## Achados confirmados

### REV-US2-01 — P2 — Remontagem de `MarkdownPreview` ao sair do modo `Fonte` acumula listeners e descarta imagens remotas já autorizadas

- **Local**: `SpecAudit/Components/Documents/MarkdownPreview.razor:30-35` e `SpecAudit/Components/Documents/MarkdownPreview.razor.js:38-39`.
- **Descrição**: Quando o usuário alterna `Preview` → `Fonte` → `Lado a lado` (ou `Preview`) no mesmo documento, `wasHidden && !Hidden` força nova chamada a `mount(container, ...)` sobre o mesmo elemento `container` (`@key="Session"`). Isso produz dois efeitos concretos:
  1. `container.addEventListener("click", activate)` e `container.addEventListener("keydown", activate)` são registrados novamente sem remover os listeners anteriores. A cada ciclo `Fonte` → `Preview`/`Lado a lado`, cliques em links disparam múltiplas chamadas SignalR `ActivateReference` (as antigas são ignoradas pelo `mountId != mount`, mas acumulam tráfego e closures no DOM).
  2. `container.innerHTML = html` sobrescreve o DOM do preview e substitui qualquer imagem remota já autorizada nessa visualização pelo placeholder `Carregar nesta sessão`, violando a preservação de conteúdo na troca de modo (`US2-AC02`).
- **Prioridade e impacto**: **P2 (impeditivo para fechamento normal)**. Não compromete segurança nem grava na origem, mas degrada a troca de modo no mesmo documento e viola a preservação de estado da visualização.
- **Correção proposta para T027**:
  1. Armazenar `AbortController` por `container` (via `WeakMap`) em `MarkdownPreview.razor.js` e abortar os listeners anteriores antes de registrar novos em `mount()`, ou usar um único par de listeners por elemento.
  2. Reter em `MarkdownPreview.razor` o dicionário de `dataUri` já autorizados por índice de referência para a `Session` corrente e reaplicá-los automaticamente durante `mount()` quando o painel volta a ficar visível, sem nova requisição de rede.
  3. Adicionar cobertura em teste UI verificando que alternar `Preview` → `Fonte` → `Lado a lado` não duplica ativações nem perde o estado do preview.
- **Situação**: **ABERTO** (correção e revalidação na T027).

### REV-US2-02 — P3 — Link `Projetos` na sidebar não fica ativo na rota raiz `/`

- **Local**: `SpecAudit/Components/Layout/MainLayout.razor:6`.
- **Descrição**: `Projects.razor` responde tanto a `/` quanto a `/projetos`, mas o `NavLink` na sidebar usa `href="/projetos" Match="NavLinkMatch.All"`. Ao abrir a raiz `/`, nenhum item da sidebar recebe a classe `.active`.
- **Prioridade e impacto**: **P3 (não impeditivo)**. Apenas detalhe visual no acesso inicial pela raiz `/`; navegabilidade e acessibilidade funcionam normalmente.
- **Correção proposta para T027**: Marcar o item `Projetos` como ativo quando a rota atual for `/` ou `/projetos` (ou ajustar `NavigationManager` / `MainLayout.razor`).
- **Situação**: **ABERTO** (pode ser resolvido junto na T027).

## Segurança, contratos e limites

- **Somente leitura e catálogo**: `Documents.razor` e `MarkdownPreview.razor` só acessam arquivos via `ProjectStore.ReadDocument` e `ProjectStore.OpenPreview` (handles Windows somente leitura do `DocumentBroker`). Nenhuma API aceita caminho livre; `PresentationAsset` não aparece na árvore; nenhum arquivo de código é aberto.
- **Conteúdo ativo e rede**: sanitização por lista branca em `MarkdownRenderer`, iframe opaco `sandbox="allow-scripts"` + CSP estrita em `preview.js`, limpeza de SVG pós-Mermaid e transporte server-side `RemotePreviewImages` conferidos.
- **Visual e regras V1**: capturas de [T025](captures/T025/documents-preview.png) conferidas contra [documentos-sugestoes.png](../design/mockups/documentos-sugestoes.png) e [README.md](../design/mockups/README.md). Nenhuma funcionalidade histórica descartada (edição, análise de código, `npm test`, salvamento automático) foi introduzida.
- **Limites desta revisão**: autorrevisão técnica (não substitui revisão independente ou humana de PR); tema claro permanece pendente da US8 (T044); aceite humano da US2 permanece pendente do DEV (T028).

## Conclusão e passagem

Passagem de revisão **T026** concluída com 1 achado **P2** (`REV-US2-01`) e 1 achado **P3** (`REV-US2-02`) abertos. O fechamento normal da US2 (T028) fica bloqueado até a resolução e revalidação na **T027**. Nenhum código de produção ou teste foi alterado nesta task.

