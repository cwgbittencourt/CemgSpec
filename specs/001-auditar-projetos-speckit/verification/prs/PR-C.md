# PR-C — Verificações permitidas: combo fechada e `check_gate` offline (US5 parcial)

- **Estado de aprovação humana**: **PENDENTE** (este arquivo prepara a descrição revisável do PR; não abre, publica ou integra PR).
- **Predecessor**: PR-B ([PR-B.md](PR-B.md), base `524f2f7`, aprovação humana pendente).
- **Tasks cobertas**: `T053–T062` (`US5-AC03/05/06/07` parcial; `speckit-analyze` permanece desabilitado até `PR-D`).

## Escopo final entregue

1. **Domínio e máquina de estados de verificações (`T054`)**:
   - [GateCandidate.cs](../../../../SpecAudit/Domain/GateCandidate.cs): enum fechado `GateStage` (`Spec`, `Plan`, `Tasks`, `Pr`, `Acceptance`, `DevImpossibility`) e `RevisionSource` (`ArtifactSha256`, `UserConfirmedExternal`), impedindo copiar revisão não confirmada do próprio registro.
   - [VerificationRun.cs](../../../../SpecAudit/Domain/VerificationRun.cs) e [VerificationService.cs](../../../../SpecAudit/Application/VerificationService.cs): separação estrita entre `CapabilityAvailability`, `RunState` (`Queued → Preparing → Running → Completed | Failed | Cancelled` e `Preparing → Blocked`), `SemanticOutcome` (`Ready | Blocked | DevImpossibilityRecorded`) e `Freshness` (`Current → Stale`), exigindo término confirmado da árvore de processos antes de `Cancelled`.
2. **Pacote confiável CEMG `3.0.0` e adaptador `GateExecutor` (`T055–T057`)**:
   - [check_gate.py](../../../../SpecAudit/Infrastructure/Gates/Core/check_gate.py) e [gate-requirements.json](../../../../SpecAudit/Infrastructure/Gates/Core/policies/gate-requirements.json) empacotados byte a byte do preset CEMG `3.0.0` com hashes fixados em [GatePackage.cs](../../../../SpecAudit/Infrastructure/Gates/GatePackage.cs) e resolução de Python `3.10+` absoluto fora da origem.
   - [catalog_path.py](../../../../SpecAudit/Infrastructure/Gates/catalog_path.py), [runner.py](../../../../SpecAudit/Infrastructure/Gates/runner.py) e [GateExecutor.cs](../../../../SpecAudit/Infrastructure/Gates/GateExecutor.cs): preflight completo do grafo de evidências (`evidence`, `test-run` `stdout`/`stderr`, `product-execution` `artifacts`, `human_decisions`, `human_confirmation`), execução Python `-I -B` em `cwd` externo sem caminho real da origem, leitura mediada em tempo real por `DocumentBroker` (eliminando TOCTOU) e prova formal [GateAdapter-proof.md](../../evidence/GateAdapter-proof.md) (`7/7` aprovados).
3. **Combo fechada compartilhada e UI de `Verificações` + `Ficha do requisito` (`T058`)**:
   - [VerificationSelector.razor](../../../../SpecAudit/Components/Verifications/VerificationSelector.razor) e [Verifications.razor](../../../../SpecAudit/Components/Pages/Verifications.razor) integrados a [RequirementSheet.razor](../../../../SpecAudit/Components/Requirements/RequirementSheet.razor): exatamente duas opções (`speckit-analyze — Consistência documental` e `check_gate — Validar gates`), seleção sincronizada por projeto sem autoexecução, `speckit-analyze` desabilitado até `PR-D`, `check_gate` habilitado somente com selo `GateAdapter` `Passed` e visual alinhado a [verificacoes-pasta-externa.png](../../design/mockups/verificacoes-pasta-externa.png) com as substituições da V1 ([README.md](../../design/mockups/README.md)).

## Evidências

- Prova de proteção do adaptador: [GateAdapter-proof.md](../../evidence/GateAdapter-proof.md) (**7/7 testes aprovados**).
- Validação do incremento: [PR-C-validation.md](../../evidence/PR-C-validation.md) (**108/108 testes aprovados**: `23` `SpecAudit.Tests`, `23` `SpecAudit.Windows.Tests`, `62` `SpecAudit.Ui.Tests`).
- Review do incremento: [PR-C-review.md](../../evidence/PR-C-review.md) (`REV-PRC-01` a `REV-PRC-04` resolvidos e revalidados; revisão independente e aprovação humana pendentes).

## Decisão humana (reservada ao revisor/DEV)

- Aprovação de PR: PENDENTE
- Integração: PENDENTE
- Aceite funcional da história US5: PENDENTE (história completa fecha no `PR-D` após `T063–T078`)
