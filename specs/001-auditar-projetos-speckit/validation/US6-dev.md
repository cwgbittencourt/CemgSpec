# Roteiro de Validação DEV — US6 (T088)

- **Feature**: `001-auditar-projetos-speckit`
- **História**: `US6` (Sugestões, Save e histórico)
- **Status da validação humana DEV**: `PENDENTE`

## Passos reproduzíveis

1. Inicie o host local do SpecAudit (`dotnet run --project SpecAudit/SpecAudit.csproj`) e abra `http://127.0.0.1:5187`.
2. Em **Projetos**, selecione uma pasta de projeto SpecKit (origem) e uma pasta externa vazia (destino) e clique em **Confirmar carregamento**.
3. Navegue até **Sugestões** e confirme que a pasta externa de destino continua vazia (nenhum relatório gravado automaticamente).
4. Em uma ocorrência listada, selecione **Falso positivo** e clique em **Registrar decisão** sem preencher justificativa; confirme o alerta exigindo justificativa e responsável.
5. Preencha **Justificativa** e **Responsável** e clique em **Registrar decisão**; confirme que a decisão aparece preservando o achado técnico original.
6. Clique em **Salvar resultados**, revise destino, atualidade e formatos (`Markdown`, `JSON`, `CSV`) no diálogo **Confirmar salvamento externo** e clique em **Confirmar e salvar**.
7. Confirme a criação da pasta `SpecAudit-<utc>-<uuid>` contendo `report.md`, `report.json`, `findings.csv`, `indicators.csv` e `manifest.json` no destino externo, sem qualquer alteração na origem.
8. Salve um segundo registro, clique em **Comparar com atual** em um registro do **Histórico salvo** e depois clique em **Excluir registro** em apenas um deles, confirmando que somente o registro selecionado foi removido.

## Registro de execução DEV (preenchimento humano)

| Campo | Valor |
| --- | --- |
| Responsável DEV | _Pendente_ |
| Data | _Pendente_ |
| Resultado | _Pendente_ |
| Observações | _Pendente_ |
