# Plano de teste manual — US3: Acompanhar progresso documental e impedimentos — Prioridade P1

**Estado: ESCRITO / RASCUNHO. Roteiro não conferido no produto e não executado pelo DEV.**
**Estado da implementação:** migração para constituição 4.0.0 validada; trabalho parado após T011 por solicitação do usuário. A história completa ainda depende das tarefas restantes; não usar este roteiro como evidência de execução/aceite.

## Versão e escopo

Base de desenvolvimento: `197548db2e3efa20c23e1f66b6a0ffb86c0f328d` + alterações locais de implementação iniciadas em 2026-11-16. Consultar `evidence/implementation-code-manifest.json` para os hashes deste checkpoint. O checkpoint técnico até T011 está identificado; a revisão funcional final da história ainda depende das tarefas restantes. Registrar o commit/manifesto realmente testado ao executar.

Cobertura planejada: todos os cenários ativos de US3. Não equivale a aceite, produto pronto ou teste aprovado.

## Preparação passo a passo

1. Após liberar as dependências desta história em tasks.md, obter a revisão identificada e conferir Windows x64 e SDK estável com `dotnet --info`.
2. Na raiz de desenvolvimento, executar `dotnet restore SpecAudit.slnx --locked-mode` e `dotnet test SpecAudit.slnx --no-restore`. A suíte ativa deve passar antes da validação funcional; os testes de sandbox v3 arquivados não são requisitos da política v4.
3. Criar somente dados sintéticos seguindo a receita em [quickstart.md](../quickstart.md#preparar-dados-sintéticos--ação-do-dev-fora-da-origem-real). Guardar os caminhos absolutos e baseline em dev-evidence. Repetir com novo GUID para o segundo projeto quando necessário. Não usar projeto real ou dados pessoais.
4. Preparar os dados específicos dos cenários abaixo. Gates negativos, corpus Mermaid e servidor de imagem devem ser sintéticos/controlados. Se uma dependência não existir, registrar NÃO EXECUTADO e o motivo; não instalar ou elevar automaticamente.
5. Iniciar o próprio SpecAudit: `dotnet run --project SpecAudit/SpecAudit.csproj --no-launch-profile -- --urls http://127.0.0.1:5187`. Abrir esse endereço no navegador local. Não executar comandos da solução auditada.
6. Escolher origem sintética e destino externo pelo picker nativo quando estiver implementado e habilitado. Usar o perfil local do usuário; autenticação de agente só será necessária para cenários analyze da US5, pela integração validada, sem copiar tokens.

## Execução por cenário

### US3-S01 — US3-AC01

**Estado inicial/dados:** 30/40 tarefas marcadas e 12/20 requisitos vinculados.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Carregar o corpus de 40 tarefas, 30 marcadas, e 20 requisitos, 12 vinculados explicitamente. | O escopo e as contagens correspondem à fixture. |
| 2 | Abrir Dashboard e conferir os dois indicadores. | 75% de progresso declarado e 60% de cobertura documental; nenhuma medida é chamada de implementação comprovada. |
| 3 | Selecionar um escopo sem itens. | Exibir Sem itens, sem percentual inventado ou divisão por zero. |

Registrar para US3-S01: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US3-S02 — US3-AC02

**Estado inicial/dados:** Registro READY e declaração humana em documento.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Abrir o painel para documentos com registro READY e declaração humana textual. | Os registros são apresentados com fonte, revisão e atualidade. |
| 2 | Consultar o detalhe das validações/declarações. | READY/hash/texto não são tratados como teste executado ou aceite humano autenticado pelo SpecAudit. |

Registrar para US3-S02: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US3-S03 — US3-AC03

**Estado inicial/dados:** Contradição, vínculo ausente e referência a código.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Carregar documentos com contradição, requisito sem tarefa e referência a código. | As ocorrências documentais aparecem no escopo correto. |
| 2 | Abrir a fonte de cada ocorrência. | Fonte documental e limite da conclusão ficam identificáveis; código não é lido. |

Registrar para US3-S03: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US3-S04 — US3-AC04

**Estado inicial/dados:** Resultado anterior e documento alterado.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Selecionar filtros de funcionalidade, história, gravidade e situação existentes. | A lista muda de acordo com os dados documentais. |
| 2 | Editar externamente tasks.md e aguardar a atualização. | Contagens locais são renovadas e resultados afetados ficam desatualizados. |
| 3 | Observar processos/rede durante a alteração. | A atualização não dispara analyze nem check_gate. |

Registrar para US3-S04: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

## Evidências da IA e limitações

- Estado global de execução: [implementation-status.md](../evidence/implementation-status.md).
- Evidência específica da história será consolidada em `evidence/US3-validation.md` e `evidence/US3-review.md` quando as respectivas tasks forem executadas. Esses arquivos não são presumidos existentes.
- Os passos da UI acima descrevem o contrato aprovado; nomes, localização final de controles e capturas precisam ser conferidos na implementação. Este documento não comprova esse comportamento.
- Divergências entre ambiente da IA e DEV devem ser registradas, sem prometer reprodução só pela escrita do roteiro.

## Limpeza e repetição

1. Encerrar somente a instância do host e os descendentes pertencentes a esta rodada. Conferir término confirmado no relatório técnico.
2. Comparar arquivos/hashes da origem com baseline; separar edições intencionais do DEV. Não restaurar uma alteração indevida e apresentar sucesso.
3. Preservar fixture/evidências para inspeção. Não excluir origem real, histórico anterior ou caches do agente. Eventual exclusão da fixture exige decisão explícita do DEV e validação de que o caminho absoluto fica sob a raiz sintética desta rodada.
4. Para repetir, gerar nova raiz sintética exclusiva e registrar novos caminhos/versão.

## Registro humano e aceite

Preencher somente após execução real: responsável; data/hora/fuso; versão; cenário; resultado esperado/observado; evidência; defeito e reteste quando necessário.
O aceite será registrado separadamente em `acceptance/US3.md`. A IA não concede aceite nem marca EXECUTADO_PELO_DEV. Se o DEV confirmar impossibilidade, usar o template CEMG próprio e decidir AGUARDAR, REJEITAR ou ACEITAR_COM_RESSALVAS, mantendo resultados reais.
