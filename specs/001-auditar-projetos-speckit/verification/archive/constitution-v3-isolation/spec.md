# Especificação: Auditoria de projetos SpecKit

**Preset**: `cemg-speckit` **3.0.0**.
**Template**: `spec-template`.
**Estratégia de composição**: `replace` (o template do preset substitui o core).
**Fonte do template**: `.specify/presets/cemg-speckit/templates/spec-template.md`.
**Manifesto**: `.specify/presets/cemg-speckit/preset.yml`.
**Resolução**: `.specify/scripts/powershell/resolve-template.ps1`.
Estes metadados registram a procedência desta revisão; não comprovam funcionalidades implementadas.

**Branch**: main | **Data original**: 2026-11-15 | **Revisão**: 2026-11-16 | **Estado**: revisão documental; consultar verification/spec-stage.json
**Solicitação vigente**: Limitar a primeira versão a speckit-analyze e check_gate, ambos em uma combo, mantendo a solução somente leitura. Análise de código pertence a versão futura. Esta decisão substitui o escopo original de comparação com implementação.

## Objetivo e escopo

Permitir ao responsável acompanhar o progresso declarado nos documentos, inconsistências documentais e estados registrados dos gates do SpecKit, com conclusões rastreáveis. Aplicação local no navegador, conforme constituição 3.0.0. Inclui projetos, funcionalidades, dashboard, ficha documental de requisito, documentos, verificações, sugestões e histórico salvo.

A primeira versão lê exclusivamente documentos e registros gerados pelo SpecKit e recursos locais de apresentação a eles vinculados e autorizados. O suporte independe da linguagem do projeto: não há interpretação de código C#, Razor ou de outras linguagens. Referências a código podem ser exibidas como texto do documento, sem abrir ou analisar o código referenciado.

A combo de verificações contém exatamente duas opções: **speckit-analyze — Consistência documental** e **check_gate — Validar gates**. Não admite digitação de comandos ou outras opções. analyze usa agente compatível e é identificado como adaptação documental restrita, sem executar hooks/scripts da origem; check_gate é determinístico e funciona sem IA. Codex e Antigravity continuam como integrações previstas para analyze; descoberta não comprova compatibilidade. Claude Code e Kiro podem ser descobertos, mas execução não é requisito da V1.

Fora da V1: leitura/análise de código-fonte, comparação requisito–implementação, parser C#/Razor, converge, checklist, clarify e demais comandos; inventário/consulta de vulnerabilidades da solução carregada; execução de solução, testes, builds, restore, serviços ou scripts/hooks da origem. Sem shell livre, comandos encadeados, cópia isolada da solução ou edição seguida de restauração. PDF, diagramas além de Mermaid e serviço com app fechado continuam excluídos. A auditoria de vulnerabilidades do próprio SpecAudit durante desenvolvimento/manutenção permanece obrigatória.

## Referências visuais aprovadas

Os três mockups aprovados estão versionados em [design/mockups/README.md](design/mockups/README.md), com [dashboard/ficha](design/mockups/dashboard-requisito.png), [documentos/sugestões](design/mockups/documentos-sugestoes.png) e [verificações/pasta externa](design/mockups/verificacoes-pasta-externa.png). A implementação deve preservar sua identidade visual, estrutura de navegação, hierarquia, cards/painéis e linguagem de cores/temas, observando o guia de adaptação para V1. Os arquivos originais permanecem sem edição e têm hashes no manifesto local.

As imagens são históricas e ilustrativas. Seus comandos, percentuais, textos e estados antigos NÃO ampliam o escopo: apenas speckit-analyze/check_gate na combo, análise exclusivamente documental, nenhuma leitura de código ou execução de testes/builds/restore da solução, sem cópia isolada, relatórios oficiais só por Salvar externo e habilitação após prova de proteção. Especificação/contratos atuais prevalecem. Não reproduzir erros de texto da imagem ou confundir mockup com software/teste demonstrado; não se exige cópia pixel a pixel.

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
| US1-S04 | US1-AC04 | Análise em andamento | Usuário editar documento monitorado | Preservar edição e indicar perda de atualidade |

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

