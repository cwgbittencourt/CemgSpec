<!-- Situação vigente: constitution 2.0.0; B1 encerrado por decisão humana. Conclusões históricas de reprovação por persistência externa foram superadas pela emenda; efeitos observados e limites dos ensaios permanecem válidos. Ver persistence-amendment.md. -->
# Prova técnica de agentes e proteção Windows

**Data local**: 2026-11-15 (America/Sao_Paulo). Timestamps brutos de execução estão em UTC.
**Base**: 460af9e3e38d4cb200517c45424c7569b377a322; constitution 1.0.1; preset cemg-speckit 3.0.0.
**Autorização**: usuário solicitou prova com dados sintéticos e atualização do plano.
**Modalidade**: execução real de CLIs autenticados e harness nativo Windows. Não é execução do produto final, revisão independente ou aceite de história.

## Dados e isolamento do ensaio

Fixtures foram criadas em diretórios novos SpecAudit-Probe-* sob TEMP, fora do repositório e de qualquer solução real. spec.md exige limites inicial e final inclusivos; Filter.cs aplica apenas o inicial. Marcadores exclusivos são gerados pelo harness para rastrear propagação do conteúdo. Nenhum segredo ou código real foi enviado deliberadamente.

Handles de leitura sem compartilhamento de escrita/exclusão preservaram os dois arquivos sintéticos durante cada execução. Esta proteção de ensaio não é o design do produto: bloquearia a edição desses arquivos por qualquer processo e não demonstra proteção de um diretório inteiro. Nenhuma ACL da origem foi alterada. As três rodadas mantiveram hashes originais e não criaram novos arquivos sob a origem sintética.

Resultados brutos são evidências de desenvolvimento autorizadas explicitamente, não um salvamento automático de resultados pelo produto. Sessões internas do agente, por outro lado, são efeito observado e relevante para constitution II. As fixtures e sessões foram preservadas para inspeção; nenhum diretório de usuário foi limpo automaticamente.

## Rodadas e resultados

### Rodada 1 — calibração (não prova a análise)

Arquivo: agent-probe-43fd3483975f4b4db80f619cadf21eab.json.

Codex terminou, mas a captura foi consultada antes de o stream concluir; stdout vazio não pode ser usado como prova. Antigravity respondeu que o diretório ativo era scratch e não encontrou a fixture. Exit code 0, isoladamente, não indica análise aprovada. Corrigidos espera de streams e caminhos absolutos; esta rodada foi preservada como falha de calibração.

### Rodada 2 — análise real e primeira observação de persistência

Arquivo: agent-probe-75b75212a4fd424789337dd01ec76c3c.json.

Codex 0.147.0 executado com exec --ephemeral --json --sandbox read-only --ignore-user-config --ignore-rules --skip-git-repo-check, recebendo solicitação por stdin. Leu os dois arquivos com Get-Content e apontou corretamente a ausência do limite final, citando Filter.cs. Uma leitura inicial composta foi rejeitada pela política; duas leituras simples permitidas concluíram o trabalho, sem contornar a proteção.

Não foi localizado marcador em seus arquivos de estado monitorados na rodada. Isso não prova ausência absoluta: um arquivo grande foi excluído da leitura, snapshots não capturam criação seguida de exclusão e alterações do Codex desktop concorrente podem aparecer. Há warnings de cache de modelos e shell snapshot, sem impedir a resposta. Nenhuma sessão efêmera foi declarada persistida apenas por metadados modificados.

Antigravity (versão por execução não capturada nessa rodada) leu os arquivos pelos caminhos absolutos e identificou corretamente o mesmo defeito. O marcador foi encontrado em transcrições e bancos de conversa externos à fixture antes de qualquer Salvar. A CLI informou que --disable-slash-commands neutralizava --mode plan; por isso essa configuração NÃO comprova modo plan e foi descartada como candidata.

### Rodada 3 — Antigravity com plan efetivo

Arquivo: agent-probe-53e3d38501c04ff2b3389f553c5ada7b.json.

Nova fixture e marcador; executado com --add-dir, --mode plan, --sandbox, --output-format stream-json e print não interativo, sem --disable-slash-commands. O produto final ainda precisaria impedir instruções/configurações da origem por outro mecanismo: remover a flag no ensaio sintético não resolve esse requisito.

A resposta voltou a detectar o limite final ausente; exit 0; arquivos sintéticos intactos. Mesmo com plan efetivo, o marcador foi encontrado em nove arquivos de estado do agente, incluindo transcript.jsonl, transcript_full.jsonl, chunks e banco de conversa. Não foi necessário ler credenciais ou conversas alheias: a busca limitou-se ao marcador sintético nos arquivos alterados das raízes monitoradas e excluiu nomes de autenticação.

**Conclusão de B1**: a configuração instalada/testada do Antigravity é incompatível com a regra de nenhum conteúdo persistido antes de Salvar. Não significa que toda integração futura seja impossível; exige modo sem persistência comprovado ou decisão humana explícita de emenda. Codex permanece candidato, com análise real comprovada e não persistência apenas parcialmente observada.

## Ensaio de token Windows

Arquivo: windows-token-probe.json. Harness: probes/probe-windows-token.ps1.

CreateRestrictedToken com privilégios desabilitados e integridade baixa; CreateProcessAsUser iniciou PowerShell sem perfil, sem solicitar elevação. O filho leu conteúdo sintético e recebeu UnauthorizedAccessException ao tentar escrever em canary externo de integridade normal. Exit 0 do harness significa que as duas asserções passaram; hashes da fonte e canary permaneceram iguais. Sem alteração de ACL.

Isso demonstra um mecanismo real de negação de escrita para aquele processo e cenário. NÃO demonstra agentes funcionando nesse token, AppContainer, restrição de egress, proteção contra serviços existentes que executem tarefas por IPC, Job Object ou cancelamento de todos os descendentes. B2 continua parcial/pendente.

