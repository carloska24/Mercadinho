# BANCO DE DADOS

## Banco
PostgreSQL.

## ORM
Entity Framework Core.

## Princípios
- migrations;
- índices;
- foreign keys;
- constraints;
- transações;
- auditoria;
- timestamps;
- concorrência quando necessário.

## Tabelas iniciais
```text
filiais
pdvs
usuarios
perfis
permissoes
operadores
categorias
produtos
clientes
fornecedores
caixas
movimentacoes_caixa
vendas
itens_venda
pagamentos
estoques
movimentacoes_estoque
```

## Índices
Produto.Codigo, Produto.EAN, Produto.PLU, Produto.Descricao, Venda.Numero, Venda.DataHora, Venda.PDV e Venda.Operador.

## Valores
Usar `decimal` para dinheiro. Não usar `double`.

Quantidade deve aceitar casas decimais para produtos pesáveis.

## Finalização
A venda deve ser transacional:
```text
BEGIN
→ validar
→ registrar venda
→ registrar itens
→ registrar pagamento
→ movimentar estoque
→ COMMIT
```

Erro deve resultar em rollback.
