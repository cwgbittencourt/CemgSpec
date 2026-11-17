# Plano de teste manual — US4: Consultar ficha documental do requisito — Prioridade P1

**Estado: ESCRITO / RASCUNHO. Roteiro não conferido no produto e não executado pelo DEV.**
**Estado da implementação:** migração para constituição 4.0.0 validada; trabalho parado após T011 por solicitação do usuário. A história completa ainda depende das tarefas restantes; não usar este roteiro como evidência de execução/aceite.

## Versão e escopo

Base de desenvolvimento: `197548db2e3efa20c23e1f66b6a0ffb86c0f328d` + alterações locais de implementação iniciadas em 2026-11-16. Consultar `evidence/implementation-code-manifest.json` para os hashes deste checkpoint. O checkpoint técnico até T011 está identificado; a revisão funcional final da história ainda depende das tarefas restantes. Registrar o commit/manifesto realmente testado ao executar.

Cobertura planejada: todos os cenários ativos de US4. Não equivale a aceite, produto pronto ou teste aprovado.

## Preparação passo a passo

1. Após liberar as dependências desta história em tasks.md, obter a revisão identificada e conferir Windows x64 e SDK estável com `dotnet --info`.
2. Na raiz de desenvolvimento, executar `dotnet restore SpecAudit.slnx --locked-mode` e `dotnet test SpecAudit.slnx --no-restore`. A suíte ativa deve passar antes da validação funcional; os testes de sandbox v3 arquivados não são requisitos da política v4.
3. Criar somente dados sintéticos seguindo a receita em [quickstart.md](../quickstart.md#preparar-dados-sintéticos--ação-do-dev-fora-da-origem-real). Guardar os caminhos absolutos e baseline em dev-evidence. Repetir com novo GUID para o segundo projeto quando necessário. Não usar projeto real ou dados pessoais.
4. Preparar os dados específicos dos cenários abaixo. Gates negativos, corpus Mermaid e servidor de imagem devem ser sintéticos/controlados. Se uma dependência não existir, registrar NÃO EXECUTADO e o motivo; não instalar ou elevar automaticamente.
5. Iniciar o próprio SpecAudit: `dotnet run --project SpecAudit/SpecAudit.csproj --no-launch-profile -- --urls http://127.0.0.1:5187`. Abrir esse endereço no navegador local. Não executar comandos da solução auditada.
6. Escolher origem sintética e destino externo pelo picker nativo quando estiver implementado e habilitado. Usar o perfil local do usuário; autenticação de agente só será necessária para cenários analyze da US5, pela integração validada, sem copiar tokens.

## Execução por cenário

### US4-S01 — US4-AC01

**Estado inicial/dados:** Requisito com vínculos explícitos/inferidos.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Abrir a ficha de um requisito com vínculos explícitos e sugestões inferidas. | Descrição, história, critérios, plano, tarefas e ocorrências aparecem no mesmo contexto. |
| 2 | Consultar os vínculos e suas fontes. | Explícito e inferido são diferenciados e remetem aos documentos corretos. |

Registrar para US4-S01: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US4-S02 — US4-AC02

**Estado inicial/dados:** Critério sem tarefa e outro ambíguo.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Abrir requisito com critério sem tarefa e outro com vínculo ambíguo. | Cada critério é apresentado individualmente. |
| 2 | Consultar a cobertura de cada critério. | MissingLink/Ambiguous ou rótulo equivalente é informado; não concluir implementado/não implementado. |

Registrar para US4-S02: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US4-S03 — US4-AC03

**Estado inicial/dados:** Documento declara validação sem prova acessível.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Abrir documento que declara validação/aceite, sem evidência verificável disponível. | A declaração é mostrada com procedência e versão. |
| 2 | Consultar a consolidação da ficha. | A declaração permanece separada do gate calculado; ausência de prova não vira aprovação pelo SpecAudit. |

Registrar para US4-S03: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US4-S04 — US4-AC04

**Estado inicial/dados:** Documentos de projetos em linguagens diferentes e link .cs.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Carregar dois projetos sintéticos cujos documentos representem linguagens diferentes. | A análise documental mantém o mesmo alcance e comportamento. |
| 2 | Acionar uma referência .cs ou .razor na ficha. | O destino não é aberto; referência fica não verificada e análise de código é informada como futura. |

Registrar para US4-S04: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

## Evidências da IA e limitações

- Estado global de execução: [implementation-status.md](../evidence/implementation-status.md).
- Evidência específica da história será consolidada em `evidence/US4-validation.md` e `evidence/US4-review.md` quando as respectivas tasks forem executadas. Esses arquivos não são presumidos existentes.
- Os passos da UI acima descrevem o contrato aprovado; nomes, localização final de controles e capturas precisam ser conferidos na implementação. Este documento não comprova esse comportamento.
- Divergências entre ambiente da IA e DEV devem ser registradas, sem prometer reprodução só pela escrita do roteiro.

## Limpeza e repetição

1. Encerrar somente a instância do host e os descendentes pertencentes a esta rodada. Conferir término confirmado no relatório técnico.
2. Comparar arquivos/hashes da origem com baseline; separar edições intencionais do DEV. Não restaurar uma alteração indevida e apresentar sucesso.
3. Preservar fixture/evidências para inspeção. Não excluir origem real, histórico anterior ou caches do agente. Eventual exclusão da fixture exige decisão explícita do DEV e validação de que o caminho absoluto fica sob a raiz sintética desta rodada.
4. Para repetir, gerar nova raiz sintética exclusiva e registrar novos caminhos/versão.

## Registro humano e aceite

Preencher somente após execução real: responsável; data/hora/fuso; versão; cenário; resultado esperado/observado; evidência; defeito e reteste quando necessário.
O aceite será registrado separadamente em `acceptance/US4.md`. A IA não concede aceite nem marca EXECUTADO_PELO_DEV. Se o DEV confirmar impossibilidade, usar o template CEMG próprio e decidir AGUARDAR, REJEITAR ou ACEITAR_COM_RESSALVAS, mantendo resultados reais.
