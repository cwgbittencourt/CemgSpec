# Escopo vigente — V1 documental (2026-11-16)

| Item | Situação na V1 |
|---|---|
| Combo | Apenas speckit-analyze e check_gate, em Verificações e ficha |
| Código, builds/testes/restore e consulta de dependências da solução | Fora da V1 |
| Provas WFP/worker antigas | Histórico de escopo anterior; duas falhas preservadas; não são pré-condição obrigatória da V1 |
| Proteção do adaptador analyze documental | V1-A PENDENTE; agente sem prova fica indisponível |
| Gate empacotado offline + grafo de evidências documental | V1-B PENDENTE; não executar script/hook da origem |
| Origem somente leitura / Salvar externo | Mantidos, sem exceção |
| Prontidão | Spec documental a revalidar; plan BLOCKED até pesquisa/design finais |

Mudança de escopo autorizada explicitamente pelo usuário; não é aprovação do isolamento ou da rede. Nenhuma nova instalação/elevação/prova. Histórico abaixo preservado e não deve ser aplicado automaticamente como exigência da nova V1.

---

# Estado vigente — única prova elevada da revisão 2 (2026-11-16)

| Verificação | Resultado atual | Limite |
|---|---|---|
| Manifesto e binários autorizados | CONFERIDOS | Uma execução, sem rebuild/retry |
| Instalação de filtros WFP por SID e rollback | PASSOU | Somente identidade sintética e sessão dinâmica |
| Controle fora da identidade | PASSOU | Conectado durante a política |
| Token AppContainer sem Administradores habilitado | PASSOU | Worker criado, não inicializado |
| Inicialização do worker nativo elevado | FALHOU | Mesmo 0xC0000142; sem WorkerReady |
| Filho, leitura/negações e rede TCP/UDP/IPv6 | NÃO EXECUTADOS | Zero conexões não comprova bloqueio |
| Fonte sintética e arquivos monitorados | INTACTOS | 20 hashes preservados; nenhuma solução real usada |
| Limpeza de filtros/subcamada/job/perfil | PASSOU | GetByKey independente sem elevação confirmou ausência; PID encerrado |
| Instalação/firewall global | NENHUM | Somente filtros dinâmicos por SID do protótipo |
| Integração final de agentes e hard-crash | PENDENTE | Plan BLOCKED |

**Autorização da revisão 2 consumida. Nenhuma repetição realizada ou autorizada.** A troca do worker não resolveu a falha no contexto elevado; causa exata segue inconclusiva. [Relatório e limites](mediator-v2-elevated-summary.md). Histórico de preparação e provas anteriores abaixo.

---

# Estado vigente — revisão 2 preparada sem elevação (2026-11-16)

| Verificação | Resultado | Limite |
|---|---|---|
| Build da revisão 2 | PASSOU | Zero avisos/erros; ferramentas existentes |
| Autotestes, incluindo escape de argumentos | PASSOU | 23 checks sem WFP/rede |
| Inicialização do worker nativo e filho | PASSOU | Sem elevação; ambos AppContainer, Administradores desabilitado; exit 0 |
| Limpeza de job/perfil do diagnóstico | PASSOU | ActiveAfterTerminate=0; ProfileDeleteResult=0 |
| Causa exata do 0xC0000142 elevado anterior | INCONCLUSIVA | DLL não identificada; novo contexto elevado ainda não testado |
| Nova prova elevada com WFP/rede/fixture | NÃO EXECUTADA | Aguarda autorização do manifesto revisão 2 |
| Instalação/elevação nesta investigação | NENHUMA | Não houve aplicação de filtros |
| Vulnerabilidades NuGet do protótipo | SEM ACHADOS REPORTADOS | Sem PackageReference; não cobre Windows/CRT/SDK/runtime/código nativo |
| Compatibilidade final Codex/Antigravity e hard-crash | PENDENTE | Plan BLOCKED |

Pedido concreto: [mediator-authorization-request-v2.md](mediator-authorization-request-v2.md). Diagnóstico: [worker-0xc0000142-investigation.md](worker-0xc0000142-investigation.md). Autorização antiga consumida; histórico abaixo não comprova a revisão 2.

---

# Atualização vigente — única prova elevada (2026-11-16)

| Item | Resultado atual |
|---|---|
| Filtros WFP por SID e rollback | PASSOU |
| Controle fora da identidade permanece conectado | PASSOU |
| Token restrito, sem grupo Administradores habilitado | PASSOU |
| Inicialização do worker | FALHOU: 0xC0000142 |
| Conexões permitidas/negadas e operações do worker | NÃO EXECUTADO; não inferir sucesso de contagens zero |
| Origem sintética intacta e limpeza dos filtros/subcamada | PASSOU; limpeza consultada novamente sem elevação |
| Rede final e agentes | PENDENTE; plan BLOCKED |

Execução única autorizada realizada e autorização consumida. Nenhum retry. Fonte e limites em [mediator-elevated-summary.md](mediator-elevated-summary.md). Abaixo está o histórico dos ensaios anteriores, com seus escopos originais.

---
# Estado consolidado das provas de isolamento e rede

