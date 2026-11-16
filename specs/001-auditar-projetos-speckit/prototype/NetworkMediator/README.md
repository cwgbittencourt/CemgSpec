# Protótipo do mediador — revisão 2

Planejamento cemg-speckit 3.0.0; constituição 2.0.0. Harness sintético separado do aplicativo. Não é serviço, IPC de produção nem integração final dos agentes.

## Correções e limites

O worker anterior em PowerShell terminou na única prova elevada com 0xC0000142 (STATUS_DLL_INIT_FAILED). Não foi identificada a DLL causadora. PowerShell inicializou no ensaio sem elevação; portanto a causa exata no contexto elevado permanece inconclusiva.

A revisão 2 corrige o escape dos argumentos Windows, inclusive aspas e barras finais. Substitui exclusivamente o worker sintético por SyntheticWorker.exe Win32 em C, compilado com ferramentas já instaladas e CRT estático; aplicativo e mediador continuam em C#. Importações diretas do worker: KERNEL32, ADVAPI32 e WS2_32; não importa USER32 nem CLR diretamente. Isso reduz dependências, mas não prova causalidade nem correção no contexto elevado.

Somente o executável sintético é copiado para TEMP, com hash conferido. Sua subpasta recebe ReadAndExecute para o SID exclusivo do AppContainer, permitindo que o filho execute o mesmo binário. Não se copia projeto/código analisado nem se alteram suas ACLs. Nenhuma permissão de escrita é concedida ao AppContainer nessa subpasta. A fixture-fonte fica separada, sem essa concessão.

## Preparação sem privilégios

- `Build-Prototype.ps1`: .NET Release e compilador MSVC/SDK existentes, sem instalar ferramentas ou elevar. `/W4 /WX /MT /guard:cf`, ASLR e DEP.
- `dotnet <bin/Release/net10.0/NetworkMediator.dll> --self-test`: 23 checks de gates, layouts, ciclo de vida e round-trip de argumentos.
- `dotnet <DLL> --diagnose-worker`: recusa administrador; cria fixture TEMP e AppContainer, inicia worker + filho apenas para diagnóstico de bootstrap, sem rede nem WFP; encerra Job Object e remove seu perfil. Ambos reportaram READY, AppContainer=true, AdminGroupEnabled=false; exit=0 e ActiveAfterTerminate=0.
- Auditoria NuGet sem achados reportados; não há PackageReference. Não cobre vulnerabilidades do Windows, SDK, compilador, runtime, CRT estático ou análise completa do código nativo.

## Uma nova prova elevada, ainda não executada

O manifesto vigente é verification/mediator-prototype-manifest.json. A autorização anterior está consumida e preservada em verification/archive/mediator-v1. Pedido novo: verification/mediator-authorization-request-v2.md. Executar somente o wrapper com o SHA256 integral do manifesto revisado; ele verifica DLL, deps, runtimeconfig e SyntheticWorker.exe e recusa extensões de runtime conhecidas. Não pede UAC automaticamente.

Escopo fixo: criar TEMP SpecAudit-MediatorProof-<guid>, source.txt sintético, cópia somente do worker e relatório; abrir canários TCP IPv4/IPv6 e UDP locais; testar rollback antes de iniciar worker; instalar três filtros WFP dinâmicos exclusivamente para o SID temporário (permit TCP loopback/porta exata, deny restante IPv4/IPv6); executar worker suspenso depois da política instalada, com token restrito e lista explícita de handles (saída + fonte somente leitura); filho herda identidade/job.

Worker e filho devem ler o handle, negar abertura com GENERIC_WRITE e DELETE e negar CREATE_NEW na fixture. RenameDenied nesta revisão significa recusa do direito DELETE necessário à renomeação; não é uma chamada de rename nem prova de todas as variantes de renomeação. O arquivo original não recebe escrita, exclusão ou restauração. Tentativas de criação são exclusivamente sintéticas e uma criação inesperada reprova o ensaio. Verificar TCP permitido e TCP IPv4/IPv6/UDP negados, identidade de ambos, controle externo ao SID conectado, hash da fonte intacto, job zerado e filtros/subcamada/perfil removidos. Logs incluem WorkerReady, erros Winsock e status de saída em hexadecimal.

Sem serviço/driver/tarefa/certificado instalado, sem perfis globais de firewall, sem regra persistente, sem alterar ACLs de desktop/WinSta, sem solução real, agentes, credenciais ou serviços externos. Apenas ACL da nova subpasta do worker TEMP é modificada. Artefatos técnicos da prova são preservados para inspeção; isto não muda a regra do produto de gravar relatórios oficiais somente ao clicar Salvar.

Qualquer falha/ausência de saída/limpeza inconclusiva reprova ou deixa incompleto. Zero conexões sem worker executado nunca comprova bloqueio. Não repetir sem nova autorização; qualquer rebuild exige novos hashes. Autenticação/proxy/domínios/IPC real, Codex/Antigravity sob política final e hard-crash fail-closed permanecem pendentes. Plan BLOCKED.