## Limites de instrumentação e segurança

Snapshots por metadados foram feitos antes/depois, não houve trace completo do sistema. Só raízes explícitas foram observadas; arquivos acima de 64 MiB foram excluídos da busca e erros são contados no JSON. Persistência comprovada é achado positivo; ausência de marcador não é garantia. MarkerReturned sozinho também não comprova leitura, pois a rodada 2/3 incluiu o marcador no pedido: a leitura foi confirmada nos eventos e na resposta sobre código cujo defeito não foi informado no pedido.

Nenhum teste/build/serviço da solução foi executado. Os CLIs, não o projeto analisado, foram executados. Nenhuma dependência foi instalada/adicionada; não houve nova auditoria de pacotes nesta rodada. Dados sintéticos podem permanecer no provedor conforme políticas dos agentes; não foi avaliada retenção remota ou paging/dumps do Windows.

## Decisão e próximo passo

Manter plan BLOCKED. Não alterar constitution/spec nem habilitar Antigravity por conta própria. Para liberar integração, provar um modo sem persistência real ou solicitar ao usuário emenda que permita estado interno do agente fora da origem antes de Salvar, preservando salvamento manual de relatórios. Essa alternativa não está autorizada neste registro.

Independente dessa decisão, B2 ainda exige ensaio dos CLIs reais sob fronteira efetiva de arquivos/rede/descendentes e controle de instruções da origem. Não iniciar tasks dependentes nem declarar histórias aceitas.
## Correção de procedência de versão

O inventário anterior reportou agy 1.1.10; a consulta atual retornou 1.5.9 e hash 8803c289cac04d6e53f2211592d9981c2d5760bc0d9b4a7f222ef6ecc7abb38a. O binário foi modificado em 2026-11-16T02:44:41Z; não foi emitido comando update. Como a identidade não foi capturada por rodada inicial, não atribuir versão exata a essas rodadas. Harness atualizado captura versão/hash antes de novas execuções.

## Opções propostas pelo usuário — conferência adicional

### 1. research nativo

Documentação oficial de Dynamic Workflows confirma type_name research para pesquisa somente leitura; Custom subagents confirma invoke_subagent e herança de workspace. Na instalação atual, agy 1.5.9, --agent research retornou unknown agent (no custom agents are available), sem consumir turno. Isso não significa que o subagente nativo não exista: o seletor de agente principal e a delegação são interfaces diferentes.

A rodada agent-probe-04a77cbd4ff44c649d96df231b31239e.json solicitou delegação pelo agente principal. O evento invoke_subagent confirmou type_name research, workspace da fixture e conversa filha 7645d69d-cb9e-4ae3-a0f1-56713393a231. A transcrição sintética da filha mostrou chamadas read_file e o conteúdo dos dois arquivos. A CLI principal terminou dizendo que aguardava o diagnóstico; exit 0 não equivale ao término da tarefa filha. Esse lifecycle exige tratamento no adaptador.

Mesmo nesse modo, conteúdo sintético foi persistido em transcrições/bancos tanto do pai quanto do filho antes de Salvar. Portanto research é candidato útil para reduzir ferramentas de escrita, mas NÃO resolve por si só constitution II nem substitui fronteira OS para a árvore inteira de execução.

### 2. workspace branch

A documentação descreve um Git worktree isolado (não necessariamente clone completo). Não foi executado, pois cria outra árvore e pode alterar metadados do repositório; contraria o fluxo atual sem cópia isolada e sem gravações na solução. Não usar para desbloquear silenciosamente.

### 3. request-review, strict e artifact review

Opções oficiais existem, com diferenças de versão/conta. A documentação de CLI distingue políticas de ferramentas e revisão de artefatos; revisão de artefatos não impede por si só edições de arquivo. Pedir aprovação não é uma negação absoluta de escrita nem desliga estado interno.

CLI 1.5.9 oferece --settings para overrides só da sessão. Prova de delegação usou request-review e deny explícito de write_file(*), command(*), read_url(*), execute_url(*), download_url(*) e mcp(*), sem alterar settings globais. Os dados da fixture ficaram intactos. Isso não comprova exaustivamente a interceptação de toda ferramenta/efeito ou proteção de processos existentes.

### 4. artifacts externos

Artefatos fora da origem são compatíveis com preservação da solução, mas sua gravação antes de Salvar conflita com a regra atual de persistência manual. O achado não é código alterado na origem: é conteúdo salvo pelo agente em estado interno. Não foi solicitado gerar um artifact adicional, pois transcrições/bancos já demonstraram o efeito relevante e não havia necessidade de mais gravações.

Fontes primárias consultadas:
- https://antigravity.google/docs/dynamic-workflows/
- https://antigravity.google/docs/subagents?tab=cli
- https://antigravity.google/docs/settings
- https://antigravity.google/docs/permissions?tab=cli
- https://antigravity.google/docs/cli/reference/
- https://antigravity.google/docs/artifact-review

Conclusão: research com permissões temporárias restritas é a melhor opção das propostas para análise sem ferramentas de escrita; porém a regra de não persistir conteúdo antes de Salvar continua bloqueando a configuração testada. Nenhuma alteração de política/escopo foi concedida pelo usuário.

Snapshot da filha preservado em research-subagent-snapshot.jsonl: leitura dos dois arquivos observada, sem resposta diagnóstica final capturada. O pai retornou aguardando; não declarar a tarefa filha completa por esse exit 0. Hashes da fixture rechecados após delegação, ambos intactos. Nenhum processo CLI iniciado pelo harness permaneceu em execução ao encerrar cada rodada; atividade interna de subagentes em serviço exige controle específico no produto.

