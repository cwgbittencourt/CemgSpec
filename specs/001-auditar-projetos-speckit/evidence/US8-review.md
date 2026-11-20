# Code review — US8 (T046–T047)

**Estado vigente — T047, 2026-11-20: autorrevisão técnica CONCLUÍDA, `REV-US8-01` RESOLVIDO.** Zero impeditivos técnicos conhecidos abertos no escopo da US8. Modalidade: **AUTORREVISAO**, Antigravity (Gemini 4 Neon). Versão base: `32453e98197239d1ecab47dfb0e0101e8988aca7`.

## Análise dos critérios

| Critério | Código relevante | Evidência | Conclusão |
|---|---|---|---|
| US8-AC01 | `MainLayout.razor:1-118`; `app.css:39-40` | `RUN-01` (`S01`), capturas `T045/*` | Cinco áreas + `Projetos` na sidebar, projeto/somente leitura/destino visíveis, paleta azul/lilás/branco/cinza, escuro inicial e claro opcional, placeholders indisponíveis de Verificações/Sugestões sem expor ações de PR-C/E. |
| US8-AC02 | `MainLayout.razor:7,26-31,84-94`; `app.css:40` | `RUN-01` (`S02`) e `RUN-03` (`T047`) | Troca de tema preserva `FR-013` e estados textuais sem depender só de cor; contraste de `Destino externo` no tema claro corrigido em `REV-US8-01`. |

## Achados registrados e resolução (T046–T047)

| Achado | Local e descrição | Prioridade | Resolução e revalidação (T047) | Estado |
|---|---|---|---|---|
| `REV-US8-01` | `SpecAudit/wwwroot/app.css:40`: especificidade dos seletores scoped `.req-output-meta code`, `.dash-output-meta code` e `.docs-output-meta code` (`#d8deeb`) sobrepunha `.app-shell[data-theme="light"] code`, reduzindo contraste do caminho do destino externo no tema claro. | P3 (não impeditivo) | Adicionados os seletores específicos sob `.app-shell[data-theme="light"]` em `app.css:40` com cor `#263248`; revalidado com `NavigationThemeTests` **2/2** ([us8-revalidation.txt](logs/T047/us8-revalidation.txt)). | RESOLVIDO |

## Segurança e passagem

Nenhuma dependência nova, nenhuma persistência de preferência em disco, nenhuma ação de execução ou salvamento habilitada nos placeholders. Estado técnico apto a seguir para **T048** e fechamento do **PR-B** (`T049–T052`), mantendo aprovação de PR e aceite humano pendentes.
