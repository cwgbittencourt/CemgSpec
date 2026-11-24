# Evidências de validação — PR-A / T017

Estado: APROVADA (validação técnica do incremento; revisão final e decisões humanas pendentes).
Executor: IA / speckit-implement. Data: 2026-11-17, America/Sao_Paulo.
Commit candidato: `3bff5005a061b746e826c26b14d7e0f5f6e96016`. Fonte exata: `sha256:530031e68a790d3b2b186300c23441091ef951d6fb93fdc1bae4029b11b451f8` em maintenance-code-manifest.json.
Alterações locais: apenas evidências T017 e acompanhamento; produto, testes e ferramentas iguais ao candidato. Histórico e fonte reproduzíveis pelo Git e manifesto.
Ambiente: Windows, .NET SDK 10.0.404; detalhes em logs/T017/environment.txt. Dados sintéticos isolados; nenhuma solução real auditada.

## Identificação do incremento e histórico

PR previsto: PR-A, sem URL criado. Escopo: INCREMENTO, US1-AC01–AC04 e fundação/proteção de leitura, T001–T016 e T095–T098. Base resolvida: `79d9c3d08d75afe2fef7e233493b8648e61ed4e0`; base de código anterior à implementação: `197548db2e3efa20c23e1f66b6a0ffb86c0f328d`. Ambas verificadas como ancestrais do candidato. Sem PR predecessor. T016 concluída; T018–T020 permanecem pendentes.

## Execuções

Diretório de todos os comandos: `__REPO_ROOT__`.

