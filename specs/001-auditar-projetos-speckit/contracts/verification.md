# Contratos de verificações e proteção — V1

**Revisão**: 2026-11-16 | cemg-speckit 3.0.0 | constituição 3.0.0.
Este é o desenho validado documentalmente no planejamento. As provas operacionais abaixo devem ser implementadas/executadas ANTES de habilitar cada caminho. READY do plan não é ProtectionAssessment.Passed e não autoriza instalação/elevação.

## Fronteiras e mecanismo planejado

1. Host Blazor em usuário normal, catálogo e broker confiáveis são donos da seleção da origem; nenhum executor recebe uma raiz irrestrita da solução. Broker aceita IDs de documentos, abre por handle Windows somente leitura (GENERIC_READ, compartilhamento que permita edição pelo usuário), resolve identidade/caminho final e confere contenção no momento de cada acesso. Não solicita write/delete, não muda ACL da origem e não copia árvore da solução. Erro/identidade trocada invalida resultado, sem restore.
2. Processo externo comum não elevado, executável/argumentos definidos pelo adaptador, UseShellExecute=false, cwd externo e entrada documental selecionada. Cancelar/encerrar somente a árvore própria. Não exigir AppContainer, tokens restritos ou Job Object; não há contenção de agente comprometido pelo sistema operacional.
3. Broker fornece bytes/snapshots por canal local autenticado à execução (ID/token efêmero e handles explicitamente herdados), nunca ferramenta geral de filesystem ou shell. Agente não recebe credencial do broker de outro projeto. Mensagens contêm RunId, CatalogGeneration, DocumentId e conteúdo autorizado; resposta sempre relacionada ao snapshot. Assets só para preview; não há leitura de .cs/.razor/manifests/configuração. Buffer em memória não é cópia isolada da solução em disco.
4. Conteúdo documental não autoriza comandos, filesystem geral, instruções/plugins/hooks/configuração da origem ou credenciais da solução. Ferramentas do agente devem ser limitadas pela integração e sua capacidade real declarada. Rede somente para finalidade do provedor, sem promessa de firewall OS. Compatibilidade é demonstrada no fluxo real.
5. Broker só lê; writer só grava no destino externo quando Salvar; UI usa IDs. Falhas de controle de aplicação impedem o fluxo. A integração indisponível não impede leitor/gate independentes.

Constituição 4.0.0 substitui a exigência de isolamento pelo SO. Antes de analyze informar seus limites. Sem instalação, elevação, alteração de ACL da origem, cópia da solução ou experimentos de bootstrap.

## ReadDocumentCapability

Entrada fechada: RunId, DocumentId do catálogo da sessão, SnapshotGeneration, finalidade Document/Presentation/GateEvidence. Não aceita absolutePath, URI, glob, shell ou arquivo pedido em texto pelo agente. Limites internos versionados para tamanho/profundidade/saída; excesso retorna LimitExceeded, não conteúdo parcial tratado como completo.

Retorno: snapshot de bytes em memória, hash, identidade, geração e localização documental ou Missing/Changed/OutOfScope/Unreadable/LimitExceeded. Revalidar handle/caminho final e ligações antes de leitura, confirmar hash/geração ao concluir. Não basta pré-checagem de string ou detectar hash depois de já ler arquivo proibido. Mudanças concorrentes geram Stale; nenhuma escrita/ACL/restore na origem. Provar junção trocada, links de prefixo semelhante e acesso por filho.

## AnalyzeDocuments

Entrada: projeto/funcionalidade, SnapshotIds de constituição/spec/plan/tasks completos, foco opcional de requisito, AgentInstallation com ProtectionAssessment Passed da mesma versão/política. Se pré-requisito ausente, Blocked, sem criar documento. Adaptação somente leitura de speckit-analyze identificada no resultado; não executar hooks do projeto. A análise é da funcionalidade; o foco apenas destaca achados pertinentes.

Agente recebe conteúdo via canal/stdin, sem cwd na origem. Caminhos de executável são os descobertos/conferidos, não comandos do projeto. Configuração do adaptador define ferramentas fechadas, saída estruturada com referências aos DocumentIds/hashes, estado externo e cancelamento. Tratar saída como não confiável: validar schema/tamanho/referências; fonte inexistente/inventada vira limitação, nunca arquivo aberto automaticamente. Texto original pode ser exibido sanitizado junto ao resultado não interpretável.

Saída: RunState, duração, saída/erro separados, Findings e conclusões com fontes, hashes, escopo, agente/versão e limites. Nenhum resultado de implementação. NoFindings não certifica projeto correto; falta de prova/saída válida vira Inconclusive/Failed. Sem escrita de relatório oficial antes de Save.

## CheckGate

Entrada: FeatureId, Stage enum (spec/plan/tasks/pr/acceptance/dev-impossibility), RecordDocumentId, ExpectedRevision confirmada e CatalogGeneration. Revisão derivada pelo usuário ou hash do artefato aplicável; não aceitar valor do próprio registro como confirmação independente. Gate de PR/aceite pode usar revisão externa explicitamente informada, rotulada como informada e não autenticada. Não há acesso Git da origem para inferi-la.

