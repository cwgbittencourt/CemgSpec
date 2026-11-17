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
Registro humano: responsável, data/fuso, revisão, resultados, capturas e decisão PENDENTES. Aceite da história permanece em seu fluxo próprio.
