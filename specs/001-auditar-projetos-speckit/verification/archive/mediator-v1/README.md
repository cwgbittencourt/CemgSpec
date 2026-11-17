# Protótipo revisável do mediador de rede

Preset de planejamento: cemg-speckit 3.0.0. Constituição vigente: 2.0.0.
Este projeto é uma prova técnica separada do aplicativo. Não é serviço instalado, API privilegiada de produção ou integração final dos agentes.

## Preparado e executado sem privilégios

- Build Release do protótipo, sem dependências NuGet de terceiros.
- Autotestes de gates, ciclo de vida e layouts x64, sem abrir WFP ou conectar à rede.
- Modo descritivo sem mutações.
- Consulta de vulnerabilidades de pacotes: sem avisos; não há PackageReference. Não comprova segurança integral de SDK/runtime ou código nativo.

## Escopo exato da futura autorização

Autorizar somente uma execução da DLL identificada no manifesto, em console elevada já aberta pelo usuário, com `--run-wfp-proof --ack-synthetic-only`.

Essa execução poderá:

1. Criar uma pasta nova `SpecAudit-MediatorProof-<guid>` sob TEMP contendo somente um arquivo-fonte sintético e o relatório da prova.
2. Abrir servidores locais descartáveis TCP IPv4, TCP IPv6 e UDP para atuar como alvos de teste, sem acesso a serviços reais.
3. Ensaiar primeiro rollback de uma instalação parcial de filtro, com falha sintética antes do commit e antes de criar o worker; verificar limpeza. Depois criar um novo AppContainer temporário com SID exclusivo e lançar PowerShell sintético com privilégios desabilitados, grupo Administradores desabilitado e integridade baixa. Uma lista explícita de handles herda somente saída e um arquivo aberto somente para leitura.
4. Criar uma sessão WFP dinâmica, uma subcamada própria e três filtros condicionados obrigatoriamente ao SID dessa execução: permitir apenas TCP para `127.0.0.1:<porta sintética permitida>`; negar demais conexões IPv4 e IPv6 dessa identidade.
5. Executar tentativas de leitura, escrita, criação, renomeação e exclusão apenas na fixture, e conexões aos alvos sintéticos permitidos/negados, tanto pelo worker quanto por um filho.
6. Encerrar o Job Object, verificar processos terminados, fechar a sessão dinâmica e conferir a ausência de seus filtros/subcamada. O perfil AppContainer próprio também é removido, com resultado registrado.
7. Preservar a fixture e `proof-result.json` para inspeção, sem limpar diretórios do usuário ou restaurar arquivos para ocultar alterações.

O componente elevado é o mediador/harness, não o executor. A prova NÃO deve receber diretório real de projeto, SID externo, PID de outro processo, hostname, porta ou executável fornecidos pelo usuário. Esses argumentos não existem e são rejeitados.

## Não autorizado por essa prova

- Instalar serviço, driver, certificado ou tarefa agendada; mudar UAC ou conceder direitos persistentes.
- Alterar perfis globais do firewall, exceções de loopback, regras de terceiros ou criar filtros persistentes.
- Executar Codex/Antigravity, acessar suas credenciais, copiar tokens, conectar a provedores externos ou ler solução real.
- Considerar a prova aprovação de produção, integração dos agentes ou aceite humano das histórias.

Os filtros usam o motor WFP do Windows, mas seu predicado é o SID exclusivo da fixture. Não são regras de abrangência geral. O permit não usa CLEAR_ACTION_RIGHT, preservando bloqueios de outras políticas.

## Comandos de preparação, sem elevação

Na raiz do repositório:

```powershell
$project = '__REPO_ROOT__\specs\001-auditar-projetos-speckit\prototype\NetworkMediator\NetworkMediator.csproj'
dotnet build $project -c Release --no-restore
$binary = '__REPO_ROOT__\specs\001-auditar-projetos-speckit\prototype\NetworkMediator\bin\Release\net10.0\NetworkMediator.dll'
dotnet $binary --self-test
dotnet $binary --describe
Get-FileHash -LiteralPath $binary -Algorithm SHA256
```

