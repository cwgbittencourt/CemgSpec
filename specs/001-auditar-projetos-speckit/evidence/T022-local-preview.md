# T022 — Renderização local de Markdown e Mermaid

T022 concluída no escopo do renderizador e dos assets do PR-B. Autorrevisão e execução pelo Codex em 2026-11-18, America/Sao_Paulo. Base Git `ddf8f316bb249e526d74ae63edd188c0f671269a` com T021 local. O candidato inclui os arquivos identificados por SHA-256 abaixo.

## Dependências auditadas

| Componente | Versão e licença | Papel |
|---|---|---|
| [Markdig](https://www.nuget.org/packages/Markdig/1.6.0) | 1.6.0, BSD-2-Clause | CommonMark, IDs de títulos, tabelas e listas de tarefas |
| [AngleSharp](https://www.nuget.org/packages/AngleSharp/1.10.5) | 1.10.5, MIT | Parse de HTML em memória. A lista de elementos e atributos permitidos é aplicada por `MarkdownRenderer`, sem carregador de recursos |
| [Mermaid](https://github.com/mermaid-js/mermaid/releases/tag/mermaid%4012.3.0) | 12.3.0, MIT | Renderização local dos diagramas |
| [Highlight.js](https://github.com/highlightjs/highlight.js/releases/tag/11.12.0) | 11.12.0, BSD-3-Clause | Destaque local de blocos com linguagens da distribuição comum |
| [esbuild](https://www.npmjs.com/package/esbuild/v/0.28.2) | 0.28.2, MIT | Empacotamento de desenvolvimento |

O [lockfile NuGet](../../../CemgSpec/packages.lock.json) fixa Markdig e AngleSharp, com hashes de conteúdo e sem outras dependências para `net10.0`. SHA-256 dos pacotes NuGet baixados para a auditoria:

- Markdig 1.6.0: `5a41668881b286aa531ad37b5c9d4c88bf68da5df1cec9f128f4fce08e510bb2`
- AngleSharp 1.10.5: `94c062a4bab5b677e22937d3389c161bf24e1de8b61224ca3e18adf8d8b8c21b`

O [lockfile npm](../../../tools/preview/package-lock.json) fixa as dependências diretas, transitivas e opcionais do empacotamento. O [manifesto distribuído](../../../CemgSpec/wwwroot/lib/preview/vendor/manifest.json) identifica os 65 pacotes efetivamente presentes nos dois bundles, versões, licenças, integridade dos pacotes e SHA-256 dos assets. Os [avisos de terceiros](../../../CemgSpec/wwwroot/lib/preview/vendor/THIRD-PARTY-NOTICES.txt) incluem os textos de licença, com Apache-2.0 para DOMPurify e EPL-2.0 para ELK, mais a revisão da fonte correspondente de ELK.

A auditoria NuGet direta/transitiva e a auditoria do lockfile npm (incluindo ferramentas de build) retornaram zero avisos conhecidos nas fontes consultadas. [Saída NuGet](logs/T022/nuget.txt) · [Saída npm](logs/T022/npm-audit.json) · [Reprodução dos hashes dos bundles](logs/T022/bundles.txt).

## Contrato local e limites

- O renderizador interpreta o snapshot em UTF-8, preserva texto e identifica títulos, destinos e âncoras. A reconstrução do HTML permite formatação básica e caixas de tarefa desabilitadas. URLs de links e imagens ficam em metadados para classificação e resolução posterior.
- A representação de imagens locais aceita bytes raster PNG/JPEG/WebP em snapshots e limita a inserção acumulada a 12 MiB de caracteres. O corpus recebe uma imagem PNG sintética por esse contrato. A resolução de imagens a partir do catálogo e a prova pelo broker pertencem à T023. SVG de entrada e URLs externas ficam fora do HTML ativo do renderizador.
- Mermaid recebe o texto por mensagem em um iframe de origem opaca, em modo estrito, com política de conteúdo que bloqueia rede. O SVG só fica no iframe. O conteúdo da fonte continua disponível. Diretivas e frontmatter embutidos, limites excedidos e falhas de renderização mantêm a fonte visível. PlantUML e Graphviz exibem a fonte acompanhada da explicação.
- T023 ainda precisa validar resolução autorizada, não execução e efeitos em rede antes de habilitar o preview. T024 integra a tela, os modos e a autorização explícita. Na integração, o teste de SVG da T021 deve localizar o SVG dentro do iframe, como no caso executado de `PreviewAssetsTests`.

## Execução e autorrevisão

Ambiente Windows, SDK .NET 10.0.404, Edge headless e Playwright 1.64.1. Bundles gerados com Node.js 24.19.0 e npm 11.17.0. Para reproduzir a partir da raiz:

```powershell
npm --prefix tools/preview ci --ignore-scripts
npm --prefix tools/preview audit --json
node tools/preview/build.mjs
dotnet restore CemgSpec.slnx --locked-mode --source https://api.nuget.org/v3/index.json
dotnet list CemgSpec.slnx package --vulnerable --include-transitive --no-restore --source https://api.nuget.org/v3/index.json
$filtro = 'FullyQualifiedName!~DocumentNavigationTests&FullyQualifiedName!~S01SearchTree&FullyQualifiedName!~S02PreviewSource&FullyQualifiedName!~S03MermaidRenders&FullyQualifiedName!~S04ActiveContent&FullyQualifiedName!~RealBrowserRendersLocalProjectSelectionWithoutCircuitErrors&FullyQualifiedName!~US1IntegratedHarnessTests'
dotnet test CemgSpec.slnx --no-restore --filter $filtro --logger 'console;verbosity=normal'
```

| Verificação | Observação | Resultado |
|---|---|---|
| Renderização de Markdown e HTML | Títulos, lista, tarefas desabilitadas, tabela, citação, HTML básico e fonte preservados. Bytes UTF-8 inválidos têm estado explícito | PASSOU |
| Corpus no Edge a partir do host loopback | Imagem PNG local e destaque C# visíveis. Flowchart e sequência em SVG isolado, zoom e fonte acessíveis. Mermaid inválido, PlantUML e Graphviz exibiram fallback | PASSOU |
| Conteúdo ativo e rede no corpus | Script e formulário não apareceram no HTML ativo. Canário permaneceu inerte. Imagem remota sem elemento carregável e zero requisições externas observadas | PASSOU |
| Regressão selecionada | 61 aprovados, sendo 10 domínio, 16 Windows e 35 UI, incluindo os quatro testes da T022 | PASSOU |
| Resolução autorizada e tela completa | Casos antecipados da T021 excluídos do filtro. Cinco testes preexistentes que gravam em caminhos de evidências históricos também foram excluídos | PENDENTE T023–T025 |

[Log do corpus e da regressão](logs/T022/corpus-and-regression.txt). A autorrevisão conferiu versões, integridade, reconstrução da marcação, limitação de recursos, isolamento do Mermaid e a ausência de integração da tela nesta revisão. A execução e o aceite da US1 pelo DEV seguem pendentes.

### SHA-256 dos arquivos da T022


- `CemgSpec/Infrastructure/Documents/MarkdownRenderer.cs` — `0416ceb2836d56bfdc9617b6594f2496abedc5119a4dd2d04bbe4fa92147e0e6`
- `CemgSpec/CemgSpec.csproj` — `d034d99457c8fa5ac53847884ccb270baad29075073e60c1bec4dc9fe18f4ed6`
- `CemgSpec/packages.lock.json` — `a6de8a69117d16c7f232618c68c0614c6e2b8bd42de36b8cea90d4e47dbf38bd`
- `CemgSpec/wwwroot/lib/preview/preview.js` — `8fb02491f8491d371ff15d973b1c0b1370fbdc30a71b3ac9af043a852b5d8c19`
- `CemgSpec/wwwroot/lib/preview/preview.css` — `15d574bd365da6797ae6297bf6b7e2c23de3e100acf7e6f5c9d12e9e4940993d`
- `CemgSpec/wwwroot/lib/preview/vendor/manifest.json` — `fe833ac82fd07e2acee804b1f22a5ce3394f7e11c91f83743c9f43ff82dcc39f`
- `tests/CemgSpec.Tests/MarkdownRendererTests.cs` — `18c150199b6ca0381fac908b3feaccf55d8e08865b8d8fb1f66ffb757ce8c2ec`
- `tests/CemgSpec.Ui.Tests/PreviewAssetsTests.cs` — `03967b05b4c3f898e2c1213f55ef4f4a746f2f4c2ac93f5b5c9a11c1742e4399`
- `tools/preview/build.mjs` — `55c03be1936dbd33868e0624eb3f97fe5f7da9758344e5eba92f41a4ccf8f0c9`
- `tools/preview/package-lock.json` — `7a02af830956c1a1326c1d7b64f7ef6332150a88851e93c291b3872f96f94950`
