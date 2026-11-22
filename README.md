# SpecAudit — Auditoria Documental e Governança SpecKit

**Autor:** Carlos Wagner G. Bittencourt  
**Plataforma:** Windows 10 / 11 (x64) · .NET 10 (Self-Contained)  
**Licença:** [MIT License](LICENSE) — `Copyright (c) 2026 Carlos Wagner G. Bittencourt` (Repositório Privado)

---

## Visão Geral

O **SpecAudit** é uma aplicação desktop/local para **Windows x64** voltada à auditoria documental, rastreabilidade de requisitos (`FR` / `NFR` / `RF` / `RNF`) e acompanhamento executivo de projetos estruturados com **SpecKit (preset CEMG 3.0.0)**.

O sistema opera sob uma arquitetura **estritamente somente leitura (*Read-Only Origin*)**:
- **Zero escrita no projeto auditado:** Todos os arquivos da solução analisada (`.specify/` e `specs/`) são abertos exclusivamente com permissão de leitura e validação criptográfica `SHA-256` contra alterações concorrentes (*TOCTOU*).
- **Gravação isolada em pasta externa:** Relatórios técnicos, pareceres humanos e resumos executivos só são gravados em um diretório externo explicitamente escolhido pelo usuário, fora da árvore do projeto auditado.
- **Dupla perspectiva de governança:**
  - **Visão Técnica:** Conformidade de artefatos (`constitution.md`, `spec.md`, `plan.md`, `tasks.md`), gates formais (`READY` / `BLOCKED`), matriz de cobertura e ocorrências com navegação direta para a linha exata do documento.
  - **Visão de Negócio (BA / PO / Diretoria):** Indicadores executivos (KPIs), entregas por História de Usuário (`US1..USn`), homologações humanas (**Validação DEV**, **Validação Usuário / UAT** e **Aceite PO**), comparativo de evolução (*Antes × Agora*) e exportação de Resumo Executivo.

---

## Como Instalar e Executar no Windows

### Opção 1 — Instalador Gráfico Único (`SpecAudit-Setup.exe`) *(Recomendado)*

O SpecAudit possui um instalador gráfico independente de arquivo único (**não exige privilégios de Administrador** e **não requer .NET pré-instalado** na máquina de destino):

1. Baixe ou execute diretamente o instalador publicado no repositório em **[`installer/SpecAudit-Setup.exe`](installer/SpecAudit-Setup.exe)** (também disponível na seção **Releases (`v1.0.0`)** do GitHub).
2. Na janela do instalador, confirme as opções:
   - *Criar atalho com ícone SA na Área de Trabalho e no Menu Iniciar*
   - *Iniciar o SpecAudit em segundo plano (ícone SA perto do relógio) e abrir navegador*
3. Clique em **Instalar agora**.
4. O programa será instalado em `%LOCALAPPDATA%\Programs\SpecAudit` e abrirá automaticamente no navegador padrão no endereço local `http://127.0.0.1:5187`.

#### Execução Silenciosa em Segundo Plano (Sem Janela de Prompt)
- Ao abrir o SpecAudit pelo atalho da Área de Trabalho ou do Menu Iniciar, **nenhuma janela preta de prompt de comando permanece aberta**.
- O servidor local roda de forma invisível em segundo plano e exibe o ícone **`SA ✓` na bandeja do sistema (perto do relógio do Windows)**.
- Clique com o **botão direito** no ícone **`SA ✓`** perto do relógio para:
  - **Abrir SpecAudit no navegador (`127.0.0.1:5187`)** *(ou dê duplo-clique no ícone)*
  - **Encerrar SpecAudit**

#### Como Desinstalar no Windows
Você pode remover o SpecAudit a qualquer momento por qualquer um dos 3 caminhos abaixo (seus projetos e relatórios salvos não são afetados):
1. **Configurações do Windows:** Abra *Configurações → Aplicativos → Aplicativos instalados*, localize **SpecAudit**, clique em `...` e selecione **Desinstalar**.
2. **Menu Iniciar:** Abra *Menu Iniciar → Todos os aplicativos → SpecAudit → Desinstalar SpecAudit*.
3. **Pelo próprio Instalador (`SpecAudit-Setup.exe`):** Abra o `SpecAudit-Setup.exe` e clique no botão vermelho **Desinstalar versão atual**.

---

### Opção 2 — Gerar o Instalador ou Executar a partir do Código-Fonte

Caso esteja com o repositório clonado em uma máquina de desenvolvimento com o **.NET 10 SDK** no Windows x64:

- **Para compilar e gerar o instalador único (`installer/dist/SpecAudit-Setup.exe`):**
  ```powershell
  powershell -ExecutionPolicy Bypass -File .\installer\Build-Installer.ps1
  ```
  *(Adicione o parâmetro `-InstallLocally` caso deseje atualizar imediatamente a instalação local da máquina).*

- **Para executar diretamente via CLI do .NET:**
  ```powershell
  dotnet run --project .\SpecAudit\SpecAudit.csproj -- --open-browser
  ```

---

## Como Utilizar o Programa (Fluxo Passo a Passo)

### 1. Carregar um Projeto (`Projetos`)
1. Na tela inicial **Projetos**, clique no botão **Carregar projeto**.
2. **1. Pasta do projeto:** Clique em **Escolher projeto** e selecione a pasta-raiz da solução SpecKit que deseja auditar (pasta que contém `.specify` e `specs`).
3. **2. Pasta dos relatórios:** Clique em **Escolher destino** e selecione uma pasta **externa** ao projeto (onde relatórios e pareceres poderão ser salvos).
4. Clique em **Confirmar carregamento**.
   - *Dica:* Os últimos projetos abertos ficam registrados na grade **Projetos recentes**, permitindo reabri-los com apenas 1 clique no botão **Abrir**.

