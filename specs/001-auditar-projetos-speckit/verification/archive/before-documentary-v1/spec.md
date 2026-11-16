# Especificação: Auditoria de projetos SpecKit

**Preset**: `cemg-speckit` **3.0.0**.
**Template**: `spec-template`.
**Estratégia de composição**: `replace` (o template do preset substitui o core).
**Fonte do template**: `.specify/presets/cemg-speckit/templates/spec-template.md`.
**Manifesto**: `.specify/presets/cemg-speckit/preset.yml`.
**Resolução**: `.specify/scripts/powershell/resolve-template.ps1`.
Estes metadados registram a procedência desta revisão; não comprovam funcionalidades implementadas.

**Branch**: não criada; workspace sem Git detectado | **Data**: 2026-11-15 | **Estado**: revisão documental; consultar verification/spec-stage.json
**Solicitação original**: Consolidar as decisões aprovadas na conversa: acompanhar documentos SpecKit e comparar requisitos ao código, somente leitura, usando agentes instalados e salvando resultados apenas por ação explícita em pasta externa.

## Objetivo e escopo

Permitir ao responsável entender progresso declarado, consistência documental e implementação evidenciada, com conclusões rastreáveis. Aplicação local no navegador, conforme plataforma da constituição 2.0.0. Inclui projetos, funcionalidades, dashboard, ficha de requisito, documentos, verificações, sugestões e histórico salvo.

Primeira versão: leitura aprofundada de C# e Razor e integrações reais de Codex e Antigravity. Outras linguagens têm análise documental e interpretação pelo agente, com suporte parcial explícito. Claude Code e Kiro podem ser descobertos, mas sua execução não é requisito da primeira entrega. A tecnologia interna permanece regida pela constituição, não prescrita aqui.

Exclui edição na origem, alteração temporária seguida de restauração, cópia isolada, execução da solução analisada, testes/builds, serviços/bancos do projeto, atualizações de dependências, PDF e diagramas renderizados além de Mermaid. Não há serviço de análise com aplicativo fechado. Não há certificação de ausência de defeitos ou vulnerabilidades.

## Cenários de uso e teste — obrigatório

### US1 — Carregar e preservar o projeto — Prioridade P1

**História**: Como responsável, quero selecionar origem e destino externo para analisar sem alterar minha solução.
**Justificativa da prioridade**: Segurança é pré-condição de todo o produto.
**Dependências e estado inicial**: Aplicação local e projeto de demonstração não sensível acessíveis.
**Teste independente**: Carregar projeto e testar destino inválido, escrita proibida, alternância e edição externa.

#### Critérios de aceite

- US1-AC01: Solicitar origem e destino externo ao carregar; reconhecer funcionalidades e artefatos SpecKit presentes/ausentes, sem escrever na origem.
- US1-AC02: Recusar destino interno/igual à origem, inclusive por links/junções. Bloquear criação, alteração, renomeação e exclusão pelo aplicativo, agentes e subprocessos, incluindo caches, temporários e metadados, sem limitar a edição normal do usuário.
- US1-AC03: Separar contexto por projeto; ao trocar destino, novos registros usam a nova pasta e o histórico anterior permanece onde está, sem movimento/exclusão automática.
- US1-AC04: Detectar mudança externa durante/depois da análise e marcar resultados afetados como desatualizados/inconclusivos, sem restaurar arquivos nem misturar versões silenciosamente.

#### Cenários de aceite

| Cenário | Critério | Dado | Quando | Então |
|---|---|---|---|---|
| US1-S01 | US1-AC01 | Projeto com duas funcionalidades | Carregar com destino externo | Mostrar artefatos e escopos, sem escrita na origem |
| US1-S02 | US1-AC02 | Link que resolve para origem e executor restrito | Escolher destino ou tentar escrita | Recusar e preservar conteúdo |
| US1-S03 | US1-AC03 | Dois projetos e histórico salvo | Alternar projeto e mudar destino | Manter contextos e arquivos anteriores |
| US1-S04 | US1-AC04 | Análise em andamento | Usuário editar código/documento | Preservar edição e indicar perda de atualidade |

