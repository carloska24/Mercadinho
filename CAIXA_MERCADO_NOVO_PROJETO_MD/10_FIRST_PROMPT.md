# PROMPT INICIAL

Você está iniciando um projeto NOVO chamado **Caixa Mercado**.

Leia TODOS os arquivos `.md` deste diretório antes de implementar.

A documentação é a especificação oficial inicial.

## Stack
- C#
- .NET 10 LTS
- WPF
- XAML
- MVVM
- PostgreSQL
- Entity Framework Core
- ASP.NET Core quando necessário

## Importante
Não reutilize o projeto React anterior.
Não transforme o projeto em aplicação web.
Não crie um SaaS.
Não crie um dashboard.

O objetivo é um aplicativo desktop de frente de caixa.

## Antes de codificar
Analise:
1. SDK .NET instalado;
2. versão;
3. ferramentas;
4. estrutura;
5. dependências;
6. arquitetura;
7. estratégia de testes.

Depois apresente o plano.

## Primeira implementação
Crie:
```text
CaixaMercado.PDV
CaixaMercado.Domain
CaixaMercado.Application
CaixaMercado.Infrastructure
CaixaMercado.Api
tests
```

Se algum projeto não for necessário imediatamente, mantenha a arquitetura simples.

## Primeiro objetivo funcional
Abrir uma janela WPF profissional contendo:
- header;
- menu;
- campo EAN/PLU;
- DataGrid;
- resumo;
- total;
- atalhos;
- status.

Pode usar dados mock temporariamente.

Não implementar ainda:
- TEF real;
- PIX real;
- NFC-e real;
- balança real;
- impressora real;
- gaveta real.

Depois da primeira tela compilando e executando, pare e valide.
