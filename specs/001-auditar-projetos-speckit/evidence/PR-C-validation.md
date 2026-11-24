# Evidências de validação — PR-C (US5 parcial: combo fechada e `check_gate` offline)

Estado: APROVADA (técnica)
Executor/ferramenta: Antigravity (Gemini 4 Argon) · dotnet SDK 10.0.404 / runtime .NET 10.0.15 / Python 3.12.7 / Playwright 1.64.1 / Microsoft Edge headless
Data/hora e fuso: 2026-11-20 10:15 America/Sao_Paulo
Base declarada / predecessor: PR-A (`ddf8f31` / `73e06b9`) e PR-B (`524f2f7`, aprovação humana pendente)
Revisão candidata PR-C: `a4c7199` + alinhamento de hash `CopyToOutputDirectory=Always` em `GatePackage.cs`/`CemgSpec.csproj`

| Execução | Escopo | Comando (raiz do repositório) | Resultado | Log |
|---|---|---|---|---|
| PRC-RUN-01 | Regressão completa PR-C (Fundação, US1, US2, US3, US4, US8 + US5 parcial: combo fechada e `check_gate` offline) | `$env:TEMP="$env:LOCALAPPDATA\Temp"; $env:TMP=$env:TEMP; $filtro='FullyQualifiedName!~RealBrowserRendersLocalProjectSelectionWithoutCircuitErrors&FullyQualifiedName!~US1IntegratedHarnessTests'; dotnet test CemgSpec.slnx --no-build --filter $filtro --verbosity minimal -- xunit.parallelizeTestCollections=false` | **108/108 aprovados** (`23` `CemgSpec.Tests`, `23` `CemgSpec.Windows.Tests`, `62` `CemgSpec.Ui.Tests`, exit code `0`) | [logs/PR-C/regression.txt](logs/PR-C/regression.txt) |

## Cobertura consolidada do incremento PR-C (`T053–T058`)

- **Contratos de domínio e máquina de estados (`T053–T054`)**: [T053-gate-selection-tests.md](T053-gate-selection-tests.md), `GateCandidate.cs`, `VerificationRun.cs`, `VerificationService.cs` e `VerificationStateMachineTests.cs` (`3/3` aprovados).
- **Empacotamento CEMG 3.0.0 e verificação de runtime Python (`T055`)**: `Core/check_gate.py`, `Core/policies/gate-requirements.json`, `GatePackage.cs` e `GatePackageTests.cs` (`3/3` aprovados).
- **Runner com `Path` de capacidade, preflight do grafo e prova `GateAdapter` (`T056–T057`)**: `catalog_path.py`, `runner.py`, `GateExecutor.cs`, `GateAdapterTests.cs` (`4/4` aprovados) e [GateAdapter-proof.md](GateAdapter-proof.md) (`7/7` aprovados).
- **Combo fechada compartilhada e UI de `Verificações` + `RequirementSheet` (`T058`)**: `VerificationSelector.razor`, `Verifications.razor`, `RequirementSheet.razor`, `VerificationSelectionTests.cs` (`3/3` aprovados) e capturas comparativas em [captures/T058/](captures/T058/) frente a [verificacoes-pasta-externa.png](../design/mockups/verificacoes-pasta-externa.png) e [design/mockups/README.md](../design/mockups/README.md).
- **Estado intermediário seguro**:
  - `speckit-analyze` permanece **desabilitado** (`Executar verificação` desabilitado quando selecionado) até as provas reais `AnalyzeCodex` e `AnalyzeAntigravity` no `PR-D`.
  - `Salvar resultados` em `/sugestoes` permanece **desabilitado** até a prova `ExternalWriter` no `PR-E`.
  - Origem auditada permanece integralmente somente leitura; nenhum arquivo gravado em destino externo; nenhum arquivo `.cs`/`.razor`/config lido.
