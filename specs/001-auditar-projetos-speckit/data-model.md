# Modelo de dados — SpecAudit V1 documental

**Revisão**: 2026-11-16 | **Preset**: cemg-speckit 3.0.0 | **Constituição**: 3.0.0.
Contrato de planejamento; não há banco, migration ou código gerado nesta fase.

## Identidade, armazenamento e isolamento

IDs opacos UUID em memória, contextualizados por ProjectId/FeatureId; identificadores de requisitos da origem nunca são globalmente únicos. Textos são dados não confiáveis. Datas UTC ISO 8601, apresentação no fuso local; hashes SHA256 dos bytes efetivamente lidos. Caminho apresentado não substitui identidade de arquivo/diretório resolvida por handle Windows.

Estado de sessão, conteúdo de documentos, índices e resultados oficiais permanecem em memória. Somente Save cria pacote em diretório externo escolhido. Preferência de tema não contém conteúdo; na V1 fica na sessão, escuro inicialmente. Não há banco ou autosave de análises. Descoberta/versão de ferramentas não é relatório oficial; logs técnicos não contêm documentos, prompts, segredos ou saída de análise. Estado interno de agentes permanece externo e é informado, sem ser importado como histórico salvo.

## Entidades e relações

| Entidade | Campos principais | Relações e invariantes |
|---|---|---|
| ProjectSession | ProjectId, DisplayName, OriginHandleIdentity, OriginDisplayPath, OutputDirectoryIdentity, CreatedUtc, ActiveFeatureId, SelectedVerification, SelectedAgentId, Generation | Origin/destino não podem coincidir nem se conter de forma que destino grave na origem; validar junções/links; contexto nunca é reaproveitado entre projetos |
| Feature | FeatureId, ProjectId, SpecKitRelativeDirectory, Title, DocumentIds, CapabilitySummary | Caminho relativo só dentro do catálogo autorizado; artefato ausente é estado explícito |
| CatalogEntry | DocumentId, ProjectId, FeatureId opcional, RelativePath, Kind, FileIdentity, ContentHash, Length, LastObservedUtc, CatalogGeneration | Kind: Constitution, Spec, Plan, Tasks, Checklist, Research, Design, GateRecord, EvidenceDocument, PresentationAsset; origem reconhecida SpecKit, não só extensão |
| DocumentSnapshot | DocumentId, SnapshotId, ContentHash, Bytes em memória, Encoding, SourceGeneration, ReadUtc, References | Obtido por handle somente leitura; não copia árvore da solução; não escreve arquivo temporário de conteúdo |
| Requirement | RequirementKey=(ProjectId,FeatureId,LocalId), SourceDocumentId, Description, StoryRefs, CriteriaIds | IDs duplicados na mesma funcionalidade geram ocorrência/ambiguidade; não mesclar |
| Criterion | CriterionKey, RequirementKey, Text, DocumentLocation, TaskLinks, CoverageState | CoverageState: Linked, MissingLink, Ambiguous; nunca Implemented/NotImplemented |
| TaskDeclaration | TaskKey contextual, Text, MarkedComplete, SourceLocation, ExplicitRequirementRefs, Phase | Checkbox é declaração documental; não comprova implementação |
| EvidenceLink | LinkId, SourceLocation, TargetDocumentId opcional, TargetText, Relation, Provenance, Resolution | Provenance Explicit/Inferred; Resolution Authorized, Missing, Ambiguous, OutOfScope; referência a .cs/.razor/config não é aberta |
| GateCandidate | CandidateId, FeatureId, Stage, RecordDocumentId, ExpectedRevision, RevisionSource, EvidenceDocumentIds | Stage fechado: spec, plan, tasks, pr, acceptance, dev-impossibility; revisão confirmada explicitamente, não aceita só por repetir registro |
| AgentInstallation | AgentId, Provider, ExecutableIdentity, Version, DiscoveryMethod, ExternalStateLocations, CompatibilityState | Provider Codex/Antigravity/ClaudeCode/Kiro; manual apenas se não encontrado; encontrado não significa habilitado |
| ProtectionAssessment | AssessmentId, ComponentVersion, PlatformFingerprint, CapabilityDigest, Checks, EvidenceRefs, State | State NotVerified, Passed, Failed, Stale; só Passed da versão/configuração aplicável permite habilitar integração |
| VerificationRun | RunId, ProjectId, FeatureId, FocusRequirementKey opcional, Type, CandidateId/AgentId, InputSnapshotIds, State, Freshness, StartedUtc, EndedUtc, ExitCode, Output, Findings, Limitations | Type somente AnalyzeDocuments/CheckGate; estado de execução separado do resultado semântico e atualidade |
| Finding | FindingId, RunId opcional, Category, Severity, Explanation, SourceLocations, Suggestion, ContextFingerprint | Gravidade justificada; conclusão de IA não vira aceite humano |
| HumanDecision | DecisionId, FindingId, Classification, Reason, Actor, TimestampUtc, ContextFingerprint, NeedsReview | Pending/Confirmed/FalsePositive/AcceptedRisk/Deferred; FalsePositive/AcceptedRisk exigem justificativa e responsável; decisão técnica original preservada |
| DashboardProjection | ProjectId, FeatureId, CompletedTaskCount, TaskCount, ExplicitlyLinkedRequirementCount, RequirementCount, RecordedGateStates, Freshness | Percentual=100*n/d se d>0; d=0 é NoItems; sem percentual de implementação |
| SavedRecord | SavedRecordId, ProjectId, Scope, CreatedUtc, SchemaVersion, SourceHashes, ToolVersions, ContentManifest, SelectedFormats, DestinationIdentity | Criado somente em Save bem-sucedido; novo diretório/arquivo por registro; nunca sobrescrever existente |