- US2-AC01: Mostrar árvore de todos os documentos e registros SpecKit reconhecidos das funcionalidades; permitir busca, links relativos e âncoras nos limites autorizados; informar ausência/referência inválida sem inventar conteúdo.
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

### US3 — Acompanhar progresso documental e impedimentos — Prioridade P1

**História**: Como responsável, quero indicadores explicáveis dos registros SpecKit, para identificar pendências sem confundir documentação com implementação.
**Justificativa da prioridade**: Benefício central, independente de IA para contagens explícitas.
**Dependências e estado inicial**: US1; documentos com totais conhecidos.
**Teste independente**: Conferir contagens e navegar de cada indicador às suas fontes.

#### Critérios de aceite

- US3-AC01: Mostrar tarefas marcadas concluídas/total de tarefas e requisitos com vínculo documental explícito a tarefas/total de requisitos. Nomear a segunda medida cobertura documental, nunca implementação evidenciada. Manter itens sem vínculo/inconclusivos no denominador; mostrar regra/contagens/escopo; zero é sem itens.
- US3-AC02: Mostrar estados e validações registrados nos documentos/gates com fonte, revisão e atualidade. Distinguir estado registrado do resultado atual de check_gate e de falha da execução. READY e hashes válidos não comprovam funcionamento ou aceite humano autenticado. Não oferecer percentual de implementação comprovada.
- US3-AC03: Mostrar impedimentos registrados, dependências pendentes, contradições, referências inválidas, requisitos sem tarefas e tarefas sem requisito, com fontes. Referência a código fica não verificada nesta versão, sem leitura do destino.
- US3-AC04: Filtrar por funcionalidade, história, fase, gravidade e situação quando presentes. Atualizar índices/contagens locais ao abrir e após pausa nas mudanças dos documentos monitorados. analyze e check_gate só executam por clique; mudanças documentais invalidam seus resultados afetados. Não monitorar/analisar conteúdo de código.

#### Cenários de aceite

| Cenário | Critério | Dado | Quando | Então |
|---|---|---|---|---|
| US3-S01 | US3-AC01 | 30/40 tarefas marcadas e 12/20 requisitos vinculados | Abrir painel e escopo vazio | Mostrar 75% declarado e 60% de cobertura documental; vazio sem percentual |
| US3-S02 | US3-AC02 | Registro READY e declaração humana em documento | Consultar painel | Exibir procedência, sem implementação/teste/aceite comprovados |
| US3-S03 | US3-AC03 | Contradição, vínculo ausente e referência a código | Abrir ocorrências | Mostrar fontes e limite documental, sem ler código |
| US3-S04 | US3-AC04 | Resultado anterior e documento alterado | Atualizar documento e filtrar | Atualizar contagens e marcar verificação desatualizada, sem executar agente/gate |

#### Condições para reprodução pelo DEV

- Perfil/permissões: usuário local normal com projeto carregado.
- Dados/estado inicial: totais conhecidos, escopo vazio e inconsistências documentais.
- Integrações/acessos: nenhuma IA para contagens explícitas; sem leitura de código.
- Observação: fórmulas, procedência, fontes e ausência de execução automática.
- Restrições: documentação ausente gera limitação; checkbox não comprova implementação.

### US4 — Consultar ficha documental do requisito — Prioridade P1

**História**: Como responsável, quero consultar descrição, critérios e vínculos documentais de cada requisito.
**Justificativa da prioridade**: Permite compreender a origem de cada conclusão.
**Dependências e estado inicial**: US1/US3; interpretação por IA depende de US5.
**Teste independente**: Abrir requisito e seguir critérios/vínculos até documentos permitidos.

#### Critérios de aceite

