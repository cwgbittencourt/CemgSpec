# Plano de teste manual — US2: Ler documentos e diagramas — Prioridade P1

**Estado: ESCRITO / RASCUNHO. Roteiro não conferido no produto e não executado pelo DEV.**
**Estado da implementação:** migração para constituição 4.0.0 validada; trabalho parado após T011 por solicitação do usuário. A história completa ainda depende das tarefas restantes; não usar este roteiro como evidência de execução/aceite.

## Versão e escopo

Base de desenvolvimento: `197548db2e3efa20c23e1f66b6a0ffb86c0f328d` + alterações locais de implementação iniciadas em 2026-11-16. Consultar `evidence/implementation-code-manifest.json` para os hashes deste checkpoint. O checkpoint técnico até T011 está identificado; a revisão funcional final da história ainda depende das tarefas restantes. Registrar o commit/manifesto realmente testado ao executar.

Cobertura planejada: todos os cenários ativos de US2. Não equivale a aceite, produto pronto ou teste aprovado.

## Preparação passo a passo

1. Após liberar as dependências desta história em tasks.md, obter a revisão identificada e conferir Windows x64 e SDK estável com `dotnet --info`.
2. Na raiz de desenvolvimento, executar `dotnet restore SpecAudit.slnx --locked-mode` e `dotnet test SpecAudit.slnx --no-restore`. A suíte ativa deve passar antes da validação funcional; os testes de sandbox v3 arquivados não são requisitos da política v4.
3. Criar somente dados sintéticos seguindo a receita em [quickstart.md](../quickstart.md#preparar-dados-sintéticos--ação-do-dev-fora-da-origem-real). Guardar os caminhos absolutos e baseline em dev-evidence. Repetir com novo GUID para o segundo projeto quando necessário. Não usar projeto real ou dados pessoais.
4. Preparar os dados específicos dos cenários abaixo. Gates negativos, corpus Mermaid e servidor de imagem devem ser sintéticos/controlados. Se uma dependência não existir, registrar NÃO EXECUTADO e o motivo; não instalar ou elevar automaticamente.
5. Iniciar o próprio SpecAudit: `dotnet run --project SpecAudit/SpecAudit.csproj --no-launch-profile -- --urls http://127.0.0.1:5187`. Abrir esse endereço no navegador local. Não executar comandos da solução auditada.
6. Escolher origem sintética e destino externo pelo picker nativo quando estiver implementado e habilitado. Usar o perfil local do usuário; autenticação de agente só será necessária para cenários analyze da US5, pela integração validada, sem copiar tokens.

## Execução por cenário

### US2-S01 — US2-AC01

**Estado inicial/dados:** Links válidos e quebrados.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Abrir Documentos e expandir as funcionalidades do corpus. | Árvore apresenta os documentos/registros reconhecidos. |
| 2 | Pesquisar um termo conhecido; abrir o link relativo para plan.md e uma âncora existente. | Busca e navegação chegam ao documento/trecho esperado. |
| 3 | Abrir referência quebrada e referência para DoNotRead.cs. | Ausência/limite é informado; arquivo de código não é aberto. |

Registrar para US2-S01: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US2-S02 — US2-AC02

**Estado inicial/dados:** Corpus conhecido.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Abrir research.md em Preview e conferir títulos, listas, tabela, citação e bloco de código. | O conteúdo conhecido do corpus está representado, sem edição. |
| 2 | Trocar para Fonte e Lado a lado. | As mesmas informações permanecem disponíveis; a fonte é somente leitura. |
| 3 | Tentar alterar uma checkbox do documento no preview. | A tarefa permanece somente leitura e a origem não muda. |

Registrar para US2-S02: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US2-S03 — US2-AC03

**Estado inicial/dados:** Diagramas válidos/inválidos e não suportados.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Sem rede, abrir o bloco Mermaid válido do corpus. | O diagrama é renderizado com bibliotecas locais. |
| 2 | Acionar zoom e consultar a fonte do diagrama. | Zoom funciona e a fonte correspondente é legível. |
| 3 | Abrir Mermaid inválido e bloco PlantUML. | Fonte e explicação são exibidas; o aplicativo não inventa renderização suportada. |

Registrar para US2-S03: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

### US2-S04 — US2-AC04

**Estado inicial/dados:** Script e imagem remota.

| Passo | O que fazer | O que validar |
|---|---|---|
| 1 | Abrir o documento com script e imagem remota, monitorando a rede do navegador. | O script não executa e a imagem não é buscada automaticamente. |
| 2 | Autorizar explicitamente a imagem controlada nesta sessão. | Somente então ocorre a requisição esperada, sem credenciais herdadas. |
| 3 | Abrir um link externo por clique e comparar baseline. | O link abre em outra aba; a origem continua intacta. Sem servidor de imagem controlado, registrar NÃO EXECUTADO. |

Registrar para US2-S04: resultado observado, PASSOU/FALHOU/NÃO EXECUTADO, evidência e revisão avaliada. Não preencher como passou por existir este roteiro.

## Evidências da IA e limitações

- Estado global de execução: [implementation-status.md](../evidence/implementation-status.md).
- Evidência específica da história será consolidada em `evidence/US2-validation.md` e `evidence/US2-review.md` quando as respectivas tasks forem executadas. Esses arquivos não são presumidos existentes.
- Os passos da UI acima descrevem o contrato aprovado; nomes, localização final de controles e capturas precisam ser conferidos na implementação. Este documento não comprova esse comportamento.
- Divergências entre ambiente da IA e DEV devem ser registradas, sem prometer reprodução só pela escrita do roteiro.

## Limpeza e repetição

1. Encerrar somente a instância do host e os descendentes pertencentes a esta rodada. Conferir término confirmado no relatório técnico.
2. Comparar arquivos/hashes da origem com baseline; separar edições intencionais do DEV. Não restaurar uma alteração indevida e apresentar sucesso.
3. Preservar fixture/evidências para inspeção. Não excluir origem real, histórico anterior ou caches do agente. Eventual exclusão da fixture exige decisão explícita do DEV e validação de que o caminho absoluto fica sob a raiz sintética desta rodada.
4. Para repetir, gerar nova raiz sintética exclusiva e registrar novos caminhos/versão.

## Registro humano e aceite

Preencher somente após execução real: responsável; data/hora/fuso; versão; cenário; resultado esperado/observado; evidência; defeito e reteste quando necessário.
O aceite será registrado separadamente em `acceptance/US2.md`. A IA não concede aceite nem marca EXECUTADO_PELO_DEV. Se o DEV confirmar impossibilidade, usar o template CEMG próprio e decidir AGUARDAR, REJEITAR ou ACEITAR_COM_RESSALVAS, mantendo resultados reais.
