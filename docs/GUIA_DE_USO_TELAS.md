# Guia de Leitura das Telas — CEMG Spec

Este guia explica de forma prática **o que cada tela do CEMG Spec mostra**, **como interpretar os números e badges** e **quais ações você pode executar** em cada área.

> **Regra de ouro do CEMG Spec**: o aplicativo **nunca altera** o projeto auditado (origem estritamente somente leitura) e **nunca analisa código-fonte** (`.cs`, `.razor`, etc.). Tudo o que você vê vem exclusivamente dos documentos SpecKit (`constitution.md`, `spec.md`, `plan.md`, `tasks.md`, `verification/`, `validation/`, `acceptance/`, `evidence/`).

---

## 1. Projetos (`/projetos`)

É o ponto de entrada para abrir e alternar projetos.

- **Projetos recentes**: grid com os projetos já carregados anteriormente, mantendo **Origem** e **Destino externo** gravados. Clique em **Abrir** para voltar ao projeto com 1 clique ou **Remover** para tirar da lista.
- **Carregar projeto**: abre o fluxo de seleção de duas pastas:
  1. **Pasta do projeto (Origem)**: pasta raiz da solução SpecKit (somente leitura).
  2. **Pasta dos relatórios (Destino externo)**: pasta **fora** da origem onde os relatórios serão salvos quando você clicar em **Salvar resultados**.
- **Funcionalidades**: lista as pastas `specs/NNN-nome/` detectadas e indica se os 3 artefatos centrais (`spec.md`, `plan.md`, `tasks.md`) estão **Presente** ou **Ausente**.
- **Aviso de limite (3.000 documentos)**: se um projeto tiver mais de 3.000 arquivos em `specs/`, o CEMG Spec carrega os 3.000 mais essenciais (priorizando `spec/plan/tasks/verification`) e lista os arquivos deixados de fora.

---

## 2. Dashboard (`/dashboard`)

Visão executiva da saúde **documental** do projeto ou funcionalidade selecionada.

- **Filtros no topo**: permitem filtrar por **Funcionalidade**, **Fase**, **História**, **Gravidade** e **Situação**.
- **3 cartões de KPI**:
  1. **Progresso declarado (tarefas)**: razão `tarefas [x] / total de tarefas` em `tasks.md`. *Atenção*: mede o que foi **declarado** em `tasks.md`, não comprova implementação em código.
  2. **Cobertura documental**: razão `requisitos com vínculo explícito / total de requisitos` em `spec.md`. Se aparecer `0%`, significa que as tarefas em `tasks.md` citam histórias (`US1`, `US2`...) mas não citam explicitamente os códigos `FR-001`, `FR-002`...
  3. **Registros de gate**: contagem de estados (`READY`, `BLOCKED`, etc.) encontrados nos arquivos `verification/*.json`.
- **Requisitos no escopo**: lista cada `FR-xxx` encontrado em `spec.md` e mostra se ele tem vínculo documental explícito ou não.
- **Ocorrências documentais**: lista impedimentos, lacunas, inconsistências e referências a arquivos de código mencionadas nos documentos (com scroll interno próprio).
- **Registros de gate (coluna direita)**: mostra cada arquivo `.json` encontrado em `verification/` como **registro documental de terceiros** (o CEMG Spec exibe o que está gravado lá, mas não autentica aprovação humana).

---

## 3. Requisitos (`/requisitos`)

Rastreabilidade detalhada requisito por requisito (`FR-001`, `FR-002`...).

- **Coluna esquerda (`REQUISITOS`)**: clique em qualquer requisito para abrir sua ficha documental à direita.
- **Ficha documental (direita)**:
  - **Resumo**: mostra a descrição extraída de `spec.md` e a procedência (arquivo e linha).
  - **Vínculos documentais**: lista as tarefas e documentos ligados ao requisito.
    - Badge **Explícito**: a tarefa cita diretamente o código `FR-xxx`.
    - Badge **Infeirido / Ambíguo**: a tarefa pertence à mesma história (`USx`), mas não cita `FR-xxx` explicitamente.
  - **Declarações registradas (registro de terceiros)**: mostra os arquivos de gate associados à funcionalidade.
  - **Abas `Vínculos`, `Verificações`, `Sugestões`**: permitem navegar rapidamente pelos itens relacionados àquele requisito.

---

## 4. Documentos (`/documentos`)

Leitor seguro e somente leitura de todos os artefatos autorizados do catálogo.

- **Coluna esquerda (`ARTEFATOS DO CATÁLOGO`)**: árvore de pastas e arquivos (`.md`, `.json`, `.txt`, `.trx` e imagens de `design/`). Use o campo **Buscar nos documentos** no topo direito para filtrar.
- **Painel central (Leitor)**:
  - **Preview**: renderiza Markdown formatado (incluindo diagramas Mermaid).
  - **Fonte**: mostra o texto bruto do arquivo.
  - **Lado a lado**: exibe Preview e Fonte simultaneamente.
- **Coluna direita (`Neste documento` + `Procedência documental`)**: sumário clicável dos títulos do documento aberto e metadados (caminho relativo, categoria e geração do catálogo).

---

## 5. Verificações (`/verificacoes`)

Executa a verificação determinística e offline **`check_gate`** (`cemg-speckit-preset 1.0.0`) sem usar IA e sem alterar a origem.

- **Como usar**:
  1. Escolha a **Funcionalidade** e a **Etapa do gate** (`spec`, `plan`, `tasks`, `pr`, `acceptance`, `dev-impossibility`).
  2. Selecione o **Registro do gate** (`verification/*.json`) — se a funcionalidade não tiver arquivos `verification/*.json`, o CEMG Spec informa que não é verificável e não cria nenhum arquivo na origem.
  3. Confira a **Revisão esperada** (SHA-256 derivado automaticamente do artefato atual).
  4. Clique em **Executar verificação**: em menos de 1 segundo, o resultado aparece em **Última execução**:
     - **`READY`**: etapa íntegra e coerente com a revisão atual.
     - **`BLOCKED`**: lista objetivamente o que está faltando ou desatualizado.
     - **`DEV_IMPOSSIBILITY_RECORDED`**: confirma registro formal de impossibilidade DEV.

---

## 6. Sugestões (`/sugestoes`)

Consolida as ocorrências documentais, permite registrar decisões humanas e exportar relatórios oficiais para a pasta externa.

- **Ocorrências e decisões humanas**:
  - Para cada ocorrência, você pode classificar como **Pendente**, **Confirmado**, **Falso positivo**, **Risco aceito** ou **Adiado**.
  - Para **Falso positivo** e **Risco aceito**, os campos **Justificativa** e **Responsável** são obrigatórios.
  - A decisão humana nunca apaga o achado técnico original; tudo fica em memória até você salvar.
- **Salvar resultados**:
  - Abre o diálogo **Confirmar salvamento externo** mostrando projeto, atualidade (`Atual` / `DESATUALIZADO`), pasta de destino e checkboxes de formato (**Markdown**, **JSON**, **CSV seguro**).
  - Ao confirmar, cria uma nova pasta `CEMG Spec-<utc>-<uuid>` fora da origem com `manifest.json` gravado por último.
- **Histórico salvo**:
  - Lista apenas pacotes oficiais `CEMG Spec-*` do destino externo, verificando integridade SHA-256.
  - **Comparar com atual**: mostra achados `New` (novos), `Persistent` (persistentes) e `Resolved` (resolvidos).
  - **Excluir registro**: apaga somente a pasta do registro escolhido, sem tocar outros registros nem a origem.
