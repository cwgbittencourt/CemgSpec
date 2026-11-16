# CEMG Spec Kit

Preset `cemg-speckit` versão 3.0.0, baseado no pacote de templates v2 revisado.
Preparado para inspeção e instalação futura em projetos escolhidos. Não foi
instalado em projetos existentes; somente em fixtures isoladas para validação.

## O que fornece

- Templates de spec, plan e tasks com rastreabilidade e validação obrigatória.
- Evidências de execução e review, roteiro do DEV, aceite e descrição de PR.
- Política de PRs por escopo, complexidade e possibilidade de teste.
- Adendo ao modelo de constituição e dez comandos habituais personalizados.
- Exemplos, instruções de adoção e relatório de validação técnica.

## Compatibilidade

Requer Spec Kit 1.1.3, a versão realmente verificada nesta entrega. O manifesto
restringe a instalação a essa versão porque os comandos preservam snapshots
do fluxo oficial 1.1.3. Validar e publicar nova versão do preset para ampliar
compatibilidade, sem alterar a restrição apenas para contornar um erro.

Os comandos usuais continuam sendo speckit.specify, speckit.plan, speckit.tasks
e demais comandos. O nome e a forma de invocação dependem da integração; em
Codex com skills são usados $speckit-specify, $speckit-plan e assim por diante.
Não há executável separado nem necessidade de renomear o CLI specify.

## Instalação local após inspeção

Extrair o ZIP em uma pasta dedicada e entrar no projeto já inicializado com
Spec Kit. Apontar --dev para a pasta que contém preset.yml:

```powershell
specify preset add --dev "C:\CAMINHO\cemg-speckit-3.0.0"
specify preset list
specify preset info cemg-speckit
specify preset resolve spec-template
specify preset resolve speckit.tasks
```

--dev copia o pacote para o projeto; não presumir sincronização com a origem.
A instalação muda arquivos de preset e comandos do projeto alvo. Ela não
reescreve automaticamente specs existentes nem a constituição já preenchida.
Antes de instalar, revisar o estado local e registrar mudanças em branch/PR.
Ver docs/ADOCAO.md para referências de prioridade, atualização e remoção.

## Escopo da garantia

O relatório de validação distingue instalação/resolução de execução funcional.
O smoke test não comprova que uma IA cumprirá todo o processo em um projeto real.
Planejar um piloto com a equipe. Não inclui CI, proteção de branch, publicação
remota nem automatização de abertura ou merge de PRs.

## Versões e convivência

O ID permanece cemg-speckit. A pasta e o ZIP incluem 3.0.0 apenas para identificar
a release. Projetos diferentes podem adotar releases diferentes; no mesmo projeto,
manter uma única versão ativa. Ver docs/VERSIONAMENTO.md e docs/VALIDACAO.md.

## Novidades desta versão

Condições obrigatórias de passagem, esclarecimentos sem quota total, análise
impeditiva, review mínimo e roteiro com estados separados. Inclui verificador
local de registros, coletor de execução, exemplo de CI e guia de branch protection.
Esses controles não são instalados nem ativados automaticamente. Ver docs/CONTROLES.md
e docs/TABELA-REVISAO.md. A versão 1.0.0 foi preservada.

## Execução reproduzível

Esta versão exige execução do comportamento real e observação do resultado com
ou sem tela. O DEV pode confirmar impossibilidade e decidir aguardar, rejeitar
ou aceitar com ressalvas. Há modelos Markdown/JSON e verificação estrutural
específica, sem converter impossibilidade em teste aprovado. Ver
policies/EXECUCAO-REPRODUZIVEL.md e docs/EXECUCAO.md.

A versão major muda o contrato dos registros de prontidão; registros antigos
precisam dos novos campos e evidências. Releases anteriores foram preservadas.
