# T023 — Segurança e navegação do preview

T023 concluída no escopo do broker, do renderizador e do transporte de imagens antes da integração da tela. Autorrevisão e execução pelo Codex em 2026-11-19, America/Sao_Paulo. Base `9e4ffbd` com os arquivos desta rodada identificados abaixo. Ambiente Windows, SDK .NET 10.0.404, Edge headless e Playwright 1.64.1.

| Cenário | Execução e efeito observado | Resultado |
|---|---|---|
| US2-S01 — links e âncoras | `DocumentNavigationTests` abriu um catálogo Windows real. O link relativo e a âncora local retornaram o ID do documento e do título. Arquivo ausente e âncora ausente retornaram estados diferentes. Código, caminho que escapava da origem e separador codificado foram recusados. Uma sessão antiga falhou após a atualização do catálogo. | PASSOU |
| US2-S02 — imagens locais | O Edge decodificou o PNG de 1 px. A instrumentação do broker confirmou leituras somente do Markdown e do PNG durante o preview. SVG de origem e imagem fora do catálogo ficaram inertes. O teste conferiu os bytes do texto e do PNG após a execução. | PASSOU |
| US2-S03 — Mermaid e SVG | O Edge exibiu flowchart e sequência no iframe de origem opaca. O link do diagrama foi removido e o texto do nó permaneceu. Diagrama inválido, PlantUML e Graphviz exibiram a fonte. | PASSOU |
| US2-S04 — HTML e rede automática | O navegador observou zero requisições externas. Script, formulário, SVG inserido como HTML e estilo do documento ficaram inertes. O transporte de imagem não abriu conexão antes do clique. | PASSOU |
| US2-S04 — clique e credenciais | O botão do harness acionou a operação do produto por índice de referência. Um servidor HTTP sintético em loopback recebeu uma requisição após o clique, sem `Cookie`, `Authorization`, `Proxy-Authorization` ou `Referer`. A imagem retornada foi decodificada. Um segundo pedido controlado retornou HTTP 302 e foi recusado sem conexão adicional. | PASSOU |
| US2-S04 — endereços de conexão | Nove casos de classificação IPv4/IPv6 cobriram loopback, rede privada, endereço link-local, endereço de documentação e endereço público. O conector de produção resolve o nome e aplica a mesma classificação antes de abrir um socket pelo IP resolvido. | PASSOU |

O harness usou o host local, os assets reais e a operação de autorização do preview. Só o destino físico do socket remoto foi substituído por um servidor de teste. O cliente HTTP e os cabeçalhos foram reais. A política de IP foi exercitada separadamente, sem resolver ou contatar servidores externos. Imagens remotas exigem HTTP(S) na porta padrão, resposta direta, conteúdo raster PNG/JPEG/WebP e no máximo 4 MiB. O aplicativo revalida o snapshot e a sessão antes e depois do carregamento.

Na [primeira rodada](logs/T023/initial-regression.txt), Mermaid produziu um link SVG mesmo com modo estrito. A correção remove links e elementos ativos do SVG antes de inseri-lo no iframe. Uma [rodada intermediária](logs/T023/xml-regression.txt) mostrou que o parser XML rejeitava parte da saída gerada. O parser HTML em um template inerte preservou o diagrama e permitiu limpar a árvore antes da inserção. A [rodada final](logs/T023/regression.txt) aprovou 71 testes (11 de domínio, 16 Windows e 44 UI).

Comando a partir da raiz:

```powershell
$filtro = 'FullyQualifiedName!~S01SearchTree&FullyQualifiedName!~S02PreviewSource&FullyQualifiedName!~S03MermaidRenders&FullyQualifiedName!~S04ActiveContent&FullyQualifiedName!~RealBrowserRendersLocalProjectSelectionWithoutCircuitErrors&FullyQualifiedName!~US1IntegratedHarnessTests'
dotnet test SpecAudit.slnx --no-restore --filter $filtro --logger 'console;verbosity=normal'
```

O filtro exclui os quatro testes antecipados que dependem da tela de Documentos e cinco testes existentes que escrevem em caminhos históricos de evidência. A navegação na tela, a abertura de link externo em nova aba e a interface de autorização serão integradas na T024 e validadas na T025. O aceite da US1 permanece pendente de execução e decisão do DEV.

### SHA-256 dos arquivos executados


- `SpecAudit/Application/ProjectService.cs` — `9ea9221cdfccbcc839621f71c91a4f8358f6b46370e84653823a4b6cf7d57b51`
- `SpecAudit/Infrastructure/Documents/DocumentBroker.cs` — `87ffca3563fac63c4c399e84882025f390c538789088fe73f19c40c72ef3d629`
- `SpecAudit/Infrastructure/Documents/DocumentPreviewSession.cs` — `b911b2b83a715c6a099d29aaa4137aefc407ca03e998f3319bf42bbc397810a6`
- `SpecAudit/Infrastructure/Documents/RemotePreviewImages.cs` — `ee3bc0606d555a96c62a1431be1b00dcd7d9242fed72fc95241dbfe260726074`
- `SpecAudit/wwwroot/lib/preview/preview.js` — `ec49964dfc3b9438c5f00b6ae7f8b4b935ed1cab16489ebed304a86b37aeba00`
- `tests/SpecAudit.Tests/DocumentNavigationTests.cs` — `548a29700d267b13c76c081becde7213893ad02720f3b4b137aa121e7da7a12c`
- `tests/SpecAudit.Ui.Tests/PreviewAssetsTests.cs` — `3693dace43ad303a29b3c844b68c00a9ee54c0dc35274c69fdc11e25121cb1ed`
