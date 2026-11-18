# Code review — PR-A / T018

Estado: BLOQUEADA para fechamento do PR-A; passagem de revisão executada, dois impeditivos abertos.
Revisor: IA / speckit-implement. Modalidade: AUTORREVISAO, não independente.
Data: 2026-11-17T22:01:16.888283-03:00, America/Sao_Paulo.
Base resolvida do incremento: `79d9c3d08d75afe2fef7e233493b8648e61ed4e0`. Base de código pré-implementação: `197548db2e3efa20c23e1f66b6a0ffb86c0f328d`.
Versão revisada: `0bf951ec4704e118fd80f63e59cc6819341fc93a`; fonte `sha256:530031e68a790d3b2b186300c23441091ef951d6fb93fdc1bae4029b11b451f8`. Alterações locais desta task: probes e evidências/acompanhamento; nenhum código de produto corrigido.
Diff estável: `git diff --binary 79d9c3d08d75afe2fef7e233493b8648e61ed4e0 0bf951ec4704e118fd80f63e59cc6819341fc93a`; SHA256 e inventário de caminhos em T018-review-context.json. PR previsto PR-A, sem URL e sem predecessor. Escopo INCREMENTO: T001–T017 e migração T095–T098, US1-AC01–AC04 e fundação.

## Análise dos critérios e do incremento

| Critério/área | Código relevante | Teste/evidência consultada | Conclusão e limites |
|---|---|---|---|
| US1-AC01, catálogo | Infrastructure/Documents/DocumentCatalog.cs:33,69,77; Components/Pages/Projects.razor:33 | ReaderBoundaryTests, ProjectSelectionTests, US1IntegratedHarnessTests; probes T018 | Duas features funcionam na baseline, mas constituição omitida e presença de raiz incorreta; achados abaixo |
| US1-AC02, fronteiras readonly | WindowsReadHandle.cs:18; DocumentBroker.cs:12; ProjectService.cs:97 | ReaderBoundaryTests, S02, PR-A-validation.md | Handles de leitura, identidade/contenção e destino resolvido; sem garantia de sandbox OS |
| US1-AC03, contexto/histórico | Application/ProjectService.cs:61; Domain/ProjectSession.cs:9 | SessionContractTests, ProjectSelectionTests, S03 | Contextos por sessão/projeto e troca de destino sem mover histórico; Save futuro indisponível |
| US1-AC04, versão/edição externa | DocumentChangeMonitor.cs:15; ProjectService.cs:72; DocumentBroker.cs:12 | S04, ProjectCorrectionTests, T015-findings.json | Edição externa preservada, invalidação/cancelamento e limites exercitados; sem analisar agente real |
| Sessão e host | Program.cs:8–65; LocalSessionService.cs:6 | LocalSessionHttpTests, HostBaselineTests | Listener/Host/Origin loopback, sessão emitida pelo host, antiforgery e logs sem conteúdo; não autenticação humana |
| Fundação de execução/capacidades | TrustedCommandRunner.cs:13–75; ExecutionChannel.cs:5; CapabilityRegistry.cs:5; ExecutionLimits.cs:8 | TrustedCommandRunnerTests, ExecutionContractTests, CapabilityContractTests | Comandos internos confiáveis, hash, cwd externo, limites/cancelamento no cenário testado; sem prova de todos os padrões de descendentes e sem adaptadores habilitados |
| Integração/scaffold/ferramentas | SpecAudit.slnx, global.json, App.razor, MainLayout.razor, app.css, tools/maintain_evidence.py | T017 TRX, captura histórica da página inicial, testes do sanitizador e manifesto | SDK/locks fixados, host e Edge reais na baseline; screenshot inicial não prova todos os fluxos carregados |
| Governança/migração/evidências | Constituição4.1.0, contratos/adendos v4, tasks, US1-review, PR-A-validation, validation/US1-dev, acceptance/US1 | Git incremental, manifestos e estado do gate | Experimentos OS antigos permanecem históricos; execução/decisão humana em branco; falhas históricas preservadas |

Referências de código acima são relativas à raiz do repositório (pastas Documents/Execution sob SpecAudit/Infrastructure e demais sob SpecAudit). Revisão leu o código corrente e diff/contexto das áreas listadas; arquivos históricos de evidência foram conferidos por índices, revisões, manifestos e referências pertinentes, sem alegar auditoria manual integral de cada byte dos 484 caminhos do diff. Hash não prova suficiência ou autenticidade humana.

## Achados

