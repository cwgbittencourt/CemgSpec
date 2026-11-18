# Evidências de validação — US1 (T013–T015)

**Estado vigente — T015, 2026-11-17: APROVADA no escopo técnico corrigido.** Os dois achados da T014 foram corrigidos e revalidados: 52/52 na suíte permanente, 3/3 no replay exato dos probes originais e 5/5 no teste antes intermitente. Não há aceite humano ou aprovação de PR inferidos. Os registros T013/T014 abaixo são históricos; a rodada T015 ao final e o manifesto corrente identificam a revisão atual.

## Registros históricos T013 e T014

Atualização de revisão T014 (2026-11-17): validação adicional FALHOU em três probes, confirmando dois achados; ver [US1-review.md](US1-review.md) e `logs/T014-review/`. A aprovação técnica T013 descrita abaixo é histórica e limitada aos cenários então executados; não representa aprovação da revisão atual. Correção/revalidação pendentes na T015.

Estado: APROVADA no escopo técnico da T013. Review completo de US1 (T014), fechamento de PR-A, reprodução e aceite humano continuam pendentes.
Executor: Codex, dotnet test/xUnit, harness Windows e Playwright/Edge.
Data: 2026-11-17, America/Sao_Paulo (UTC−03:00); horários UTC exatos nos TRX e JSONs.
Base: `139fd81db66943bbc2e167ccd13a75b4a317a2fc`.
Versão candidata: base + `US1IntegratedHarnessTests.cs`, identificado em `T013-code-manifest.json`; fonte preservada em `T013-code/`.
Ambiente: Windows 10.0.26300 x64, SDK 10.0.404, runtime 10.0.15; inventário em `logs/T013-20261117/environment.txt`. Sem nova dependência ou alteração no código de produção.
PR previsto: PR-A; predecessor técnico T012. Escopo: validação do carregamento/preservação/invalidação da US1 por APIs públicas em harness real, conforme contrato da task.

## Pré-condições e consistência

Checklist requirements.md: 19 itens, 19 marcados, zero pendentes; nenhum marcador alterado. Prerequisites retornou FEATURE_DIR absoluto e research/data-model/contracts/quickstart/tasks. T001–T012 e migração T095–T098 concluídas. Constituição vigente 4.1.0: emenda 4.0 de confiança e 4.1 de registro humano preservadas. Cabeçalhos históricos de documentos não substituem essas emendas explícitas. Spec/plan/modelo/contratos/tasks inspecionados para este incremento; nenhum requisito futuro foi imposto como dependência da T013.

Hooks: `.specify/extensions.yml` ausente na entrada e na saída; nenhuma obrigação adicional identificada. Templates CEMG instalados e overrides locais de roteiro/aceite lidos; scripts ativos locais em `.specify/scripts/powershell/`. `.gitignore` cobre bin/obj/configurações privadas; nenhuma mudança de tecnologia ou publicação nesta task.

O executor padrão do ambiente Codex falhou antes de iniciar processos (`setup refresh had errors`). Execução fora desse sandbox foi autorizada pelo mecanismo de aprovação e funcionou. Isso não é alteração de token/ACL, instalação nem retorno ao sandbox OS retirado do produto.

## Execuções

Diretório de todos os comandos: `__REPO_ROOT__`.

| Rodada | Comando/procedimento | Esperado | Observado | Resultado | Evidência |
|---|---|---|---|---|---|
| Inicial | `dotnet restore SpecAudit.slnx --locked-mode` | Restaurar respeitando locks | Exit 0 | PASSOU | logs/T013-20261117/restore.txt |
| Regressão inicial | `dotnet test SpecAudit.slnx --no-restore` com logger TRX | Suíte existente sem regressões | 41 aprovados, zero falhos/ignorados | PASSOU | logs/T013-20261117/suite.txt; results/ |
| Harness rodada 1 | Filtro `FullyQualifiedName~US1IntegratedHarnessTests` | Selecionar pastas em diálogo nativo | 4 falhas: harness buscava título da janela, mas título fornecido fica em controle filho | FALHOU | logs/T013-20261117/harness.txt; harness-results/ |
| Harness rodada 2 | Mesmo filtro, após corrigir descoberta de janela | Quatro cenários observados | 4 aprovados, zero falhos/ignorados | PASSOU | logs/T013-20261117/harness-r2.txt; harness-r2-results/ |
| Regressão final | Comando completo abaixo | Suíte e quatro cenários aprovados | 45 aprovados (7 domínio + 14 Windows + 24 UI/harness), zero falhos/ignorados; exit 0 | PASSOU | logs/T013-20261117/final-suite.txt; final-results/ |
| Dependências | `dotnet list SpecAudit.slnx package --vulnerable --include-transitive --no-restore --format json` | Consulta NuGet aplicável | Exit 0, nenhum pacote vulnerável listado; nenhuma dependência adicionada | PASSOU | logs/v4-package-audit.json |

