# Evidências de validação — US3

Estado: APROVADA
Executor/ferramenta: Antigravity (Gemini 4 Neon) · dotnet SDK 10.0.404 / runtime .NET 10.0.15 / Playwright 1.64.1 / Microsoft Edge headless
Data/hora e fuso: 2026-11-19 23:16 America/Sao_Paulo
Commit: `517aea5e4547c02e69dcd12221d779b156d0c379`
Alterações não commitadas: nenhuma no código/testes; apenas novos artefatos desta evidência (`evidence/US3-validation.md`, `evidence/logs/T032/*`, `evidence/captures/T032/*`) e atualização de `tasks.md`.
Ambiente, versões e dados: Windows x64, host local `SpecAudit` em loopback (`http://127.0.0.1:<porta-efêmera>`), fixtures sintéticas criadas por `DocumentaryFixture.Create()` e `TempDocumentaryOrigin` sob `%LOCALAPPDATA%\Temp` (removidas ao final de cada teste), nenhum projeto real nem acesso à internet.

| Execução | Critério/cenário | Comando ou procedimento e diretório | Esperado | Observado | Resultado | Log/artefato |
|---|---|---|---|---|---|---|
| RUN-01 | US3-S01–S04 / US3-AC01–AC04 + invariante V1 (domínio e projeção via broker) | Raiz do repositório: `dotnet test tests/SpecAudit.Tests/SpecAudit.Tests.csproj --no-restore --filter "FullyQualifiedName~DocumentaryProgressTests" --logger "console;verbosity=normal"` | `30/40` = `75%` declarado, `12/20` = `60%` cobertura documental, vazio = `NoItems` (`Sem itens`), duplicidades mantidas no denominador (`11/21`), gates registrados com procedência, `DoNotRead.cs` nunca lido pelo broker, `Stale` por mudança e `31/40` após refresh sem agente/gate | 5/5 aprovados (exit code 0): `V1DomainForbidsCodeAnalysisAndImplementationScoreEntities`, `S01`, `S02`, `S03`, `S04` | PASSOU | [run-01-domain.txt](logs/T032/run-01-domain.txt) |
| RUN-02 | US3-S01–S04 / US3-AC01–AC04 (E2E real no host loopback + Edge headless + seleção nativa) | Raiz do repositório com `$env:TEMP = "$env:LOCALAPPDATA\Temp"` e `$env:SPECAUDIT_CAPTURE_DIR = "...\evidence\captures\T032"`: `dotnet test tests/SpecAudit.Ui.Tests/SpecAudit.Ui.Tests.csproj --no-restore --filter "FullyQualifiedName~DashboardTests" --logger "console;verbosity=normal"` | Cards `75%` (`30 / 40`), `60%` (`12 / 20`), `Cobertura documental`, ausência de `Implementação evidenciada` e `Validação aprovada`, `003-empty` exibindo `Sem itens` sem percentual, gates registrados com procedência, ocorrências com `DoNotRead.cs` `não verificada nesta versão`, atualização externa para `31 / 40` após clique em `Atualizar documentos`, origem preservando edição e pasta de relatórios vazia | 4/4 aprovados (exit code 0): `S01`, `S02`, `S03`, `S04` | PASSOU | [run-02-us3-e2e.txt](logs/T032/run-02-us3-e2e.txt) · [Demo](captures/T032/dashboard-demo.png) · [Escopo vazio](captures/T032/dashboard-empty.png) |
| RUN-03 | Regressão relacionada (fundação, ReaderBoundary, sessão/antiforgery, US1, US2, US3) | Raiz do repositório com `$env:TEMP = "$env:LOCALAPPDATA\Temp"`: `dotnet test SpecAudit.slnx --no-build --no-restore --filter 'FullyQualifiedName!~RealBrowserRendersLocalProjectSelectionWithoutCircuitErrors&FullyQualifiedName!~US1IntegratedHarnessTests' --logger "console;verbosity=normal"` | Nenhuma regressão em domínio, limites Windows ou UI | 84/84 aprovados (exit code 0): 16 `SpecAudit.Tests`, 16 `SpecAudit.Windows.Tests`, 52 `SpecAudit.Ui.Tests` | PASSOU | [run-03-regression.txt](logs/T032/run-03-regression.txt) |

## Comparação visual com o mockup aprovado

- Referência: [dashboard-requisito.png](../design/mockups/dashboard-requisito.png) e [README dos mockups](../design/mockups/README.md).
- Versão avaliada: `517aea5e4547c02e69dcd12221d779b156d0c379`, viewport 1440 × 1000, tema escuro (único tema disponível antes da US8/T044).
- Aspectos preservados: sidebar persistente com destaque azul em `Dashboard`, paleta `#11151F`/`#1B2230`/`#4F8CFF`/`#AA8CF5`, 3 cards de indicadores no topo com barras azul e lilás, chips de resumo, tabela principal à esquerda e painel lateral à direita.
- Substituições obrigatórias da V1 aplicadas:
  - `Implementação evidenciada` substituída por **Cobertura documental** com regra e denominador explícitos.
  - `Validação aprovada` substituída por **Registros de gate** / **Estados documentais e gates registrados** com procedência, revisão e aviso de que `READY` não comprova funcionamento ou aceite humano.
  - Botão `Analisar projeto` e evidências em código (`src/search.ts`) removidos; referências a `.cs` aparecem como **não verificada nesta versão** sem leitura do arquivo-fonte.
  - Escopo vazio (`003-empty`) exibe **Sem itens** sem percentual ou barra.

