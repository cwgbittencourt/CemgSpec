# Autorização operacional a solicitar — prova sintética do mediador

**Estado atual**: AUTORIZAÇÃO ÚNICA CONSUMIDA em 2026-11-16; execução terminou incompleta. Não reutilizar este pedido para outra execução. Ver mediator-elevated-summary.md. O escopo abaixo preserva o pedido original aprovado.
**Manifesto SHA256**: d0fd1c46893c69d51dd2aba717834b8aed30a09423ff4b53b9bb5fc458eec115
**Artefato**: prototype/NetworkMediator, conforme arquivos/binários registrados no manifesto.

## Texto preciso para autorizar uma próxima etapa

“Autorizo uma única execução elevada do protótipo sintético NetworkMediator identificado pelo manifesto SHA256 d0fd1c46893c69d51dd2aba717834b8aed30a09423ff4b53b9bb5fc458eec115. Autorizo somente os filtros WFP dinâmicos vinculados aos AppContainers temporários dessa prova, testes locais de conexão permitida/negada, rollback, encerramento dos workers sem privilégios e verificação de limpeza. Não autorizo instalação de serviço, regras persistentes ou globais, acesso a uma solução real, execução de Codex/Antigravity ou uso de credenciais.”

A autorização é para executar a prova, não para considerar resultados aprovados antecipadamente. O usuário precisará aprovar a elevação na etapa apropriada; o wrapper não solicita UAC nem se eleva automaticamente.

## Comando exato, somente após autorização

Em PowerShell já aberto como Administrador:

```powershell
& '__REPO_ROOT__\specs\001-auditar-projetos-speckit\prototype\NetworkMediator\Invoke-MediatorProof.ps1' -ExecuteSyntheticProof -ExpectedManifestSha256 'd0fd1c46893c69d51dd2aba717834b8aed30a09423ff4b53b9bb5fc458eec115'
```

O wrapper exige administrador, verifica o manifesto e os hashes da DLL, deps e runtimeconfig. Se algo mudou, interrompe antes da prova; revisão e autorização precisam ser renovadas.

## O que a execução fará

- Criar apenas dados novos em TEMP e dois perfis AppContainer exclusivos (um para rollback antes de iniciar worker e outro para a prova normal).
- Usar a sessão WFP dinâmica de cada fase para filtros condicionados ao SID exclusivo; não alterar perfis globais do firewall nem objetos de terceiros.
- Na fase normal, permitir somente TCP à porta local do alvo aprovado e negar demais conexões IPv4/IPv6 da identidade, incluindo a tentativa UDP ao alvo negado.
- Lançar somente PowerShell sintético e seu filho com token restrito e grupo Administradores desabilitado; fonte passada por handle somente leitura. Nenhum agente real será executado.
- Encerrar o job antes da retirada normal dos filtros e verificar limpeza. Preservar proof-result.json e a fixture para inspeção. Não apagar diretórios ou históricos do usuário.

## Limites e risco remanescente

É um protótipo de política e encerramento ordenado. O comportamento de crash abrupto do próprio mediador, a allowlist real de provedores, o proxy, autenticação e os agentes ainda exigem provas posteriores. Uma falha do processo privilegiado pode encerrar a sessão dinâmica; o protótipo não comprova ausência absoluta de janela de tráfego durante crash. Por isso a autorização é exclusivamente sintética, sem dados, credenciais ou serviços reais.

Nenhum serviço, driver, certificado ou tarefa será instalado. Nenhum executável do agente será elevado. Qualquer resultado incompleto ou limpeza não comprovada mantém o plano bloqueado.