# PR-D — Descoberta de agentes e `speckit-analyze` real por adaptador (US5 completo)

- **Estado de aprovação humana**: **PENDENTE** (este arquivo prepara a descrição revisável do PR; não abre, publica ou integra PR).
- **Predecessor**: PR-C ([PR-C.md](PR-C.md), base `7b87505`, aprovação humana pendente).
- **Revisão candidata**: `e399c67` (`main`).
- **Tasks cobertas**: `T063–T078` (`US5` completo: `US5-AC01` a `US5-AC07`).

## Escopo final entregue

1. **Descoberta e inventário de agentes (`T064`)**: [AgentInstallation.cs](../../../../SpecAudit/Domain/AgentInstallation.cs) e [AgentDiscovery.cs](../../../../SpecAudit/Infrastructure/Agents/AgentDiscovery.cs) para `Codex`, `Antigravity`, `Claude Code` e `Kiro`, com `Claude Code`/`Kiro` `Unsupported` na V1, locais externos de estado informados e fallback manual restrito a agente ausente fora da origem.
2. **Adaptador documental `Codex` e prova real `AnalyzeCodex` (`T065–T067`)**: [CodexAnalyzeAdapter.cs](../../../../SpecAudit/Infrastructure/Agents/CodexAnalyzeAdapter.cs), [AnalyzeCodex-proof.md](../../evidence/AnalyzeCodex-proof.md) (`2/2` aprovados com `codex.exe` `0.147.0`), [AgentCompatibilityService.cs](../../../../SpecAudit/Application/AgentCompatibilityService.cs) e [AgentSelector.razor](../../../../SpecAudit/Components/Verifications/AgentSelector.razor).
3. **Adaptador documental `Antigravity` e prova real `AnalyzeAntigravity` (`T068–T070`)**: [AntigravityAnalyzeAdapter.cs](../../../../SpecAudit/Infrastructure/Agents/AntigravityAnalyzeAdapter.cs) e [AnalyzeAntigravity-proof.md](../../evidence/AnalyzeAntigravity-proof.md) (`9/9` aprovados com `agy.exe` `1.5.13`), habilitado com selo independente sem afetar `Codex` ou `check_gate`.

## Evidências e roteiro do DEV

- Validação da história e do PR: [US5-validation.md](../../evidence/US5-validation.md) · [PR-D-validation.md](../../evidence/PR-D-validation.md)
- Autorrevisão da história e do PR: [US5-review.md](../../evidence/US5-review.md) · [PR-D-review.md](../../evidence/PR-D-review.md)
- Roteiro reproduzível e aceite técnico: [US5-dev.md](../../validation/US5-dev.md) · [acceptance/US5.md](../../acceptance/US5.md)

## Decisão humana (reservada ao revisor/DEV)

- Aprovação de PR: PENDENTE
- Integração: PENDENTE
- Aceite funcional da história US5: PENDENTE
