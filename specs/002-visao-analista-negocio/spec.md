# Especificação: Visão do Analista de Negócio (Dashboard BA)

- **Feature**: `002-visao-analista-negocio`
- **Branch**: `main`
- **Status**: `Especificada e planejada (pronta para implementação)`
- **Referência visual aprovada**: `docs/mockups/dashboard-visao-negocio-ba.jpg`
- **Restrição permanente**: Origem estritamente somente leitura via `DocumentBroker`; nenhuma aprovação humana (`DEV` ou `PO`) pode ser marcada automaticamente pelo aplicativo; nenhuma exportação ocorre antes de ação explícita do usuário.

---

## Objetivo e Escopo

Oferecer ao **Analista de Negócio (BA)** uma visão gerencial clara no **Dashboard** do SpecAudit, complementar à **Visão Técnica**, permitindo acompanhar entregas por **História de Usuário (`USx`)**, **pendências humanas (Validação DEV e Aceite PO)**, **evolução entre auditorias salvas (Antes × Agora)**, **glossário contextual (`?`)** e **exportação de Resumo Executivo de 1 página** para stakeholders.

---

## Histórias de Usuário

### US1 — Visão por História de Usuário e Pendências Humanas (Prioridade P1)
Como Analista de Negócio, quero alternar o Dashboard entre **Visão Técnica** e **Visão de Negócio (BA)** para enxergar rapidamente quantas histórias estão prontas tecnicamente e quais ainda aguardam validação DEV ou aceite PO.

### US2 — Evolução entre Auditorias Salvas (Antes × Agora) (Prioridade P2)
Como Analista de Negócio, quero comparar o estado atual com um pacote salvo anteriormente no destino externo para acompanhar a evolução de progresso, impedimentos e gates `READY` ao longo do tempo.

### US3 — Glossário Contextual e Resumo Executivo Exportável (Prioridade P2)
Como Analista de Negócio, quero consultar explicações rápidas dos indicadores na própria tela (`? Glossário`) e exportar um **Resumo Executivo** limpo (HTML pronto para impressão/PDF) para compartilhar com stakeholders.

---

## Requisitos Funcionais

- **FR-001**: Permitir alternar no topo do Dashboard entre **Visão Técnica** e **Visão de Negócio (BA)** preservando o projeto ativo, a funcionalidade selecionada e o tema (Escuro/Claro).
- **FR-002**: Exibir 3 KPIs gerenciais na Visão de Negócio: **Histórias prontas** (`concluídas / total V1`), **Aguardando validação DEV** e **Aguardando aceite PO**, derivados exclusivamente de `spec.md`, `tasks.md`, `validation/*-dev.md` e `acceptance/*.md` via `DocumentBroker`.
- **FR-003**: Exibir a tabela **Entregas por História de Usuário (`US1`..`USn`)** com colunas `US`, `História`, `Tasks` (`n/d` + `%`), `Validação DEV` e `Aceite PO` (destacando histórias adiadas como `Adiada / Fora V1` e permitindo abrir o documento correspondente em `/documentos`).
- **FR-004**: Exibir o card **Evolução (Antes × Agora)** comparando o pacote salvo mais recente (ou selecionado pelo usuário) no destino externo contra o estado atual (delta de progresso declarado, impedimentos ativos e gates `READY`), sem inferir resolução quando a amostra estiver incompleta.
- **FR-005**: Disponibilizar o botão **`? Glossário`** no cabeçalho do Dashboard abrindo um painel/modal explicando de forma simples cada indicador (*Progresso declarado*, *Cobertura documental*, *READY*, *Validação DEV*, *Aceite PO* e *Geração*).
- **FR-006**: Disponibilizar o botão **Exportar Resumo Executivo** que gera um relatório executivo limpo (HTML autocontido pronto para impressão/PDF) somente após confirmação explícita do usuário no destino externo, sem jamais gravar na origem.
