# T002–T003 — Execução da preparação

Data: 2026-11-16; executor IA; base 197548db2e3efa20c23e1f66b6a0ffb86c0f328d + diff local.
Diretório de execução: raiz do SpecAudit; .NET 10.0.404/10.0.15, Windows x64.

## T002 — Fixtures

DocumentaryFixture cria raízes TEMP exclusivas com corpus sintético e baseline, sem copiar projeto real. Teste cria duas origens independentes, verifica duas funcionalidades, 40 tarefas/30 marcadas/12 links e 20 requisitos, parses dos negativos gate e detecção de edição externa. FixtureJunction usa junção Windows real: a barreira pausa a observação enquanto o destino muda de origin para reports. Origem permanece intacta. Diretórios/evidências preservados em TEMP, sem remoção recursiva automática. Isso valida o mecanismo de ensaio, não a proteção do produto.

`dotnet test tests/SpecAudit.Windows.Tests/SpecAudit.Windows.Tests.csproj --no-restore --logger trx --results-directory specs/001-auditar-projetos-speckit/evidence/logs/fixture-complete`: exit 0; três testes aprovados. [Saída](logs/fixture-complete.txt).
LocalHostFixture inicia e encerra somente host/árvore próprios em porta loopback dinâmica. O teste HTTP real confirma resposta, sem host externo ou solução auditada.

## T003 — Sessão local

Antes da implementação, os seis testes HTTP foram executados contra o scaffold: cinco falharam por comportamento ausente, um passou por antiforgery já existente no framework. [Rodada red](logs/session-red.txt).
Após implementação: Host indevido=421, Origin indevido=403, mutação sem Origin=403, token não emitido=401, mutação sem antiforgery=400; cookies distintos emitidos com HttpOnly/SameSite Strict e diretiva NoStore. Serviço rejeita ID de sessão alheia e ausência de sessão emitida. Nenhuma operação de análise/Save existe ou é habilitada neste incremento.

Regressão final: `dotnet test SpecAudit.slnx --no-restore --logger trx --results-directory specs/001-auditar-projetos-speckit/evidence/logs/phase1-tests`: exit 0; 13 testes aprovados, zero ignorados/falhos. [Saída](logs/phase1-tests.txt).

Autorrevisão e limitações: [preparation-review.md](preparation-review.md). Nenhuma história funcional concluída, nenhum selo de proteção ou aceite humano concedido.
