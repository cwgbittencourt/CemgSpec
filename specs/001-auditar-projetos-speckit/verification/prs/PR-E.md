# PR-E — US6 (Sugestões, Save Externo, Histórico) e Fechamento Integrado V1 (T093)

- **Feature**: `001-auditar-projetos-speckit`
- **Branch base**: `main`
- **Predecessor**: `PR-D` (`verification/prs/PR-D.md`)
- **Tasks incluídas**: `T079`–`T094`
- **Status de abertura/integração de PR**: `NÃO ABERTO / NÃO INTEGRADO` (aguardando revisão e aprovação humana)

## Resumo do escopo

1. **Decisões humanas e registros salvos (`T080`)**: `HumanDecision`, `SavedRecord` e `FindingDecisionService` com exigência de `Justificativa` + `Responsável` para `FalsePositive`/`AcceptedRisk` e `NeedsReview` quando `ContextFingerprint` muda.
2. **Writer externo e serialização segura (`T081`–`T082`)**: `ExternalReportWriter` + `ReportSerializer` (MD/JSON/CSV com neutralização de fórmulas, `manifest.json` por último, proteção TOCTOU/reparse point, falha parcial honesta) e prova real `evidence/ExternalWriter-proof.md`.
3. **Histórico salvo, comparação e exclusão seletiva (`T083`)**: `SavedHistoryReader` + `SavedHistoryService` com validação SHA-256, detecção de pacotes incompletos/adulterados, diff `New`/`Persistent`/`Resolved` (sem inferência em amostra incompleta) e `DeleteSaved` restrito ao registro escolhido.
4. **UI `/sugestoes` e `SaveDialog` (`T084`)**: habilitação de `Salvar resultados` condicionada ao selo `ExternalWriter` `Passed`, substituição do placeholder em `MainLayout.razor` e capturas visuais em `evidence/captures/T084/`.
5. **Fechamento integrado (`T085`–`T094`)**: regressão determinística **128/128** aprovada (`evidence/feature-validation.md`), roteiros DEV e documentos de aceite com decisões humanas pendentes.

## Evidências

- `evidence/ExternalWriter-proof.md`
- `evidence/US6-validation.md` e `evidence/US6-review.md`
- `evidence/feature-validation.md`
- `evidence/PR-E-validation.md` e `evidence/PR-E-review.md`
- `validation/US6-dev.md` e `validation/feature-dev.md`

## Aprovação humana (preenchimento humano)

| Papel | Nome | Decisão | Data |
| --- | --- | --- | --- |
| Revisor PR | _Pendente_ | _Pendente_ | _Pendente_ |
| Aprovador / Merge | _Pendente_ | _Pendente_ | _Pendente_ |
