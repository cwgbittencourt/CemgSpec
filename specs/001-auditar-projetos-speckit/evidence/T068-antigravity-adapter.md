# T068 — Adaptador documental Antigravity

Implementação e autorrevisão em 2026-11-20, America/Sao_Paulo. Base `747a802`, incremento PR-D. Ambiente Windows x64, SDK .NET 10.0.404. A instalação local respondeu `agy.exe --version` com `1.5.9`. T064 consta concluída, e a execução no produto permanece indisponível sem o selo individual da T069.

| Checklist | Total | Atendidos | Pendentes |
|---|---:|---:|---:|
| requirements.md | 19 | 19 | 0 |

## Contrato exercitado

| Verificação | Observação | Resultado |
|---|---|---|
| US5-AC02, resposta documental | O harness recebeu quatro snapshots pelo canal restrito, produziu uma resposta estruturada e conferiu o ID e hash de origem do achado. Referência de outro documento, hash inválido e evento de ferramenta produziram Inconclusive sem achados. | PASSOU |
| US5-AC04, integridade | O harness conferiu argumentos fixos, políticas de negação e ausência de conteúdo de código, segredo sintético e outra funcionalidade no canal. Pré-requisito ausente bloqueou antes do executor. Origem e destino permaneceram intactos. | PASSOU |
| US5-AC04, estado externo | O adaptador criou uma pasta de execução temporária e um perfil exclusivo fora da origem e do destino. O teste lançou um subprocesso real que recebeu os diretórios de perfil e temporários esperados. Tentativa de configurar perfil na origem foi recusada. O harness manteve a conversa sintética no perfil externo e o resultado oficial em memória. | PASSOU |
| US5-AC05, falhas | Autenticação ausente e término incerto retornaram Failed. Cancelamento com confirmação do executor retornou Cancelled. Os três estados ficaram sem resultado semântico. A prova de descendentes do Antigravity real pertence à T069. | PASSOU |
| Proteção de habilitação | Sem avaliação individual, o serviço recusou preparar uma execução do Antigravity. O teste de regressão confirmou a independência de Codex e check_gate. | PASSOU |

O adaptador envia um turno por stdin e aceita o campo `structured_output` do resultado terminal. A linha de comando desativa a expansão de comandos e skills e aplica configurações de permissão estritas. Na T068, as respostas e o cancelamento do Antigravity foram simulados no harness. A CLI real foi consultada para conferir versão e opções de linha de comando. A T069 deve validar autenticação em perfil isolado, comportamento das permissões, finalidade de rede, estado e descendentes com o binário real e fixtures sintéticas. [Protocolo headless](https://antigravity.google/docs/cli/headless/) · [Regras de permissões](https://antigravity.google/docs/permissions?tab=cli).

## Reprodução

A partir da raiz do repositório, rode:

```powershell
dotnet test tests/SpecAudit.Windows.Tests/SpecAudit.Windows.Tests.csproj --no-restore --filter 'Provider!=Codex' --logger 'console;verbosity=normal'
```

O filtro exclui o teste que exige um Codex autenticado. A rodada aprovou **36 testes**, incluindo oito cenários do harness Antigravity e a prova do ambiente do subprocesso. O aceite do DEV segue pendente. [Saída da rodada](logs/T068/windows-contract.txt).

### SHA-256 dos arquivos de código e teste desta rodada


- `SpecAudit/Infrastructure/Agents/AntigravityAnalyzeAdapter.cs` — `17dcff8ac25a6aa9cd3dffb7da8e72d0a54fe6f67d0e69275dfe7c91f60d7305`
- `SpecAudit/Infrastructure/Agents/AgentDiscovery.cs` — `3656083ba45ef09f75161840f1e20235ed192529936bc2b3c83b99add5bdaf06`
- `SpecAudit/Infrastructure/Agents/CodexAnalyzeAdapter.cs` — `0b2721a0d1d67acc562bdc5bb9c9303a5aacb9fcfafc2eca2080761cbfd3bbad`
- `SpecAudit/Infrastructure/Execution/TrustedCommandRunner.cs` — `003e06d63b8526d06fdf12b6b6f4be2e1579783384b274f579593c8738f60142`
- `tests/SpecAudit.CommandFixture/Program.cs` — `7a25490c1af113947496f183bb6d9b8f2fa58c48e734e9394ae5a739f562699d`
- `tests/SpecAudit.Windows.Tests/AnalyzeProtectionTests.cs` — `46fa44cf60ff3aeec293559855f494353fe1cdc57f8975db8250331a386d2f1c`
- `tests/SpecAudit.Windows.Tests/TrustedCommandRunnerTests.cs` — `91332d43473f42197d92acbfd6d802dd20cd0723be0ee45838e8c13a759f967a`
