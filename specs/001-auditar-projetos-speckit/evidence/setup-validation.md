# T001 — Preparação e auditoria

Data: 2026-11-16 (America/Sao_Paulo). Executor: IA. Base Git: 197548db2e3efa20c23e1f66b6a0ffb86c0f328d + alterações locais.
Modalidade: execução real de ferramentas de desenvolvimento, sem solução auditada ou agente externo.

Windows x64; SDK 10.0.404, runtime ASP.NET Core/.NET 10.0.15; global.json impede previews.
Versões fixadas: xunit 2.9.3, Microsoft.NET.Test.Sdk 18.4.0, xunit.runner.visualstudio 4.0.1, Microsoft.Playwright 1.64.1.
Dependências auditadas ANTES de introdução em projeto temporário SpecAudit-DependencyAudit-bf6ced6841b44656b63903070ddfa7e6; restore e consulta NuGet exit 0, sem achados retornados.
Referência de integração dos runners: https://xunit.net/docs/nuget-packages-v2 .

## Execuções e resultados

Diretório: raiz de desenvolvimento do SpecAudit.
- `dotnet restore SpecAudit.slnx --verbosity minimal`: exit 0; [saída](logs/setup-restore.txt).
- `dotnet test SpecAudit.slnx --no-restore --logger trx --results-directory specs/001-auditar-projetos-speckit/evidence/logs/setup-tests`: exit 0; três testes aprovados (runtime estável, Windows x64, inicialização do Playwright); [saída](logs/setup-tests.txt) e TRX no diretório setup-tests.
- `dotnet list SpecAudit.slnx package --vulnerable --include-transitive --format json`: exit 0, nenhum achado retornado; [JSON](logs/T017/packages.json).

Esses três testes verificam pré-requisitos de execução; não exercitam histórias de usuário nem iniciam navegador. Nenhuma alegação de segurança integral, proteção Windows ou produto funcional.
Sem instalação de ferramenta, elevação de privilégio ou alteração de solução de terceiros. Restore apenas das dependências próprias do desenvolvimento.
Autorrevisão: projetos referenciam apenas o SpecAudit; versões explícitas, arquivos lock de testes, logs sem conteúdo auditado. Aceite humano e PR pendentes.
