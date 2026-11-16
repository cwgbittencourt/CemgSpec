# Pesquisa técnica — Auditoria documental de projetos SpecKit

**Preset**: cemg-speckit 3.0.0 | **Constituição vigente**: 3.0.0 | **Revisão**: 2026-11-16.
**Estado vigente**: Phase 0 pendente em V1-A/V1-B. Plan BLOCKED, não por obrigação de repetir WFP, mas pela prova da nova fronteira documental e design final.

## R17 — Decisão vigente: V1 somente analyze/check_gate

- Decisão humana: pedido explícito do usuário limita a V1 a speckit-analyze e check_gate, ambos na combo, com origem somente leitura e análise de código futura.
- Motivo: acompanhar progresso/consistência documental e estados/hashes sem comparação de implementação ou comandos amplos. Suporte documental independe da linguagem; desenvolvimento permanece Blazor/C#.
- Ferramentas: analyze adaptado e identificado, apenas documentos; gate determinístico empacotado/conferido pelo SpecAudit, sem executar script/hook da origem. Combo em Verificações/ficha; só duas opções e clique Executar.
- Descoberta preservada: Codex/Antigravity obrigatórios para analyze na entrega completa, ambos bloqueados individualmente se proteção não comprovada. Gate/leitor não exigem IA ou rede. Estado interno do agente externo permitido; relatório oficial somente por Salvar externo.
- Evidência de inspeção: check_gate.py local CEMG importa subprocess e sua opção --project executa specify preset resolve; NÃO usar essa opção no produto V1. O restante pode ler evidências aninhadas: preflight/catálogo deve limitar TODOS os caminhos, inclusive logs/artifacts. Não executar scripts do projeto nem inferir revisão confiável apenas do registro.
- V1-A pendente: provar catálogo/broker e integração documental real, sem leitura de código, escrita ou ferramentas arbitrárias, com cancelamento/descendentes/estado externo e rede delimitados. Prompt sozinho não comprova isolamento.
- V1-B pendente: empacotamento confiável de script/política/runtime gate, execução offline isolada e preflight documental completo, incluindo rejeição de referências a código/configuração/fora do catálogo e mudança concorrente.
- Alternativas rejeitadas para V1: liberar shell/hooks ou todos os comandos, ler código para obter cobertura, substituir prova por allowlist nominal, executar check_gate.py da solução ou declarar WFP aprovado. Recursos futuros exigem nova revisão.
- Consulta de vulnerabilidades da solução (US7/FR-017) adiada; manutenção segura do próprio SpecAudit permanece obrigatória. Nenhum pacote adicionado por esta alteração documental.
- Efeito histórico: R1–R16 e provas antigas abaixo preservam decisões/versões da época. B1 foi resolvido pela emenda 2.0.0; B2-WFP continua falho no escopo antigo, mas não é dependência obrigatória do escopo V1 reduzido. Dois fracassos 0xC0000142 não viram sucesso; autorizações elevadas consumidas não são reutilizáveis. Nenhuma nova prova executada.
- Próximo passo técnico: resolver V1-A/B antes de design final (data-model/contracts/quickstart) e gate de tasks. A alteração documental não libera implementação/aceite automaticamente.

## Histórico R1–R16 — não define o escopo vigente

Os parágrafos seguintes são registros cronológicos com versões e conclusões originais. Para obrigações atuais, usar constituição 3.0.0, spec.md e plan.md revisados.

## R1 — Ambiente e template

- Decisão: preservar Blazor Interactive Server existente, net10.0, C# 14 estável, Windows x64. SDK encontrado 10.0.404; runtime ASP.NET Core 10.0.15. Não selecionar SDK preview instalado.
- Motivo: solução já usa AddInteractiveServerComponents; corresponde à constituição 1.0.1.
- Alternativas: WebAssembly não fornece acesso local seguro às pastas; serviço remoto está fora do escopo.
- Evidência: SpecAudit.csproj, Program.cs, dotnet --info. Nenhum código alterado ou produto executado.
- Preset: registro enabled=true, versão 3.0.0 e hash de manifesto correspondente. setup-plan.ps1 resolveu plan-template do preset com replace.
- Observação: setup retornou BRANCH=001-auditar-projetos-speckit como contexto lógico da feature; git branch --show-current confirmou main. Não foi criada branch nova.