- US4-AC01: Ficha é uma tela, não arquivo SpecKit. Reunir descrição, história, critérios, referências ao plano/tarefas, ocorrências, verificações e sugestões. Distinguir vínculos explícitos de inferidos. Não abrir código/linhas nem oferecer avaliação de implementação.
- US4-AC02: Para cada critério, mostrar vínculos presentes, ausentes ou ambíguos nos documentos. Consolidar cobertura documental sem concluir implementado/não implementado. Ausência de registro é evidência documental insuficiente, não ausência de implementação.
- US4-AC03: Exibir declarações de validação/aceite como registros de terceiros, com fonte e versão; separar de consistência/gate calculados. IA não aprova requisito, teste, PR ou história. Ambiguidade gera pendência; não aplicável exige justificativa.
- US4-AC04: Manter o mesmo escopo documental em projetos C#, Razor ou outras linguagens. Referência a arquivo-fonte é texto não verificado; links fora dos documentos autorizados não abrem o destino. Informar análise de código como capacidade futura.

#### Cenários de aceite

| Cenário | Critério | Dado | Quando | Então |
|---|---|---|---|---|
| US4-S01 | US4-AC01 | Requisito com vínculos explícitos/inferidos | Abrir ficha | Diferenciar fontes e mostrar registros/sugestões |
| US4-S02 | US4-AC02 | Critério sem tarefa e outro ambíguo | Consultar critérios | Informar lacunas documentais sem classificar implementação |
| US4-S03 | US4-AC03 | Documento declara validação sem prova acessível | Consolidar | Mostrar declaração e limite, sem aprovação pelo SpecAudit |
| US4-S04 | US4-AC04 | Documentos de projetos em linguagens diferentes e link .cs | Navegar | Mesma análise documental; link .cs não lido |

#### Condições para reprodução pelo DEV

- Perfil/permissões: leitor restrito de documentos.
- Dados/estado inicial: requisito vinculado, sem tarefa, ambíguo e com referência a código.
- Integrações/acessos: agente somente para interpretação documental solicitada.
- Observação: fontes e estados documentais; acesso a código deve permanecer ausente.
- Restrições: nenhum resultado de implementação ou teste funcional é produzido.

### US5 — Selecionar e executar as duas verificações permitidas — Prioridade P1

**História**: Como responsável, quero selecionar speckit-analyze ou check_gate em uma combo e obter resultados sem alterar a solução.
**Justificativa da prioridade**: Delimita o que a aplicação pode executar.
**Dependências e estado inicial**: US1; agentes autenticados apenas para analyze; documentos/registro para o gate.
**Teste independente**: Usar cada opção, testar ausência de pré-requisitos, limites e cancelamento.

#### Critérios de aceite

- US5-AC01: Descobrir Codex, Antigravity, Claude Code e Kiro; localização manual só para agente desejado não encontrado. Mostrar versão/compatibilidade/limitações e seleção por projeto. Não exigir agente para check_gate.
- US5-AC02: Codex e Antigravity devem comprovar análise documental, captura de saída/erros, cancelamento e proteção efetiva de origem/escopo/persistência. Integração sem prova fica indisponível, sem bloquear leitor/gates. Não ampliar permissões para superar falha.
- US5-AC03: Combo fechada com exatamente speckit-analyze — Consistência documental e check_gate — Validar gates, visível em Verificações e na ficha do requisito. Mesma seleção por projeto entre essas telas. Sem campo de comando, terceira opção, cadeia de comandos ou execução automática ao selecionar. Executar só ao clicar Executar; exibir progresso, duração, resumo, saída, estado e ferramenta/versão.
- US5-AC04: analyze opera sobre documentos autorizados; não lê código nem executa scripts, hooks, plugins, instruções ou configurações da origem. Informar adaptação, escopo e possível envio ao serviço do agente. Conversas/caches internos só fora da origem; relatórios oficiais só por Salvar. Credenciais/serviços da solução não são utilizados. Pré-requisitos ausentes de spec/plan/tasks geram bloqueio, sem criar esses arquivos.
- US5-AC05: Falha, cancelamento, autenticação ausente e incompatibilidade não viram aprovação/reprovação do requisito. Encerrar descendentes pertencentes à execução ao cancelar. Leitor e gate continuam utilizáveis sem IA.
- US5-AC06: check_gate usa ferramenta CEMG fornecida/conferida pelo SpecAudit, sem executar o script do projeto. Selecionar funcionalidade, etapa, registro e revisão esperada; revisar a seleção antes de Executar. Validar estado/bloqueios/checks/hashes aplicáveis e apresentar resultado com limites: não autentica decisão humana nem testa implementação. A ficha mostra o alcance da funcionalidade/etapa, nunca finge gate individual do requisito. Sem registro ou revisão identificável, informar não verificável sem criá-los.
- US5-AC07: check_gate funciona localmente sem IA, rede ou hooks. Referência que exigiria leitura de código, configuração, arquivo fora da origem autorizada ou recurso externo bloqueia a verificação completa com motivo, sem leitura do destino e sem READY. Mudança dos documentos/registro/evidências pertinentes torna o resultado desatualizado. Escolher etapa não autoriza encadear comandos do fluxo SpecKit.

