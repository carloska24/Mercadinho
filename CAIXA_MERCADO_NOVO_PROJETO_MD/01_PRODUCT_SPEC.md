# ESPECIFICAÇÃO DO PRODUTO

## Visão
O Caixa Mercado será um sistema de varejo com foco inicial em frente de caixa.

Deve ser:
- rápido;
- orientado a teclado;
- compatível com scanner;
- adequado para touchscreen;
- preparado para periféricos;
- preparado para fiscal;
- preparado para operação em rede.

## Fluxo principal
```text
Abrir aplicação
→ identificar operador
→ verificar caixa
→ abrir caixa se necessário
→ nova venda
→ EAN/PLU
→ produto
→ adicionar item
→ repetir
→ F9 pagamento
→ confirmar
→ finalizar venda
```

## Módulos
### Venda
Adicionar, quantidade, remover, desconto, cliente, consulta, cancelamento.

### Caixa
Abertura, sangria, suprimento, consulta e fechamento.

### Consultas
Produto, preço, estoque e venda.

### Produtos
Código, EAN, PLU, descrição, categoria, unidade, custo, venda, estoque, mínimo, pesável e status.

### Produto pesável
Suportar preço por kg + peso + total. Preparar futura integração com balança e códigos de balança.

### Pagamentos
Dinheiro, PIX, débito, crédito e futuramente TEF/pagamento misto. Não criar integrações falsas.

### Fiscal
Preparar arquitetura para NFC-e, impressão, cancelamento e contingência.

### Segurança
Operador, Supervisor e Administrador, com permissões centralizadas.

### Auditoria
Registrar descontos, cancelamentos, sangrias, suprimentos, autorizações e fechamento.
