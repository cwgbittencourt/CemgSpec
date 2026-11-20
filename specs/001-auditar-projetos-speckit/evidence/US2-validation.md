# Evidências de validação — US2

Estado: APROVADA
Executor/ferramenta: Antigravity (Gemini 4 Neon) · dotnet SDK 10.0.404 / runtime .NET 10.0.15 / Playwright 1.64.1 / Microsoft Edge headless
Data/hora e fuso: 2026-11-19 22:25 America/Sao_Paulo
Commit: `14865b7fb8aba1e95792ddca52d4286ef30edd38`
Alterações não commitadas: nenhuma no código/testes; apenas novos artefatos desta evidência (`evidence/US2-validation.md`, `evidence/logs/T025/*`, `evidence/captures/T025/*`) e atualização de `tasks.md`.
Ambiente, versões e dados: Windows x64, host local `SpecAudit` em loopback (`http://127.0.0.1:<porta-efêmera>`), fixtures sintéticas criadas por `DocumentaryFixture.Create()` sob `%LOCALAPPDATA%\Temp\SpecAudit-V1-*` (removidas ao final de cada teste), nenhum projeto real nem acesso à internet.

| Execução | Critério/cenário | Comando ou procedimento e diretório | Esperado | Observado | Resultado | Log/artefato |
|---|---|---|---|---|---|---|
| RUN-01 | US2-AC01 / US2-S01, US2-AC02 / US2-S02, US2-AC04 / US2-S04 (domínio, sanitização e navegação do broker) | Raiz do repositório: `dotnet test tests/SpecAudit.Tests/SpecAudit.Tests.csproj --no-restore --filter "FullyQualifiedName~DocumentNavigationTests\|FullyQualifiedName~MarkdownRendererTests" --logger "console;verbosity=normal"` | Resolução de links/âncoras apenas no catálogo; ausência e fora do catálogo distintos; HTML ativo descartado; UTF-8 inválido tratado | 4/4 aprovados (exit code 0): `S01PreviewReferencesOnlyAuthorizeCataloguedDestinations`, `RendersDocumentStructureAndPreservesMarkdownSource`, `DropsActiveMarkupAndKeepsDestinationsInert`, `ReportsInvalidUtf8` | PASSOU | [run-01-domain.txt](logs/T025/run-01-domain.txt) |
| RUN-02 | US2-S01–S04 / US2-AC01–AC04 (E2E real no host loopback + Edge headless + seleção nativa + transporte controlado de imagem remota) | Raiz do repositório com `$env:TEMP = "$env:LOCALAPPDATA\Temp"` e `$env:SPECAUDIT_CAPTURE_DIR = "...\evidence\captures\T025"`: `dotnet test tests/SpecAudit.Ui.Tests/SpecAudit.Ui.Tests.csproj --no-restore --filter "FullyQualifiedName~MarkdownPreviewTests\|FullyQualifiedName~PreviewAssetsTests" --logger "console;verbosity=normal"` | Árvore, busca, âncoras, links autorizados/ausentes/fora do catálogo; Preview/Fonte/Lado a lado somente leitura; Mermaid offline em iframe opaco com zoom/fonte; fallback de Mermaid inválido/PlantUML/Graphviz; HTML ativo inerte; imagem remota só após clique sem cookies/auth/referer; link externo em nova aba; origem intacta e pasta de relatórios vazia | 15/15 aprovados (exit code 0): `BrowserCanPrepareCorpusWithNativeDialogs`, `S01SearchTreeAndFollowOnlyAuthorisedDocumentLinks`, `S02PreviewSourceAndSplitUseTheSameReadOnlyDocument`, `S03MermaidRendersOfflineAndOtherDiagramsShowSourceAndExplanation`, `S04ActiveContentIsInertAndExternalRequestsRequireAnAction`, `RendersOnlyMermaidWithLocalAssetsAndKeepsFallbackSource` e 9 casos de `RemoteTransportOnlyDialsPublicAddresses` | PASSOU | [run-02-us2-e2e.txt](logs/T025/run-02-us2-e2e.txt) · [Preview](captures/T025/documents-preview.png) · [Fonte](captures/T025/documents-source.png) · [Lado a lado](captures/T025/documents-split.png) |
| RUN-03 | Regressão relacionada (fundação, ReaderBoundary, sessão/antiforgery, US1, US2) | Raiz do repositório com `$env:TEMP = "$env:LOCALAPPDATA\Temp"`: `dotnet test SpecAudit.slnx --no-build --no-restore --filter 'FullyQualifiedName!~RealBrowserRendersLocalProjectSelectionWithoutCircuitErrors&FullyQualifiedName!~US1IntegratedHarnessTests' --logger "console;verbosity=normal"` | Nenhuma regressão em domínio, limites Windows ou UI existente | 75/75 aprovados (exit code 0): 11 `SpecAudit.Tests`, 16 `SpecAudit.Windows.Tests`, 48 `SpecAudit.Ui.Tests` | PASSOU | [run-03-regression.txt](logs/T025/run-03-regression.txt) |

