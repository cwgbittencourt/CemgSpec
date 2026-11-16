# SpecAudit Constitution

**Preset**: `cemg-speckit` **3.0.0**.
**Template**: `constitution-template`.
**Composição**: scaffold core + adendo do preset com estratégia `append`.
**Fonte do adendo**: `.specify/presets/cemg-speckit/templates/constitution-addendum.md`.
**Manifesto**: `.specify/presets/cemg-speckit/preset.yml`.
**Resolução**: `.specify/scripts/powershell/resolve-template.ps1`.
Estes metadados registram a procedência desta revisão; não comprovam controles implementados.

## Core Principles

### I. Preservação integral da solução analisada

O SpecAudit DEVE tratar a solução analisada como origem somente leitura. A aplicação,
os agentes e seus subprocessos NÃO DEVEM criar, modificar, renomear ou excluir qualquer
arquivo nessa origem, incluindo documentos SpecKit, código, configurações, metadados Git,
caches e temporários. Alterar e depois restaurar o original NÃO É um mecanismo permitido.
O fluxo NÃO DEVE depender de uma cópia isolada da solução.

A restrição DEVE ser imposta tecnicamente ao executor e aos processos descendentes,
sem depender apenas de prompts, atributos de pasta ou promessas do agente. O executor
NÃO DEVE elevar privilégios ou ampliar permissões para contornar incompatibilidade.
As permissões normais do usuário para editar sua solução NÃO DEVEM ser restringidas.
Mudanças externas durante uma análise DEVEM ser detectadas e ter seu impacto informado;
a aplicação NÃO DEVE sobrescrevê-las nem misturar versões silenciosamente.

### II. Resultados externos e persistência por decisão do usuário

Ao carregar um projeto, o usuário DEVE escolher um diretório externo para resultados.
A aplicação DEVE validar o destino real, incluindo links e junções, recusando destinos
que permitam gravar dentro da solução verificada. Todo artefato produzido DEVE permanecer
fora da origem; a configuração e o estado técnico necessários NÃO DEVEM ser gravados nela.

Resultados de análises, sugestões, exportações e histórico derivado SOMENTE DEVEM ser
persistidos mediante a ação explícita Salvar. Antes disso, DEVEM permanecer transitórios,
sem autosave, logs de conteúdo, sessões de agente ou caches que persistam esses resultados.
Integrações que exijam persistência inevitável de conteúdo antes de Salvar NÃO DEVEM ser
habilitadas nesse fluxo. Gravações técnicas indispensáveis, sem conteúdo de análise,
DEVEM ser delimitadas, externas à origem e documentadas no plano.

Salvar NÃO DEVE aplicar correções na solução. Histórico salvo NÃO DEVE ser sobrescrito
ou excluído automaticamente. Mudar o destino NÃO autoriza mover ou apagar o histórico anterior.

### III. Evidências e conclusões rastreáveis

Os artefatos SpecKit DEVEM definir o comportamento esperado; a leitura do código PODE
fornecer evidências da implementação. Cada conclusão DEVE identificar escopo, fontes,
localização e versão ou identificador dos conteúdos analisados, além de suas limitações.
Relações explícitas DEVEM ser diferenciadas de associações inferidas ou sugeridas.

Ausência de evidência NÃO DEVE ser apresentada automaticamente como ausência de implementação.
Nomes semelhantes, tarefas marcadas, testes existentes ou conclusões de IA isoladas NÃO
comprovam atendimento. Ambiguidades DEVEM gerar pendência, sem inventar requisitos.
Evidências afetadas por mudanças DEVEM ser reavaliadas; resultados antigos DEVEM ser
identificáveis como desatualizados, preservando contexto suficiente quando salvos.

### IV. Indicadores transparentes e validação honesta

Progresso declarado, implementação evidenciada e validação DEVEM ser avaliações distintas.
Os cálculos DEVEM exibir quantidade, denominador e escopo. Itens inconclusivos NÃO DEVEM
ser excluídos silenciosamente para elevar percentuais. Atendimento parcial DEVE permanecer
visível, sem atribuição de pesos subjetivos não acordados.

A avaliação DEVE partir dos critérios obrigatórios e consolidar por requisito. Evidência
completa de implementação NÃO equivale a validação funcional aprovada. Falha funcional,
bloqueio, execução malsucedida, falta de evidência e ausência de verificação DEVEM ser distintos.
Validações apenas registradas em documentos DEVEM indicar essa procedência e NÃO DEVEM
ser apresentadas como executadas ou comprovadas pelo SpecAudit.

