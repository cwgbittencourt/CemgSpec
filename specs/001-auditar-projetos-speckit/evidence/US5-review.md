# Code review — História US5 (T072–T073)

Estado do review: **CONCLUÍDO (técnico) — zero impeditivos abertos na revisão `e399c67`**.
Modalidade: **AUTORREVISAO**, Antigravity; revisão independente e aceite humano permanecem **PENDENTES**.
Data/hora e fuso: 2026-11-20 15:07 America/Sao_Paulo.

## Inspeção de código e evidências (`T053–T070`)

- **Escopo**: descoberta de agentes (`AgentInstallation.cs`, `AgentDiscovery.cs`), compatibilidade por selo individual (`AgentCompatibilityService.cs`), adaptadores documentais (`CodexAnalyzeAdapter.cs`, `AntigravityAnalyzeAdapter.cs`), gate offline (`GatePackage.cs`, `GateExecutor.cs`, `runner.py`, `catalog_path.py`) e UI compartilhada (`AgentSelector.razor`, `VerificationSelector.razor`, `Verifications.razor`, `RequirementSheet.razor`).
- **Achados resolvidos durante `PR-D` (`T063–T070`)**:
  - `REV-US5-01` (**P1** — `CemgSpec/Infrastructure/Execution/TrustedCommandRunner.cs:27`): `StandardInputEncoding` padrão no Windows usava CP850/CP1252, rejeitado pelo `codex.exe` em acentos UTF-8. **Resolvido** fixando `StandardInputEncoding = new UTF8Encoding(false)` e `StandardOutputEncoding`/`StandardErrorEncoding = Encoding.UTF8`.
  - `REV-US5-02` (**P2** — `CemgSpec/Infrastructure/Agents/CodexAnalyzeAdapter.cs:26,171`): `Encoding.UTF8` gravava BOM no arquivo `--output-schema` e incluía `$schema` rejeitado por Structured Outputs. **Resolvido** removendo `$schema` e gravando sem BOM.
  - `REV-US5-03` (**P2** — `CemgSpec/Infrastructure/Agents/AgentDiscovery.cs:250`, `AntigravityAnalyzeAdapter.cs:13,112,156,201`): descoberta não incluía `%USERPROFILE%\.gemini\bin\agy.exe`, `RestrictedSettings` incluía chave desconhecida `enableTelemetry`, `stream-json` rejeitava `step_type == "finish"` e cancelamento pré-execução marcava `terminationConfirmed: false`. **Resolvido** e revalidado em `T069` (`9/9` aprovados).
  - `REV-US5-04` (**P3** — `CemgSpec/Components/Verifications/AgentSelector.razor:21,93`): quebras de linha em `<option>` e assinatura duplicada em `AgentStore.Changed` causavam corrida de renderização em trocas consecutivas de agente. **Resolvido** e revalidado em `T067`/`T070`.
- **Invariantes preservados**: origem somente leitura, zero código-fonte aberto, zero relatórios oficiais gravados antes de `Salvar`, selos `GateAdapter`, `AnalyzeCodex` e `AnalyzeAntigravity` estritamente independentes.
- **T073**: todos os achados acima foram resolvidos e revalidados; nenhum impeditivo técnico aberto.
