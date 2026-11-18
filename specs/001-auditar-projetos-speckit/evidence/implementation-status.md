# Implementação — checkpoint T016 (roteiro consolidado e conferido)

Atualização vigente — T017 concluída: validação técnica PR-A e regressão 52/52, manutenção 6/6; bases ancestrais e fonte inalterada verificadas. Evidência PR-A-validation.md. Próxima T018; revisão completa, fechamento e decisões humanas pendentes. Checkpoints abaixo são históricos.


Atualização vigente — 2026-11-17, America/Sao_Paulo: T016 concluída, produto/testes iguais ao commit f2b29a20e5f9e724a9c32f8d8e3a703e0587d541 e manifesto corrente. Roteiro validation/US1-dev.md refeito para o caminho principal de harness real e conferido: restore locked, cenários 4/4, correções 7/7 e suíte 52/52. Exploração GUI adicional tem conferência parcial explícita. acceptance/US1.md separa estado técnico de execução e decisão humanas, com campos reservados em branco. Próxima task T017; fechamento PR-A e aceite humano pendentes. Nenhum código de produto/teste, dependência ou decisão humana alterado. Checkpoints abaixo são históricos.

Atualização vigente — 2026-11-17, America/Sao_Paulo: T015 concluída. REV-US1-01 e REV-US1-02 resolvidos na revisão local sobre `aaf89ba8c232e69e8fcd1529e2437d6147836dd4`; fonte exata no manifesto corrente. Metadados de diretórios existentes não invalidam documentos; eventos estruturais e erros do watcher continuam monitorados. Limites são tratados em Open/Refresh com mensagem e recuperação segura. Suíte 52/52, replay dos probes T014 3/3 e teste antes intermitente 5/5 aprovados. Review renovado sem impeditivo conhecido no escopo dos dois achados. Próxima task T016; fechamento/aceite/aprovação humana pendentes. Os checkpoints abaixo são históricos, não o estado corrente.

## Checkpoints anteriores

Data 2026-11-17, America/Sao_Paulo. T013 concluída tecnicamente após corrigir lacuna de cobertura com harness integrado real. Próxima task T014; nenhuma task posterior iniciada. Checkpoint anterior preservado em T013-before/implementation-status.md.

Atualização T014: passagem de autorrevisão executada sobre `89242b285c15e24fac3fb89cfc6ec93dd076e18a`. Review para aprovação BLOQUEADO por REV-US1-01 e REV-US1-02. Baseline 45/45; três probes novos falharam, com origem preservada e sem relatório criado. Próxima task T015, correção/revalidação. Nenhum código de produção ou teste permanente alterado nesta task. Detalhes e reprodução em US1-review.md e T014-review-context.json. As seções T013/manutenção abaixo registram os checkpoints anteriores, não aprovação da revisão atual.

Constituição 4.0.0 preservada: origem readonly pela aplicação; apenas speckit-analyze/check_gate; nenhum retorno aos experimentos de isolamento OS. SEC-001 continua histórica, não foi convertida em aprovação.

Concluídas: T001–T013 e migração T095–T098. Suíte final T013: 45 testes aprovados, 0 falhos/ignorados, logs/T013-20261117/final-suite.txt e TRX em final-results/. Falha inicial do harness preservada e corrigida; rodada 2 e regressão final aprovadas. Base atual 139fd81db66943bbc2e167ccd13a75b4a317a2fc e hashes em T013-code-manifest.json. Nenhum código de produção alterado nesta task.

T012: filtro documental, detecção de alterações/renomeações/novas pastas, revisão monotônica, rejeição de snapshot desatualizado, cancelamento próprio por troca/refresh/dispose e sincronização de funcionalidades. Serviço/broker executados com filesystem real; picker controlado. Fronteiras futuras demonstradas pela API genérica de versão, sem alegar Prepare/Execute/Save completos.

T013: quatro cenários US1-S01–S04/AC01–AC04 comprovados em harness com NativeFolderPicker real, serviço, broker e monitor Windows, baselines integrais e edição externa intencional. S04 exercita trabalho documental em andamento, não agentes ou Save futuros. Página inicial também exercitada em host loopback/Edge reais. Validação, reprodução e autorrevisão do incremento em US1-validation.md; revisão completa da história ainda é T014. Templates/roteiro de aceite humano preservados. Nenhuma User Story aceita, PR aprovado, commit, push ou deploy. Gate PR-A continua BLOCKED por revisão/fechamento e aprovação humana pendentes; isso não bloqueia iniciar T014. Hooks extensions.yml ausente na entrada e na saída.

## Manutenção das evidências — 2026-11-17

Organização/sanitização autorizadas e preparadas para commit. Índice em README.md; transformações e hashes em maintenance-manifest.json, exports atuais em maintenance-inventory.json e versão testada em maintenance-code-manifest.json. Regressão renovada após sanitização na geração do harness: 45/45 aprovados; seis testes do sanitizador aprovados. A rodada anterior teve uma falha intermitente do monitor; cinco retestes isolados passaram, mas a causa não foi confirmada. Ver OBS-MAINT-01 em maintenance-review.md para revisão T014. Falha e retestes preservados, nenhum aceite inferido.
