<!-- Override local CemgSpec: cemg-speckit 3.0.0 + constituição 4.1.0. -->
# Roteiro do DEV — [US1]

Versão exata a validar: [commit + artefato/diff quando aplicável]
Ambiente/endereço: [valor]
Tempo aproximado: [estimativa]

## Preparação

1. Obter a versão: [procedimento].
2. Instalar/iniciar: [comandos, diretório, versões e serviços].
3. Configurar acesso: [perfil e obtenção segura das credenciais].
4. Preparar dados isolados: [comando/procedimento e valores].
5. Confirmar estado inicial: [verificação].

## Execução

| Passo | Critério/cenário | Ação exata e entrada | Resultado esperado | Como confirmar |
|---|---|---|---|---|
| 1 | [IDs] | [ação] | [resultado] | [tela/resposta/consulta] |

Cobrir cenário principal, erros e limites previstos; incluir permissões quando
pertinentes. Para API, CLI ou biblioteca, fornecer requisição, comando ou exemplo
executável em vez de inventar uma interface gráfica.

## Limpeza e repetição

[Restaurar dados e estado sem destruir informações compartilhadas. Instruções
devem permitir nova execução com o mesmo resultado.]

## Evidências da IA e limitações

- Validação: [link relativo]
- Revisão: [link relativo]
- O que foi executado e o que ficou pendente: [resumo]
- Bloqueios conhecidos e solução: [detalhes]
- Roteiro conferido pela IA: [sim/não/parcial e evidência; nunca presumir]

## Registro humano e aceite

### Registro da execução humana

Preenchimento reservado ao DEV. Os campos abaixo permanecem em branco: a existência do roteiro e os testes da IA não representam execução humana.

Após avaliar cada cenário, registrar o resultado como **PASSOU**, **FALHOU** ou **NÃO EXECUTADO**, descrevendo o observado ou o motivo da não execução. Informar o hash do commit efetivamente testado; se houver alterações locais, identificar também o diff/manifesto. Registrar data/hora com fuso e links para capturas ou logs em `../evidence/`.

| Cenário / critério | Resultado | Resultado observado / motivo da não execução | Evidências (links) | Commit testado / diff ou manifesto | Responsável | Data/hora e fuso | Defeito / observações |
|---|---|---|---|---|---|---|---|
| [CENARIO] / [CRITERIO] | | | | | | | |

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

Quando solicitado pelo DEV, a IA deverá conferir esta declaração contra a versão, os resultados e as evidências referenciadas e transcrever a decisão para `acceptance/[USn].md`, preservando autoria, data, escopo e referência a esta seção. Havendo inconsistência ou informação essencial ausente, deverá apontar a pendência antes de registrar o aceite como concluído. A transcrição não altera os resultados dos testes nem equivale a aprovação de PR ou autorização de publicação.

Não há monitoramento automático deste arquivo. A IA não concede aceite por conta própria nem marca EXECUTADO_PELO_DEV sem referência à execução humana real. Se o DEV confirmar impossibilidade, usar o template CEMG próprio e decidir AGUARDAR, REJEITAR ou ACEITAR_COM_RESSALVAS, mantendo os resultados reais; eventual aceite com ressalvas deve ser declarado explicitamente acima.

## Consolidação dos PRs da história

- PRs integrantes e ordem: [IDs/URLs]
- Versão final consolidada: [hash/artefato]
- Roteiro: INCREMENTO / HISTORIA_COMPLETA
- Critérios ainda pendentes: [IDs ou nenhum]

Um roteiro de incremento não substitui o roteiro integrado para aceite. Entregar
instruções completas num ponto de entrada, sem exigir combinação de passos
dispersos. Atualizar a versão e os passos após correções ou integrações relevantes.

## Estado de reprodução

- Roteiro: ESCRITO / CONFERIDO_PELA_IA / CONFERENCIA_PARCIAL / EXECUTADO_PELO_DEV
- Passos conferidos pela IA e evidência:
- Passos pendentes, motivo e impacto:
- Execução humana: responsável, data, versão, resultados e referência real.

Somente aceite normal após execução humana e decisão explícita na versão final.
Conferência parcial deve permanecer visível e exigir tratamento antes de declarar
prontidão normal. Escrita do roteiro não comprova sua reprodutibilidade.

## Execução com ou sem tela

O DEV deve conseguir iniciar e acionar o programa, API, integração, mensagem,
job, CLI ou harness de biblioteca e verificar resultado e efeitos. Incluir comando,
requisição ou entrada completos, dados, acesso, resultado esperado e observação.
Depuração pode complementar: indicar pontos úteis e valores esperados, sem tratar
atingir breakpoint como sucesso.

Se o DEV confirmar que não pode executar, preencher dev-impossibility-template,
preservar os cenários não executados e registrar AGUARDAR, REJEITAR ou
ACEITAR_COM_RESSALVAS. A IA não confirma impossibilidade nem aceite por ele.