#### Cenários de aceite

| Cenário | Critério | Dado | Quando | Então |
|---|---|---|---|---|
| US5-S01 | US5-AC01 | Agente encontrado e outro ausente | Abrir seleção e escolher gate | Descoberta automática, fallback restrito; gate não pede agente |
| US5-S02 | US5-AC02 | Dois agentes reais e integração incompatível | Executar analyze | Demonstrar contrato ou bloquear apenas o adaptador com motivo |
| US5-S03 | US5-AC03 | Verificações e ficha abertas | Alternar combo, tentar terceira opção e clicar Executar | Só duas opções, seleção coerente; execução apenas por clique |
| US5-S04 | US5-AC04 | Documentos com instruções maliciosas e tasks ausente | Analisar | Não executar instruções/ler código; informar pré-requisito sem criar arquivos |
| US5-S05 | US5-AC05 | Agente sem autenticação ou em execução | Cancelar/consultar leitor e gate | Mostrar estado real, encerrar descendentes e manter funções locais |
| US5-S06 | US5-AC06 | Registros conhecidos com READY, BLOCKED, hash adulterado e revisão divergente | Selecionar etapa/registro/revisão e executar gate | Resultado correto e delimitado, sem inventar aprovação humana/implementação |
| US5-S07 | US5-AC07 | Rede desativada e evidências documentais, .cs, link externo e alteração concorrente | Executar gate | Documental funciona offline; demais bloqueiam ou invalidam sem ler destinos proibidos |

#### Condições para reprodução pelo DEV

- Perfil/permissões: usuário normal; integração documental restrita e ferramenta gate conferida.
- Dados/estado inicial: corpus seguro, duas instalações reais, registros válidos/inválidos e referências fora do escopo.
- Integrações/acessos: Codex/Antigravity somente para analyze; gate sem agente/rede.
- Observação: combo em ambas as telas, saída/erro/cancelamento, acessos reais e arquivos intactos; ausência de relatórios oficiais antes de Salvar.
- Restrições: não existe promessa de isolamento implementado; a prova do adaptador documental continua obrigatória antes de habilitá-lo.

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

### US7 — Consulta de vulnerabilidades da solução — ADIADA

A história US7 e seus antigos critérios US7-AC01 a US7-AC03 ficam reservados para versão futura. Não integram aceite, contagens ou PRs ativos da V1. A análise de dependências da solução excede a decisão de consultar somente documentos SpecKit e executar apenas analyze/check_gate. A obrigação de verificar/avisar vulnerabilidades do desenvolvimento do SpecAudit permanece constitucional. A especificação anterior está preservada em verification/archive/before-documentary-v1/spec.md.

### US8 — Navegar em temas escuro e claro — Prioridade P2

**História**: Como responsável, quero uma interface consistente mantendo o contexto do projeto.
**Justificativa da prioridade**: Materializa a experiência aprovada nos mockups.
**Dependências e estado inicial**: US1 e áreas disponíveis conforme incrementos.
**Teste independente**: Percorrer áreas e trocar tema durante leitura de resultado.

#### Critérios de aceite

