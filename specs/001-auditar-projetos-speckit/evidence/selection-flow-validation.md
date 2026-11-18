# Fluxo de seleção explícito — correção após teste DEV

Estado: CORREÇÃO IMPLEMENTADA; VALIDAÇÃO INTEGRAL SOLICITADA BLOQUEADA. Fonte `sha256:89a4535be36e2b206ac62881eeb8f7eabbcc0346dd26a2f16c0e060e1eb1f70f`; alterações locais sobre0fa5428. Modalidade AUTORREVISAO; não revisão independente.

A tela agora separa Carregar projeto → Escolher projeto → Escolher destino → Confirmar carregamento. Cada caminho retornado pelo seletor é resolvido por handle e apresentado antes de carregar. Ao recusar destino interno, ambos continuam visíveis para diagnóstico; origem alterada limpa destino anterior. Cancelamento não muda o projeto ativo.

Suíte61/61 (7 domínio,16 Windows,38 UI), zero falhas/ignorados. Comando e hashes em selection-flow-test-run.json. Teste real Edge verifica abertura/painel/confirmar desabilitado/cancelar. Teste com picker controlado e filesystem real verifica caminhos exatos A/B, recusa e retry. Harness nativo existente também passa, separado da nova sequência UI. Primeira rodada do novo teste de navegador não esperou render assíncrono após cancelar; falha preservada, teste corrigido para aguardar Detached e rodada completa repetida com sucesso.

**Não comprovado:** sequência completa no navegador operando os seletores nativos. Computer-use foi inicializado conforme skill, mas node_repl e cua_repl falharam por erro do sandbox Windows. Não substituir essa comprovação por testes internos. Próximo passo é restaurar ferramenta de desktop e realizar a sequência, conferindo caminhos reais exibidos. Não pedir ao usuário que faça esse reteste antes disso. Sem aceite/aprovação/commit concedidos. Detalhes em selection-flow-review.json.
