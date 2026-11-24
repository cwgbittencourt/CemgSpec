# T043 — Testes de navegação das 5 áreas e temas escuro/claro (US8)

T043 concluída no escopo de preparação dos testes da US8. Execução e autorrevisão por Antigravity (Gemini 4 Neon) em 2026-11-20, America/Sao_Paulo. Base Git `4136dbf` (US4 tecnicamente concluída). Nenhum código de produção alterado.

## Execução

```powershell
dotnet test tests/CemgSpec.Ui.Tests/CemgSpec.Ui.Tests.csproj --no-restore --filter 'FullyQualifiedName~NavigationThemeTests' --logger 'console;verbosity=normal'
```

| Cenário | Contrato preparado | Resultado nesta revisão |
|---|---|---|
| US8-S01 / AC01 | Navegação completa pelas cinco áreas (`Dashboard`, `Requisitos`, `Documentos`, `Verificações`, `Sugestões`) com contexto do projeto e `Somente leitura` visíveis e placeholders indisponíveis em `Verificações` (`Executar verificação` desabilitado) e `Sugestões` (`Salvar resultados` desabilitado) | Falha esperada (red): link `Verificações` ainda ausente na sidebar |
| US8-S02 / AC02 | Tema escuro inicial (`data-theme="dark"`), alternância para tema claro (`#F7F8FA` / `rgb(247, 248, 250)`), preservação do requisito `FR-013` e do estado textual `Evidência documental insuficiente`, e retorno ao escuro na sessão | Falha esperada (red): atributo `data-theme` e alternador ainda ausentes em `MainLayout.razor` |

Log: [red.txt](logs/T043/red.txt) (exit code 1; 2 falhas esperadas).

## Comparação visual preparada para T044–T045

Conferir [dashboard-requisito.png](../design/mockups/dashboard-requisito.png) (incluindo o painel **Tema claro** e **Paleta de cores**) e [README dos mockups](../design/mockups/README.md):
- Tema escuro inicial (`#11151F`, `#1B2230`, `#4F8CFF`, `#AA8CF5`, `#F7F8FA`) e tema claro opcional (`#F7F8FA`, superfícies brancas/cinza claro com contraste legível).
- Sidebar com as 5 áreas + `Projetos`, indicador `Somente leitura` e alternador `Escuro` / `Claro`.
- Placeholders indisponíveis para `Verificações` e `Sugestões` sem expor ações não provadas (PR-C/E).

