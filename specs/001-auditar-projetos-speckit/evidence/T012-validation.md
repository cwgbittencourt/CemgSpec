# Validação T012 — US1-AC04 / INCREMENTO

Executor: IA, Windows x64, SDK 10.0.404, 2026-11-16. Base 197548db2e3efa20c23e1f66b6a0ffb86c0f328d + alterações locais identificadas em implementation-code-manifest.json; fontes incrementais em T012-code/. Sem aceitação da história.

| Rodada | Comando na raiz do repositório | Esperado/observado | Resultado e evidência |
|---|---|---|---|
| Red | dotnet test tests/SpecAudit.Ui.Tests --no-restore --filter FullyQualifiedName~UncataloguedMarkdown | Demonstrar invalidação indevida; observado pending=true | FALHOU conforme esperado antes da correção; logs/T012-red.txt |
| Suíte inicial | dotnet test SpecAudit.slnx --no-restore | 39 testes aprovados | PASSOU; logs/T012-suite-1.txt |
| Autorrevisão | Suíte após cenário de diretório com ponto | 40 testes aprovados | PASSOU; logs/T012-suite-final.txt |
| Novas funcionalidades | Suíte após sincronização de funcionalidades | Refresh coincidiu com entrega de eventos e retornou Changed; teste ainda supunha prazo fixo | FALHOU; logs/T012-suite-verified.txt; proteção de produção preservada |
| Final | dotnet test SpecAudit.slnx --no-restore --logger 'trx;LogFilePrefix=T012-complete' --results-directory specs/001-auditar-projetos-speckit/evidence/logs/T012-complete-results | 41 aprovados, 0 falhos/ignorados, asserções de efeitos preservadas | PASSOU; logs/T012-suite-complete.txt e TRX |
| Roteiro DEV | dotnet test tests/SpecAudit.Ui.Tests --no-restore --filter FullyQualifiedName~ProjectSelectionTests --logger "console;verbosity=normal" | 12 aprovados com arquivos Windows reais | PASSOU; logs/T012-manual-harness.txt |
| Roteiro DEV | dotnet test tests/SpecAudit.Windows.Tests --no-restore --filter FullyQualifiedName~ExternalEditDuringRead --logger "console;verbosity=normal" | 1 aprovado, snapshot não misturado e edição mantida | PASSOU; logs/T012-manual-reader.txt |

Meio real: API de biblioteca/serviço (ProjectService, catálogo e broker) acionada pelo harness, com FileSystemWatcher/IO Windows reais e fixtures exclusivas. Picker controlado, barreira determinística para edição concorrente; não simula leitura/escrita. Regressão também inicia host/Edge, mas não opera o diálogo nativo. Não foram implementados/executados Prepare/Execute/Save completos. Fronteiras de versão genéricas e cancelamento estão comprovados; integrações finais seguem pendentes.

Gate: python .specify/presets/cemg-speckit/tools/check_gate.py --feature specs/001-auditar-projetos-speckit --record verification/implementation-stage.json --stage pr --revision <revision do registro>. Resultado BLOCKED por dependências da história e aprovação humana ainda pendentes, sem erro de hash/evidência. Primeira chamada com caminho incorreto preservada em logs/T012-gate-path-error.txt; corrigida em logs/T012-gate.txt. Não é falha dos testes nem concessão de prontidão de PR.

Roteiro: ../validation/T012-dev.md. Limpeza: testes encerram os próprios processos/handles e preservam fixtures TEMP para inspeção; novas execuções criam GUID. Nenhuma ação de limpeza foi aplicada a pastas do usuário. Sem hooks antes/depois: extensions.yml ausente. Revisão: T012-review.md (AUTORREVISAO). Próxima tarefa T013 não iniciada.