#### Condições para reprodução pelo DEV

- Perfil/permissões: usuário local normal; executor sem elevação.
- Dados/estado inicial: duas soluções seguras, destinos externo/interno e link para origem.
- Integrações/acessos: arquivos locais, sem serviços do projeto.
- Observação: interface, conteúdo antes/depois e tentativas reais de escrita por executor e descendentes.
- Restrições: proteção efetiva exige comprovação no plan antes do fluxo dependente.

### US2 — Ler documentos e diagramas — Prioridade P1

**História**: Como responsável, quero consultar os artefatos como no preview padrão do VS Code, sem editá-los.
**Justificativa da prioridade**: Documentação é a referência da auditoria e deve funcionar sem agente.
**Dependências e estado inicial**: US1; corpus Markdown conhecido.
**Teste independente**: Navegar e comparar modos/recursos de um corpus, com conteúdo ativo e remoto de teste.

#### Critérios de aceite

- US2-AC01: Mostrar árvore de todos os artefatos SpecKit reconhecidos e arquivos das funcionalidades; permitir busca, links relativos e âncoras nos limites autorizados; informar ausência/referência inválida sem inventar conteúdo.
- US2-AC02: Oferecer Preview, Fonte e Lado a lado somente leitura; títulos, listas, tarefas não editáveis, tabelas, citações, imagens locais e código destacado. Usar preview padrão do VS Code como referência, sem reproduzir extensões/personalizações do editor.
- US2-AC03: Renderizar Mermaid localmente com zoom e fonte; diagrama inválido ou outro formato, incluindo PlantUML/Graphviz, mostra fonte e explicação.
- US2-AC04: Sanitizar HTML básico e bloquear scripts/formulários/conteúdo ativo. Recursos remotos não carregam automaticamente; imagem exige Carregar nesta sessão, link externo exige clique e outra aba. Não enviar credenciais herdadas; bibliotecas/fontes/estilos do leitor são locais.

#### Cenários de aceite

| Cenário | Critério | Dado | Quando | Então |
|---|---|---|---|---|
| US2-S01 | US2-AC01 | Links válidos e quebrados | Buscar/navegar | Abrir destino autorizado ou explicar ausência |
| US2-S02 | US2-AC02 | Corpus conhecido | Alternar modos | Preservar equivalência do conteúdo e origem |
| US2-S03 | US2-AC03 | Diagramas válidos/inválidos e não suportados | Abrir offline | Renderizar suportado e explicar demais |
| US2-S04 | US2-AC04 | Script e imagem remota | Abrir e depois autorizar imagem | Nenhum script; rede só após ação |

#### Condições para reprodução pelo DEV

- Perfil/permissões: leitor local de US1.
- Dados/estado inicial: corpus com todos os recursos e referência visual do preview padrão.
- Integrações/acessos: nenhum para locais; endereço remoto controlado para cenário explícito.
- Observação: conteúdo, capturas, navegação, solicitações externas e arquivos intactos.
- Restrições: aparência próxima, sem fidelidade a extensões particulares; só Mermaid renderizado.

### US3 — Acompanhar indicadores e inconsistências — Prioridade P1

**História**: Como responsável, quero indicadores explicáveis para encontrar pendências e evolução.
**Justificativa da prioridade**: É o benefício central, sem dependência obrigatória de IA.
**Dependências e estado inicial**: US1; tarefas/requisitos com totais conhecidos.
**Teste independente**: Conferir números e seguir ocorrências até suas fontes.

#### Critérios de aceite

- US3-AC01: Mostrar tarefas concluídas/todas as tarefas e requisitos integralmente evidenciados/todos os requisitos do escopo. Manter parciais/inconclusivos no denominador; exibir contagens, regra e escopo. Denominador zero é sem itens, não percentual inventado.
- US3-AC02: Mostrar validação separada: requisitos com aprovação integral registrada/total, com origem documental/humana, versão e indicação de que o produto não executou testes. Separar parcial, reprovado, inconclusivo e não verificado.
- US3-AC03: Mostrar impedimentos, dependências pendentes, contradições, referências inválidas, requisitos sem tarefas e tarefas sem requisito, com fontes. Falta de evidência não vira reprovação automática.
- US3-AC04: Filtrar por funcionalidade, história, fase, gravidade e situação quando presentes. Analisar documentos ao abrir e após pausa nas edições; código alterado só marca análise pendente, sem disparar agente. Exibir atualização e escopo.

