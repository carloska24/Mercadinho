# Plano de Implementação do Núcleo Multicaixa

Status: em execução

Branch: `codex/nucleo-dados-multicaixa`

Arquitetura de referência: `ARQUITETURA_NUCLEO_MULTICAIXA.md`

## Estratégia de transição

O PDV atual continuará usando `IVendaService` e `VendaService` em memória até que a API persistente tenha cobertura suficiente. O novo núcleo será criado em namespaces paralelos e não modificará os contratos nem as entidades usadas pelos bindings WPF.

A troca futura será feita por um adaptador configurável. O modo legado continuará disponível durante homologação para permitir comparação e reversão segura.

## Fatia 1 — Catálogo e carrinho persistente

Objetivo: estabelecer domínio, banco e API sem implementar uma finalização parcialmente segura.

- novo modelo de catálogo e venda com invariantes;
- itens com snapshot de produto e preço;
- versão otimista da venda;
- EF Core 8 + PostgreSQL;
- mapeamentos, constraints, índices e migração inicial;
- criação e consulta de venda;
- inclusão e remoção de item;
- idempotência persistida dos comandos;
- `ProblemDetails`, correlação e health checks;
- testes unitários, de contrato e de integração.

Critério de aceite: banco vazio recebe a migração; venda sobrevive a novo contexto; snapshots permanecem estáveis; repetição da mesma chave não duplica operação; alterações concorrentes produzem um vencedor e um conflito; testes legados continuam verdes.

## Fatia 2 — Estoque, pagamento e caixa

Objetivo: finalizar vendas de forma atômica.

- sessão e movimentos de caixa;
- saldo e razão de movimentos de estoque;
- tentativas e divisão de pagamento;
- baixa condicional de estoque;
- finalização transacional e idempotente;
- cancelamento/reversão auditável;
- teste de dois PDVs disputando o último item.

Critério de aceite: venda, pagamento, estoque, caixa e auditoria confirmam juntos ou não confirmam; reinício e retry não duplicam registros.

### Recorte aprovado para a primeira entrega da fatia 2

- uma única sessão aberta por terminal, garantida também por índice único parcial no PostgreSQL;
- saldo materializado não negativo e razão de movimentos append-only por produto;
- venda finalizada uma única vez, somente com sessão aberta, versão atual e itens válidos;
- dinheiro pode ser aprovado internamente quando o valor recebido cobre o total;
- Pix, débito e crédito somente podem finalizar quando houver confirmação externa explícita;
- pagamento, baixa de estoque, movimento de caixa, venda e idempotência participam do mesmo commit;
- integrações externas ficam fora da transação e serão tratadas por estado persistido/outbox em entrega posterior;
- autenticação, TEF, fiscal, sangria, suprimento e fechamento cego completo não serão simulados nesta entrega.

### Invariantes e falhas esperadas

- duas finalizações concorrentes do último saldo produzem exatamente um sucesso;
- estoque insuficiente não grava pagamento, movimento, venda parcial nem idempotência de sucesso;
- repetição da mesma chave e conteúdo devolve o resultado anterior sem nova baixa;
- mesma chave com conteúdo diferente retorna conflito;
- meio eletrônico sem aprovação retorna estado pendente ou erro operacional, nunca sucesso;
- falha antes do commit permite repetição segura; falha depois do commit é resolvida pela mesma chave idempotente.

### Registro de decisões da fatia 2

| Decisão | Alternativas consideradas | Motivo |
|---|---|---|
| Finalização em um único commit local | commits separados por módulo | elimina venda, estoque ou caixa parcialmente confirmados |
| Ledger + saldo materializado | apenas campo de estoque | permite auditoria e consulta rápida sem perder reconciliação |
| Concorrência otimista com constraint de saldo | lock longo ou estoque negativo | atende dois caixas com menor contenção e impede saldo inválido |
| Eletrônico exige aprovação explícita | aprovar ao escolher a forma | evita venda finalizada sem recebimento comprovado |
| Provedor externo fora da transação | chamada TEF/Pix dentro do commit | evita transação longa e resultado externo ambíguo |
| PDV legado permanece desacoplado | trocar o WPF nesta fatia | mantém reversibilidade enquanto o núcleo transacional amadurece |

## Fatia 3 — Integração gradual do PDV

- cliente HTTP tipado;
- configuração por terminal;
- adaptador compatível com o ViewModel atual;
- estados de carregamento, timeout e reconexão;
- carrinho preservado quando o servidor fica indisponível;
- testes reais de teclado, scanner e dois processos.

## Fatia 4 — Retaguarda e operação

- cadastros essenciais;
- caixa e relatórios do proprietário;
- diagnóstico, logs e alertas;
- backup, verificação e restauração;
- instalador e serviços do Windows;
- piloto em um caixa e posterior liberação do segundo.

## Regras de engenharia

- nenhuma entidade EF será retornada pela API;
- nenhum PDV terá credencial do PostgreSQL;
- IDs técnicos serão UUID e datas serão UTC;
- dinheiro usará `numeric(18,2)` e quantidade `numeric(18,3)`;
- migrations serão versionadas e aplicadas pelo instalador/bundle;
- não será usado `EnsureCreated` para validar o schema;
- concorrência e migrações serão testadas em PostgreSQL real;
- cada fatia deve manter a suíte anterior verde;
- exclusões locais preexistentes ficam fora dos commits.

## Controle de risco da primeira fatia

O risco de dados é alto, mas está isolado do PDV em produção. A fatia só será conectada à interface depois que os testes de domínio, banco, idempotência e concorrência passarem. Não será criado endpoint de finalização antes de estoque, pagamento e caixa poderem participar da mesma unidade transacional.
