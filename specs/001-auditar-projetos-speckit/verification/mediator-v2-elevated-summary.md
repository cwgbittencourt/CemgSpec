# Única prova elevada — revisão 2, 2026-11-16

**Resultado: FAILED_OR_INCOMPLETE. Plan BLOCKED. Autorização consumida, sem retry.**

Manifesto SHA256 `3f0839d1f20f77ae77f872e0337753fd7adcf55622b7659f417300520f7b4cc7` e seus 10 arquivos-fonte/4 binários conferidos antes da execução. Nenhum rebuild ou alteração do protótipo/manifesto. Usuário autorizou explicitamente uma execução elevada; realizada uma chamada RunAs ao wrapper, exit=1. Nenhuma instalação, alteração de perfis globais de firewall, agente/credencial/solução real acessada. Política dinâmica somente para SID sintético.

| Verificação | Resultado | Evidência |
|---|---|---|
| Rollback parcial antes de iniciar worker | PASSOU | rollbackPassed=true; cleanup=true; PID=0 |
| Instalação WFP por SID exclusivo | PASSOU no escopo | Três filtros e subcamada próprios instalados |
| Controle fora da identidade durante política | PASSOU | controlUnaffectedDuringPolicy=true |
| Identidade restrita do worker | PASSOU | TokenIsAppContainer=1; AdminGroupEnabled=false |
| Inicialização do worker nativo | FALHOU | 0xC0000142; BootstrapReached=false; saída vazia |
| Filho e verificações de rede/arquivo | NÃO EXECUTADOS | Nenhuma linha Role ou WorkerReady; zero conexões não comprova bloqueio |
| Preservação da fonte sintética | PASSOU | sourceIntact=true; conteúdo consultado independentemente |
| Encerramento/limpeza | PASSOU | ActiveAfterTerminate=0; ProfileDeleteResult=0; três filtros/subcamada ausentes em consulta independente não elevada |
| Integridade dos arquivos monitorados | PASSOU | 20 hashes de aplicativo/solução/constituição/spec inalterados em relação ao início desta prova |

A revisão 2 falhou com o mesmo STATUS_DLL_INIT_FAILED da revisão 1, apesar de retirar PowerShell/CLR e importação direta de USER32 do worker sintético. Logo, a mitigação de dependências não resolveu o problema observado sob o contexto elevado/WFP. Os sucessos anteriores sem elevação continuam válidos somente naquele contexto. A DLL/componente que falhou permanece não identificado; não atribuir causalidade a PowerShell ou USER32. A falha antecede o main do worker e não comprova execução de subprocesso nem operações de IO/rede.

## Verificação independente e preservação

Consulta normal, sem elevação, usando somente WFP GetByKey: filtros retornaram 0x80320003 (FILTER_NOT_FOUND), subcamada 0x80320007 (SUBLAYER_NOT_FOUND). PID observado 36928 ausente. O relatório registra job zerado/perfil removido; origem sintética intacta. Manifesto permanece com o hash autorizado. Cópia do resultado é idêntica ao original TEMP. Os únicos arquivos da fixture listados são source.txt, worker/SyntheticWorker.exe e proof-result.json.

O manifesto permanece congelado como registro de preparação (privilegedProofExecuted=false na época da preparação). O resultado posterior está neste documento e em mediator-v2-elevated-execution.json/result.json; não modificar o manifesto para reescrever seu estado histórico. Pedido de autorização v2 marcado consumido. A autorização não permite correção/rebuild seguido de nova tentativa elevada.

## Pendências e próximo trabalho

Investigar o contexto de inicialização com observação não privilegiada antes de propor outra revisão; não relaxar proteções da origem, alterar ACLs de desktop/WinSta, liberar rede global ou instalar ferramentas. Se novo teste elevado for necessário, preparar novo manifesto/escopo e obter autorização própria. Bloqueios B2-worker, rede funcional, agentes/proxy/autenticação/IPC e hard-crash fail-closed permanecem. Nenhum aceite humano ou prontidão para tasks concedido.

Evidências: mediator-v2-elevated-execution.json, mediator-v2-elevated-result.json, mediator-v2-cleanup-verification.json e mediator-v2-origin-baseline.json. Código do protótipo/aplicativo, constituição/spec e manifesto não alterados nesta execução; somente registros e documentos de planejamento atualizados. Sem commit/push.
