# Evidências de validação — US5 (Verificações permitidas: `speckit-analyze` e `check_gate`)

Estado: APROVADA (técnica)
Executor/ferramenta: Antigravity · dotnet SDK 10.0.404 / runtime .NET 10.0.15 / Python 3.12.7 / Codex CLI `0.147.0` / Antigravity CLI `1.5.13` / Playwright 1.64.1 / Microsoft Edge headless
Data/hora e fuso: 2026-11-20 15:06 America/Sao_Paulo
Revisão candidata: `e399c67` (`main`)

## Execuções comprobatórias dos cenários `US5-S01–S07` / `US5-AC01–AC07`

| Cenário / Critério | Prova / Suíte | Resultado observado | Evidência |
|---|---|---|---|
| `US5-S01` (`US5-AC01`) | `AgentDiscoveryTests` (`3/3`) + `VerificationSelectionTests` | Descoberta automática de `Codex`, `Antigravity`, `ClaudeCode`, `Kiro`; `ClaudeCode`/`Kiro` `Unsupported`; fallback manual restrito a agente ausente fora da origem; `check_gate` não exige agente. | [PR-D/regression.txt](logs/PR-D/regression.txt) |
| `US5-S02` (`US5-AC02`) | Prova real `AnalyzeCodex` (`2/2`) + Prova real `AnalyzeAntigravity` (`9/9`) + `AgentDiscoveryTests.S02` | Ambos os agentes comprovaram análise documental real, contrato de 4 snapshots, ferramentas restritas, estado externo e cancelamento; invalidação de um selo bloqueia apenas o adaptador afetado. | [AnalyzeCodex-proof.md](AnalyzeCodex-proof.md) · [AnalyzeAntigravity-proof.md](AnalyzeAntigravity-proof.md) |
| `US5-S03` (`US5-AC03`) | `VerificationSelectionTests.S03` | Combo fechada com exatamente `speckit-analyze — Consistência documental` e `check_gate — Validar gates`, sincronizada por projeto entre `Verificações` e `Ficha documental do requisito`, sem campo livre e sem autoexecução. | [captures/T070/](captures/T070/) |
| `US5-S04` (`US5-AC04`) | `AnalyzeProtectionTests` (`Provider=Contract`, `Provider=Codex`, `Provider=Antigravity`, `Provider=AntigravityContract`) | Pré-requisitos ausentes (`tasks.md`) bloqueiam sem criar arquivos; instruções maliciosas, `DoNotRead.cs` e `.env` jamais são lidos; referências inventadas viram `Inconclusive`; estado externo informado; origem intacta. | [AnalyzeCodex-proof.md](AnalyzeCodex-proof.md) · [AnalyzeAntigravity-proof.md](AnalyzeAntigravity-proof.md) |
| `US5-S05` (`US5-AC05`) | `VerificationStateMachineTests` (`3/3`) + `AnalyzeProtectionTests` + `GateAdapterTests` | Falha de autenticação/quota, cancelamento e incompatibilidade nunca viram aprovação/reprovação de requisito; cancelamento confirma término da árvore de processos; leitor e `check_gate` permanecem operacionais. | [PR-D/regression.txt](logs/PR-D/regression.txt) |
| `US5-S06` (`US5-AC06`) | Prova `GateAdapter` (`7/7`) + `VerificationSelectionTests.S05/S06` | Equivalência exata com CLI oficial CEMG `3.0.0` em 7 casos (`spec` ready/blocked/bad-hash/wrong-revision, `pr`, `acceptance`, `dev-impossibility`); limites explícitos visíveis; ficha mantém alcance `funcionalidade/etapa`. | [GateAdapter-proof.md](GateAdapter-proof.md) |
| `US5-S07` (`US5-AC07`) | `GateAdapterTests.S07` + `VerificationSelectionTests.S06AndS07` | Gate offline sem IA/rede; `.cs`, logs/artifacts aninhados externos e junções bloqueados no preflight sem leitura; TOCTOU interceptado no ponto da leitura; edição concorrente marca `Desatualizado`. | [GateAdapter-proof.md](GateAdapter-proof.md) |

## Observação sobre cota externa na regressão completa (`task-1087`)

- Na execução integrada `task-1087`, **121/121** testes locais, de contrato, de fronteira Windows e E2E Playwright foram aprovados (`23/23` `SpecAudit.Tests`, `36/36` locais em `SpecAudit.Windows.Tests`, `62/62` `SpecAudit.Ui.Tests`).
- Os 2 testes de chamada em nuvem (`Provider=Codex` e `Provider=Antigravity`), que já haviam passado integralmente em `T066` (`AnalyzeCodex-proof.md`, `2/2`) e `T069` (`AnalyzeAntigravity-proof.md`, `9/9`), falharam na reexecução posterior por esgotamento externo de créditos da conta (`ERROR: Your workspace is out of credits`), confirmando adicionalmente que o adaptador trata erro externo de provedor como `RunState.Failed` sem fabricar diagnóstico nem afetar o leitor ou o `check_gate`.