Implementar pacote confiável com core check_gate.py e gate-requirements.json CEMG 3.0.0 pinados por hash e runner do SpecAudit. **Não usar CLI bruta com caminhos reais da origem depois de um simples preflight.** O core abre paths novamente e isso deixaria TOCTOU. A integração executa as funções validate/validate_impossibility do core conferido por um runner confiável com objetos Path de capacidade: resolve/contained/join/parents/read_bytes/read_text são resolvidos no catálogo virtual, e cada leitura passa por ReadDocumentCapability. Somente a política empacotada pode ser lida da área confiável do aplicativo. Nenhuma cópia do core da solução é importada e nenhum documento é copiado para diretório isolado.

O runner não chama main/active, não aceita --project, não despacha subprocessos e não expõe filesystem geral. Comparar resultados do core via runner com a CLI oficial em fixtures documentais controladas para demonstrar equivalência; resultado identifica **check_gate CEMG 3.0.0 / adaptador de leitura SpecAudit**, com hash do core/runner/política. Não alterar critérios de validação para obter READY. Adaptador de I/O é parte da implementação, sem reescrever preset instalado ou executar Python fornecido pelo projeto.

Runtime Python absoluto/conferido, `-I -B`, cwd externo, UseShellExecute=false/ArgumentList e ambiente mínimo (sem PYTHONPATH/site customization/extension hooks do projeto). Script, imports e bibliotecas confiáveis são do pacote/runtime, sem restauração de pacotes no projeto. Estado gate não grava conteúdo e não precisa de agente, autenticação ou rede. O runner não implementa rede ou subprocessos arbitrários; isto não equivale a uma negação por sandbox OS.

Enumerar e validar grafo: registro; evidence; stdout/stderr em test-run; artifacts em product-execution; fontes indexadas de human_decisions e human_confirmation. Detectar referências inválidas/ciclos/limites sem seguir destinos proibidos. Extensão não basta: cada nó precisa ser documento/registro SpecKit reconhecido no catálogo. Código/configuração, path absoluto externo, traversal, junction fora do escopo ou asset não documental como evidência exigida gera Blocked de preflight/escopo; nunca pular hash obrigatório e apresentar READY parcial. Repetir validação no ponto real da leitura pelo broker para eliminar janela TOCTOU. Logs históricos são dados, não comandos.

Saída nativa core: READY/exit 0, BLOCKED/exit 1 ou DEV_IMPOSSIBILITY_RECORDED/exit 0. Falha de runtime/runner/schema vira Failed, sem forjar JSON de sucesso; indisponível é CapabilityAvailability.Unavailable e impede criar Run; se Run já existir, Preparing termina Blocked. Cancelamento confirmado vira Cancelled. Mostrar stdout/erros/exit separados. Resultado é integridade/estado registrados, não teste funcional ou autenticidade humana. Foco da ficha não transforma gate da etapa em gate individual do requisito.

## Contrato de cancelamento e habilitação

Quebra do canal, alteração de geração, usuário cancelando/trocando projeto ou encerrando sessão: sinalizar cancelamento, encerrar processo/descendentes próprios, fechar capacidades e somente então declarar término. Nunca matar PID arbitrário recebido da UI. Timeout técnico/limite interno resulta em erro/limitação observável; não Save automático. Em encerramento inconclusivo, bloquear novas execuções do adaptador e informar motivo.

| Selo interno de habilitação | Provas obrigatórias antes de uso | Falha implica |
|---|---|---|
| ReaderBoundary | Leitura autorizada; .cs/config e junction negados; nenhuma gravação nos fluxos/APIs da aplicação; usuário edita normalmente; origem intacta | Não carregar origem em executor; só fixture de desenvolvimento |
| GateAdapter | Runtime/core/política íntegros; offline sem --project; equivalência positiva/negativa; referências aninhadas; TOCTOU no momento de leitura; cancelamento/zero writes | Gate visível indisponível; leitor só se selo próprio passou |
| AnalyzeCodex | Documentos reais sintéticos via canal, diagnóstico correto, contrato documental/ferramentas configuradas e proteção de segredos, estado externo, descendentes/cancelamento | Só Codex indisponível |
| AnalyzeAntigravity | Mesma matriz, versão/estado de agente próprios; transcrição externa informada | Só Antigravity indisponível |
| ExternalWriter | Save explícito, destino canônico externo, create-new, reparse/TOCTOU, falhas parciais e histórico | Save indisponível; resultados continuam em memória |

Selo vinculado à versão/binário/política/ambiente relevante e registrado como evidência de desenvolvimento, sem conteúdo de análise. Mudança invalida selo afetado. Não há botão para usuário ignorar falha e habilitar inseguro. Nenhum selo marcado Passed por este planejamento. Tasks devem implementar infraestrutura + provas antes da tarefa funcional dependente; isso é dependência de implementação, não requisito de execução antecipada para escrever o plan.


## Precedência da emenda 4.0.0 e selos funcionais

Qualquer referência anterior a negação de syscalls, privilégio de token, firewall de processo, herança de handle de sandbox ou executor incapaz de acessar filesystem está substituída pelo contrato de aplicação acima. As configurações dos adaptadores continuam obrigatórias, mas não são garantia de contenção contra agente comprometido.

ReaderBoundary v4 requer authorized-read, code-config-denied pelo catálogo, junction-denied, application-read-only, user-edit-preserved e origin-intact. AnalyzeCodex/AnalyzeAntigravity requerem real-analysis, document-contract, configured-tools-and-disclosure, external-state e descendants-cancel. GateAdapter e ExternalWriter preservam integridade, equivalência, TOCTOU, cancelamento e persistência explícita. Selo de política v3 não habilita v4. Falha dos novos controles impede habilitação; aceite humano permanece separado.
