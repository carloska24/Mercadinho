# REGRAS PARA O ANTIGRAVITY

## 1. Não inventar
Não inventar funcionalidades, integrações, APIs externas, credenciais, dados fiscais ou hardware.

## 2. Não criar SaaS
O núcleo é um aplicativo desktop de PDV.

## 3. Stack
```text
C# + .NET 10 + WPF + XAML + MVVM
ASP.NET Core quando necessário
PostgreSQL + EF Core
```

## 4. MVVM
Usar Commands, Data Binding, ResourceDictionary, Styles e Templates. Evitar regra de negócio em code-behind.

## 5. Primeiro analisar
Leia todos os `.md`, confira SDK e ferramentas e apresente plano antes de grandes alterações.

## 6. Não fazer Big Bang
Implementar em fases.

## 7. Compilar frequentemente
```text
dotnet restore
dotnet build
dotnet test
```

## 8. Testar como operador
```text
Abrir caixa → ler produto → adicionar → quantidade → pagamento → finalizar
```

## 9. Visual
Parecer software desktop empresarial de varejo. Não copiar marcas.

## 10. Teclado
F2, F3, F4, F6, F7, F8, F9, ESC, ENTER e DELETE.

## 11. Scanner
EAN → buscar → adicionar → atualizar → foco retorna.

## 12. Dinheiro
Usar `decimal`.

## 13. Domínio
Não conhece WPF, XAML, PostgreSQL ou ASP.NET.

## 14. Hardware
Criar abstrações antes de integrações reais.

## 15. Fiscal
Não simular emissão fiscal real.

## 16. Documentação
Decisões arquiteturais importantes devem ser registradas.

## 17. Critério final
Perguntar:
> Isso parece um software profissional de frente de caixa Windows?

Se não, revisar.
