# Autorrevisão — preparação T001–T003

Executor: IA; modalidade: autorrevisão, sem revisão independente ou aceite humano.
Data: 2026-11-16. Base: 197548db2e3efa20c23e1f66b6a0ffb86c0f328d + diff local.

Escopo: projetos de teste próprios, fixtures e sessão local. Nenhum executor/agente/broker/writer de produção habilitado.
- Dependências fixas, restore/auditoria e runners executados: setup-validation.md.
- Fixture cria raiz TEMP exclusiva e dois projetos independentes; corpus com duas funcionalidades, 30/40 tarefas e 12/20 requisitos, negativos gate, canário e baseline. Junction swap real coordenado por Barrier, sem alterar ACL ou elevar. Diretórios sintéticos preservados, sem limpeza recursiva automática.
- LocalHostFixture inicia processo próprio com porta loopback, espera resposta real, encerra árvore própria. Testes HTTP exercitam host real, sem TestServer/mock.
- Sessão usa token criptográfico de 256 bits e registro exclusivamente em memória; cookie HttpOnly/SameSite Strict; IDs alheios negados pelo serviço. Host/Origin exatos e cliente loopback exigidos; sem CORS/forwarded headers.
- Ações HTTP mutantes exigem Origin + sessão emitida + antiforgery. Negociação SignalR é a exceção de transporte: exige Origin exato e sessão prévia, não oferece ação de negócio. Nenhum endpoint de caminho ou mutação de produto existe neste incremento.
- Tokens antiforgery usam data protection efêmero; DetailedErrors desativado; nenhuma sessão/documento é salvo em disco pela aplicação.

Achado corrigido durante execução: teste exigia string exata no-store, mas o framework acrescenta no-cache; asserção agora verifica diretiva NoStore sem rejeitar proteção adicional. Evidência anterior preservada.

Limitações: fixtures não comprovam barreira Windows do produto; Playwright apenas inicializado, sem navegador lançado. T006/T010 e histórias ainda pendentes. Sessão/circuito de uma UI interativa e controles do produto serão revalidados quando forem introduzidos. Nenhum teste de produto ou selo de proteção concedido.
