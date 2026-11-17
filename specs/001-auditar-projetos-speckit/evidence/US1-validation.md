# Evidências de validação — US1 / T013

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