## R2 — Agentes realmente instalados

### Codex

- Descoberto: C:/Users/cwagn/AppData/Roaming/npm/codex.ps1; codex-cli 0.147.0.
- Ajuda instalada: exec, entrada por stdin, --json, --ephemeral, --sandbox read-only, --ignore-user-config e --ignore-rules.
- Decisão candidata: adaptador de execução não interativa, JSON por pipe e sessão efêmera, sem shell livre nem carregamento de instruções da origem.
- Motivo: opções correspondem ao contrato funcional; nenhuma demonstra proteção completa por si só.
- Alternativas rejeitadas: aprovação automática, sandbox desativado, CWD na origem e flags de somente leitura como única barreira.
- Estado inicial: CANDIDATO; posteriormente executada análise sintética autenticada, sem prova integral de compatibilidade. Ver atualização R8.
- Pendente: provar logs/caches/estado sem conteúdo e operação com executor restrito, autenticação delimitada e rede controlada.

### Antigravity

- Descoberto: C:/Users/cwagn/AppData/Local/agy/bin/agy.exe; agy 1.1.10. App/IDE e CLI são instalações diferentes, não presumir equivalência.
- Ajuda instalada: -p, --output-format json/stream-json, --mode plan, --sandbox, --disable-slash-commands, --log-file, --conversation e --continue.
- Fontes oficiais: https://antigravity.google/docs/cli/headless/ e https://antigravity.google/docs/cli/reference/.
- Decisão: manter integração obrigatória como requisito, mas indisponível até prova; não transformar detecção em compatibilidade.
- Motivo: não foi encontrada opção documentada de não persistência de sessões. Conversas retomáveis não provam violação por si sós, mas exigem investigação.
- Alternativas não autorizadas: dispensar Antigravity da entrega ou permitir sessões temporárias externas antes de Salvar. Ambas alteram decisões do usuário.
- Estado: BLOQUEIO B1. Falta comprovar ausência de conteúdo persistido antes de Salvar, inclusive logs/telemetria/caches e subprocessos. Plan mode não é garantia técnica.

## R3 — Proteção Windows e ciclo de vida

- Decisão candidata: host Blazor loopback; leitor/broker com operações apenas de leitura; worker isolado por sistema operacional; writer separado ativado somente por Salvar.
- Worker não recebe acesso ambiente à origem. Recebe conteúdo em memória ou handles estritamente de leitura; isso não é cópia persistida da solução.
- Paths devem ser validados por handles abertos/caminho efetivo, recusando escapes por reparse points; leitura com compartilhamento que não impeça edição normal e invalidação por hash antes/depois.
- Job Object com kill-on-close/no-breakaway controla descendentes/cancelamento, mas NÃO impõe sozinho proteção de arquivos ou rede.
- AppContainer é candidato à fronteira de segurança; não conceder acesso arbitrário à origem alterando ACLs do usuário. Autenticação e dependências dos CLIs podem ser incompatíveis com esse perfil.
- Alternativas rejeitadas: mudar ACL da origem, atributo de pasta, prompt-only, escrever/restaurar original e copiar solução.
- Estado: BLOQUEIO B2. Falta executar prova com CLIs reais e broker, incluindo criação, escrita, rename, delete, subprocesso e rede fora das finalidades permitidas. Não há mecanismo instalado ou proteção comprovada nesta fase.
- Fontes: https://learn.microsoft.com/en-us/windows/win32/secauthz/appcontainer-isolation e https://learn.microsoft.com/en-us/windows/win32/procthread/job-objects.

## R4 — Leitura de código sem execução

- Decisão candidata: árvores sintáticas Roslyn em memória (CSharpSyntaxTree.ParseText); extração estrutural de Razor sem gerar/compilar o projeto. Resolução de símbolos incompleta deve ser declarada, não inferida como ausência.
- NÃO usar MSBuildWorkspace/OpenSolution, executar targets, analyzers ou source generators da origem. Avaliar projeto pode executar lógica MSBuild e viola o escopo de somente leitura sem execução.
- Alternativas: análise por palavras-chave não sustenta rastreabilidade suficiente; build/restore da origem são proibidos.
- Fonte: https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp.csharpsyntaxtree.parsetext.
- Não adotar pacote Razor 6 antigo só pelo nome: parser e versão compatíveis precisam ser selecionados e auditados antes de implementação. O contrato deve preservar limitações, sem execução do código analisado.

