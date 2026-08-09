# CAIXA MERCADO — MELHORIAS DO PDV E ÁREA DE ITENS

## 1. ENTENDIMENTO DA REFERÊNCIA

A ideia é que os produtos lançados apareçam com a sensação de um **cupom/nota de venda sendo montado em tempo real**, e não como uma tabela SaaS genérica.

Exemplo:

```text
VENDA Nº 1001

ITEM  CÓDIGO          DESCRIÇÃO              QTD    VL.UN     TOTAL
----------------------------------------------------------------------
001   7891234567890   ARROZ TIPO 1 5KG       1     22,90      22,90
002   7891234567891   FEIJÃO CARIOCA 1KG     2      8,50      17,00
003   7891234567892   LEITE INTEGRAL 1L      3      5,20      15,60
----------------------------------------------------------------------
                                                    TOTAL: R$ 55,50
```

Não é para desenhar uma nota fiscal literalmente. É para criar a **sensação de cupom eletrônico de supermercado**.

---

# 2. ÁREA DE ITENS

A área central deve continuar sendo o coração da venda.

```text
┌──────────────────────────────────────────────────────────────┐
│ ITENS REGISTRADOS NA VENDA                                   │
├─────┬───────────────┬──────────────────────┬─────┬───────────┤
│ ITEM│ CÓDIGO        │ DESCRIÇÃO            │ QTD │ TOTAL     │
├─────┼───────────────┼──────────────────────┼─────┼───────────┤
│ 001 │ 7891234567890 │ ARROZ TIPO 1 5KG     │  1  │ R$ 22,90  │
│     │               │ R$ 22,90 / UN        │     │           │
├─────┼───────────────┼──────────────────────┼─────┼───────────┤
│ 002 │ 7891234567891 │ FEIJÃO CARIOCA 1KG   │  2  │ R$ 17,00  │
│     │               │ R$ 8,50 / UN          │     │           │
└─────┴───────────────┴──────────────────────┴─────┴───────────┘
```

A tabela deve ter comportamento de **lista de cupom**, e não de tabela administrativa.

Colunas:

- Item;
- Código/EAN/PLU;
- Descrição;
- Quantidade;
- Unidade;
- Valor unitário;
- Desconto;
- Total.

---

# 3. COMPORTAMENTO DO SCANNER

Fluxo obrigatório:

```text
Scanner
  ↓
EAN
  ↓
Localizar produto
  ↓
Adicionar item
  ↓
Atualizar quantidade
  ↓
Atualizar subtotal
  ↓
Atualizar total
  ↓
Atualizar "Último item"
  ↓
Destacar item recém-adicionado
  ↓
Voltar foco para EAN
```

O operador não deve precisar usar o mouse depois de cada leitura.

---

# 4. ÚLTIMO ITEM ADICIONADO

Manter o painel atual.

Vazio:

```text
ÚLTIMO ITEM ADICIONADO

Nenhum item registrado
```

Após leitura:

```text
ÚLTIMO ITEM ADICIONADO

FEIJÃO CARIOCA 1KG

Código:
7891234567891

Quantidade:
2 UN

Valor unitário:
R$ 8,50

VALOR DO ITEM:
R$ 17,00
```

Esse painel serve para confirmação visual imediata.

---

# 5. DESTAQUE DO ÚLTIMO ITEM

A linha recém-adicionada pode receber um destaque discreto e temporário.

Não usar neon ou animações chamativas.

Objetivo:

> facilitar a conferência do operador.

---

# 6. PRODUTO REPETIDO

Comportamento recomendado para mercadinho:

```text
Feijão → QTD 1
Feijão → QTD 2
Feijão → QTD 3
```

Resultado:

```text
FEIJÃO CARIOCA 1KG
QTD: 3
VL.UN: R$ 8,50
TOTAL: R$ 25,50
```

Inicialmente, consolidar o mesmo produto na mesma linha.

---

# 7. QUANTIDADE

Atalho:

```text
F4 = Alterar quantidade
```

Fluxo:

```text
Selecionar item
↓
F4
↓
Digitar quantidade
↓
ENTER
↓
Recalcular
```

Para produtos pesáveis, aceitar casas decimais:

```text
0,250
1,250
1,750
```

---

# 8. PRODUTOS POR PESO

Exemplo:

```text
BANANA PRATA
PLU 001
R$ 4,50/kg
1,250 kg
TOTAL R$ 5,63
```

Na lista:

```text
001 | PLU 001 | BANANA PRATA | 1,250 KG | R$ 4,50/kg | R$ 5,63
```

Preparar futura integração com balança.

---

# 9. RESUMO DA VENDA

A lateral direita deve ser um painel financeiro, não um dashboard.

```text
┌──────────────────────────────┐
│ RESUMO DA VENDA              │
├──────────────────────────────┤
│ QUANTIDADE DE ITENS:      6  │
│                              │
│ SUBTOTAL:          R$ 55,50  │
│ DESCONTO:          R$  0,00  │
│                              │
│ CLIENTE: CONSUMIDOR          │
├──────────────────────────────┤
│ TOTAL A PAGAR                │
│                              │
│        R$ 55,50              │
│                              │
│      [ F9 PAGAMENTO ]        │
└──────────────────────────────┘
```

---

# 10. PAGAMENTO

Ao pressionar F9:

```text
┌─────────────────────────────────────────────────────────────┐
│                    FINALIZAR VENDA                          │
├─────────────────────────────────────────────────────────────┤
│ TOTAL DA VENDA:              R$ 139,90                      │
│                                                             │
│ [ F1 DINHEIRO ] [ F2 PIX ] [ F3 DÉBITO ] [ F4 CRÉDITO ]   │
│                                                             │
│ VALOR RECEBIDO:             R$ 150,00                      │
│                                                             │
│ TROCO:                       R$ 10,10                      │
│                                                             │
│                    [ CONFIRMAR ]                           │
└─────────────────────────────────────────────────────────────┘
```

