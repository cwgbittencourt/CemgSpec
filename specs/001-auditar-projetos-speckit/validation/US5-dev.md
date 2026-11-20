# Plano de teste manual — US5: Selecionar e executar as duas verificações permitidas — Prioridade P1

**Estado: CONFERIDO PELA IA (T074) — NÃO EXECUTADO PELO DEV.**
**Estado da implementação:** `T053–T074` concluídas tecnicamente na revisão `e399c67` (`main`), com provas `GateAdapter` (`7/7`), `AnalyzeCodex` (`2/2`) e `AnalyzeAntigravity` (`9/9`) registradas. Decisão e execução independente pelo DEV permanecem **PENDENTES**.

## Versão e escopo

Revisão conferida pela IA: `e399c67` (`main`, 2026-11-20 America/Sao_Paulo). Evidências vinculadas: [US5-validation.md](../evidence/US5-validation.md), [US5-review.md](../evidence/US5-review.md), [GateAdapter-proof.md](../evidence/GateAdapter-proof.md), [AnalyzeCodex-proof.md](../evidence/AnalyzeCodex-proof.md), [AnalyzeAntigravity-proof.md](../evidence/AnalyzeAntigravity-proof.md). Registrar o commit realmente testado ao executar pelo DEV.

Cobertura planejada: todos os cenários ativos de US5. Não equivale a aceite, produto pronto ou teste aprovado.

## Preparação passo a passo

