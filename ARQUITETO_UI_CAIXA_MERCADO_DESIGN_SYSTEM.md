# CAIXA MERCADO — DIRETRIZ DE ARQUITETURA VISUAL E UI
## Refinamento dos Ícones, Atalhos, Estados Animados e Design System do PDV

> **Papel deste documento:** agir como uma especificação de arquitetura de UI/UX para o Antigravity.
>
> Não quero apenas "trocar os ícones".
> Quero estabelecer uma **biblioteca visual consistente**, componentes reutilizáveis, regras de estados e uma arquitetura que permita evoluir o PDV sem criar uma coleção de elementos genéricos.

---

# 1. DIAGNÓSTICO DA INTERFACE ATUAL

A estrutura geral da tela está boa.

Porém, a barra inferior atualmente apresenta este problema:

```text
[F2] 🛒 Produto
[F3] 👤 Cliente
[F4] ▣ Qtd
[F6] 🏷 Desconto
[F7] × Canc Item
[F8] 🔍 Consultar
[F9] ▣ Pagamento
[ESC] ⊘ Cancelar
[DEL] 🗑 Remover
```

Os ícones funcionam semanticamente, mas visualmente parecem:

- genéricos;
- pequenos;
- pouco integrados;
- sem uma linguagem visual forte;
- com pouca hierarquia;
- mais próximos de "ícones colocados em botões" do que de um Design System.

## Objetivo

A barra inferior deve parecer parte de um **produto profissional de PDV**, não de uma coleção de botões independentes.

---

# 2. DECISÃO ARQUITETURAL — UMA ÚNICA FAMÍLIA DE ÍCONES

## Biblioteca principal recomendada

### Phosphor Icons

Usar:

```text
@phosphor-icons/react
```

Documentação:

https://phosphoricons.com/

Pacote:

```bash
npm install @phosphor-icons/react
```

## Por que Phosphor?

Porque precisamos de uma biblioteca que permita trabalhar com diferentes pesos sem trocar de família visual.

O Phosphor oferece:

```text
thin
light
regular
bold
fill
duotone
```

Isso é extremamente útil para este projeto.

### Regra

Não misturar:

```text
Lucide + Font Awesome + Material + SVG aleatório
```

na mesma camada visual.

A barra de atalhos deve utilizar **uma única família principal**.

---

# 3. PESO DOS ÍCONES

Para a barra inferior:

### Preferência

```text
weight="bold"
```

ou:

```text
weight="duotone"
```

dependendo do ícone.

Não utilizar `thin`.

Não utilizar ícones extremamente finos.

O operador precisa enxergar rapidamente os atalhos.

### Tamanho

Desktop:

```text
18px – 22px
```

Preferência:

```text
20px
```

Para funções críticas:

```text
20px – 22px
```

---

# 4. MAPEAMENTO OFICIAL DOS ÍCONES

Usar Phosphor como fonte.

## F2 — Produto

Ícone:

```text
ShoppingCart
```

Não usar apenas `MagnifyingGlass`.

Motivo:

F2 representa a consulta/seleção de produtos e o próprio domínio do PDV é representado pelo carrinho.

Visual:

```text
[ 🛒 ] [F2] Produto
```

---

## F3 — Cliente

Ícone:

```text
UserCircle
```

Preferir `UserCircle` em vez de apenas `User`.

Motivo:

A ação representa identificação/seleção do cliente.

---

## F4 — Quantidade

Ícone:

```text
ListNumbers
```

Alternativa:

```text
Stack
```

Preferência:

```text
ListNumbers
```

Porque comunica melhor quantidade/itens.

---

## F6 — Desconto

Ícone:

```text
Tag
```

Alternativa:

```text
Percent
```

Preferência:

```text
Tag
```

O `Tag` representa uma operação comercial.

Se a interface mostrar explicitamente percentual, pode utilizar:

```text
Percent
```

---

## F7 — Cancelar Item

Ícone:

