# CAIXA MERCADO — DIRETRIZ FINAL DE UI/UX DO PDV

> Refinar a tela atual. NÃO refazer o projeto, NÃO trocar a stack e NÃO transformar em SaaS.

## 1. OBJETIVO

O sistema deve parecer:

**um software desktop profissional de frente de caixa de supermercado instalado no Windows.**

Não deve parecer:
- SaaS
- dashboard
- e-commerce
- landing page
- aplicativo mobile
- sistema "dark tech"

A inspiração é a experiência operacional de PDVs/ERPs profissionais, sem copiar marca, layout proprietário ou código.

---

# 2. PRINCIPAL MUDANÇA — EMPTY STATE COM CARRINHO

Hoje, quando não há venda, o centro fica vazio. Isso passa sensação de tela incompleta.

Criar um Empty State:

```text
┌──────────────────────────────────────────────────────────────┐
│ VENDA EM ANDAMENTO                                    0 itens│
├──────────────────────────────────────────────────────────────┤
│                                                              │
│                         ╭────────╮                           │
│                        ╱          ╲                          │
│                       │     🛒     │                         │
│                        ╲          ╱                          │
│                         ╰────────╯                           │
│                           ○    ○                             │
│                                                              │
│                       CAIXA LIVRE                            │
│                                                              │
│             Aguardando leitura do produto                    │
│                                                              │
│                 F2 — Consultar produto                       │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

**Não implementar o emoji.**

Criar uma ilustração SVG/vetorial de carrinho de supermercado:
- simples;
- profissional;
- monocromática;
- discreta;
- baixa intensidade;
- sem fotografia;
- sem desenho infantil;
- sem neon;
- sem gradiente;
- sem glow.

O carrinho existe somente como **estado vazio**.

---

# 3. COMPORTAMENTO DO EMPTY STATE

### Caixa sem venda

```text
VENDA EM ANDAMENTO                         0 itens

                  [ ÍCONE CARRINHO ]

                    CAIXA LIVRE

            Aguardando leitura do produto

                 F2 — Consultar produto
```

### Primeiro produto lançado

```text
EMPTY STATE
     ↓
DESAPARECE
     ↓
GRID DA VENDA APARECE
```

Não manter o carrinho junto com os produtos.

---

# 4. LISTA DE PRODUTOS — SENSAÇÃO DE CUPOM

A área central deve parecer um **cupom da venda sendo construído em tempo real**, e não uma tabela administrativa.

```text
┌──────────────────────────────────────────────────────────────┐
│ VENDA EM ANDAMENTO                                    3 itens│
├─────┬───────────────┬──────────────────────┬─────┬───────────┤
│ #   │ CÓDIGO        │ DESCRIÇÃO            │ QTD │ TOTAL     │
├─────┼───────────────┼──────────────────────┼─────┼───────────┤
│ 001 │ 7891234567890 │ ARROZ TIPO 1 5KG     │  1  │ R$ 22,90  │
│ 002 │ 7891234567891 │ FEIJÃO CARIOCA 1KG   │  2  │ R$ 17,00  │
│ 003 │ 7891234567892 │ LEITE INTEGRAL 1L    │  3  │ R$ 15,60  │
└─────┴───────────────┴──────────────────────┴─────┴───────────┘
```

Não é para desenhar uma nota fiscal literalmente. É para transmitir a sensação de **cupom eletrônico em tempo real**.

Trocar:

`ITENS REGISTRADOS NA VENDA (CUPOM EM TEMPO REAL)`

por:

**VENDA EM ANDAMENTO**

Exemplos:

```text
VENDA EM ANDAMENTO                         0 itens
VENDA EM ANDAMENTO                         1 item
VENDA EM ANDAMENTO                         5 itens
```

---

# 5. TEMA / PALETA

O tema atual está bom, mas um pouco "dark tech".

A direção final é:

**ERP/PDV corporativo + desktop Windows + varejo.**

Sugestão inicial:

```text
Fundo geral:          #182235
Painéis:              #202C40
Área da venda:        #10192B
Cabeçalhos:           #25334A
Bordas:               #33445C
Campo de entrada:     #080F1E
```

Criar tokens/ResourceDictionary centralizados. Não espalhar hexadecimais pelo XAML.

---

# 6. CORES SEMÂNTICAS

```text
VERDE
→ sucesso
→ caixa aberto
→ pagamento
→ total