1. Após liberar as dependências desta história em tasks.md, obter a revisão identificada e conferir Windows x64 e SDK estável com `dotnet --info`.
2. Na raiz de desenvolvimento, executar `dotnet restore SpecAudit.slnx --locked-mode` e `dotnet test SpecAudit.slnx --no-restore`. A suíte ativa deve passar antes da validação funcional; os testes de sandbox v3 arquivados não são requisitos da política v4.
3. Criar somente dados sintéticos seguindo a receita em [quickstart.md](../quickstart.md#preparar-dados-sintéticos--ação-do-dev-fora-da-origem-real). Guardar os caminhos absolutos e baseline em dev-evidence. Repetir com novo GUID para o segundo projeto quando necessário. Não usar projeto real ou dados pessoais.
4. Preparar os dados específicos dos cenários abaixo. Gates negativos, corpus Mermaid e servidor de imagem devem ser sintéticos/controlados. Se uma dependência não existir, registrar NÃO EXECUTADO e o motivo; não instalar ou elevar automaticamente.
5. Iniciar o próprio SpecAudit: `dotnet run --project SpecAudit/SpecAudit.csproj --no-launch-profile -- --urls http://127.0.0.1:5187`. Abrir esse endereço no navegador local. Não executar comandos da solução auditada.
6. Escolher origem sintética e destino externo pelo picker nativo quando estiver implementado e habilitado. Usar o perfil local do usuário; autenticação de agente só será necessária para cenários analyze da US5, pela integração validada, sem copiar tokens.

## Execução por cenário

### US5-S01 — US5-AC01

**Estado inicial/dados:** Agente encontrado e outro ausente.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Abrir a seleção de agente com um agente conhecido instalado e outro não localizado. | Descoberta mostra identidade, versão, compatibilidade e limitações. |
| 2 | Solicitar seleção manual somente do agente não localizado. | Fallback é limitado ao agente desejado ausente. |
| 3 | Selecionar check_gate na combo. | Agente e autenticação deixam de ser exigidos para o gate. |

Registrar para US5-S01: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US5-S02 — US5-AC02

**Estado inicial/dados:** Dois agentes reais e integração incompatível.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Executar analyze sobre corpus sintético com Codex e depois Antigravity, cada qual com proteção comprovada. | Ambos produzem análise documental real com saída/erro e limites. |
| 2 | Repetir com adaptador sem prova ou incompatível. | Somente esse adaptador fica indisponível, com motivo; leitor e gate permanecem independentes. |
| 3 | Conferir evidências de proteção, estado externo e cancelamento de cada versão. | Nenhuma simulação é apresentada como integração real. Falta de agente/autenticação mantém NÃO EXECUTADO. |

Registrar para US5-S02: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US5-S03 — US5-AC03

**Estado inicial/dados:** Verificações e ficha abertas.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Abrir Verificações e a ficha de requisito do mesmo projeto. | Ambas apresentam a mesma seleção de verificação. |
| 2 | Selecionar cada opção da combo sem clicar Executar. | Existem exatamente analyze/check_gate; selecionar não inicia processo. |
| 3 | Conferir ausência de campo livre, terceira opção e cadeia de comandos; clicar Executar em escopo preparado. | Somente o clique explícito dispara a opção escolhida; progresso/saída/escopo ficam identificados. |

Registrar para US5-S03: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US5-S04 — US5-AC04

**Estado inicial/dados:** Documentos com instruções maliciosas e tasks ausente.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Carregar documentos com instruções maliciosas e referência para código. | Conteúdo é tratado como dados; não amplia ferramentas nem executa instruções. |
| 2 | Preparar analyze em funcionalidade sem tasks.md. | Execução é bloqueada por pré-requisito, sem criar o arquivo ausente. |
| 3 | Conferir estado interno do agente e baseline da origem. | Conversas/caches ficam externos e não viram relatório oficial salvo; origem intacta. |

Registrar para US5-S04: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US5-S05 — US5-AC05

**Estado inicial/dados:** Agente sem autenticação ou em execução.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Tentar analyze sem autenticação ou com agente incompatível. | Mostrar estado real e causa; não declarar requisito aprovado/reprovado. |
| 2 | Com integração disponível, iniciar analyze e solicitar cancelamento. | Só declarar cancelado após término dos descendentes próprios; término inconclusivo mantém novas execuções indisponíveis. |
| 3 | Abrir leitor e executar gate local. | Funções locais não dependem do agente indisponível. |

Registrar para US5-S05: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US5-S06 — US5-AC06

**Estado inicial/dados:** Registros conhecidos com READY, BLOCKED, hash adulterado e revisão divergente.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Escolher funcionalidade, etapa, registro ready.json e revisão conhecida da fixture. | Resumo informa alcance da funcionalidade/etapa e limites do gate. |
| 2 | Executar e comparar resultado e exit com o core oficial CEMG em fixture documental. | READY somente no caso válido, sem autenticar decisão humana ou testar implementação. |
| 3 | Repetir com blocked.json, bad-hash.json, wrong-revision.json e registro ausente. | BLOCKED/erro/não verificável conforme o caso; nunca forjar sucesso ou criar registros. |

Registrar para US5-S06: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US5-S07 — US5-AC07

**Estado inicial/dados:** Rede desativada e evidências documentais, .cs, link externo e alteração concorrente.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Sem rede, executar gate sobre evidências documentais válidas. | O gate funciona sem IA, autenticação, hooks ou rede. |
| 2 | Repetir com referência a .cs, recurso externo, junção para fora e referência proibida em stdout/artifact aninhado. | Bloquear antes de ler o destino proibido; nenhum READY parcial. |
| 3 | Alterar registro/evidência durante preparação/leitura. | Resultado é invalidado como Changed/Stale; a etapa escolhida não encadeia comandos. |

Registrar para US5-S07: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

## Evidências da IA e limitações

- Estado global de execução: [implementation-status.md](../evidence/implementation-status.md).
- Evidência específica da história será consolidada em `evidence/US5-validation.md` e `evidence/US5-review.md` quando as respectivas tasks forem executadas. Esses arquivos não são presumidos existentes.
- Os passos da UI acima descrevem o contrato aprovado; nomes, localização final de controles e capturas precisam ser conferidos na implementação. Este documento não comprova esse comportamento.
- Divergências entre ambiente da IA e DEV devem ser registradas, sem prometer reprodução só pela escrita do roteiro.

## Limpeza e repetição

1. Encerrar somente a instância do host e os descendentes pertencentes a esta rodada. Conferir término confirmado no relatório técnico.
2. Comparar arquivos/hashes da origem com baseline; separar edições intencionais do DEV. Não restaurar uma alteração indevida e apresentar sucesso.
3. Preservar fixture/evidências para inspeção. Não excluir origem real, histórico anterior ou caches do agente. Eventual exclusão da fixture exige decisão explícita do DEV e validação de que o caminho absoluto fica sob a raiz sintética desta rodada.
4. Para repetir, gerar nova raiz sintética exclusiva e registrar novos caminhos/versão.

## Registro humano e aceite

### Registro da execução humana

Preenchimento reservado ao DEV. Os campos abaixo permanecem em branco: a existência do roteiro e os testes da IA não representam execução humana.

Após avaliar cada cenário, registrar o resultado como **PASSOU**, **FALHOU** ou **NÃO EXECUTADO**, descrevendo o observado ou o motivo da não execução. Informar o hash do commit efetivamente testado; se houver alterações locais, identificar também o diff/manifesto. Registrar data/hora com fuso e links para capturas ou logs em `../evidence/`.

| Cenário / critério | Resultado | Resultado observado / motivo da não execução | Evidências (links) | Commit testado / diff ou manifesto | Responsável | Data/hora e fuso | Defeito / observações |
|---|---|---|---|---|---|---|---|
| US5-S01 / US5-AC01 | | | | | | | |
| US5-S02 / US5-AC02 | | | | | | | |
| US5-S03 / US5-AC03 | | | | | | | |
| US5-S04 / US5-AC04 | | | | | | | |
| US5-S05 / US5-AC05 | | | | | | | |
| US5-S06 / US5-AC06 | | | | | | | |
| US5-S07 / US5-AC07 | | | | | | | |

### Retestes e novas rodadas

Preservar os registros anteriores. Em caso de correção ou mudança de versão, acrescentar uma linha por cenário reexecutado, identificando a rodada e a nova versão.

| Rodada | Cenário / critério | Resultado | Resultado observado / motivo da não execução | Evidências (links) | Commit testado / diff ou manifesto | Responsável | Data/hora e fuso | Defeito / observações |
|---|---|---|---|---|---|---|---|---|
| | | | | | | | | |

### Decisão de aceite

Preenchimento reservado ao DEV, após conferir os resultados e a versão avaliada. Selecionar explicitamente **ACEITO**, **REJEITADO** ou **ACEITO_COM_RESSALVAS** no campo Decisão. Campos em branco significam decisão pendente; resultados PASSOU na tabela não concedem aceite automaticamente.

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

Quando solicitado pelo DEV, a IA deverá conferir esta declaração contra a versão, os resultados e as evidências referenciadas e transcrever a decisão para `acceptance/US5.md`, preservando autoria, data, escopo e referência a esta seção. Havendo inconsistência ou informação essencial ausente, deverá apontar a pendência antes de registrar o aceite como concluído. A transcrição não altera os resultados dos testes nem equivale a aprovação de PR ou autorização de publicação.

Não há monitoramento automático deste arquivo. A IA não concede aceite por conta própria nem marca EXECUTADO_PELO_DEV sem referência à execução humana real. Se o DEV confirmar impossibilidade, usar o template CEMG próprio e decidir AGUARDAR, REJEITAR ou ACEITAR_COM_RESSALVAS, mantendo os resultados reais; eventual aceite com ressalvas deve ser declarado explicitamente acima.