#### Cenários de aceite

| Cenário | Critério | Dado | Quando | Então |
|---|---|---|---|---|
| US3-S01 | US3-AC01 | 30/40 tarefas e 12/20 requisitos evidenciados | Abrir painel | Mostrar 75% e 60%, com denominadores |
| US3-S02 | US3-AC02 | Aprovação documentada e teste apenas existente | Consultar validação | Diferenciar aprovação registrada de não verificado |
| US3-S03 | US3-AC03 | Contradição e vínculo inexistente | Abrir ocorrência | Explicar e apontar evidências sem remediar |
| US3-S04 | US3-AC04 | Projeto aberto | Editar documentos/código | Atualizar documental após pausa e marcar código pendente |

#### Condições para reprodução pelo DEV

- Perfil/permissões: usuário local com projeto carregado.
- Dados/estado inicial: totais conhecidos, escopo vazio e inconsistências intencionais.
- Integrações/acessos: sem IA para contagens/referências explícitas.
- Observação: fórmulas, filtros, procedência, fontes e atualidade.
- Restrições: artefato ausente gera limitação, não total presumido.

### US4 — Consultar ficha e evidências de implementação — Prioridade P1

**História**: Como responsável, quero entender a avaliação de cada requisito e seus critérios.
**Justificativa da prioridade**: Impede conclusões opacas e falsa comprovação.
**Dependências e estado inicial**: US1/US3; interpretação real depende de US5.
**Teste independente**: Abrir requisito e conferir critérios, vínculos e evidências.

#### Critérios de aceite

- US4-AC01: Ficha é uma tela, não arquivo SpecKit; reunir descrição, história, critérios, plano, tarefas, código/linhas, verificações e sugestões; distinguir vínculos explícitos e sugeridos.
- US4-AC02: Avaliar critérios antes do requisito: evidenciada exige todos os comportamentos obrigatórios no fluxo; parcial indica lacuna; não implementado exige evidência clara no escopo relevante; demais casos são evidência insuficiente. Nome, checkbox, teste existente ou placeholder isolado não comprovam conclusão integral.
- US4-AC03: Separar implementação de validação: aprovação integral exige verificação adequada registrada de todos os critérios na versão aplicável; reprovação exige falha demonstrada; falta de execução é não verificado/inconclusivo conforme causa. IA isolada não aprova, ambiguidade gera pendência e não aplicável exige justificativa.
- US4-AC04: Em C#/Razor, relacionar páginas, rotas, componentes, entradas e caminho até regras/consultas por leitura, com limites explícitos. Outras linguagens mantêm análise documental/interpretativa com suporte parcial identificado.

#### Cenários de aceite

| Cenário | Critério | Dado | Quando | Então |
|---|---|---|---|---|
| US4-S01 | US4-AC01 | Vínculo explícito e inferido | Abrir ficha | Diferenciar e abrir respectivas fontes |
| US4-S02 | US4-AC02 | Filtro só aplica data inicial | Avaliar requisito | Mostrar parcial e evidência do comportamento faltante |
| US4-S03 | US4-AC03 | Código completo e critério sem validação | Consolidar | Implementação evidenciada e validação parcial |
| US4-S04 | US4-AC04 | Fluxo Razor/C# e projeto em outra linguagem | Analisar | Mostrar fluxo e nível de suporte correto |

#### Condições para reprodução pelo DEV

- Perfil/permissões: leitor e agente compatível para interpretação.
- Dados/estado inicial: filtro completo/parcial, requisito ambíguo e placeholder isolado.
- Integrações/acessos: agente validado e autenticação; nenhum serviço do projeto.
- Observação: arquivos/linhas citados, estados e ausência de execução/escrita na origem.
- Restrições: leitura não comprova comportamento em funcionamento.