```text
XCircle
```

Não usar somente:

```text
X
```

Motivo:

`X` sozinho é genérico demais.

`XCircle` comunica uma ação destrutiva/cancelamento.

---

## F8 — Consultar

Ícone:

```text
MagnifyingGlass
```

Esse é um dos poucos casos em que o ícone é extremamente óbvio e deve continuar.

---

## F9 — Pagamento

Ícone:

```text
CreditCard
```

Alternativa:

```text
Wallet
```

Preferência:

```text
CreditCard
```

O cartão é imediatamente reconhecível em um PDV.

---

## ESC — Cancelar Venda

Ícone:

```text
Prohibit
```

Alternativa:

```text
XCircle
```

Preferência:

```text
Prohibit
```

Porque ESC cancela a operação/venda, não apenas um elemento.

---

## DEL — Remover

Ícone:

```text
Trash
```

Preferir:

```text
Trash
```

ou:

```text
TrashSimple
```

Não utilizar ícones muito detalhados.

---

# 5. REGRA IMPORTANTE — NÃO USAR EMOJI

Não usar:

```text
🛒
👤
🔍
💳
🗑
```

na interface.

Emoji dependem do sistema operacional e podem mudar drasticamente de aparência.

Usar somente:

```text
Phosphor Icons
SVG
```

---

# 6. COMPONENTE ShortcutButton

Criar um componente reutilizável.

Conceito:

```text
ShortcutButton
```

Props conceituais:

```text
icon
shortcut
label
variant
disabled
active
onClick
```

Exemplo:

```text
<ShortcutButton
    icon={ShoppingCart}
    shortcut="F2"
    label="Produto"
/>
```

Não criar nove componentes diferentes.

Criar:

```text
ShortcutButton
```

e reutilizar.

---

# 7. ESTRUTURA VISUAL DO BOTÃO

Quero abandonar a aparência atual de:

```text
[F2] 🛒 Produto
```

e trabalhar com:

```text
┌─────────────────────────┐
│                         │
│   🛒    F2              │
│         Produto         │
│                         │
└─────────────────────────┘
```

Porém, como a barra precisa economizar espaço, a implementação final pode ser:

```text
┌─────────────────────────┐
│  🛒   [F2]              │
│       Produto            │
└─────────────────────────┘
```

A decisão final deve considerar a largura real da janela.

---

# 8. HIERARQUIA DO BOTÃO

A prioridade visual deve ser:

```text
1. ÍCONE
2. TECLA
3. FUNÇÃO
```

A tecla deve ser facilmente identificável.

Exemplo:

```text
       ┌────┐
 🛒    │ F2 │
       └────┘
      Produto
```

---

# 9. DESIGN DA TECLA

A tecla deve parecer uma pequena tecla física, sem parecer skeuomorphism antigo.

Exemplo:

```text
╭──────╮
│  F2  │
╰──────╯
```

Características:

- fundo ligeiramente diferente do botão;
- borda discreta;
- radius pequeno;
- fonte monoespaçada ou semibold;
- contraste alto;
- altura aproximada de 20–24 px;
- padding horizontal de 5–7 px.

Sugestão:

```text
font-size: 10px–11px
font-weight: 700
letter-spacing: 0.02em
```

---

# 10. BOTÃO COMPLETO

Sugestão visual:

```text
┌──────────────────────────┐
│  🛒    ┌────┐             │
│        │ F2 │             │
│        └────┘             │
│        Produto            │
└──────────────────────────┘
```

Ou, se a altura da barra não comportar duas linhas:

```text
┌──────────────────────────┐
│ 🛒   [F2]   Produto      │
└──────────────────────────┘
```

A segunda opção deve ser usada se a barra tiver altura limitada.

---

# 11. COR DOS ÍCONES

Não dar uma cor diferente para cada botão.

Isso deixa o PDV com aparência de dashboard.

Usar cor semântica apenas quando fizer sentido.

## Normal

