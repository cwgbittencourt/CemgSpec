# Roteiro de incremento — US1 / T012

Versão: base 197548db2e3efa20c23e1f66b6a0ffb86c0f328d + hashes de evidence/implementation-code-manifest.json. Windows x64, SDK .NET 10.0.404; Edge instalado para a regressão de navegador. Constituição 4.0.0. Estimativa: 10–15 minutos.
Estado: ESCRITO; CONFERIDO_PELA_IA no harness de biblioteca/serviço com filesystem Windows real; CONFERENCIA_PARCIAL na tela (picker e edição visual não operados pela IA). EXECUTADO_PELO_DEV: PENDENTE.
Escopo: INCREMENTO de US1-AC04; não substitui o roteiro integrado US1-dev.md nem aceite humano.

## Reprodução conferida pela IA

1. Abrir PowerShell na raiz D:/Codex_Workspace/Projetos/SpecAudit. Conferir a versão/manifesto e `dotnet --version`.
2. Executar `dotnet restore SpecAudit.slnx --locked-mode` se as dependências locais ainda não estiverem restauradas. A execução da IA reutilizou o restore existente.
3. Executar `dotnet test tests/SpecAudit.Ui.Tests --no-restore --filter FullyQualifiedName~ProjectSelectionTests --logger "console;verbosity=normal"`. Validar aprovação dos cenários abaixo. Cada teste cria sua própria origem sintética TEMP/SpecAudit-V1-GUID e destino externo; o picker humano é substituído somente pela entrada controlada de pastas.
4. Executar `dotnet test tests/SpecAudit.Windows.Tests --no-restore --filter FullyQualifiedName~ExternalEditDuringRead --logger "console;verbosity=normal"`. Validar Changed, edição externa preservada e snapshot anterior imutável.
5. Executar `dotnet test SpecAudit.slnx --no-restore`. Checkpoint da IA: 41 aprovados, zero falhos/ignorados. Host local e Edge headless são iniciados e encerrados pelo teste de navegador.

| Cenário | Ação realizada pelo harness / observação no debugger opcional | O que validar |
|---|---|---|
| Código fora do catálogo | Editar notes.md e source.cs após abrir projeto | Nenhuma invalidação documental por conteúdo fora do catálogo |
| Versão capturada | Capturar versão; editar spec.md; validar; atualizar; validar versão antiga e uma nova | Token antigo cancelado e Changed antes/depois do refresh; nova versão aceita; edição preservada |
| Durante leitura | Parar em BeforeRead; escrita externa real; continuar broker | Sem snapshot misturado; Changed; snapshot anterior intacto |
| Renomeação | spec.md vira notes.md; mover diretório evidence/round.1 para fora | Pendência detectada; arquivo/pasta não restaurado pela aplicação |
| Novos diretórios | Abrir raiz vazia e criar specs/003-new/spec.md | Mudança observada mesmo sem specs na abertura |
| Atualização | Adicionar funcionalidade e pedir refresh | Novidade aparece; IDs anteriores preservados. Changed durante lote concorrente exige aguardar e repetir refresh, sem aceitar snapshot parcial |
| Troca de projeto | Abrir A, capturar trabalho; abrir B; voltar A | Trabalho anterior cancelado, contextos separados e destinos sem gravação |
| Encerramento | Descartar serviço com versão capturada | Cancelamento do trabalho e nenhuma gravação automática |

## Conferência manual na interface — pendente de execução humana