**Atualizado**: 2026-11-16, America/Sao_Paulo. **Constituição**: 2.0.0.
**Preset**: cemg-speckit 3.0.0. **Estado**: BLOCKED; nenhum aceite funcional concedido.
Passou significa somente que as asserções do ensaio indicado foram observadas, não aprovação integral do produto.

## Tabela de acompanhamento

| Verificação | Codex | Antigravity | Evidência / limite |
|---|---|---|---|
| Diagnóstico correto da divergência sintética, execução normal | PASSOU | PASSOU | agent-probe-75b75212a4fd424789337dd01ec76c3c.json e rodada plan efetivo |
| Preservação da origem sintética nos ensaios | PASSOU | PASSOU | Hashes intactos; sem novos arquivos na origem; nenhum projeto real usado |
| Cancelamento do CLI e processos filhos durante inicialização | PASSOU | PASSOU | agent-cancellation-probe.json: dois processos ativos por job, zero após terminação, sem PIDs observados sobreviventes |
| Análise completa sob token de baixa integridade + Job Object | NÃO PASSOU | PASSOU COM LIMITAÇÕES | Codex falhou ao abrir estado/app-server; Antigravity retornou diagnóstico com erros de logs/cache; não há política de egress nesse mecanismo |
| Inicialização do binário em AppContainer | PASSOU, SOMENTE --version | PASSOU, SOMENTE --version | appcontainer-probe.json; TokenIsAppContainer=1; não comprova análise autenticada |
| Estado externo gravável e autenticação sob isolamento final | PENDENTE | PENDENTE | Não copiar credenciais ou alterar ACLs existentes para contornar a prova |
| Bloqueio de TCP pelo mecanismo low integrity | NÃO PASSOU | NÃO PASSOU | Harness Windows alcançou canário; não é ensaio individual de cada agente sob política final |
| Bloqueio de TCP pelo AppContainer sem capabilities testado | NÃO PASSOU | NÃO PASSOU | Processo sintético em AppContainer alcançou canário; causa ambiental não determinada |
| Teste de rede no sandbox nativo do agente | NÃO EXECUTADO: comando recusado pela política | NÃO EXECUTADO: headless negou permissão do comando | native-network-probe-a427813989e243439109395e934fc6ef.json; zero conexões não prova bloqueio de tráfego |
| Allowlist efetiva permitindo somente provedores e avisos | PENDENTE | PENDENTE | Nenhuma política por processo foi aplicada; não declarar conexões permitidas/negadas comprovadas |
| Integração completa aprovada | NÃO | NÃO | B2 permanece |

## Mecanismos Windows

| Mecanismo | Resultado | Evidência |
|---|---|---|
| Negar create/write/rename/delete e permitir leitura no processo restrito | PASSOU | boundary-probe-88e8434a0dd749ec8085f53c95cdf4cd.json |
| Herança da restrição pelo filho sintético | PASSOU | Mesma rodada: leitura passou, quatro operações negadas |
| Terminar Job Object e confirmar zero processos ativos | PASSOU | Mesma rodada e agent-cancellation-probe.json |
| Ler por handle do broker, sem acesso direto à pasta | PASSOU | read-broker-probe.json, TokenIsAppContainer=1 |
| Negar escrita pelo handle de leitura | PASSOU | read-broker-probe.json; exit 0 e fonte intacta |
| Sessão WFP dinâmica aberta sem elevação | PASSOU | wfp-access-probe.json, retorno 0 |
| Criar subcamada WFP dinâmica vazia | BLOQUEADO POR PERMISSÃO | wfp-sublayer-probe.json, retorno 5: acesso negado |
| Adicionar filtros de tráfego | NÃO EXECUTADO | Nenhum filtro foi adicionado, pois criação da subcamada falhou |
| Firewall global e ACLs da origem inalterados | PRESERVADO | Não foram emitidas alterações globais ou concessões de acesso na origem |

## Execução e calibração

boundary-probe-171e5a600fb94b19b8d633232381b7d7.json: ensaio inicial de token/job, incluindo CLIs reais. Codex 0.147.0 falhou com acesso negado a diretórios técnicos/SQLite e inicialização do app-server; não equivale a falha de autenticação do usuário. Antigravity 1.5.9 completou diagnóstico, embora não conseguisse gravar alguns logs/caches. Não remover restrições para ocultar esses erros.

boundary-probe-88e8434a0dd749ec8085f53c95cdf4cd.json: refinou a observação do cancelamento, com três processos ativos antes e zero depois, e filho observado encerrado. Não contém nova execução autenticada dos agentes.

appcontainer-probe.json: AppContainer real confirmado pelo token, negação de leitura direta/escrita da fonte, porém conexão ao canário permitida. Firewall foi somente consultado e seus três perfis estavam habilitados; não foi atribuída causa definitiva à conexão. Perfis temporários criados pelo ensaio foram removidos, retornos de limpeza 0.

