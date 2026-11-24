# Evidências de validação — US8

Estado: APROVADA
Executor/ferramenta: Antigravity (Gemini 4 Neon) · dotnet SDK 10.0.404 / runtime .NET 10.0.15 / Playwright 1.64.1 / Microsoft Edge headless
Data/hora e fuso: 2026-11-20 08:59 America/Sao_Paulo
Commit base validado: `32453e98197239d1ecab47dfb0e0101e8988aca7` (com ajuste de contraste T047 em `CemgSpec/wwwroot/app.css`).

| Execução | Critério/cenário | Comando ou procedimento e diretório | Esperado | Observado | Resultado | Log/artefato |
|---|---|---|---|---|---|---|
| RUN-01 | US8-S01 / US8-AC01 e US8-S02 / US8-AC02 (E2E real no host loopback + Edge headless) | Raiz do repositório: `dotnet test tests/CemgSpec.Ui.Tests/CemgSpec.Ui.Tests.csproj --no-restore --filter "FullyQualifiedName~NavigationThemeTests" --logger "console;verbosity=normal"` | 5 áreas na sidebar com contexto/somente leitura visíveis, placeholders indisponíveis de `Verificações` (`Executar verificação` desabilitado) e `Sugestões` (`Salvar resultados` desabilitado), tema escuro inicial, troca para claro (`rgb(247, 248, 250)`) preservando `FR-013` e `Evidência documental insuficiente` | 2/2 aprovados (exit code 0) | PASSOU | [run-01-us8-e2e.txt](logs/T045/run-01-us8-e2e.txt) · [Escuro](captures/T045/theme-dark-requirements.png) · [Claro Requisitos](captures/T045/theme-light-requirements.png) · [Claro Dashboard](captures/T045/theme-light-dashboard.png) |
| RUN-02 | Regressão completa (fundação, US1, US2, US3, US4, US8) | Raiz do repositório: `dotnet test CemgSpec.slnx --no-build --no-restore --filter 'FullyQualifiedName!~RealBrowserRendersLocalProjectSelectionWithoutCircuitErrors&FullyQualifiedName!~US1IntegratedHarnessTests' --logger "console;verbosity=normal"` | Nenhuma regressão em domínio, limites Windows ou UI | 95/95 aprovados (exit code 0): 20 domínio, 16 Windows, 59 UI | PASSOU | [run-02-regression.txt](logs/T045/run-02-regression.txt) |
| RUN-03 (T047) | Revalidação após ajuste de contraste `REV-US8-01` em `app.css` | `dotnet test tests/CemgSpec.Ui.Tests/CemgSpec.Ui.Tests.csproj --no-restore --filter "FullyQualifiedName~NavigationThemeTests" --logger "console;verbosity=normal"` | 2/2 aprovados | 2/2 aprovados (exit code 0) | PASSOU | [us8-revalidation.txt](logs/T047/us8-revalidation.txt) |

## Comparação visual nos dois temas

- Referência: [dashboard-requisito.png](../design/mockups/dashboard-requisito.png) (temas escuro e claro + paleta) e [README dos mockups](../design/mockups/README.md).
- Tema escuro inicial (`#11151F`, `#1B2230`, `#4F8CFF`, `#AA8CF5`, `#F7F8FA`) e tema claro opcional (`#F7F8FA`, superfícies brancas e contraste legível) conferidos nas capturas de `T045`.
- Preferência de tema mantida apenas na sessão em memória (`MainLayout.razor`); nenhuma gravação em disco.
