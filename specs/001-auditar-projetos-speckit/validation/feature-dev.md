# Roteiro de Validação DEV Integrado — CemgSpec V1 (T094)

- **Feature**: `001-auditar-projetos-speckit`
- **Status da validação humana DEV**: `PENDENTE`

## Roteiro integrado ponta a ponta

1. **Seleção e proteção somente leitura (US1)**: abra `http://127.0.0.1:5187`, selecione origem SpecKit e destino externo em **Projetos** e confirme carregamento; verifique que nenhum arquivo foi gravado na origem ou no destino.
2. **Dashboard e indicadores documentais (US2)**: navegue para **Dashboard** e confirme que os percentuais refletem apenas declarações documentais (`[x]` / total e cobertura documental explícita), nunca implementação.
3. **Requisitos e rastreabilidade (US3)**: navegue para **Requisitos**, abra uma ficha de requisito e inspecione os vínculos documentais e lacunas.
4. **Documentos e atualização (US4)**: navegue para **Documentos**, abra um documento autorizado em modo somente leitura e confirme que arquivos de código-fonte não aparecem.
5. **Verificações e agentes (US5)**: navegue para **Verificações**, selecione `check_gate` offline ou agente compatível (`Codex` / `Antigravity`) e confirme execução controlada sem alterar a origem.
6. **Sugestões, Save e Histórico (US6)**: navegue para **Sugestões**, classifique uma ocorrência, salve externamente via **Salvar resultados**, confira os arquivos gerados (`report.md`, `report.json`, `findings.csv`, `indicators.csv`, `manifest.json`), compare com o histórico e exclua apenas um registro selecionado.
7. **Temas (US8)**: alterne entre **Escuro** e **Claro** na barra lateral e confirme preservação do contexto do projeto.

## Registro de execução DEV (preenchimento humano)

| Campo | Valor |
| --- | --- |
| Responsável DEV | _Pendente_ |
| Data | _Pendente_ |
| Resultado | _Pendente_ |
| Observações | _Pendente_ |
