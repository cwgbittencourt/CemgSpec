# Code review — US1 / T014

Estado do review: BLOQUEADA para aprovação normal — ALTERAÇÕES SOLICITADAS.
Execução da task T014: CONCLUÍDA; dois achados abertos para T015. Executar uma revisão e aprovar a implementação são estados distintos.
Modalidade: AUTORREVISAO, Codex; nenhuma revisão independente ou humana atribuída.
Data: 2026-11-17, America/Sao_Paulo (UTC−03:00); horários exatos das execuções nos TRX.
Versão revisada: `89242b285c15e24fac3fb89cfc6ec93dd076e18a`, sem alterações de produção ou de testes permanentes nesta task.
PR previsto: PR-A, base planejada `79d9c3d08d75afe2fef7e233493b8648e61ed4e0`; base imediatamente anterior à implementação `197548db2e3efa20c23e1f66b6a0ffb86c0f328d`.
Revisão de código: `sha256:2d8c07395c5c6d039b94d4578224317ec103414a2885e4597e0e08429a574228` (`maintenance-code-manifest.json`).

O diff de código é reproduzível pelos objetos Git locais das bases e do candidato. Comando, SHA256 do diff e fonte dos probes em [T014-review-context.json](T014-review-context.json); não se duplica todo o código nas evidências. Revisados host, sessões, domínio/capacidades, catálogo/broker/handles, monitor, seleção/destino, página Projects e testes correspondentes. Execução de agentes/gate e Save continuam fora deste incremento e indisponíveis na UI.

## Pré-condições e conformidade

Checklist requirements.md: 19/19, zero pendentes; nenhuma alteração nos marcadores. Prerequisites executado com RequireTasks/IncludeTasks. T013 concluída tecnicamente, evidências correntes conferidas antes de iniciar: 455 referências de hash válidas. Constituição vigente 4.1.0, contratos e emenda 4.0 de confiança aplicados; cabeçalhos históricos não reintroduzem sandbox OS. Origem dos templates/scripts locais conferida; template CEMG de review usado neste registro. `.specify/extensions.yml` ausente na entrada e na saída, sem obrigação adicional identificada. `.gitignore` adequado à tecnologia atual. Nenhuma nova dependência, instalação ou alteração de política.

## Análise dos critérios

| Critério | Código relevante | Evidência consultada/executada | Conclusão e limites |
|---|---|---|---|
| US1-AC01 | ProjectService.cs:48–66; DocumentCatalog.cs:31–80; Projects.razor:8–39 | T013 S01 nativo, ProjectSelectionTests e suíte baseline T014; probes de limite | Caminho normal reconhece duas funcionalidades e ausências sem escrita. Exceções esperadas de limite escapam da tela: REV-US1-02. |
| US1-AC02 | WindowsReadHandle.cs:19–25; DocumentBroker.cs:14–37; ProjectService.cs:104–110 | ReaderBoundaryTests (filesystem/handles/junção); T013 S02 nativo | APIs correntes usam acesso readonly, validam identidade/caminho no acesso, recusam destino igual/descendente/junção e IDs alheios. Nenhuma escrita indevida observada. Não comprova sandbox OS ou integrações futuras. |
| US1-AC03 | ProjectSession.cs:20–56; ProjectService.cs:66–76; Projects.razor:18–23 | T013 S03 nativo, separação de contexto e arquivo histórico sintético | Contextos/seleção e destinos independentes; hash/local do histórico preservados; sem mover/excluir. Save futuro continua indisponível; arquivo histórico foi preparado pelo harness. |
| US1-AC04 | DocumentChangeMonitor.cs:21–40; ProjectService.cs:24–28,78–103 | Edição durante leitura e snapshot imutável; T013 S04; probe de metadados T014 | Edição real é preservada e versão antiga rejeitada. Falso Changed/cancelamento com documentos intactos: REV-US1-01. A UI também não trata limites no refresh (inspeção estática em Projects.razor:52). |

## Achados confirmados

### REV-US1-01 — P2 — Metadados da pasta invalidam documentos intactos

Local principal: `SpecAudit/Infrastructure/Documents/DocumentChangeMonitor.cs:30`; entrada de eventos em linha 16, efeito em `SpecAudit/Application/ProjectService.cs:24`.

