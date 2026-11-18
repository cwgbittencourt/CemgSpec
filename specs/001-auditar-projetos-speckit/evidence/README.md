# Evidências de desenvolvimento

- [Rodada T019](T019-findings.json): dois achados PR-A resolvidos, regressão57/57 e manutenção6/6; detalhes nas rodadas finais de PR-A-validation.md e PR-A-review.md.


- [Review PR-A / T018](PR-A-review.md): autorrevisão com dois achados P2 abertos; correção/revalidação na T019.


- [Validação PR-A / T017](PR-A-validation.md): regressão 52/52, manutenção 6/6 e estado intermediário seguro; próxima T018.


Este diretório guarda comprovação técnica do desenvolvimento do SpecAudit, não relatórios oficiais produzidos sobre uma solução auditada. Relatórios do produto continuam sujeitos ao botão Salvar e ao destino externo.

## Pontos de entrada

- [Estado atual da implementação](implementation-status.md) e [gate do incremento](../verification/implementation-stage.json).
- [Validação US1 / T013](US1-validation.md): quatro cenários reais e respectivas limitações.
- [Review US1 / T014–T015](US1-review.md): histórico dos dois achados e rodada atual de correção/revalidação, sem aceite humano.
- [Roteiro US1 conferido na T016](../validation/US1-dev.md) e [estado técnico/aceite pendente](../acceptance/US1.md): ponto de entrada para reprodução e registro do DEV.
- [Manutenção das evidências](maintenance-review.md): sanitização, consolidação, execução renovada e verificações.
- [Histórico de transformações](maintenance-manifest.json): hashes anteriores/posteriores e consolidações por rodada.
- `logs/`: saídas, TRX e capturas indispensáveis à inspeção dos resultados registrados.
- `T012-before/`, `T012-code/`, `T013-before/`, `T013-code/`: checkpoints anteriores necessários para conferir versões que diferem do código corrente. Os manifestos antigos descrevem a revisão histórica indicada, não o checkout atual.

## O que permanece no Git

Manter resumos de validação, review, roteiros, manifestos, logs de falhas/correções e resultados finais que sustentem uma afirmação. TRX permitem verificar quais casos foram executados; snapshots anteriores só permanecem quando necessários à revisão histórica. Não guardar dumps de ambiente completo, credenciais, árvores de projetos auditados, bin/obj ou cópias exatas redundantes. Dados de fixtures sintéticas em TEMP permanecem locais; não copiar origens para o repositório.

Artefatos idênticos compartilham um arquivo canônico. A referência de cada execução continua no seu registro, inclusive quando duas execuções produziram saída byte a byte igual. Não inferir que uma rodada ocorreu apenas porque outra usa o mesmo log. Nomes, horários, resultados, erros, critérios e hashes preservam a distinção entre rodadas.

Os logs e TRX existentes são exports sanitizados: substituições de caminhos e identidade da estação não representam nova execução nem alteram PASSOU/FALHOU/NÃO EXECUTADO. `maintenance-manifest.json` registra hashes de entrada e saída. Hashes históricos que não correspondiam ao arquivo atual antes da manutenção permanecem históricos; não são substituídos pelo hash de outra revisão para simular validade. Revisões antigas READY são registros da época, não nova declaração de prontidão. O gate vigente de PR continua BLOCKED enquanto review e decisões humanas estiverem pendentes.

## Caminhos portáveis e privacidade

`__REPO_ROOT__` representa a raiz do checkout; `__USER_HOME__`, o perfil local; `__TEMP_ROOT__`, a pasta temporária. `__USER__`, `__MACHINE__` e `runner` substituem identidade técnica em logs/TRX; `__EMAIL__` omite endereços pessoais encontrados em saídas antigas de autenticação. Sufixos e GUIDs de fixtures são preservados para distinguir as execuções. Esses marcadores são de apresentação, não variáveis de shell: substituir pelo ambiente local ao reproduzir comandos históricos. Para iniciar uma receita nova, usar `Set-Location (git rev-parse --show-toplevel)` e o Python confiável disponível, sem depender do nome de usuário de quem executou originalmente.

Imagens mantidas foram conferidas visualmente: três mockups com dados ilustrativos e a captura da página inicial sem projeto carregado. Não contêm caminhos pessoais visíveis. Novas capturas exigem a mesma conferência antes de versionar. A sanitização textual não remove informação de imagens ou garante detecção de todos os segredos.

## Fluxo para novas evidências

Da raiz do checkout, em Python 3 e ambiente de teste Windows apropriado:

```powershell
python -m unittest discover -s tools -p test_maintain_evidence.py -v
python tools/maintain_evidence.py --check
python tools/maintain_evidence.py --verify
# Após produzir as evidências, sanitizar antes de git add:
python tools/maintain_evidence.py --apply
python tools/maintain_evidence.py --check
```

`--check` é somente leitura e retorna 1 quando há sanitização/consolidação pendente. `--apply` opera somente nos artefatos desta feature, repara hashes transitivamente, consolida apenas bytes idênticos sob logs/ e registra a transformação. Não executa scripts da origem, não altera o código do produto, não apaga falhas únicas e não aprova testes ou decisões humanas. Arquivos não textuais permanecem inalterados. Preservar finais de linha existentes; o repositório registra bytes exatos por SHA256.

`--verify` confere hashes e contenção de todas as referências do inventário exportado, manifesto de código corrente e gate vigente. Depois de adicionar novas evidências, atualizar o inventário com a revisão efetivamente conferida antes de executar essa verificação. O inventário exclui a si próprio e o gate que o referencia para evitar dependência circular. Registros históricos são conferidos contra sua revisão/snapshot e o manifesto de transformação, não tratados como se fossem do código corrente.

Na T015, o manifesto de código corrente foi renovado para a revisão corrigida. O manifesto anterior e os resultados negativos T014 permanecem recuperáveis no commit `aaf89ba8c232e69e8fcd1529e2437d6147836dd4`; seus hashes/revisões históricos não são convertidos em aprovação da versão nova. As observações positivas do replay estão em `logs/T015/original-probe-observations/`; os JSONs negativos originais T014 foram preservados byte a byte.

Antes do commit: conferir diff, conteúdo e nomes de arquivos, executar busca de segredos e os gates afetados. Os testes T013 já gravam os caminhos de fixtures usando `__TEMP_ROOT__`; o exportador TRX do framework ainda requer a passagem de sanitização. Não há CI ou hook automático instalado por esta manutenção.

A manutenção altera o checkout atual, sem reescrever commits antigos. Caminhos pessoais previamente commitados continuam recuperáveis no histórico Git. Não foram identificadas credenciais nos padrões examinados; isso não é garantia absoluta. Uma eventual credencial real exige tratamento próprio, incluindo revogação, e não apenas troca por marcador.