AZUL
→ ação
→ consulta
→ seleção
→ navegação

AMARELO
→ atenção
→ desconto
→ aviso

VERMELHO
→ cancelamento
→ erro
→ bloqueio
```

Não usar todas as cores como decoração.

O verde não deve dominar a interface.

---

# 7. COMPONENTE EMPTY SALE STATE

Criar componente reutilizável:

```text
EmptySaleState
├── Icon
├── Title
├── Description
└── ShortcutHint
```

Conteúdo:

```text
Ícone: carrinho de supermercado
Título: CAIXA LIVRE
Descrição: Aguardando leitura do produto
Atalho: F2 — Consultar produto
```

Criar recurso semelhante a:

```text
EmptySaleState.xaml
```

---

# 8. NÃO USAR FOTOGRAFIA

Não colocar:
- foto de carrinho;
- foto de supermercado;
- foto de produtos;
- banner;
- publicidade.

Foto faria parecer e-commerce.

Usar somente ilustração vetorial simples.

---

# 9. ÚLTIMO ITEM ADICIONADO

Manter o painel atual.

Vazio:

```text
┌──────────────────────────────┐
│ ÚLTIMO ITEM ADICIONADO       │
│                              │
│ Nenhum item registrado       │
│                              │
│ VALOR DO ITEM:       R$ 0,00 │
└──────────────────────────────┘
```

Com item:

```text
┌──────────────────────────────┐
│ ÚLTIMO ITEM ADICIONADO       │
│                              │
│ FEIJÃO CARIOCA 1KG           │
│                              │
│ Código: 7891234567891        │
│ Quantidade: 2 UN             │
│ Unitário: R$ 8,50            │
│                              │
│ VALOR DO ITEM:       R$17,00 │
└──────────────────────────────┘
```

---

# 10. DESTAQUE DO ÚLTIMO PRODUTO

Ao adicionar uma linha:

```text
GRID
↓
nova linha
↓
destaque discreto
↓
operador confere
↓
estado normal
```

Não usar animações chamativas.

---

# 11. FLUXO DO SCANNER

```text
┌──────────────┐
│ SCANNER /    │
│ EAN / PLU    │
└──────┬───────┘
       ↓
┌──────────────┐
│ LOCALIZAR    │
│ PRODUTO      │
└──────┬───────┘
       ↓
┌──────────────┐
│ ADICIONAR    │
│ ITEM         │
└──────┬───────┘
       ↓
┌──────────────┐
│ RECALCULAR   │
│ TOTAIS       │
└──────┬───────┘
       ↓
┌──────────────┐
│ ATUALIZAR    │
│ ÚLTIMO ITEM  │
└──────┬───────┘
       ↓
┌──────────────┐
│ RETORNAR     │
│ FOCO AO EAN  │
└──────────────┘
```

O operador deve conseguir trabalhar praticamente sem mouse.

---

# 12. PRODUTO REPETIDO

Regra inicial:

```text
FEIJÃO → QTD 1
FEIJÃO → QTD 2
FEIJÃO → QTD 3
```

Resultado:

```text
FEIJÃO CARIOCA 1KG
QTD: 3
VL.UN: R$ 8,50
TOTAL: R$ 25,50
```

Consolidar o mesmo produto na mesma linha inicialmente.

---

# 13. PRODUTO PESÁVEL

Exemplo:

```text
BANANA PRATA
PLU 001
R$ 4,50 / KG
1,250 KG
TOTAL R$ 5,63
```

Aceitar quantidades decimais:

```text
0,250
1,250
1,750
2,500
```

Preparar arquitetura para futura balança.

---

# 14. RESUMO DA VENDA

A lateral direita deve parecer painel financeiro de PDV:

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
├──────────────────────────────┤
│      [ F9 PAGAMENTO ]        │
└──────────────────────────────┘
```

Evitar aparência de dashboard.

---

# 15. PAGAMENTO