```text
ícone: azul claro / ciano
```

## Pagamento

```text
ícone: verde
```

## Desconto

```text
ícone: amarelo
```

## Ações destrutivas

```text
ícone: vermelho
```

## Neutras

```text
ícone: azul/cinza
```

---

# 12. PALETA SEMÂNTICA

Criar tokens.

Conceito:

```text
--color-action
--color-success
--color-warning
--color-danger
--color-muted
--color-surface
--color-border
--color-text
```

Não espalhar hexadecimais pelo código.

Exemplo:

```css
color: var(--color-action);
```

e não:

```css
color: #1e90ff;
```

em dezenas de componentes.

---

# 13. ESTADOS DOS SHORTCUTS

Cada `ShortcutButton` deve possuir:

```text
default
hover
focus-visible
pressed
disabled
```

## Default

Discreto.

## Hover

Aumentar ligeiramente:

- border;
- background;
- brilho do ícone.

Não aumentar o tamanho do botão.

## Pressed

Pequena mudança de superfície.

Pode utilizar:

```text
transform: translateY(1px)
```

## Focus

Muito importante para teclado.

Mostrar foco visível.

## Disabled

Reduzir:

- opacity;
- contraste;
- saturação.

Mas não deixar completamente invisível.

---

# 14. TECLADO É PRIORIDADE

A aplicação é um PDV.

Portanto:

```text
F2
F3
F4
F6
F7
F8
F9
ESC
DEL
```

devem continuar funcionando independentemente da interação com mouse.

A barra inferior é uma representação visual dos atalhos.

Não transformar a aplicação em uma interface mouse-first.

---

# 15. BARRA RESPONSIVA

A barra deve ocupar a largura disponível.

Conceito:

```text
┌──────┬──────┬──────┬──────┬──────┬──────┬──────┬──────┬──────┐
│ F2   │ F3   │ F4   │ F6   │ F7   │ F8   │ F9   │ ESC  │ DEL  │
└──────┴──────┴──────┴──────┴──────┴──────┴──────┴──────┴──────┘
```

Todos devem ter:

```text
flex: 1
```

ou grid equivalente.

Nenhum botão deve parecer espremido.

---

# 16. TECLAS ESPECIAIS

Para:

```text
ESC
DEL
```

não usar exatamente o mesmo tratamento das teclas F.

Visualmente:

```text
[F2]
[F3]
[F4]
```

e:

```text
[ESC]
[DEL]
```

podem ter largura ligeiramente maior.

---

# 17. AGRUPAMENTO VISUAL

Se houver espaço, considerar pequenos grupos:

```text
┌─────────────── FUNÇÕES ───────────────┐
│ F2 │ F3 │ F4 │ F6 │ F7 │ F8 │ F9 │
└───────────────────────────────────────┘

┌──── OPERAÇÃO ────┐
│ ESC │ DEL        │
└──────────────────┘
```

Mas isso só deve ser usado se não prejudicar a simplicidade.

Não criar separadores visuais excessivos.

---

# 18. BIBLIOTECA PARA ANIMAÇÕES

Para o carrinho e estados de venda:

## Se o projeto for React

Preferir:

```text
motion
```

Pacote:

```bash
npm install motion
```

Documentação:

https://motion.dev/

Usar para:

- entrada do carrinho;
- saída do carrinho;
- transição de estados;
- check de pagamento;
- mudança do Empty State.

---

# 19. QUANDO NÃO USAR MOTION

Não utilizar uma biblioteca de animação para tudo.

Para:

- hover;
- pressed;
- focus;
- pequenos efeitos;
- pulse;
- opacity;

preferir CSS.

Usar `motion` apenas para animações de estado.

---

# 20. ARQUITETURA DE ANIMAÇÃO

Separar:

```text
UI Animation
```

de:

```text
Business Logic
```

A lógica da venda não deve depender diretamente da animação.

Errado:

```text
finalizarVenda()
    → esperar animação
    → salvar venda
```

