# T010 — ReaderBoundary: FALHOU / BLOQUEADO

Data: 2026-11-16; executor: IA. Windows x64, .NET 10.0.15. Base Git 197548db2e3efa20c23e1f66b6a0ffb86c0f328d + diff identificado em implementation-code-manifest.json.
Nenhuma solução real, instalação, elevação ou mudança de ACL de usuário. A ACL ampla foi configurada somente na criação de uma fixture TEMP exclusiva. Perfis AppContainer por execução foram removidos pelo próprio launcher; resultado HRESULT 0. Staging do executável sintético permanece para inspeção; não houve cópia de solução.

## Procedimento executado

Na raiz de desenvolvimento:
`dotnet test SpecAudit.slnx --no-restore --logger trx --results-directory specs/001-auditar-projetos-speckit/evidence/logs/implementation-checkpoint`

Resultado real: exit 1, 27 testes totais, 26 aprovados e 1 falho. [Saída completa](logs/implementation-checkpoint.txt). Os TRX estão no diretório implementation-checkpoint.

- Catálogo por IDs, leitura por handle somente leitura, junção trocada antes de ler, arquivo substituído, geração divergente e projeto alheio: testes aprovados.
- Fixture com ACL normal: worker pai e filho não conseguiram abrir código/config nem criar/escrever/renomear/excluir; leitura autorizada pelo broker funcionou; edição normal pelo usuário permaneceu permitida. [Resultado](logs/reader-process-proof.json).
- Fixture com ALL APPLICATION PACKAGES (S-1-15-2-1) autorizado a ler: teste BoundaryMustAlsoDenyCodeWhenOriginHasBroadAppPackageReadPermission FALHOU. [Resultado](logs/reader-broad-acl-proof.json).

## Achado impeditivo SEC-001

Esperado: o executor/filho jamais obtém acesso direto de leitura a código/config da origem, independentemente de concessões amplas preexistentes.
Observado: o worker em AppContainer, sem Administradores habilitado, recebeu um handle GENERIC_READ para DoNotRead.cs e retornou exit 81. O harness fecha o handle imediatamente e não lê o conteúdo; a concessão do acesso já viola a garantia exigida. Não atribuir sucesso à ausência de escrita ou aos hashes.

A prova padrão passou, mas não cobre essa configuração de permissões. O launcher atual depende de ausência de concessões amplas no filesystem. Não basta um SID exclusivo sem grant explícito quando há grants preexistentes aplicáveis ao AppContainer.

T010 permanece FALHOU. ReaderBoundary não recebeu selo Passed. T006/T007 e as partes implementadas de T008/T009 não concedem prontidão da fundação nem habilitam projeto real. T011 e demais tarefas dependentes ficam BLOQUEADAS. A IA não aceitou exceção nem alterou a constituição.

## Próximo passo proposto para discussão

Revisar o desenho do token/executor e a prova de acesso efetivo, preservando o contrato de nenhum acesso direto à origem, sem modificar ACL da solução, copiar solução ou depender de elevação/instalação. A revisão deve explicitar como tratar concessões amplas e demonstrar o carregamento das dependências confiáveis sob a nova restrição. Não remover esse teste nem redefinir a origem como suportada apenas quando o teste padrão passa. Nova necessidade de privilégio, instalação ou redução de escopo exige decisão própria antes da ação.
