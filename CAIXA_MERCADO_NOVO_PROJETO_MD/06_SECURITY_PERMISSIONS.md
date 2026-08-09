# SEGURANÇA E PERMISSÕES

## Perfis

### Operador
Venda, produto, preço e recebimento.

### Supervisor
Tudo do operador + descontos especiais, cancelamentos, autorizações, sangria e suprimento conforme configuração.

### Administrador
Configurações, usuários, permissões, produtos e relatórios.

## Arquitetura
```text
Usuario → Perfil → Permissoes
```

Não espalhar permissões pelos botões WPF.

## Auditoria
Registrar:
- DataHora;
- Usuario;
- Operador;
- PDV;
- Acao;
- Entidade;
- EntidadeId;
- dados relevantes.

## Segurança
Nunca colocar senha de banco, chave de API, credencial fiscal ou segredo TEF no código/Git.

Quando existir API, validar autorização também no servidor.
