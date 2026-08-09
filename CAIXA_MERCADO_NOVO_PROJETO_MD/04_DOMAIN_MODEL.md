# DOMÍNIO E MODELO

## Entidades
```text
Filial
PDV
Operador
Usuario
Perfil
Permissao
Produto
Categoria
Fornecedor
Cliente
Venda
ItemVenda
Pagamento
Caixa
MovimentacaoCaixa
Sangria
Suprimento
FechamentoCaixa
Estoque
MovimentacaoEstoque
```

## Venda
Id, Numero, DataHora, Filial, PDV, Operador, Itens, Subtotal, Desconto, Acrescimo, Total e Status.

Status:
```text
EmAberto
AguardandoPagamento
Finalizada
Cancelada
```

## Item
Produto, Quantidade, Unidade, PrecoUnitario, Desconto e Total.

## Produto
Codigo, EAN, PLU, Descricao, Categoria, Unidade, PrecoCusto, PrecoVenda, Estoque, EstoqueMinimo, EstoqueMaximo, ControlaEstoque, ProdutoPesavel e Ativo.

## Caixa
Filial, PDV, Operador, DataAbertura, ValorInicial, DataFechamento, ValorFinal e Status.

## Pagamento
Venda, Tipo, Valor, Status e DataHora.

Tipos:
Dinheiro, PIX, Debito, Credito. Preparar extensão para TEF e pagamentos mistos.

## Regras
- venda não finaliza sem itens;
- quantidade > 0;
- produto inativo não pode ser vendido;
- caixa deve estar aberto;
- total não pode ser negativo;
- desconto deve respeitar política;
- operações críticas podem exigir autorização.

Regras pertencem ao domínio/application, não à interface.
