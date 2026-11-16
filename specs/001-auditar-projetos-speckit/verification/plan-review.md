# Revisão final do plano e Phase 1 — 2026-11-16

**Candidato**: sha256:6002634026f05e2338e26c0a754815518089b126e4915cefb3c689967159fb89. **Base**: 79d9c3d (main).
**Spec**: sha256:fa084b53b7bc12108f6e3625a4c191de582a4db2122ee9564b111b296eb9d6fa; constituição 3.0.0 SHA256 a2ad0b009b908efd8e2b1ef0b859391ddf19bb5e5c2f46d047139cb8f2a88bb0.
**Preset**: cemg-speckit 3.0.0; plan-template replace resolvido; setup preservou plan existente. Contexto lógico retornado 001-auditar-projetos-speckit; branch Git real main. Hooks pré/pós extensions.yml ausente, sem obrigação adicional identificada.

## Modalidade e limites

Revisão documental pelo agente principal com segunda leitura por subagente de pesquisa (exigida pelo fluxo speckit-plan). Não é aprovação humana nem revisão independente de implementação. Pedido explícito do usuário: finalizar modelo/contratos/roteiro e deixar provas de proteção para implementação, revalidando plan para gerar tarefas. Nenhuma dispensa de somente leitura, funcionalidade habilitada, execução de agente/prova, instalação ou elevação.

## Verificações

| Check de planejamento | Evidência e justificativa |
|---|---|
| spec_ready | Requisitos atuais e gate spec revalidados; produto documental e duas opções, 29 critérios ativos |
| coverage_semantic | 29 pares USn-ACnn/USn-Snn na spec, matriz plan e quickstart; US7 futura excluída; cada linha tem ação/entrada/expectativa |
| procedures_concrete | quickstart traz preparação TEMP, comandos futuros de host/testes, ações por UI/CLI, asserções/efeitos/limpeza; execução ainda NÃO EXECUTADA |
| environment_planned | Windows usuário normal, .NET10, browser loopback, Python confiável, agentes só para analyze, versões de pacotes auditadas ao introduzir; ausências falham fechado |
| data_and_access_planned | Modelo/snapshots e receita sintética conhecidos; broker por handle/IDs, domínio OS restrito, runner Path-capability sem paths reais/--project; estado externo/Save delimitados |
| pr_slices_testable | PR-A a E com base/predecessores/escopo/testes; infraestrutura e provas antes de habilitação dependente; sem IDs prematuros de tasks |
| real_execution_planned | Roteiro reproduzível IA/DEV pelo meio real, 29 cenários, cancelamento/IO/rede/TOCTOU/Save, evidências e decisões humanas separadas |

Constituição I–IX comparada ao desenho. Validação no planejamento é revisão da solução arquitetural e caminho de comprovação; implementação continua obrigada a provar operação real antes de uso. READY do plan não é selo Passed nem aceite. Design AppContainer/broker/Job Object e runner do gate estão definidos; incompatibilidade futura bloqueia adaptador/tarefa/PR dependente e pode exigir revisão, sem relaxar controles.

## Achados tratados

- Matriz do plan tinha mojibake real; regenerada de spec.md em UTF-8 sem alterar evidências históricas.
- Preflight simples do gate permitia janela TOCTOU; desenho final usa core confiável via runner com Path de capacidade e broker no ponto de cada leitura. Equivalência semântica e restrições são provas a entregar na implementação.
- Unavailable não deve ser RunState; definido CapabilityAvailability separado; indisponível impede Run, mudança em preparação termina Blocked.

## O que não foi executado

Host/fluxos UI, suíte de testes futura, proteção de SO, reader/gate/agentes/writer e reprodução do DEV permanecem NÃO EXECUTADOS. Parsing de três blocos PowerShell do roteiro é só checagem sintática, não execução. Nenhuma dependência/pacote alterado ou nova consulta de vulnerabilidade; obrigação de auditoria antes de introduzir/entregar componente preservada. Código do aplicativo/protótipo, constituição e spec não alterados; nenhuma tasks.md criada, commit/push ou aprovação humana concedida.

Resultado da revisão: desenho suficiente para READY de plan e geração de tarefas após conferir hashes/gates. Provas não aprovadas por inferência. Histórico anterior preservado em archive/before-phase1-finalization/ e demais evidências existentes.

## Vinculação dos mockups — 2026-11-16

Três imagens aprovadas copiadas sem edição para design/mockups/, com catálogo/guia V1 e manifesto SHA256. Spec, contrato de UI e tarefas de implementação/comparação visual agora as referenciam. Aparência aprovada é preservada; comportamento atual continua documental, combo analyze/check_gate, Save explícito externo e proteção antes da habilitação. Texto/comandos antigos nos pixels não são requisitos executáveis. Sem task nova, mudança de dependências ou checkbox concluída. Nenhum código/pacote/prova foi implementado/executado.
