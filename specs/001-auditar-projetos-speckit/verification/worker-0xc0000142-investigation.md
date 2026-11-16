# Investigação do worker 0xC0000142 — revisão 2, 2026-11-16

## Diagnóstico observado

A única execução elevada autorizada da revisão 1 retornou STATUS_DLL_INIT_FAILED antes de qualquer saída do worker PowerShell. Não há evidência que identifique a DLL exata. Eventos Application consultados no intervalo não explicaram a falha. Não foi repetida execução elevada.

Comparação sem elevação (worker-bootstrap-comparison.json): PowerShell mínimo inicializou no AppContainer. Portanto a falha não é universal nesse executável. A documentação de CreateProcessAsUser descreve contexto não interativo e requisitos de estação/desktop; issue microsoft/mxc#1465 relata falhas similares relacionadas a USER32. São hipóteses de contexto, não identificação da causa neste computador. Não foram modificadas ACLs de WinSta/desktop.

Fontes primárias: https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-createprocessasuserw ; https://learn.microsoft.com/en-us/windows/win32/winstation/process-connection-to-a-window-station ; https://github.com/microsoft/mxc/issues/1465 .

## Correções aplicadas

1. Launcher: escape Windows correto para aspas, argumentos vazios, espaços e barras finais. Seis casos round-trip, além dos gates/layouts: 23 autotestes passaram.
2. Removido PowerShell/CLR do worker sintético. Executável Win32 mínimo, CRT estático, sem USER32 nas importações diretas. A aplicação/mediador permanecem em C#.
3. Diagnóstico inicial do filho revelou ERROR_ACCESS_DENIED ao executar o binário no checkout. Corrigido por staging só do worker em TEMP com hash conferido e concessão ReadAndExecute exclusivamente ao SID temporário. Não se alterou ACL do checkout, fixture-fonte ou solução analisada. Não é cópia da solução.
4. Filho recebe apenas saída e handle de leitura explicitamente herdados; token/job preservados. Eventos WorkerReady/ChildLaunchFailed e códigos Winsock/status hexadecimal tornam a próxima falha observável.
5. Wrapper exige também hash do worker. Revisão 1 arquivada; revisão 2 requer autorização própria.

## Resultados sem elevação

Build passou com zero avisos/erros. 23 autotestes passaram. Worker e filho retornaram READY, AppContainer=true e AdminGroupEnabled=false; exit=0, ActiveAfterTerminate=0, ProfileDeleteResult=0. Ver mediator-v2-bootstrap.json e mediator-v2-build.txt. Diagnóstico de bootstrap não abre rede nem WFP e recusa administrador. Nenhum agente/projeto real foi executado.

Consulta NuGet sem achados; sem dependências PackageReference. Não equivale a atestado de ausência de vulnerabilidades no código nativo, Windows, runtime, compilador ou CRT incorporado.

## O que continua pendente

A mitigação e correções passaram sem elevação. Ainda NÃO está comprovado que o 0xC0000142 desapareceu sob o contexto elevado da prova com WFP. TCP permitido/negado, UDP, IPv6 e leitura/negações da fixture nesta nova revisão aguardam a nova prova. Integração de agentes, autenticação, proxy e falha abrupta do mediador continuam bloqueando produção. Nenhuma instalação, elevação ou mutação WFP foi realizada nesta investigação.