- US8-AC01: Preservar a identidade/estrutura visual dos mockups registrados em design/mockups/README.md, aplicando obrigatoriamente suas adaptações de comportamento à V1. Oferecer Dashboard, Requisitos, Documentos, Verificações e Sugestões, com projeto/origem somente leitura/destino visíveis; azul, lilás, branco e cinza; tema escuro inicial e claro opcional.
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
- Sem internet: leitura local e histórico salvo funcionam; analyze remoto pode ficar indisponível; check_gate continua local.

## Requisitos funcionais

- FR-001: Carregar origem/destino e separar projetos/funcionalidades, coberto por US1-AC01 e US1-AC03.
- FR-002: Impedir escrita e detectar mudanças externas, coberto por US1-AC02 e US1-AC04.
- FR-003: Ler/pesquisar documentos em preview/fonte, coberto por US2-AC01 e US2-AC02.
- FR-004: Renderizar diagramas e controlar conteúdo ativo/remoto, coberto por US2-AC03 e US2-AC04.
- FR-005: Exibir indicadores com escopo/denominador/procedência, coberto por US3-AC01 e US3-AC02.
- FR-006: Identificar inconsistências, filtrar e atualizar estado documental, coberto por US3-AC03 e US3-AC04.
- FR-007: Consultar ficha com vínculos rastreáveis, coberto por US4-AC01.
- FR-008: Apresentar cobertura documental e declarações de validação com procedência, coberto por US4-AC02 e US4-AC03.
- FR-009: Oferecer suporte documental independente de linguagem, sem ler código, coberto por US4-AC04.
- FR-010: Descobrir agentes automaticamente com fallback manual restrito, coberto por US5-AC01.
- FR-011: Validar Codex/Antigravity e bloquear incompatíveis, coberto por US5-AC02.
- FR-012: Selecionar apenas analyze/check_gate na combo e executar por clique, coberto por US5-AC03 e US5-AC05; validar gates locais e seus limites por US5-AC06 e US5-AC07.
- FR-013: Restringir execução/rede/segredos/persistência dos agentes, coberto por US5-AC04.
- FR-014: Salvar/exportar apenas por ação explícita no destino externo, coberto por US6-AC01 e US6-AC02.
- FR-015: Revisar ocorrências com decisões justificadas/reavaliáveis, coberto por US6-AC03.
- FR-016: Comparar histórico salvo e excluir apenas por ação explícita, coberto por US6-AC04.
- FR-017: ADIADO com US7; consulta de vulnerabilidades da solução não pertence à V1.
- FR-018: Navegar pelas áreas nos dois temas, coberto por US8-AC01 e US8-AC02.

## Entidades principais

- Projeto: origem, destino externo, funcionalidades, escopo documental, verificação e agente selecionados.
- Funcionalidade: escopo SpecKit que contextualiza documentos e identificadores.
- Documento: caminho autorizado, tipo, conteúdo, referências e identificador de versão.
- Requisito/critério: comportamento especificado, vínculos/tarefas e declarações documentais; sem estado de implementação calculado.
- Evidência/vínculo: fonte, trecho, versão, procedência e natureza explícita/inferida.
- Análise/verificação: tipo, escopo, agente/ferramenta, início/fim, estado, saída e limitações.
- Ocorrência/sugestão: problema, gravidade justificada, evidências e proposta.
- Decisão humana: classificação, justificativa, responsável, data e contexto de validade.
- Registro salvo/exportação: conteúdo autorizado, formato, destino e metadados; não existe para análise descartada.
- Gate: etapa, registro, revisão esperada, ferramenta/versão, resultado, bloqueios, evidências e limites; sem decisão humana autenticada.

## Critérios de sucesso mensuráveis

