# Revisão de isolamento — estado real

Data: 2026-11-16. Nenhuma correção de segurança aprovada; nenhuma retomada de tarefa dependente.

O pedido autorizou revisar o desenho, atualizar plan/tasks e retomar somente após correção e revalidação. plan.md e tasks.md receberam uma revisão e T095–T098, mas o mecanismo proposto não passou nas provas. A constituição não foi relaxada.

## Resultados e limites

- LPAC combinado com SID restritivo adicional não completou o bootstrap. Rodadas e TRX estão em evidence/logs/isolation-v2-*.
- A alternativa de token restrito verificou as restrições pedidas no processo suspenso, mas não iniciou o harness funcional. Não constitui correção nem selo ReaderBoundary.
- O diagnóstico com eventos de depuração identificou falha de inicialização de KERNELBASE.dll em DLL_PROCESS_ATTACH, antes de entrada funcional. A hipótese de carga paralela não resolveu a falha no ensaio serial e não é apresentada como causa comprovada.
- Foram usados processos próprios sintéticos, tokens e, no diagnóstico, leitura/ajuste de metadados na memória do worker próprio e suspenso. Não foram usados documentos de usuário, instalação, elevação ou ACL de solução real. Essas técnicas foram excessivas para a etapa de entrega e exigem revisão de abordagem.
- Houve um erro de criação de processo Acesso negado, sem causa antivírus confirmada. Separadamente, a revisão automática recusou um ensaio que manteria o privilégio de travessia e alteraria a asserção de zero privilégios. Esse ensaio não foi executado; a remoção do privilégio e a asserção foram preservadas.

## Contenção após a preocupação do usuário

O agente interrompeu os experimentos de baixo nível. RestrictedLauncher contém guarda fail-closed que impede executar o candidato experimental inclusive por testes. Nenhum selo Passed ou exceção humana foi registrado. Os testes originais de negação de acesso não foram removidos ou enfraquecidos.

O último resultado completo 26 aprovados/1 falho pertence ao checkpoint anterior; não representa a árvore experimental atual. As rodadas posteriores foram filtradas e falharam no bootstrap. Os manifestos/READY anteriores são históricos e não devem ser usados para declarar prontidão desta revisão. Os planos manuais continuam rascunhos não executados.

Próximo trabalho: revisar uma abordagem suportada e proporcional antes de executar novos experimentos. Não contornar antivírus, revisão automática, restrições de privilégio ou garantias constitucionais para obter um teste verde.
