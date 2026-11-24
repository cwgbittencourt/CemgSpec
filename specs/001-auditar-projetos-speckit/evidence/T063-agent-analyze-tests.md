# Evidência — T063 (Testes de contrato de descoberta de agentes e proteção `AnalyzeDocuments`)

**Data**: 2026-11-20 (America/Sao_Paulo) | **Base**: `main` (`7b87505`) | **Task**: `T063` (`US5-AC01/02/04/05`).

## Artefatos criados

- [AgentDiscoveryTests.cs](../../../tests/CemgSpec.Windows.Tests/AgentDiscoveryTests.cs) (`2` cenários de contrato para `AgentProvider`, `AgentDiscoveryMethod`, `AgentCompatibilityState`, `AgentInstallation` e `AgentDiscovery`: separação entre descoberta e compatibilidade, `ClaudeCode`/`Kiro` como `Unsupported`, fallback manual restrito apenas a agente não encontrado fora da origem e independência de `check_gate`/leitor documental).
- [AnalyzeProtectionTests.cs](../../../tests/CemgSpec.Windows.Tests/AnalyzeProtectionTests.cs) (`3` cenários filtráveis por `[Trait("Provider", "Contract" | "Codex" | "Antigravity")]` para pré-requisitos `spec/plan/tasks` ausentes sem criar arquivos, instruções maliciosas/segredos sintéticos/código-fonte negados, estado externo e cancelamento de descendentes).

## Execução observada (RED esperado + regressão verde)

- **RED esperado (`T063`)**:
  - Comando: `dotnet test tests\CemgSpec.Windows.Tests\CemgSpec.Windows.Tests.csproj --filter "FullyQualifiedName~AgentDiscoveryTests|FullyQualifiedName~AnalyzeProtectionTests" --verbosity normal`
  - Resultado: **5/5 falhas RED esperadas** (`2/2` em `AgentDiscoveryTests`, `3/3` em `AnalyzeProtectionTests`).
  - Log completo: [t063-red.txt](logs/T063/t063-red.txt).
- **Regressão `CemgSpec.Windows.Tests`**:
  - Resultado: **23/23 aprovados**, exit code `0`.
