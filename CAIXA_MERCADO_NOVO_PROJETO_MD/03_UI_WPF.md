# UI/UX — WPF

## Objetivo
A interface deve parecer um **software desktop profissional de PDV**, não uma aplicação SaaS.

## Estrutura
```text
┌──────────────────────────────────────────────────────────────────────┐
│ CAIXA MERCADO | FILIAL 01 | PDV 01 | OPERADOR: CARLOS | CAIXA ABERTO│
├────────────┬─────────────────────────────────────────────────────────┤
│ OPERAÇÕES  │ VENDA                                                   │
│            │                                                         │
│ F2 Produto │ Código de barras / PLU                                  │
│ F3 Cliente │ [____________________________________________]          │
│ F4 Qtd     │                                                         │
│ F6 Desconto│ ┌─────────────────────────────────────────────────────┐ │
│ F9 Pagamento││ Código │ Produto │ Qtd │ Un │ Unit │ Total         │ │
│            │├─────────────────────────────────────────────────────┤ │
│ CAIXA      ││ 001    │ Arroz   │  2  │ UN │22,90 │ 45,80         │ │
│ Abertura   │└─────────────────────────────────────────────────────┘ │
│ Sangria    │                                                         │
│ Suprimento │                              TOTAL R$ 45,80             │
│ Fechamento │                              [ F9 PAGAMENTO ]           │
├────────────┴─────────────────────────────────────────────────────────┤
│ Venda 000001 | Itens 2 | PDV 01 | Carlos | Caixa ABERTO | ONLINE   │
└──────────────────────────────────────────────────────────────────────┘
```

## Usar
- DataGrid;
- Menu;
- ToolBar;
- campos;
- botões;
- dialogs;
- status;
- tabs;
- comandos;
- ResourceDictionary;
- Styles e Templates.

## Evitar
- cards gigantes;
- gradientes;
- neon;
- glow;
- glassmorphism;
- estética de dashboard;
- excesso de arredondamento;
- animações desnecessárias.

## Teclado
F2 Produto, F3 Cliente, F4 Quantidade, F6 Desconto, F7 Cancelar item, F8 Consultar venda, F9 Pagamento, ESC Cancelar, ENTER Confirmar, DELETE Remover.

## Scanner
```text
EAN → buscar → adicionar → atualizar total → foco volta ao campo
```

## Resoluções
Priorizar 1280x720, 1366x768 e 1920x1080.

## Estados
Toda tela deve considerar loading, vazio, sucesso, erro e bloqueado.
