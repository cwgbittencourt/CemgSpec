# T036 — Testes de rastreabilidade e ficha documental do requisito (US4)

T036 concluída no escopo de preparação dos testes da US4. Execução e autorrevisão por Antigravity (Gemini 4 Neon) em 2026-11-19, America/Sao_Paulo. Base Git `32170ad` (US3 tecnicamente concluída). Ambiente Windows x64, SDK .NET 10.0.404, runtime .NET 10.0.15, Playwright 1.64.1 e Edge headless sobre host loopback. Nenhum código de produção alterado.

## Execução

```powershell
$red = 'FullyQualifiedName~RequirementTraceTests|FullyQualifiedName~RequirementSheetTests'
dotnet test CemgSpec.slnx --no-restore --filter $red --logger 'console;verbosity=normal'

$reg = 'FullyQualifiedName!~RequirementTraceTests&FullyQualifiedName!~RequirementSheetTests&FullyQualifiedName!~RealBrowserRendersLocalProjectSelectionWithoutCircuitErrors&FullyQualifiedName!~US1IntegratedHarnessTests'
dotnet test CemgSpec.slnx --no-build --no-restore --filter $reg --logger 'console;verbosity=minimal'
```

| Cenário | Contrato preparado | Resultado nesta revisão |
|---|---|---|
| US4-S01 / AC01 | Chaves contextuais `(ProjectId, FeatureId, LocalId)` separando `FR-001` entre `001-demo` e `002-other` sem mesclagem; distinção `Explícito` vs `Inferido`; ausência de `Evidências da implementação`, `npm test` e `Cópia isolada` | Duas falhas esperadas (red): domínio ausente e HTTP 404 em `/requisitos` |
| US4-S02 / AC02 | Critérios com `CoverageState` (`Linked`, `MissingLink`, `Ambiguous`), duplicidade na mesma funcionalidade sem mesclar, `Evidência documental insuficiente` sem `Não implementado` | Duas falhas esperadas (red): domínio ausente e HTTP 404 em `/requisitos` |
| US4-S03 / AC03 | Declarações de validação/aceite registradas exibidas como `registro de terceiros` com fonte/versão, sem aprovação pelo CemgSpec | Duas falhas esperadas (red): serviço ausente e HTTP 404 em `/requisitos` |
| US4-S04 / AC04 | Referências a `.cs`/`.razor`/configuração preservadas como texto `não verificada`, sem abrir arquivo-fonte e informando análise de código como `capacidade futura` | Duas falhas esperadas (red): serviço ausente e HTTP 404 em `/requisitos` |
| Regressão selecionada | Suíte existente (fundação, US1, US2, US3) | **84/84 aprovados** (16 domínio, 16 Windows, 52 UI), exit code 0 |

Logs: [red.txt](logs/T036/red.txt) (exit code 1; 8 falhas esperadas) · [regression.txt](logs/T036/regression.txt) (exit code 0; 84/84 aprovados).

## Comparação visual preparada para T038–T039

Conferir a metade inferior de [dashboard-requisito.png](../design/mockups/dashboard-requisito.png) com a [referência aprovada](../design/mockups/README.md):

| Estado para capturar no app | Aspecto visual a conferir | Substituição obrigatória da V1 |
|---|---|---|
| Ficha de `FR-001` em `001-demo`, 1440 × 1000 | Sidebar, breadcrumb/cabeçalho do requisito, abas (`Resumo`, `Evidências`, `Verificações`, `Sugestões`), cards de descrição/vínculos/critérios e painel lateral direito | Substituir `Evidências da implementação` por **Vínculos documentais** (`Explícito` / `Inferido`); remover `npm test`, `Cópia isolada` e link ativo para `src/search.ts`; manter verificação futura indisponível até PR-C/D |
| Requisito sem tarefa (`FR-013`) e `FR-001` em `002-other` | Separação visual por funcionalidade e estado de lacuna documental | Exibir **Evidência documental insuficiente** (nunca `Não implementado`) e nunca mesclar `FR-001` de funcionalidades distintas |