### US5 — Descobrir agentes e executar análises — Prioridade P1

**História**: Como responsável, quero usar agentes instalados para verificações sob demanda sem comprometer a origem.
**Justificativa da prioridade**: Viabiliza interpretação sem impor outro provedor ao usuário.
**Dependências e estado inicial**: US1; Codex e Antigravity instalados/autenticados para comprovar integrações.
**Teste independente**: Descobrir, validar, executar e cancelar com proteção ativa.

#### Critérios de aceite

- US5-AC01: Descobrir Codex, Antigravity, Claude Code e Kiro; localização manual só para agente desejado não encontrado. Mostrar versão, disponibilidade, compatibilidade/limitações e seleção por projeto.
- US5-AC02: Codex e Antigravity devem comprovar automação, captura de resultados/erros, cancelamento de descendentes e proteção de escrita/persistência. Encontrado mas incompatível fica indisponível; demais integrações podem indicar ainda não suportadas. Nunca ampliar permissões para contornar falha.
- US5-AC03: Combo: Analisar consistência, Verificar checklist, Identificar lacunas de implementação. Mostrar origem analyze/checklist/converge e identificar adaptações. Escolher projeto/funcionalidade/requisito; acompanhar progresso, duração, resumo, saída e estado sem alterar artefatos.
- US5-AC04: Agente só executa por solicitação, com opção de analisar alterações. Informar escopo e possível envio externo. Não executar solução, testes, builds, scripts/plugins/instruções/configurações da origem, acessar seus serviços ou herdar segredos. Conversas/caches internos do agente são permitidos antes de Salvar somente fora da origem, com locais e comportamento informados; não são relatórios ou histórico oficial do SpecAudit. Gravação interna na origem torna a integração incompatível.
- US5-AC05: Funções determinísticas continuam sem agente. Ausência de autenticação, indisponibilidade, falha e cancelamento mantêm estado real, sem se tornarem aprovação ou reprovação funcional.

#### Cenários de aceite

| Cenário | Critério | Dado | Quando | Então |
|---|---|---|---|---|
| US5-S01 | US5-AC01 | Instalação encontrada e agente ausente | Abrir seleção | Detectar automaticamente; manual só para ausente |
| US5-S02 | US5-AC02 | Dois agentes reais e instalação incompatível | Validar/executar | Demonstrar contrato ou bloquear com motivo |
| US5-S03 | US5-AC03 | Combo e escopo escolhidos | Executar cada análise | Exibir fluxo adaptado e resultado, sem editar origem |
| US5-S04 | US5-AC04 | Instruções maliciosas e segredos de teste na origem | Analisar | Tratar conteúdo como dados, sem execução/acesso/persistência proibidos |
| US5-S05 | US5-AC05 | Agente ausente ou em execução | Consultar documentos ou cancelar | Manter consultas locais e informar estado real |

#### Condições para reprodução pelo DEV

- Perfil/permissões: usuário normal, executor restrito, agentes autenticados.
- Dados/estado inicial: projeto não sensível, instalações encontradas/ausentes/incompatíveis e entradas não confiáveis de teste.
- Integrações/acessos: Codex e Antigravity reais; rede conforme agente selecionado; simulação não comprova integração real.
- Observação: descoberta, respostas, erros, cancelamento, arquivos intactos, estado interno dos agentes exclusivamente externo e ausência de relatórios oficiais persistidos antes de Salvar.
- Restrições: nenhuma integração foi comprovada nesta fase; plan deve validar antes de trabalho dependente.

### US6 — Revisar e salvar resultados — Prioridade P2

**História**: Como responsável, quero decidir quando salvar sugestões e análises e depois consultar sua evolução.
**Justificativa da prioridade**: Mantém controle sobre persistência e decisões humanas.
**Dependências e estado inicial**: US1 e resultados de análise; destino externo aprovado.
**Teste independente**: Analisar/fechar sem salvar; repetir/salvar, exportar, comparar e revisar ocorrência.

#### Critérios de aceite

