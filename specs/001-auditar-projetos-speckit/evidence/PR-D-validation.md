# Evidências de validação — PR-D (US5 completo: descoberta e `speckit-analyze` real por adaptador)

Estado: APROVADA (técnica)
Executor/ferramenta: Antigravity · dotnet SDK 10.0.404 / runtime .NET 10.0.15 / Python 3.12.7 / Codex CLI `0.147.0` / Antigravity CLI `1.5.13` / Playwright 1.64.1 / Microsoft Edge headless
Data/hora e fuso: 2026-11-20 15:08 America/Sao_Paulo
Base declarada / predecessor: PR-C (`7b87505`, aprovação humana pendente)
Revisão candidata PR-D: `e399c67` (`main`)

| Execução | Escopo | Resultado | Log / Evidência |
|---|---|---|---|
| PRD-PROOF-CODEX | Prova real individual `AnalyzeCodex` (`Provider=Codex\|Provider=Contract`) | **2/2 aprovados** (exit code `0`) | [AnalyzeCodex-proof.md](AnalyzeCodex-proof.md) |
| PRD-PROOF-AGY | Prova real individual `AnalyzeAntigravity` (`Provider=Antigravity\|Provider=AntigravityContract`) | **9/9 aprovados** (exit code `0`) | [AnalyzeAntigravity-proof.md](AnalyzeAntigravity-proof.md) |
| PRD-REG-01 | Regressão integrada da solução (`CemgSpec.Tests`, `CemgSpec.Windows.Tests`, `CemgSpec.Ui.Tests`) | **121/121** testes locais/contrato/UI aprovados (`23` `CemgSpec.Tests`, `36` locais `CemgSpec.Windows.Tests`, `62` `CemgSpec.Ui.Tests`); os 2 testes de chamada em nuvem já comprovados acima retornaram erro externo de cota (`out of credits`) na reexecução posterior. | [logs/PR-D/regression.txt](logs/PR-D/regression.txt) |

## Estado intermediário seguro preservado

- `ReaderBoundary`, `GateAdapter`, `AnalyzeCodex` e `AnalyzeAntigravity` operam com selos independentes; `Claude Code` e `Kiro` permanecem `Unsupported`.
- `Salvar resultados` em `/sugestoes` permanece **desabilitado** até `PR-E` (`ExternalWriter`).
- Origem auditada permanece somente leitura; zero relatórios gravados no destino externo.
