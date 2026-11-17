# Autorrevisão do incremento PR-A parcial

Data 2026-11-16; modalidade AUTORREVISÃO PELA IA, sem revisão independente ou aprovação humana.
Base Git 197548db2e3efa20c23e1f66b6a0ffb86c0f328d + implementation-code-manifest.json.

## Escopo

T001–T005 e implementação parcial T006–T010: runners, fixtures, sessão local, modelos, token/AppContainer/Job, canal, registro de capacidades, limites, catálogo/broker. UI ainda scaffold; não existe picker nativo/projeto carregável, agente, gate empacotado ou writer habilitado. Nenhuma aprovação de PR, história ou deploy.

## Achados

| ID | Prioridade | Local | Evidência e efeito | Estado |
|---|---|---|---|---|
| SEC-001 | CRÍTICA / impeditiva | RestrictedNative.cs / token AppContainer; ReaderBoundaryTests.cs / cenário ACL ampla | Handle GENERIC_READ concedido para canário de código em origem com grant ALL APPLICATION PACKAGES; exit 81. Ver ReaderBoundary-proof.md. | ABERTO; sem selo ReaderBoundary |
| DEV-001 | Corrigido | BuildSyntheticWorker / build nativo | pwsh não estava disponível no PATH do MSBuild; target usa PowerShell já fornecido pelo Windows e descobre MSVC/SDK existentes, sem instalar. | Revalidado pelo build/test final |
| DEV-002 | Corrigido | Ambiente mínimo do launcher | Sem LOCALAPPDATA, CreateProcessAsUser falhou com 203. Incluída somente variável de localização de perfil, mantendo ambiente mínimo sem segredos herdados. | Provas de bootstrap/cancelamento passaram |
| DEV-003 | Corrigido | SessionContractTests / construção de dicionário | Erro CS9174 durante a primeira rodada; não é evidência red de comportamento. Corrigido; quatro testes de modelos passaram. | Corrigido, saídas preservadas |

Verificações: broker não recebe path da UI/agente; IDs contextualizados; handles readonly e validação final no acesso; dados em memória; nenhum conteúdo analisado/logado. Casos normais de fixtures e TOCTOU passaram. NativeWorker histórico foi usado apenas como referência de código não elevado; mediador/WFP, provas antigas e autorizações consumidas não foram executados/reutilizados. O resultado histórico não substitui as provas atuais.

Canal efêmero usa handles explicitamente herdados e RunId/token/geração, hash do binário é conferido antes/depois do staging; Job confirma encerramento antes de término. ACL alterada apenas no staging próprio e na preparação da fixture de permissão ampla. Nenhuma ACL de solução real alterada.

Limitações: garantia geral de isolamento FALHOU; não registrar Passed nem integrar. T008 ainda não inclui ProjectSelectionTests/UI do picker; T009 backend exercitado, sem fechamento da história. Roteiros manuais antecipados são ESCRITOS e não conferidos no produto; suas tasks de fechamento permanecem pendentes. Nenhum navegador foi lançado, agente autenticado ou fluxo funcional de história demonstrado.

Conclusão: incremento parcial BLOQUEADO por SEC-001. Não existe PR publicado. Hipótese de remediação exige revisão do mecanismo mantendo constituição e contratos; não diluir o teste para obter verde.
