# T021 — Testes de navegação e preview Markdown

T021 concluída no escopo de preparação dos testes. Execução e autorrevisão pelo Codex em 2026-11-18, America/Sao_Paulo. Incremento PR-B, sobre a base local do PR-A `ddf8f316bb249e526d74ae63edd188c0f671269a`. A revisão candidata é essa base mais os arquivos de teste identificados abaixo. Ambiente Windows, .NET SDK 10.0.404, runtime .NET 10.0.15, Playwright 1.64.1 e Edge headless com host loopback. Os testes geram e removem projetos temporários sintéticos.

## Execução

Comandos a partir da raiz do repositório:

```powershell
$red = 'FullyQualifiedName~DocumentNavigationTests|FullyQualifiedName~S01SearchTree|FullyQualifiedName~S02PreviewSource|FullyQualifiedName~S03MermaidRenders|FullyQualifiedName~S04ActiveContent'
dotnet test CemgSpec.slnx --no-restore --filter $red --logger 'console;verbosity=normal'

$regressao = 'FullyQualifiedName!~DocumentNavigationTests&FullyQualifiedName!~S01SearchTree&FullyQualifiedName!~S02PreviewSource&FullyQualifiedName!~S03MermaidRenders&FullyQualifiedName!~S04ActiveContent&FullyQualifiedName!~RealBrowserRendersLocalProjectSelectionWithoutCircuitErrors&FullyQualifiedName!~US1IntegratedHarnessTests'
dotnet test CemgSpec.slnx --no-build --no-restore --filter $regressao --logger 'console;verbosity=minimal'
```

| Cenário | Contrato preparado | Resultado nesta revisão |
|---|---|---|
| US2-S01 / AC01 | Busca e árvore de funcionalidades e registros; snapshot e links relativos, âncoras, referências ausentes e fora do catálogo | Duas falhas esperadas. Snapshot preservou o texto original e o catálogo excluiu o arquivo de código sintético. A coleção de referências permaneceu vazia e o navegador recebeu HTTP 404 em Documentos. |
| US2-S02 / AC02 | Preview, Fonte e Lado a lado; títulos, listas, tarefas desabilitadas, tabela, citação, imagem via catálogo, código destacado e equivalência com a fonte | Falha esperada no navegador, HTTP 404 antes de chegar às asserções da tela. |
| US2-S03 / AC03 | Mermaid local, zoom, fonte, mensagens de Mermaid inválido e fallback de PlantUML e Graphviz | Falha esperada no navegador, HTTP 404 antes das asserções de renderização e ausência de requisições externas. |
| US2-S04 / AC04 | HTML básico, script inerte, formulário removido, imagem sob ação explícita sem credenciais herdadas e link externo em nova aba | Falha esperada no navegador, HTTP 404 antes das asserções de conteúdo ativo e remoto. |
| Preparação e regressão | Abrir painel após conexão Blazor, escolher origem e destino pelos seletores nativos, conferir ambos os caminhos, carregar e checar integridade do corpus e ausência de relatórios | 57 aprovados na seleção, sendo 7 de domínio, 16 Windows e 34 de UI. Inclui a preparação do corpus no Edge. |

[Rodada de falhas esperadas](logs/T021/red.txt), código de saída 1. [Regressão selecionada](logs/T021/regression.txt), código de saída 0. O filtro de regressão exclui os cinco testes antecipados da US2 e cinco testes existentes que gravam capturas ou registros em caminhos históricos deste checkout. A regressão inclui a nova preparação do corpus e o teste existente de cancelamento. O navegador e a seleção de pastas são reais. As respostas remotas dos testes de preview são interceptadas pelo Playwright com respostas sintéticas, sem chamadas à internet. A validação funcional da US2 fica para T023–T025.

### Rodadas intermediárias

