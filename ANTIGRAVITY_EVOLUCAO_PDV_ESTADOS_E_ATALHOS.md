# ANTIGRAVITY — EVOLUÇÃO DA INTERFACE DO PDV
## Sistema Caixa Mercado — Estados Visuais, Animações e Barra de Atalhos

> **IMPORTANTE:** Não quero que você gere uma imagem/mockup.  
> Quero que você implemente essas mudanças **na interface real do projeto**, preservando a funcionalidade existente.

---

# 1. CONTEXTO

A interface atual do PDV já possui uma boa estrutura:

- Cabeçalho com filial, PDV, operador e status do caixa
- Campo de quantidade
- Campo de código de barras / EAN / PLU
- Área principal "VENDA EM ANDAMENTO"
- Painel "ÚLTIMO ITEM ADICIONADO"
- "RESUMO DA VENDA"
- Área de "TOTAL A PAGAR"
- Barra inferior com atalhos F2, F3, F4, F6, F7, F8, F9, ESC e DEL
- Status da venda no rodapé

A aparência atual deve ser preservada como base.

## NÃO fazer

- Não reconstruir o projeto inteiro.
- Não trocar a stack sem necessidade.
- Não remover funcionalidades existentes.
- Não transformar o PDV em um dashboard.
- Não adicionar banners, gráficos ou informações desnecessárias.
- Não colocar animações exageradas.
- Não prejudicar a velocidade de operação do caixa.

O objetivo é **evoluir a UX/UI**, não descaracterizar o sistema.

---

# 2. CONCEITO PRINCIPAL

Quero transformar a área central do PDV em um **sistema visual de estados**.

O carrinho não será apenas um ícone decorativo.

Ele deve representar visualmente o estado atual da operação.

A experiência deve funcionar aproximadamente assim:

```text
                    🛒
                     │
                     ▼
              ┌───────────────┐
              │  CAIXA LIVRE  │
              │   aguardando  │
              │    produto    │
              └───────┬───────┘
                      │
                 produto lido
                      │
                      ▼
              ┌───────────────┐
              │     VENDA     │
              │ EM ANDAMENTO  │
              └───────┬───────┘
                      │
                 F9 pagamento
                      │
                      ▼
              ┌───────────────┐
              │   PAGAMENTO   │
              │    R$ XX,XX   │
              └───────┬───────┘
                      │
                  aprovado
                      │
                      ▼
                 🛒 ───────→
                      ✨
                      │
                      ▼
              ┌───────────────┐
              │ ✓ CONCLUÍDA   │
              └───────┬───────┘
                      │
                      ▼
                     🛒
                CAIXA LIVRE
```

---

# 3. ESTADO 01 — CAIXA LIVRE

Quando não houver nenhum item na venda, manter a área central como um **Empty State premium**.

Visual desejado:

```text
                         ·
                   ·           ·

                       🛒

                    CAIXA LIVRE

             Aguardando leitura do produto

                [ F2 — Consultar ]

                         ·
```

## Carrinho

Usar um **SVG**, preferencialmente um SVG próprio ou uma biblioteca já utilizada pelo projeto.

Não usar emoji.

Não usar PNG.

Não usar GIF.

O SVG deve:

- ser grande o suficiente para ser percebido;
- ter aproximadamente 110–160 px;
- utilizar a identidade azul/ciano do sistema;
- ter contorno moderno;
- possuir halo/glow muito discreto;
- funcionar perfeitamente em fundo escuro;
- ser nítido em diferentes resoluções.

---

# 4. ANIMAÇÃO DO CARRINHO — ESTADO IDLE

O carrinho deve possuir uma animação muito sutil enquanto o caixa estiver livre.

A intenção é transmitir:

> "O caixa está pronto e aguardando."

Não quero uma animação chamativa.

### Sugestão

Uma combinação de:

- pequena variação de escala;
- pequeno deslocamento vertical;
- brilho suave;
- eventualmente pequenos elementos decorativos;
- duração longa;
- easing suave;
- repetição infinita.

Exemplo conceitual:

```text
       🛒
       ↑
       ↓
       ↑
       ↓
```