| Execução | Critério/cenário | Comando ou procedimento | Esperado | Observado | Resultado | Artefato |
|---|---|---|---|---|---|---|
| RUN-01 | Dependências fixadas | `dotnet restore CemgSpec.slnx --locked-mode` | Restore sem mudar locks | Exit 0, locks intactos | PASSOU | logs/T016/restore.txt |
| RUN-02 | Fundação, fronteiras, US1-AC01–AC04, regressão dos achados | `dotnet test CemgSpec.slnx --no-restore --logger 'trx;LogFilePrefix=T017' --results-directory specs/001-auditar-projetos-speckit/evidence/logs/T017/results` | Todas as asserções passam | Exit 0; 7 domínio +14 Windows +31 UI =52; zero falhas/ignorados | PASSOU | logs/T017/full.txt; results/*.trx; harness/*.json |
| RUN-03 | Sanitização e rastreabilidade | `python -m unittest discover -s tools -p test_maintain_evidence.py -v` (Python local 3.11) | 6 testes passam | Exit 0, 6/6 | PASSOU | logs/T017/maintenance-tests.txt |
| RUN-04 | Dependências transitivas | `dotnet list CemgSpec.slnx package --vulnerable --include-transitive --no-restore --format json` | Consulta conclui e não reporta vulneráveis | Exit 0; nenhum pacote vulnerável reportado | PASSOU | logs/T017/packages.json ou alias no manifesto de manutenção |

## Execução real do produto e efeitos

Meio BIBLIOTECA/INTEGRACAO, com o comando RUN-02. Os quatro US1IntegratedHarnessTests acionam o NativeFolderPicker real por mensagens Win32 nas próprias janelas, ProjectService, broker e monitor sobre filesystem Windows real. Entradas: duas features SpecKit, uma tasks ausente, origem e destino externos temporários; tentativas de destino igual/interno/junction; troca de projeto/destino com histórico sintético; leitura documental em andamento e edição externa.

Asserções observadas: descoberta e ausência documental corretas; rejeições de fronteira; baselines da origem preservados; relatórios não gerados; contextos separados e histórico com caminho/hash preservado; alteração externa conservada, versão Changed e cancelamento do trabalho próprio. Sete testes de correção cobrem metadados de diretórios e limites Open/Refresh com recuperação. Outros testes verificam sessões, catálogo/broker, identidade/reparse, canais e limites de execução. O teste da página inicial usa host loopback e Edge reais. Evidência estruturada por cenário em logs/T017/harness; TRX identifica cada teste.

## Estado intermediário seguro e limitações

Inspeção da revisão: Program restringe loopback/Host/Origin e sessão; broker abre handles para leitura e revalida identidade/versão. VerificationType permanece fechado em AnalyzeDocuments e CheckGate. Botões Executar verificação e Salvar resultados estão desabilitados. Não há uso de agentes reais nem salvamento habilitado. Runner exercitado apenas com comandos confiáveis de teste. Proteção readonly pela aplicação, sem promessa de isolamento pelo sistema operacional.

Não executados: passeio manual completo da GUI carregada e funcionalidades de incrementos futuros; fora do contrato de T017. Handlers de limites usam reflexão, picker controlado e filesystem real. Execução/aceite do DEV, aprovação humana e fechamento do PR continuam pendentes. Auditoria NuGet não cobre SDK, Windows ou navegador. Roteiro reproduzível: ../validation/US1-dev.md; campos humanos preservados.

## Rodadas e passagem de revisão

Esta rodada executa novamente a suíte sobre o candidato corrente. Não houve correção de fonte nesta task; evidências históricas negativas T014 e red T015 preservadas. Manifesto de código conferido sem divergências. AUTORREVISAO T017 da base, evidência, efeitos e controles sem novo impeditivo encontrado. Isso não substitui a revisão completa do incremento prevista na T018. Gate geral permanece BLOCKED por T018–T020 e decisões humanas, sem invalidar a validação técnica aqui registrada.


## Rodada T019 — correção e revalidação vigente

Estado técnico: APROVADA em 2026-11-17, America/Sao_Paulo. Executor IA / speckit-implement. Base local `0bf951ec4704e118fd80f63e59cc6819341fc93a` + alterações T018/T019; fonte exata `sha256:fbdf9dd12adb61c19c2eee94ffbd7ba6c971a3cd013b2c6946b85ec6abc79b40` no manifesto corrente. Base incremental PR-A permanece `79d9c3d08d75afe2fef7e233493b8648e61ed4e0`; predecessores nenhum. As seções T017 acima são históricas. Correção: DocumentCatalog normaliza caminho completo/relativo e finaliza hashes mesmo sem specs; Projects consulta caminho exato do artefato raiz, com comparação sem diferença de caixa. Nenhuma redução dos controles de identidade, hardlink/reparse ou readonly.

Todos os comandos abaixo foram executados na raiz `__REPO_ROOT__`, Windows/SDK10.0.404, fixtures sintéticas novas.

| Execução | Comando | Esperado / observado | Resultado / evidência |
|---|---|---|---|
| Red dos achados | `dotnet test CemgSpec.slnx --no-restore --filter 'FullyQualifiedName~PrACorrectionTests\|FullyQualifiedName~PrAArtifactPresenceTests' --logger 'trx;LogFilePrefix=T019-red' --results-directory specs/001-auditar-projetos-speckit/evidence/logs/T019/red` | Cinco falhas comportamentais antes da correção; exit1 | FALHOU esperado; logs/T019/red.txt e red/*.trx |
| Green | Mesmo filtro, prefixo T019-green e diretório logs/T019/green | Cinco casos passam após corrigir; exit0 | PASSOU; logs/T019/green.txt e green/*.trx |
| Restore | `dotnet restore CemgSpec.slnx --locked-mode` | Exit0, locks intactos | PASSOU; logs/T019/restore.txt ou alias no manifesto |
| Regressão | `dotnet test CemgSpec.slnx --no-restore --logger 'trx;LogFilePrefix=T019-full' --results-directory specs/001-auditar-projetos-speckit/evidence/logs/T019/full` | 57/57 (7 domínio,16 Windows,34 UI), zero falhas/ignorados; exit0 | PASSOU; logs/T019/full.txt e full/*.trx |
| Replay cenários | `dotnet run --project specs/001-auditar-projetos-speckit/evidence/T019-probes/T019ReviewProbe.csproj` | Constituição reconhecida e raiz ausente corretamente identificada; dois checks true; exit0 | PASSOU; logs/T019/probes.txt |
| Manutenção | `python -m unittest discover -s tools -p test_maintain_evidence.py -v` | 6/6, exit0 | PASSOU; logs/T019/maintenance-tests.txt ou alias |
| Dependências | `dotnet list CemgSpec.slnx package --vulnerable --include-transitive --no-restore --format json` | Exit0, nenhum pacote vulnerável reportado | PASSOU; logs/T017/packages.json ou alias; não audita SO/SDK/browser |

A barra antes de `|` no filtro da tabela é escape Markdown; o filtro executado usa `|` literal. Green repete exatamente o filtro da rodada red. Comandos concretos preservados nos logs/TRX. Fonte e saída negativas T018 não foram sobrescritas. O replay T019 atualiza o predicado copiado para a consulta corrigida; os três testes permanentes renderizam Projects pelo HtmlRenderer real, com catálogo/serviço/filesystem reais e picker controlado, sem alegar navegador carregado.

Constituição: dois casos com/sem specs validam existência, bytes/hash e rejeição Changed após edição de mesmo tamanho/timestamp, preservando a edição e o snapshot anterior. Artefatos: três casos spec/plan/tasks verificam HTML Ausente com homônimo em evidence e Presente com nome raiz em maiúsculas. Baselines e destinos vazios verificados. Harness completo mantém picker nativo, broker/monitor e host/Edge iniciais reais, contextos/histórico e invalidação AC01–AC04.

Revisão das correções AUTORREVISAO em PR-A-review.md e T019-findings.json, zero impeditivos conhecidos abertos. Roteiro US1-dev.md renovado para57 casos e fonte corrente; campos humanos intactos. T020 e aprovação/execução/aceite humanos pendentes; não autoriza integração.


## Rodada vigente — falha relatada no teste manual de projetos

Fonte `sha256:f6e296be248c77cb91dd07dbd4236c955755ff03ea63fd159278112bac008d58`; correção local posterior ao commit0fa5428. Reabertura pela identidade da origem reutiliza contexto; fluxo nativo concorrente recusado; prompts1/2 e orientação da página esclarecidos. Red2 antes da correção, regressão59/59 depois, zero falhas/ignorados. Primeira tentativa de build bloqueada pelo executável aberto; usuário encerrou a própria instância e repetição passou. Detalhes em [review da correção](manual-project-opening-review.json), [execução](manual-project-opening-test-run.json) e [produto](manual-project-opening-product.json). Modalidade AUTORREVISAO; sem novo impeditivo conhecido no escopo, reteste manual/aceite humano pendentes. Seções T019 anteriores são históricas; nenhum resultado humano concedido.


## Estado vigente — fluxo explícito de seleção

Fonte `sha256:89a4535be36e2b206ac62881eeb8f7eabbcc0346dd26a2f16c0e060e1eb1f70f`;61 testes aprovados, mas demonstração integral navegador+seletor nativo exigida pelo usuário permanece BLOQUEADA por falha de inicialização da ferramenta desktop. [Validação e limites](selection-flow-validation.md). Rodadas anteriores são históricas; não declarar prontidão por61 testes passarem.
