# Plano de teste manual — US6: Revisar e salvar resultados — Prioridade P2

**Estado: ESCRITO / RASCUNHO. Roteiro não conferido no produto e não executado pelo DEV.**
**Estado da implementação:** migração para constituição 4.0.0 validada; trabalho parado após T011 por solicitação do usuário. A história completa ainda depende das tarefas restantes; não usar este roteiro como evidência de execução/aceite.

## Versão e escopo

Base de desenvolvimento: `197548db2e3efa20c23e1f66b6a0ffb86c0f328d` + alterações locais de implementação iniciadas em 2026-11-16. Consultar `evidence/implementation-code-manifest.json` para os hashes deste checkpoint. O checkpoint técnico até T011 está identificado; a revisão funcional final da história ainda depende das tarefas restantes. Registrar o commit/manifesto realmente testado ao executar.

Cobertura planejada: todos os cenários ativos de US6. Não equivale a aceite, produto pronto ou teste aprovado.

## Preparação passo a passo

1. Após liberar as dependências desta história em tasks.md, obter a revisão identificada e conferir Windows x64 e SDK estável com `dotnet --info`.
2. Na raiz de desenvolvimento, executar `dotnet restore SpecAudit.slnx --locked-mode` e `dotnet test SpecAudit.slnx --no-restore`. A suíte ativa deve passar antes da validação funcional; os testes de sandbox v3 arquivados não são requisitos da política v4.
3. Criar somente dados sintéticos seguindo a receita em [quickstart.md](../quickstart.md#preparar-dados-sintéticos--ação-do-dev-fora-da-origem-real). Guardar os caminhos absolutos e baseline em dev-evidence. Repetir com novo GUID para o segundo projeto quando necessário. Não usar projeto real ou dados pessoais.
4. Preparar os dados específicos dos cenários abaixo. Gates negativos, corpus Mermaid e servidor de imagem devem ser sintéticos/controlados. Se uma dependência não existir, registrar NÃO EXECUTADO e o motivo; não instalar ou elevar automaticamente.
5. Iniciar o próprio SpecAudit: `dotnet run --project SpecAudit/SpecAudit.csproj --no-launch-profile -- --urls http://127.0.0.1:5187`. Abrir esse endereço no navegador local. Não executar comandos da solução auditada.
6. Escolher origem sintética e destino externo pelo picker nativo quando estiver implementado e habilitado. Usar o perfil local do usuário; autenticação de agente só será necessária para cenários analyze da US5, pela integração validada, sem copiar tokens.

## Execução por cenário

### US6-S01 — US6-AC01

**Estado inicial/dados:** Resultado em sessão.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Produzir resultado em sessão e fechar sem clicar Salvar. | Não há novo relatório, log de conteúdo ou cache oficial do SpecAudit no disco. |
| 2 | Reabrir, repetir e clicar Salvar após conferir conteúdo e destino externo. | Somente a ação explícita cria novo pacote externo, sem sobrescrever o anterior. |
| 3 | Conferir estado interno externo do agente. | Sua eventual permanência é informada e não importada automaticamente como histórico salvo. |

Registrar para US6-S01: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US6-S02 — US6-AC02

**Estado inicial/dados:** Resultado revisado.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Selecionar resultado revisado e solicitar exportação Markdown, JSON e CSV no destino externo. | Arquivos existem somente após ação explícita de salvamento. |
| 2 | Abrir os formatos e o manifesto e comparar escopo, fontes, data, hashes e ferramenta. | Metadados e conteúdo rastreáveis, hashes coerentes; CSV trata valores iniciados por fórmula como texto seguro. |
| 3 | Comparar baseline da origem. | Nenhum arquivo da solução foi alterado. |

Registrar para US6-S02: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US6-S03 — US6-AC03

**Estado inicial/dados:** Ocorrência.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Classificar uma ocorrência como falso positivo e depois risco aceito sem responsável/justificativa. | A decisão incompleta é recusada. |
| 2 | Repetir com responsável e justificativa explícitos. | Decisão atribuída é registrada em memória; o resultado técnico original é preservado. |
| 3 | Alterar documento/contexto relevante e consultar a decisão. | Revisão é solicitada; risco aceito não vira teste aprovado. |

Registrar para US6-S03: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US6-S04 — US6-AC04

**Estado inicial/dados:** Duas análises salvas.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Salvar duas análises sintéticas em registros separados. | Dois pacotes externos distintos com manifesto válido. |
| 2 | Comparar os registros e consultar novas, persistentes e resolvidas. | Diferenças mantêm contexto e fontes; amostra incompleta não implica resolução. |
| 3 | Selecionar um registro e solicitar exclusão explícita. | Somente o selecionado é removido; outros registros e origem permanecem intactos. Nenhuma exclusão automática. |

Registrar para US6-S04: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

## Evidências da IA e limitações

- Estado global de execução: [implementation-status.md](../evidence/implementation-status.md).
- Evidência específica da história será consolidada em `evidence/US6-validation.md` e `evidence/US6-review.md` quando as respectivas tasks forem executadas. Esses arquivos não são presumidos existentes.
- Os passos da UI acima descrevem o contrato aprovado; nomes, localização final de controles e capturas precisam ser conferidos na implementação. Este documento não comprova esse comportamento.
- Divergências entre ambiente da IA e DEV devem ser registradas, sem prometer reprodução só pela escrita do roteiro.

## Limpeza e repetição

1. Encerrar somente a instância do host e os descendentes pertencentes a esta rodada. Conferir término confirmado no relatório técnico.
2. Comparar arquivos/hashes da origem com baseline; separar edições intencionais do DEV. Não restaurar uma alteração indevida e apresentar sucesso.
3. Preservar fixture/evidências para inspeção. Não excluir origem real, histórico anterior ou caches do agente. Eventual exclusão da fixture exige decisão explícita do DEV e validação de que o caminho absoluto fica sob a raiz sintética desta rodada.
4. Para repetir, gerar nova raiz sintética exclusiva e registrar novos caminhos/versão.

## Registro humano e aceite

Preencher somente após execução real: responsável; data/hora/fuso; versão; cenário; resultado esperado/observado; evidência; defeito e reteste quando necessário.
O aceite será registrado separadamente em `acceptance/US6.md`. A IA não concede aceite nem marca EXECUTADO_PELO_DEV. Se o DEV confirmar impossibilidade, usar o template CEMG próprio e decidir AGUARDAR, REJEITAR ou ACEITAR_COM_RESSALVAS, mantendo resultados reais.