Consulta de pacotes não audita integralmente SDK, Windows ou navegador e não garante ausência de todas as vulnerabilidades.

## Cenários e asserções reais

| Cenário / critério | Entrada e acionamento | Observado e efeitos conferidos | Resultado |
|---|---|---|---|
| US1-S01 / AC01 | Fixture com duas funcionalidades, segunda sem tasks; NativeFolderPicker real → OpenProjectAsync → SelectFeature/ReadDocument | Duas funcionalidades e ausência reconhecidas; todos os documentos catalogados lidos pelo broker; baseline integral preservada; destino vazio | PASSOU |
| US1-S02 / AC02 | NativeFolderPicker real seleciona origem, descendente e junção para descendente como destino | Três DestinationInsideOrigin; nenhum projeto aberto, origem intacta, nenhum relatório. Regressão ReaderBoundary confirma exclusão de código/config e rejeição de junção antes de ler bytes | PASSOU |
| US1-S03 / AC03 | Duas fixtures, arquivo histórico sintético preexistente, seleção nativa e ChangeOutputDirectoryAsync | Dois contextos, funcionalidade selecionada preservada; só destino do primeiro muda; hash/local do histórico preservados, segunda saída vazia, duas origens intactas | PASSOU |
| US1-S04 / AC04 | CaptureDocumentVersion e leitura documental em Task em andamento; barreira segura após snapshot; escritor externo edita spec; monitor real notifica; retomada valida versão | Changed, cancelamento sinalizado, catálogo pendente; snapshot antigo imutável; edição preservada; todos os demais arquivos/caminhos iguais à baseline; nenhum relatório criado | PASSOU |

Arquivos `logs/T013-20261117/harness/US1-S0n-*.json` registram raízes TEMP exclusivas, observações e horários por execução. Fixtures, baseline e paths.json permanecem nos diretórios sintéticos externos para inspeção. O arquivo histórico do S03 foi preparado pelo harness, não produzido por Save fictício.

## Execução real e limites

Meio: harness da API pública de carregamento/leitura/versionamento, com NativeFolderPicker, catálogo, handles Windows, broker e FileSystemWatcher reais. A automação seleciona a pasta no diálogo real com mensagem Windows documentada, verifica o caminho retornado e limita descoberta de janelas ao próprio processo. Diálogo é ocultado após identificação. Não há picker falso nos quatro novos casos. Host loopback e Edge headless reais também foram iniciados/encerrados pela regressão; captura em `logs/T011-project-page.png`.

A cobertura de navegador nesta rodada é a página inicial; o fluxo completo de carregamento é comprovado pelo harness permitido na T013. Não foi executada navegação humana do diálogo. O S04 exercita trabalho documental em andamento e sua invalidação, não análise de agente, Run persistido ou Save de histórias futuras. Nenhuma terceira verificação foi adicionada ao produto; executar análise Codex/Antigravity pertence à US5. As integrações futuras ainda precisam validar seus resultados Stale usando esta fronteira. Nenhum aceite humano inferido.

## Reprodução pelo DEV

Na revisão identificada, com Windows e Edge instalados:

```powershell
Set-Location '__REPO_ROOT__'
dotnet restore SpecAudit.slnx --locked-mode
dotnet test tests/SpecAudit.Ui.Tests/SpecAudit.Ui.Tests.csproj --no-restore --filter FullyQualifiedName~US1IntegratedHarnessTests
dotnet test SpecAudit.slnx --no-restore --logger 'trx;LogFilePrefix=T013-final' --results-directory specs/001-auditar-projetos-speckit/evidence/logs/T013-20261117/final-results
```

Esperado: quatro casos do harness aprovados e 45 na suíte total. Cada rodada cria fixtures/JSONs únicos; não reutiliza solução real. Para preservar a captura T011 em uma nova regressão, copiar a captura histórica antes do comando e guardar a nova imagem em outro nome, restaurando apenas o artefato histórico do checkout de desenvolvimento. Isso nunca restaura a origem auditada. Os testes iniciam e encerram seus próprios hosts/processos; não há exclusão automática das fixtures. Não instalar ferramentas faltantes silenciosamente. Roteiro do harness CONFERIDO_PELA_IA; execução do DEV pendente. Consolidação do roteiro de história é T016.

