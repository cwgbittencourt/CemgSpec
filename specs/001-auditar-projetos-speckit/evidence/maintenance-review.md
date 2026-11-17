# Organização e sanitização das evidências

Data: 2026-11-17, America/Sao_Paulo. Autorização: solicitação do usuário para organizar evidências, sanitizar caminhos pessoais, atualizar hashes e fazer commit. Modalidade: AUTORREVISAO por Codex. Base: `139fd81db66943bbc2e167ccd13a75b4a317a2fc` + trabalho T013 ainda não commitado.

## Resultado e escopo

- Oito cópias exatas consolidadas, liberando 117.026 bytes sem descartar resultados únicos. Capturas iguais compartilham a imagem T011; consultas de pacotes iguais e logs iguais apontam para arquivos canônicos. As referências foram ajustadas.
- Textos da feature, incluindo arquivos históricos, logs e TRX exportados, usam raízes simbólicas em vez de caminhos pessoais. Sessenta e cinco nomes de TRX passaram a usar `runner` em lugar do usuário/estação. Emails pessoais encontrados em dois logs antigos de autenticação foram substituídos por `__EMAIL__`; não eram chaves ou tokens. Não houve alteração dos arquivos de produção do SpecAudit.
- Cada transformação registra hashes anteriores/posteriores em `maintenance-manifest.json`. Hashes embutidos que correspondiam aos bytes transformados foram recalculados transitivamente até estabilizar. Hashes de revisões antigas sem os mesmos bytes de entrada permanecem históricos; não foi inventada revalidação semântica dessas revisões.
- Índice/política em [README.md](README.md); código e testes reutilizáveis em `tools/maintain_evidence.py` e `tools/test_maintain_evidence.py`. O harness T013 passou a emitir caminhos de fixture com `__TEMP_ROOT__` desde a geração. Logs/TRX do framework ainda exigem sanitização antes de versionar.
- Três mockups e a captura da página inicial conferidos visualmente, sem caminhos pessoais ou credenciais visíveis. Nenhuma imagem alterada. Nenhum novo pacote, CI ou hook instalado.

## Verificações reais e limites

| Rodada | Comando | Resultado observado | Evidência |
|---|---|---|---|
| Sanitizador | `python -m unittest discover -s tools -p test_maintain_evidence.py -v` | Seis testes passaram: JSON válido/emails omitidos, hashes transitivos, deduplicação, histórico preservado, metadados sem alterar palavras comuns e detecção de adulteração | logs/maintenance-20261117/maintenance-tests-sanitized.txt |
| Exportação | `python tools/maintain_evidence.py --apply` e `--check` | Transformação idempotente; segunda passagem sem mudanças pendentes | maintenance-manifest.json; maintenance-verification.json |
| Regressão inicial | `dotnet test SpecAudit.slnx --no-restore` com logger TRX | 44/45; uma falha no teste existente UncataloguedMarkdownAndCodeDoNotInvalidateDocuments | logs/maintenance-20261117/suite.txt; results/ |
| Investigação | Mesmo teste, cinco execuções isoladas com `--no-build --no-restore --filter FullyQualifiedName~UncataloguedMarkdownAndCodeDoNotInvalidateDocuments` | 5/5 passaram; falha inicial não reproduzida | logs/maintenance-20261117/watcher-retests/; watcher-retest-1.txt a watcher-retest-5.txt |
| Regressão final | `dotnet test SpecAudit.slnx --no-build --no-restore --logger 'trx;LogFilePrefix=maintenance-final' --results-directory specs/001-auditar-projetos-speckit/evidence/logs/maintenance-20261117/final-results` | 45/45 aprovados, zero falhos/ignorados; quatro cenários T013 com picker nativo/arquivos reais e host/Edge reais na regressão | logs/maintenance-20261117/final-suite.txt; final-results/ |
| Gate PR-A | check_gate.py com `verification/implementation-stage.json`, stage pr e revisão do manifesto corrente | BLOCKED esperado por review/fechamento/decisão humana pendentes; integridade e registros de execução conferidos | logs/T013-20261117/gate.txt |

Os logs exportados continuam representando essas execuções reais, com dados de máquina omitidos. A alteração nos testes C# é somente na serialização das evidências; as asserções funcionais permanecem as mesmas e a suíte foi reexecutada depois dela. A busca por credenciais usa padrões, não ferramenta especializada ou garantia absoluta. Os commits anteriores não foram reescritos; caminhos previamente versionados continuam no histórico.

## Achados e tratamento

- Durante a implementação do sanitizador, a revisão identificou que o hostname também era palavra comum em português. A regra foi restringida a campos de máquina e identificadores de execução; os trechos de prosa foram restaurados e os hashes novamente recalculados. Teste específico cobre esse risco. Nenhum resultado de teste foi convertido por substituição de texto.
- OBS-MAINT-01: a falha intermitente do monitor acima continua sem causa confirmada. O código trata certos eventos de diretório conservadoramente; os retestes não provam correção nem ausência de problema. A falha original permanece registrada e deve ser analisada na T014. Não foi modificada produção ou relaxada a asserção para obter aprovação. Review completo/PR/aceite permanecem pendentes, sem apagar esta observação porque a rodada final passou.
- Nenhuma autorização humana de PR, aceite ou deploy preenchida. Nenhuma mudança na origem auditada. A organização documental não transforma gates históricos em aprovação atual.

## Reprodução e rastreabilidade

Usar Python 3 confiável e a raiz retornada por `git rev-parse --show-toplevel`; Windows/.NET/Edge para a suíte. Verificar os arquivos do candidato em `maintenance-code-manifest.json` e os exports atuais em `maintenance-inventory.json`. A fonte do harness original T013 permanece em `T013-code/`, porque sua revisão difere apenas na emissão posterior de caminhos. `maintenance-test-run.json` e `maintenance-product-execution.json` vinculam a rodada atual ao novo manifesto.

Para conferir um registro histórico, usar a base declarada e os snapshots disponíveis, mais o encadeamento de hashes do manifesto de manutenção; não comparar uma revisão histórica diretamente com outro código atual. Não reexecutar experimentos antigos de isolamento ou scripts da origem para conferir a organização. A próxima task continua T014.