## Disponibilidade e máquina de estados

CapabilityAvailability é separada de VerificationRun.State: Available, PendingValidation, Unsupported, Unavailable. Sem disponibilidade, PrepareVerification retorna motivo sem criar Run. Se disponibilidade/pré-requisito mudar depois de criar o Run, Preparing termina Blocked; não adicionar Unavailable ao estado de execução. Alteração durante Running exige cancelamento/Failed com motivo, sem resultado semântico inventado.


VerificationRun.State: Queued → Preparing → Running → Completed, Failed ou Cancelled; Preparing também pode terminar Blocked. Preparing pode terminar Blocked se faltarem artefatos, ferramenta, autenticação, proteção ou escopo válido. Failed significa erro da ferramenta/execução, não requisito reprovado. CancelRequested é sinal separado até confirmar término de processos; só então Cancelled. Se término não for confirmado, Failed com execução ainda não encerrada e novas execuções da integração indisponíveis.

VerificationRun.SemanticResult: analyze Findings/NoFindings/Inconclusive; gate READY/BLOCKED/DEV_IMPOSSIBILITY_RECORDED. Falha de preflight não executa o script e apresenta Blocked + motivo de escopo, sem inventar resultado nativo. Registrar stdout/exit da ferramenta separadamente. Só Completed permite declarar verificação terminada; nenhum desses estados aceita história.

Freshness: Current → Stale quando qualquer documento/registro/evidência pertinente mudar ou sua identidade não puder ser confirmada. Nova execução cria RunId, não reescreve a anterior. Mudança de geração durante preparação/execução invalida; não restaurar arquivos. O usuário pode salvar resultado Stale com aviso/metadados explícitos; não o apresentar como atual.

Save: Unsaved → Saving → Saved ou SaveFailed. Enquanto Saving, não reutilizar sessão de outro projeto. SaveFailed não anuncia sucesso e informa eventuais arquivos parciais externos; nada migra para a origem. Fechar sem Save descarta estado oficial em memória; não apaga cache externo do agente.

Agent compatibility: Discovered → NotVerified → Compatible ou Incompatible; mudança de executável/versão/política torna Stale/NotVerified. ClaudeCode/Kiro podem permanecer Unsupported na V1; Codex/Antigravity só podem ser habilitados após provas reais por versão. Projeto pode ser consultado mesmo sem agente compatível.

## Regras adicionais

- Proibir entidades SourceCodeAnalysis, Compilation, ImplementationScore ou DependencyInventory na V1.
- Referências relativas e assets só passam pelo broker; não abrir caminho arbitrário recebido da UI/agente.
- Decisão salva reutilizada apenas se ContextFingerprint corresponder; mudança relevante exige revisão, sem apagar decisão anterior.
- Histórico só contém SavedRecord, não transcrições de agente, execuções descartadas ou resultados de outra origem com mesmo nome.
- Limites operacionais pertencem à configuração técnica interna versionada, não a meta inventada de produto. Ao atingir limite, resultado explícito Inconclusive/Blocked, nunca truncamento silencioso ou READY parcial.