`Relevant` aceita qualquer evento relativo à pasta `specs/NNN-feature` porque `parts.Length==2` retorna true, sem distinguir alteração de estrutura documental e LastWrite de diretório. O probe alterou somente o timestamp da pasta de fixture: conjunto de caminhos e hashes de todos os arquivos ficaram iguais, mas HasPendingChanges tornou-se true e a versão capturada recebeu cancelamento. O observador independente registrou `specs/001-demo`; nenhum relatório foi criado. Resultado esperado das asserções: documentos intactos continuam atuais; observado: assert False falhou.

Efeito: trabalho documental pode ser cancelado/inutilizado por mudança irrelevante de metadados; é um mecanismo concreto de falso positivo compatível com a observação intermitente OBS-MAINT-01. A rodada original não tinha instrumentação de eventos, portanto não se afirma que sua causa exata foi demonstrada. Prioridade P2 por disponibilidade/atualidade incorreta, sem perda ou escrita de documentos observada. Impeditivo para fechamento normal por falha de comportamento e de asserção de regressão exigida.

Evidências: [directory-metadata.json](logs/T014-review/directory-metadata.json), [probes.txt](logs/T014-review/probes.txt), TRX em `logs/T014-review/probe-results/`.

Correção proposta para T015: distinguir eventos de diretório/estrutura e revalidar alterações documentais reais antes de invalidar por metadados, mantendo descoberta de novas funcionalidades, remoção/rename de diretórios e falhas do watcher conservadoras. Não simplesmente ignorar todos os diretórios nem desativar o monitor. Adicionar caso determinístico, repetir o teste de arquivo fora do catálogo e os casos de edição/rename/refresh/cancelamento.
Situação: ABERTO. Correção/revalidação: NÃO EXECUTADAS nesta task de review.

### REV-US1-02 — P2 — Limites escapam do tratamento de erros da tela

Local principal: `SpecAudit/Components/Pages/Projects.razor:48`; mesmo filtro incompleto em `Refresh`, linha 52. Tipo de exceção: `SpecAudit/Infrastructure/Execution/ExecutionLimits.cs:2`.

`Open` trata DocumentAccessException/IO/InvalidOperation/Unauthorized/Win32, mas LimitExceededException deriva diretamente de Exception e não entra no filtro. Foram executados dois casos: spec sintético com 4 MiB + 1 byte e catálogo com 257 evidências adicionais. O handler real compilado de Projects foi invocado pelo harness: ambos propagaram LimitExceededException, deixaram `error` null e não abriram projeto. Nenhum arquivo da origem ou relatório foi modificado pelo produto. O método deveria retornar erro/limitação visível sem propagar uma exceção operacional esperada.

Efeito: ao atingir os limites técnicos atuais, o fluxo de carregamento perde sua recuperação prevista; em Blazor a exceção escapa do callback da UI. O probe comprova o escape e a ausência de mensagem, não uma captura do encerramento de circuito em navegador. Prioridade P2: caso negativo esperado e recuperável deve ser tratado; não há evidência de corrupção. Impeditivo para fechamento normal por violar a condição de limite explícito/inconclusivo de plan/modelo/T007.

Evidências: [page-limit-document-bytes.json](logs/T014-review/page-limit-document-bytes.json), [page-limit-evidence-graph.json](logs/T014-review/page-limit-evidence-graph.json), [probes.txt](logs/T014-review/probes.txt) e TRX.

Correção proposta para T015: tratar LimitExceededException nos handlers de carregamento e refresh, mostrar dimensão/limite sem conteúdo sensível, preservar projeto anterior e estado seguro. Testar a mensagem/recuperação nos dois limites e no refresh. Não aumentar arbitrariamente os limites nem ocultar a exceção como sucesso.
Situação: ABERTO. Correção/revalidação: NÃO EXECUTADAS nesta task de review.

## Segurança e estado intermediário

Inspecionados os acessos do broker, validações por identidade/handle, recusa de hardlinks/reparse e geração/hash antes/depois da leitura; cobertura existente não autoriza ampliar confiança para agentes. Destino é resolvido por handle antes de sessão/troca; writer futuro deverá revalidá-lo no ponto de gravação. Sessão emitida pelo host, IDs contextuais e testes reais de Host/Origin/antiforgery/cookies conferidos. Seleção não inicia comando; verificação/Save continuam desabilitados. Documentos são dados; nenhuma execução de scripts/hooks/configuração da fixture foi realizada. Nenhum segredo ou conteúdo de projeto real usado.

O runner compartilhado e o registro de capacidades foram inspecionados como dependências, sem alegar validação de adaptadores ainda inexistentes ou comprovação adicional de descendentes de agentes. O escopo técnico é o carregamento/leitura/invalidação da US1. Não foi identificado outro achado confirmado neste escopo; isso não garante ausência de defeitos nem substitui T018/revisão humana.