Movimento extremamente pequeno.

O operador deve conseguir trabalhar durante horas sem a animação incomodar.

### Regra

Se `venda.itens.length === 0`:

```text
mostrar Empty State
mostrar carrinho animado
mostrar "CAIXA LIVRE"
```

---

# 5. ESTADO 02 — PRIMEIRO PRODUTO LIDO

Quando o primeiro produto for adicionado:

O Empty State deve desaparecer de maneira suave.

Não deve simplesmente "sumir" instantaneamente.

Sugestão:

```text
Carrinho idle
      ↓
pequeno movimento
      ↓
fade/slide out
      ↓
lista de produtos aparece
```

A área passa a mostrar os produtos normalmente.

Exemplo:

```text
┌──────────────────────────────────────────────┐
│ VENDA EM ANDAMENTO                  3 itens │
├──────────────────────────────────────────────┤
│ PRODUTO                         QTD    TOTAL │
│                                              │
│ Arroz Tipo 1 — 5kg              1     25,90 │
│ Feijão Carioca — 1kg            2     15,80 │
│ Refrigerante 2L                 1      8,99 │
│                                              │
└──────────────────────────────────────────────┘
```

---

# 6. FEEDBACK AO ADICIONAR PRODUTO

Toda leitura de produto deve possuir um pequeno feedback visual.

Exemplo:

```text
Produto lido
     ↓
linha aparece
     ↓
pequeno highlight
     ↓
valor atualiza
     ↓
highlight desaparece
```

Não exagerar.

O objetivo é permitir ao operador perceber:

> "O sistema recebeu o produto."

---

# 7. ESTADO 03 — PAGAMENTO

Ao pressionar `F9` e entrar no pagamento, a interface deve mudar claramente de contexto.

Exemplo:

```text
┌──────────────────────────────────────────────┐
│                                              │
│                  PAGAMENTO                   │
│                                              │
│                 TOTAL A PAGAR                │
│                                              │
│                    R$ 87,40                  │
│                                              │
│           Selecione a forma de pagamento     │
│                                              │
└──────────────────────────────────────────────┘
```

O estado deve ser visualmente diferente, mas manter a mesma identidade.

---

# 8. ESTADO 04 — PAGAMENTO APROVADO

Depois que o pagamento for realmente confirmado:

Mostrar uma confirmação curta.

Exemplo:

```text
                     ✓

               VENDA CONCLUÍDA

                   R$ 87,40

              Pagamento aprovado
```

O check pode possuir uma pequena animação de entrada.

---

# 9. A IDEIA MAIS IMPORTANTE — CARRINHO "PARTINDO"

Depois da confirmação da venda, quero uma animação especial.

O carrinho deve representar que aquela compra "foi embora".

Conceito:

```text
                ✓

          VENDA CONCLUÍDA

             R$ 87,40


              🛒 ───────→
                   ✨
```

Ou:

```text
          🛒
           \
            \
             \────────→
```

## Comportamento

1. Pagamento aprovado.
2. Mostrar check.
3. Mostrar "VENDA CONCLUÍDA".
4. Mostrar valor final.
5. Carrinho aparece.
6. Carrinho se desloca horizontalmente para fora da área.
7. Pequeno efeito de brilho/partículas pode acompanhar.
8. Carrinho desaparece.
9. Estado retorna para "CAIXA LIVRE".
10. Novo carrinho entra/retorna ao centro.
11. Animação idle começa novamente.

### IMPORTANTE

Essa animação deve ser curta.

Algo na faixa de:

```text
600ms – 1200ms
```

Não bloquear o operador.

---

# 10. CICLO COMPLETO

Implementar o seguinte fluxo visual:

```text
IDLE
 │
 │ produto adicionado
 ▼
VENDA
 │
 │ F9
 ▼
PAGAMENTO
 │
 │ pagamento aprovado
 ▼
CONCLUÍDA
 │
 │ animação carrinho partindo
 ▼
IDLE
```

A máquina de estados deve ser clara no código.

Preferencialmente algo conceitual como:

```text
IDLE
SALE
PAYMENT
COMPLETED
```

