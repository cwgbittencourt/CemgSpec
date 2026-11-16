# Nota de escopo — decisão histórica

A decisão do mediador abaixo pertence ao escopo anterior. A V1 documental (constituição 3.0.0) não depende obrigatoriamente de instalar ou repetir o protótipo WFP. Proteção somente leitura e fronteira do agente permanecem obrigatórias e pendentes de prova própria. Nenhuma autorização consumida é renovada. Consultar plan.md vigente.

---

# Decisão arquitetural: mediador mínimo por execução

Data: 2026-11-16. Origem: mensagem explícita do usuário nesta conversa.
Decisão: “Sim, aceito o mediador privilegiado mínimo para filtros temporários por execução, mantendo o executor sem privilégios e a solução somente leitura. Atualize o plano com essa arquitetura, sem instalar ou elevar nada ainda.”

Escopo autorizado: documentação de arquitetura e atualização do plano/validações.
Não autorizado: instalar serviço, elevar processos, aplicar filtros, alterar firewall global, modificar solução verificada ou conceder aceite funcional.

Plano descreve fronteiras, IPC autenticado, perfis de rede fechados, ciclo de vida fail-closed, rollback, integração de leitura/estado e provas obrigatórias. Constituição 2.0.0 preservada: o executor não é elevado e origem permanece intacta.

Decisão sobre arquitetura encerrada; B2 técnico e design final continuam pendentes. Não classificar como testes aprovados ou permissão operacional futura. A autorização de instalação deve ser solicitada sobre um artefato concreto e revisável na etapa apropriada.