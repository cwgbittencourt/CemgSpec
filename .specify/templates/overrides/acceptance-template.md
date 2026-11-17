<!-- Override local SpecAudit: cemg-speckit 3.0.0 + constituição 4.1.0. -->
# Estado e aceite — [US1]

Versão candidata: [commit + identificação do artefato/diff]

| Dimensão | Estado | Evidência |
|---|---|---|
| Implementação | PENDENTE / CONCLUIDA | [tasks e diff] |
| Validação da IA | PENDENTE / APROVADA / FALHOU / BLOQUEADA | [registro] |
| Code review | PENDENTE / CONCLUIDA / BLOQUEADA | [registro e achados] |
| Roteiro do DEV | PENDENTE / DISPONIVEL | [roteiro] |
| Prontidão para aceite normal | NAO / SIM | [justificativa] |
| Aceite humano | PENDENTE | [decisão abaixo] |

Prontidão SIM exige implementação concluída, validação aprovada, revisão da
versão candidata sem achados impeditivos abertos e roteiro disponível.
Não avançar para aceite normal com bloqueios silenciosos.

## Decisão humana — reservada ao DEV

- Decisão: PENDENTE / ACEITO / REJEITADO / ACEITO_COM_RESSALVAS
- Responsável:
- Data/hora e fuso:
- Versão efetivamente validada:
- Referência à execução humana ou à decisão de exceção explícita:
- Observações:

A IA pode transcrever decisão expressa do DEV com atribuição e referência.
Não pode deduzi-la da ausência de resposta ou dos próprios testes.

## Exceções decididas pelo DEV

Registrar condição não atendida, motivo, risco, escopo, responsável pela decisão
e ação posterior. Exceção não transforma teste não executado em aprovado.
Preservar os estados reais. ACEITO_COM_RESSALVAS só por decisão humana explícita.

## Mudança posterior

Uma nova versão exige avaliar impacto, renovar evidências afetadas e registrar
nova decisão quando alterar o que foi aceito. Preservar o histórico anterior.

## PRs e aprovações vinculados

| PR | Base e versão revisadas | Review da IA | Aprovação humana e referência | Integração e versão resultante |
|---|---|---|---|---|
| [ID/URL] | [hashes] | [registro] | PENDENTE | [pendente/hash] |

Registrar cada aprovação com autoria, data e fonte. Aprovação de PR não concede
aceite funcional. Validar o conjunto consolidado e os critérios completos antes
do aceite normal. Exceções ao fluxo de PR ficam atribuídas ao decisor humano.

## Condições verificáveis

Aplicar policies/PASSAGENS.md do preset instalado. Referenciar saídas de execução
reais e a revisão atual; presença de arquivo não comprova aprovação. Registrar
impeditivos, correções e revalidação. Exceções exigem condição, risco, escopo,
responsável, data, versão, decisão atribuível e ação posterior; nunca converter
NÃO EXECUTADO em PASSOU. Aprovação humana deve apontar a fonte real da decisão.

## Caminho excepcional por impossibilidade de teste

- Registro confirmado pelo DEV: [referência, versão, cenários e decisão]
- Decisão: AGUARDAR / REJEITAR / ACEITAR_COM_RESSALVAS
- Escopo aceito ou bloqueado, riscos e ação posterior:

Aceite normal continua exigindo execução e comprovação. Um registro válido de
impossibilidade não muda os testes para PASSOU e não torna o gate normal READY.
Somente decisão explícita do DEV pode conceder aceite com ressalvas.

## Fonte da declaração do DEV

O registro de resultados e a declaração explícita devem estar em
`validation/[USn]-dev.md`, nas seções de execução humana e decisão de aceite.
Ao transcrever por solicitação do DEV, referenciar a seção, a rodada e a versão
do documento-fonte (commit ou hash), além da versão do produto avaliada.
Conferir autoria declarada, data/fuso, commit testado, cenários e evidências.
Preservar decisão, justificativa, riscos e escopo; não inferir aceite de PASSOU.
Campo vazio ou inconsistência mantém a pendência. Não substituir uma decisão
anterior: acrescentar nova rodada/versionamento. Aceite de incremento não aceita
história completa. Declaração em Markdown não autentica identidade humana.
