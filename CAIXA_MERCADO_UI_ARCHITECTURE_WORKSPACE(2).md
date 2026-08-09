# CAIXA MERCADO — UI ARCHITECTURE SPECIFICATION
## Arquitetura da Área Central do PDV + Estados Visuais + Microinterações

> **DESTINATÁRIO:** Antigravity
>
> **PAPEL:** Implementar esta especificação na interface real do Caixa Mercado.
>
> **OBJETIVO:** deixar de tratar a área central como um simples Empty State e transformá-la em um **palco operacional inteligente**, com identidade visual própria, estados claros, animações funcionais e componentes reutilizáveis.
>
> **IMPORTANTE:** não gerar mockup, não reconstruir o projeto do zero e não substituir a stack atual sem justificativa. Primeiro analisar a implementação existente e evoluir incrementalmente.

---

# 1. VISÃO DE PRODUTO

A área central atual funciona, mas visualmente está crua:

```text
VENDA EM ANDAMENTO

              🛒

          CAIXA LIVRE
     Aguardando produto

       [ F2 Produto ]
```

O problema não é o carrinho.

O problema é a **composição**.

Existe uma grande área vazia sem uma linguagem visual que comunique o estado operacional do caixa.

Queremos transformar essa região em:

> **PDV Idle Stage**

Uma área que informa ao operador, de forma visual e imediata:

```text
O CAIXA ESTÁ PRONTO
        ↓
O SCANNER ESTÁ AGUARDANDO
        ↓
O OPERADOR PODE LER UM PRODUTO
        ↓
A VENDA COMEÇARÁ
```

---

# 2. PRINCÍPIO FUNDAMENTAL

A área central não deve parecer:

- um dashboard;
- uma página de e-commerce;
- um banner;
- uma tela vazia;
- um card genérico de aplicação web.

Ela deve parecer uma **área operacional de um software de frente de caixa**.

Princípio:

```text
CLAREZA
   +
IDENTIDADE
   +
FEEDBACK
   +
VELOCIDADE
```

---

# 3. ARQUITETURA VISUAL DA ÁREA CENTRAL

Dividir a área em quatro camadas.

```text
┌─────────────────────────────────────────────┐
│                                             │
│  CAMADA 1 — AMBIENTAÇÃO                     │
│  grid / halo / pontos / linhas              │
│                                             │
│          CAMADA 2 — HERO                    │
│              🛒                             │
│                                             │
│          CAMADA 3 — ESTADO                  │
│           CAIXA LIVRE                       │
│         ● SCANNER PRONTO                    │
│                                             │
│          CAMADA 4 — AÇÃO                    │
│       Aguardando produto                    │
│       [ F2 Produtos ]                       │
│                                             │
└─────────────────────────────────────────────┘
```

Cada camada deve ter uma responsabilidade.

---

# 4. CAMADA 1 — AMBIENTAÇÃO

A área vazia precisa ganhar profundidade, mas sem ficar poluída.

Criar um background visual sutil.

Possibilidades:

- radial glow;
- pequenos pontos;
- linhas geométricas;
- grid extremamente discreto;
- círculos concêntricos;
- pequenos elementos decorativos;
- linhas horizontais lembrando scanner.

### Opacidade

Os elementos de ambientação devem ser muito discretos.

Exemplo conceitual:

```text
opacity: 0.04 — 0.12
```

Nunca permitir que o background concorra com:

- produtos;
- valores;
- texto;
- scanner;
- carrinho.

---

# 5. NÃO USAR IMAGEM DE FUNDO

Não usar:

- foto de supermercado;
- foto de carrinho;
- imagem de banco;
- wallpaper;
- PNG gigante;
- GIF.

O ambiente deve ser construído com:

```text
CSS
+
SVG
+
gradients muito discretos
+
pseudo-elements
```

Isso deixa o sistema leve e escalável.

---

# 6. CAMADA 2 — HERO VISUAL

O carrinho central é um **Hero State Icon**.

Ele NÃO deve ser tratado como um simples ícone da barra inferior.

## Regra

```text
Ícone da barra
    ↓
ícone funcional

Carrinho central
    ↓
elemento de identidade
```

---

# 7. BIBLIOTECA DE ÍCONES

Biblioteca operacional oficial:

```bash
npm install @phosphor-icons/react
```

Usar:

```text
Phosphor Icons
```

para ícones pequenos e funcionais.

Não misturar bibliotecas sem necessidade.

---

# 8. HERO SVG

Para o carrinho central, existem duas opções aceitáveis:

### Opção A — SVG customizado

Preferência.

Criar um SVG próprio inspirado na linguagem visual do sistema.

### Opção B — Phosphor ShoppingCart

Pode ser utilizado como base, mas deve receber tratamento visual próprio.

---

# 9. CARACTERÍSTICAS DO HERO

Tamanho sugerido:

```text
120px — 160px
```

Desktop.

O carrinho deve possuir:

- linhas claras;
- espessura consistente;
- azul/ciano;
- pequeno glow;
- halo radial;
- boa leitura em fundo escuro.

Não transformar o carrinho em neon exagerado.

---

# 10. HERO CONTAINER

O carrinho pode ficar dentro de uma composição visual:

```text
                ·       ·

             ╭───────────╮
          ·  │           │  ·
             │    🛒     │
          ·  │           │  ·
             ╰───────────╯

                ·       ·
```

Mas evitar o tradicional:

```text
⭕
🛒
```

O círculo atual parece um ícone isolado.

Queremos um **hero composition**.

---

# 11. HALO

Adicionar um halo radial muito discreto atrás do carrinho.

Conceito:

```css
background:
  radial-gradient(
    circle,
    rgba(..., 0.12),
    transparent 65%
  );
```

O halo deve desaparecer naturalmente no ambiente.

---

# 12. ANIMAÇÃO IDLE DO CARRINHO

Enquanto o caixa estiver livre:

```text
IDLE
```

o carrinho pode executar uma microanimação.

Sugestão:

```text
scale
+
translateY
+
opacity/glow
```

Exemplo conceitual:

```text
100%
 ↓
102%
 ↓
100%
```

Movimento vertical:

```text
0px
 ↓
-3px
 ↓
0px
```

Duração:

```text
3s — 5s
```

Easing suave.

---

# 13. NÃO ANIMAR DEMAIS

Não fazer:

```text
girar
pular
piscar
aumentar
diminuir
mover lateralmente
```

simultaneamente.

O operador ficará olhando para essa tela durante muito tempo.

A animação deve ser quase subconsciente.

---

# 14. CAMADA 3 — ESTADO

Abaixo do hero:

```text
CAIXA LIVRE
```

Este é o principal estado textual.

Configuração:

```text
font-size: 22px — 28px
font-weight: 700/800
```

De acordo com a escala atual do projeto.

---

# 15. STATUS OPERACIONAL

Adicionar uma pequena indicação:

```text
● SCANNER PRONTO
```

ou:

```text
● PRONTO PARA VENDA
```

Preferência:

```text
● SCANNER PRONTO
```

Isso é melhor do que somente:

```text
Aguardando leitura do produto
```

porque comunica uma condição operacional.

---

# 16. STATUS VISUAL

O indicador:

```text
● SCANNER PRONTO
```

pode utilizar verde/ciano.

Exemplo:

```text
● SCANNER PRONTO
```

com um pequeno ponto pulsando de forma muito discreta.

Não utilizar uma animação forte.

---

# 17. TEXTO DE ORIENTAÇÃO

Abaixo:

```text
Aproxime o produto do scanner
ou digite o código manualmente
```

Isso é melhor do que somente:

```text
Aguardando leitura do produto
```

porque explica o que fazer.

---

# 18. CAMADA 4 — AÇÃO

A ação secundária:

```text
F2 — Consultar produto
```

deve aparecer abaixo da instrução.