## Comparação visual com o mockup aprovado

- Referência: [documentos-sugestoes.png](../design/mockups/documentos-sugestoes.png) (SHA-256 `537bd823510bd235aaa6b72a190ea966942002c106daefe17d6d280534dd74fd`) e [README dos mockups](../design/mockups/README.md).
- Versão avaliada: `14865b7fb8aba1e95792ddca52d4286ef30edd38`, viewport 1440 × 1000, tema escuro (único tema disponível antes da US8/T044).
- Aspectos preservados: sidebar persistente com destaque azul da área ativa, busca no topo à direita, faixa de contexto com nome do projeto, selo `Somente leitura` e destino externo, árvore à esquerda com ícone lilás de pasta e seleção azul, leitor central estilo preview padrão do VS Code e painel lateral direito (`Neste documento` + `Procedência documental`).
- Diferenças intencionais da V1 aplicadas:
  - Modo `Lado a lado` adicionado aos controles (`Preview`, `Fonte`, `Lado a lado`), além dos dois modos ilustrados no mockup.
  - Painel direito exibe `Procedência documental` (arquivo, categoria, geração do catálogo e limites de proteção) em vez de ocorrências fictícias ou links para código (`src/search.ts`).
  - Seletor de tema claro/escuro e demais áreas do shell (`Dashboard`, `Requisitos`, `Verificações`, `Sugestões`) pertencem às fases US3/US4/US8 e serão conferidos em ambos os temas na T044/T089.

## Cobertura e limitações

- **Critérios cobertos**: US2-AC01 (`US2-S01`), US2-AC02 (`US2-S02`), US2-AC03 (`US2-S03`), US2-AC04 (`US2-S04`).
- **Integrações reais versus simuladas**:
  - Host Kestrel loopback, Blazor Server, seletores nativos de pasta Windows, `DocumentCatalog`/`DocumentBroker` via handles Windows somente leitura, renderização Markdig/AngleSharp/Highlight.js/Mermaid em Edge headless e abertura de link externo em nova aba são reais.
  - Para manter execução 100% offline sem tráfego à internet: em `S04` (host externo `SpecAudit.dll`), o clique em `Carregar nesta sessão` aciona `RemotePreviewImages.Default` no servidor, que recusa o domínio sintético `.test` sem disparar requisição do navegador nem encaminhar cookies; a entrega positiva de PNG via servidor HTTP controlado em loopback sem `Cookie`/`Authorization`/`Proxy-Authorization`/`Referer` e a recusa de HTTP 302 são comprovadas em `PreviewAssetsTests.RendersOnlyMermaidWithLocalAssetsAndKeepsFallbackSource` sobre a mesma operação `DocumentPreviewSession.AuthorizeRemoteImageAsync`.
  - Navegação para `https://docs.preview-fixture.test/guide` em nova aba é respondida localmente via `context.RouteAsync` do Playwright para evitar acesso real à internet.