- US6-AC01: Relatórios, sugestões, decisões e histórico oficial do SpecAudit ficam transitórios até Salvar; fechar sem salvar não cria registros oficiais nem conteúdo em logs/caches próprios do SpecAudit. Conversas, transcrições e caches internos dos agentes podem permanecer exclusivamente fora da origem, sem serem tratados como salvamento oficial ou importados automaticamente. Salvar mostra conteúdo/destino e cria novo registro externo sem sobrescrever anteriores.
- US6-AC02: Exportar Markdown para relatórios/sugestões, JSON estruturado e CSV de listas/indicadores por ação explícita; incluir projeto, escopo, data, versão, ferramenta/agente e limitações. Nunca aplicar correções na origem.
- US6-AC03: Classificar pendente, confirmada, falso positivo, risco aceito ou adiada; exigir justificativa/data/responsável para falso positivo e risco aceito. Manter filtrável e solicitar revisão se contexto relevante mudar; risco aceito não aprova falha e falso positivo não comprova atendimento.
- US6-AC04: Comparar histórico salvo em ocorrências novas, persistentes e resolvidas; preservar referências/trechos mínimos necessários, mostrar espaço e permitir exclusão selecionada por ação humana, sem exclusão automática, cópia integral da origem ou exposição de segredos.

#### Cenários de aceite

| Cenário | Critério | Dado | Quando | Então |
|---|---|---|---|---|
| US6-S01 | US6-AC01 | Resultado em sessão | Fechar sem salvar e repetir/salvar | Primeiro não gera registro oficial; segundo fica no destino externo; estado interno do agente permanece externo |
| US6-S02 | US6-AC02 | Resultado revisado | Exportar três formatos | Conteúdo rastreável e origem intacta |
| US6-S03 | US6-AC03 | Ocorrência | Classificar sem/com justificativa e mudar contexto | Exigir justificativa e nova revisão quando afetada |
| US6-S04 | US6-AC04 | Duas análises salvas | Comparar/excluir uma | Exibir diferenças e preservar demais registros |

#### Condições para reprodução pelo DEV

- Perfil/permissões: usuário com escrita externa, sem escrita do executor na origem.
- Dados/estado inicial: ocorrências novas/persistentes/resolvidas e destinos conhecidos.
- Integrações/acessos: nenhuma adicional para resultados existentes.
- Observação: arquivos antes/depois, conteúdo exportado, decisões e histórico.
- Restrições: sem histórico oficial de análises não salvas; histórico interno do agente é separado e pode permanecer externo.

### US7 — Consultar avisos de segurança — Prioridade P2

**História**: Como responsável, quero saber quais vulnerabilidades conhecidas foram encontradas e os limites da consulta.
**Justificativa da prioridade**: Informação de segurança atual sem falsa garantia.
**Dependências e estado inicial**: US1; inventário consultável por leitura e fonte de avisos.
**Teste independente**: Consultar amostras afetadas, grafo incompleto e fonte indisponível.

#### Critérios de aceite

- US7-AC01: Separar avisos do SpecAudit e do projeto; consultar dependências identificáveis por leitura, diretas/transitivas quando disponíveis, sem restore/build. Dependências não identificáveis indicam cobertura incompleta, não ausência de vulnerabilidades.
- US7-AC02: Consultar ao abrir se última consulta disponível superar 24h e quando dependências mudarem; oferecer atualização manual. Operar apenas com app aberto e manter resultados transitórios até Salvar; sem registro salvo, não presumir consulta prévia ao reabrir.
- US7-AC03: Mostrar componente, versão, gravidade, aviso/fonte, correção conhecida e data. Avisar novos achados/aumento de gravidade sem repetição contínua na sessão. Offline mostra último resultado disponível como desatualizado ou ausência de consulta, nunca seguro por falha. Não atualizar dependências automaticamente.

#### Cenários de aceite