Ela não deve competir com o campo de código de barras no topo.

---

# 19. BOTÃO F2

Estrutura:

```text
┌───────────────────────────┐
│       [ F2 ]              │
│    Consultar produto      │
└───────────────────────────┘
```

ou versão compacta:

```text
[ F2 ]  Consultar produto
```

Usar a mesma linguagem do `ShortcutButton`.

---

# 20. HIERARQUIA FINAL DO IDLE

A composição deverá ser aproximadamente:

```text
                  ·       ·

                    🛒

               CAIXA LIVRE

             ● SCANNER PRONTO

      Aproxime o produto do scanner
       ou digite o código manualmente

             [ F2 ] Produtos

                  ·       ·
```

Essa composição deve ocupar melhor a região central sem parecer exagerada.

---

# 21. RESPONSIVIDADE VERTICAL

O conteúdo deve permanecer visualmente centralizado.

Não utilizar:

```css
position: absolute;
top: 50%;
```

sem considerar o conjunto completo.

O centro visual deve considerar:

```text
hero
+
título
+
status
+
descrição
+
ação
```

e não somente o carrinho.

---

# 22. ESTADO DE DIGITAÇÃO

Quando o operador começar a digitar um código:

```text
IDLE
 ↓
SCANNING
```

A área central pode reagir.

Exemplo:

```text
               ╭──────────────╮
               │ 789123456... │
               ╰──────────────╯

                LENDO PRODUTO

              ● AGUARDANDO ENTER
```

Se o scanner envia ENTER automaticamente, a transição deve acontecer imediatamente.

---

# 23. ESTADO DE PRODUTO ENCONTRADO

Quando o produto for localizado:

```text
             ✓

        PRODUTO ENCONTRADO

         ARROZ TIPO 1
            5 KG

           R$ 25,90
```

Esse estado deve ser muito curto.

Não criar uma tela intermediária que atrase a operação.

Ele serve apenas como feedback.

---

# 24. ESTADO DE VENDA

Quando existir pelo menos um item:

```text
SALE
```

remover o Empty State.

Mostrar:

```text
lista de produtos
```

A transição deve ser:

```text
fade-out do Empty State
+
fade/slide-in da lista
```

---

# 25. TRANSIÇÃO DO HERO

O carrinho não deve simplesmente desaparecer.

Sequência:

```text
Carrinho idle
      ↓
pequeno movimento
      ↓
fade
      ↓
lista aparece
```

Duração aproximada:

```text
250ms — 450ms
```

---

# 26. LISTA DE PRODUTOS

A área central passa a ter prioridade operacional.

Exemplo:

```text
┌───────────────────────────────────────────────────┐
│ VENDA EM ANDAMENTO                   3 item(ns)  │
├───────────────────────────────────────────────────┤
│                                                   │
│ PRODUTO                          QTD        TOTAL │
│                                                   │
│ Arroz Tipo 1 — 5kg                1        25,90 │
│ Feijão Carioca — 1kg              2        15,80 │
│ Refrigerante 2L                   1         8,99 │
│                                                   │
└───────────────────────────────────────────────────┘
```

---

# 27. FEEDBACK DE NOVO ITEM

Quando um produto for adicionado:

```text
linha nova
 ↓
highlight
 ↓
normal
```

Duração:

```text
150ms — 300ms
```

Não usar flash branco.

Preferir alteração sutil de:

- background;
- border;
- glow.

---

# 28. ÚLTIMO ITEM

O painel lateral "Último Item Adicionado" deve acompanhar o mesmo evento.

Quando produto entra:

```text
Último item
```

recebe pequeno feedback.

Não exagerar.

---

# 29. TOTAL A PAGAR

O valor deve continuar sendo o maior elemento visual do painel direito.

Quando mudar:

```text
R$ 25,90
   ↓
R$ 41,70
```

pode haver uma pequena transição.

Não atrasar o valor real.

---

# 30. ESTADO DE PAGAMENTO

Ao entrar em F9:

```text
SALE
 ↓
PAYMENT
```

A área central pode mudar para:

```text
               PAGAMENTO

             TOTAL A PAGAR

               R$ 41,70

       Selecione a forma de pagamento
```

A implementação deve respeitar o fluxo de pagamento já existente.

Não reescrever lógica financeira apenas para alterar UI.

---

# 31. PAGAMENTO APROVADO

Quando a venda realmente for finalizada:

```text
PAYMENT
 ↓
COMPLETED
```

Mostrar:

```text
                   ✓

             VENDA CONCLUÍDA

                R$ 41,70

            Pagamento aprovado
```

---

# 32. ANIMAÇÃO DO CARRINHO PARTINDO

Este é um elemento importante da identidade do sistema.

Depois da confirmação:

```text
                   ✓

             VENDA CONCLUÍDA

                R$ 41,70


                  🛒 ─────────→
                         ✦
```

O carrinho representa que a compra foi concluída.

---

# 33. SEQUÊNCIA DA ANIMAÇÃO

```text
Pagamento aprovado
        ↓
Check aparece
        ↓
Venda concluída
        ↓
Carrinho entra
        ↓
Carrinho acelera
        ↓
Move para a direita
        ↓
Fade
        ↓
Novo estado IDLE
```

Duração:

```text
600ms — 1200ms
```

Não bloquear a próxima operação.

---

# 34. MOTION

Se o projeto for React:

```bash
npm install motion
```

Usar `motion` para:

- entrada do carrinho;
- saída do carrinho;
- troca de estados;
- check;
- transições entre Empty State e Sale State.

Usar CSS para:

- hover;
- focus;
- pulse;
- pequenos glows;
- estados de botão.

Não usar Motion para absolutamente tudo.

---

# 35. ARQUITETURA DE COMPONENTES

Criar/reutilizar componentes conceitualmente:

```text
SaleWorkspace
│
├── WorkspaceHeader
│
├── SaleStateRenderer
│   │
│   ├── EmptySaleState
│   │   ├── HeroCart
│   │   ├── OperationalStatus
│   │   └── QuickAction
│   │
│   ├── ScanningState
│   │
│   ├── SaleItems
│   │
│   ├── PaymentState
│   │
│   └── CompletedState
│
└── WorkspaceBackground
```

Não precisa utilizar exatamente esses nomes se a arquitetura atual possuir outra convenção.

---

# 36. COMPONENTE WorkspaceBackground

Criar uma camada responsável somente pela ambientação.

Ela deve poder receber:

```text
state
```

e eventualmente alterar suavemente o ambiente.

Exemplo:

```text
IDLE
→ azul/ciano

PAYMENT
→ ambiente mais neutro

COMPLETED
→ pequeno feedback verde
```

Porém:

> Não mudar drasticamente a cor do painel inteiro.

A identidade geral deve permanecer estável.

---

# 37. COMPONENTE HeroCart

Responsável pelo carrinho central.

Propriedades conceituais:

```text
state
size
animated
variant
```

Exemplo:

```text
<HeroCart
    state="idle"
    animated
/>
```

Estados:

```text
idle
entering
leaving
completed
```

---

# 38. COMPONENTE OperationalStatus

Responsável por:

```text
● SCANNER PRONTO
```

Poderá futuramente suportar:

```text
SCANNER PRONTO
LENDO PRODUTO
PRODUTO ENCONTRADO
AGUARDANDO PAGAMENTO
PAGAMENTO APROVADO
```

Isso cria uma arquitetura extensível.

---

# 39. MÁQUINA DE ESTADOS

Não espalhar condições pela interface.

Evitar vários:

```text
if (...)
if (...)
if (...)
```

sem uma fonte de verdade.

Criar ou utilizar uma estrutura clara:

```text
IDLE
SCANNING
SALE
PAYMENT
COMPLETED
ERROR
```

O estado deve determinar a apresentação.

---

