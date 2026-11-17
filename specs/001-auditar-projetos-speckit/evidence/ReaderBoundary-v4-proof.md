# ReaderBoundary v4 — controles da aplicação

Data 2026-11-16; execução pela IA; constituição 4.0.0. Este registro não é sandbox OS.

Prova: ReaderBoundaryTests exercita catálogo por IDs, leitura real por handle readonly, rejeição de projeto/documento alheio, código/config fora do catálogo, junção trocada no acesso, arquivo substituído e geração divergente. ApplicationBrokerRemainsReadOnlyWhenOriginHasBroadApplicationAcl confirma leitura autorizada e hashes/conjunto de arquivos preservados sob ACL ampla. ProjectSelectionTests exercita destino interno/igual/junção recusado, dois projetos/contextos, histórico anterior preservado e edição externa mantida. Nenhum relatório é escrito ao carregar.

Suíte completa atual: 32 aprovados, zero falhos/ignorados; saída logs/v4-T011-complete-2.txt e TRX correspondentes. Hashes do candidato: implementation-code-manifest.json.

O antigo teste que demonstrou acesso direto de um processo a código sob ACL ampla permanece arquivado como falha da garantia v3. Não se transformou em teste aprovado. A v4 não promete conter agente arbitrário pelo Windows. Os controles de agente/gate/writer têm provas próprias futuras; este registro só cobre broker/seleção do aplicativo.
