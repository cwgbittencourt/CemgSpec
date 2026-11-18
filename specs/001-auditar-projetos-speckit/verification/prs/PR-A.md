# PR-A — Carregar projetos SpecKit com leitura protegida e invalidação documental

Atualização posterior ao teste manual: candidato local sobre0fa5428, fonte `sha256:f6e296be248c77cb91dd07dbd4236c955755ff03ea63fd159278112bac008d58`, suíte59/59. A rodada final em PR-A-validation.md/PR-A-review.md e manual-project-opening-review.json substitui os resultados57 abaixo como estado corrente. Duplicação e sobreposição de seletores corrigidas; reteste manual e aprovação humanos pendentes.

Estado: BLOQUEADO para integração normal; descrição preparada, aprovação humana pendente.
Tipo: FECHAMENTO_DE_HISTORIA (escopo técnico US1; aceite funcional humano pendente).
História e critérios: US1-AC01–AC04, fundação e proteção de leitura.
Tasks incluídas: T001–T020 e migração T095–T098. T020 acrescenta somente descrição e acompanhamento documental.
Base resolvida: `79d9c3d08d75afe2fef7e233493b8648e61ed4e0`. Candidato de produto/testes: `0fa5428159cc57707617ed51b5b236ead0155e92`. Fonte exata validada: `sha256:fbdf9dd12adb61c19c2eee94ffbd7ba6c971a3cd013b2c6946b85ec6abc79b40`, conforme [manifesto](../../evidence/maintenance-code-manifest.json).
Predecessores: nenhum PR. Primeiro incremento do plano, anterior a PR-B/C/D. Base de código pré-implementação: `197548db2e3efa20c23e1f66b6a0ffb86c0f328d`.
Diff incremental reproduzível: `git diff 79d9c3d08d75afe2fef7e233493b8648e61ed4e0 0fa5428159cc57707617ed51b5b236ead0155e92`. Mudanças locais T020: este documento, índice, estado técnico, task e hashes; produto/testes/ferramentas inalterados. PR previsto, sem URL: nenhum PR foi aberto ou integrado por esta task. O desenvolvimento está no checkout main; referências acima identificam o intervalo histórico, não um PR remoto existente.

## Problema e comportamento entregue

Ao escolher uma solução SpecKit, o usuário precisa consultar os documentos sem alterar a origem e sem misturar contextos. Este incremento permite selecionar origem e destino externo pelo seletor nativo, descobrir funcionalidades e indicar artefatos presentes/ausentes. Destinos iguais, internos ou junções que resolvam para a origem são recusados.

O catálogo autoriza documentos por IDs e handles Windows de leitura. A aplicação detecta edições externas, invalida a versão em uso e preserva os bytes editados e os snapshots anteriores. Alternar projeto ou destino mantém contextos separados e não move nem apaga o histórico externo existente.

As correções de review incluem tratar limites de Open/Refresh com recuperação, ignorar alterações irrelevantes de metadados de diretório, reconhecer a constituição com hash mesmo sem pasta specs e calcular presença apenas pelo caminho do artefato na raiz da funcionalidade. Um evidence/spec.md não preenche a ausência do spec.md raiz.

## Escopo e estado intermediário

- Incluído: sessão local emitida pelo host; listener/Host/Origin loopback e antiforgery; modelos de identidade/snapshot/proteção; catálogo/broker; seletor e página Projetos; monitor de alterações; infraestrutura interna de comandos confiáveis/canais/limites; testes e evidências próprias de desenvolvimento.
- Fora deste incremento: preview Markdown/Mermaid, dashboard/ficha/temas completos, execução real de analyze ou gate no produto, adaptadores de agentes, Salvar e histórico oficial gerado pelo produto. Pertencem aos incrementos posteriores.
- Invariantes: origem somente leitura pela aplicação, identidade/contenção revalidadas no acesso real, rejeição de hardlinks/reparse indevidos, nenhuma execução de código/scripts/configuração da solução auditada, nenhum relatório antes de Salvar. Baselines e destino vazio são verificados nas fixtures.
- Comportamento incompleto protegido: Executar verificação e Salvar resultados permanecem desabilitados; enum de verificações fechado em AnalyzeDocuments/CheckGate. Runner interno exercitado com executáveis confiáveis de teste; não constitui prova dos futuros adaptadores.
- Compatibilidade: Windows x64, SDK .NET10 conforme global.json, dependências de testes fixadas por locks; sem banco/migração de dados do usuário. Host local Blazor Interactive Server, sem exposição LAN.
- Divisão: fronteira de leitura, seleção e invalidação formam uma unidade verificável e mantêm autorização/identidade desde o primeiro uso. Preview, projeções documentais, gate, agentes e persistência ficam em incrementos próprios com provas específicas antes da habilitação.

## Testes e evidências

Resultados abaixo executados na T019, na mesma fonte do candidato; conferidos na T020, sem alegar nova execução da suíte. Diretório de execução: raiz do próprio SpecAudit; fixtures novas sintéticas em TEMP, nunca solução real auditada.