## R5 — Interface, arquivos e persistência

- Decisão candidata: picker nativo de pastas em broker STA, acionado pelo usuário local; navegador trabalha com IDs opacos de projeto, nunca acesso arbitrário por parâmetros de caminho.
- InputFile no navegador é upload, não substitui seleção segura de diretório local com identidade do caminho para análise.
- Estado da análise em memória; stdout do agente em pipe, sem arquivos de relatório, sessões ou logs de conteúdo antes de Salvar. Sem banco persistente para resultados não salvos.
- Writer gera pacote externo novo com manifesto, Markdown/JSON/CSV, valida caminho efetivo, gravação sem sobrescrever histórico e falha explícita em destino indisponível.
- Retenção somente dos registros salvos; alteração de destino não move registros. Decisões e consultas antigas só são reutilizadas se salvas.
- Fonte do picker: https://learn.microsoft.com/en-us/windows/win32/shell/common-file-dialog.

## R6 — Markdown, diagramas e segurança

- Decisão candidata: Markdig para parsing; sanitizador allowlist separado; estilos locais e Mermaid vendorizado localmente em modo estrito. HTML gerado pelo parser não é automaticamente seguro.
- Sanitizar links e SVG/diagramas; CSP e isolamento do preview; bloquear rede automática e conteúdo ativo; recursos remotos explícitos com bloqueio de acesso indevido a serviços locais/privados e sem credenciais herdadas.
- Alternativas: CDN automática, extensões do editor e HTML arbitrário não atendem privacidade/segurança.
- Fontes: https://github.com/xoofx/markdig e https://code.visualstudio.com/docs/languages/markdown.
- Versões e pacotes só serão fixados após auditoria; não foram adicionadas dependências nesta fase.

## R7 — Inventário e vulnerabilidades

- Decisão: ler declarações/locks e grafos já existentes; nunca restore/build do projeto analisado. Dependências condicionais/não resolvidas indicam cobertura incompleta.
- Consulta de vulnerabilidades do próprio SpecAudit executada com dotnet package list --project SpecAudit/SpecAudit.csproj --vulnerable --include-transitive --no-restore --format json.
- Resultado: comando retornou sem avisos de pacotes; csproj não possui PackageReference. Isso NÃO comprova ausência de vulnerabilidades do runtime, SDK ou futuras bibliotecas.
- Runtime/SDK e componentes candidatos ainda exigem conferência de advisories na implementação/entrega. Sem declaração de ambiente integralmente seguro.
- Fonte: https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-package-list.

## Bloqueios e condição de retomada

B1 atualizado: Antigravity persistiu conteúdo nas configurações testadas; Codex ainda exige comprovação ampliada de não persistência.
B2: proteção real de origem, rede e descendentes precisa de prova no executor escolhido sem alterar ACLs do usuário ou copiar solução.

Próximo passo: prova controlada com dados sintéticos, autenticação dos agentes, instrumentação de arquivos/processos/rede e nenhum acesso a projeto real. Preservar requisitos. Se falhar, apresentar evidência e solicitar decisão humana sobre alternativa; não reduzir requisitos por conta própria.

Phase 1 (modelo, contratos finais e quickstart completo) NÃO foi iniciada: a habilidade exige research completo antes desse design. A parte independente do plano é um rascunho revisável, não prontidão para tasks.
## R8 — Prova sintética real e resultado atualizado

Usuário autorizou execução com dados sintéticos e atualização do plano. Evidências em verification/agent-probe-results.md e JSONs associados. Rodada inicial foi calibração (captura Codex incompleta e workspace Antigravity não encontrado); rodada corrigida provou leitura dos dois arquivos e diagnóstico correto em ambos. Origem sintética preservada e sem novos arquivos nas três rodadas de análise.

Antigravity gravou marcador exclusivo em transcrições/bancos antes de Salvar, inclusive com plan ativo: configuração testada incompatível com não persistência. Codex --ephemeral concluiu leitura e diagnóstico, sem marcador nas raízes efetivamente lidas, com cobertura parcial. Uma leitura composta recusada pela política foi substituída por leituras simples permitidas; sem bypass.

