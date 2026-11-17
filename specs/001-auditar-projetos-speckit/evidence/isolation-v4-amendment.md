# Emenda autorizada de isolamento — 4.0.0

Data 2026-11-16; autorização explícita do usuário neste chat: flexibilizar o isolamento, manter solução somente leitura e apenas speckit-analyze/check_gate; atualizar plan/tasks e retomar.

A constituição passou 3.0.0 → 4.0.0, preservando ratificação 2026-11-15, qualidade CEMG, execução pela IA/roteiros DEV e decisões humanas. Spec/plan/contrato e T006–T010/T095–T098 foram sincronizados. Instalação do preset não foi alterada.

Retirados do escopo ativo: contenção pelo kernel de agentes arbitrários, AppContainer/LPAC, tokens/impersonação, memória/depuração de bootstrap e worker nativo. Fontes e hashes estão em verification/archive/constitution-v3-isolation/. Os testes de processo de RestrictedLauncherTests e os dois testes de negação por syscall de ReaderBoundaryTests foram arquivados, não aprovados. SEC-001 não foi corrigida: sua garantia foi retirada por decisão humana. O novo contrato permanece testável em APIs reais e filesystem.

Permanece obrigatório: broker readonly e seleção de documentos, validação no acesso, IDs/sessão, destinos externos seguros, nenhuma escrita/execução de origem no fluxo do produto, integrações configuradas sem ferramentas livres/cwd da origem, limites/cancelamento e aviso de ausência de contenção OS. Integração incompatível permanece indisponível.

Sem hook extensions.yml configurado. Templates/políticas de qualidade permanecem instalados e lidos; não há autorização de merge, deploy ou aceite de história. Nenhum experimento anteriormente recusado será retomado.