O produto, no escopo atual, NÃO DEVE executar a solução analisada, seus testes ou builds.
A leitura do código NÃO constitui autorização para executá-lo. Código de saída zero de
uma ferramenta ou opinião do agente NÃO substitui verificação adequada do critério.

### V. Agentes substituíveis e compatibilidade comprovada

A aplicação DEVE descobrir automaticamente instalações conhecidas de agentes. Informar
um executável manualmente SOMENTE DEVE ser oferecido quando a descoberta não localizar
o agente desejado. Encontrar a instalação NÃO significa comprovar compatibilidade.

Cada integração DEVE demonstrar automação, captura de resultados e erros, cancelamento e
respeito às restrições de acesso, incluindo subprocessos e estado de sessão. Limitações
DEVEM ser explícitas. Uma integração sem proteção comprovada DEVE ficar indisponível.
Verificações determinísticas DEVEM funcionar sem IA. Fluxos adaptados do SpecKit DEVEM
ser identificados como adaptações; NÃO DEVEM executar etapas que alterem os artefatos originais.

### VI. Segurança, privacidade e conteúdo não confiável

O produto DEVE operar com acesso mínimo. Documentos, código e configurações encontrados
na origem DEVEM ser tratados como dados não confiáveis, sem autorizar execução automática
de scripts, plugins, instruções embutidas ou configurações de agente do projeto.

Acesso de rede DEVE ser limitado às finalidades autorizadas: agente selecionado e consulta
de vulnerabilidades. O produto NÃO DEVE acessar bancos, APIs ou serviços da solução
analisada no escopo atual. Credenciais e conexões encontradas na origem NÃO DEVEM ser
utilizadas automaticamente. Autenticação existente do agente só PODE ser utilizada pela
integração validada, sem copiar segredos para relatórios.

Possível envio de conteúdo pelo agente a serviços externos DEVE ser informado; execução
local do aplicativo NÃO garante inferência local. Segredos e dados pessoais desnecessários
NÃO DEVEM integrar evidências, logs ou solicitações deliberadamente. Filtros NÃO DEVEM
ser apresentados como garantia de identificação de todo segredo.

O preview DEVE sanitizar conteúdo, bloquear scripts e recursos ativos e funcionar com
bibliotecas locais. Recursos remotos NÃO DEVEM carregar automaticamente; carregamento
excepcional exige ação explícita, sem reutilizar credenciais do agente ou da origem.

### VII. Vulnerabilidades conhecidas e manutenção segura

O desenvolvimento e a manutenção do SpecAudit DEVEM verificar vulnerabilidades conhecidas
das dependências diretas e transitivas e dos componentes utilizados, nas verificações
aplicáveis a mudanças e entregas. Achados DEVEM ser comunicados com origem, versão,
gravidade, data e correção conhecida, quando disponíveis. Componentes estáveis e suportados
DEVEM receber correções de segurança sob validação apropriada.

Falha de consulta DEVE ser informada como indisponibilidade, nunca como ausência de
vulnerabilidades. Resultado sem achados NÃO comprova ausência de todas as vulnerabilidades.
No produto, consultas autorizadas DEVEM identificar escopo, fonte e atualidade, sem instalar
atualizações ou corrigir dependências automaticamente e sem executar a solução analisada.

### VIII. Decisões humanas preservadas

Falso positivo e risco aceito DEVEM permanecer distintos e exigir justificativa atribuível.
Decisões DEVEM ser vinculadas à ocorrência e ao contexto, preservadas no destino externo
quando salvas e reavaliadas quando a evidência relevante mudar. Aceitar um risco NÃO
transforma falha em aprovação; descartar falso positivo NÃO comprova implementação.

A IA NÃO DEVE conceder aprovação de PR, aceite de história, confirmação de impossibilidade
ou autorização de publicação em nome do responsável humano. Decisões humanas e resultados
técnicos DEVEM permanecer separados e rastreáveis.

### IX. Validação real do desenvolvimento do SpecAudit

Cada história de desenvolvimento do SpecAudit DEVE ter critérios observáveis, plano de
validação da IA, revisão e roteiro reproduzível pelo DEV. A IA DEVE executar o comportamento
entregue pelo meio real de uso e comparar resultados antes de declarar disponibilidade
funcional. Compilação, testes apenas escritos, mocks ou breakpoint isolado NÃO substituem
a comprovação do fluxo integrado. Testes adequados existentes DEVEM ser reutilizados.

