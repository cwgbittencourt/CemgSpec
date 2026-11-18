# Estado técnico e aceite — US1: carregar e preservar o projeto

Atualização técnica T019: fonte `sha256:fbdf9dd12adb61c19c2eee94ffbd7ba6c971a3cd013b2c6946b85ec6abc79b40` sobre `0bf951ec4704e118fd80f63e59cc6819341fc93a`;57/57 testes e6/6 manutenção aprovados, dois achados PR-A resolvidos. Ver [rodada atual](../evidence/PR-A-validation.md) e [review](../evidence/PR-A-review.md). T020 e decisões humanas pendentes. A consolidação T016 abaixo é histórica; campos humanos preservados.

Consolidação T016, 2026-11-17, America/Sao_Paulo. Produto/testes: commit `f2b29a20e5f9e724a9c32f8d8e3a703e0587d541`, revisão de código `sha256:530031e68a790d3b2b186300c23441091ef951d6fb93fdc1bae4029b11b451f8`. Documentação/evidência T016 local; nenhum código de produto/teste alterado. Referência de bytes: [manifesto corrente](../evidence/maintenance-code-manifest.json).

| Dimensão | Estado técnico | Evidência |
|---|---|---|
| Implementação funcional US1 / correções | CONCLUÍDA no escopo T011–T015 | [Tasks](../tasks.md), [resolução dos dois achados](../evidence/T015-findings.json) |
| Validação da IA | APROVADA no caminho de harness da US1 | T016: restore, quatro cenários 4/4, correções 7/7 e suíte 52/52; [execução](../evidence/T016-test-run.json) |
| Review da US1 | CONCLUÍDA, modalidade AUTORREVISAO, sem achado impeditivo conhecido aberto | [Review US1](../evidence/US1-review.md), [conferência documental T016](../evidence/T016-verification.json) |
| Roteiro do DEV | DISPONÍVEL; caminho principal CONFERIDO_PELA_IA | [US1-dev.md](../validation/US1-dev.md); exploração GUI adicional tem CONFERENCIA_PARCIAL explícita |
| Prontidão para executar o roteiro | SIM, no ambiente e na revisão indicados | Mesmo caminho de execução, dados sintéticos novos, asserções e observação entregues ao DEV |
| Fechamento/review do PR-A | PENDENTE T017–T020 | Verificação, revisão e descrição do incremento ainda são tasks próprias |
| Execução pelo DEV | PENDENTE | Campos humanos do roteiro em branco |
| Prontidão para aceite normal | NÃO nesta consolidação | Fechamento do incremento e execução/decisão humanas pendentes; não inferir prontidão final do simples preenchimento deste arquivo |
| Aceite humano | PENDENTE | Nenhuma decisão atribuída ou autenticada pela IA |

O caminho principal é de HISTÓRIA US1 técnica (AC01–AC04), por harness real, permitido pelos contratos e políticas. Não é apenas escrita de testes ou compilação: a IA iniciou e acionou APIs reais, native picker, filesystem/monitor Windows e o host/Edge da regressão. O S04 cobre trabalho documental em andamento e snapshots, não execução de agentes. Save/analyze/check_gate de histórias futuras continuam fora deste aceite. A conferência parcial da exploração manual GUI não é apresentada como execução completa dessa modalidade nem dispensada por exceção inferida.

## Critérios e escopo da comprovação

| Critério | Comprovação técnica conferida | Limite |
|---|---|---|
| US1-AC01 | Seleção nativa de origem/destino, duas funcionalidades e artefato ausente; baseline intacta | Navegação manual de cada sessão GUI carregada não declarada executada pela IA |
| US1-AC02 | Destino igual/descendente/junção recusado; broker readonly, código/config fora do catálogo e IDs alheios rejeitados | Controles de aplicação v4; não há garantia de confinamento OS de agente comprometido |
| US1-AC03 | Contextos separados, alteração de destino, hash/local do histórico sintético preservados | Histórico foi preparado pelo harness, não produzido por Save simulado |
| US1-AC04 | Edição real durante trabalho documental invalida versão sem restaurar; snapshot anterior imutável, cancelamento e ausência de relatório | Integração com Run/Save/agentes futuros não foi antecipada |

Os limites de erro e recuperação da T015 foram repetidos: metadados irrelevantes mantêm documentos atuais; erro de tamanho/grafo em Open/Refresh informa a dimensão, conserva contexto/rollback e permite nova ação. Resultados históricos negativos continuam em suas rodadas; a versão nova não reescreve falhas antigas como sucesso.

## PRs e aprovações vinculados

| PR previsto | Base e revisão | Review da IA | Aprovação humana | Integração |
|---|---|---|---|---|
| PR-A — fronteira, seleção e invalidação | Base planejada 79d9c3d; produto/testes corrigidos f2b29a2, manifesto acima | US1 T014/T015, conferência de roteiro T016; review de fechamento T018 pendente | PENDENTE | PENDENTE |

Não há URL de PR inventada ou publicação autorizada por esta consolidação. Aprovação de PR, integração, execução do DEV e aceite funcional são decisões distintas. Nenhum CI, proteção de branch ou mecanismo de autenticação de aceite foi instalado por esta task.

## Decisão humana — reservada ao DEV

Preencher somente a partir de decisão explícita referente à versão efetivamente executada. Os campos permanecem em branco nesta entrega.

- Decisão:
- Responsável:
- Data/hora e fuso:
- Commit/diff/manifesto efetivamente validado:
- Rodada e referência à execução humana:
- Fonte atribuível da decisão:
- Justificativa e escopo:
- Ressalvas/riscos e ações posteriores, se aplicável:

## Fonte da declaração e transcrição

A execução por cenário e a declaração explícita de ACEITO, REJEITADO ou ACEITO_COM_RESSALVAS devem ser registradas em [US1-dev.md](../validation/US1-dev.md), nas seções humanas. PASSOU em teste ou campo vazio não é aceite. Quando o DEV solicitar transcrição, a IA confere versão, autoria declarada, data/fuso, resultados, evidências e hash do documento-fonte e registra a decisão aqui sem mudar seu sentido. Inconsistências mantêm a pendência. Não há monitoramento automático do arquivo ou autenticação de autoria humana concedida pelo preset.

## Exceções e impossibilidade

Nenhuma exceção ou impossibilidade confirmada pelo DEV. O caminho principal foi executado pela IA. Se o DEV não conseguir reproduzir, registrar o cenário, tentativas, causa, evidências, risco e próximo passo; a IA pode preparar rascunho do template de impossibilidade, mas só o DEV confirma e decide AGUARDAR, REJEITAR ou ACEITAR_COM_RESSALVAS. NÃO EXECUTADO nunca vira PASSOU por essa declaração.

## Mudança posterior

Mudança de código/testes ou ambiente relevante exige avaliação de impacto e renovação das evidências afetadas. Alterar documentação não concede aceite a outra revisão. Preservar as rodadas existentes; não sobrescrever uma futura decisão humana. O gate corrente de PR-A permanece BLOCKED por fechamento e decisões pendentes, enquanto a execução técnica da T016 está concluída.