# 40. REGRA DE NEGÓCIO X UI

A lógica de negócio não deve depender de animação.

Correto:

```text
finalizarVenda()
      ↓
venda confirmada
      ↓
state = COMPLETED
      ↓
UI anima
```

Errado:

```text
animação
      ↓
aguarda animação
      ↓
salva venda
```

A UI reage ao estado real.

---

# 41. ESTADO ERROR

Criar arquitetura para erro mesmo que a primeira versão seja simples.

Exemplo:

```text
              !

       PRODUTO NÃO ENCONTRADO

     Verifique o código informado

              [ F8 ]
```

Não utilizar vermelho em excesso.

---

# 42. ESTADO DE SCANNER

Se possível, representar:

```text
● SCANNER PRONTO
```

Quando o scanner estiver recebendo dados:

```text
● LENDO
```

Pode utilizar um pequeno efeito horizontal:

```text
────────────→
```

Mas somente durante leitura.

---

# 43. BACKGROUND GRID

Uma possibilidade:

```text
┼────┼────┼────┼────┼
│    │    │    │    │
├────┼────┼────┼────┤
│    │    │    │    │
├────┼────┼────┼────┤
```

Porém:

- baixa opacidade;
- sem linhas fortes;
- não aplicar no painel inteiro se ficar pesado.

O grid deve ser percebido apenas quando o usuário prestar atenção.

---

# 44. LINHAS DE SCANNER

Outra possibilidade para o Idle Stage:

```text
            ─────────
                 ↓
                🛒
            ─────────
```

Usar uma linha extremamente sutil passando ocasionalmente.

Não deixar rodando constantemente.

---

# 45. PARTÍCULAS

Pode haver:

```text
·
     ·
          ✦
  ·
```

Mas limitar drasticamente.

Quantidade aproximada:

```text
5 — 12 elementos
```

Máximo.

---

# 46. PERFORMANCE

Não utilizar:

- canvas pesado;
- vídeo;
- GIF;
- imagens grandes;
- filtros de blur excessivos;
- dezenas de elementos DOM animados.

Preferir:

```text
SVG
CSS
transform
opacity
```

Para animações:

```text
transform
opacity
```

sempre que possível.

---

# 47. REDUCED MOTION

Implementar:

```css
@media (prefers-reduced-motion: reduce)
```

Nesse modo:

```text
sem carrinho andando
sem partículas
sem pulse contínuo
sem transições longas
```

Pode manter:

```text
fade mínimo
```

ou mudança instantânea.

---

# 48. BARRA DE ATALHOS

A barra inferior deve seguir o Design System definido anteriormente.

Biblioteca:

```text
@phosphor-icons/react
```

Componente:

```text
ShortcutButton
```

---

# 49. MAPEAMENTO

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

---

# 50. TECLA + ÍCONE + LABEL

A composição deve ser:

```text
┌──────────────────────┐
│  🛒     [ F2 ]       │
│         Produto      │
└──────────────────────┘
```

ou:

```text
🛒  [F2]  Produto
```

dependendo da altura disponível.

A tecla precisa ser claramente reconhecível.

---

# 51. NÃO DEIXAR OS ÍCONES GENÉRICOS

Os ícones devem:

- pertencer à mesma família;
- possuir peso consistente;
- possuir tamanho consistente;
- respeitar semântica;
- ter estados;
- ter contraste.

Não fazer:

```text
ícone fino
ícone grosso
emoji
material icon
SVG aleatório
```

na mesma barra.

---

# 52. DESIGN TOKENS

Criar ou consolidar:

```text
--color-action
--color-success
--color-warning
--color-danger
--color-surface
--color-surface-elevated
--color-border
--color-text
--color-text-muted

--spacing-xs
--spacing-sm
--spacing-md
--spacing-lg
--spacing-xl

--radius-sm
--radius-md
--radius-lg

--icon-sm
--icon-md
--icon-lg
--icon-hero

--motion-fast
--motion-normal
--motion-slow
```

Não espalhar valores arbitrários pelo CSS.

---

# 53. TIPOGRAFIA

Se ainda não houver uma fonte definida:

Preferência:

```bash
npm install @fontsource/inter
```

Usar:

```text
Inter
```

A tipografia deve possuir:

- boa legibilidade;
- números muito claros;
- pesos consistentes;
- excelente leitura em monitor de caixa.

---

# 54. NÚMEROS

Valores monetários devem possuir atenção especial.

Exemplo:

```text
R$ 41,70
```

Os números precisam ser:

- grandes;
- pesados;
- facilmente escaneáveis.

Evitar fontes excessivamente estilizadas.

---

# 55. O QUE NÃO MEXER SEM NECESSIDADE

Não alterar:

- posição do campo principal de código;
- atalhos funcionais;
- lógica do scanner;
- lógica de produtos;
- cálculo do total;
- lógica de pagamento;
- status do caixa;
- informações essenciais do operador.

A evolução é principalmente de:

```text
UI
UX
Estados
Microinterações
Design System
```

---

# 56. ORDEM DE IMPLEMENTAÇÃO

Implementar por etapas.

## ETAPA 1 — AUDITORIA

Antes de escrever código:

- identificar stack;
- identificar framework;
- identificar biblioteca de ícones;
- identificar sistema de estilos;
- identificar componentes existentes;
- identificar gerenciamento de estado;
- identificar fluxo de venda.

Não instalar dependências desnecessárias.

---

## ETAPA 2 — DESIGN TOKENS

Centralizar:

```text
cores
spacing
radius
tipografia
motion
```

---

## ETAPA 3 — ICON SYSTEM

Instalar:

```bash
npm install @phosphor-icons/react
```

somente se não houver uma biblioteca visual coerente já adotada.

---

## ETAPA 4 — SHORTCUT SYSTEM

Criar:

```text
ShortcutButton
ShortcutBar
```

---

## ETAPA 5 — WORKSPACE

Criar:

```text
WorkspaceBackground
HeroCart
OperationalStatus
EmptySaleState
```

---

## ETAPA 6 — ESTADOS

Implementar:

```text
IDLE
SCANNING
SALE
PAYMENT
COMPLETED
ERROR
```

---

## ETAPA 7 — ANIMAÇÕES

Implementar:

```text
idle
enter
sale transition
payment
completed
cart exit
reset
```

---

## ETAPA 8 — REFINAMENTO

Ajustar:

- espaçamento;
- escala;
- contraste;
- densidade;
- hierarquia;
- responsividade.

---

# 57. CRITÉRIOS DE ACEITAÇÃO — ÁREA CENTRAL

## Visual

- [ ] A área central não parece vazia.
- [ ] O carrinho é visualmente especial.
- [ ] Existe ambientação discreta.
- [ ] O texto possui hierarquia.
- [ ] O status operacional é evidente.
- [ ] F2 aparece como ação secundária.

## Operação

- [ ] Scanner continua sendo a ação principal.
- [ ] Digitação continua funcionando.
- [ ] Primeiro produto remove Empty State.
- [ ] Lista aparece sem atrasar operação.
- [ ] Total continua correto.

## Estados

- [ ] IDLE funciona.
- [ ] SCANNING funciona.
- [ ] SALE funciona.
- [ ] PAYMENT funciona.
- [ ] COMPLETED funciona.
- [ ] ERROR possui tratamento.

## Animação

- [ ] Carrinho possui idle sutil.
- [ ] Entrada é suave.
- [ ] Saída é suave.
- [ ] Carrinho parte após venda concluída.
- [ ] Novo carrinho retorna.
- [ ] Reduced Motion funciona.

---

# 58. CRITÉRIOS DE ACEITAÇÃO — ATALHOS

