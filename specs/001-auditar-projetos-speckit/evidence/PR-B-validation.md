# Evidências de validação — PR-B (US2, US3, US4 e US8)

Estado: APROVADA (técnica)
Executor/ferramenta: Antigravity (Gemini 4 Neon) · dotnet SDK 10.0.404 / runtime .NET 10.0.15 / Playwright 1.64.1 / Microsoft Edge headless
Data/hora e fuso: 2026-11-20 09:03 America/Sao_Paulo
Base declarada / predecessor: PR-A (`ddf8f31` / `73e06b9`, aprovação humana pendente)
Revisão candidata PR-B: `ac53fd27d52ddc0b9cd4ab4fbc7c2e02462eae53`

| Execução | Escopo | Comando (raiz do repositório) | Resultado | Log |
|---|---|---|---|---|
| PRB-RUN-01 | Regressão completa PR-B (Fundação, US1, US2, US3, US4, US8 e fix de transporte SignalR) | `$env:TEMP="$env:LOCALAPPDATA\Temp"; $env:TMP=$env:TEMP; $filtro='FullyQualifiedName!~RealBrowserRendersLocalProjectSelectionWithoutCircuitErrors&FullyQualifiedName!~US1IntegratedHarnessTests'; dotnet test SpecAudit.slnx --no-restore --filter $filtro --logger 'console;verbosity=normal'` | **95/95 aprovados** (20 `SpecAudit.Tests`, 16 `SpecAudit.Windows.Tests`, 59 `SpecAudit.Ui.Tests`, exit code 0) | [logs/PR-B/regression.txt](logs/PR-B/regression.txt) |

## Cobertura consolidada do incremento PR-B

- **US2 (Documentos e Preview Markdown)**: [T021-preview-tests.md](T021-preview-tests.md), [T022-local-preview.md](T022-local-preview.md), [preview-safety.md](preview-safety.md), [US2-validation.md](US2-validation.md), [US2-review.md](US2-review.md).
- **US3 (Dashboard documental)**: [T029-dashboard-tests.md](T029-dashboard-tests.md), [US3-validation.md](US3-validation.md), [US3-review.md](US3-review.md).
- **US4 (Ficha documental do requisito)**: [T036-requirement-tests.md](T036-requirement-tests.md), [US4-validation.md](US4-validation.md), [US4-review.md](US4-review.md).
- **US8 (Navegação das 5 áreas e Temas escuro/claro)**: [T043-navigation-theme-tests.md](T043-navigation-theme-tests.md), [US8-validation.md](US8-validation.md), [US8-review.md](US8-review.md).
- **Estado intermediário seguro**: `Verificações` (`Executar verificação` desabilitado) e `Sugestões` (`Salvar resultados` desabilitado) permanecem indisponíveis até PR-C/D/E; origem integralmente somente leitura; nenhum arquivo gravado em destino externo; nenhum arquivo `.cs`/`.razor`/config aberto.