## Comando que depende de autorização posterior

Somente depois da autorização específica e da conferência do hash no manifesto, em PowerShell aberto como Administrador:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' '__REPO_ROOT__\specs\001-auditar-projetos-speckit\prototype\NetworkMediator\bin\Release\net10.0\NetworkMediator.dll' --run-wfp-proof --ack-synthetic-only
```

O programa não abre UAC nem usa RunAs. Sem console elevada ou sem a opção de confirmação, recusa a prova. Se recompilar os binários, revisar os novos hashes antes de executar; autorização não se transfere automaticamente para um binário alterado.

## Resultados necessários

- Rollback da transação injetada confirmado, sem worker iniciado e sem filtros/subcamada residuais.
- Duas respostas, root e child, com leitura por broker aprovada e quatro operações de escrita negadas.
- Duas conexões permitidas à porta sintética aprovada e zero às portas IPv4/IPv6/UDP negadas.
- TokenIsAppContainer=1 e grupo Administradores não habilitado no worker.
- Origem intacta, nenhum processo ativo após terminação e nenhum filtro/subcamada próprios restante; perfil removido com retorno zero.
- Processo-controle fora do AppContainer continua conectando ao alvo, mostrando que a política é específica. A prova não deve relaxar bloqueios de terceiros para obter sucesso.
- Resultado final `PASSED_SYNTHETIC_SCOPE`; qualquer exceção, ausência de saída ou limpeza não comprovada é falha/inconclusivo, não sucesso.

## Testes posteriores obrigatórios, fora desta primeira autorização

- Broker/proxy real com domínios exatos, DNS, redirects, IPs literais, IPv4/IPv6/UDP e autenticação dos agentes.
- Codex e Antigravity com estado externo gravável, restrição de instruções/plugins e canais internos autorizados.
- IPC autenticado do mediador como serviço e tentativa de cliente não autorizado/PID reutilizado.
- Falha abrupta do mediador, perda do proxy e cancelamento durante análise longa. O protótipo cobre encerramento ordenado; NÃO comprova ausência de janela de rede livre em crash abrupto. Job e sessão dinâmica encerrados no mesmo processo não fornecem sozinhos essa garantia.

A liberação do plano completo continua dependente dessas provas. Não instalar o protótipo como serviço ou usá-lo com dados reais.

## Revisão de segurança do escopo

`Program.cs` aceita apenas modos fechados; `MediatorSession.cs` requer um SID não nulo do perfil gerado pelo próprio launcher; não há API de regras arbitrárias. A instalação é transacional, sem flags persistentes ou hard permit. `NativeWorker.cs` lança suspenso, associa Job Object e aplica política antes de retomar, com handles permitidos explicitamente. A rotina de cleanup e falhas será exercitada na prova autorizada, não considerada comprovada por compilação/autoteste.
## Wrapper com conferência de hashes

Para a execução autorizada, preferir Invoke-MediatorProof.ps1 -ExecuteSyntheticProof -ExpectedManifestSha256 com o hash completo abaixo. O wrapper confere manifesto e os três arquivos carregados, exige console elevada existente e nunca solicita UAC. Sem ExecuteSyntheticProof, executa apenas autotestes sem privilégios.

Manifesto revisável: verification/mediator-prototype-manifest.json (relativo à feature).
SHA256 do manifesto: d0fd1c46893c69d51dd2aba717834b8aed30a09423ff4b53b9bb5fc458eec115

Qualquer alteração nos binários/manifesto invalida esta identificação. Nenhuma autorização operacional foi concedida nesta preparação.


Reforços de preparação: imports nativos restritos a System32; caminho do PowerShell derivado do sistema; wrapper recusa variáveis de extensão do runtime, sem alterar ambiente global. Estes controles não substituem revisão do binário e prova privilegiada.
