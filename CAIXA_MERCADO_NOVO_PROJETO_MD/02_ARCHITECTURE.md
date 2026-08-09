# ARQUITETURA

## Estrutura
```text
CaixaMercado
├── src
│   ├── CaixaMercado.PDV
│   ├── CaixaMercado.Application
│   ├── CaixaMercado.Domain
│   ├── CaixaMercado.Infrastructure
│   └── CaixaMercado.Api
└── tests
    ├── CaixaMercado.Domain.Tests
    ├── CaixaMercado.Application.Tests
    └── CaixaMercado.Api.Tests
```

## Responsabilidades

### PDV
Interface, teclado, mouse, touch, scanner e experiência do operador.

### Domain
Entidades, value objects, regras de domínio e enums. Não conhece WPF, PostgreSQL ou ASP.NET.

### Application
Casos de uso, comandos, consultas, DTOs, validações e interfaces.

Casos de uso:
- AdicionarItemVenda
- AlterarQuantidade
- CancelarItem
- AplicarDesconto
- FinalizarVenda
- AbrirCaixa
- RegistrarSangria
- RegistrarSuprimento
- FecharCaixa

### Infrastructure
EF Core, PostgreSQL, persistência, integrações e serviços.

### API
Autenticação, endpoints, sincronização e comunicação com retaguarda quando necessário.

## MVVM
```text
View → ViewModel → Application → Domain → Infrastructure
```

Não colocar regras de negócio no XAML ou code-behind.

## Evolução
Inicialmente pode existir:
```text
PDV → Application → Infrastructure → PostgreSQL
```

Quando houver necessidade de rede:
```text
PDV → ASP.NET Core API → Application → Infrastructure → PostgreSQL
```