F9:

```text
┌─────────────────────────────────────────────────────────────┐
│                    FINALIZAR VENDA                          │
├─────────────────────────────────────────────────────────────┤
│ TOTAL DA VENDA:              R$ 139,90                      │
│                                                             │
│ [ F1 DINHEIRO ] [ F2 PIX ] [ F3 DÉBITO ] [ F4 CRÉDITO ]   │
│                                                             │
│ VALOR RECEBIDO:              R$ 150,00                      │
│ TROCO:                        R$ 10,10                      │
│                                                             │
│                    [ CONFIRMAR ]                            │
└─────────────────────────────────────────────────────────────┘
```

A tela deve ser operável por teclado.

---

# 16. ATALHOS

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

A interface deve ser keyboard-first.

---

# 17. ESTADOS OPERACIONAIS

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

Evitar apenas "Sistema OK".

---

# 18. CABEÇALHO

Manter:

```text
CAIXA MERCADO | PDV VAREJO

FILIAL 01 — MATRIZ
PDV 01
OPERADOR: CARLOS EDUARDO
● CAIXA ABERTO
```

Compacto e operacional.

---

# 19. BARRA DE OPERAÇÕES

Se houver barra lateral:

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

Só adicionar se não prejudicar a área de venda.

---

# 20. O QUE NÃO FAZER

```text
❌ SaaS
❌ Dashboard
❌ E-commerce
❌ Landing page
❌ Cards gigantes
❌ Gradientes
❌ Glassmorphism
❌ Glow
❌ Neon
❌ Fotos
❌ Banners
❌ Animações chamativas
❌ Excesso de arredondamento
❌ Espaços vazios desnecessários
❌ Ícones decorativos sem função
```

---

# 21. ESTRUTURA FINAL

### Caixa vazio

```text
┌───────────────────────────────────────────────────────────────────────┐
│ CAIXA MERCADO | FILIAL 01 | PDV 01 | OPERADOR | ● CAIXA ABERTO      │
├───────────────────────────────────────────────────────────────────────┤
│ QTD     │ CÓDIGO DE BARRAS / EAN / PLU               │ ENTER         │
├──────────────────────────────────────────────────────┬────────────────┤
│ VENDA EM ANDAMENTO                              0     │ ÚLTIMO ITEM    │
│                                                      │                │
│                         🛒                           │ Nenhum item    │
│                                                      ├────────────────┤
│                    CAIXA LIVRE                       │ RESUMO         │
│                                                      │                │
│            Aguardando leitura do produto             │ Subtotal       │
│                                                      │ Desconto       │
│                 F2 Consultar produto                │ Cliente        │
│                                                      ├────────────────┤
│                                                      │ TOTAL          │
│                                                      │ R$ 0,00        │
│                                                      │                │
│                                                      │ F9 PAGAMENTO    │
├──────────────────────────────────────────────────────┴────────────────┤
│ F2 Produto │ F3 Cliente │ F4 Qtd │ F6 Desconto │ F9 Pagamento │ ESC │
├───────────────────────────────────────────────────────────────────────┤
│ VENDA Nº 1001 | CAIXA LIVRE — AGUARDANDO PRODUTO | ONLINE            │
└───────────────────────────────────────────────────────────────────────┘
```

### Venda iniciada