## Execuções e reprodução

Diretório: `__REPO_ROOT__`. Ambiente da baseline: Windows/.NET 10/Edge já instalados, mesmo código do commit revisado. Nenhuma instalação ou dependência nova.

| Rodada | Comando/procedimento | Esperado | Observado | Resultado |
|---|---|---|---|---|
| Baseline | `dotnet test SpecAudit.slnx --no-restore --logger 'trx;LogFilePrefix=T014-baseline' --results-directory specs/001-auditar-projetos-speckit/evidence/logs/T014-review/baseline-results` | Suíte existente sem regressões | 45/45, zero falhos/ignorados; host/Edge e picker nativo reais | PASSOU |
| Probes | Comando abaixo, com fonte temporariamente adicionada ao projeto de testes | Documentos intactos atuais e limites tratados | Três falhas: metadados, limite de bytes e limite de grafo; exit 1 | FALHOU |
| Pacotes | `dotnet list SpecAudit.slnx package --vulnerable --include-transitive --no-restore --format json` | Consulta disponível | Exit 0, nenhum pacote vulnerável listado | PASSOU no alcance da consulta |

Logs/saídas em `logs/T014-review/`; consulta NuGet não cobre toda a segurança do SDK, Windows ou Edge. As novas falhas não invalidam a ocorrência histórica dos 45 testes aprovados; demonstram lacunas de cobertura que impedem aprovar a revisão corrente. Não repetir apenas a baseline para esconder as falhas dos probes.

Fonte exata preservada em `T014-probes/T014ReviewProbe.cs`, SHA256 em `T014-review-context.json`. Não é carregada automaticamente pela aplicação ou suíte normal. Para reprodução de desenvolvimento, conferir a fonte e copiar exclusivamente esse arquivo para o caminho temporário indicado; não executar instruções de documentos de uma solução auditada.

```powershell
Set-Location (git rev-parse --show-toplevel)
$probeTarget = Join-Path (Get-Location) 'tests/SpecAudit.Ui.Tests/T014ReviewProbe.cs'
if (Test-Path -LiteralPath $probeTarget) { throw 'Destino já existe; não sobrescrever.' }
Copy-Item -LiteralPath 'specs/001-auditar-projetos-speckit/evidence/T014-probes/T014ReviewProbe.cs' -Destination $probeTarget
try {
    dotnet test tests/SpecAudit.Ui.Tests/SpecAudit.Ui.Tests.csproj --no-restore --filter FullyQualifiedName~T014ReviewProbe --logger 'trx;LogFilePrefix=T014-probes' --results-directory specs/001-auditar-projetos-speckit/evidence/logs/T014-review/probe-results
} finally {
    Remove-Item -LiteralPath $probeTarget
}
```

O caminho temporário fixo é arquivo criado por esta receita dentro do checkout; remoção não é recursiva e não toca a origem sintética. Preservar as fixtures externas para inspeção. Para a versão revisada, esperado da reprodução é exit 1/três falhas; o comportamento correto continua sendo o das asserções e deve passar após T015. No término da execução da IA, a fonte temporária foi movida para a evidência, sem alteração permanente em `SpecAudit/` ou `tests/`.

## Conclusão e passagem

Dois achados P2 confirmados e abertos. A passagem de autorrevisão T014 está executada e registrada; o review para aprovação permanece BLOQUEADO. T015 é a task autorizável de correção/revalidação; não iniciar fechamento normal T016/PR-A com esses achados abertos. Nenhuma alteração de produção foi feita para corrigi-los nesta solicitação de T014. Execução humana, aprovação de PR, integração e aceite continuam pendentes. Não foi presumida confirmação de impossibilidade ou exceção humana.

Gate PR-A executado com `check_gate.py`, stage `pr`, registro `verification/implementation-stage.json` e revisão de código indicada acima: BLOCKED/exit 1 esperado pelos achados, probes falhos e condições de fechamento/decisão humana pendentes. Log final em `logs/T014-review/gate-final.txt`; `gate.txt` preserva a primeira conferência durante a atualização, ainda com um vínculo de hash pendente, posteriormente reparado. A conferência final não deve apresentar erro de hash/revisão; o erro de execução de teste é esperado e corresponde aos probes realmente falhos. Formatos, sanitização e vínculo ao código conferidos em `T014-verification.json`; o inventário corrente e o comando `python tools/maintain_evidence.py --verify` permitem conferir a integridade estrutural, sem transformar os resultados negativos em aprovação.
