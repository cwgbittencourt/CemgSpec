# Resultado da única prova elevada autorizada

Data local: 2026-11-16. Manifesto autorizado e preservado: d0fd1c46893c69d51dd2aba717834b8aed30a09423ff4b53b9bb5fc458eec115.
Autorização: uma execução elevada exclusivamente sintética, sem serviço, firewall global ou solução real. Execução realizada uma vez; não houve repetição automática. Esta autorização está consumida.

## Resultado atual

| Verificação | Resultado | Evidência |
|---|---|---|
| Manifesto e binários correspondem ao autorizado | PASSOU | Preflight e manifesto inalterado após execução |
| Instalar os três filtros WFP limitados ao SID próprio | PASSOU | Commit concluído antes da criação/retomada do worker; três chaves no relatório |
| Rollback de transação com falha antes de commit | PASSOU | rollbackPassed=true; nenhum worker iniciado nessa fase e limpeza verificada |
| Processo-controle fora do SID permanece conectado | PASSOU | controlUnaffectedDuringPolicy=true |
| Worker sem grupo Administradores habilitado | PASSOU | TokenIsAppContainer=1; AdminGroupEnabled=false |
| Inicialização do PowerShell restrito | FALHOU | Exit -1073741502, 0xC0000142; stdout vazio |
| Leitura por broker e tentativas de escrita nessa execução | NÃO EXECUTADO | Worker não chegou a emitir asserções; não substituir por resultados de rodadas antigas |
| TCP permitido e TCP IPv4/IPv6/UDP negados | NÃO EXECUTADO | Contagens zero sem worker ativo não comprovam bloqueio |
| Origem sintética preservada | PASSOU, observação | sourceIntact=true e conteúdo rechecado posteriormente |
| Encerramento e limpeza | PASSOU no escopo observado | Job sem processos ativos; perfil temporário removido com retorno 0; filtros/subcamada ausentes |
| Bloqueio de rede final e integração dos agentes | PENDENTE | Prova integral incompleta; nenhum agente real foi executado |

## Diagnóstico, sem nova prova

O código 0xC0000142 corresponde a STATUS_DLL_INIT_FAILED. Isso descreve falha de inicialização de DLL; não identifica qual DLL nem prova a causa. A consulta de eventos Application no intervalo da execução não encontrou evento correspondente. Não alterar permissões, desktop, DLLs ou políticas do sistema com base em suposição.

Fonte primária do significado do status: Microsoft MS-ERREF, https://winprotocoldoc.z19.web.core.windows.net/MS-ERREF/%5BMS-ERREF%5D.pdf.

Após a execução, uma consulta WFP NÃO elevada e somente leitura confirmou retorno FILTER_NOT_FOUND para cada uma das três chaves e SUBLAYER_NOT_FOUND para a subcamada. O PID observado não estava ativo e a fixture mantinha o conteúdo esperado. A verificação não adicionou/modificou/excluiu filtros. Rollback da fase inicial possui verificação própria no relatório do protótipo.

## Arquivos

- mediator-elevated-execution.json: contagem de tentativas, manifesto, horário e código da console.
- mediator-elevated-result.json: resultado bruto copiado da fixture, preservado.
- mediator-cleanup-verification.json: consulta independente somente leitura de limpeza/integridade.
- probes/ReadOnlyWfpCleanup.cs: verificador sem APIs de escrita.

A fixture foi preservada em TEMP, contendo source.txt e proof-result.json. Não foram acessados projetos reais, agentes ou credenciais; nenhum serviço foi instalado e nenhum perfil global do firewall foi alterado. Código do aplicativo, código/binários do protótipo e manifesto não foram modificados nesta etapa.

## Estado de continuação

Plan continua BLOCKED. Próximo trabalho é diagnosticar e corrigir a inicialização do worker em uma nova revisão do protótipo, revisar hashes e pedir nova autorização operacional se outra execução elevada for necessária. A autorização consumida não permite retries nem binários modificados. As garantias de origem somente leitura e estado externo/manual Save permanecem inalteradas.