- **Cenários não executados**:
  - Comparação visual no tema claro: **NÃO EXECUTADO** nesta etapa porque o alternador de tema é entregue apenas na US8 (T044). Impacto: restrito à aparência no tema claro; próximo passo: validar ambos os temas em T044/T045 e T089.
  - Cinco testes históricos da US1 (`RealBrowserRendersLocalProjectSelectionWithoutCircuitErrors` e `US1IntegratedHarnessTests`) foram excluídos do filtro de regressão porque gravam em caminhos históricos de evidências deste checkout (mesmo filtro de T021–T023).
- **Regressão executada**: 75/75 aprovados em RUN-03 (0 falhas).

## Rodadas após correções

- Durante a T024 foram corrigidos três achados observados em execução real: (1) sincronização do zoom em `S03` aguardando o controle `Aumentar zoom` antes de medir a largura do iframe; (2) remontagem do `MarkdownPreview` ao retornar de `Hidden=true` para preservar o iframe Mermaid no modo `Lado a lado`; (3) adição de `@key` nos nós da árvore (`DocumentTreeNode.razor` e `Documents.razor`) para evitar reuso de nó DOM pelo Blazor durante filtragem por busca. Todas as rodadas RUN-01–RUN-03 acima ocorreram após essas correções, sobre o commit `14865b7fb8aba1e95792ddca52d4286ef30edd38`.

## Identificação do incremento e histórico

- PR previsto ou URL real: PR-B (local, ainda não aberto/integrado).
- História e critérios cobertos: US2 (US2-AC01–AC04, cobertura técnica completa da história no escopo do tema escuro).
- Base resolvida: `73e06b9` (precedido por PR-A `ddf8f31` e T021–T023 `9e4ffbd` / `3803ffb`).
- Versão candidata: `14865b7fb8aba1e95792ddca52d4286ef30edd38`.
- Predecessores e ordem: PR-A (técnico concluído, aprovação humana pendente), T021–T024.
- Escopo desta evidência: `INCREMENTO` (US2 dentro do PR-B).
- Nova rodada após correção ou mudança de base: não necessária até o momento; sujeita à revisão T026.

## Execução real do produto

- **Versão e ambiente**: commit `14865b7fb8aba1e95792ddca52d4286ef30edd38`, Windows x64, .NET 10.0.15, Edge headless, viewport 1440 × 1000.
- **Meio**: `UI` (navegador real sobre host loopback `SpecAudit.dll` com diálogos nativos Windows).
- **Inicialização e entrada exata**: `LocalHostFixture` inicia `SpecAudit.dll --urls http://127.0.0.1:<porta>`, abre `/projetos`, seleciona origem e destino sintéticos de `DocumentaryFixture`, confirma carregamento e navega pelo link `Documentos` (`/documentos`) até `specs/001-demo/research.md`.
- **Resultado esperado e asserções**: cenários US2-S01–S04 com árvore filtrável, âncora local e entre documentos (`:target`), mensagens de referência ausente e fora do catálogo, Preview/Fonte/Lado a lado somente leitura, imagem PNG local via broker, Mermaid em iframe opaco com zoom/fonte, fallback de Mermaid inválido/PlantUML/Graphviz, bloqueio de scripts/formulários, zero requisições externas automáticas, clique em `Carregar nesta sessão` sem envio de cookies do navegador, link externo em nova aba, integridade da origem (`fixture.IsUnchanged() == true`) e pasta de destino vazia.
- **Resultado observado e efeitos**: todas as asserções passaram em RUN-02 (15/15); origem permaneceu bit a bit inalterada e nenhum arquivo foi gravado na pasta de relatórios antes de Salvar.
- **Evidência**: [run-01-domain.txt](logs/T025/run-01-domain.txt), [run-02-us2-e2e.txt](logs/T025/run-02-us2-e2e.txt), [run-03-regression.txt](logs/T025/run-03-regression.txt), [documents-preview.png](captures/T025/documents-preview.png), [documents-source.png](captures/T025/documents-source.png) e [documents-split.png](captures/T025/documents-split.png).
- **Limites de simulação e cenários não executados**: respostas remotas controladas localmente para manter execução offline; tema claro pendente da US8/T044; aceite humano da US2 pendente de execução e decisão do DEV (T028).