## Cobertura e limitações

- **Critérios cobertos**: US3-AC01 (`US3-S01`), US3-AC02 (`US3-S02`), US3-AC03 (`US3-S03`), US3-AC04 (`US3-S04`).
- **Integrações reais versus simuladas**: host Kestrel loopback, Blazor Server, seleção nativa de pastas Windows, `DocumentCatalog`/`DocumentBroker` via handles Windows somente leitura, edição real de `tasks.md` no filesystem e navegação em Edge headless são reais; nenhuma IA ou `check_gate` é invocada pelo Dashboard.
- **Cenários não executados**:
  - Comparação visual no tema claro: **NÃO EXECUTADO** nesta etapa porque o alternador de tema é entregue apenas na US8 (T044). Próximo passo: validar ambos os temas em T044/T045 e T089.
  - Cinco testes históricos da US1 excluídos do filtro de regressão porque gravam em caminhos históricos de evidências deste checkout (mesmo filtro de T021–T027).
- **Regressão executada**: 84/84 aprovados em RUN-03 (0 falhas).

## Rodadas após correções

- **Rodada T034 (2026-11-19 23:25 America/Sao_Paulo, sobre base `fdd5150`)**: após resolver `REV-US3-03` (preservação da última projeção válida marcada como `Freshness.Stale` tanto quando `HasPendingChanges == true` quanto quando o broker detecta edição antes do evento do watcher), `REV-US3-02` (reset de `selectedPhase`/`selectedStory` em `OnFeatureChanged`) e `REV-US3-01` (remoção do truncamento silencioso `.Take(25)` na tabela de requisitos), a primeira execução revelou a corrida broker-antes-do-watcher em `S04`; após tratá-la em `Dashboard.razor`, a reexecução completa aprovou **84/84 testes** (16 `SpecAudit.Tests`, 16 `SpecAudit.Windows.Tests`, 52 `SpecAudit.Ui.Tests`, exit code 0), incluindo filtro por `Gravidade` e preservação de `30 / 40` + `Desatualizado` antes do clique em `Atualizar documentos`. Log: [logs/T034/regression.txt](logs/T034/regression.txt).
  - SHA-256 dos arquivos alterados na T034:
    - `SpecAudit/Components/Pages/Dashboard.razor` — `f87f6f60c8e089558be645cb206d3c4101428171a87684e01608bb0e54e23695`
    - `tests/SpecAudit.Ui.Tests/DashboardTests.cs` — `d1259fabc35e6870b8ec8095a7bbe6fd89c94bbc91ac3408f6e45a8b30662441`

## Identificação do incremento e histórico

- PR previsto ou URL real: PR-B (local, ainda não aberto/integrado).
- História e critérios cobertos: US3 (US3-AC01–AC04, cobertura técnica completa no tema escuro).
- Base resolvida: `d2a34bc` (US2 tecnicamente concluída).
- Versão candidata: `517aea5e4547c02e69dcd12221d779b156d0c379`.
- Predecessores e ordem: PR-A (técnico concluído), US2 (T021–T028), T029–T031.
- Escopo desta evidência: `INCREMENTO` (US3 dentro do PR-B).

## Execução real do produto

- **Versão e ambiente**: commit `517aea5e4547c02e69dcd12221d779b156d0c379`, Windows x64, .NET 10.0.15, Edge headless, viewport 1440 × 1000.
- **Meio**: `UI` (navegador real sobre host loopback `SpecAudit.dll` com diálogos nativos Windows).
- **Inicialização e entrada exata**: `LocalHostFixture` inicia `SpecAudit.dll`, seleciona origem (`001-demo`, `002-other`, `003-empty`) e destino externo sintéticos, confirma carregamento e acessa `/dashboard` pelo link `Dashboard`.
- **Resultado esperado e observado**: todas as asserções de `US3-S01`–`US3-S04` passaram (RUN-01 5/5, RUN-02 4/4, RUN-03 84/84); origem preservou a edição externa em `S04` e permaneceu inalterada nos demais cenários; pasta de destino permaneceu vazia; nenhum arquivo `.cs` foi aberto.
- **Evidência**: [run-01-domain.txt](logs/T032/run-01-domain.txt), [run-02-us3-e2e.txt](logs/T032/run-02-us3-e2e.txt), [run-03-regression.txt](logs/T032/run-03-regression.txt), [dashboard-demo.png](captures/T032/dashboard-demo.png) e [dashboard-empty.png](captures/T032/dashboard-empty.png).
- **Limites**: tema claro pendente da US8/T044; aceite humano da US3 pendente de revisão (T033–T034) e execução/decisão do DEV (T035).

