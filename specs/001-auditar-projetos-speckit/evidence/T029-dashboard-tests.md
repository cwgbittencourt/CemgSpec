# T029 — Testes de progresso documental e Dashboard (US3)

T029 concluída no escopo de preparação dos testes da US3. Execução e autorrevisão por Antigravity (Gemini 4 Neon) em 2026-11-19, America/Sao_Paulo. Base Git `d2a34bc` (US2 tecnicamente concluída). Ambiente Windows x64, SDK .NET 10.0.404, runtime .NET 10.0.15, Playwright 1.64.1 e Edge headless sobre host loopback. Nenhum código de produção alterado.

## Execução

Comandos a partir da raiz do repositório (com `$env:TEMP = "$env:LOCALAPPDATA\Temp"`):

```powershell
$red = 'FullyQualifiedName~DocumentaryProgressTests|FullyQualifiedName~DashboardTests'
dotnet test SpecAudit.slnx --no-restore --filter $red --logger 'console;verbosity=normal'

$reg = 'FullyQualifiedName!~DocumentaryProgressTests&FullyQualifiedName!~DashboardTests&FullyQualifiedName!~RealBrowserRendersLocalProjectSelectionWithoutCircuitErrors&FullyQualifiedName!~US1IntegratedHarnessTests'
dotnet test SpecAudit.slnx --no-build --no-restore --filter $reg --logger 'console;verbosity=minimal'
```

| Cenário | Contrato preparado | Resultado nesta revisão |
|---|---|---|
| Invariante V1 | Proibição das entidades `SourceCodeAnalysis`, `Compilation`, `ImplementationScore` e `DependencyInventory` no assembly `SpecAudit` | PASSOU (1/1) |
| US3-S01 / AC01 | 30/40 tarefas = `75%` declarado, 12/20 requisitos = `60%` de cobertura documental, denominador mantendo inconclusivos, escopo vazio `003-empty` exibindo `Sem itens` sem percentual e ausência de rótulos históricos (`Implementação evidenciada`, `Validação aprovada`) | Duas falhas esperadas (red): domínio ausente (`DocumentaryDashboardService`/`DashboardProjection`/`TaskDeclaration`) e HTTP 404 em `/dashboard` |
| US3-S02 / AC02 | Estados documentais e gates registrados (`ready.json`, `blocked.json` etc.) com procedência e aviso explícito de que `READY` não comprova implementação, teste funcional ou aceite humano | Duas falhas esperadas (red): projeção ausente e HTTP 404 em `/dashboard` |
| US3-S03 / AC03 | Ocorrências documentais (impedimentos registrados, requisitos sem tarefa `FR-013`, tarefas sem requisito `T013`, duplicidades e referência a código `DoNotRead.cs` marcada como `não verificada nesta versão` sem leitura do destino) | Duas falhas esperadas (red): serviço ausente e HTTP 404 em `/dashboard` |
| US3-S04 / AC04 | Filtros e atualização documental após edição externa (`31 / 40`) marcando verificação anterior como desatualizada sem executar agente ou gate e preservando edição na origem e destino vazio | Duas falhas esperadas (red): serviço ausente e HTTP 404 em `/dashboard` |
| Regressão selecionada | Suíte existente (fundação, US1 e US2) | **75/75 aprovados** (11 domínio, 16 Windows, 48 UI), exit code 0 |

Logs: [red.txt](logs/T029/red.txt) (exit code 1; 8 falhas esperadas, 1 invariante aprovada) · [regression.txt](logs/T029/regression.txt) (exit code 0; 75/75 aprovados).

## Comparação visual preparada para T031–T032

Conferir a metade superior de [dashboard-requisito.png](../design/mockups/dashboard-requisito.png) com a [referência aprovada](../design/mockups/README.md):

| Estado para capturar no app | Aspecto visual a conferir | Substituição obrigatória da V1 |
|---|---|---|
| Dashboard com `001-demo` ativo, 1440 × 1000 | Sidebar, cabeçalho, cards de indicadores com barra azul/lilás, chips de resumo, tabela de requisitos/ocorrências e painel lateral de estados registrados | Substituir `Implementação evidenciada` por **Cobertura documental** e `Validação aprovada` por **Estados e gates registrados**; nunca exibir percentual de implementação comprovada nem coluna de evidência em código (`src/search.ts`) |
| Escopo vazio (`003-empty`) | Legibilidade dos cards sem barra/percentual inventado | Exibir **Sem itens** quando o denominador for zero (nunca `0%` ou `NaN%`) |
| Alteração externa + filtro aplicado | Destaque de estado desatualizado e ação `Atualizar documentos` | Atualização recalcula somente contagens documentais, sem executar `analyze` ou `check_gate` automaticamente |

Repetir as capturas nos dois temas após a entrega da US8 (T044) e na regressão integrada (T089).