read-broker-probe-initial.json: primeira tentativa negou escrita, mas a limpeza de um FileStream com buffer pendente causou finalização com exceção. Foi corrigido no harness, preservando o relatório inicial. read-broker-probe.json confirma leitura pelo handle, negação por caminho e escrita negada com encerramento normal. Scripts evoluíram entre rodadas; seus hashes atuais não são alegação de identidade retroativa de cada rodada inicial.

agent-cancellation-probe.json: cancelamento dos dois CLIs durante inicialização; não equivale a cancelamento de uma análise longa ou tarefa delegada a serviço preexistente.

## Investigação de rede desta rodada

1. FwpmEngineOpen0 com FWPM_SESSION_FLAG_DYNAMIC retornou sucesso no usuário normal; a sessão foi fechada.
2. FwpmSubLayerAdd0 de subcamada única, vazia e temporária retornou ERROR_ACCESS_DENIED (5). Nenhum filtro de tráfego foi criado. Não houve UAC, instalação de serviço, delegação de ACL WFP ou alteração do firewall global.
3. Codex foi chamado em exec/read-only sem bypass para tentar somente conexão TCP a canário local sintético. Leu a especificação, mas a chamada terminal de conexão foi recusada por política antes da execução. O próprio resultado foi NOT_EXECUTED.
4. Antigravity foi chamado com sandbox e proceed-in-sandbox, negações de escrita/web/MCP. Leu a especificação, mas run_command foi negado por necessidade de permissão indisponível no headless. O agente não chegou a realizar conexão. Não usar dangerously-skip-permissions ou reescrever a operação para contornar a recusa.
5. O listener observou zero conexões nas duas invocações nativas. Isso é consistente com não execução; não é comprovação de firewall ou de allowlist.

Fontes primárias para o desenho:
- https://learn.microsoft.com/en-us/windows/win32/api/fwpmu/nf-fwpmu-fwpmengineopen0
- https://learn.microsoft.com/en-us/windows/win32/fwp/access-control
- https://antigravity.google/docs/sandbox-mode
- https://learn.chatgpt.com/docs/sandboxing

## Componente mínimo proposto, não instalado nem aprovado

A solução candidata é separar executor não privilegiado e mediador de rede com capacidade para instalar filtros WFP dinâmicos exclusivamente para uma identidade de execução dedicada. A operação do mediador seria restrita a iniciar/encerrar uma política de job/AppContainer registrado, sem APIs de regra arbitrária, sem acesso à solução, sem ler credenciais e sem modificar perfis globais do firewall.

O executor só poderia alcançar um proxy local autenticado por execução; o proxy permitiria destinos exatos de inferência/autenticação do agente e consultas de avisos, negando demais destinos/portas, redirects não aprovados, endereços privados e acesso direto por IP. Conexões locais necessárias ao controle interno do agente exigiriam identidade/porta demonstradas, não liberação genérica de loopback. DNS e subdomínios não seriam autorizados por wildcards amplos. IPv4, IPv6, UDP/QUIC, subprocessos e sockets já existentes precisariam de testes negativos próprios.

Os filtros seriam vinculados à identidade dedicada e sessão dinâmica, instalados antes de retomar o processo suspenso e removidos ao finalizar/falhar. O serviço não poderia ser usado para elevar o executor. O proxy sem bloqueio OS de conexões diretas NÃO atende ao requisito. Essa proposta requer nova prova e autorização específica para componente privilegiado; não foi implementada/instalada nem aceita por inferência.

## Decisão atual

O bloqueio de rede NÃO está resolvido. Não foi identificada, nesta máquina e neste ensaio, uma configuração comprovada de controle integral por processo que dispense a capacidade WFP necessária. Isso não prova impossibilidade de todo mecanismo alternativo. Manter B2 e design dependente bloqueados, sem alterar as garantias da constituição ou permitir tráfego extra.

Próximo passo requer decidir/provisionar o mecanismo privilegiado estreito ou demonstrar integração suportada equivalente. Instalação/elevação, modificação de regras globais e relaxamento de política NÃO estão autorizados por este registro.
## Atualização da decisão — 2026-11-16

Mediador privilegiado mínimo aprovado como arquitetura; sem instalação, elevação ou novos testes nesta atualização. Linha de decisão arquitetural: APROVADA. Linhas de aplicação de filtros, canário e allowlist: continuam NÃO EXECUTADAS/PENDENTES para o mediador novo. Executor sem privilégios, origem somente leitura e firewall global preservados. Ver network-mediator-decision.md e a seção de arquitetura em plan.md.


## Preparação do protótipo — 2026-11-16

| Item novo | Estado |
|---|---|
| Protótipo separado e build Release | PASSOU (zero avisos/erros) |
| Autotestes sem privilégios | PASSOU (15 checks; zero operações privilegiadas) |
| Rollback e filtros por SID no Windows | PREPARADO, NÃO EXECUTADO |
| TCP permitido e destinos negados no mediador | PREPARADO, NÃO EXECUTADO |
| Limpeza de filtros/perfis na prova privilegiada | PREPARADO, NÃO EXECUTADO |
| Instalação/elevação | NÃO REALIZADA |

Manifesto e pedido exato de autorização registrados. Não reclassificar as linhas anteriores de rede como aprovadas até executar e revisar a prova.

