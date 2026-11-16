# Emenda autorizada: estado dos agentes e relatórios oficiais

Constituição: 1.0.1 -> 2.0.0. Preset: cemg-speckit 3.0.0.
Fonte da decisão: mensagem explícita do usuário nesta conversa: “Sim, pode permitir conversas e caches dos agentes fora da solução verificada. Os relatórios do SpecAudit só devem ser gravados ao clicar em Salvar.”

- Conversas, transcrições, caches e estado interno dos agentes podem persistir antes de Salvar, exclusivamente fora da solução verificada, com localização/comportamento informados.
- Relatórios, sugestões, decisões, exportações e histórico oficial do SpecAudit só persistem por Salvar no destino externo selecionado; logs/caches próprios não podem contornar a regra.
- Estado do agente não exige a mesma pasta dos relatórios e não é importado automaticamente como salvamento oficial.
- Origem permanece somente leitura para aplicação, agente e subprocessos, incluindo metadados e gravações indiretas por links/junções.
- Nenhuma execução de solução/testes/builds ou ampliação de rede foi autorizada.

B1: encerrado por mudança explícita do requisito, não por demonstração de ausência de persistência. As evidências históricas mostram persistência externa e foram preservadas. B2: permanece bloqueio técnico de integração, isolamento, rede, IPC e descendentes. Não há aceite de história ou aprovação técnica integral dos agentes.

Artefatos sincronizados: constituição, especificação, checklist, plan, research, relatório contextualizado da prova, revisões e hashes dos gates. JSONs brutos e scripts dos ensaios não foram reescritos para modificar resultados antigos. Reavaliar condições técnicas antes de liberar tasks.