ou equivalente à arquitetura atual do projeto.

Não duplicar lógica.

Não criar vários estados paralelos desnecessários.

---

# 11. CUIDADO COM O FLUXO REAL

A animação de "venda concluída" **só deve acontecer depois de a venda realmente ter sido concluída**.

Não disparar a animação apenas porque o usuário abriu a tela de pagamento.

Fluxo correto:

```text
F9
 ↓
Pagamento
 ↓
Forma de pagamento selecionada
 ↓
Pagamento confirmado
 ↓
Venda registrada/finalizada
 ↓
ANIMAÇÃO DE CONCLUSÃO
```

---

# 12. BARRA DE ATALHOS — REFAZER A HIERARQUIA VISUAL

A barra inferior atual funciona, mas pode ficar muito melhor.

Atualmente os atalhos estão muito "espremidos".

Quero transformar cada atalho em um **mini botão de operação**, com:

1. tecla de atalho em destaque;
2. ícone claramente visível;
3. nome da função.

---

# 13. NOVO PADRÃO DOS ATALHOS

Em vez de:

```text
🛒 F2 Produto
👤 F3 Cliente
▣ F4 Qtd
🏷 F6 Desconto
× F7 Canc Item
⌕ F8 Consultar
▣ F9 Pagamento
⊘ ESC Cancelar
▣ DEL Remover
```

usar uma composição mais organizada:

```text
┌────────────────┐
│  🛒   F2       │
│       Produto  │
└────────────────┘
```

ou preferencialmente:

```text
┌────────────────┐
│ 🛒  F2         │
│     Produto    │
└────────────────┘
```

A tecla deve ter **peso visual maior** que o texto da função.

---

# 14. ESTRUTURA DOS BOTÕES

Cada botão deve possuir:

```text
┌──────────────────────────┐
│  [ÍCONE]   [F2]          │
│            Produto       │
└──────────────────────────┘
```

Ou em telas menores:

```text
┌──────────────────────────┐
│ [ÍCONE]  [F2] Produto    │
└──────────────────────────┘
```

Escolher automaticamente a melhor composição de acordo com a largura disponível.

---

# 15. TECLA DE ATALHO COMO ELEMENTO VISUAL

A tecla deve parecer uma tecla real do teclado, mas sem exagero.

Exemplo:

```text
┌────┐
│ F2 │
└────┘
```

Para teclas especiais:

```text
┌─────┐
│ ESC │
└─────┘
```

```text
┌─────┐
│ DEL │
└─────┘
```

Visualmente:

- pequena;
- borda discreta;
- contraste suficiente;
- texto legível;
- tamanho consistente.

---

# 16. ÍCONES DOS ATALHOS

Os ícones precisam ser mais visíveis que os atuais.

Usar SVG/icon library já existente no projeto.

Sugestão sem obrigatoriedade:

| Atalho | Função | Ícone sugerido |
|---|---|---|
| F2 | Produto | shopping cart / search |
| F3 | Cliente | user |
| F4 | Quantidade | list / layers |
| F6 | Desconto | tag / percent |
| F7 | Cancelar item | x-circle |
| F8 | Consultar | search |
| F9 | Pagamento | credit-card / wallet |
| ESC | Cancelar | ban / x |
| DEL | Remover | trash |

### IMPORTANTE

Não escolher os ícones apenas por aparência.

Eles devem ser semanticamente óbvios.

---

# 17. CORES DOS ÍCONES

Não quero cada botão com uma cor extremamente diferente.

Usar a paleta do sistema.

Sugestão:

```text
Ações normais
→ azul/ciano

Pagamento
→ verde

Desconto
→ amarelo

Cancelamento
→ vermelho

Funções neutras
→ azul/cinza
```

As cores devem ser discretas.

---

# 18. TAMANHO DOS BOTÕES

A barra precisa ter boa área clicável, mesmo sendo principalmente operada pelo teclado.

Cada botão deve:

- possuir altura confortável;
- ter padding interno;
- possuir ícone de tamanho legível;
- separar visualmente a tecla;
- ter espaço suficiente entre elementos;
- manter alinhamento perfeito.