- [ ] Todos os atalhos continuam funcionando.
- [ ] Ícones pertencem à mesma família.
- [ ] F2/F3/F4/F6/F7/F8/F9 são claramente identificáveis.
- [ ] ESC e DEL são visualmente distinguíveis.
- [ ] Teclas possuem destaque.
- [ ] Ícones possuem pelo menos ~18px de leitura.
- [ ] Botões não ficam espremidos.
- [ ] Focus de teclado é visível.
- [ ] Disabled é claramente distinguível.
- [ ] Não há emojis.

---

# 59. TESTE DE USABILIDADE

Depois de implementar, olhar a tela por alguns segundos e verificar:

### Pergunta 1

> Eu sei imediatamente que o caixa está pronto?

### Pergunta 2

> Sei imediatamente o que devo fazer?

### Pergunta 3

> Consigo identificar F2/F9/ESC sem procurar?

### Pergunta 4

> Sei que a venda terminou sem precisar ler um log?

### Pergunta 5

> A animação ajuda ou atrapalha?

### Pergunta 6

> A tela ainda parece um PDV profissional?

Se qualquer resposta for negativa, ajustar a composição.

---

# 60. VISÃO FINAL

O estado inicial deve transmitir algo próximo de:

```text
┌─────────────────────────────────────────────────────────┐
│ VENDA EM ANDAMENTO                         0 item(ns)  │
│                                                         │
│                                                         │
│              ·                         ·                │
│                                                         │
│                       ╭───────╮                         │
│                    ╭──│  🛒   │──╮                     │
│                       ╰───────╯                         │
│                                                         │
│                     CAIXA LIVRE                        │
│                                                         │
│                  ● SCANNER PRONTO                      │
│                                                         │
│           Aproxime o produto do scanner                │
│            ou digite o código manualmente              │
│                                                         │
│                  [ F2 ] Produtos                        │
│                                                         │
│              ·                         ·                │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

Isso é apenas uma **referência conceitual**, não um mockup para copiar literalmente.

---

# 63. SVG ASSETS OFICIAIS — NÃO SUBSTITUIR

Os dois SVGs abaixo são os **assets visuais definidos para o produto**.

Não substituir por outro ícone genérico, emoji, Phosphor ou outro SVG.

A biblioteca **Phosphor Icons continua sendo a biblioteca dos ícones operacionais pequenos**, como atalhos e ações. Estes dois SVGs são exceções: são **Hero Assets de estado**.

---

## 63.1 SVG PRINCIPAL — IDLE / CAIXA LIVRE

Este é o SVG oficial do carrinho que deve aparecer na **tela principal do PDV**, quando não existe nenhum item na venda.

O arquivo fornecido possui o asset `fi_9725740`.

### CÓDIGO SVG COMPLETO — `cart-idle.svg`

O Antigravity deve copiar este SVG **integralmente** para `src/assets/illustrations/cart-idle.svg`.
Não resumir, não substituir por outro SVG e não reconstruir o desenho com outra biblioteca.

```svg
<svg id="fi_9725740" enable-background="new 0 0 512 512" height="512" viewBox="0 0 512 512" width="512" xmlns="http://www.w3.org/2000/svg"><g clip-rule="evenodd" fill-rule="evenodd"><g><g><path d="m303.304 223.884c27.358-45.85 90.555-130.467 101.96-141.872 14.874-14.874 38.989-14.874 53.862 0 14.874 14.874 14.874 38.988 0 53.862-11.406 11.406-96.022 74.602-141.872 101.96-4.054 2.417-9.068 1.792-12.405-1.545s-3.962-8.35-1.545-12.405z" fill="#fca62f"></path><path d="m461.738 84.9c12.204 14.959 11.334 37.029-2.611 50.974-11.405 11.406-96.022 74.602-141.872 101.96-4.054 2.417-9.068 1.792-12.405-1.545-2.283-2.283-3.296-5.349-2.936-8.356 2.932 1.294 6.351 1.132 9.272-.61 45.85-27.358 130.466-90.555 141.872-101.96 10.939-10.94 13.832-26.878 8.68-40.463z" fill="#fb9927"></path></g><g><path d="m273.153 58.78c-18.524-2.593-30.256 4.045-50.952 26.712-19.918 3.318-32.687 15.463-38.308 36.436-16.044 9.764-24.125 25.624-24.245 47.581-27.925 41.126 9.981 130.649 79.737 60.894l101.482-101.483c38.139-38.52-14.11-102.919-67.714-70.14z" fill="#c48958"></path><path d="m150.922 193.873c-5.269 42.769 30.662 94.33 88.463 36.529l101.482-101.482c11.615-11.73 14.835-25.859 12.199-38.913-1.607 7.688-5.507 15.243-12.199 22.003l-101.482 101.481c-50.267 50.266-83.99 17.823-88.463-19.618z" fill="#b5733e"></path><path d="m198.481 184.04c-4.786-3.153-10.586-6.049-17.259-8.547-6.423-2.405-13.656-4.441-21.573-5.985.028-5.069.48-9.813 1.356-14.233.503-2.535 1.145-4.963 1.927-7.285 22.286 9.001 36.697 18.544 43.141 28.458 3.665 5.637 4.753 11.395 3.249 17.24-2.601-3.369-6.256-6.627-10.841-9.648zm116.904-87.353c-2.664-9.189-14.348-21.451-27.591-30.16-4.794-3.153-9.793-5.84-14.642-7.747 1.472-.9 2.943-1.727 4.411-2.483 6.202-3.194 12.352-5.125 18.312-5.987 23.922 17.169 29.322 32.835 19.51 46.377zm-68.739 4.148c-7.826-7.068-16.692-12.694-24.445-15.343 6.399-7.009 11.941-12.485 17.092-16.63 8.396 5.449 15.367 12.014 20.424 18.901 6.388 8.7 9.721 17.913 9.009 26.032-.344 3.919-1.63 7.584-3.97 10.814-.66-1.522-1.424-3.047-2.278-4.565-3.831-6.806-9.49-13.481-15.832-19.209zm-34.551 34.55c8.229 5.266 14.496 10.95 18.397 16.645.597.872 1.14 1.745 1.625 2.617.834-1.966 1.4-3.94 1.687-5.929 1.145-7.914-2.109-16.048-10.312-24.729-6.641-7.029-16.525-14.417-29.945-22.338-4.274 5.454-7.492 12.213-9.653 20.277 10.919 3.886 20.434 8.485 28.201 13.457z" fill="#f2d1a5"></path><path d="m198.481 184.04c-4.786-3.153-10.586-6.049-17.259-8.547-6.423-2.405-13.656-4.441-21.573-5.985.011-2.042.091-4.032.24-5.969 8.327 1.274 16.552 3.47 24.44 6.423 6.674 2.499 12.474 5.394 17.26 8.548 3.129 2.061 5.824 4.233 8.047 6.475.543 2.877.44 5.779-.313 8.704-2.602-3.37-6.257-6.628-10.842-9.649zm116.904-87.353c-2.664-9.189-14.348-21.451-27.591-30.16-4.794-3.153-9.793-5.84-14.642-7.747 1.472-.9 2.943-1.727 4.411-2.483.901-.464 1.802-.901 2.701-1.313 3.562 1.678 7.153 3.723 10.636 6.013 13.24 8.707 24.923 20.967 27.589 30.156-.758 1.881-1.794 3.726-3.104 5.534zm-68.739 4.148c-7.826-7.068-16.692-12.694-24.445-15.343 1.599-1.752 3.145-3.408 4.645-4.971 7.372 2.834 15.593 8.179 22.906 14.784 6.342 5.728 12.002 12.403 15.832 19.208.797 1.416 1.515 2.837 2.143 4.257-.669 2.063-1.653 4.02-2.971 5.839-.66-1.522-1.424-3.047-2.278-4.565-3.831-6.806-9.49-13.481-15.832-19.209zm-34.551 34.55c8.229 5.266 14.496 10.95 18.397 16.645.597.872 1.14 1.745 1.625 2.617.834-1.966 1.4-3.94 1.687-5.929.08-.55.137-1.101.174-1.653-.124-.189-.251-.377-.38-.566-3.9-5.694-10.168-11.378-18.396-16.644-7.352-4.706-21.151-11.771-29.478-13.762-.677 1.863-1.287 3.808-1.831 5.835 10.92 3.886 20.435 8.485 28.202 13.457z" fill="#ecba78"></path></g><g><path d="m413.99 36.819c4.422 2.553 5.951 8.259 3.398 12.68l-34.108 59.077c-2.553 4.422-8.259 5.95-12.68 3.398-4.421-2.553-5.951-8.259-3.398-12.68l34.108-59.077c2.553-4.422 8.259-5.951 12.68-3.398z" fill="#abd641"></path><path d="m298.643 212.355c17.895-63.141 22.986-109.192 32.899-126.362 12.928-22.391 41.559-30.063 63.95-17.135s30.063 41.559 17.135 63.95c-9.913 17.17-47.249 44.604-92.983 91.672-4.045 4.16-10.195 5.013-15.22 2.112-5.024-2.901-7.361-8.654-5.781-14.237z" fill="#c2ed56"></path><path d="m380.107 63.266c5.274.916 10.48 2.76 15.386 5.592 22.391 12.928 30.063 41.559 17.135 63.95-9.913 17.17-47.249 44.604-92.983 91.672-4.045 4.16-10.195 5.013-15.22 2.112-1.515-.875-2.784-2.009-3.777-3.319 2.036-.588 3.944-1.71 5.528-3.339 45.734-47.068 83.07-74.502 92.983-91.673 12.927-22.391 5.256-51.022-17.135-63.95-.634-.365-1.274-.712-1.917-1.045z" fill="#b7e546"></path></g><g><path d="m148.295 33.216h94.134c2.673 0 4.859 2.186 4.859 4.859v21.727c0 2.673-2.186 4.859-4.859 4.859h-94.134c-2.673 0-4.859-2.186-4.859-4.859v-21.727c0-2.673 2.186-4.859 4.859-4.859z" fill="#c4e2ff"></path><path d="m148.294 33.216h10.989c-2.673 0-4.859 2.186-4.859 4.859v21.727c0 2.673 2.186 4.859 4.859 4.859h-10.989c-2.673 0-4.859-2.186-4.859-4.859v-21.727c.001-2.673 2.187-4.859 4.859-4.859z" fill="#add5fa"></path><path d="m247.288 58.745 30.763 53.285v165.38h-102.02l10.604-218.665z" fill="#60b7ff"></path><path d="m143.436 58.745h103.852l-30.765 53.285v45.357l-103.852-2.995v-42.362z" fill="#d8ecfe"></path><path d="m143.436 58.745h10.989l-30.765 53.285v42.362l92.863 2.678v.317l-103.852-2.995v-42.362z" fill="#c4e2ff"></path><path d="m112.671 112.03h103.852v165.38h-95.754c-4.454 0-8.098-3.644-8.098-8.098z" fill="#8ac9fe"></path><path d="m112.671 112.03h10.989v157.282c0 4.454 3.644 8.098 8.098 8.098h-10.988c-4.454 0-8.098-3.644-8.098-8.098v-157.282z" fill="#60b7ff"></path><path d="m142.003 162.033h45.189c4.454 0 8.098 3.644 8.098 8.098v49.178c0 4.454-3.644 8.098-8.098 8.098h-45.189c-4.454 0-8.098-3.644-8.098-8.098v-49.178c0-4.454 3.644-8.098 8.098-8.098z" fill="#eceff1"></path><path d="m187.191 162.033c4.455 0 8.099 3.644 8.099 8.098v49.178c0 4.454-3.644 8.098-8.098 8.098h-45.189c-4.454 0-8.098-3.644-8.098-8.098h45.188c4.454 0 8.098-3.644 8.098-8.098z" fill="#d1d1d6"></path></g><g><path d="m104.941 167.684h346.788l-60.002 181.558h-246.785z" fill="#c4e2ff"></path><path d="m368.634 408.381c8.23 0 14.902 6.672 14.902 14.902s-6.672 14.902-14.902 14.902h-226.525c-15.329 0-29.254-6.26-39.34-16.346s-16.346-24.011-16.346-39.34 6.26-29.254 16.346-39.34c5.911-5.91 13.14-10.507 21.198-13.3l-56.791-235.253c-.128-.531-.409-.967-.784-1.263-.347-.273-.839-.431-1.422-.431h-34.041c-8.23 0-14.902-6.672-14.902-14.902s6.672-14.902 14.902-14.902h34.041c7.425 0 14.315 2.502 19.817 6.834 5.475 4.31 9.512 10.426 11.262 17.679l16.203 67.118h343.282c8.23 0 14.903 6.672 14.903 14.902 0 1.944-.373 3.8-1.05 5.502l-43.723 141.754c-3.681 11.935-10.851 21.836-20.247 28.771-9.465 6.986-21.056 10.949-33.498 10.949h-229.81c-7.102 0-13.569 2.916-18.268 7.614-4.698 4.698-7.614 11.165-7.614 18.267s2.916 13.57 7.614 18.268 11.165 7.614 18.268 7.614h226.525zm-214.842-81.567h218.127c6.008 0 11.469-1.814 15.802-5.012 4.403-3.25 7.78-7.941 9.536-13.635 17.514-56.784 11.572-37.517 38.13-123.622-143.328 0-174.183 0-315.94 0 20.123 83.359 15.047 62.332 34.345 142.269z" fill="#d8ecfe"></path><path d="m30.928 92.913c-8.23 0-14.902-6.672-14.902-14.902s6.672-14.902 14.902-14.902h34.042c7.425 0 14.315 2.502 19.817 6.834 5.475 4.31 9.512 10.426 11.262 17.679l1.277 5.291z" fill="#d1d1d6"></path><circle cx="188.946" cy="429.776" fill="#9facba" r="48.771"></circle><ellipse cx="188.946" cy="429.776" fill="#eceff1" rx="19.858" ry="19.858" transform="matrix(.23 -.973 .973 .23 -272.743 514.925)"></ellipse><circle cx="370.188" cy="429.776" fill="#9facba" r="48.771"></circle><ellipse cx="370.188" cy="429.776" fill="#eceff1" rx="19.858" ry="19.858" transform="matrix(.707 -.707 .707 .707 -195.472 387.641)"></ellipse></g></g><path d="m370.149 417.83c6.598 0 11.958 5.37 11.958 11.96 0 6.6-5.36 11.97-11.958 11.97-6.579 0-11.93-5.37-11.93-11.97 0-6.59 5.351-11.96 11.93-11.96zm0 39.909c15.408 0 27.95-12.539 27.95-27.949 0-15.411-12.542-27.95-27.95-27.95-15.389 0-27.922 12.539-27.922 27.95.001 15.41 12.533 27.949 27.922 27.949zm-181.221-39.909c6.579 0 11.939 5.37 11.939 11.96 0 6.6-5.36 11.97-11.939 11.97-6.589 0-11.959-5.37-11.959-11.97 0-6.59 5.37-11.96 11.959-11.96zm0 39.909c15.399 0 27.922-12.539 27.922-27.949 0-15.411-12.523-27.95-27.922-27.95-15.408 0-27.95 12.539-27.95 27.95 0 15.41 12.542 27.949 27.95 27.949zm227.822-233.54h-63.152v-31.181h72.758c-3.939 12.761-7.06 22.881-9.606 31.181zm-43.283 95.72h-19.869v-32.079h43.523c-1.633 5.28-3.544 11.481-5.864 19.01-2.49 8.059-9.309 13.069-17.79 13.069zm-220.546-32.08h47.349v32.079h-39.603c-3.219-13.338-5.709-23.648-7.746-32.079zm47.349-94.82v31.181h-62.71l-7.53-31.181zm84.669 31.18v-31.181h52.672v31.181zm0 63.64h52.672v32.079h-52.672zm-15.992 0v32.079h-52.691v-32.079zm-52.69-63.64v-31.181h52.691v31.181zm0 15.99h52.691v31.66h-52.691zm-74.838 0h58.851v31.66h-51.213c-2.81-11.629-4.767-19.78-7.638-31.66zm196.192 31.66h-52.672v-31.66h52.672zm64.447 0h-48.46v-31.66h58.22c-1.548 5.021-2.842 9.181-4 12.949-2.028 6.591-3.69 11.981-5.76 18.711zm35.131-94.82h-317.311c-2.452 0-4.763 1.121-6.283 3.052-1.515 1.919-2.057 4.439-1.487 6.818l12.152 50.331c7.13 29.541 8.41 34.85 22.35 92.56.856 3.59 4.08 6.12 7.77 6.12h219.088c15.521 0 28.501-9.559 33.061-24.349 9.389-30.41 12.071-39.111 16.57-53.721 3.939-12.78 9.281-30.099 21.729-70.46.748-2.429.301-5.059-1.21-7.109-1.501-2.042-3.892-3.242-6.429-3.242zm-92.608-83.498c8.768-15.1 28.2-20.301 43.335-11.591 3.822 2.21 5.144 7.09 2.932 10.92-2.198 3.829-7.087 5.14-10.909 2.939-7.52-4.329-17.168-1.75-21.531 5.761-1.478 2.55-4.16 3.98-6.918 3.98-1.36 0-2.744-.351-4-1.081-3.822-2.221-5.121-7.108-2.909-10.928zm108.868 36.708c-.56.571-1.379 1.322-2.409 2.211l-19.903-4.75c-4.287-1.031-8.608 1.619-9.629 5.911-1.031 4.299 1.619 8.609 5.911 9.638l7.94 1.89c-.819.641-1.652 1.291-2.509 1.95h-20.961c3.421-3.869 5.85-7.16 7.648-10.26 7.323-12.679 9.262-27.45 5.459-41.589-1.275-4.811-3.195-9.34-5.638-13.521 11.243-5.329 24.891-3.23 34.101 5.98 5.681 5.671 8.81 13.22 8.81 21.261 0 8.039-3.13 15.6-8.82 21.279zm9.728 35.89c-1.318-1.939-3.408-3.05-5.751-3.05h-344.776c-3.694 0-6.904-2.53-7.77-6.12l-16.27-67.4c-2.63-10.89-12.273-18.489-23.441-18.489h-34.209c-3.85 0-6.984 3.13-6.984 6.971 0 3.85 3.134 6.979 6.984 6.979h34.209c4.767 0 8.768 3.09 9.958 7.701.014.04.014.081.024.11l57.039 236.269c.96 4.001-1.261 8.08-5.153 9.431-19.338 6.72-32.327 24.928-32.327 45.31 0 22.499 16.02 41.79 37.24 46.738-.009-.27-.023-.52-.023-.789 0-4.55.551-8.971 1.562-13.221-14.311-4.01-24.83-17.159-24.83-32.728 0-18.741 15.239-33.98 33.979-33.98h230.83c21.399 0 40.022-13.76 46.333-34.251l43.909-142.381c.061-.209.132-.399.207-.599.86-2.17.592-4.53-.74-6.501zm-274.249 222.681c-22.609 0-41.01 18.38-41.01 40.98 0 22.61 18.401 40.99 41.01 40.99 22.599 0 40.991-18.38 40.991-40.99 0-22.6-18.392-40.98-40.991-40.98zm125.519 29.02c-.828 3.859-1.28 7.859-1.28 11.96 0 .679.033 1.35.061 2.019h-67.369c.019-.669.047-1.34.047-2.019 0-4.101-.447-8.101-1.275-11.96zm55.702-29.02c-22.599 0-40.991 18.38-40.991 40.98 0 22.61 18.392 40.99 40.991 40.99s40.986-18.38 40.986-40.99c.001-22.6-18.386-40.98-40.986-40.98zm-222.132-322.05h85.403l-21.55 37.3h-85.37zm91.262-25.54v9.55h-87.884v-9.55zm30.751 72.979v32.89h-45.551v-32.89l22.792-39.449zm2.009-50.869c13.737 5.541 23.089 11.96 27.338 18.749-9.629-6.559-21.728-11.999-33.169-15.379 1.963-1.291 3.911-2.41 5.831-3.37zm-63.552 56.719v27.04h-87.841v-27.04zm90.754-62.1c12.029.951 23.667 6.261 32.346 14.521-2.617 2.869-4.979 6.05-6.998 9.55-6.09 10.569-10.198 29.3-15.879 55.23-.701 3.169-1.421 6.46-2.17 9.839h-20.519v-35.03c0-1.399-.372-2.78-1.073-4l-12.98-22.48c14.029 5.69 25.508 13.74 29.212 19.839 1.449 2.41 4.047 3.86 6.829 3.86.24 0 .48-.009.72-.029 3.04-.27 5.662-2.261 6.749-5.111 3.52-9.219 3.431-18.069-.278-26.299-3.379-7.521-9.582-14.15-18.73-20.031.959.021 1.877.071 2.771.141zm92.227 17.861c-8.989-5.191-19.451-6.57-29.461-3.892-10.019 2.68-18.387 9.102-23.569 18.08-4.852 8.421-8.942 27.072-14.123 50.671-.456 2.09-.927 4.239-1.407 6.419h66.729c6.203-5.989 12.999-12.979 16.062-18.269 5.168-8.96 6.546-19.421 3.859-29.44-2.681-10.009-9.11-18.39-18.09-23.569zm16.731-31.59c.339-.601 1.181-.82 1.779-.471.372.209.522.541.579.779.071.241.104.601-.141 1l-9.869 17.1c-.357-.22-.72-.45-1.087-.669-.381-.212-.762-.41-1.139-.621zm90.091 29c-5.28-1.581-10.292-2.54-15.14-2.921l9.879-9.359c3.2-3.03 3.341-8.091.31-11.301-3.031-3.201-8.09-3.34-11.299-.31l-11.074 10.48c-.259-5.431-1.266-11.019-3.036-16.959-1.261-4.23-5.723-6.64-9.953-5.381-4.231 1.259-6.636 5.71-5.379 9.94 2.32 7.789 2.979 14.57 1.972 21.26-11.37-6.361-24.844-7.561-37.061-3.43l6.739-11.69c2.329-3.98 2.951-8.65 1.76-13.121-1.186-4.489-4.038-8.22-8.038-10.531-8.26-4.758-18.863-1.92-23.621 6.331l-11.262 19.522c-13.441-2.75-27.39-.311-39.019 6.57-11.45-11.551-27.24-19.01-43.56-20.31-15.888-1.261-31.63 3.269-45.25 12.85v-21.63c0-4.419-3.577-8-7.986-8h-103.863c-4.419 0-8 3.581-8 8v23.399l-26.999 46.79-4.26-17.619c-4.372-18.092-20.401-30.721-38.981-30.721h-34.209c-12.66 0-22.962 10.3-22.962 22.961 0 12.659 10.302 22.96 22.962 22.96h29.719l54.361 225.21c-21.992 10.57-36.323 32.899-36.323 57.631 0 16.7 6.438 32.519 18.119 44.539 10.241 10.54 23.512 17.13 37.852 18.91 7.408 22.88 28.92 39.481 54.239 39.481 25.131 0 46.492-16.362 54.051-38.98h73.13c7.549 22.619 28.91 38.98 54.041 38.98 31.419 0 56.978-25.561 56.978-56.98 0-31.41-25.559-56.97-56.978-56.97-21.262 0-39.833 11.711-49.622 29.021h-81.977c-9.794-17.31-28.35-29.021-49.622-29.021-21.15 0-39.631 11.58-49.472 28.721-8.405-1.511-14.815-8.861-14.815-17.7 0-9.921 8.076-18.001 17.997-18.001h230.83c28.458 0 53.222-18.29 61.614-45.52l43.818-142.112c2.659-6.98 1.727-14.83-2.489-21.049-4.085-6.021-10.73-9.71-17.94-10.03 2.645-2.221 4.866-4.169 6.278-5.579 8.711-8.71 13.511-20.281 13.511-32.59 0-7.97-2.024-15.621-5.803-22.39 6.702-1.011 13.469-.351 21.272 1.969 4.231 1.261 8.678-1.149 9.94-5.379 1.257-4.231-1.153-8.681-5.379-9.941z"></path></g></svg>
```
 O SVG contém uma ilustração completa de carrinho de compras e elementos associados. fileciteturn0file0L1-L1

### REGRA VISUAL ABSOLUTA

**NÃO colocar círculo, borda circular, container circular ou badge ao redor do SVG.**

Não fazer:

```text
        ╭────────╮
        │   🛒   │
        ╰────────╯