Correto:

```text
finalizarVenda()
    ↓
venda confirmada
    ↓
estado = COMPLETED
    ↓
UI executa animação
```

A animação é consequência do estado.

---

# 21. SISTEMA DE ESTADOS DO PDV

Criar/usar estados conceituais:

```text
IDLE
SALE
PAYMENT
COMPLETED
```

## IDLE

```text
Carrinho animado
CAIXA LIVRE
Aguardando leitura
```

## SALE

```text
Lista de produtos
Último produto
Subtotal
Total
```

## PAYMENT

```text
Pagamento
Forma de pagamento
Total
```

## COMPLETED

```text
Check
Venda concluída
Carrinho partindo
```

---

# 22. CARRINHO PRINCIPAL

O carrinho central deve utilizar um SVG de maior qualidade.

Biblioteca principal:

```text
Phosphor Icons
```

Ícone:

```text
ShoppingCart
```

Mas existe uma exceção:

## Para o carrinho animado principal

É permitido criar um **SVG customizado** baseado na linguagem do Phosphor.

Motivo:

O carrinho central é um elemento de identidade do produto.

Ele pode ter:

- rodas;
- pequenos detalhes;
- elementos de compra;
- glow;
- linhas de movimento;
- partículas.

Não usar o mesmo SVG simples da barra inferior.

### Portanto:

```text
ShoppingCart da barra
≠
Carrinho Hero do Empty State
```

Isso é importante.

---

# 23. POR QUE NÃO USAR O MESMO ÍCONE EM TODO LUGAR?

Porque:

```text
ícone pequeno = função
ícone grande = identidade
```

O carrinho dos atalhos precisa ser rápido de reconhecer.

O carrinho central precisa transmitir:

> "Este é o estado do Caixa Mercado."

---

# 24. ANIMAÇÃO DO CARRINHO IDLE

Implementar com:

```text
opacity
transform
scale
```

Evitar alterar:

```text
width
height
top
left
margin
```

durante a animação.

Isso reduz problemas de layout.

---

# 25. ANIMAÇÃO DE VENDA CONCLUÍDA

Sequência:

```text
Pagamento confirmado
        ↓
Check
        ↓
VENDA CONCLUÍDA
        ↓
Carrinho aparece
        ↓
Carrinho se move para direita
        ↓
Fade
        ↓
Novo estado IDLE
```

O tempo total não deve ser suficiente para atrasar o próximo atendimento.

---

# 26. IDEIA EXTRA — CARRINHO COM "RABO DE MOVIMENTO"

Na saída:

```text
       🛒
        ─────
          ─────
            ─────→
```

Pode ser feito com:

- linhas SVG;
- pseudo-elements;
- opacity;
- blur extremamente discreto.

Não usar GIF.

---

# 27. IDEIA EXTRA — MICROFEEDBACK DO SCANNER

Quando o produto for lido:

```text
scanner
   ↓
pequeno flash na área central
   ↓
produto aparece
```

Pode ser apenas:

```text
border-color
box-shadow
opacity
```

durante ~150–250ms.

Muito sutil.

---

# 28. EMPTY STATE — NÃO USAR UMA IMAGEM ESTÁTICA GENÉRICA

Não utilizar:

```text
imagem de carrinho de banco de imagens
```

O ideal é:

```text
SVG
+
animação
+
estado
```

Assim o visual pertence ao sistema.

---

# 29. SISTEMA DE ÍCONES — REGRA DE OURO

Dentro da aplicação:

```text
Phosphor
        ↓
ícones operacionais
```

```text
SVG customizado
        ↓
hero / empty states / animações especiais
```

Não usar:

```text
emoji
```

Não misturar:

```text
Material
Font Awesome
Lucide
Phosphor
SVG aleatório
```

na mesma área.

---

# 30. POSSIBILIDADE FUTURA — ICONIFY

Não adicionar agora se não houver necessidade.