Não deixar tudo colado.

---

# 19. HIERARQUIA DA BARRA

Quero que visualmente o operador consiga identificar rapidamente:

```text
ÍCONE → O QUE FAZ
TECLA → COMO ACESSAR
```

Exemplo:

```text
 🛒   F2
      Produto
```

O olhar deve encontrar primeiro a função.

Depois a tecla.

---

# 20. ESTADOS DOS BOTÕES

Os atalhos devem ter estados visuais.

### Normal

```text
┌────────────────┐
│ 🛒   F2         │
│      Produto    │
└────────────────┘
```

### Hover

Pequeno aumento de brilho/borda.

### Pressionado

Quando F2 for pressionado:

```text
┌────────────────┐
│ 🛒  [F2]       │
│      Produto    │
└────────────────┘
```

Pequena redução visual / highlight.

### Desabilitado

Exemplo F9 quando não há produtos:

```text
┌────────────────┐
│ 💳  F9         │
│     Pagamento  │
└────────────────┘
```

Com aparência claramente desabilitada.

---

# 21. TECLADO CONTINUA SENDO O PROTAGONISTA

Isso é extremamente importante.

O PDV é uma aplicação operacional.

Portanto:

> Não transformar os botões inferiores em botões de aplicativo mobile.

Eles são uma **referência visual para atalhos de teclado**.

O operador deve conseguir trabalhar praticamente sem mouse.

---

# 22. ACESSIBILIDADE VISUAL

Garantir:

- contraste suficiente;
- textos legíveis;
- ícones não excessivamente finos;
- foco de teclado claramente visível;
- estados disabled distinguíveis;
- não depender somente de cor para indicar estado.

---

# 23. ÁREA CENTRAL — NÃO EXAGERAR

O Empty State deve preencher melhor o espaço vazio, mas sem parecer um banner.

Não colocar uma ilustração enorme.

O centro deve continuar respirando.

Sugestão:

```text
                 ·        ·

                      🛒

                 CAIXA LIVRE

             Aguardando produto

                 [ F2 ]

                 ·        ·
```

O carrinho pode ocupar mais espaço que o atual, mas a composição deve permanecer elegante.

---

# 24. BACKGROUND DO EMPTY STATE

Pode existir uma decoração muito sutil.

Exemplos:

- pequenos pontos;
- círculos;
- linhas;
- glow radial;
- grid quase invisível.

Opacidade baixa.

O objetivo é tirar a sensação de "área vazia", não criar um wallpaper.

---

# 25. TOTAL A PAGAR

Manter o painel atual, mas melhorar sua percepção.

O valor deve continuar sendo o elemento dominante:

```text
TOTAL A PAGAR

R$ 0,00
```

Quando houver venda:

```text
TOTAL A PAGAR

R$ 87,40
```

Quando o total mudar, pode haver uma microanimação de atualização.

Nunca atrasar a atualização do valor real.

---

# 26. ÚLTIMO ITEM ADICIONADO

Aproveitar melhor esse painel.

Estado vazio:

```text
NENHUM ITEM REGISTRADO

Valor do item:
R$ 0,00
```

Com produto:

```text
ÚLTIMO ITEM ADICIONADO

Arroz Tipo 1 — 5kg

QTD             1
VALOR           R$ 29,90
```

Para produtos pesáveis:

```text
BANANA PRATA

1,245 kg
R$ 8,99/kg

TOTAL
R$ 11,19
```

---

# 27. REGRAS DE PERFORMANCE

As animações devem ser leves.

Preferir:

- CSS transitions;
- CSS keyframes;
- transform;
- opacity;
- SVG.

Evitar:

- vídeos;
- GIFs;
- imagens pesadas;
- bibliotecas enormes somente para uma animação;
- efeitos que provoquem layout thrashing.

Não comprometer o desempenho do PDV.

---

# 28. REDUCED MOTION

Respeitar preferência do sistema:

```css
@media (prefers-reduced-motion: reduce)
```

Nesse caso:

