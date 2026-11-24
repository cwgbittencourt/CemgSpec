# Roteiro do DEV — testar a tela de Projetos

Atualização de fluxo: Carregar projeto abre o painel. Clique Escolher projeto e confira a origem exibida; clique Escolher destino e confira o destino exibido; só então Confirmar carregamento. Em caso de recusa os caminhos permanecem visíveis. A sequência automática de duas janelas descrita em registros anteriores foi substituída. Validação integral pelo desktop ainda bloqueada; não solicitar novo reteste do DEV/usuário antes da conferência. Ver ../evidence/selection-flow-validation.md.

**Público:** desenvolvedor responsável por preparar o ambiente e validar tecnicamente. Para uma pessoa sem conhecimento técnico, entregar o [roteiro do usuário comum](US1-user.md).

Você pode testar a interface manualmente ou repetir os testes automatizados. São atividades diferentes: a interface permite conferir o uso da tela; os testes automatizados também verificam hashes, fronteiras de leitura, versões e cancelamento que não ficam visíveis na tela.

**Estado:** testes automatizados conferidos pela IA; percurso manual completo ainda não declarado executado. Execução e aceite do DEV PENDENTES. Este roteiro não aprova nem integra PR.

## 1. Abrir a aplicação

Requisitos: Windows com desktop, PowerShell, SDK .NET10 compatível com global.json (ambiente validado:10.0.404) e navegador. Não precisa de agente, credencial ou projeto real.

Abra o PowerShell na pasta do repositório CemgSpec. Cole **esta única linha** e pressione Enter:

```powershell
& .\specs\001-auditar-projetos-speckit\validation\Start-US1Manual.ps1
```

O script prepara dois projetos fictícios novos na pasta temporária do Windows, mostra os caminhos e inicia o CemgSpec. As pastas são criadas pelo preparador do teste, não pelo aplicativo auditando uma origem. A cada execução os dados ficam em uma pasta nova; não há exclusão ou restauração automática. O preparador inclui um arquivo de histórico fictício em Relatorios-A e uma baseline de hashes fora das origens.

