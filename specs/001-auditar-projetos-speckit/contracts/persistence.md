# Contrato de persistência e histórico — V1

**Revisão**: 2026-11-16 | constituição 3.0.0 | cemg-speckit 3.0.0.

## Autoridade de escrita

Writer externo é o único componente do SpecAudit que aceita conteúdo oficial para disco; broker, UI, agentes e executor gate não têm operação de gravação na origem. A permissão normal do usuário na solução não é alterada. O writer não aceita path arbitrário do agente/UI: usa identidade do destino escolhido no picker e nome de registro gerado internamente.

Antes de Salvar: resultados, sugestões, decisões, índices e histórico novo em memória. Nenhum autosave, exportação provisória em disco, log de prompt/saída ou cache próprio com conteúdo. Conversas/caches de agente são exceção expressa exclusivamente externa, informados separadamente; não são report.json nem histórico oficial e nunca importados automaticamente.

## SaveResults

Entrada: ProjectId, RunIds/DecisionIds, formatos escolhidos (Markdown/JSON/CSV), geração, identificação de resultado Stale e confirmação explícita de conteúdo/destino. Revalidar destino por identidade resolvida, inclusive todos os ancestrais/reparse points; recusar origem/descendente da origem ou mudança de identidade. Nenhum fallback para pasta do projeto.

Criar novo diretório com nome gerado `SpecAudit-<timestampUtc>-<uuid>` usando semântica create-new. Arquivos dentro dele: `manifest.json`, `report.md` quando selecionado, `report.json` quando selecionado e `findings.csv`/`indicators.csv` para listas solicitadas. Manifesto técnico acompanha qualquer formato salvo e pertence à mesma ação Salvar. Não incluir cópia integral da solução, credenciais, fontes não autorizadas ou conversas internas do agente.

Metadados comuns: SchemaVersion=1, SavedRecordId, projeto/funcionalidade, escopo, timestamps, hashes documentais, revisão esperada/registrada, agente/ferramenta/versão, limites, atualidade, fórmula dos indicadores, achados e decisões humanas atribuídas. Caminhos podem ser relativos à origem indicada; segredos e trechos desnecessários são excluídos. CSV deve tratar texto iniciado por fórmula como texto seguro, preservando valor lógico e explicando codificação no manifesto.

Gravação segura: reservar nome novo; escrever arquivos apenas dentro desse novo destino; concluir manifesto por último com estado Complete e hashes finais. Interrupção produz erro SaveFailed e identifica eventual diretório externo incompleto, sem anunciá-lo como registro válido e sem apagar automaticamente para esconder efeito. Não sobrescrever registro anterior. O usuário pode excluir o incompleto explicitamente na área externa. TOCTOU/caminhos e escrita em destino trocado devem ser testados antes de habilitar writer.

## Histórico e exclusão

Listar somente pacotes oficiais com schema conhecido/manifesto válido, validando hashes; pacote incompleto/adulterado é identificável e não tratado como resultado atual. Comparação por chaves contextualizadas + fingerprints, separando ocorrências novas/persistentes/resolvidas e mudanças de conteúdo/escopo; ausência em amostra incompleta não significa resolvido. Risco aceito não aprova teste e falso positivo não comprova implementação.

Sem retenção/exclusão automática. Trocar destino não move registros. DeleteSaved requer seleção explícita de SavedRecordId e ação humana; validar identidade e contenção no destino externo, sem seguir links e sem remover outros diretórios. Origem nunca é alvo de DeleteSaved. Limpeza de fixtures de desenvolvimento não integra esse comando.

## Provas de implementação

Antes de habilitar: fechar sessão sem Save e observar zero relatórios oficiais; salvar nos três formatos e conferir conteúdo/hashes; simular destino interno/junção trocada/permissão revogada/disco cheio; demonstrar estado parcial honesto e origem intacta; comparar históricos e excluir somente pacote selecionado. IA executa fluxo real, DEV reproduz versão avaliada; sem aprovação por declaração documental isolada.