- reduzir animações;
- eliminar deslocamentos;
- manter apenas mudanças de estado instantâneas ou fade muito discreto.

---

# 29. NÃO CRIAR UMA "ANIMAÇÃO DE ENFEITE"

Cada animação precisa comunicar alguma coisa.

```text
Carrinho respirando
→ caixa pronto

Produto entrando
→ produto recebido

Valor mudando
→ venda atualizada

Check
→ pagamento aprovado

Carrinho partindo
→ venda finalizada

Novo carrinho
→ próximo atendimento
```

Esse é o princípio.

---

# 30. IMPLEMENTAÇÃO

Antes de modificar:

1. Analise a arquitetura atual.
2. Identifique onde o estado da venda é controlado.
3. Identifique onde os itens são adicionados/removidos.
4. Identifique onde o pagamento é finalizado.
5. Identifique os componentes da barra de atalhos.
6. Identifique a biblioteca de ícones já utilizada.
7. Reutilize componentes existentes quando possível.

Depois implemente.

Não duplicar lógica.

Não criar componentes paralelos sem necessidade.

---

# 31. COMPONENTIZAÇÃO SUGERIDA

Se a stack permitir, organizar conceitualmente:

```text
POS
├── Header
├── ProductInput
├── SaleWorkspace
│   ├── EmptySaleState
│   ├── SaleItems
│   ├── PaymentState
│   └── SaleCompletedState
├── LastItemPanel
├── SaleSummary
├── TotalPanel
├── ShortcutBar
│   └── ShortcutButton
└── StatusBar
```

Os nomes podem ser adaptados à arquitetura existente.

---

# 32. SISTEMA DE ESTADOS VISUAIS

Criar uma fonte única de verdade para o estado visual.

Conceito:

```text
IDLE
SALE
PAYMENT
COMPLETED
```

### IDLE

```text
Carrinho idle
CAIXA LIVRE
Aguardando produto
```

### SALE

```text
Lista de produtos
Total atualizado
Último item
```

### PAYMENT

```text
Tela/área de pagamento
Total em destaque
Formas de pagamento
```

### COMPLETED

```text
Check
Venda concluída
Valor
Carrinho partindo
```

Depois:

```text
COMPLETED → IDLE
```

---

# 33. TRANSIÇÕES

As transições devem ser curtas.

Sugestão:

```text
Idle → Sale
300–500ms

Sale → Payment
200–400ms

Payment → Completed
300–500ms

Completed → Idle
600–1200ms
```

A última pode ser um pouco mais longa por causa da animação do carrinho.

---

# 34. O CARRINHO "PARTINDO"

Essa animação merece atenção especial.

Não quero simplesmente:

```text
transform: translateX(100%);
```

Quero que pareça que o carrinho realmente está saindo.

Sugestão visual:

```text
          🛒
           \
            \
             \──────→
                   ·
                 ·
               ✦
```

Pode combinar:

- translateX;
- pequeno translateY;
- leve rotação;
- fade;
- partículas simples em SVG/CSS;
- easing de aceleração.

Mas manter a animação elegante.

---

# 35. ENTRADA DO NOVO CARRINHO

Depois da conclusão:

```text
            ←────── 🛒
```

ou simplesmente:

```text
             🛒
              ↓
           posição
           central
```

O novo carrinho entra no estado idle.

Depois começa a animação de respiração.

---

# 36. SOM — OPCIONAL

Não adicionar sons automaticamente se o projeto ainda não possuir infraestrutura para isso.

Se já houver suporte, futuramente podemos ter:

```text
Produto lido
→ beep curto

Pagamento aprovado
→ confirmação curta

Erro
→ alerta
```

Mas isso deve ser configurável.

O sistema precisa funcionar perfeitamente sem som.

---

# 37. RESULTADO ESPERADO

Quero que a interface transmita:

```text
PROFISSIONAL
MODERNA
RÁPIDA
OPERACIONAL
LIMPA
VIVA
```

Mas sem parecer:

```text
GAME
SITE DE E-COMMERCE
DASHBOARD
APLICATIVO MOBILE
```

---

# 38. CRITÉRIO DE ACEITAÇÃO