A tela deve funcionar por teclado.

Não implementar TEF/PIX real como se já existisse.

---

# 11. ESTADOS DO PDV

Usar estados reais na barra inferior:

```text
CAIXA LIVRE — AGUARDANDO PRODUTO
VENDA EM ANDAMENTO
AGUARDANDO PAGAMENTO
PAGAMENTO EM PROCESSAMENTO
VENDA FINALIZADA
CAIXA FECHADO
CAIXA BLOQUEADO
SISTEMA OFFLINE
ERRO DE PERIFÉRICO
```

Evitar apenas:

```text
Sistema OK
```

---

# 12. ATALHOS

Manter:

```text
F2 Produto
F3 Cliente
F4 Quantidade
F6 Desconto
F7 Cancelar item
F8 Consultar
F9 Pagamento
ESC Cancelar venda
DEL Remover item
ENTER Confirmar
```

O operador deve conseguir trabalhar quase sem mouse.

---

# 13. DESCONTO

F6 deve abrir uma janela compacta:

```text
┌─────────────────────────────┐
│ APLICAR DESCONTO            │
├─────────────────────────────┤
│ ( ) R$    ( ) %             │
│                             │
│ Valor: [____________]       │
│                             │
│ [ CONFIRMAR ] [ CANCELAR ]  │
└─────────────────────────────┘
```

Descontos acima do limite podem exigir supervisor.

---

# 14. CANCELAMENTO

Diferenciar:

```text
DEL = cancelar/remover item
ESC = cancelar venda
```

Cancelar venda com itens deve pedir confirmação.

Operações sensíveis podem exigir supervisor.

---

# 15. CONSULTA DE PRODUTO

F2 deve permitir buscar por:

- código;
- EAN;
- PLU;
- descrição.

A consulta deve parecer uma janela operacional de ERP:

```text
┌──────────────────────────────────────────────────────────┐
│ CONSULTA DE PRODUTO                                     │
├──────────────────────────────────────────────────────────┤
│ Buscar: [ ARROZ________________ ] [ F2 BUSCAR ]         │
├────────┬──────────────────────┬───────────┬──────────────┤
│ Código │ Produto              │ Preço     │ Estoque      │
├────────┼──────────────────────┼───────────┼──────────────┤
│ 001    │ Arroz Tipo 1 5KG     │ R$ 22,90  │ 35 UN        │
│ 002    │ Arroz Integral 1KG   │ R$  8,90  │ 12 UN        │
└────────┴──────────────────────┴───────────┴──────────────┘
```

---

# 16. CABEÇALHO

Manter a ideia atual:

```text
CAIXA MERCADO | PDV VAREJO
FILIAL 01 — MATRIZ
PDV 01
OPERADOR: CARLOS EDUARDO
● CAIXA ABERTO
```

O cabeçalho deve informar contexto operacional.

---

# 17. BARRA LATERAL OPCIONAL

Se for necessária uma área de operações:

```text
┌───────────────┐
│ OPERAÇÕES     │
├───────────────┤
│ F2 Produto    │
│ F3 Cliente    │
│ F4 Quantidade │
│ F6 Desconto   │
│ F7 Canc. Item │
│ F8 Consulta   │
│ F9 Pagamento  │
├───────────────┤
│ CAIXA         │
├───────────────┤
│ Abertura      │
│ Sangria       │
│ Suprimento    │
│ Fechamento    │
└───────────────┘
```

Não deixar essa barra dominar a tela.

---

# 18. O QUE NÃO FAZER

Não voltar para aparência SaaS.

Evitar:

- cards de produtos na tela principal;
- dashboard;
- grandes espaços vazios;
- glassmorphism;
- neon;
- glow;
- gradientes;
- animações exageradas;
- excesso de arredondamento;
- imagens desnecessárias;
- menus gigantes.

A referência é:

> **software desktop empresarial de frente de caixa.**

---

# 19. TESTES FUNCIONAIS

## Produto

```text
EAN
→ ENTER
→ produto encontrado
→ item aparece
→ total atualiza
→ foco volta ao EAN
```

## Quantidade

```text
F4
→ nova quantidade
→ ENTER
→ total recalculado
```

## Produto repetido

```text
Feijão
Feijão
Feijão
```

Esperado:

```text
FEIJÃO CARIOCA 1KG
QTD 3
```

## Pagamento

```text
F9
→ forma
→ valor recebido
→ troco
→ confirmar
→ venda finalizada
```

## Cancelamento

Testar separadamente:

```text
DEL
ESC
```

---

# 20. ORDEM DE IMPLEMENTAÇÃO

Não redesenhar a aplicação inteira novamente.

Implementar nesta ordem:

```text
1. Congelar shell atual
        ↓
2. Melhorar lista para sensação de cupom
        ↓
3. Scanner / EAN
        ↓
4. Quantidade
        ↓
5. Produto repetido
        ↓
6. Último item
        ↓
7. Totais
        ↓
8. F6 Desconto
        ↓
9. DEL / ESC
        ↓
10. F9 Pagamento
        ↓
11. Finalização
        ↓
12. Caixa
        ↓
13. Produtos pesáveis
        ↓
14. Hardware
```

# 21. CRITÉRIO FINAL

Ao olhar somente para a tela, ela deve transmitir:

> "Este é um programa de frente de caixa instalado no computador de um supermercado."

E não:

> "Este é um sistema SaaS feito com cards."

A inspiração é a **experiência operacional de PDVs profissionais**, sem copiar visual, marca ou código de qualquer empresa.