| Cenário | Critério | Dado | Quando | Então |
|---|---|---|---|---|
| US7-S01 | US7-AC01 | Pacote afetado e grafo incompleto | Consultar | Expor achado e limite de cobertura |
| US7-S02 | US7-AC02 | Consulta salva antiga ou dependência alterada | Abrir | Atualizar sem salvar automaticamente |
| US7-S03 | US7-AC03 | Resultado anterior e fonte indisponível | Atualizar | Mostrar data/limitação sem declarar seguro |

#### Condições para reprodução pelo DEV

- Perfil/permissões: leitor local com consulta de rede autorizada.
- Dados/estado inicial: inventários seguros, registros com datas conhecidas e aviso verificável.
- Integrações/acessos: fonte real para comprovar integração; indisponibilidade controlada no cenário negativo.
- Observação: aviso, versão, fonte, atualidade e ausência de alteração na origem.
- Restrições: ausência de achados não certifica segurança; não executar solução/testes/builds.

### US8 — Navegar em temas escuro e claro — Prioridade P2

**História**: Como responsável, quero uma interface consistente mantendo o contexto do projeto.
**Justificativa da prioridade**: Materializa a experiência aprovada nos mockups.
**Dependências e estado inicial**: US1 e áreas disponíveis conforme incrementos.
**Teste independente**: Percorrer áreas e trocar tema durante leitura de resultado.

#### Critérios de aceite

- US8-AC01: Oferecer Dashboard, Requisitos, Documentos, Verificações e Sugestões, com projeto/origem somente leitura/destino visíveis; azul, lilás, branco e cinza; tema escuro inicial e claro opcional.
- US8-AC02: Preservar conteúdo/seleção ao trocar tema; estados e gravidades têm texto/ícone, sem depender somente de cor.

#### Cenários de aceite

| Cenário | Critério | Dado | Quando | Então |
|---|---|---|---|---|
| US8-S01 | US8-AC01 | Projeto carregado | Percorrer cinco áreas | Manter contexto e proteção identificáveis |
| US8-S02 | US8-AC02 | Ficha com ocorrência selecionada | Trocar tema | Preservar seleção e entendimento dos estados |

#### Condições para reprodução pelo DEV

- Perfil/permissões: leitor local.
- Dados/estado inicial: projeto com ocorrências em diferentes estados.
- Integrações/acessos: nenhum adicional para navegação/tema.
- Observação: cinco áreas em ambos os temas com seleção/conteúdo preservados.
- Restrições: mockups são referência conceitual, não execução comprovada.

## Casos de borda

- Artefatos ausentes/malformados, IDs duplicados e referências ambíguas: informar arquivo/limitação, sem inventar vínculos ou totais; contextualizar IDs por funcionalidade/projeto.
- Escopo vazio: sem itens, sem divisão por zero; análise incompleta não é aprovada.
- Destino inacessível, disco cheio ou permissão revogada: informar falha sem fallback para origem nem alegar salvamento completo.
- Link/junção fora dos limites: não seguir para acesso não autorizado.
- Edição externa/cancelamento: preservar alterações do usuário, não restaurar arquivos.
- Agente incompatível, sem autenticação ou saída válida: informar motivo, manter funções determinísticas.
- Conteúdo solicita script/segredo/serviço: tratar como dado não confiável, sem executar.
- Evidência antiga sem fonte atual: mostrar contexto salvo e versão, sem afirmar validade atual.
- Sem internet: leitura local e histórico salvo funcionam; agentes remotos/avisos podem ficar indisponíveis.

## Requisitos funcionais