| ID | Prioridade/impacto | Arquivo/linha | Problema e evidência | Correção proposta | Situação | Revalidação |
|---|---|---|---|---|---|---|
| REV-PRA-01 | P2, impeditivo: documento obrigatório omitido | SpecAudit/Infrastructure/Documents/DocumentCatalog.cs:33 (comparação:77) | A chamada usa `.specify/memory/constitution.md`. Path.Combine mantém barras internas; FinalPath retorna barras Windows. A comparação textual rejeita o mesmo arquivo. Probe cria constituição real: fileExists=true, catalogCount=0, constitutionRecognized=false. | Normalizar caminhos antes da comparação sem relaxar identidade/contenção; testar constituição lida/hash na origem com e sem specs. | ABERTO | T019 pendente |
| REV-PRA-02 | P2, impeditivo: presença documental falsa | SpecAudit/Components/Pages/Projects.razor:33 | A tabela compara só basename + FeatureId. Com spec raiz ausente e evidence/spec.md existente, o predicado real devolve shownPresent=true contra actualPresent=false. Afeta igualmente plan/tasks homônimos aninhados. | Comparar artefato exato da raiz da feature, usando caminho/kind e comparação Windows apropriada; teste de colisões. | ABERTO | T019 pendente |

Reprodução: da raiz, `dotnet run --project specs/001-auditar-projetos-speckit/evidence/T018-probes/T018ReviewProbe.csproj`. Ambiente Windows/.NET10. Dados novos em TEMP por GUID; somente fixtures próprias criadas; nenhuma solução real carregada. Resultado exigido: dois checks verdadeiros, exit0. Observado: dois falsos, exit1; logs/T018/probes.txt. O segundo probe avalia o mesmo predicado da página sobre catálogo real, sem afirmar renderização completa em navegador. Fonte do probe preservada; bin/obj ignorados. Calibração inicial e ajuste do probe descritos em T018-review-context.json.

## Conclusão e demonstração funcional

Dois impeditivos abertos; não aprovar fechamento/integração. A baseline T017 52/52 +6/6 é válida na mesma fonte conferida pelo manifesto, porém não detectava os dois casos novos. Não foi repetida nesta task documental; probes foram efetivamente executados e falharam. Demonstração anterior usa picker nativo, APIs/IO/watchers reais e host/Edge na página inicial; handlers de limites usam reflexão/picker controlado. Roteiro do DEV permanece disponível, com execução e aceite humanos pendentes.

T014/T015 e evidências T017 permanecem históricas e não foram convertidas em aprovação dos novos casos. T018 registra review e encaminha correção/revalidação à T019; nenhuma correção implementada aqui. Após corrigir: testes permanentes significativos, replay dos probes, suíte pertinente e renovação dos manifestos/revisão. T020 e aprovação humana continuam pendentes. Hooks extensions.yml ausente; nenhum hook obrigatório identificado. Sem PR, push, merge ou aceite concedido.


## Rodada T019 — conclusão vigente após correções

Estado da revisão técnica: CONCLUIDA, modalidade AUTORREVISAO em2026-11-17, America/Sao_Paulo; nenhuma revisão independente. Os estados BLOQUEADA/ABERTO acima descrevem a rodada T018 preservada. Base PR-A e base pré-implementação mantidas. Candidato local: `0bf951ec4704e118fd80f63e59cc6819341fc93a` + diff T019, fonte `sha256:fbdf9dd12adb61c19c2eee94ffbd7ba6c971a3cd013b2c6946b85ec6abc79b40` no manifesto corrente; contexto T018 não foi reescrito.

| Achado | Correção conferida | Revalidação | Situação vigente |
|---|---|---|---|
| REV-PRA-01 | DocumentCatalog.cs:68 normaliza full/relative; Build não sai antes de hashear constituição quando specs falta; comparação handle/identidade/reparse preservada | Dois testes reais com/sem specs: catálogo/hash/read; edição com metadados preservados rejeitada; probe positivo | RESOLVIDO |
| REV-PRA-02 | Projects.razor:33 compara caminho relativo exato da raiz selecionada com OrdinalIgnoreCase | Três testes renderizam componente real: homônimo aninhado não preenche raiz, raiz uppercase reconhecida; probe atualizado | RESOLVIDO |

Red5 antes da correção, green5, full57, manutenção6 e probes2 aprovados. Falhas anteriores preservadas. Revisão renovada do diff de correção, integração com baseline PR-A, testes e controles readonly não identificou outro impeditivo no escopo examinado. Mudança restrita a dois arquivos do produto e dois testes permanentes, sem dependências novas. Manifesto/diff e logs permitem conferir a versão; hash não autentica execução humana.

A correção dos dois achados está tecnicamente validada. Não há impeditivos conhecidos abertos nesta revisão; prontidão para integração normal permanece bloqueada por T020 e decisões humanas. Limites anteriores mantidos: não é prova de todo padrão de processo descendente, passeio completo GUI, agentes ou Save futuros. Próxima T020; nenhum PR/commit/push/merge/aceite concedido.
