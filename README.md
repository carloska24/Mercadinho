# 🛒 Caixa Mercado — Frente de Caixa & Sistema PDV Multicaixa

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![WPF](https://img.shields.io/badge/WPF-Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white)](https://learn.microsoft.com/dotnet/desktop/wpf/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4?style=for-the-badge&logo=nuget&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![Swagger](https://img.shields.io/badge/OpenAPI-Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)](https://swagger.io/)
[![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)](LICENSE)

O **Caixa Mercado** é um sistema completo e de altíssima performance para **Frente de Caixa (PDV)** e **Gestão de Varejo**, projetado especialmente para minimercados, padarias, açougues e comércio local. 

Construído sob os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, o sistema conta com uma interface desktop WPF ultra-responsiva e ergonômica para operadores de caixa, conectada a uma API centralizada em .NET 8 com persistência PostgreSQL.

---

## 🌟 Destaques da Interface & UX/UI Design

- **Ergonomia Operacional:** Interface desenhada para operadores de caixa, priorizando leitura rápida, contraste calibrado e operação 100% via teclado/scanner.
- **Teclas de Atalho de Acesso Direto:** Todos os atalhos (`F2`, `F4`, `F6`, `F9`, `ESC`, `DEL`) visivelmente destacados com atalhos contextuais.
- **Estado Vazio Interativo (Scanner Edition):** Ícone de scanner de código de barras com iluminação laser animada (`SineEase`), retículo de alinhamento e anel de pulso do status de prontidão.
- **Hierarquia Visual Forte:**
  - **Total a Pagar:** Painel em destaque com tipografia `52px` e fundo verde suave para legibilidade a distância.
  - **Ação Principal de Pagamento (`[F9]`):** Botão com cor dedicada (`#16A34A`), elevação visual (`DropShadowEffect`) e feedback tátil ao clicar.
  - **Ações Destrutivas:** Botões de cancelamento (`ESC`, `F7`) e remoção (`DEL`) com aviso visual em tom vermelho (`DangerShortcutStyle`).
- **Suporte a Temas (Light / Dark Mode):** Suporte nativo a temas dinâmicos via `ResourceDictionary`.
- **Zero Bloco de Seleção / Alinhamento Perfeito:** Campos de texto com alinhamento vertical perfeito via `ControlTemplate` customizado e seleção suave.

---

## 🛠️ Stacks & Tecnologias Utilizadas

### **Frontend / Cliente Desktop**
- ![WPF](https://img.shields.io/badge/WPF-0078D6?style=flat-square&logo=windows&logoColor=white) **WPF (Windows Presentation Foundation)** — Interface desktop nativa rica em XAML
- ![C#](https://img.shields.io/badge/C%23_MVVM-239120?style=flat-square&logo=csharp&logoColor=white) **Padrão MVVM (Model-View-ViewModel)** — Desacoplamento total de UI e lógica de apresentação
- ![XAML Vector Graphics](https://img.shields.io/badge/XAML-Vector_Graphics-0078D6?style=flat-square&logo=xaml&logoColor=white) **Desenho Vetorial / SVG em XAML** — Ícones, animações de scanner e temas
- ![Themes](https://img.shields.io/badge/ResourceDictionary-DynamicResource-blueviolet?style=flat-square) **Sistema de Temas Dinâmicos** (Light & Dark Mode)

### **Backend / API & Serviços**
- ![.NET 8](https://img.shields.io/badge/.NET_8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white) **ASP.NET Core Web API** — Serviço RESTful centralizado para operação multicaixa
- ![Swagger](https://img.shields.io/badge/Swagger-OpenAPI_v3-85EA2D?style=flat-square&logo=swagger&logoColor=black) **Swagger / OpenAPI** — Documentação e teste interativo das rotas
- ![Clean Architecture](https://img.shields.io/badge/Architecture-Clean_Architecture-lightgrey?style=flat-square) **Clean Architecture / DDD** — Camadas bem definidas de *Domain*, *Application*, *Infrastructure* e *Api*

### **Banco de Dados & Persistência**
- ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=flat-square&logo=postgresql&logoColor=white) **PostgreSQL** — Banco de dados relacional robusto com suporte a transações concorrentes
- ![EF Core](https://img.shields.io/badge/EF_Core-8.0-512BD4?style=flat-square&logo=nuget&logoColor=white) **Entity Framework Core 8 (Npgsql Provider)** — ORM e controle de Migrations

### **Testes & Qualidade**
- ![xUnit](https://img.shields.io/badge/Testes-xUnit-blue?style=flat-square) **xUnit** — Suíte de testes unitários e de integração
- ![FluentAssertions](https://img.shields.io/badge/Assertions-FluentAssertions-orange?style=flat-square) **FluentAssertions** — Asserções legíveis para testes

---

## 📐 Arquitetura da Solução

O sistema adota o modelo de **Monólito Modular Multicaixa**:

```text
┌─────────────────────────────────────────────────────────────┐
│                    CLIENTES DESKTOP (PDV)                   │
│                                                             │
│   ┌──────────────────┐               ┌──────────────────┐   │
│   │   PDV 01 (WPF)   │               │   PDV 02 (WPF)   │   │
│   └────────┬─────────┘               └────────┬─────────┘   │
└────────────┼──────────────────────────────────┼─────────────┘
             │ HTTP / REST                      │ HTTP / REST
             ▼                                  ▼
┌─────────────────────────────────────────────────────────────┐
│                   API CENTRALIZADA (.NET 8)                 │
│                                                             │
│   ┌─────────────────────────────────────────────────────┐   │
│   │                  CaixaMercado.Api                   │   │
│   └──────────────────────┬──────────────────────────────┘   │
│                          │                                  │
│   ┌──────────────────────▼──────────────────────────────┐   │
│   │              CaixaMercado.Application               │   │
│   │  (Casos de Uso, Serviços, DTOs, Validadores)         │   │
│   └──────────────────────┬──────────────────────────────┘   │
│                          │                                  │
│   ┌──────────────────────▼──────────────────────────────┐   │
│   │                CaixaMercado.Domain                  │   │
│   │  (Entidades, Objetos de Valor, Regras de Negócio)    │   │
│   └──────────────────────┬──────────────────────────────┘   │
│                          │                                  │
│   ┌──────────────────────▼──────────────────────────────┐   │
│   │             CaixaMercado.Infrastructure             │   │
│   │  (EF Core 8, Npgsql, Repositórios, Migrações)       │   │
│   └──────────────────────┬──────────────────────────────┘   │
└────────────┼─────────────┼──────────────────────────────────┘
             │             │
             ▼             ▼
┌─────────────────────────────────────────────────────────────┐
│                    BANCO DE DADOS                           │
│                                                             │
│                 ┌──────────────────────┐                    │
│                 │   PostgreSQL 16 DB   │                    │
│                 └──────────────────────┘                    │
└─────────────────────────────────────────────────────────────┘
```

### Estrutura de Projetos

```text
Mercadinho/
├── src/
│   ├── CaixaMercado.Domain/          # Entidades (Venda, ItemVenda, Produto, Caixa, Operador), interfaces e validações
│   ├── CaixaMercado.Application/     # Casos de uso, DTOs, Mapeamentos, Serviços de Aplicação
│   ├── CaixaMercado.Infrastructure/  # DbContext EF Core, Mapeamentos Fluent API, Repositórios PostgreSQL
│   ├── CaixaMercado.Api/             # Controllers RESTful, Swagger UI, Middlewares, Dependency Injection
│   └── CaixaMercado.PDV/             # Aplicação WPF (.NET 8 Windows), Views XAML, ViewModels, Estilos e Temas
└── tests/
    ├── CaixaMercado.Domain.Tests/        # Testes unitários do domínio
    ├── CaixaMercado.Application.Tests/   # Testes dos casos de uso
    ├── CaixaMercado.Infrastructure.Tests/# Testes de integração/persistência
    └── CaixaMercado.PDV.Tests/           # Testes unitários de ViewModels e conversores
```

---

## ⌨️ Tabela de Atalhos do Operador (PDV)

| Tecla | Função | Descrição |
| :---: | :--- | :--- |
| **`F2`** | **Consultar Produto** | Abre o modal de busca de produtos por código EAN, código interno ou nome |
| **`F3`** | **Identificar Cliente** | Vincula CPF/CNPJ do cliente à venda atual |
| **`F4`** | **Alterar Quantidade** | Foca no campo de quantidade para venda fracionada ou em lote |
| **`F6`** | **Aplicar Desconto** | Abre o modal para aplicação de desconto em valor R$ ou percentual % |
| **`F7`** | **Cancelar Item** | Solicita o cancelamento/estorno de um item já registrado |
| **`F8`** | **Consultar Vendas** | Exibe histórico de vendas realizadas na sessão |
| **`F9`** | **Finalizar Pagamento** | Abre o modal principal de pagamento (Dinheiro `F1`, PIX `F2`, Débito `F3`, Crédito `F4`) |
| **`ESC`**| **Cancelar Venda / Voltar** | Cancela a operação atual ou fecha o modal aberto |
| **`DEL`**| **Remover Item Selecionado** | Remove o item destacado na grade de produtos |

---

## 🚀 Como Executar o Projeto

### Pré-requisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado
- [PostgreSQL 16](https://www.postgresql.org/download/) rodando localmente ou em servidor
- Sistema Operacional: Windows 10/11 (para a aplicação PDV WPF)

### 1. Clonar o Repositório

```bash
git clone https://github.com/carloska24/Mercadinho.git
cd Mercadinho
```

### 2. Configurar a String de Conexão

Edite o arquivo `src/CaixaMercado.Api/appsettings.json` com suas credenciais do PostgreSQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=caixa_mercado_db;Username=postgres;Password=sua_senha"
  }
}
```

### 3. Aplicar as Migrações do Banco de Dados

```bash
dotnet ef database update --project src/CaixaMercado.Infrastructure --startup-project src/CaixaMercado.Api
```

### 4. Executar a API (Backend)

```bash
dotnet run --project src/CaixaMercado.Api
```

A documentação Swagger estará disponível em `http://localhost:5000/swagger` ou `https://localhost:5001/swagger`.

### 5. Executar o PDV (Aplicação Desktop)

```bash
dotnet run --project src/CaixaMercado.PDV
```

Ou execute diretamente o compilado de release:
```cmd
src\CaixaMercado.PDV\bin\Release\net8.0-windows\CaixaMercado.PDV.exe
```

---

## 🧪 Executando os Testes

Para executar toda a suíte de testes unitários e de integração:

```bash
dotnet test
```

---

## 📄 Licença

Este projeto é distribuído sob a licença **MIT**. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

<p align="center">
  Desenvolvido com 💚 para transformar a experiência de frente de caixa do pequeno e médio varejo.
</p>
