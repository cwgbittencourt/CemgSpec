# Evidências de validação — PR-A / T017

Estado: APROVADA (validação técnica do incremento; revisão final e decisões humanas pendentes).
Executor: IA / speckit-implement. Data: 2026-11-17, America/Sao_Paulo.
Commit candidato: `3bff5005a061b746e826c26b14d7e0f5f6e96016`. Fonte exata: `sha256:530031e68a790d3b2b186300c23441091ef951d6fb93fdc1bae4029b11b451f8` em maintenance-code-manifest.json.
Alterações locais: apenas evidências T017 e acompanhamento; produto, testes e ferramentas iguais ao candidato. Histórico e fonte reproduzíveis pelo Git e manifesto.
Ambiente: Windows, .NET SDK 10.0.404; detalhes em logs/T017/environment.txt. Dados sintéticos isolados; nenhuma solução real auditada.

## Identificação do incremento e histórico

PR previsto: PR-A, sem URL criado. Escopo: INCREMENTO, US1-AC01–AC04 e fundação/proteção de leitura, T001–T016 e T095–T098. Base resolvida: `79d9c3d08d75afe2fef7e233493b8648e61ed4e0`; base de código anterior à implementação: `197548db2e3efa20c23e1f66b6a0ffb86c0f328d`. Ambas verificadas como ancestrais do candidato. Sem PR predecessor. T016 concluída; T018–T020 permanecem pendentes.

## Execuções

Diretório de todos os comandos: `__REPO_ROOT__`.

| Execução | Critério/cenário | Comando ou procedimento | Esperado | Observado | Resultado | Artefato |
|---|---|---|---|---|---|---|
| RUN-01 | Dependências fixadas | `dotnet restore SpecAudit.slnx --locked-mode` | Restore sem mudar locks | Exit 0, locks intactos | PASSOU | logs/T016/restore.txt |
| RUN-02 | Fundação, fronteiras, US1-AC01–AC04, regressão dos achados | `dotnet test SpecAudit.slnx --no-restore --logger 'trx;LogFilePrefix=T017' --results-directory specs/001-auditar-projetos-speckit/evidence/logs/T017/results` | Todas as asserções passam | Exit 0; 7 domínio +14 Windows +31 UI =52; zero falhas/ignorados | PASSOU | logs/T017/full.txt; results/*.trx; harness/*.json |
| RUN-03 | Sanitização e rastreabilidade | `python -m unittest discover -s tools -p test_maintain_evidence.py -v` (Python local 3.11) | 6 testes passam | Exit 0, 6/6 | PASSOU | logs/T017/maintenance-tests.txt |
| RUN-04 | Dependências transitivas | `dotnet list SpecAudit.slnx package --vulnerable --include-transitive --no-restore --format json` | Consulta conclui e não reporta vulneráveis | Exit 0; nenhum pacote vulnerável reportado | PASSOU | logs/T017/packages.json ou alias no manifesto de manutenção |

## Execução real do produto e efeitos

Meio BIBLIOTECA/INTEGRACAO, com o comando RUN-02. Os quatro US1IntegratedHarnessTests acionam o NativeFolderPicker real por mensagens Win32 nas próprias janelas, ProjectService, broker e monitor sobre filesystem Windows real. Entradas: duas features SpecKit, uma tasks ausente, origem e destino externos temporários; tentativas de destino igual/interno/junction; troca de projeto/destino com histórico sintético; leitura documental em andamento e edição externa.

Asserções observadas: descoberta e ausência documental corretas; rejeições de fronteira; baselines da origem preservados; relatórios não gerados; contextos separados e histórico com caminho/hash preservado; alteração externa conservada, versão Changed e cancelamento do trabalho próprio. Sete testes de correção cobrem metadados de diretórios e limites Open/Refresh com recuperação. Outros testes verificam sessões, catálogo/broker, identidade/reparse, canais e limites de execução. O teste da página inicial usa host loopback e Edge reais. Evidência estruturada por cenário em logs/T017/harness; TRX identifica cada teste.

## Estado intermediário seguro e limitações

Inspeção da revisão: Program restringe loopback/Host/Origin e sessão; broker abre handles para leitura e revalida identidade/versão. VerificationType permanece fechado em AnalyzeDocuments e CheckGate. Botões Executar verificação e Salvar resultados estão desabilitados. Não há uso de agentes reais nem salvamento habilitado. Runner exercitado apenas com comandos confiáveis de teste. Proteção readonly pela aplicação, sem promessa de isolamento pelo sistema operacional.

Não executados: passeio manual completo da GUI carregada e funcionalidades de incrementos futuros; fora do contrato de T017. Handlers de limites usam reflexão, picker controlado e filesystem real. Execução/aceite do DEV, aprovação humana e fechamento do PR continuam pendentes. Auditoria NuGet não cobre SDK, Windows ou navegador. Roteiro reproduzível: ../validation/US1-dev.md; campos humanos preservados.

## Rodadas e passagem de revisão

Esta rodada executa novamente a suíte sobre o candidato corrente. Não houve correção de fonte nesta task; evidências históricas negativas T014 e red T015 preservadas. Manifesto de código conferido sem divergências. AUTORREVISAO T017 da base, evidência, efeitos e controles sem novo impeditivo encontrado. Isso não substitui a revisão completa do incremento prevista na T018. Gate geral permanece BLOCKED por T018–T020 e decisões humanas, sem invalidar a validação técnica aqui registrada.