- FR-001: Carregar origem/destino e separar projetos/funcionalidades, coberto por US1-AC01 e US1-AC03.
- FR-002: Impedir escrita e detectar mudanças externas, coberto por US1-AC02 e US1-AC04.
- FR-003: Ler/pesquisar documentos em preview/fonte, coberto por US2-AC01 e US2-AC02.
- FR-004: Renderizar diagramas e controlar conteúdo ativo/remoto, coberto por US2-AC03 e US2-AC04.
- FR-005: Exibir indicadores com escopo/denominador/procedência, coberto por US3-AC01 e US3-AC02.
- FR-006: Identificar inconsistências, filtrar e atualizar estado documental, coberto por US3-AC03 e US3-AC04.
- FR-007: Consultar ficha com vínculos rastreáveis, coberto por US4-AC01.
- FR-008: Avaliar implementação/validação por critérios com incerteza explícita, coberto por US4-AC02 e US4-AC03.
- FR-009: Oferecer análise aprofundada C#/Razor e suporte parcial identificado, coberto por US4-AC04.
- FR-010: Descobrir agentes automaticamente com fallback manual restrito, coberto por US5-AC01.
- FR-011: Validar Codex/Antigravity e bloquear incompatíveis, coberto por US5-AC02.
- FR-012: Executar verificações adaptadas por escopo e capturar estados, coberto por US5-AC03 e US5-AC05.
- FR-013: Restringir execução/rede/segredos/persistência dos agentes, coberto por US5-AC04.
- FR-014: Salvar/exportar apenas por ação explícita no destino externo, coberto por US6-AC01 e US6-AC02.
- FR-015: Revisar ocorrências com decisões justificadas/reavaliáveis, coberto por US6-AC03.
- FR-016: Comparar histórico salvo e excluir apenas por ação explícita, coberto por US6-AC04.
- FR-017: Consultar avisos com cobertura/atualidade honestas, coberto por US7-AC01, US7-AC02 e US7-AC03.
- FR-018: Navegar pelas áreas nos dois temas, coberto por US8-AC01 e US8-AC02.

## Entidades principais

- Projeto: origem, destino externo, funcionalidades, nível de suporte e agente selecionado.
- Funcionalidade: escopo SpecKit que contextualiza documentos e identificadores.
- Documento: caminho autorizado, tipo, conteúdo, referências e identificador de versão.
- Requisito/critério: comportamento esperado, tarefas e estados independentes de implementação/validação.
- Evidência/vínculo: fonte, trecho, versão, procedência e natureza explícita/inferida.
- Análise/verificação: tipo, escopo, agente/ferramenta, início/fim, estado, saída e limitações.
- Ocorrência/sugestão: problema, gravidade justificada, evidências e proposta.
- Decisão humana: classificação, justificativa, responsável, data e contexto de validade.
- Registro salvo/exportação: conteúdo autorizado, formato, destino e metadados; não existe para análise descartada.
- Aviso de segurança: componente, versão, fonte, gravidade, correção conhecida, cobertura e data.

## Critérios de sucesso mensuráveis

- SC-001: Em todos os cenários de análise/cancelamento, zero arquivos da origem são criados, alterados, renomeados ou excluídos pelo aplicativo e seus descendentes.
- SC-002: Em todos os cenários sem Salvar, zero relatórios/decisões/sugestões oficiais do SpecAudit são persistidos; após Salvar, 100% desses artefatos ficam no destino externo autorizado. Conversas/caches internos dos agentes podem existir antes da ação, com 100% de seus locais fora da origem.
- SC-003: Nos conjuntos conhecidos, 100% dos indicadores correspondem às contagens esperadas, incluindo itens inconclusivos e escopos vazios.
- SC-004: Todas as conclusões da amostra apontam evidência verificável ou declaram insuficiência; nenhuma aprovação deriva somente de nome, tarefa marcada ou opinião do agente.
- SC-005: O usuário percorre as cinco áreas, consulta fontes e alterna modos/temas sem editar documentos ou perder seleção em todos os cenários previstos.
- SC-006: As duas integrações obrigatórias demonstram descoberta, execução real, resultado/erro e cancelamento com proteção ativa; 100% das instalações incompatíveis de teste são bloqueadas.
- SC-007: Todos os resultados salvos identificam projeto, escopo, versão, data, procedência e limitações; zero riscos aceitos são apresentados como teste aprovado.
- SC-008: Todos os cenários de consulta indisponível/grafo incompleto comunicam sua limitação; zero correções automáticas são aplicadas às dependências analisadas.

## Premissas, dependências e questões abertas

Fonte: decisões e correções aprovadas na conversa, subordinadas à constituição 2.0.0. A correção final mantém leitura de código. Salvar manualmente substitui propostas antigas de persistência automática. Ler código não autoriza executar solução, testes ou builds.