```

Nem:

```text
          ◯
         🛒
```

O SVG deve aparecer **sozinho, grande e centralizado** na área principal.

### Composição desejada

```text
┌─────────────────────────────────────────────────────┐
│                                                     │
│                                                     │
│                    [ SVG GRANDE ]                   │
│                                                     │
│                    CAIXA LIVRE                      │
│                 ● SCANNER PRONTO                    │
│                                                     │
│       Aproxime o produto do scanner                 │
│        ou digite o código manualmente               │
│                                                     │
│                  [ F2 ] Produtos                    │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### Tamanho

Não utilizar o tamanho original de `512px` como regra fixa.

O SVG deve ser responsivo, por exemplo:

```css
width: clamp(180px, 18vw, 300px);
height: auto;
```

Ajustar conforme a resolução real do PDV.

**Preferência:** o carrinho deve ser visualmente grande o suficiente para preencher a região central, mas sem encostar no cabeçalho ou no rodapé.

### Posicionamento

Usar layout flex/grid para centralização.

Preferência:

```css
display: grid;
place-items: center;
```

ou uma composição equivalente.

**Não usar posicionamento absoluto frágil baseado em `top: 50%`.**

O centro deve considerar o conjunto:

```text
SVG
+
CAIXA LIVRE
+
STATUS
+
INSTRUÇÃO
+
F2
```

