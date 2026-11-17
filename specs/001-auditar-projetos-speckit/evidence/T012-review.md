# T012 — invalidação documental da US1

Estado: CONCLUIDA no escopo técnico do incremento. Modalidade: AUTORREVISAO pela IA, 2026-11-16, America/Sao_Paulo. Não é revisão independente nem aprovação humana.
Base: 197548db2e3efa20c23e1f66b6a0ffb86c0f328d + alterações locais identificadas por implementation-code-manifest.json. Fontes deste incremento preservadas em T012-code/; checkpoint anterior em T012-before/. Constituição 4.0.0, predecessor T011, PR-A previsto e não aberto.

## Critério e implementação

US1-AC04, cobertura de infraestrutura: DocumentChangeMonitor observa nomes/metadados no diretório raiz, filtra caminhos reconhecidos, considera ambos os lados de renomeação, diretórios criados depois da abertura e erro de observação como invalidação conservadora. Nenhum conteúdo de código é lido pelo monitor. Broker mantém acesso readonly por handles/IDs; filtro de configs do catálogo agora ignora diferenças de caixa.

ProjectService captura DocumentVersion imutável, valida geração/revisão/cancelamento e relê os documentos pelo broker para conferir identidade/hash sem depender somente da entrega assíncrona de eventos. Alteração, refresh, troca de projeto e descarte cancelam trabalho próprio. Uma versão antiga continua rejeitada depois do refresh. Atualizar não restaura edições nem salva relatórios. Catálogo recusa refresh que concorra com nova alteração, mantém dados anteriores pendentes e sincroniza funcionalidades após refresh bem-sucedido, preservando identidades existentes.

## Achados e revalidação

- REV-T012-01, impeditivo corrigido: monitor invalidava notes.md fora do catálogo. Reproduzido em logs/T012-red.txt; filtro corrigido e teste passou.
- REV-T012-02, corrigido em autorrevisão: remoção/renomeação de diretório documental com ponto no nome precisava considerar descendentes já catalogados. Teste de filesystem real incluído e aprovado.
- REV-T012-03, limitação de ordenação explicitada: lote de eventos pode chegar durante refresh. logs/T012-suite-verified.txt preserva a falha do teste que presumia estabilização em 150 ms. Harness passou a aceitar Changed, conferir pendência e repetir a ação explícita por até 5 segundos; não houve relaxamento da proteção de produção. logs/T012-suite-complete.txt: 41 aprovados, 0 falhos/ignorados.

Nenhum impeditivo conhecido aberto para a infraestrutura implementada. Avaliados fonte, contratos, chamadas readonly, cancelamento e testes. A suíte inclui broker/serviço com arquivos reais Windows e host/Edge real para regressão da página. Picker é controlado por IFolderPicker nos testes do serviço; edição no meio da leitura usa barreira BeforeRead e escrita real. Isso não demonstra operação humana do diálogo nativo.

Prepare/Execute/Save e resultados Stale completos ainda não existem. A prova nas respectivas fronteiras é da API genérica CaptureDocumentVersion/EnsureCurrent; integração aos comandos, persistência e fluxo completo permanece nas tasks posteriores. Nenhum agente/gate foi acionado automaticamente. Sem aceite de US1, PR aprovado, commit, push ou publicação.