A evidência DEVE identificar versão/base, ambiente, entrada, procedimento, resultado
esperado e observado, efeitos e limitações. Mudanças relevantes exigem renovar evidências.
O DEV DEVE reproduzir o fluxo e registrar aceite na versão avaliada, ou confirmar uma
impossibilidade e decidir explicitamente AGUARDAR, REJEITAR ou ACEITAR_COM_RESSALVAS.
Sem execução possível, manter NÃO EXECUTADO, causa, impacto e próximo passo; a IA PODE
rascunhar o registro, mas NÃO confirmar nem aceitar pelo DEV.

Este princípio rege o desenvolvimento do próprio SpecAudit. NÃO autoriza o produto a
executar ou modificar soluções de terceiros carregadas para análise.

## Restrições de plataforma e segurança

A plataforma inicial DEVE ser Windows, com execução local e interface no navegador.
A aplicação DEVE usar Blazor, ASP.NET Core 10 com patch estável atualizado e C# estável
suportado para esse alvo, sem recursos de linguagem em preview. A análise inicial
aprofundada DEVE contemplar C# e Razor; detalhes de cobertura pertencem à especificação.

O executor DEVE ser separado e restrito pelo sistema operacional, com acesso de leitura
à origem e escrita apenas nas áreas externas autorizadas. O mecanismo exato DEVE ser
validado no planejamento, sem alegar proteção instalada apenas por existir esta regra.

Telas, paleta, formatos, frequências, estados de interface, critérios detalhados de análise
e integrações de entrega DEVEM ser definidos na especificação. Esta constituição NÃO
substitui critérios de aceite funcionais nem comprovação de compatibilidade dos agentes.

## Processo de desenvolvimento e revisão

As políticas instaladas em `.specify/presets/cemg-speckit/policies/QUALIDADE.md`,
`POLITICA-PRS.md`, `PASSAGENS.md` e `EXECUCAO-REPRODUZIVEL.md` DEVEM orientar o
processo de desenvolvimento, conforme o adendo incorporado abaixo.

O plan DEVE dividir PRs por objetivo, escopo, complexidade e possibilidade de teste,
sem metas de linhas ou duração. Cada incremento DEVE preservar estado seguro, possuir
verificação própria e declarar base e dependências. Código, testes e evidências pertinentes
DEVEM acompanhar o incremento. Autorrevisão DEVE ser declarada como tal.

Integração normal exige verificações aplicáveis executadas, review atual, ausência de
impeditivos, dependências satisfeitas e aprovação humana explícita por PR. Merge NÃO
substitui aceite funcional ou deploy. Mudanças de base exigem reavaliação dos checks afetados.

Pré-condições DEVEM ser reavaliadas em cada passagem de etapa. Decisões materiais pendentes,
hooks obrigatórios não concluídos ou evidências exigidas ausentes bloqueiam a etapa dependente.
Exceções humanas DEVEM registrar condição, escopo, risco, responsável, versão e ação posterior,
sem falsificar estados de teste ou dispensar silenciosamente as garantias desta constituição.
Documentação pura exige verificações adequadas ao seu escopo, sem alegar produto demonstrado.

## Governance

Esta constituição prevalece sobre decisões técnicas e artefatos derivados do SpecAudit.
Conflitos DEVEM ser apresentados ao responsável humano antes de prosseguir com trabalho
dependente; nenhuma ferramenta PODE reescrever silenciosamente princípios para declarar conformidade.

Emendas exigem proposta explícita, justificativa, avaliação de impacto, decisão humana e
atualização dos artefatos afetados. Aplicar versionamento semântico: MAJOR para remoção
ou redefinição incompatível de princípios; MINOR para novos princípios ou expansão material;
PATCH para esclarecimentos sem mudança de obrigação. Preservar a data original de ratificação
e atualizar a data de emenda. A versão 1.0.0 representa a primeira adoção deste projeto.

Spec, plan, tasks e reviews DEVEM verificar conformidade e registrar limitações reais.
Nenhuma disposição declara CI, proteção de branch, isolamento ou autenticação de aceite
instalados: tais controles exigem implementação e verificação próprias. Os detalhes funcionais
acordados na conversa DEVEM ser transpostos à especificação em etapa posterior, sem criação
ou implementação de funcionalidades nesta etapa constitucional.