```text
┌───────────────────────────────────────────────────────────────────────┐
│ CAIXA MERCADO | FILIAL 01 | PDV 01 | OPERADOR | ● CAIXA ABERTO      │
├───────────────────────────────────────────────────────────────────────┤
│ QTD     │ CÓDIGO DE BARRAS / EAN / PLU               │ ENTER         │
├──────────────────────────────────────────────────────┬────────────────┤
│ VENDA EM ANDAMENTO                              3     │ ÚLTIMO ITEM    │
├─────┬───────────────┬──────────────────────┬─────────┤                │
│ #   │ CÓDIGO        │ DESCRIÇÃO            │ TOTAL   │ FEIJÃO         │
├─────┼───────────────┼──────────────────────┼─────────┤                │
│ 001 │ 789...        │ ARROZ 5KG            │ 22,90   │ R$ 17,00       │
│ 002 │ 789...        │ FEIJÃO 1KG           │ 17,00   ├────────────────┤
│ 003 │ 789...        │ LEITE 1L             │ 15,60   │ RESUMO         │
│     │               │                      │         │                │
│     │               │                      │         │ Subtotal       │
│     │               │                      │         │ Desconto       │
│     │               │                      │         │ Cliente        │
│     │               │                      │         ├────────────────┤
│     │               │                      │         │ TOTAL          │
│     │               │                      │         │ R$ 55,50       │
│     │               │                      │         │                │
│     │               │                      │         │ F9 PAGAMENTO    │
├─────┴───────────────┴──────────────────────┴─────────┴────────────────┤
│ F2 Produto │ F3 Cliente │ F4 Qtd │ F6 Desconto │ F9 Pagamento │ ESC │
├───────────────────────────────────────────────────────────────────────┤
│ VENDA Nº 1001 | VENDA EM ANDAMENTO | ONLINE                          │
└───────────────────────────────────────────────────────────────────────┘
```

**Observação:** nos mockups acima o carrinho aparece como `🛒` somente para representar a posição. A implementação final deve usar SVG/vetor.

---

# 22. WPF

Criar recursos centralizados:

```text
Resources/
├── Colors.xaml
├── Typography.xaml
├── Controls.xaml
├── DataGrid.xaml
├── Buttons.xaml
└── EmptyState.xaml
```

Não espalhar estilos pelo projeto.

O Empty State deve ser componente reutilizável.

---

# 23. ARQUITETURA

Não colocar regra de venda na View.

```text
View
 ↓
ViewModel
 ↓
Application
 ↓
Domain
 ↓
Infrastructure
```

O Empty State deve depender do estado real da venda:

```text
Venda.Items.Count == 0
```

e não de uma variável visual independente.

---

# 24. TESTES VISUAIS

## Caixa vazio

```text
[ ] Carrinho aparece
[ ] Carrinho é SVG/vetor
[ ] CAIXA LIVRE aparece
[ ] F2 aparece
[ ] Não existe espaço morto
[ ] Não há foto
[ ] Não há emoji na implementação
```

## Primeiro produto

```text
[ ] Carrinho desaparece
[ ] DataGrid aparece
[ ] Produto aparece
[ ] Último item atualiza
[ ] Total atualiza
[ ] Foco volta ao EAN
```

## Vários produtos

```text
[ ] Lista parece cupom
[ ] Produtos legíveis
[ ] Total sempre visível
[ ] Último item sempre visível
[ ] Não parece dashboard
```

---

# 25. ORDEM DE EXECUÇÃO

O Antigravity deve executar nesta ordem:

```text
1. PRESERVAR arquitetura atual
        ↓
2. PRESERVAR funcionalidades existentes
        ↓
3. Criar sistema de cores centralizado
        ↓
4. Ajustar tema para ERP/PDV corporativo
        ↓
5. Criar EmptySaleState
        ↓
6. Criar ilustração SVG de carrinho
        ↓
7. Implementar Empty State ↔ Venda
        ↓
8. Refinar DataGrid
        ↓
9. Refinar último item
        ↓
10. Refinar resumo
        ↓
11. Refinar estados
        ↓
12. Validar teclado
        ↓
13. Validar scanner
        ↓
14. Validar pagamento
```

---

# 26. REGRA ABSOLUTA

**NÃO REFAZER O PROJETO.**

**NÃO TROCAR A STACK.**

**NÃO CRIAR OUTRO LAYOUT DO ZERO.**

A estrutura atual já está correta.

Esta etapa existe para dar:

```text
DESKTOP
   +
ERP
   +
PDV DE SUPERMERCADO
   +
CUPOM EM TEMPO REAL
   +
EMPTY STATE COM CARRINHO
   +
TECLADO / SCANNER
   =
CAIXA MERCADO
```

O objetivo final é:

> **"Se eu instalar isso em um computador de um mercadinho, parece um sistema de frente de caixa profissional."**

Se não parecer, corrigir primeiro densidade, hierarquia, espaçamento, informação, teclado, feedback e estados — e não adicionar efeitos.