## Autorrevisão do incremento T013

Modalidade AUTORREVISAO, Codex, base e candidato acima. Inspecionados o novo teste, diff de controle de tasks, registros e asserções contra AC01–AC04. A primeira falha era localização incorreta do diálogo no harness; corrigida usando texto do controle filho e PID próprio, revalidada em rodada 2 e suíte final. Não houve alteração de produção para fazer o teste passar. Nenhum achado impeditivo conhecido no escopo da T013 após revalidação. T014 continua responsável pela revisão completa do diff funcional US1/PR-A; esta passagem não a conclui. Aprovação de PR e aceite permanecem pendentes.

## Gate e controle final

`check_gate.py --feature __REPO_ROOT__/specs/001-auditar-projetos-speckit --record verification/implementation-stage.json --stage pr --revision sha256:43352aaa8e3fd85b0de05446d353d21f9f8602801356487e1daa0a2104fb9d47` executado com `__USER_HOME__/anaconda3_11/python.exe`. Resultado esperado e observado BLOCKED/exit 1: revisão completa, fechamento/dependências de PR e decisão humana pendentes. Sem erros de hash/revisão ou evidência de execução. Saída em `logs/T013-20261117/gate.txt`. Este é gate do PR-A, não gate de passagem T013→T014; a próxima task de review pode iniciar.

Finais de linha CRLF preservados conforme `.gitattributes` (`* -text`); `git -c core.whitespace=cr-at-eol diff --check` passou. Sem testhost ou host SpecAudit restantes após a suíte; os três processos dotnet remanescentes foram identificados como servidores MSBuild, não execuções da origem. Captura histórica T011 preservada byte a byte. Nenhum hook obrigatório configurado ou despachado, nenhuma instalação, commit, push ou publicação.


## Rodada T015 — correção dos achados e regressão

Data: 2026-11-17, America/Sao_Paulo. Base `aaf89ba8c232e69e8fcd1529e2437d6147836dd4` + diff local identificado pelo manifesto corrente `maintenance-code-manifest.json` e `T015-context.json`. Executor: Codex/dotnet/xUnit/Playwright/Edge, Windows x64, .NET 10; nenhuma dependência adicionada. Cwd: `__REPO_ROOT__`.

REV-US1-01: `DocumentChangeMonitor.Notify` ignora somente Changed/LastWrite/Size de diretório que ainda existe. Created/Deleted/Renamed e Error continuam no fluxo anterior; não há leitura de código nem restauração. REV-US1-02: `Projects.Open` e `Refresh` tratam LimitExceededException; Dimension identifica tamanho/grafo na mensagem. Limites numéricos, origem readonly e rollback do catálogo permanecem iguais.

| Rodada | Comando/entrada | Esperado | Observado | Resultado | Evidência |
|---|---|---|---|---|---|
| Preparação dos testes | Filtro ProjectCorrectionTests antes da correção | Detectar defeitos reais | Quatro casos de limite falharam; três casos de metadados atingiram timeout porque o observador de teste comparava separadores diferentes | FALHOU; calibração do harness, não prova dos três casos de metadados | logs/T015/red.txt; red-results/ |
| Red conferido | Mesmo filtro após normalizar o caminho do observador, ainda sem alterar produção | Sete asserções detectam os defeitos | 7 falhas de comportamento: três metadados, dois Open e dois Refresh | FALHOU | logs/T015/red-verified.txt; red-verified-results/ |
| Green | Mesmo filtro após correção | Sete casos aprovados, recuperação segura | 7/7, zero falhos/ignorados | PASSOU | logs/T015/green.txt; green-results/ |
| Replay T014 | Fonte exata preservada de T014ReviewProbe; filtro correspondente | Mesmos três casos agora aprovados | 3/3; JSONs mostram HasPendingChanges/cancelled false e erros tratados, origem intacta e zero relatórios | PASSOU | logs/T015/original-probes.txt; original-probe-results/; original-probe-observations/ |
| Suíte final | `dotnet test SpecAudit.slnx --no-restore --logger 'trx;LogFilePrefix=T015-final' --results-directory specs/001-auditar-projetos-speckit/evidence/logs/T015/final-results` | Regressão das fronteiras, estado, sessão, seleção, IO e UI | 52/52: 7 domínio + 14 Windows + 31 UI/harness, zero falhos/ignorados | PASSOU | logs/T015/final-suite.txt; final-results/ |
| Repetição do teste antes intermitente | Cinco execuções de `UncataloguedMarkdownAndCodeDoNotInvalidateDocuments`, com --no-build --no-restore | Nenhuma invalidação por arquivo fora do catálogo | 5/5 | PASSOU | logs/T015/watcher-1.txt a watcher-5.txt; watcher-results/ |
| Pacotes | `dotnet list SpecAudit.slnx package --vulnerable --include-transitive --no-restore --format json` | Consulta aplicável disponível | Exit 0, nenhum pacote vulnerável listado; não cobre integralmente SDK/Windows/Edge | PASSOU no alcance da consulta | Artefato canônico listado no manifesto de manutenção |

