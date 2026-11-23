# T066 — Prova Real Individual `AnalyzeCodex`: PASSED

**Data**: 2026-11-20 (America/Sao_Paulo) | **Executor**: IA | **Ambiente**: Windows x64, .NET `10.0.15`.  
**Agente avaliado**: OpenAI Codex CLI `0.147.0` (`%APPDATA%\npm\node_modules\@openai\codex\...\codex.exe`).  
**SHA-256 do executável `codex.exe`**: `935a1911ed2556e4ffcec995f4886ac2ac425863ba26fed264df62e30272ad9d`.  
**Política**: Constituição `4.0.0` / `4.1.0` (`v4`), `cemg-speckit 3.0.0`.

> Apenas fixtures documentais sintéticas temporárias foram utilizadas; nenhuma solução real foi analisada, nenhuma permissão foi ampliada e nenhuma escrita ocorreu na origem ou no destino externo.

## Comando Executado (filtro explícito `Provider=Codex|Provider=Contract`)

```powershell
dotnet test tests\SpecAudit.Windows.Tests\SpecAudit.Windows.Tests.csproj --filter "FullyQualifiedName~AnalyzeProtectionTests&(Provider=Codex|Provider=Contract)" --verbosity normal
```

- **Resultado**: **2/2 testes aprovados**, exit code `0` (`35,15 s`).
- **Log completo**: [analyze-codex-proof.txt](logs/T066/analyze-codex-proof.txt).

## Matriz dos 5 Checks Obrigatórios do Selo `AnalyzeCodex`

| Check (`CapabilityRegistry`) | Teste comprobatório | Resultado observado |
|---|---|---|
| `real-analysis` | `AnalyzeProtectionTests.S02AndS04CodexAdapterProvesDocumentaryContractExternalStateAndCancellation` | **PASS** — Execução real autenticada do `codex.exe` (`0.147.0`) sobre a funcionalidade `001-demo` (com foco em `FR-002`) identificou a lacuna documental de `FR-002` ausente em `tasks.md`, retornando JSON estruturado válido (`RunState.Completed`, `AnalyzeSemanticResult.Findings`) com referências exatas de `DocumentId` e `ContentHash` dos 4 snapshots autorizados. |
| `document-contract` | `AnalyzeProtectionTests.S04AndS05AnalyzeBlocksMissingPrerequisitesWithoutCreatingFilesAndRejectsCodeOrSecrets`, `S02AndS04CodexAdapterProvesDocumentaryContractExternalStateAndCancellation` | **PASS** — Apenas os 4 snapshots documentais (`constitution.md`, `spec.md`, `plan.md`, `tasks.md`) são lidos pelo `DocumentBroker` via `ExecutionChannel`; ausência de `tasks.md` em `002-other` bloqueia em `Preparing → Blocked` sem criar arquivos; `DoNotRead.cs` e segredo sintético `.env` (`NEVER_READ_ME_12345`) jamais são lidos ou expostos; `DocumentId` inventado é rejeitado sem acesso a disco e rebaixado para `Inconclusive`. |
| `configured-tools-and-disclosure` | `AnalyzeProtectionTests.S02AndS04CodexAdapterProvesDocumentaryContractExternalStateAndCancellation` | **PASS** — `codex.exe exec` iniciado com `cwd` externo vazio, `--sandbox read-only`, `--skip-git-repo-check`, `--ignore-user-config`, `--ignore-rules`, `--ephemeral`, `-c mcp_servers={}`, `-c tools.web_search=false`, `--output-schema`, `UseShellExecute=false`; resultado divulga adaptação somente leitura `speckit-analyze`, versão/hash do agente e envio ao serviço do provedor. |
| `external-state` | `AnalyzeProtectionTests.S02AndS04CodexAdapterProvesDocumentaryContractExternalStateAndCancellation` | **PASS** — Estado interno do agente (`~/.codex`) informado nas limitações e mantido exclusivamente fora da origem auditada; origem 100% intacta (`fixture.IsUnchanged() == true`) e destino de relatórios vazio (`0` arquivos antes de `Salvar`). |
| `descendants-cancel` | `AnalyzeProtectionTests.S02AndS04CodexAdapterProvesDocumentaryContractExternalStateAndCancellation`, `S04AndS05AnalyzeBlocksMissingPrerequisitesWithoutCreatingFilesAndRejectsCodeOrSecrets` | **PASS** — Cancelamento de execução real do `codex.exe` em andamento encerrou a árvore de processos (`TerminationConfirmed = true`), transicionou para `RunState.Cancelled` (`SemanticResult = null`) e manteve origem e destino intactos. |

## Conclusão e Selo Técnico

- **Selo `AnalyzeCodex`: `Passed`** para `Version = "0.147.0"`, `ExecutableSha256 = "935a1911ed2556e4ffcec995f4886ac2ac425863ba26fed264df62e30272ad9d"`, `PlatformFingerprint = "win-x64"`, `PolicyVersion = "v4"`.
- Libera o prosseguimento para **T067** (habilitação condicional de `Codex` em `AgentCompatibilityService.cs` e `AgentSelector.razor`). `Antigravity` permanece desabilitado até `T068–T070`.
