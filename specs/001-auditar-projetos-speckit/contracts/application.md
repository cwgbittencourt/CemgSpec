# Contratos da aplicação e UI — V1

**Revisão**: 2026-11-16 | cemg-speckit 3.0.0 | constituição 3.0.0.
Contratos internos tipados de serviços Blazor Interactive Server; não introduzem REST público ou acesso remoto. Modelo: [data-model.md](../data-model.md).

## Sessão local e entrada

Host somente loopback. Sessão local emitida pelo host, validação Host/Origin e antiforgery nas ações mutantes; nenhum endpoint de caminho livre, nenhum token em URL. Picker nativo de pastas executa no host local por solicitação explícita; navegador recebe IDs opacos e caminhos somente de apresentação. F5/reconexão não recupera análise descartada de logs/caches. Não registrar conteúdo de circuito em logs de erros.

| Operação | Entrada | Saída | Erros/efeitos permitidos |
|---|---|---|---|
| OpenProject | Seleção nativa de origem + destino externo | ProjectSession, funcionalidades, catálogo reconhecido | InvalidOrigin, DestinationInsideOrigin, Unreadable; nenhuma escrita na origem ou relatório criado |
| ChangeOutputDirectory | ProjectId e nova seleção nativa | Destino validado | Sem mover/apagar histórico anterior |
| ListFeatures/ListDocuments | IDs da sessão | Metadados do catálogo | Missing/Unsupported explícitos; não varrer código |
| ReadDocument | ProjectId, DocumentId, modo Preview/Source/Split | Snapshot/preview sanitizado | OutOfScope, Changed, InvalidEncoding; sem parâmetro path |
| ResolveReference | DocumentId, referência e geração | Destino catalogado/âncora ou estado de limite | Nunca seguir código/config ou junção que escape |
| GetDashboard/GetRequirement | IDs + filtros fechados | Projeção documental/fontes | Escopo vazio não gera percentuais; ficha não é arquivo SpecKit |
| SelectVerification | ProjectId, enum AnalyzeDocuments/CheckGate | Seleção da sessão | Enum desconhecido rejeitado também no servidor; não executa |
| PrepareVerification | IDs, tipo selecionado, foco opcional, candidato gate ou agente | Resumo de inputs/alcance/limites e capacidade de executar | Pré-requisito ausente/proteção não validada bloqueia; sem criar arquivos |
| ExecuteVerification | PreparationId + geração confirmada | RunId e fluxo de estado/saída | Somente clique Executar; preparação vencida precisa refazer; validação repetida no servidor |
| CancelVerification | RunId pertencente à sessão | Estado após término confirmado | Não encerrar PID de outra execução; novas execuções indisponíveis se término inconclusivo |
| ClassifyFinding | FindingId, classificação, justificativa/ator | Decisão em memória | Responsável/motivo obrigatórios para falso positivo/risco; não altera resultado técnico |
| SaveResults | IDs de resultados/decisões, formatos e confirmação de conteúdo/destino | SavedRecord | Única operação que grava relatório; contrato persistence.md |
| List/Compare/DeleteSaved | IDs de registros no destino externo, seleção explícita para excluir | Histórico/diferenças/resultado de exclusão | Nunca excluir em background ou usar transcrição como histórico |

## Combo e navegação

Em Verificações e ficha do requisito, mesmos rótulos e valores:
1. `AnalyzeDocuments`: **speckit-analyze — Consistência documental**.
2. `CheckGate`: **check_gate — Validar gates**.

A seleção é única por ProjectSession, sincronizada entre telas; nova sessão inicia em AnalyzeDocuments (padrão de apresentação reversível, sem execução). As duas opções permanecem visíveis mesmo indisponíveis, com motivo e botão Executar desabilitado. Não há terceira opção, entrada livre, importação de comando, console ou encadeamento. Se gate estiver selecionado, agente/autenticação não são solicitados. Foco no requisito não restringe artificialmente análise/gate: resumo informa alcance da funcionalidade/etapa e quais resultados podem ser associados ao requisito.

Cinco áreas: Dashboard, Requisitos, Documentos, Verificações, Sugestões. Projeto/origem somente leitura/destino sempre visíveis. Azul/lilás/branco/cinza, escuro inicial e claro opcional; texto/ícone acompanha cor. Tema não perde seleção/conteúdo. Preview/fonte/lado a lado, sem checkboxes editáveis; Markdown/Mermaid sanitizados com bibliotecas locais. HTML ativo bloqueado; imagem remota exige ação nesta sessão e link externo clique/outra aba, sem credenciais herdadas. Asset local permitido é recurso documental, não porta para ler fonte/configuração.

## Descoberta e compatibilidade

Executáveis conhecidos por localização instalada/registro/PATH: somente identidade/versão por adaptador confiável, sem executar arquivo fornecido pelo projeto. Descoberta não executa análise. Fallback de seleção manual apenas para agente desejado não encontrado; identidade conferida antes de chamar. Codex/Antigravity precisam cumprir [verification.md](verification.md); Claude Code/Kiro podem aparecer Unsupported. Ausência/incompatibilidade não impede documentos/dashboard/gate.

## Observabilidade

Estados e erros tipados, texto claro com escopo/fonte/versão. Saída de agente é texto não confiável; nenhum link/ferramenta é executado a partir dela. Atualização de documentos altera geração e marca resultado Stale; monitoramento não dispara analyze/gate. Cancelamento, troca de projeto e desconexão não autorizam salvamento. Endpoint/action sempre confirma propriedade do ID na sessão, mesmo se UI validou antes.

## Referência visual registrada

A aparência das cinco áreas deve seguir [design/mockups/README.md](../design/mockups/README.md) e suas três imagens aprovadas. Preservar hierarquia/paleta/sidebar/cards/preview sem reproduzir comportamentos antigos das imagens. Somente os dois valores da combo e demais contratos V1 deste documento são executáveis. Nenhum mockup comprova proteção/teste/aceite ou autoriza persistência antes de Salvar.