### Tratamento visual

O SVG pode receber:

- leve glow;
- pequena sombra;
- opacidade/transparência controlada;
- animação idle extremamente sutil.

Mas:

**não alterar a identidade visual interna do SVG.**

Não aplicar filtros que destruam as cores ou detalhes da ilustração.

### Animação Idle

O SVG pode executar:

```text
translateY: 0 → -4px → 0
```

com duração aproximada de:

```text
3s — 5s
```

Loop suave.

Nada de:

- rotação;
- salto;
- giro;
- bounce agressivo;
- escala exagerada.

---

## 63.2 SVG DE CHECKOUT — PAYMENT / CHECKOUT

Este segundo SVG é o asset oficial da **área de Checkout/Pagamento**.

Ele possui o identificador:

```text
fi_13791848
```

É um SVG de `90 × 90`, com carrinho e marca de confirmação.

### REGRA

Este SVG **não deve aparecer no estado inicial de caixa livre**.

Ele pertence ao contexto:

```text
SALE
   ↓
PAYMENT / CHECKOUT
```

e/ou aos estados de confirmação relacionados ao checkout, conforme o fluxo existente.

### Uso visual

No checkout, criar uma composição semelhante a:

```text
┌─────────────────────────────────────────────────────┐
│                                                     │
│                                                     │
│                       [ SVG ]                       │
│                                                     │
│                     CHECKOUT                        │
│                                                     │
│                   TOTAL A PAGAR                     │
│                                                     │
│                     R$ 41,70                        │
│                                                     │
│              Selecione o pagamento                  │
│                                                     │
└─────────────────────────────────────────────────────┘
```