Os sete testes permanentes estão em `tests/SpecAudit.Ui.Tests/ProjectCorrectionTests.cs`. O observador independente comprova evento real do Windows antes de testar ausência de invalidação. Open com tamanho/grafo excedido preserva projeto anterior e versão válida; nova seleção funciona. Refresh preserva catálogo anterior pendente, informa a dimensão e permite nova atualização após reparo externo intencional da fixture. O teste, na função de usuário, corrige/move dados sintéticos; o produto não restaura nem apaga conteúdo para esconder erro. Baselines e ausência de relatórios são conferidas antes e depois de cada ação.

A suíte reutiliza os testes de criação de funcionalidades, rename/remoção de diretório com ponto, edição durante leitura, snapshot imutável, refresh, cancelamento, junção e isolamento de contexto. Os quatro cenários US1 com picker nativo real e o host/Edge reais foram repetidos. Os testes de handlers de limite invocam o Razor compilado por reflexão com picker controlado; não se afirma clique humano ou cobertura visual de cada mensagem de erro.

O replay do probe antigo grava três nomes fixos sob T014: suas novas observações foram separadas em T015 e os JSONs negativos originais restaurados do commit base byte a byte. Assim, rodadas negativas e positivas permanecem distintas. A fonte temporária saiu da suíte normal antes da execução final; os sete casos de regressão novos permanecem nela.

Reprodução do incremento, conferida pela IA: na revisão do manifesto, executar `dotnet test tests/SpecAudit.Ui.Tests/SpecAudit.Ui.Tests.csproj --no-restore --filter FullyQualifiedName~ProjectCorrectionTests` (7/7) e a suíte final acima (52/52). Cada execução usa fixtures novas externas; preservar baseline/paths para inspeção, não excluir automaticamente. O roteiro completo da história e registro de execução humana serão consolidados na T016; não preencher aceite do DEV nesta task.

Hashes, formatos e busca de dados pessoais foram conferidos no fechamento T015 em `T015-verification.json`. A busca por padrões não garante ausência de todo segredo. O gate de PR-A foi executado e retornou BLOCKED/exit 1 somente por fechamento/dependências e aprovação humana pendentes, sem erro de evidência/hash/revisão ou impeditivo técnico destes dois achados; saída em `logs/T012-gate.txt` ou seu arquivo canônico indicado na manutenção. Isso não impede concluir T015 e iniciar T016. As falhas T014 e red T015 ficam preservadas; a validação positiva corresponde somente à fonte corrigida identificada no manifesto.


## Conferência do roteiro — T016

Produto/testes iguais ao commit f2b29a20e5f9e724a9c32f8d8e3a703e0587d541 e à revisão de código do manifesto corrente. A IA executou novamente o caminho entregue ao DEV: restore --locked-mode, quatro cenários US1 (4/4), sete casos de correção (7/7) e suíte completa (52/52), com logs/TRX em logs/T016/. Dados gerados automaticamente em fixtures novas externas, native picker, filesystem/monitor Windows e host/Edge reais conforme alcance dos testes. Não se somam as contagens, pois os grupos estão incluídos na suíte completa.

validation/US1-dev.md agora é o ponto de entrada autossuficiente para a execução por harness, com condições, comandos, dados, asserções, observação e limpeza. O estado principal é CONFERIDO_PELA_IA; a exploração GUI adicional permanece CONFERENCIA_PARCIAL explícita. acceptance/US1.md registra apenas estado técnico e pendências de fechamento/execução/decisão humanas. Campos do DEV preservados em branco; nenhum aceite ou execução humana inferido. Detalhes e autorrevisão documental em T016-verification.json. A fonte do produto não mudou nesta task.
