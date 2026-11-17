# T011 — seleção e carregamento de projetos

Data 2026-11-16; autorrevisão pela IA; constituição 4.0.0. Escopo técnico da tarefa, não fechamento/aceite da US1.

Implementado: picker nativo Windows de pastas sem botão de criar pasta; origem/destino escolhidos no host, sem campo de caminho arbitrário recebido da web; validação por handle/caminho final; destino interno/igual/junção recusado; sessão contextualizada; alternância de projetos; mudança de destino sem mover/excluir histórico; UI em / e /projetos com funcionalidades/artefatos presentes ou ausentes. Verificações e Salvar permanecem indisponíveis nesta etapa.

Validação: quatro ProjectSelectionTests usam filesystem real e seleção controlada via IFolderPicker para testar os caminhos/dados da aplicação. O teste ProjectPageBrowserTests abre host próprio em Edge headless instalado, confere título, tela e ausência de erro de circuito/JavaScript e captura logs/T011-project-page.png. Revisão visual da captura: paleta escura azul/lilás, hierarquia e estado vazio legíveis; nenhuma gravação oferecida ao carregar.

Limite explícito: o diálogo nativo de pastas não foi operado pela IA de ponta a ponta; os testes de serviço substituem somente a seleção humana de caminhos. Sua execução manual no desktop permanece na validação integrada da US1, sem ser declarada conferida aqui. Nenhum agente externo ou exportação foi demonstrado. O monitor inicial escrito antes do pedido de parada não conclui T012.

Achados corrigidos: componente/serviço tinham o mesmo nome; título com codificação regional incorreta; erros tipados de destino precisavam ser capturados na UI. A suíte final passou após as correções. Não há achado impeditivo conhecido de implementação da T011 no escopo acima; aceite da história, integração PR-A e UI completa continuam pendentes.