CreateRestrictedToken + baixa integridade + CreateProcessAsUser passou na leitura e negação de escrita de canary sintético. Nenhuma ACL foi alterada, nenhuma elevação foi solicitada. Ainda não foram comprovados agentes no token, isolamento completo/rede, IPC para processos existentes ou cancelamento de descendentes. Preservar B2.

Versão Antigravity: inventário anterior registrou 1.1.10, mas nova verificação retornou 1.5.9. O binário foi modificado em 2026-11-16T02:44:41Z; ensaios iniciais não capturaram versão/hash por rodada, então NÃO atribuir versão exata a eles. Novas rodadas registram versão/hash. Não foi executado comando update por esta prova; causa da atualização não foi comprovada.

## R9 — research e permissões Antigravity

Documentação e execução confirmaram subagente research via invoke_subagent, não via --agent research nesta instalação. Políticas temporárias deny reforçam leitura, mas transcrições pai/filho persistem conteúdo antes de Salvar. Ver verification/agent-probe-results.md. workspace branch é worktree e não foi executado por conflito com escopo. Revisão de artefatos não substitui negação técnica de escrita. B1 permanece. CLI atual 1.5.9 com versão/hash capturados nas duas novas rodadas.


## R10 — Reavaliação após emenda autorizada

Decisão do usuário: permitir conversas/caches dos agentes fora da solução e gravar relatórios SpecAudit somente ao clicar Salvar. B1 encerrado; registros da prova permanecem factuais e não são apagados. Redesenhar compatibilidade para validar locais reais externos em vez de exigir efemeridade absoluta. Nenhuma permissão de escrita na origem, execução de solução ou acesso adicional a serviços foi concedida. Não importar sessões como relatórios salvos. B2 mantém plan BLOCKED.

## R11 — Provas adicionais e acesso WFP (2026-11-16)

Ver verification/isolation-network-status.md e dados brutos. Leitura por handle readonly e negação de escrita em AppContainer passaram; token/job protegeu processo e filho; cancelamento dos CLIs em startup terminou dois processos por execução. Rede continuou acessível nos canários low/AppContainer. Sessão WFP dinâmica abre, mas FwpmSubLayerAdd0 retorna 5 no contexto não elevado. Nenhum filtro adicionado. Os comandos de canário nos sandboxes nativos foram recusados antes da execução, não comprovando uma barreira de rede. B2 permanece. Nenhum serviço privilegiado ou mudança global foi instalado. Arquitetura de mediador WFP estreito + proxy exige decisão e prova; não foi presumida permissão.


## R12 — Arquitetura de mediador aprovada (2026-11-16)

Usuário aprovou o mediador privilegiado mínimo de filtros temporários por execução, com executor sem privilégios e origem somente leitura; não autorizou instalação/elevação. Decisão registrada em verification/network-mediator-decision.md; responsabilidades, IPC, fail-closed, destinos e matriz de provas detalhados em plan.md. Escolha arquitetural resolvida, mas acesso WFP e integração protegida ainda não comprovados. Não mudar BLOCKED para READY apenas pela aprovação.


## R13 — Protótipo sem privilégios preparado (2026-11-16)

Separado da aplicação em prototype/NetworkMediator. Sessão dinâmica WFP com filtros por SID gerado internamente; sem regras persistentes ou targets arbitrários. 15 autotestes de gates/ciclo de vida/layout e build passaram; nenhum filtro/serviço/UAC nesta preparação. Autorização operacional ainda necessária conforme verification/mediator-authorization-request.md. Mantidos limites de hard-crash e ausência de integração real dos agentes. Manifesto identifica os binários que serão revisados.


## R14 — Execução elevada única e diagnóstico (2026-11-16)

Instalação WFP por SID e rollback passaram com a autorização específica. Worker AppContainer sem Administradores habilitado falhou na inicialização com 0xC0000142, sem asserções. Conexões e operações planejadas permanecem NÃO EXECUTADAS. Causa exata não determinada. Limpeza de filtros/subcamada confirmada por consulta posterior não elevada. Ver verification/mediator-elevated-summary.md. Autorização consumida, sem retry e sem alterar binários/manifesto.


