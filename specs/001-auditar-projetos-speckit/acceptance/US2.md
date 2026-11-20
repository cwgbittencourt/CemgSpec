# Estado técnico e aceite — US2: ler documentos e diagramas

Consolidação T028, 2026-11-19 America/Sao_Paulo. Produto e testes no commit `62c6e3ca264220ab939ca6a056e9d59ddae8ef71` (sem alteração de código de produção ou testes nesta consolidação).

| Dimensão | Estado técnico | Evidência |
|---|---|---|
| Implementação funcional US2 / correções | CONCLUÍDA no escopo T021–T027 | [Tasks](../tasks.md), commits `9e4ffbd`, `3803ffb`, `14865b7`, `62c6e3c` |
| Prova de segurança do preview | PASSOU (T023) | [preview-safety.md](../evidence/preview-safety.md) |
| Validação da IA | APROVADA na revisão `62c6e3c` (tema escuro) | [US2-validation.md](../evidence/US2-validation.md) — 4/4 domínio, 15/15 US2 E2E/assets, 75/75 regressão |
| Review da US2 | CONCLUÍDA, modalidade AUTORREVISAO; `REV-US2-01` e `REV-US2-02` RESOLVIDOS | [US2-review.md](../evidence/US2-review.md) |
| Roteiro do DEV | DISPONÍVEL; caminho automatizado CONFERIDO_PELA_IA; percurso manual CONFERENCIA_PARCIAL | [US2-dev.md](../validation/US2-dev.md) |
| Fechamento/review do PR-B | PENDENTE (US3, US4, US8 e T049–T052) | PR-B ainda em construção |
| Execução pelo DEV | PENDENTE | Campos humanos do roteiro em branco |
| Prontidão para aceite normal | NÃO nesta consolidação | Comparação no tema claro pendente da US8/T044; fechamento PR-B e execução/decisão humana pendentes |
| Aceite humano | PENDENTE | Nenhuma decisão atribuída ou autenticada pela IA |

## Critérios e escopo da comprovação

| Critério | Comprovação técnica conferida | Limite |
|---|---|---|
| US2-AC01 | Árvore e busca somente sobre o catálogo; exclusão de código; links relativos, âncoras locais/cruzadas, ausência e fora do catálogo com estados distintos | Exploração manual livre do DEV pendente |
| US2-AC02 | Preview, Fonte e Lado a lado somente leitura; elementos Markdown, tarefas desabilitadas, destaque de código e PNG local via broker; preservação de estado e limpeza de listeners em trocas de modo (`REV-US2-01` resolvido) | Comparação visual no tema claro pendente da US8 (T044) |
| US2-AC03 | Mermaid local offline em iframe opaco `sandbox="allow-scripts"` com zoom e fonte; fallback com explicação para Mermaid inválido, PlantUML e Graphviz | Limite de 12 diagramas por documento e 20.000 caracteres por diagrama |
| US2-AC04 | HTML ativo removido; scripts/formulários inertes; rede automática zero; imagem remota mediada pelo servidor (`RemotePreviewImages`) sem cookies/auth/referer/redirecionamentos e apenas IPs públicos; link externo em nova aba somente após clique | Respostas externas controladas localmente nos testes para manter execução offline |

## PRs e aprovações vinculados

| PR previsto | Base e revisão | Review da IA | Aprovação humana | Integração |
|---|---|---|---|---|
| PR-B — Documentos, Dashboard, Ficha e Navegação/Temas | Base após PR-A `ddf8f31` / `73e06b9`; US2 concluída tecnicamente em `62c6e3c` | US2 T026/T027 concluída; fechamento PR-B (T049–T052) pendente | PENDENTE | PENDENTE |

## Decisão humana — reservada ao DEV

Preencher somente a partir de decisão explícita referente à versão efetivamente executada. Todos os campos permanecem em branco nesta entrega.

- Decisão:
- Responsável:
- Data/hora e fuso:
- Commit/diff/manifesto efetivamente validado:
- Rodada e referência à execução humana:
- Fonte atribuível da decisão:
- Justificativa e escopo:
- Ressalvas/riscos e ações posteriores, se aplicável:

## Fonte da declaração e transcrição

A execução por cenário e a declaração explícita (`ACEITO`, `REJEITADO` ou `ACEITO_COM_RESSALVAS`) devem ser registradas em [US2-dev.md](../validation/US2-dev.md). A IA não concede aceite humano, não aprova PR e não marca `EXECUTADO_PELO_DEV` sem declaração humana explícita e atribuível.