- A [primeira rodada integrada](logs/T021/initial-red.txt) e a [regressão inicial](logs/T021/initial-regression.txt) registraram um clique antes de o painel ficar interativo no teste de preparação. O teste passou a repetir a abertura depois de aguardar o painel.
- Após [repetir as falhas esperadas](logs/T021/parallel-red.txt), uma [regressão posterior](logs/T021/parallel-regression.txt) detectou a mesma corrida no teste existente de cancelamento. Ambos passaram a usar a mesma preparação em `LocalHostFixture.OpenProjectPanelAsync`. A última rodada executou novamente o conjunto de falhas esperadas e a regressão selecionada.

## Comparação visual preparada para T024–T025

Conferir a metade superior de [documentos-sugestoes.png](../design/mockups/documentos-sugestoes.png) com a [referência aprovada](../design/mockups/README.md). SHA-256 da imagem conferido contra o manifesto, `537bd823510bd235aaa6b72a190ea966942002c106daefe17d6d280534dd74fd`.

| Estado para capturar no app | Mesmo aspecto a conferir | Comportamento V1 a conferir |
|---|---|---|
| Documentos com `research.md` ativo, 1440 × 1000 | Sidebar, busca no cabeçalho, árvore à esquerda, prévia central, contexto à direita, destaque do documento ativo, margens e contraste | Identificar projeto, origem somente leitura e destino externo. A árvore e a busca mostram documentos e registros do catálogo. Ocorrências, se exibidas, trazem procedência documental. |
| Preview, Fonte e Lado a lado no mesmo documento | Hierarquia dos títulos, tabela e bloco de código na prévia; controles de modo visíveis no painel | Incluir Lado a lado além dos dois modos ilustrados. Fonte e tarefas permanecem somente leitura. A mudança de modo preserva o documento e seu conteúdo. |
| Âncora, referência ausente, Mermaid válido/inválido e imagem remota antes e depois da autorização | Legibilidade dos estados e alinhamento dos avisos com os painéis | Exibir mensagens para referências e diagramas sem destino/renderização. A imagem remota exige ação explícita. O leitor não abre arquivo de código nem executa verificações. |

Repetir as capturas nos temas disponíveis na validação da história. Conferir ambos os temas após a entrega do seletor da US8 na T044 e na regressão integrada. Registrar versão, tema, imagem usada, diferença intencional e justificativa. A interface de Documentos ainda retorna 404, então as capturas comparativas ficam pendentes.

## Autorrevisão

Escopo examinado: assertivas por critério, isolamento e limpeza do corpus, origens remotas controladas, reaproveitamento de seleção e sincronização Blazor. A revisão corrigiu a sincronização da abertura do painel e substituiu a imagem sintética inicial por um PNG com CRC válido antes da repetição final. A superfície de produção segue na base declarada. O aceite da US1 depende da validação e decisão do DEV. Aprovação e integração do PR-A preservam seu próprio registro.

### SHA-256 dos arquivos de teste executados


- `tests/CemgSpec.Tests/DocumentNavigationTests.cs` — `6c096f9a2cb735e1045b852c8ab392ce7f73b331e8219500e1145dd1e84479b1`
- `tests/CemgSpec.Ui.Tests/MarkdownPreviewTests.cs` — `4be4372b139eea9461c57e7432edd8b8460d8e237df54e2f08ed547b09a83cc0`
- `tests/CemgSpec.Ui.Tests/Fixtures/LocalHostFixture.cs` — `9b04b247336ac7c2e4143c7f56df67c7cc19235eba39908a107f2b1d176ca555`
- `tests/CemgSpec.Ui.Tests/ProjectPageBrowserTests.cs` — `da1994405870c6a54efa031ea3d023f71242dc4176e7a1fd7b5d880ed7b9578e`
- `tests/CemgSpec.Ui.Tests/US1IntegratedHarnessTests.cs` — `9316f0b059bcc66939eaa026cacb10702077971f413254eedfdd0fdaf8f1434a`

### Gate do PR-A

O verificador foi executado com a revisão registrada do PR-A e retornou BLOCKED. O relatório aponta a execução de produto da IA e a aprovação humana pendentes no registro do PR-A. Também aponta uma divergência de origem para os templates de plan e tasks. A emenda 4.1.0 documenta os overrides locais desses templates, enquanto o verificador espera o preset como fonte ativa. [Saída do gate](logs/T021/pr-a-gate.txt).