O SVG pode ser escalado acima do tamanho original de 90px quando houver espaço, mantendo a proporção.

Sugestão:

```css
width: clamp(90px, 9vw, 150px);
height: auto;
```

---

## 63.3 SVG DE CHECKOUT — CÓDIGO OFICIAL

Usar exatamente este asset:

```svg
<svg id="fi_13791848" enable-background="new 0 0 90 90" height="90" viewBox="0 0 90 90" width="90" xmlns="http://www.w3.org/2000/svg">
<path d="m62.877 74.083c1.761.006 3.183 1.433 3.183 3.188 0 1.761-1.428 3.188-3.183 3.188-1.761 0-3.188-1.427-3.188-3.188.001-1.755 1.428-3.182 3.188-3.188zm0-3.781c-3.854 0-6.975 3.12-6.975 6.969 0 3.854 3.121 6.975 6.975 6.975 3.85 0 6.968-3.12 6.968-6.975.001-3.849-3.118-6.969-6.968-6.969z"></path>
<path d="m40.133 74.083c1.756.006 3.182 1.433 3.182 3.188 0 1.761-1.426 3.188-3.182 3.188-1.76 0-3.187-1.427-3.187-3.188 0-1.755 1.427-3.182 3.187-3.188zm0-3.781c-3.854 0-6.973 3.12-6.973 6.969 0 3.854 3.12 6.975 6.973 6.975 3.849 0 6.969-3.12 6.969-6.975 0-3.849-3.12-6.969-6.969-6.969z"></path>
<path d="m9.018 5.813c-.063 0-.129 0-.191.004-1.881.131-2.429 2.636-.777 3.542l7.473 4.229c.672.38 1.161 1.01 1.371 1.755l12.438 44.489c1.202 4.297 5.14 7.292 9.598 7.292h31.276c2.521 0 2.521-3.781 0-3.781h-31.276c-2.781 0-5.203-1.839-5.953-4.521l-.599-2.146h32.771c4.708 0 8.88-3.094 10.246-7.599l4.254-14 3.171-10.447c.735-2.412-2.879-3.51-3.615-1.098l-3.177 10.451-4.25 13.996c-.891 2.927-3.568 4.916-6.631 4.916h-33.827l-10.781-38.568c-.475-1.707-1.615-3.155-3.156-4.03l-7.464-4.224c-.276-.162-.583-.251-.901-.26z"></path>
<path d="m51.236 11.822c7.917 0 14.287 6.386 14.287 14.303 0 7.916-6.37 14.292-14.287 14.292-7.921 0-14.291-6.376-14.291-14.292.001-7.917 6.37-14.303 14.291-14.303zm0-3.776c-9.962 0-18.072 8.115-18.072 18.079 0 9.958 8.109 18.067 18.072 18.067 9.959 0 18.068-8.109 18.068-18.067.001-9.964-8.109-18.079-18.068-18.079z"></path>
<path d="m58.34 19.125c-.557-.005-1.093.234-1.452.656l-8.396 9.511c-.043.047-.109.052-.156.005l-2.85-2.776c-.745-.745-1.958-.734-2.693.021-.733.756-.708 1.97.058 2.693l2.85 2.771c1.579 1.536 4.172 1.438 5.625-.213l8.396-9.506c1.093-1.209.249-3.141-1.382-3.162z"></path>
</svg>
```

Não alterar os paths.

Pode alterar apenas:

- tamanho;
- posicionamento;
- `opacity`;
- `transform`;
- `filter` visual extremamente sutil, se necessário para integração.

---

# 64. MAPA OFICIAL DOS DOIS HERO ASSETS

