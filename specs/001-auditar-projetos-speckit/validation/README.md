# Planos de teste manual por User Story

Constituição 4.1.0. Sete roteiros ESCRITOS, cobrindo os 29 cenários ativos; nenhuma execução ou decisão humana inferida. Checkpoint técnico T001–T012 concluído; T013 não iniciada. A migração de isolamento foi validada, mas nenhuma história completa foi aceita.

- [US1 — Carregar e preservar](US1-dev.md)
- [US2 — Documentos](US2-dev.md)
- [US3 — Dashboard](US3-dev.md)
- [US4 — Ficha documental](US4-dev.md)
- [US5 — Verificações](US5-dev.md)
- [US6 — Salvar e histórico](US6-dev.md)
- [US8 — Navegação e temas](US8-dev.md)

US7 adiada. Escrita do roteiro não conclui suas tasks de conferência/fechamento. A seleção real pelo diálogo nativo e os fluxos integrados ainda precisam ser validados; não chamar de CONFERIDO_PELA_IA ou EXECUTADO_PELO_DEV.

## Registro e decisão do DEV — padrão obrigatório

Todos os roteiros acima contêm tabela de execução por cenário, tabela de retestes
e declaração explícita de aceite. O DEV preenche os resultados, evidências,
commit testado/diff, responsável e data/hora/fuso. Campos em branco permanecem
pendentes; PASSOU não significa ACEITO. Os campos não foram preenchidos pela IA.

Após preencher e solicitar a conferência, a IA verifica versão, execução, evidências
e declaração e transcreve a decisão para `acceptance/USn.md`, com referência à fonte.
Inconsistências impedem declarar aceite concluído. Não existe monitoramento automático.
O roteiro [T012-dev.md](T012-dev.md) usa o mesmo padrão para o incremento e não aceita
US1 inteira. Em impossibilidade, seguir o template CEMG mantendo NÃO EXECUTADO e
aguardar decisão humana; ressalvas exigem escopo, riscos e ações posteriores.

Próximos roteiros usam os overrides locais em `.specify/templates/overrides/`,
resolvidos antes do preset. A customização pertence ao projeto; o cemg-speckit
instalado continua na versão 3.0.0. Evidências históricas não foram reescritas.
