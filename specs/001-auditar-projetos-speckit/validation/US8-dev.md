# Roteiro do DEV — testar Navegação das 5 áreas e Temas (US8)

**Estado do roteiro:** caminho automatizado **CONFERIDO_PELA_IA** sobre a versão `32453e9` + ajuste `REV-US8-01`; percurso manual interativo com **CONFERENCIA_PARCIAL**. Execução e aceite do DEV **PENDENTES**.

## Repetir a validação automatizada

```powershell
$env:TEMP = "$env:LOCALAPPDATA\Temp"; $env:TMP = $env:TEMP
dotnet test tests/SpecAudit.Ui.Tests/SpecAudit.Ui.Tests.csproj --no-restore --filter 'FullyQualifiedName~NavigationThemeTests' --logger 'console;verbosity=normal'
```

Esperado: **2/2 aprovados** (`S01` cinco áreas + placeholders indisponíveis; `S02` tema escuro inicial, claro opcional `rgb(247, 248, 250)` preservando `FR-013` e `Evidência documental insuficiente`).

## Registro humano e aceite

| Cenário / critério | Resultado | Resultado observado / motivo da não execução | Evidências (links) | Commit testado / diff ou manifesto | Responsável | Data/hora e fuso | Defeito / observações |
|---|---|---|---|---|---|---|---|
| US8-S01 / US8-AC01 | | | | | | | |
| US8-S02 / US8-AC02 | | | | | | | |

### Decisão de aceite

| Campo | Registro do DEV |
|---|---|
| Decisão explícita | |
| Responsável pela decisão | |
| Data/hora e fuso | |
| Commit efetivamente validado / diff ou manifesto | |
| Rodada e referência à execução humana / evidências | |
| Justificativa e observações | |
| Escopo aceito ou rejeitado | |
| Ressalvas, critérios não atendidos e riscos, se aplicável | |
| Ações posteriores e responsáveis, se aplicável | |
