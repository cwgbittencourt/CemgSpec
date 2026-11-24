# Validação Técnica — US6: Sugestões, Save e Histórico (T085 / T087)

- **Feature**: `001-auditar-projetos-speckit`
- **História**: `US6` (Sugestões, decisões humanas, salvamento externo explícito e histórico)
- **Commit base avaliado**: `0caff61` (`main`)
- **Data (UTC)**: `2026-11-20T18:38:34Z`
- **Status técnico**: `PASS` (validação humana DEV pendente em `validation/US6-dev.md`)

## Comandos executados

```powershell
$env:TEMP = "$env:LOCALAPPDATA\Temp"; $env:TMP = $env:TEMP
dotnet test tests/CemgSpec.Tests/CemgSpec.Tests.csproj --nologo -v minimal
dotnet test tests/CemgSpec.Windows.Tests/CemgSpec.Windows.Tests.csproj --nologo -v minimal --filter "Provider!=Codex&Provider!=Antigravity"
dotnet test tests/CemgSpec.Ui.Tests/CemgSpec.Ui.Tests.csproj --nologo -v minimal --filter "FullyQualifiedName!~RealBrowserRendersLocalProjectSelectionWithoutCircuitErrors&FullyQualifiedName!~US1IntegratedHarnessTests" -- xunit.parallelizeTestCollections=false
```

## Resultados observados

| Suíte | Aprovados | Falhas | Duração |
| --- | ---: | ---: | ---: |
| `CemgSpec.Tests` (`FindingDecisionTests` incluído) | 25 | 0 | 194 ms |
| `CemgSpec.Windows.Tests` (`ExternalWriterTests` incluído) | 39 | 0 | 13 s |
| `CemgSpec.Ui.Tests` (`SaveHistoryTests` + `NavigationThemeTests` incluídos) | 64 | 0 | 2 m 37 s |
| **Total determinístico** | **128** | **0** | — |

## Cobertura dos cenários US6-S01–S04 / US6-AC01–AC04

- **US6-S01 / AC01 (Em memória até Salvar)**: Confirmado em `SaveHistoryTests.S01AndS02...` — nenhuma gravação ocorre ao abrir/auditar ou navegar para `/sugestoes`; a pasta de destino permanece vazia até a confirmação explícita em `SaveDialog`.
- **US6-S02 / AC02 (Exportação MD/JSON/CSV + `manifest.json` por último)**: Confirmado em `ExternalWriterTests.S01AndS02...` e `SaveHistoryTests.S01AndS02...` — criação exclusiva `CemgSpec-<utc>-<uuid>`, `manifest.json` (`schema_version=1`, `state="Complete"`) gravado por último, neutralização de fórmulas CSV (`=,+,-,@`), proteção contra reparse point/TOCTOU e falha parcial honesta sem apagar artefatos parciais.
- **US6-S03 / AC03 (Decisões humanas atribuíveis)**: Confirmado em `FindingDecisionTests.S03...` e `SaveHistoryTests.S03AndS04...` — `FalsePositive` e `AcceptedRisk` exigem `Justificativa` e `Responsável`; alteração de `ContextFingerprint` marca `NeedsReview` preservando o achado técnico original.
- **US6-S04 / AC04 (Histórico, comparação e exclusão seletiva)**: Confirmado em `ExternalWriterTests.S04...` e `SaveHistoryTests.S03AndS04...` — validação SHA-256 contra `manifest.json`, sinalização de pacotes incompletos/adulterados, comparação `New`/`Persistent`/`Resolved` sem inferir resolução em amostra incompleta e exclusão exclusiva do registro selecionado mantendo demais registros e a origem intactos.

## Achados pós-revisão (T087)

Nenhum achado bloqueante identificado na revisão real registrada em `evidence/US6-review.md`.