1. Criar em uma pasta TEMP própria `origem/specs/001-demo/spec.md`, `plan.md`, `tasks.md` com texto sintético e uma pasta irmã `reports` vazia. Registrar os conteúdos/hashes antes. Não usar projeto real.
2. Iniciar `dotnet run --project SpecAudit/SpecAudit.csproj --no-launch-profile -- --urls http://127.0.0.1:5187`. Abrir http://127.0.0.1:5187 localmente, com o mesmo usuário; sem credenciais de agente.
3. Clicar **Carregar projeto**, selecionar origem e depois reports. Validar os três artefatos como Presente.
4. Em editor externo, acrescentar uma linha em spec.md e salvar. Validar aviso de alteração externa e botão **Atualizar documentos**; a linha deve permanecer no arquivo.
5. Clicar **Atualizar documentos**. Validar remoção do aviso após leitura estável. Se o lote ainda estiver em andamento, aguardar e repetir; nenhum relatório deve ser salvo.
6. Renomear spec.md para notes.md externamente. Validar aviso; atualizar e conferir spec.md Ausente. Confirmar que notes.md permanece com a edição. Em seguida, editar somente notes.md: não deve aparecer novo aviso documental.
7. Criar specs/002-outro/spec.md e atualizar após o aviso. Validar nova funcionalidade e preservação da seleção existente quando ainda presente.
8. Carregar outra origem sintética, alternar pelo seletor **Projeto** e conferir dados separados. Validar reports vazio e botões **Executar verificação**/**Salvar resultados** indisponíveis nesta etapa.
9. Comparar hashes/lista da origem: somente suas alterações deliberadas devem existir. Encerrar com Ctrl+C. Preservar logs/capturas externamente; remover somente a pasta sintética própria após revisão, se desejado. Fixtures do harness permanecem em TEMP para inspeção; cada repetição usa novo GUID.

Prepare/Execute/Save reais não foram implementados nesta task: não há como validá-los na tela ainda. A versão genérica demonstra a rejeição de entradas antigas, sem declarar os fluxos futuros executados.

Evidência: ../evidence/T012-review.md, ../evidence/T012-product-execution.json e ../evidence/logs/T012-suite-complete.txt.
## Registro humano e decisão do incremento

### Registro da execução humana

Preenchimento reservado ao DEV. Os campos abaixo permanecem em branco: a existência do roteiro e os testes da IA não representam execução humana.

Após avaliar cada cenário, registrar o resultado como **PASSOU**, **FALHOU** ou **NÃO EXECUTADO**, descrevendo o observado ou o motivo da não execução. Informar o hash do commit efetivamente testado; se houver alterações locais, identificar também o diff/manifesto. Registrar data/hora com fuso e links para capturas ou logs em `../evidence/`.

| Cenário / critério | Resultado | Resultado observado / motivo da não execução | Evidências (links) | Commit testado / diff ou manifesto | Responsável | Data/hora e fuso | Defeito / observações |
|---|---|---|---|---|---|---|---|
| T012-C01 — Código fora do catálogo | | | | | | | |
| T012-C02 — Versão capturada | | | | | | | |
| T012-C03 — Durante leitura | | | | | | | |
| T012-C04 — Renomeação | | | | | | | |
| T012-C05 — Novos diretórios | | | | | | | |
| T012-C06 — Atualização | | | | | | | |
| T012-C07 — Troca de projeto | | | | | | | |
| T012-C08 — Encerramento | | | | | | | |

### Retestes e novas rodadas

Preservar os registros anteriores. Em caso de correção ou mudança de versão, acrescentar uma linha por cenário reexecutado, identificando a rodada e a nova versão.

| Rodada | Cenário / critério | Resultado | Resultado observado / motivo da não execução | Evidências (links) | Commit testado / diff ou manifesto | Responsável | Data/hora e fuso | Defeito / observações |
|---|---|---|---|---|---|---|---|---|
| | | | | | | | | |

### Decisão sobre o incremento

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

Quando solicitado pelo DEV, a IA deverá conferir esta declaração contra a versão, os resultados e as evidências referenciadas e transcrever a decisão para o registro de incremento vinculado a `acceptance/US1.md`, sem conceder aceite à história completa, preservando autoria, data, escopo e referência a esta seção. Havendo inconsistência ou informação essencial ausente, deverá apontar a pendência antes de registrar o aceite como concluído. A transcrição não altera os resultados dos testes nem equivale a aprovação de PR ou autorização de publicação.

Não há monitoramento automático deste arquivo. A IA não concede aceite por conta própria nem marca EXECUTADO_PELO_DEV sem referência à execução humana real. Se o DEV confirmar impossibilidade, usar o template CEMG próprio e decidir AGUARDAR, REJEITAR ou ACEITAR_COM_RESSALVAS, mantendo os resultados reais; eventual aceite com ressalvas deve ser declarado explicitamente acima.

A decisão desta tabela cobre somente T012. Aceite final da US1 exige execução do roteiro integrado `US1-dev.md` e cumprimento das dependências da história.