### 2. Auditar Indicadores e Entregas (`Dashboard`)
Acesse **Dashboard** no menu lateral e alterne entre as duas visões no topo da tela:

- **Visão Técnica:**
  - Exibe os cartões de progresso documental (**Requisitos cobertos**, **Tarefas concluídas**, **Documentos esperados** e **Ocorrências abertas**).
  - Clique em qualquer cartão de resumo para filtrar imediatamente a lista de ocorrências documentais.
  - Filtre por **Fase $\rightarrow$ História** (em cascata), **Gravidade** e **Situação**, ou clique no caminho do arquivo (ex.: `spec.md · linha 42`) para abrir o documento diretamente na linha destacada.

- **Visão de Negócio (BA):**
  - Exibe os 4 cartões executivos: **Prontidão da Funcionalidade**, **Histórias Prontas para Aceite**, **Aguardando validação Usuário** e **Pendências Críticas**.
  - Na tabela **Entregas por História de Usuário**, acompanhe para cada história (`US1`, `US2`, ...):
    - **Progresso Técnico** e **Testes/Evidências**
    - **Validação DEV** (`validation/USx-dev.md`)
    - **Validação Usuário** (`validation/USx-user.md` ou `validation/USx-uat.md`)
    - **Aceite PO** (`acceptance/USx.md`)
    - **Status Geral** (*Concluída*, *Em Homologação*, *Em Construção*, *Impedida*)
  - Passe o mouse sobre qualquer selo de validação para ver o caminho exato do arquivo esperado, ou clique sobre o selo para abrir o documento correspondente (com botão **`← Voltar para Visão de Negócio (BA)`** no leitor).
  - Clique em **`? Glossário`** para consultar as regras e palavras-chave que aprovam cada etapa (`Decisão: ACEITO`, `Resultado: APROVADO`, etc.).
  - Clique em **`Exportar Resumo Executivo`** para gravar o relatório executivo em Markdown na pasta externa de relatórios.

### 3. Inspecionar Rastreabilidade (`Requisitos`)
1. Em **Requisitos**, selecione um requisito funcional ou não funcional na coluna esquerda (`FR-001`, `RF-001`, `NFR-001`, `RNF-001`).
2. Na aba **Resumo**, confira a cobertura documental e as ocorrências daquele requisito.
3. Na aba **Vínculos**, navegue pelos trechos vinculados na especificação, no plano técnico, na lista de tarefas e nas evidências.

### 4. Ler Artefatos com Segurança (`Documentos`)
1. Em **Documentos**, navegue pela árvore de arquivos do projeto (`.specify/memory/constitution.md` e `specs/<funcionalidade>/...`).
2. Alterne entre os modos **Visualização** (Markdown renderizado com tabelas e blocos de código), **Código-fonte** (texto numerado linha a linha) ou **Dividido** (lado a lado).

### 5. Registrar Pareceres e Exportar Relatórios (`Sugestões`)
1. Em **Sugestões**, registre decisões humanas sobre cada ocorrência (*Aceita*, *Rejeitada*, *Adiada* ou *Resolvida*) com justificativa.
2. Clique em **Salvar relatório externo...** para exportar o pacote auditável (`report.md`, `report.json`, `findings.csv`, `indicators.csv` e `manifest.json` com hashes `SHA-256`) na pasta externa de relatórios.

---

## Documentação Complementar no Repositório

- [Guia Completo de Uso das Telas (Markdown)](docs/GUIA_DE_USO_TELAS.md)
- [Apresentação Executiva Visual (HTML Interativo Retina 2x)](docs/apresentacao-specaudit.html)
- [Apresentação Executiva para Diretoria / Board (PDF Paisagem A4)](docs/apresentacao-specaudit.pdf)

---

## Licença, Direitos Autorais e Isenção de Responsabilidade ([MIT License](LICENSE))

> **Licença MIT — Copyright (c) 2026 Carlos Wagner G. Bittencourt**
>
> 1. **Preservação Obrigatória dos Direitos Autorais:** Todos os direitos autorais e de autoria original deste software pertencem a **Carlos Wagner G. Bittencourt**. Sob os termos da [Licença MIT](LICENSE), é **condição obrigatória** que o aviso de direitos autorais (`Copyright (c) 2026 Carlos Wagner G. Bittencourt`) e o aviso de permissão sejam mantidos em todas as cópias ou partes substanciais do software.
> 2. **Permissão de Uso ("Como Está" / *As Is*):** O software é fornecido **"COMO ESTÁ" (*"AS IS"*)**, sem garantia de qualquer tipo, expressa ou implícita, incluindo, mas não se limitando às garantias de comercialização, adequação a uma finalidade específica e não violação.
> 3. **Isenção Total de Responsabilidade:** Em nenhuma hipótese o autor ou detentor dos direitos autorais será responsável por qualquer reivindicação, danos diretos, indiretos, incidentais ou qualquer outra responsabilidade (incluindo, sem limitação, danos ao computador, perda de dados, falhas de hardware/software ou interrupção operacional) decorrentes da instalação, uso ou incapacidade de uso deste programa. O risco integral quanto à utilização do software cabe exclusivamente ao usuário.
