# Pedido de autorização — revisão 2, AUTORIZAÇÃO CONSUMIDA

Manifesto SHA256: `3f0839d1f20f77ae77f872e0337753fd7adcf55622b7659f417300520f7b4cc7`.

Substitui o manifesto `d0fd1c46893c69d51dd2aba717834b8aed30a09423ff4b53b9bb5fc458eec115`, cuja autorização de uma execução foi consumida. O novo binário NÃO herda a autorização anterior. Investigação e limites: [worker-0xc0000142-investigation.md](worker-0xc0000142-investigation.md).

## Escopo originalmente submetido e autorizado

Autorizar **uma única execução elevada da prova sintética da revisão 2**, vinculada ao SHA256 acima, com o wrapper e os quatro binários conferidos. Permite somente sessão WFP dinâmica e filtros por SID exclusivo; canários locais TCP/UDP/IPv6; worker nativo e filho sem privilégios; handle de leitura; fixture/relatório TEMP; ACL ReadAndExecute do SID na nova subpasta TEMP que contém apenas a cópia do executável sintético. Não altera ACL da solução ou de desktop/WinSta.

Sem instalação de serviço/driver, firewall global ou filtros persistentes; sem solução real, agentes, credenciais ou rede externa. Encerrar job antes de remover filtros, verificar rollback e limpeza. Preservar evidências técnicas em TEMP e atualizar a tabela/plano. Nenhum retry automático: falha consome essa autorização também.

## Comando histórico — NÃO REEXECUTAR

```powershell
& '__REPO_ROOT__\specs\001-auditar-projetos-speckit\prototype\NetworkMediator\Invoke-MediatorProof.ps1' -ExecuteSyntheticProof -ExpectedManifestSha256 '3f0839d1f20f77ae77f872e0337753fd7adcf55622b7659f417300520f7b4cc7'
```

O comando requer uma execução elevada separadamente autorizada; o wrapper não eleva automaticamente. Sem recompilar ou modificar artefatos entre a revisão e a execução. Qualquer mudança de hashes requer novo manifesto/revisão/autorização. Somente bootstrap foi validado nesta revisão; a correção no contexto elevado e o bloqueio funcional de rede permanecem pendentes. Plan BLOCKED.

## Registro posterior — 2026-11-16

Usuário autorizou explicitamente uma única execução elevada da revisão 2 nesta conversa. A execução foi realizada uma vez, exit=1, e consumiu a autorização. Worker nativo falhou com 0xC0000142 antes dos testes; sem retry. WFP/rollback e controle externo passaram; limpeza confirmada independentemente. Ver [mediator-v2-elevated-summary.md](mediator-v2-elevated-summary.md). O texto acima preserva o escopo da autorização original, não uma permissão de repetição. Qualquer próxima prova elevada exige nova revisão e autorização próprias.