Mas manter a arquitetura preparada para uma camada de abstração.

Se no futuro precisarmos de um ícone que não exista no Phosphor, considerar:

```text
Iconify
```

Pacote:

```bash
npm install @iconify/react
```

Porém:

> **Não utilizar Iconify como biblioteca principal do PDV neste momento.**

Motivo:

Uma biblioteca única facilita consistência visual.

Iconify deve ser uma exceção controlada.

---

# 31. DESIGN SYSTEM

Criar tokens para:

```text
spacing
radius
font
color
icon-size
border
shadow
motion
```

Exemplo conceitual:

```text
spacing-xs
spacing-sm
spacing-md
spacing-lg

radius-sm
radius-md

icon-sm
icon-md
icon-lg

motion-fast
motion-normal
motion-slow
```

---

# 32. TIPOGRAFIA

Se o projeto ainda não tiver uma fonte definida:

Preferência:

```text
Inter
```

Pacote:

```bash
npm install @fontsource/inter
```

Usar para:

- labels;
- botões;
- totais;
- cabeçalho;
- status.

Para a tecla:

```text
F2
F3
F9
ESC
DEL
```

usar peso semibold/bold.

---

# 33. TOTAL A PAGAR

Não alterar a filosofia atual.

Ele deve continuar sendo o maior número da lateral.

Exemplo:

```text
TOTAL A PAGAR

R$ 87,40
```

O valor deve ter:

```text
font-weight: 800
```

ou equivalente.

Não usar animações grandes.

---

# 34. PAINEL "ÚLTIMO ITEM"

Quando um item for adicionado:

```text
fade-in
+
pequeno slide
```

Exemplo:

```text
        ↓
Arroz 5kg
R$ 29,90
```

O painel deve atualizar rapidamente.

---

# 35. NÃO FAZER

Não adicionar:

- sombras exageradas;
- glassmorphism pesado;
- neon em tudo;
- gradientes em todos os componentes;
- animações contínuas em todos os elementos;
- ícones gigantes nos atalhos;
- múltiplas famílias de ícones;
- imagens de banco;
- GIFs;
- vídeos;
- partículas excessivas.

O sistema é um **PDV profissional**.

---

# 36. CRITÉRIO VISUAL

Ao olhar para a interface, a sensação desejada é:

```text
████████████████████████████
      SOFTWARE PROFISSIONAL
████████████████████████████
```

Não:

```text
████████████████████████████
      TEMPLATE DE DASHBOARD
████████████████████████████
```

---

# 37. CRITÉRIO DE QUALIDADE DOS ÍCONES

Antes de aceitar qualquer ícone, verificar:

### 1. Reconhecimento

O operador entende em menos de 1 segundo?

### 2. Consistência

Ele pertence à mesma família dos outros?

### 3. Peso

Está visível na resolução real?

### 4. Semântica

Representa corretamente a função?

### 5. Estado

Consegue mudar visualmente para disabled/active?

### 6. Escala

Continua bom em 18–22px?

---

# 38. TESTE VISUAL

Depois da implementação, analisar a tela inteira.

Perguntar:

```text
Os atalhos parecem uma única barra?
```

```text
As teclas F2/F3/F4/etc. são identificadas rapidamente?
```

```text
Os ícones parecem pertencer ao mesmo sistema?
```

```text
O carrinho central parece especial?
```

```text
As animações chamam atenção sem distrair?
```

```text
A tela continua parecendo um PDV?
```

---

# 39. ORDEM DE IMPLEMENTAÇÃO

Não alterar tudo simultaneamente.

Implementar nesta ordem:

## FASE 1

Biblioteca:

```text
@phosphor-icons/react
```

Mapear os atalhos.

---

## FASE 2

Criar:

```text
ShortcutButton
ShortcutBar
```

---

## FASE 3

Melhorar:

```text
F2
F3
F4
F6
F7
F8
F9
ESC
DEL
```

---

## FASE 4

Implementar:

