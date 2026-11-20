# Evidências de validação — US4

Estado: APROVADA
Executor/ferramenta: Antigravity (Gemini 4 Neon) · dotnet SDK 10.0.404 / runtime .NET 10.0.15 / Playwright 1.64.1 / Microsoft Edge headless
Data/hora e fuso: 2026-11-20 08:25 America/Sao_Paulo
Commit: `fa38766474cbe338ffabc2bc8d53fc3c832cf440`
Alterações não commitadas: nenhuma no código/testes; apenas novos artefatos desta evidência (`evidence/US4-validation.md`, `evidence/logs/T039/*`, `evidence/captures/T039/*`) e atualização de `tasks.md`.
Ambiente, versões e dados: Windows x64, host local `SpecAudit` em loopback (`http://127.0.0.1:<porta-efêmera>`), fixtures sintéticas criadas sob `%LOCALAPPDATA%\Temp` (removidas ao final de cada teste), nenhum projeto real nem acesso à internet.

| Execução | Critério/cenário | Comando ou procedimento e diretório | Esperado | Observado | Resultado | Log/artefato |
|---|---|---|---|---|---|---|
| RUN-01 | US4-S01–S04 / US4-AC01–AC04 (domínio e rastreabilidade via broker) | Raiz do repositório: `dotnet test tests/SpecAudit.Tests/SpecAudit.Tests.csproj --no-restore --filter "FullyQualifiedName~RequirementTraceTests" --logger "console;verbosity=normal"` | Chaves contextuais `(ProjectId, FeatureId, LocalId)` separando `FR-001` entre `001-demo` e `002-other`; `Explicit` vs `Inferred`; `CoverageState` apenas `Linked`/`MissingLink`/`Ambiguous`; duplicidades sem mesclar; `registro de terceiros` sem aprovação; `DoNotRead.cs` `OutOfScope` e nunca aberto pelo broker | 4/4 aprovados (exit code 0): `S01`, `S02`, `S03`, `S04` | PASSOU | [run-01-domain.txt](logs/T039/run-01-domain.txt) |
| RUN-02 | US4-S01–S04 / US4-AC01–AC04 (E2E real no host loopback + Edge headless + seleção nativa) | Raiz do repositório com `$env:TEMP = "$env:LOCALAPPDATA\Temp"` e `$env:SPECAUDIT_CAPTURE_DIR = "...\evidence\captures\T039"`: `dotnet test tests/SpecAudit.Ui.Tests/SpecAudit.Ui.Tests.csproj --no-restore --filter "FullyQualifiedName~RequirementSheetTests" --logger "console;verbosity=normal"` | Ficha documental de `FR-001` em `001-demo` e `002-other` independentes, `Explícito` visível, ausência de `Evidências da implementação`/`npm test`/`Cópia isolada`, `FR-013` exibindo `Evidência documental insuficiente` sem `Não implementado`, `registro de terceiros` + `não aprova`, `DoNotRead.cs` `não verificada` + `capacidade futura`, origem intacta e destino externo vazio | 4/4 aprovados (exit code 0): `S01`, `S02`, `S03`, `S04` | PASSOU | [run-02-us4-e2e.txt](logs/T039/run-02-us4-e2e.txt) · [FR-001 demo](captures/T039/requirements-fr001.png) · [FR-001 other](captures/T039/requirements-other-fr001.png) |
| RUN-03 | Regressão relacionada (fundação, ReaderBoundary, sessão/antiforgery/fallback, US1, US2, US3, US4) | Raiz do repositório com `$env:TEMP = "$env:LOCALAPPDATA\Temp"`: `dotnet test SpecAudit.slnx --no-build --no-restore --filter 'FullyQualifiedName!~RealBrowserRendersLocalProjectSelectionWithoutCircuitErrors&FullyQualifiedName!~US1IntegratedHarnessTests' --logger "console;verbosity=normal"` | Nenhuma regressão em domínio, limites Windows ou UI | 93/93 aprovados (exit code 0): 20 `SpecAudit.Tests`, 16 `SpecAudit.Windows.Tests`, 57 `SpecAudit.Ui.Tests` | PASSOU | [run-03-regression.txt](logs/T039/run-03-regression.txt) |

## Comparação visual com o mockup aprovado

- Referência: metade inferior de [dashboard-requisito.png](../design/mockups/dashboard-requisito.png) e [README dos mockups](../design/mockups/README.md).
- Versão avaliada: `fa38766474cbe338ffabc2bc8d53fc3c832cf440`, viewport 1440 × 1000, tema escuro (único tema disponível antes da US8/T044).
- Aspectos preservados: sidebar persistente com `Requisitos` ativo em azul, breadcrumb `REQUISITOS / <feature> / <id>`, abas (`Resumo`, `Vínculos`, `Verificações`, `Sugestões`), card de citação documental com borda azul, lista de itens à esquerda e painel lateral de verificações/limites à direita.
- Substituições obrigatórias da V1 aplicadas:
  - `Evidências da implementação` substituída por **Vínculos documentais** (`Explícito` / `Inferido` + `Não verificada (fora do catálogo)` para `.cs`).
  - `npm test`, `Cópia isolada` e links ativos para código (`src/search.ts`) removidos; botão `Executar verificação` mantido desabilitado até PR-C/D.
  - Declarações de terceiros e `Limites documentais da V1` explícitos.

## Cobertura e limitações

- **Critérios cobertos**: US4-AC01 (`US4-S01`), US4-AC02 (`US4-S02`), US4-AC03 (`US4-S03`), US4-AC04 (`US4-S04`).
- **Cenários não executados**:
  - Comparação visual no tema claro: **NÃO EXECUTADO** até a entrega do alternador de tema na US8 (T044).
  - Cinco testes históricos da US1 excluídos do filtro de regressão porque gravam em caminhos históricos deste checkout.
- **Regressão executada**: 93/93 aprovados em RUN-03 (0 falhas).

## Rodadas após correções

- **Rodada T041 (2026-11-20 08:31 America/Sao_Paulo, sobre base `edaa2ef`)**: após resolver `REV-US4-01` (selo `Atual`/`Desatualizado` na faixa de contexto e preservação das fichas como `Freshness.Stale` também na corrida broker-antes-do-watcher em `Requirements.razor`) e `REV-US4-02` (reset da aba por `SourceLocation` em `RequirementSheet.razor`), a suíte completa foi reexecutada e aprovou **93/93 testes** (20 `SpecAudit.Tests`, 16 `SpecAudit.Windows.Tests`, 57 `SpecAudit.Ui.Tests`, exit code 0). Log: [logs/T041/regression.txt](logs/T041/regression.txt).
  - SHA-256 dos arquivos alterados na T041:
    - `SpecAudit/Components/Pages/Requirements.razor` — `e51913a9f676845bfd1c562aa38b6e29e5a15793330029211a21ad138d2a0162`
    - `SpecAudit/Components/Pages/Requirements.razor.css` — `7b5e2ffc392ba420b7f0b9624a578eb43c061d44e9b0fb6438408f8264564cac`
    - `SpecAudit/Components/Requirements/RequirementSheet.razor` — `dd65500d4f8ddfaea76b1f7050a8c033c354d8e0de04295adaa711216f155fe1`

## Identificação do incremento e histórico

- PR previsto: PR-B (local, ainda não aberto/integrado).
- Base resolvida: `32170ad` (US3 concluída tecnicamente).
- Versão candidata: `fa38766474cbe338ffabc2bc8d53fc3c832cf440`.
- Predecessores: PR-A, US2, US3, T036–T038.
- Escopo desta evidência: `INCREMENTO` (US4 dentro do PR-B).

