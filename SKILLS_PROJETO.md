# Skills do projeto Caixa Mercado

Fonte: `sickn33/agentic-awesome-skills`

Versão fixada: `v15.12.0` (`a71166d`)

Destino local: `.agents/skills`

O projeto usa uma seleção pequena e revisada. O catálogo completo não deve ser instalado,
pois contém mais de duas mil skills, aumenta o contexto e inclui instruções sem relação com
um PDV de minimercado.

## Seleção versionada

| Skill | Uso no projeto |
|---|---|
| `dotnet-backend` | ASP.NET Core 8, EF Core, DI e testes .NET |
| `api-and-interface-design` | contratos estáveis entre PDV, API e retaguarda |
| `postgres-best-practices` | schema, índices, concorrência e desempenho PostgreSQL |
| `test-driven-development` | ciclo teste falhando, implementação mínima e regressão |
| `security-and-hardening` | fronteiras de confiança, autenticação, segredos e pagamentos |
| `accessibility-compliance-accessibility-audit` | teclado, foco, contraste e tecnologia assistiva |
| `observability-and-instrumentation` | logs estruturados, correlação, métricas e alertas |
| `windows-shell-reliability` | scripts e instalação confiáveis no Windows |
| `deployment-procedures` | backup, implantação, verificação e rollback |

## Reinstalação

Use o instalador oficial do Codex com os mesmos caminhos e a tag `v15.12.0`.
As cópias versionadas no repositório são a referência do projeto; uma atualização de versão
deve ser revisada como mudança de código e nunca aplicada automaticamente.

## Regras

- não instalar o catálogo completo;
- não adicionar skills ofensivas ou sem relação direta com o produto;
- ler o `SKILL.md` completo antes de usar uma skill;
- adaptar exemplos de outras linguagens ao .NET/WPF, sem copiar dependências desnecessárias;
- testar qualquer comando, migration ou integração no ambiente de homologação;
- não permitir que uma skill substitua revisão de segurança, testes reais ou decisão do proprietário.
