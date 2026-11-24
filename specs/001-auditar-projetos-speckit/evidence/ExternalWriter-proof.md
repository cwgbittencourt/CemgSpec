# Prova Real — ExternalWriter (T082)

- **Feature**: `001-auditar-projetos-speckit`
- **Capacidade protegida**: `ExternalWriter` (`1.0.0` / `win-x64` / `v4`)
- **Data (UTC)**: `2026-11-20T18:27:54Z`
- **Estado do Assessment**: `Passed`
- **Log de execução**: `evidence/logs/T082/external-writer-proof.log`

## Comando executado

```powershell
$env:TEMP = "$env:LOCALAPPDATA\Temp"; $env:TMP = $env:TEMP
dotnet test tests/CemgSpec.Windows.Tests/CemgSpec.Windows.Tests.csproj --nologo -v normal --filter "FullyQualifiedName~ExternalWriterTests"
```

## Matriz de proteção comprovada

| Check obrigatório | Resultado | Evidência observada em disco |
| --- | --- | --- |
| `explicit-save` | PASS | Nenhuma gravação oficial ocorre ao abrir/auditar; pacotes só existem após invocação explícita de `ExternalReportWriter.Save`. |
| `external-canonical-path` | PASS | Origem e destino externo revalidados por `WindowsReadHandle` (`FinalPath` + `Identity`); destino contido na origem é recusado. |
| `create-new` | PASS | Diretório `CemgSpec-<utc>-<uuid>` e arquivos abertos com `FileMode.CreateNew`; segunda gravação com mesmo `SavedRecordId` retorna `RecordAlreadyExists`. `manifest.json` gravado por último com `schema_version=1` e `state="Complete"`. Fórmulas CSV (`=,+,-,@`) neutralizadas com apóstrofo inicial. |
| `reparse-toctou` | PASS | Reparse points no caminho de destino são recusados (`HasReparsePointOnPath`); recriação/troca do diretório externo após seleção altera a identidade Windows e retorna `DestinationIdentityChanged`. |
| `partial-failure` | PASS | Falha simulada de `IOException` ao gravar `report.json` retorna `SaveState.SaveFailed` (`PartialWriteFailure`), mantém o diretório parcial sem `manifest.json` para inspeção humana e não toca a origem. |
| `history` | PASS | `SavedHistoryReader` / `SavedHistoryService` listam apenas pacotes `CemgSpec-*` com validação SHA-256 contra `manifest.json`, sinalizam pacotes incompletos/adulterados, comparam `New`/`Persistent`/`Resolved` sem inferir resolução em amostra incompleta e excluem apenas o registro selecionado (`DeleteSaved`), preservando demais registros e a origem intacta (`fixture.IsUnchanged() == true`). |

## Resultados dos testes (`3/3` aprovados)

1. `S01AndS02ExternalWriterWritesCreateNewManifestLastWithSafeCsvAndNeverTouchesOrigin` — **Aprovado** (`74 ms`)
2. `S01AndS02ExternalWriterRejectsOriginDestinationJunctionSwapAndReportsPartialFailureHonestly` — **Aprovado** (`119 ms`)
3. `S04SavedHistoryReadsValidPackagesDiffsFindingsAndDeletesOnlySelectedRecord` — **Aprovado** (`106 ms`)

## Decisão técnica

Selo `ProtectedCapability.ExternalWriter` registrado em `ExternalReportWriter.TryRegisterVerifiedSeal(CapabilityRegistry)` como `Passed` para `1.0.0 / win-x64 / v4`, liberando a habilitação controlada do fluxo de salvamento na UI em **T084**.