Aguarde a mensagem **Now listening on**, mantenha o terminal aberto e abra [a aplicação no navegador](http://127.0.0.1:5187). A tela deve mostrar **Projetos** e o botão **Carregar projeto**. Se aparecer erro de SDK, restore, política de execução ou porta ocupada, guarde a mensagem e informe o problema; não é necessário alterar políticas do computador ou encerrar outros processos.

O script inicia explicitamente em Development para carregar CSS e JavaScript do checkout; omitir esse ambiente com --no-launch-profile causava página sem estilos e sem Blazor. O script não abre o navegador automaticamente. Se quiser apenas preparar dados sem iniciar a aplicação, use o mesmo comando com `-PrepareOnly`. Se5187 estiver ocupada, use `-Port 5188` e abra o endereço exibido. Para iniciar diretamente sem preparar dados fictícios: `dotnet run --project CemgSpec/CemgSpec.csproj --no-launch-profile -- --environment Development --urls http://127.0.0.1:5187`.

Para alternar entre A e B já abertos, use a lista **Projeto**. **Carregar projeto** pede duas pastas em sequência: etapa1 origem, etapa2 relatórios. Reabrir a mesma origem reutiliza seu contexto, sem duplicá-la.

## 2. Testar a interface

Use os caminhos mostrados no terminal. A expressão “pasta do teste” abaixo é o caminho que começa com CemgSpec-Manual-, dentro de TEMP; abra-o no Explorador de Arquivos para localizar as pastas.

| Passo | O que fazer | O que deve acontecer |
|---|---|---|
| Carregar A — AC01 | Clique **Carregar projeto**, depois **Escolher projeto** e selecione **Projeto-A**. Confira o caminho; clique **Escolher destino** e selecione **Relatorios-A**. Confira ambos e clique **Confirmar carregamento**. | Cartões com origem/destino e selo **Somente leitura**. Duas funcionalidades encontradas. |
| Conferir documentos — AC01 | Em **Funcionalidade**, selecione **001-exemplo** e depois **002-segunda**. | Na primeira, spec.md, plan.md e tasks.md **Presente**. Na segunda, tasks.md **Ausente**; os outros presentes. |
| Recusar destino interno — AC02 | Clique **Alterar destino** e escolha a própria pasta **Projeto-A**. Repita escolhendo **Projeto-A/specs/001-exemplo**. | Mensagem pedindo destino externo. O cartão continua com Relatorios-A; nenhum novo relatório. Cancelar o seletor também deve manter o destino. |
| Separar projetos — AC03 | Abra o painel em **Carregar projeto**; use **Escolher projeto** para **Projeto-B**, **Escolher destino** para **Relatorios-B**, confira e **Confirmar carregamento**. Alterne A/B no campo **Projeto**. | A mostra seu destino e B mostra o dele. Não misturar origens nem funcionalidades entre projetos. |
| Trocar destino — AC03 | Volte a A. Em **Alterar destino**, escolha **Relatorios-A-novo**. | Só o destino de A muda. No Explorador, historico-exemplo.txt continua em Relatorios-A, com o mesmo conteúdo; não foi movido para a pasta nova. |
| Editar por fora — AC04, parte visível | No Bloco de Notas, abra Projeto-A/specs/001-exemplo/spec.md e acrescente uma frase. Salve e volte ao navegador. | Aparece aviso de alteração externa e botão **Atualizar documentos**. Clique nele: o aviso desaparece se a leitura concluir; sua frase permanece no arquivo. Não usar F5 como substituto dessa ação. |
| Confirmar limites da entrega | Observe **Executar verificação** e **Salvar resultados**. | Permanecem desabilitados nesta etapa. Não há preview dos documentos nem execução de agentes disponível nesta tela. |

A edição do passo AC04 é intencional e feita somente no projeto fictício. Não restaure o arquivo para esconder uma falha. As origens não devem sofrer outras mudanças. Relatorios-B e Relatorios-A-novo devem continuar vazias; Relatorios-A contém somente o histórico preparado no início.

A constituição faz parte do catálogo, mas ainda não tem uma lista própria nesta tela. Os testes automatizados verificam sua leitura/hash. A interface permite observar a alteração externa; a prova completa de AC04, com trabalho em andamento, cancelamento e snapshot imutável, está no teste automatizado S04. Junções, identidade de arquivos, limites e hashes também têm asserções automatizadas. Completar apenas a tabela manual não comprova esses detalhes internos.

Para encerrar, volte ao terminal que iniciou o aplicativo e pressione **Ctrl+C**. Os dados fictícios ficam disponíveis para inspeção. Cada nova execução do script cria outra pasta; não apaga a anterior.

## Preparar a sessão de um usuário comum

Execute Start-US1Manual.ps1, aguarde o host e abra o navegador. Preencha a ficha de caminhos do [roteiro do usuário](US1-user.md) com os dados exibidos no terminal, informe o commit e deixe a aplicação aberta. Acompanhe a edição externa do passo8 apenas nos dados fictícios; o usuário não precisa executar PowerShell nem editar arquivos técnicos. Registre o relato fielmente. O resultado de uso é separado da prova de fronteiras/hashes/cancelamento e do aceite técnico do DEV.

## 3. Repetir os testes automatizados

Esta opção não exige clicar na interface. No PowerShell aberto na raiz do repositório, você pode colar **todo o bloco abaixo de uma vez**. Ele executa em sequência e para se houver falha:

```powershell
$devRun = Join-Path ([IO.Path]::GetTempPath()) ('CemgSpec-US1-DEV-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $devRun -Force | Out-Null
git rev-parse HEAD
dotnet restore CemgSpec.slnx --locked-mode
if ($LASTEXITCODE -ne 0) { throw 'Restore falhou; preserve a mensagem.' }
dotnet test CemgSpec.slnx --no-restore --logger 'trx;LogFilePrefix=US1-full' --results-directory $devRun 2>&1 | Tee-Object (Join-Path $devRun 'full.txt')
if ($LASTEXITCODE -ne 0) { throw 'Teste falhou; preserve os resultados.' }
Write-Output "Resultados: $devRun"
```

Esperado na versão validada: **59 testes aprovados, zero falhos/ignorados**. Inclui os quatro cenários US1, sete regressões T015 e cinco T019; esses números não se somam ao total. No fim aparece a pasta dos resultados. TRX identifica os testes e as asserções; o log mostra o resumo. As janelas nativas do harness são controladas pelos testes; não é necessário interagir com elas.

| Cenário automatizado | Resultado conferido |
|---|---|
| S01 / AC01 | Duas funcionalidades e tasks ausente; catálogo e origem preservados, sem relatório novo. |
| S02 / AC02 | Destino igual, descendente e junção recusados; leitura fora do catálogo rejeitada. |
| S03 / AC03 | Projetos separados; caminho/hash do histórico preservados ao mudar destino. |
| S04 / AC04 | Edição durante trabalho documental causa Changed/cancelamento, preserva a edição e o snapshot anterior. |

O harness cria dados próprios, distintos dos dados do teste manual. Seus JSONs por cenário são gravados com GUID em evidence/logs/T013-20261117/harness/, caminho técnico legado; o script de teste manual não depende desses arquivos. Logs locais podem conter caminhos pessoais: antes de compartilhar ou gravar no Git, sanitizar e atualizar hashes. Não incluir credenciais ou dados reais. O teste de navegador usa uma captura técnica legada T011; conferir o diff antes de versionar uma nova rodada.

## Versão e evidências

Rodada corrente de abertura: [59 testes aprovados](../evidence/manual-project-opening-test-run.json) e [review](../evidence/manual-project-opening-review.json). As contagens57,6 e2 da T019 abaixo referem-se à rodada histórica; manutenção/probes não foram repetidos nesta correção.


Fonte validada: correção local de abertura sobre commit `0fa5428159cc57707617ed51b5b236ead0155e92`, manifesto de código `sha256:f6e296be248c77cb91dd07dbd4236c955755ff03ea63fd159278112bac008d58`. [Validação PR-A](../evidence/PR-A-validation.md), [review PR-A](../evidence/PR-A-review.md) e [execução T019](../evidence/T019-test-run.json):57/57 testes,6/6 manutenção e2/2 probes aprovados. O roteiro simplificado não altera produto/testes. Preparação de dados e sintaxe do script verificadas; os cliques manuais acima continuam com **CONFERENCIA_PARCIAL**, sem alegar que o DEV os executou.

[Descrição PR-A](../verification/prs/PR-A.md) preparada na T020. Aprovação humana, integração, execução do DEV e aceite permanecem pendentes. [Estado técnico/aceite](../acceptance/US1.md). Nada neste roteiro exige executar scripts ou código da solução auditada: os comandos são ferramentas do desenvolvimento do próprio CemgSpec.

## Como registrar sua conferência

Anote qual caminho executou (manual, automatizado ou ambos), commit/manifesto, data/hora, o que observou e eventuais erros. Use as tabelas abaixo. Se não executou um cenário completo, registre **NÃO EXECUTADO** ou explique a cobertura parcial. A IA pode ajudar a organizar seu relato; não marca execução ou aceite em seu nome. Resultados aprovados não substituem sua decisão explícita de aceite.

## Registro humano e aceite

### Registro da execução humana

Preenchimento reservado ao DEV. Os campos abaixo permanecem em branco: a existência do roteiro e os testes da IA não representam execução humana.

Após avaliar cada cenário, registrar o resultado como **PASSOU**, **FALHOU** ou **NÃO EXECUTADO**, descrevendo o observado ou o motivo da não execução. Informar o hash do commit efetivamente testado; se houver alterações locais, identificar também o diff/manifesto. Registrar data/hora com fuso e links para capturas ou logs em `../evidence/`.

| Cenário / critério | Resultado | Resultado observado / motivo da não execução | Evidências (links) | Commit testado / diff ou manifesto | Responsável | Data/hora e fuso | Defeito / observações |
|---|---|---|---|---|---|---|---|
| US1-S01 / US1-AC01 | | | | | | | |
| US1-S02 / US1-AC02 | | | | | | | |
| US1-S03 / US1-AC03 | | | | | | | |
| US1-S04 / US1-AC04 | | | | | | | |

### Retestes e novas rodadas

Preservar os registros anteriores. Em caso de correção ou mudança de versão, acrescentar uma linha por cenário reexecutado, identificando a rodada e a nova versão.

| Rodada | Cenário / critério | Resultado | Resultado observado / motivo da não execução | Evidências (links) | Commit testado / diff ou manifesto | Responsável | Data/hora e fuso | Defeito / observações |
|---|---|---|---|---|---|---|---|---|
| | | | | | | | | |

### Decisão de aceite

Preenchimento reservado ao DEV, após conferir os resultados e a versão avaliada. Selecionar explicitamente **ACEITO**, **REJEITADO** ou **ACEITO_COM_RESSALVAS** no campo Decisão. Campos em branco significam decisão pendente; resultados PASSOU na tabela não concedem aceite automaticamente.

| Campo | Registro do DEV |
|---|---|
| Decisão explícita | |
| Responsável pela decisão | |
| Data/hora e fuso | |
| Commit efetivamente validado / diff ou manifesto | |
| Rodada e referência à execução humana / evidências | |
| Justificativa e observações | |
| Escopo aceito ou rejeitado | |
| Ressalvas, critérios não atendidos e riscos, se aplicável | |
| Ações posteriores e responsáveis, se aplicável | |

Quando solicitado pelo DEV, a IA deverá conferir esta declaração contra a versão, os resultados e as evidências referenciadas e transcrever a decisão para `acceptance/US1.md`, preservando autoria, data, escopo e referência a esta seção. Havendo inconsistência ou informação essencial ausente, deverá apontar a pendência antes de registrar o aceite como concluído. A transcrição não altera os resultados dos testes nem equivale a aprovação de PR ou autorização de publicação.

Não há monitoramento automático deste arquivo. A IA não concede aceite por conta própria nem marca EXECUTADO_PELO_DEV sem referência à execução humana real. Se o DEV confirmar impossibilidade, usar o template CEMG próprio e decidir AGUARDAR, REJEITAR ou ACEITAR_COM_RESSALVAS, mantendo os resultados reais; eventual aceite com ressalvas deve ser declarado explicitamente acima.
