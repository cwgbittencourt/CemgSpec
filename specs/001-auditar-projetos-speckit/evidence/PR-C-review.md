# Code review — Incremento PR-C (T060–T061)

Estado do review: **CONCLUÍDO (técnico) — zero impeditivos abertos**.
Modalidade: **AUTORREVISAO**, Antigravity (Gemini 4 Argon); revisão independente e aprovação humana de PR permanecem **PENDENTES**.
Data/hora e fuso: 2026-11-20 10:16 America/Sao_Paulo.
Base / predecessor: PR-B (`524f2f7`).

## Inspeção do incremento completo (`T053–T058`)

- **Diff inspecionado**: `524f2f7..HEAD` (`GateCandidate.cs`, `VerificationRun.cs`, `VerificationService.cs`, `GatePackage.cs`, `GateExecutor.cs`, `catalog_path.py`, `runner.py`, `Core/check_gate.py`, `Core/policies/gate-requirements.json`, `VerificationSelector.razor/.css`, `Verifications.razor/.css`, `RequirementSheet.razor`, `MainLayout.razor`, `Program.cs`, `CemgSpec.csproj` e suítes de teste).
- **Achados identificados e resolvidos durante o incremento**:
  - `REV-PRC-01` (**P2** — `CemgSpec/Infrastructure/Gates/GateExecutor.cs:178,331`): durante mutação concorrente TOCTOU (`beforeBrokerRead`), `FileSystemWatcher` invocava `context.CancelWork()`, substituindo `context.WorkToken` por um novo `CancellationTokenSource` antes do filtro `catch (OperationCanceledException) when (... || context.WorkToken.IsCancellationRequested)`. **Resolvido** capturando `var workToken = context.WorkToken;` antes de criar `linkedCts`.
  - `REV-PRC-02` (**P1** — `CemgSpec/Application/VerificationService.cs:171`, `Verifications.razor:172`, `VerificationSelector.razor:165`): `OnStateChanged` inscrito simultaneamente em `ProjectStore.Changed` e `VerificationStore.Changed` chamava `InvalidateProjectRuns`, que disparava `VerificationStore.Changed` incondicionalmente enquanto `HasPendingChanges` era `true`, causando recursão infinita. **Resolvido** tornando `InvalidateProjectRuns` idempotente (dispara `Changed` apenas quando alguma execução muda de `Current` para `Stale`) e separando os handlers `OnProjectChanged` e `OnVerificationChanged`.
  - `REV-PRC-03` (**P2** — `CemgSpec/CemgSpec.csproj:18-24`, `GatePackage.cs:39-40`): `PreserveNewest` não atualizava `runner.py`/`catalog_path.py` no diretório de saída após restauração de timestamp pelo Git, causando divergência de hash com `TryRegisterVerifiedSeal`. **Resolvido** alterando para `CopyToOutputDirectory=Always`, fixando os hashes SHA-256 exatos (`c529d4e5...` e `e51d61f5...`) e adicionando asserção direta de `TryRegisterVerifiedSeal` em `GatePackageTests.cs`.
  - `REV-PRC-04` (**P3** — `CemgSpec/Components/Pages/Verifications.razor.css:103-114`): hash SHA-256 longo na lista `Limites explícitos` ultrapassava a borda direita do card em larguras intermediárias. **Resolvido** com `overflow-wrap: anywhere;` e conferido na captura [verifications-ready-dark.png](captures/T058/verifications-ready-dark.png).
- **Invariantes verificados**:
  - Core `check_gate.py` e `gate-requirements.json` idênticos byte a byte ao preset CEMG `3.0.0`.
  - Nenhum caminho real da origem é entregue ao processo Python; todas as leituras passam por `CatalogPath` → `BrokerClient` → `DocumentBroker.Read` com revalidação de handle, contenção, junções e hash no momento real da leitura.
  - Estado intermediário seguro preservado: `speckit-analyze` e `Salvar resultados` permanecem desabilitados; nenhuma nova dependência NuGet ou JS foi adicionada no PR-C.
- **T061 (resolução e revalidação)**: todos os 4 achados acima foram corrigidos e revalidados na regressão completa de **108/108 testes aprovados** em [PR-C-validation.md](PR-C-validation.md).