**Version**: 1.0.1 | **Ratified**: 2026-11-15 | **Last Amended**: 2026-11-15

# Adendo à constituição — qualidade por história

Cada história de usuário deve ter critérios de aceite identificados, validação
executável, revisão do código e roteiro reproduzível para o DEV.

1. A IA deve executar as validações planejadas que suas ferramentas e acessos
   permitirem. Criar testes ou ler código não equivale a executá-los.
2. Toda execução deve identificar versão do código, ambiente, procedimento,
   resultado esperado, resultado observado e evidência verificável.
3. Não execuções, limitações e falhas devem ser explícitas. Não declarar como
   aprovado o que não foi executado. Bloqueios exigem causa e próximo passo.
4. Testes devem verificar comportamento e riscos relevantes. Reutilizar cobertura
   adequada. Não impor novos testes triviais para toda task documental ou técnica.
5. Mocks comprovam o comportamento sob simulação; não comprovam uma integração real.
6. A revisão deve comparar o diff com requisitos e analisar regressões. Registrar
   autoria e modalidade; autorrevisão não é revisão independente.
7. Alterações posteriores invalidam as evidências afetadas, que devem ser renovadas.
8. Sem validação e revisão concluídas, a história não está disponível para aceite
   normal. Exceções só podem ser decididas pelo DEV, com risco e escopo registrados.
9. Implementação concluída não significa história aceita. A IA não pode preencher
   uma decisão humana sem instrução humana explícita e atribuível.
10. Evidências não devem expor segredos nem dados pessoais desnecessários.

Templates instruem o processo. A imposição técnica depende de ferramentas e
políticas adicionais do repositório, que devem ser declaradas separadamente.

## Entrega em PRs revisáveis

Aplicar `.specify/presets/cemg-speckit/policies/POLITICA-PRS.md`: planejar a divisão no plan, mapear tasks, validar e
revisar cada incremento. Exigir aprovação humana por PR no fluxo normal e
preservar o estado seguro dos incrementos intermediários. Registrar dependências,
bases e novas rodadas após alterações. Não conceder aceite de história por merge.

## Condições de passagem

Adotar policies/PASSAGENS.md do preset instalado: pré-condições reavaliadas,
perguntas sobre decisões materiais, bloqueios explícitos e exceções delimitadas.
Não conceder prontidão por limite de perguntas, aviso ou inferência de aceite.


## Princípio obrigatório de validação por história

Para cada história de usuário, o planejamento deve definir como a IA executará
o comportamento entregue, como seu resultado será observado e como o DEV
reproduzirá essa execução. Isso vale com ou sem interface gráfica.

1. Antes de declarar o incremento funcional disponível, a IA deve iniciar o
   programa/serviço ou harness apropriado, acionar o comportamento pelo meio real
   de uso e comparar dados, resposta e efeitos ao resultado esperado.
2. A IA deve entregar roteiro reproduzível com versão, ambiente, inicialização,
   acesso seguro, dados, entrada, passos, resultados esperados, observação e limpeza.
3. O DEV deve executar o programa ou fluxo pelo meio adequado, podendo depurar,
   conferir os resultados e registrar sua decisão de aceite na versão avaliada.
4. Teste unitário, compilação, arquivo de roteiro ou breakpoint isolado não
   substituem comprovação do comportamento. Automação que exercite o fluxo real
   e suas asserções é válida; declarar limites de mocks e simulações.
5. Se a IA não puder executar, preservar a comprovação pendente, explicar o motivo
   e preparar as condições para o DEV. Se o teste for impossível para o DEV, ele
   confirma o registro, identifica cenários, versão, tentativas, risco e evidências
   e decide AGUARDAR, REJEITAR ou ACEITAR_COM_RESSALVAS, com ação posterior.
6. A IA pode preparar o rascunho da impossibilidade, nunca confirmar ou aceitar
   pelo DEV. Registro de impossibilidade não equivale a teste aprovado, aceite
   automático nem autorização de integração. Preservar os resultados reais.

Aplicar o princípio a spec, plan, tasks, execução, review, PR e aceite. Manter
decisões humanas separadas dos resultados da IA. Sua adoção exige incorporação
explícita à constituição do projeto, sem sobrescrever silenciosamente princípios.
