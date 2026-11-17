# Plano de teste manual — US1: Carregar e preservar o projeto — Prioridade P1

**Estado: ESCRITO / RASCUNHO. Roteiro não conferido no produto e não executado pelo DEV.**
**Estado da implementação:** migração para constituição 4.0.0 validada; checkpoint técnico até T012; roteiro do incremento em [T012-dev.md](T012-dev.md). A história completa ainda depende das tarefas restantes; não usar este roteiro como evidência de execução/aceite.

## Versão e escopo

Base de desenvolvimento: `197548db2e3efa20c23e1f66b6a0ffb86c0f328d` + alterações locais de implementação iniciadas em 2026-11-16. Consultar `evidence/implementation-code-manifest.json` para os hashes deste checkpoint. O checkpoint técnico até T012 está identificado; a revisão funcional final da história ainda depende das tarefas restantes. Registrar o commit/manifesto realmente testado ao executar.

Cobertura planejada: todos os cenários ativos de US1. Não equivale a aceite, produto pronto ou teste aprovado.

## Preparação passo a passo

1. Após liberar as dependências desta história em tasks.md, obter a revisão identificada e conferir Windows x64 e SDK estável com `dotnet --info`.
2. Na raiz de desenvolvimento, executar `dotnet restore SpecAudit.slnx --locked-mode` e `dotnet test SpecAudit.slnx --no-restore`. A suíte ativa deve passar antes da validação funcional; os testes de sandbox v3 arquivados não são requisitos da política v4.
3. Criar somente dados sintéticos seguindo a receita em [quickstart.md](../quickstart.md#preparar-dados-sintéticos--ação-do-dev-fora-da-origem-real). Guardar os caminhos absolutos e baseline em dev-evidence. Repetir com novo GUID para o segundo projeto quando necessário. Não usar projeto real ou dados pessoais.
4. Preparar os dados específicos dos cenários abaixo. Gates negativos, corpus Mermaid e servidor de imagem devem ser sintéticos/controlados. Se uma dependência não existir, registrar NÃO EXECUTADO e o motivo; não instalar ou elevar automaticamente.
5. Iniciar o próprio SpecAudit: `dotnet run --project SpecAudit/SpecAudit.csproj --no-launch-profile -- --urls http://127.0.0.1:5187`. Abrir esse endereço no navegador local. Não executar comandos da solução auditada.
6. Escolher origem sintética e destino externo pelo picker nativo quando estiver implementado e habilitado. Usar o perfil local do usuário; autenticação de agente só será necessária para cenários analyze da US5, pela integração validada, sem copiar tokens.

## Execução por cenário

### US1-S01 — US1-AC01

**Estado inicial/dados:** Projeto com duas funcionalidades.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Na seleção de projeto, escolher a origem sintética e o destino reports externo. | O destino não é a origem nem seu descendente. |
| 2 | Carregar o projeto e selecionar cada uma das duas funcionalidades. | Artefatos presentes/ausentes aparecem por funcionalidade; não existe gravação na origem. |
| 3 | Comparar o conjunto de arquivos e hashes com baseline.json. | Nenhum arquivo da origem foi criado, alterado, renomeado ou excluído. |

Registrar para US1-S01: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US1-S02 — US1-AC02

**Estado inicial/dados:** Link que resolve para origem e executor restrito.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Selecionar a própria origem como destino e depois uma subpasta dela. | Ambas as seleções são recusadas, sem criar relatório. |
| 2 | Repetir usando uma junção que resolve para a origem. | O destino final também é recusado. |
| 3 | Executar os testes ReaderBoundaryTests na fixture Windows, incluindo o cenário readonly da aplicação sob ACL ampla. | O catálogo/broker não abre código/configuração, os fluxos da aplicação não gravam na origem e destinos internos são recusados. A política v4 não promete negação de acesso pelo kernel a agentes arbitrários; não alterar ACL de solução real. |

Registrar para US1-S02: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US1-S03 — US1-AC03

**Estado inicial/dados:** Dois projetos e histórico salvo.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Carregar o primeiro projeto com seu destino externo; depois carregar o segundo projeto sintético. | Cada projeto mantém funcionalidades, seleção e resultados próprios. |
| 2 | Voltar ao primeiro projeto e escolher outro destino externo. | A nova pasta passa a ser o destino; nenhum histórico anterior é movido ou excluído. |
| 3 | Conferir as duas origens e os diretórios de histórico já existentes. | Conteúdo anterior e hashes preservados; não há dados cruzados entre projetos. |

Registrar para US1-S03: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US1-S04 — US1-AC04

**Estado inicial/dados:** Análise em andamento.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Iniciar uma leitura/verificação sobre a fixture; editar um documento SpecKit com editor externo. | A edição normal do usuário continua permitida. |
| 2 | Aguardar a atualização documental e consultar o resultado anterior. | Mudança gera Changed/Stale ou resultado inconclusivo, sem mistura silenciosa de versões. |
| 3 | Conferir o conteúdo editado e a ausência de execução automática. | O aplicativo não restaura o arquivo nem dispara agente/gate por causa da edição. |

Registrar para US1-S04: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

## Evidências da IA e limitações

- Estado global de execução: [implementation-status.md](../evidence/implementation-status.md).
- Evidência específica da história será consolidada em `evidence/US1-validation.md` e `evidence/US1-review.md` quando as respectivas tasks forem executadas. Esses arquivos não são presumidos existentes.
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
| US1-S01 / US1-AC01 | | | | | | | |
| US1-S02 / US1-AC02 | | | | | | | |
| US1-S03 / US1-AC03 | | | | | | | |
| US1-S04 / US1-AC04 | | | | | | | |

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

Quando solicitado pelo DEV, a IA deverá conferir esta declaração contra a versão, os resultados e as evidências referenciadas e transcrever a decisão para `acceptance/US1.md`, preservando autoria, data, escopo e referência a esta seção. Havendo inconsistência ou informação essencial ausente, deverá apontar a pendência antes de registrar o aceite como concluído. A transcrição não altera os resultados dos testes nem equivale a aprovação de PR ou autorização de publicação.

Não há monitoramento automático deste arquivo. A IA não concede aceite por conta própria nem marca EXECUTADO_PELO_DEV sem referência à execução humana real. Se o DEV confirmar impossibilidade, usar o template CEMG próprio e decidir AGUARDAR, REJEITAR ou ACEITAR_COM_RESSALVAS, mantendo os resultados reais; eventual aceite com ressalvas deve ser declarado explicitamente acima.
