# T007 — Registro de capacidades e limites: implementação parcial

Data 2026-11-16; executor IA. Dependency T006 pendente de garantia completa após SEC-001/T010; T007 não concluída.

CapabilityRegistry inicia todas as capacidades em PendingValidation, exige os nomes de provas requeridos por capacidade e versão/plataforma/digest correspondentes. Nenhuma prova real é registrada pelo host neste incremento. Testes usam instâncias isoladas e evidência sintética explicitamente identificada; não concedem selo de produção.

Política técnica specaudit-limits-v1: documento 4 MiB, grafo 256 nós/profundidade 32, saída 65536 caracteres, duração 30 segundos. Calibração inicial somente com corpus pequeno de fixture e testes do valor máximo e máximo+1. Excesso produz LimitExceeded/Inconclusive; não são metas de desempenho nem limites calibrados para agentes reais. Integração futura deve reavaliá-los com evidência de ambiente/versão.

Launcher limita duração/saída e confirma término de Job; timeout/cancelamento não vira teste aprovado. Testes cobrem revogação de canal, RunId/token/geração alheios, alteração de versão/política/plataforma, prova ausente/incompleta e fronteiras dos limites. Resultado atual dos testes de contrato: aprovados; suíte global continua FALHA devido a SEC-001.