```text
┌─────────────────────────────────────────────────────────┐
│ ESTADO                         ASSET                     │
├─────────────────────────────────────────────────────────┤
│ IDLE / CAIXA LIVRE             fi_9725740               │
│                                 SVG grande              │
│                                 sem círculo             │
│                                 centralizado            │
│                                                         │
│ PAYMENT / CHECKOUT              fi_13791848              │
│                                 SVG de checkout         │
│                                 centralizado            │
│                                                         │
│ COMPLETED                       usar feedback de        │
│                                 conclusão + animação    │
└─────────────────────────────────────────────────────────┘
```

---

# 65. REGRA DE SEPARAÇÃO ENTRE ÍCONES

Não confundir:

```text
Phosphor
    ↓
ícones de operação
```

com:

```text
SVG fi_9725740
SVG fi_13791848
    ↓
Hero Assets / estados
```

Exemplo:

```text
F2 Produto       → Phosphor ShoppingCart
F3 Cliente       → Phosphor UserCircle
F8 Consultar     → Phosphor MagnifyingGlass
F9 Pagamento     → Phosphor CreditCard

Tela principal   → SVG fi_9725740
Checkout         → SVG fi_13791848
```

---

# 66. COMPONENTES DOS HERO ASSETS

Criar uma abstração semelhante a:

```text
<HeroCart />
```

e:

```text
<CheckoutHero />
```

Responsabilidades:

```text
HeroCart
├── renderiza SVG fi_9725740
├── controla tamanho
├── controla animação idle
└── respeita reduced-motion

CheckoutHero
├── renderiza SVG fi_13791848
├── controla tamanho
├── controla entrada
└── respeita reduced-motion
```

Não duplicar o SVG em vários lugares do código.

Centralizar os assets.

---

# 67. LOCALIZAÇÃO DOS ARQUIVOS

Se o projeto possuir estrutura de assets, criar algo equivalente a:

```text
src/
└── assets/
    └── illustrations/
        ├── cart-idle.svg
        └── cart-checkout.svg
```

Nomes recomendados:

```text
cart-idle.svg
cart-checkout.svg
```

Se a estrutura atual do projeto usar outra convenção, respeitar a convenção existente.

---

# 68. IMPORTANTE — SVG INLINE OU ARQUIVO

Preferência:

```text
SVG como asset separado
```

quando não houver necessidade de alterar os paths dinamicamente.

Se a animação exigir controle direto de elementos internos do SVG, pode utilizar SVG inline/componentizado.

Não transformar automaticamente o SVG em PNG.

**Preservar vetor.**

---

# 69. COMPORTAMENTO FINAL DO FLUXO

A experiência desejada:

```text
                    IDLE
                     │
                     │
            ┌────────▼────────┐
            │  SVG CARRINHO   │
            │   fi_9725740    │
            └────────┬────────┘
                     │
              leitura produto
                     │
                     ▼
                  SALE
                     │
                     │ F9
                     ▼
                 CHECKOUT
                     │
            ┌────────▼────────┐
            │ SVG CHECKOUT     │
            │  fi_13791848    │
            └────────┬────────┘
                     │
                pagamento
                     │
                     ▼
                COMPLETED
                     │
                     │
                     ▼
                    IDLE
```

O objetivo é que o usuário perceba visualmente a mudança de estado.

---

# 70. PROIBIÇÕES ESPECÍFICAS

Para esta implementação, **não fazer**:

- [ ] Não colocar círculo ao redor do `fi_9725740`.
- [ ] Não colocar o `fi_9725740` dentro de um card branco.
- [ ] Não trocar o SVG principal por Phosphor.
- [ ] Não converter os SVGs em PNG.
- [ ] Não usar emoji de carrinho.
- [ ] Não misturar o SVG principal com outro carrinho diferente.
- [ ] Não usar o SVG de checkout no estado IDLE.
- [ ] Não deixar o SVG principal pequeno como na implementação atual.
- [ ] Não posicionar o hero de forma que pareça perdido no espaço.
- [ ] Não adicionar efeitos neon exagerados.
- [ ] Não modificar os paths dos SVGs.

---

# 71. CRITÉRIO VISUAL PRINCIPAL

Antes:

```text
        ◯
       🛒

   CAIXA LIVRE
```

**NÃO queremos mais isso.**

Depois:

```text
              ┌───────────────────┐
              │                   │
              │   SVG GRANDE      │
              │  fi_9725740       │
              │                   │
              └───────────────────┘

                CAIXA LIVRE

             ● SCANNER PRONTO

      Aproxime o produto do scanner

              [ F2 ] Produtos
```

O `SVG GRANDE` representa o próprio asset e **não deve possuir círculo externo**.

A composição precisa parecer intencional, sofisticada e própria de um produto comercial.

---

# 72. DIRETRIZ PARA A PRÓXIMA IMPLEMENTAÇÃO

Antes de finalizar, comparar a implementação com estes dois assets oficiais.

### Tela principal

Perguntar:

> O carrinho é imediatamente perceptível?

> Está grande?

> Está centralizado?

> Existe algum círculo ou moldura desnecessária?

> O espaço ao redor parece intencional?

### Checkout

Perguntar:

> O segundo SVG aparece somente no contexto correto?

> O checkout parece uma mudança real de estado?

> O SVG possui destaque suficiente?

> O valor a pagar continua sendo o principal elemento financeiro?

---

# 73. RESULTADO ESPERADO

A tela principal deve transmitir:

```text
CAIXA PRONTO
      +
SCANNER PRONTO
      +
CARRINHO EM ESPERA
      +
AÇÃO CLARA
```

O checkout deve transmitir:

```text
VENDA EM PROCESSAMENTO
      +
PAGAMENTO
      +
CONFIRMAÇÃO
      +
TOTAL CLARO
```

**Esses dois SVGs passam a ser parte oficial da identidade visual do Caixa Mercado.**

# 61. PRINCÍPIO ARQUITETURAL FINAL

Não queremos simplesmente:

> "deixar o centro mais bonito."

Queremos construir:

```text
                    WORKSPACE
                       │
          ┌────────────┼────────────┐
          │            │            │
       AMBIENTE      ESTADO       AÇÃO
          │            │            │
       Background   HeroCart      F2
       Grid         Status        Scanner
       Halo         Message       Código
          │            │            │
          └────────────┼────────────┘
                       │
                  STATE MACHINE
                       │
       ┌───────────────┼────────────────┐
       │               │                │
      IDLE            SALE           PAYMENT
       │                                │
       └───────────────┬────────────────┘
                       │
                   COMPLETED
                       │
                  🛒 ─────→
                       │
                       ▼
                      IDLE
```

O resultado deve ser uma interface em que:

> **o estado da venda controla a experiência visual.**

O operador não precisa ficar pensando no que está acontecendo.

A interface comunica.

---

# 62. DIRETRIZ FINAL PARA O ANTIGRAVITY

**Não implemente este documento como uma coleção de efeitos.**

Primeiro entenda a arquitetura atual.

Depois:

```text
AUDITAR
   ↓
MODELAR ESTADOS
   ↓
CRIAR DESIGN TOKENS
   ↓
CRIAR COMPONENTES
   ↓
IMPLEMENTAR EMPTY STATE
   ↓
IMPLEMENTAR SALE STATE
   ↓
IMPLEMENTAR PAYMENT
   ↓
IMPLEMENTAR COMPLETED
   ↓
ANIMAR
   ↓
REFINAR
   ↓
TESTAR
```

Se existir uma decisão técnica melhor que a sugerida aqui, pode adaptá-la, mas preserve os princípios:

```text
consistência
+
performance
+
clareza
+
reutilização
+
identidade visual
+
operação rápida
```

**O objetivo não é criar uma tela bonita.**

**O objetivo é criar a experiência visual do Caixa Mercado.**
