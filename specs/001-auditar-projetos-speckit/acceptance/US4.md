# Estado técnico e aceite — US4: consultar ficha documental do requisito

Consolidação T042, 2026-11-20 America/Sao_Paulo. Produto e testes no commit `6d1ebf9a3a40017d04eeef7f87eb47626387a873` (sem alteração de código de produção ou testes nesta consolidação).

| Dimensão | Estado técnico | Evidência |
|---|---|---|
| Implementação funcional US4 / correções | CONCLUÍDA no escopo T036–T041 | [Tasks](../tasks.md), commits `93dd7fd`, `22b5246`, `fa38766`, `6d1ebf9` |
| Validação da IA | APROVADA na revisão `6d1ebf9` (tema escuro) | [US4-validation.md](../evidence/US4-validation.md) — 4/4 domínio, 4/4 US4 E2E, 93/93 regressão |
| Review da US4 | CONCLUÍDA, modalidade AUTORREVISAO; `REV-US4-01` e `REV-US4-02` RESOLVIDOS | [US4-review.md](../evidence/US4-review.md) |
| Roteiro do DEV | DISPONÍVEL; caminho automatizado CONFERIDO_PELA_IA; percurso manual CONFERENCIA_PARCIAL | [US4-dev.md](../validation/US4-dev.md) |
| Fechamento/review do PR-B | PENDENTE (US8 e T049–T052) | Próxima fase fecha o PR-B |
| Execução pelo DEV | PENDENTE | Campos humanos do roteiro em branco |
| Prontidão para aceite normal | NÃO nesta consolidação | Comparação no tema claro pendente da US8/T044; fechamento PR-B e execução/decisão humana pendentes |
| Aceite humano | PENDENTE | Nenhuma decisão atribuída ou autenticada pela IA |

## Critérios e escopo da comprovação

| Critério | Comprovação técnica conferida | Limite |
|---|---|---|
| US4-AC01 | Ficha em memória com descrição, critérios, vínculos explícitos/inferidos, ocorrências, verificações e sugestões; separação de `FR-001` por funcionalidade | Comparação visual no tema claro pendente da US8 (T044) |
| US4-AC02 | `CoverageState` restrito a `Linked`/`MissingLink`/`Ambiguous`; `Evidência documental insuficiente` sem `Não implementado`; duplicidades mantidas como `Ambíguo` | Exploração manual livre do DEV pendente |
| US4-AC03 | Declarações registradas exibidas como `registro de terceiros` com fonte e revisão, separadas de verificações futuras; SpecAudit não aprova | Execução de `speckit-analyze` e `check_gate` pertence à US5 (PR-C/D) |
| US4-AC04 | Referências a `.cs`/`.razor`/config permanecem `OutOfScope` (`Não verificada`) sem abrir arquivo-fonte (comprovado pelo observador do broker); análise de código informada como `capacidade futura` | Análise de código está fora da V1 |

## PRs e aprovações vinculados

| PR previsto | Base e revisão | Review da IA | Aprovação humana | Integração |
|---|---|---|---|---|
| PR-B — Documentos, Dashboard, Ficha e Navegação/Temas | Base após PR-A `ddf8f31` / `73e06b9`; US2 (`d2a34bc`), US3 (`32170ad`) e US4 (`6d1ebf9`) concluídas tecnicamente | US2, US3 e US4 concluídas; US8 e fechamento PR-B (T049–T052) pendentes | PENDENTE | PENDENTE |

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

A execução por cenário e a declaração explícita (`ACEITO`, `REJEITADO` ou `ACEITO_COM_RESSALVAS`) devem ser registradas em [US4-dev.md](../validation/US4-dev.md). A IA não concede aceite humano, não aprova PR e não marca `EXECUTADO_PELO_DEV` sem declaração humana explícita e atribuível.