Considere a implementação concluída somente quando:

### Empty State

- [ ] SVG de carrinho está centralizado.
- [ ] SVG possui animação idle sutil.
- [ ] Estado "CAIXA LIVRE" está claro.
- [ ] Empty State desaparece ao adicionar produto.

### Venda

- [ ] Produtos aparecem normalmente.
- [ ] Primeiro item produz transição suave.
- [ ] Último item é atualizado.
- [ ] Total é atualizado imediatamente.

### Pagamento

- [ ] F9 abre o fluxo existente.
- [ ] Estado visual de pagamento é claro.
- [ ] Pagamento aprovado gera confirmação.

### Conclusão

- [ ] Check aparece.
- [ ] "VENDA CONCLUÍDA" aparece.
- [ ] Valor final aparece.
- [ ] Carrinho faz animação de saída.
- [ ] Sistema retorna ao estado "CAIXA LIVRE".
- [ ] Novo carrinho volta ao estado idle.

### Barra de atalhos

- [ ] Ícones estão claramente visíveis.
- [ ] Teclas F2/F3/F4/F6/F7/F8/F9/ESC/DEL estão destacadas.
- [ ] Existe espaçamento suficiente.
- [ ] Texto da função permanece legível.
- [ ] Estados hover/pressed/disabled funcionam.
- [ ] Atalhos continuam funcionando pelo teclado.
- [ ] A barra não ocupa espaço excessivo.

### Qualidade

- [ ] Não há regressão nas funções existentes.
- [ ] Não há animações exageradas.
- [ ] Não há imagens pesadas desnecessárias.
- [ ] Reduced Motion é respeitado.
- [ ] Layout continua funcionando na resolução atual.
- [ ] Código permanece organizado e reutilizável.

---

# 39. IMPORTANTE — NÃO IMPLEMENTAR CEGAMENTE

Antes de codificar, faça uma análise da interface atual.

Se alguma sugestão deste documento entrar em conflito com a arquitetura existente, **preserve a arquitetura e adapte a implementação**.

Não quero uma reescrita desnecessária.

Quero evolução incremental.

---

# 40. VISÃO FINAL

A tela deve ter esta personalidade:

```text
┌──────────────────────────────────────────────────────────────┐
│                    CAIXA MERCADO                             │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│                  ENTRADA DO PRODUTO                          │
│                                                              │
├──────────────────────────────────────────┬───────────────────┤
│                                          │ ÚLTIMO ITEM        │
│             ESTADO DA VENDA              ├───────────────────┤
│                                          │ RESUMO             │
│                  🛒                      ├───────────────────┤
│                                          │                   │
│             CAIXA LIVRE                  │   TOTAL A PAGAR   │
│                                          │                   │
│        Aguardando produto                │     R$ 0,00       │
│                                          │                   │
├──────────────────────────────────────────┴───────────────────┤
│  🛒 F2  │ 👤 F3 │ ≡ F4 │ 🏷 F6 │ ✕ F7 │ 🔍 F8 │ 💳 F9 │ ESC │ DEL │
├──────────────────────────────────────────────────────────────┤
│ VENDA 1001     CAIXA LIVRE — AGUARDANDO PRODUTO       ONLINE │
└──────────────────────────────────────────────────────────────┘
```

A grande diferença é que **essa tela não fica realmente "parada"**.

Ela possui vida visual, mas essa vida existe para comunicar o estado da operação.

---

# 41. PRINCÍPIO FINAL

## O PDV deve contar visualmente a história da venda.

```text
🛒
"Estou pronto."

        ↓

📦
"Recebi um produto."

        ↓

🧾
"Existe uma venda."

        ↓

💳
"Estamos pagando."

        ↓

✓
"Pagamento aprovado."

        ↓

🛒 ─────→
"A compra foi concluída."

        ↓

🛒
"Estou pronto para o próximo cliente."
```

**Essa é a experiência que quero implementar.**

Não quero apenas um carrinho bonito.

Quero que o carrinho, os estados, os ícones, os atalhos e as transições formem uma **linguagem visual própria do Caixa Mercado**.