- SC-001: Em todos os cenários de análise/cancelamento, zero arquivos da origem são criados, alterados, renomeados ou excluídos pelo aplicativo e seus descendentes.
- SC-002: Em todos os cenários sem Salvar, zero relatórios/decisões/sugestões oficiais do SpecAudit são persistidos; após Salvar, 100% desses artefatos ficam no destino externo autorizado. Conversas/caches internos dos agentes podem existir antes da ação, com 100% de seus locais fora da origem.
- SC-003: Nos conjuntos conhecidos, 100% dos indicadores correspondem às contagens esperadas, incluindo itens inconclusivos e escopos vazios.
- SC-004: Todas as conclusões da amostra apontam evidência verificável ou declaram insuficiência; nenhuma aprovação deriva somente de nome, tarefa marcada ou opinião do agente.
- SC-005: O usuário percorre as cinco áreas, consulta fontes e alterna modos/temas sem editar documentos ou perder seleção em todos os cenários previstos.
- SC-006: As duas integrações obrigatórias demonstram descoberta, execução real, resultado/erro e cancelamento com proteção ativa; 100% das instalações incompatíveis de teste são bloqueadas.
- SC-007: Todos os resultados salvos identificam projeto, escopo, versão, data, procedência e limitações; zero riscos aceitos são apresentados como teste aprovado.
- SC-008: A combo apresenta exatamente duas opções em ambas as telas; check_gate funciona sem agente/rede, detecta todos os hashes/revisões inválidos da amostra e não lê referências fora do escopo documental.

## Premissas, dependências e questões abertas

Fonte: decisão explícita de 2026-11-16, subordinada à constituição 3.0.0. O escopo atual substitui leitura de código por análise exclusivamente documental e duas verificações na combo. Leitura/comparação de código é futura. Salvar continua manual e externo.

Usuário previsto: responsável local; DEV reproduz e aceita a entrega. Colaboração multiusuário e hospedagem remota não integram o escopo aprovado.

Dependências de pesquisa no plan: proteção efetiva dos executores e compatibilidade real de Codex/Antigravity, incluindo estado interno exclusivamente externo e relatórios oficiais somente após Salvar; gate limitado a evidências documentais; invalidação por mudança, limites e cancelamento. Se uma dependência for inviável, apresentar bloqueio e obter decisão sem reduzir requisitos silenciosamente. Não há alegação de compatibilidade ou controle já implementado.

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
| US4 | Vínculos e cobertura documental | Sem avaliação de implementação | Conferir requisito e documentos |
| US5 | Combo fechada; gate local; adaptadores analyze/cancelamento | Origem intacta; sem código/hooks/shell; estado interno externo | Duas verificações delimitadas e integrações reais protegidas |
| US6 | Revisão; salvamento/exportação; histórico | Somente externo e por Salvar | Salvar, reabrir, comparar e revisar |
| US7 (futura) | Fora da entrega V1 | Nenhuma consulta de dependências da solução | Exige nova revisão |
| US8 | Navegação; temas | Contexto/estados legíveis | Cinco áreas nos dois temas |

O plan define PRs por escopo, complexidade e testabilidade, sem metas de linhas/contagem. Incrementos intermediários mantêm estado seguro, sem se apresentar como capacidade completa.

## Condição de saída da etapa spec

Aplicar PASSAGENS.md: critérios observáveis, cenários, escopo, dependências e decisões materiais resolvidas. A checklist é autorrevisão de requisitos, não teste funcional ou aceite humano. Registrar revisão e hashes em verification/; a próxima etapa deve revalidá-los. Nenhuma execução funcional é registrada nesta fase.

## Demonstração da história

Cada história descreve acionamento, observação e condições de reprodução. O plan deve concretizar inicialização, dados seguros, passos, asserções, evidências e limpeza para IA/DEV, incluindo agentes reais para analyze e check_gate local. Simulações ajudam a testar apresentação/erros, mas não comprovam integração real. Não declarar demonstração apenas por compilar, criar roteiro ou atingir breakpoint.

## Registro de mudança de escopo — 2026-11-16

Constituição 2.0.0 → 3.0.0 por autorização explícita. US3/US4 e FR-008/009 passam a cobertura documental; US5-AC03 define as duas opções e US5-AC06/07 acrescentam gates. US7/FR-017 ficam adiados, com IDs reservados. Não misturar evidências históricas de análise de código ou worker WFP com comprovação da V1. Código-fonte, C#/Razor aprofundado, converge e consulta de dependências da solução exigem versão futura. Nenhuma execução/implementação ou autorização elevada é concedida por esta revisão.