## R15 — Bootstrap do worker, revisão 2

A autorização anterior permanece consumida. Correções e mitigação documentadas em verification/worker-0xc0000142-investigation.md: escape dos argumentos Windows corrigido, PowerShell substituído apenas no worker sintético por Win32 mínimo sem CLR/USER32 diretos. Aplicativo e mediador continuam C#. Filho inicialmente recebeu ERROR_ACCESS_DENIED ao carregar o binário; staging só do executável em TEMP, hash conferido e ReadAndExecute por SID exclusivo resolveram esse bootstrap. Não se copia solução nem se modifica sua ACL. Nenhuma instalação/elevação/WFP aplicada nesta revisão.

Build sem avisos/erros, 23 autotestes passaram. Worker e filho iniciaram sob AppContainer sem Administradores habilitado, exit=0, job zerado e perfil removido. Não abriram rede no diagnóstico. A causa exata do 0xC0000142 anterior permanece inconclusiva; sucesso sem elevação não comprova correção sob WFP elevado.

A revisão 1 e seu manifesto foram arquivados. Novo manifesto vigente em verification/mediator-prototype-manifest.json e autorização concreta em verification/mediator-authorization-request-v2.md, vinculados a quatro binários, inclusive SyntheticWorker.exe. Nenhuma segunda prova elevada executada. Próximo passo depende de autorização explícita de uma execução, sem retry automático; depois verificar tráfego permitido/negado, leitura por broker, recusa de direitos de mutação, controle externo e limpeza. RenameDenied nesta versão mede recusa de DELETE necessário à renomeação, não uma chamada real de rename. Não generalizar esse teste para todas as operações de arquivos.

Plan permanece BLOCKED: B2-prova-elevada-rev2, rede/integridade da fixture na nova revisão, agentes/autenticação/proxy/IPC final e hard-crash fail-closed. Fases finais de design/contratos/tasks seguem bloqueadas. Consulta NuGet sem achados e sem PackageReference; não cobre Windows/CRT/SDK/runtime ou auditoria completa nativa. Código do aplicativo, constituição e especificação não foram alterados.

Evidências: verification/mediator-v2-bootstrap.json, mediator-v2-self-tests.json, mediator-v2-build.txt, mediator-v2-package-audit.txt e worker-0xc0000142-investigation.md. Autorrevisão técnica; não concede aceite humano.


## R16 — Revisão 2 elevada: mitigação não confirmada

Manifesto 3f0839d1f20f77ae77f872e0337753fd7adcf55622b7659f417300520f7b4cc7 conferido; uma execução elevada explicitamente autorizada, sem retry, exit=1. Código/binários/manifesto não recompilados nem alterados. Sem instalação, perfis globais, solução real, agentes ou credenciais. Relatório completo: verification/mediator-v2-elevated-summary.md.

Rollback WFP antes do worker, política por SID exclusivo e controle externo passaram. Worker criado em AppContainer sem Administradores habilitado, mas voltou a falhar com 0xC0000142 antes de WorkerReady (bootstrapReached=false, saída vazia). Filho, leitura/negações e TCP/UDP/IPv6 NÃO EXECUTADOS; contagens zero não demonstram restrição efetiva de tráfego. Remover PowerShell/CLR do worker não resolveu o problema nesse contexto; não há identificação da DLL causadora.

Fonte sintética intacta, job zerado, perfil removido. Consulta independente sem elevação confirmou três filtros e subcamada ausentes, PID encerrado, manifesto inalterado e 20 arquivos monitorados do aplicativo/solução/constituição/spec com hashes preservados. Evidências JSON específicas da revisão 2 mantêm intactos os resultados anteriores.

Autorização v2 consumida. Não repetir nem instalar/elevar para diagnosticar sem autorização distinta. Próximo incremento: investigar contexto de inicialização sem relaxar isolamento e preparar nova revisão somente se necessária. Plan permanece BLOCKED por B2-worker e provas funcionais de rede/fixture, integração real dos agentes/autenticação/proxy/IPC e hard-crash fail-closed. Sucesso de bootstrap sem elevação não fecha esses bloqueios. Nenhum aceite humano/Phase 1 final/tasks liberado.

