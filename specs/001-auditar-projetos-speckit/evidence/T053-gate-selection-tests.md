# Evidência — T053 (Testes de contrato do gate e seleção fechada de verificações)

**Data**: 2026-11-20 (America/Sao_Paulo) | **Base**: `main` (`524f2f7`) | **Task**: `T053` (`US5-AC03/05/06/07`).

## Artefatos criados

- [GateAdapterTests.cs](../../../tests/CemgSpec.Windows.Tests/GateAdapterTests.cs) (`4` cenários de contrato para domínio, pacote CEMG 3.0.0, `READY`/`BLOCKED`/`DEV_IMPOSSIBILITY_RECORDED`, hash/revisão inválidos, separação entre disponibilidade/resultado/erro/cancelamento, e bloqueio offline de `.cs`, logs/artifacts aninhados e TOCTOU).
- [VerificationSelectionTests.cs](../../../tests/CemgSpec.Ui.Tests/VerificationSelectionTests.cs) (`3` cenários E2E Playwright contra o host real para combo fechada de 2 opções sincronizada entre `Verificações` e `Ficha documental do requisito`, substituições visuais da V1 conforme [design/mockups/README.md](../design/mockups/README.md) e [verificacoes-pasta-externa.png](../design/mockups/verificacoes-pasta-externa.png), `speckit-analyze` desabilitado sem prova, `check_gate` offline sem agente, `BLOCKED`/escopo negado e invalidação `Desatualizado` em edição concorrente).

## Execução observada (RED esperado + regressão verde)

- **RED esperado (`T053`)**:
  - Comando: `dotnet test CemgSpec.slnx --filter "FullyQualifiedName~GateAdapterTests|FullyQualifiedName~VerificationSelectionTests" --verbosity normal`
  - Resultado: `7/7` falhas esperadas (`4/4` em `GateAdapterTests` por ausência dos contratos de `T054–T056`; `3/3` em `VerificationSelectionTests` por ausência da combo e controles de `T058`).
  - Log completo: [t053-red.txt](logs/T053/t053-red.txt).
- **Regressão de base**:
  - Resultado: **95/95 aprovados** (`20` `CemgSpec.Tests`, `16` `CemgSpec.Windows.Tests`, `59` `CemgSpec.Ui.Tests`).
  - Log completo: [regression.txt](logs/T053/regression.txt).

## Preparação de comparação visual (V1)

- Referência: [verificacoes-pasta-externa.png](../design/mockups/verificacoes-pasta-externa.png) e [design/mockups/README.md](../design/mockups/README.md).
- `VerificationSelectionTests` já exige as substituições obrigatórias da V1 (combo fechada com exatamente 2 opções, sem campo livre de comando, sem `npm test`, sem `Cópia isolada`, sem `Validação aprovada`, sem salvamento automático) e inclui gancho `CEMGSPEC_CAPTURE_DIR` para capturas de tela em `T058`.
