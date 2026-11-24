# Autorrevisão Técnica — US6 (T086 / T087)

- **Feature**: `001-auditar-projetos-speckit`
- **História**: `US6`
- **Tipo de revisão**: Autorrevisão técnica do agente (não substitui revisão ou aprovação humana)
- **Commits revisados**: `3296b35..0caff61` (`T079`–`T084`)

## Arquivos revisados

- `CemgSpec/Domain/HumanDecision.cs`
- `CemgSpec/Domain/SavedRecord.cs`
- `CemgSpec/Application/FindingDecisionService.cs`
- `CemgSpec/Application/SavedHistoryService.cs`
- `CemgSpec/Infrastructure/Reports/ReportSerializer.cs`
- `CemgSpec/Infrastructure/Reports/ExternalReportWriter.cs`
- `CemgSpec/Infrastructure/Reports/SavedHistoryReader.cs`
- `CemgSpec/Components/Pages/Suggestions.razor` (+ `.css`)
- `CemgSpec/Components/Reports/SaveDialog.razor` (+ `.css`)
- `CemgSpec/Components/Layout/MainLayout.razor`
- `CemgSpec/Program.cs`

## Checklist de contratos e segurança

1. **Origem somente leitura**: `ExternalReportWriter` e `SavedHistoryReader` revalidam origem e destino via `WindowsReadHandle` (`FinalPath` + `Identity`) e recusam destino coincidente/contido na origem ou com reparse points.
2. **Sem auto-save**: `Suggestions.razor` mantém decisões em memória via `FindingDecisionService` e só invoca `ExternalReportWriter.Save` após confirmação explícita em `SaveDialog`.
3. **Create-new e `manifest.json` por último**: `FileMode.CreateNew` impede sobrescrita; `manifest.json` é o último arquivo gravado com hashes SHA-256; falhas parciais retornam `SaveState.SaveFailed` mantendo o diretório parcial sem `manifest.json`.
4. **Neutralização de fórmulas CSV**: `ReportSerializer.CsvCell` prefixa `'` em células iniciadas por `=`, `+`, `-`, `@`, `\t`, `\r`.
5. **Exclusão seletiva**: `SavedHistoryReader.DeletePackage` valida contenção no destino externo resolvido e ausência de reparse point antes de remover apenas o diretório do `SavedRecordId` escolhido.

## Achados

Nenhum achado pendente após os ajustes de sincronização de teste em `SaveHistoryTests.cs` (revalidados com **128/128** testes aprovados em `evidence/US6-validation.md`).