Usuário previsto: responsável local; DEV reproduz e aceita a entrega. Colaboração multiusuário e hospedagem remota não integram o escopo aprovado.

Dependências de pesquisa no plan: proteção efetiva dos executores e compatibilidade real de Codex/Antigravity, incluindo estado interno exclusivamente externo e relatórios oficiais somente após Salvar; inventários consultáveis sem executar o projeto; invalidação por mudança, limites e cancelamento. Se uma dependência for inviável, apresentar bloqueio e obter decisão sem reduzir requisitos silenciosamente. Não há alegação de compatibilidade ou controle já implementado.

Histórico oficial e reutilização de decisões/consultas do SpecAudit dependem de registros salvos. Conversas/caches internos dos agentes são permitidos fora da origem por decisão explícita do usuário e não substituem Salvar. Estado técnico externo sem conteúdo de análise deve ser delimitado no plan conforme constituição. Nomes/apresentação usam mockups como referência reversível, sem exigir reprodução de extensões ou precisão pixel a pixel.

Nenhuma decisão material de produto permanece identificada nesta revisão. Não foram inventadas metas de desempenho/volume ou retenção. Pesquisas técnicas podem revelar necessidade de decisão antes de implementação dependente.

## Política de aceite

Exigir validação inicial pela IA, revisão documentada e roteiro reproduzível pelo DEV para cada história e incremento aplicável. Autorrevisão é declarada. Aprovação de PR, integração, aceite da história e deploy são decisões distintas. História parcial não recebe aceite funcional.

O desenvolvimento do SpecAudit deve executar seu próprio comportamento e fixtures seguras conforme políticas CEMG; isso não autoriza o produto a executar a solução carregada. Se impossível, manter comprovação pendente com causa/impacto/próximo passo; somente DEV confirma impossibilidade e decide AGUARDAR, REJEITAR ou ACEITAR_COM_RESSALVAS.

## Entrega incremental da história

| História | Incrementos possíveis | Invariantes desde o início | Fluxo completo |
|---|---|---|---|
| US1 | Seleção segura; descoberta; alternância/invalidação | Origem intacta e contextos separados | Carregar e acompanhar mudanças com proteção comprovada |
| US2 | Leitor; modos; diagramas/remotos | Sem edição ou conteúdo ativo | Navegar corpus com recursos previstos |
| US3 | Contagens; ocorrências; filtros/atualidade | Denominadores e procedência honestos | Explicar indicadores até as fontes |
| US4 | Vínculos; evidências; consolidação | Inferência explícita; sem executar origem | Conferir requisito e critérios |
| US5 | Descoberta; adaptadores; análises/cancelamento | Origem intacta; estado interno externo; relatório oficial só por Salvar | Dois agentes reais sob restrições |
| US6 | Revisão; salvamento/exportação; histórico | Somente externo e por Salvar | Salvar, reabrir, comparar e revisar |
| US7 | Inventário; consulta; atualidade | Sem executar/corrigir; limitações visíveis | Fonte real e tratamento de indisponibilidade |
| US8 | Navegação; temas | Contexto/estados legíveis | Cinco áreas nos dois temas |

O plan define PRs por escopo, complexidade e testabilidade, sem metas de linhas/contagem. Incrementos intermediários mantêm estado seguro, sem se apresentar como capacidade completa.

## Condição de saída da etapa spec

Aplicar PASSAGENS.md: critérios observáveis, cenários, escopo, dependências e decisões materiais resolvidas. A checklist é autorrevisão de requisitos, não teste funcional ou aceite humano. Registrar revisão e hashes em verification/; a próxima etapa deve revalidá-los. Nenhuma execução funcional é registrada nesta fase.

## Demonstração da história

Cada história descreve acionamento, observação e condições de reprodução. O plan deve concretizar inicialização, dados seguros, passos, asserções, evidências e limpeza para IA/DEV, incluindo agentes e consultas reais. Simulações ajudam a testar apresentação/erros, mas não comprovam integração real. Não declarar demonstração apenas por compilar, criar roteiro ou atingir breakpoint.