| Critério/cenário | Execução e versão | Resultado real | Evidência |
|---|---|---|---|
| US1-AC01–AC04, fronteira, sessão e regressões | `dotnet test SpecAudit.slnx --no-restore --logger 'trx;LogFilePrefix=T019-full' --results-directory specs/001-auditar-projetos-speckit/evidence/logs/T019/full`; fonte acima | PASSOU, exit0;57/57, zero falhas/ignorados (7 domínio,16 Windows,34 UI) | [Execução](../../evidence/T019-test-run.json), [validação PR-A](../../evidence/PR-A-validation.md), [TRX/logs](../../evidence/logs/T019/full.txt) |
| REV-PRA-01/02 | Cinco casos de regressão novos, antes/depois, incluídos na suíte57 | Red5 falharam antes; green5 passaram depois | [Achados/resolução](../../evidence/T019-findings.json), [red](../../evidence/logs/T019/red.txt), [green](../../evidence/logs/T019/green.txt) |
| Replay dos cenários do review | `dotnet run --project specs/001-auditar-projetos-speckit/evidence/T019-probes/T019ReviewProbe.csproj` | PASSOU, exit0;2/2 checks | [Saída](../../evidence/logs/T019/probes.txt); T018 original preservada |
| Manutenção de evidências | `python -m unittest discover -s tools -p test_maintain_evidence.py -v` | PASSOU, exit0;6/6 | [Log](../../evidence/logs/T019/maintenance-tests.txt) |
| Dependências fixadas/auditoria | Restore locked e consulta NuGet transitiva, comandos na validação | Exit0; nenhum pacote vulnerável reportado na consulta T019 | [Rodada T019](../../evidence/PR-A-validation.md); aliases de logs idênticos no [manifesto de manutenção](../../evidence/maintenance-manifest.json) |

As evidências técnicas versionadas pertencem ao desenvolvimento do SpecAudit; não são relatórios oficiais produzidos pelo produto. Caminhos pessoais foram sanitizados; falhas e rodadas preservadas; hashes atuais no inventário.

## Review da IA

Modalidade AUTORREVISAO. [Review completo PR-A](../../evidence/PR-A-review.md), rodadas T018/T019; [review US1](../../evidence/US1-review.md), T014/T015. Os dois achados de cada passagem foram corrigidos e revalidados. [Registro T019](../../evidence/T019-findings.json): zero impeditivos conhecidos abertos na fonte acima. Não houve revisão independente ou aprovação humana inferida.

T020 confere descrição, template, links, base/candidato, manifesto e decisões pendentes, sem alteração de código. Nova correção, rebase ou mudança de ambiente relevante exige avaliar impacto, renovar testes e revisão afetados.

## Reprodução pelo revisor ou DEV

Usar o [roteiro US1](../../validation/US1-dev.md): Windows com desktop, SDK10.0.404 compatível com global.json, PowerShell e Edge instalados. O roteiro cria fixtures próprias e executa restore locked, quatro cenários nativos, sete regressões T015 e suíte completa57. Os casos filtrados estão incluídos no total; não somar como testes distintos. Não exige credencial/agente ou solução real.

Esperado: duas funcionalidades e ausência correta de documentos; destinos inválidos recusados; origem e histórico preservados; edição externa causa Changed/cancelamento; limites retornam erro recuperável. TRX, hashes/baselines e JSONs dos cenários permitem observar os efeitos. Não é necessário nem autorizado instalar/elevar para reproduzir; se faltar ambiente, registrar o bloqueio real.

Limitações: passeio manual completo da GUI carregada permanece com CONFERENCIA_PARCIAL. O harness usa seletor nativo e IO/monitor reais; host/Edge reais cobrem a página inicial. Casos T019 de presença renderizam o componente Razor real via HtmlRenderer e picker controlado. Não há promessa de sandbox OS, contenção de agente comprometido ou prova de todos os padrões de descendentes. A consulta NuGet não é auditoria do SO/SDK/browser.

## Aprovação humana e integração

- Aprovação humana: PENDENTE.
- Responsável, data, versão e fonte da decisão: não informados; nenhuma decisão atribuída pela IA.
- Política de PRs: validação técnica, autorrevisão, correções e estado seguro comprovados para este escopo. Sem predecessor pendente; T020 entrega a descrição. Aprovação humana continua ausente.
- Prontidão para integração normal: NAO.
- Exceção explícita: nenhuma.
- Integração efetiva: PENDENTE; nenhuma publicação, push, abertura ou merge realizado nesta task.

## Fechamento funcional

US1 concluída no escopo técnico do incremento, com validação integrada e roteiro consolidado. [Estado/aceite US1](../../acceptance/US1.md) e registro humano no roteiro permanecem pendentes de execução e decisão do DEV. Aprovar PR, integrar e aceitar a história são decisões separadas. T020 não concede nenhuma delas; não instala CI, proteção de branch nem autenticação de aceite.

## Condições verificáveis

Aplicadas as políticas CEMG instaladas e a constituição vigente4.1.0. [Estado do gate](../implementation-stage.json) mantém resultado BLOCKED pela ausência de aprovação humana; hashes verificam integridade, não autenticidade ou suficiência das decisões. Não há exceção nem teste obrigatório convertido de NÃO EXECUTADO em PASSOU. Históricos SEC-001/isolamento OS permanecem históricos.

## Demonstração executável do incremento

- Iniciar e acionar: seguir os comandos de [US1-dev.md](../../validation/US1-dev.md), da raiz de desenvolvimento do SpecAudit.
- Entrada/esperado/observado: fixtures sintéticas novas, APIs públicas, seletor/IO/monitor Windows reais; quatro cenários US1 e baselines preservados, conforme57 testes aprovados na fonte acima.
- Evidência da execução pela IA: [produto T019](../../evidence/T019-product-execution.json) e [validação](../../evidence/PR-A-validation.md).
- Reprodução/depuração do DEV: mesmo harness e pontos de observação descritos no roteiro; registrar resultados e decisão humana nas seções reservadas.
- Impossibilidade: nenhuma confirmada pelo DEV; não há exceção humana inferida. Execução/aceite do DEV continuam PENDENTES.
