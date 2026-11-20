# T057 — Prova de Proteção `GateAdapter`: PASSED

**Data**: 2026-11-20 (America/Sao_Paulo) | **Executor**: IA | **Ambiente**: Windows x64, .NET `10.0.15`, Python `3.12.7` (`C:\Users\cwagn\AppData\Local\Programs\Python\Python312\python.exe`).  
**Commit base**: `3a4b976` (`main`) | **Política**: Constituição `4.0.0` / `4.1.0` (`v4`), `cemg-speckit 3.0.0`.

> Apenas fixtures sintéticas temporárias foram utilizadas; nenhuma solução real, instalação automática, elevação ou alteração de ACL foi executada.

## Componentes e Hashes SHA-256 Verificados

| Componente | Caminho no repositório | SHA-256 |
|---|---|---|
| Core CEMG `3.0.0` | [check_gate.py](../../../SpecAudit/Infrastructure/Gates/Core/check_gate.py) | `7daf04483abbd52fa720e6afa0fbcde435120c96121b40008bff642ff93a42e0` |
| Política CEMG `3.0.0` | [gate-requirements.json](../../../SpecAudit/Infrastructure/Gates/Core/policies/gate-requirements.json) | `93eae48e3424949a889562eca47e8a1b93e398e5978374fbb75c8aa5f525c5a9` |
| Runner confiável | [runner.py](../../../SpecAudit/Infrastructure/Gates/runner.py) | `724e9ad9d9f2f99d85040e4d2bf849a2e7bc1c2e92da54c1ca9171709049021a` |
| `Path` de capacidade | [catalog_path.py](../../../SpecAudit/Infrastructure/Gates/catalog_path.py) | `4c5a3d584b3afd7cc79a3c001417286055eba213731592f2779435cf5d40b898` |

Log completo de hashes: [hashes.txt](logs/T057/hashes.txt).

## Comando Executado

```powershell
dotnet test tests\SpecAudit.Windows.Tests\SpecAudit.Windows.Tests.csproj --filter "FullyQualifiedName~GateAdapterTests|FullyQualifiedName~GatePackageTests" --verbosity normal
```

- **Resultado**: **7/7 testes aprovados**, exit code `0` (`5,34 s`).
- **Log completo**: [gate-adapter-proof.txt](logs/T057/gate-adapter-proof.txt).

## Matriz dos 6 Checks Obrigatórios do Selo `GateAdapter`

| Check (`CapabilityRegistry`) | Teste comprobatório | Resultado observado |
|---|---|---|
| `package-integrity` | `GatePackageTests.PackagedCoreAndPolicyMatchInstalledCemg300PresetByteForByte`, `GatePackageTests.RejectsTamperedCoreOrPolicyAndMissingOrOriginPythonRuntime`, `GatePackageTests.ResolvesVerifiedAbsolutePythonRuntimeOutsideOrigin` | **PASS** — Core e política empacotados idênticos byte a byte ao preset CEMG `3.0.0`; adulterações rejeitadas; ausência de Python reporta `Unavailable` sem instalar; runtime dentro da origem rejeitado; Python `3.12.7` absoluto conferido fora da origem. |
| `offline-no-shell` | `GateAdapterTests.S06DomainAndPackageExposeClosedStagesAndPinnedCemgCore`, `GateAdapterTests.S06ValidatesReadyBlockedDevImpossibilityAndMatchesOfficialCliEquivalence` | **PASS** — Processo iniciado com `-I -B`, `UseShellExecute=false`, `cwd` externo, ambiente mínimo sem `PYTHONPATH`, `subprocess = None` no módulo core, sem `--project`/`main()`/`active()`, sem IA e sem rede. |
| `core-equivalence` | `GateAdapterTests.S06ValidatesReadyBlockedDevImpossibilityAndMatchesOfficialCliEquivalence` | **PASS** — Equivalência exata de `result`, `errors` e `exit_code` entre o adaptador de capacidade (`GateExecutor` + `runner.py`) e a CLI oficial CEMG `3.0.0` em 7 cenários: `spec` (`READY`/0, `BLOCKED`/1, `bad-hash`/1, `wrong-revision`/1), `pr` (`READY`/0), `acceptance` (`READY`/0) e `dev-impossibility` (`DEV_IMPOSSIBILITY_RECORDED`/0). |
| `nested-evidence` | `GateAdapterTests.S07DeniesCodeReferenceNestedLogsOutsideArtifactsAndJunctionToctouOffline` | **PASS** — Grafo completo validado antes da execução (`evidence`, `test-run` `stdout`/`stderr`, `product-execution` `artifacts`, `human_decisions`, `human_confirmation`); `../../DoNotRead.cs`, `../../../outside.md` e junção `evidence/linked/secret.md` bloqueados no preflight (`RunState.Blocked`, `SemanticResult = null`) com zero leituras em alvos proibidos. |
| `read-time-toctou` | `GateAdapterTests.S07DeniesCodeReferenceNestedLogsOutsideArtifactsAndJunctionToctouOffline` | **PASS** — Mutação concorrente de `spec.md` injetada no instante exato anterior ao `DocumentBroker.Read` foi interceptada pelo broker; execução encerrada sem `Completed`/`READY` e marcada `Freshness.Stale`. |
| `cancel-zero-writes` | `GateAdapterTests.S05AndS06SeparateAvailabilityCoreResultErrorAndCancellation` | **PASS** — Cancelamento determinístico durante leitura do broker encerrou a árvore de processos (`TerminationConfirmed = true`), transicionou para `RunState.Cancelled` (`SemanticResult = null`), manteve a origem intacta (`IsUnchanged() == true`) e o destino externo vazio (`0` arquivos). |

## Limites Explícitos

- Este selo comprova apenas o adaptador offline `check_gate CEMG 3.0.0 / adaptador de leitura SpecAudit` para a versão/hashes/política `v4` acima. Qualquer alteração de core, política, runner ou runtime invalida o selo.
- `check_gate` valida integridade e estado registrados nos documentos; não autentica identidade humana nem executa testes da solução auditada.
- **Selo técnico `GateAdapter`: `Passed`** — libera o prosseguimento para **T058** (integração da combo fechada e habilitação condicional de `check_gate` na UI). `speckit-analyze` (`AnalyzeCodex`, `AnalyzeAntigravity`) e `Salvar` (`ExternalWriter`) permanecem desabilitados até suas provas próprias.
