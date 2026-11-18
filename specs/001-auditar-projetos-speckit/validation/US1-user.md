# Teste do usuário — tela de Projetos

Atualização de fluxo: Carregar projeto abre o painel. Clique Escolher projeto e confira a origem exibida; clique Escolher destino e confira o destino exibido; só então Confirmar carregamento. Em caso de recusa os caminhos permanecem visíveis. A sequência automática de duas janelas descrita em registros anteriores foi substituída. Validação integral pelo desktop ainda bloqueada; não solicitar novo reteste do DEV/usuário antes da conferência. Ver ../evidence/selection-flow-validation.md.

Este roteiro verifica se você consegue usar a tela de Projetos. Não exige terminal, programação ou leitura de logs. O DEV prepara o ambiente, abre a aplicação e fornece pastas fictícias. Não use seus projetos reais neste teste.

Situação deste roteiro: escrito; percurso completo ainda não executado por usuário. O seu relato não é preenchido pela IA. Este teste de uso complementa o teste técnico do DEV.

## Antes de começar — preparação pelo DEV

O DEV deve deixar a aplicação funcionando e abrir a tela Projetos no navegador. Deve entregar os caminhos abaixo, preenchidos a partir da pasta criada pelo preparador. Se isso não estiver pronto, peça a preparação ao DEV; não precisa executar comandos.

| Dado entregue pelo DEV | Caminho/endereço desta rodada |
|---|---|
| Aplicação | http://127.0.0.1:5187 — confirmar endereço com o DEV |
| Projeto A | |
| Relatórios A | |
| Projeto B | |
| Relatórios B | |
| Novo destino de relatórios A | |

A aplicação roda no computador local. O DEV identifica a versão testada e fica disponível para ajudar a localizar as pastas. A pasta Relatórios A já contém um arquivo fictício chamado historico-exemplo.txt; isso foi preparado antes do teste.

## Ações na tela

Faça um passo por vez. Se algo sair diferente, anote o ocorrido ou tire uma captura e avise o DEV; não precisa corrigir o problema sozinho.

| Passo | Faça isto | Você deve ver |
|---|---|---|
| 1 | Clique **Carregar projeto**. Use **Escolher projeto** para Projeto A; confira o caminho. Use **Escolher destino** para Relatórios A; confira ambos e clique **Confirmar carregamento**. | O projeto carregado, os dois caminhos e a indicação **Somente leitura**. |
| 2 | No campo **Funcionalidade**, escolha **001-exemplo**. Depois escolha **002-segunda**. | Na primeira, os três documentos aparecem como **Presente**. Na segunda, tasks.md aparece como **Ausente**. |
| 3 | Clique **Alterar destino** e escolha a pasta Projeto A como destino. | Uma mensagem pede destino externo. O destino anterior continua selecionado. |
| 4 | Clique **Alterar destino** e depois **Cancelar** no seletor. | O destino continua igual. |
| 5 | No painel **Carregar projeto**, escolha Projeto B em **Escolher projeto** e Relatórios B em **Escolher destino**; confira os caminhos e clique **Confirmar carregamento**. Alterne A/B no campo **Projeto**. | Cada projeto mostra seu próprio caminho e destino. |
| 6 | Volte a A e, em **Alterar destino**, escolha Novo destino de relatórios A. | O destino de A muda; o de B permanece igual. |
| 7 | Com ajuda do DEV, abra a pasta Relatórios A no Explorador. | historico-exemplo.txt continua no lugar original e com o mesmo conteúdo; não foi movido para o novo destino. |
| 8 | Peça ao DEV para acrescentar uma frase ao spec.md fictício do Projeto A pelo Bloco de Notas e salvar. Volte à aplicação. | Um aviso informa que os documentos foram alterados. Clique **Atualizar documentos**: o aviso deve desaparecer após a leitura. O DEV confirma que a frase permaneceu no arquivo. |
| 9 | Observe **Executar verificação** e **Salvar resultados**. | Estão desabilitados nesta versão. Esses recursos ainda serão entregues. |

Não é esperado navegar pelo conteúdo dos documentos nesta etapa. Durante o teste não devem surgir novos relatórios; o arquivo de histórico já existia antes. Ao terminar, avise o DEV para encerrar a aplicação e guardar o relato.

## Seu relato

Pode responder em texto simples, por exemplo: “Consegui carregar os dois projetos. No passo 6 apareceu esta mensagem...”. O DEV pode ajudar a registrar a sua resposta sem mudar o sentido.

| Informação | Preenchimento do usuário/DEV a partir do relato real |
|---|---|
| Nome ou identificação escolhida pelo participante | |
| Data e hora | |
| Versão testada — informada pelo DEV | |
| Passos realizados | |
| O que funcionou | |
| Problemas ou dúvidas, com passo correspondente | |
| Capturas, se houver, sem dados pessoais | |
| Resultado de uso: concluído / problemas encontrados / não concluído | |

Não ter problemas no teste de uso não aprova automaticamente o PR nem concede aceite técnico da história. O DEV registra separadamente a validação técnica e a decisão prevista no [roteiro do DEV](US1-dev.md).
