# Revisão documental de tasks — 2026-11-16

Modalidade: autorrevisão pela IA; não revisão independente ou aprovação humana.
Candidato: sha256:ebeaf59cd42bb86b3f41773610c4b9340fa477eb26cc4c148d1375ee6379d826. Base Git main/79d9c3d; artefatos Phase 1 locais identificados por hashes.
Preset cemg-speckit 3.0.0; tasks-template replace, fonte instalada resolvida por setup-tasks.ps1. Hooks pré/pós extensions.yml ausente; nenhuma obrigação adicional identificada nas políticas.

## Análise de consistência e completude

- 94 tarefas sequenciais únicas, todas pendentes, com caminho explícito e formato de checkbox CEMG. Contrato para cada ID: origem, dependências, entrega, procedimento e resultado esperado.
- Sete histórias ativas e 29 critérios/cenários cobertos; US7/FR-017 não executados/implementados, permanecem futuros. Tasks não introduzem leitura de código ou comandos extras de verificação.
- Grafo acíclico, todas as dependências referem-se a IDs anteriores; nenhuma verificação exige tarefa futura. PR-B valida também os fechamentos US2/3/4, não só shell/US8. US8 P2 antecipa-se apenas por dependência de shell/PR-B, sem mudar prioridade funcional.
- Cinco barreiras com prova real obrigatória antes da tarefa de habilitação: ReaderBoundary, GateAdapter, AnalyzeCodex, AnalyzeAntigravity e ExternalWriter. Falha/NÃO EXECUTADO não habilita; checkbox concluída sem prova aplicável não satisfaz a dependência. Gate/leitor independem da disponibilidade do agente.
- Contratos do broker/Path de capacidade cobrem TOCTOU no momento de acesso, todo grafo de evidências e execução gate sem --project/main/active/rede/shell/código da origem. Prova de equivalência do core oficial e runner está planejada antes de habilitação.
- Codex/Antigravity têm construção, prova e habilitação separados por versão/política; estado externo e rede/descendentes devem ser observados. Nenhuma prova antiga de 0xC0000142 é apresentada como sucesso ou autorização nova.
- Constraints literais de 16 entidades e estados compartilhados mapeados às tarefas de implementação, sem novos enums/limites/retencões de produto inventados. Modelos permanecem documentais e em memória.
- Todas as tarefas mapeadas a PR-A/B/C/D/E; cada PR tem execução real de testes, review, correção/revalidação e descrição pr-template. Aprovação de PR e aceite DEV continuam PENDENTES até fonte humana real; não há checkbox de aprovação humana.
- Cada história inclui execução pelo meio real de uso, comparação de efeitos, review/correção, roteiro IA/DEV e estado técnico; último PR consolida cenário integrado e auditoria das dependências do próprio SpecAudit. Escrever teste/roteiro não é teste aprovado.
- Exemplos [P] limitados a testes em arquivos distintos e adaptadores desabilitados separados, com pré-requisitos explícitos. Não autorizam iniciar agentes nesta geração.

## Achados tratados durante geração

- Fechamento PR-B precisaria aguardar também reviews/correções/roteiros de US2/3/4: dependências explícitas adicionadas ao seu ciclo de validação.
- Navegação US8 não pode exigir UI futura PR-C/E: tarefa shell define placeholders indisponíveis de Verificações/Sugestões, que serão substituídos em incrementos próprios.
- Prova Codex não pode exigir Antigravity ainda não implementado: validação deve filtrar o provider pertinente e preservar selos independentes.

Resultado: sem achado impeditivo DOCUMENTAL aberto nesta revisão; tasks prontas para futura solicitação de implementação após gate. Permanecem todas NÃO EXECUTADAS. Nenhum app/protótipo/teste foi implementado, agente/prova executado, pacote alterado, instalação/elevação/commit/push realizado. A somente leitura da solução não foi flexibilizada. Inspeção/validação da geração não constitui aceite funcional.

## Vinculação dos mockups — 2026-11-16

Três imagens aprovadas copiadas sem edição para design/mockups/, com catálogo/guia V1 e manifesto SHA256. Spec, contrato de UI e tarefas de implementação/comparação visual agora as referenciam. Aparência aprovada é preservada; comportamento atual continua documental, combo analyze/check_gate, Save explícito externo e proteção antes da habilitação. Texto/comandos antigos nos pixels não são requisitos executáveis. Sem task nova, mudança de dependências ou checkbox concluída. Nenhum código/pacote/prova foi implementado/executado.