```text
IDLE
SALE
PAYMENT
COMPLETED
```

---

## FASE 5

Carrinho central customizado.

---

## FASE 6

Animações:

```text
idle
enter
exit
completed
```

---

## FASE 7

Refinamento:

- spacing;
- tipografia;
- cores;
- bordas;
- estados;
- acessibilidade.

---

# 40. RESULTADO ESPERADO

A barra inferior deve terminar conceitualmente assim:

```text
┌────────────┬────────────┬────────────┬────────────┬────────────┐
│            │            │            │            │            │
│  🛒 [F2]   │ 👤 [F3]    │ ≡ [F4]     │ 🏷 [F6]    │ ⓧ [F7]    │
│  Produto   │ Cliente    │ Quantidade │ Desconto   │ Canc Item  │
│            │            │            │            │            │
└────────────┴────────────┴────────────┴────────────┴────────────┘

┌────────────┬────────────┬────────────┬────────────┐
│            │            │            │            │
│ 🔍 [F8]    │ 💳 [F9]    │ ⊘ [ESC]    │ 🗑 [DEL]   │
│ Consultar  │ Pagamento  │ Cancelar   │ Remover    │
│            │            │            │            │
└────────────┴────────────┴────────────┴────────────┘
```

A família visual deve ser única.

---

# 41. DIRETRIZ PARA O ANTIGRAVITY

Antes de começar a alterar código:

1. Inspecione a stack atual.
2. Verifique se já existe biblioteca de ícones.
3. Se existir uma biblioteca coerente e bem integrada, avalie se deve ser mantida.
4. Se os ícones atuais forem genéricos e não houver uma biblioteca de design system consolidada, instalar:

```bash
npm install @phosphor-icons/react
```

5. Não instalar múltiplas bibliotecas concorrentes.
6. Criar `ShortcutButton`.
7. Criar `ShortcutBar`.
8. Centralizar o mapeamento dos atalhos.
9. Criar tokens visuais.
10. Implementar os estados da venda.
11. Implementar animações.
12. Testar teclado.
13. Testar resolução atual.
14. Verificar acessibilidade.
15. Verificar performance.

---

# 42. MAPEAMENTO FINAL

```text
F2  → ShoppingCart
F3  → UserCircle
F4  → ListNumbers
F6  → Tag
F7  → XCircle
F8  → MagnifyingGlass
F9  → CreditCard
ESC → Prohibit
DEL → Trash
```

Biblioteca:

```text
@phosphor-icons/react
```

Biblioteca de animação, somente se o projeto for React:

```text
motion
```

Fonte, somente se ainda não houver uma definida:

```text
@fontsource/inter
```

---

# 43. PRINCÍPIO ARQUITETURAL FINAL

Não queremos:

```text
"um monte de ícones bonitos"
```

Queremos:

```text
                    DESIGN SYSTEM
                         │
          ┌──────────────┼──────────────┐
          │              │              │
       ÍCONES         ESTADOS        ANIMAÇÕES
          │              │              │
      Phosphor       IDLE/SALE      Motion/CSS
          │         PAYMENT/DONE
          │              │
          └──────────────┼──────────────┘
                         │
                    COMPONENTES
                         │
                 ShortcutButton
                 ShortcutBar
                 EmptyState
                 SaleState
                 PaymentState
                 CompletedState
                         │
                         ▼
                   CAIXA MERCADO
```

O objetivo é que futuras telas do sistema possam reutilizar essa mesma linguagem.

---

# 44. VISÃO DE PRODUTO

O Caixa Mercado não deve parecer um projeto que apenas "funciona".

Ele deve começar a parecer um **produto de software próprio**.

Para isso, precisamos de:

```text
Consistência
+
Semântica
+
Componentização
+
Design System
+
Estados claros
+
Microinterações
+
Performance
```

Essa é a diferença entre:

> "uma tela de caixa bonita"

e

> **"um sistema de PDV com identidade própria."**
