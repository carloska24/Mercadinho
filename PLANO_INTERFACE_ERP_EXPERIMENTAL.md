# Caixa Mercado — Plano da Interface ERP Experimental

## Entendimento aprovado

- Sistema enxuto para um mercadinho de bairro com pequena padaria e hortifruti.
- Usuários iniciais: dono e dois funcionários.
- Estrutura local: dois computadores de caixa e um computador de retaguarda.
- PDV orientado a teclado, leitor e futura balança, com interface corporativa inspirada em padrões ERP/TOTVS.
- Temas claro e escuro alternáveis sem interromper a venda.
- Operação local-first: os caixas continuam vendendo sem internet e sincronizam posteriormente.
- Primeira fase usa periféricos e emissão fiscal simulados; integrações reais ficam para uma etapa posterior.

## Objetivos

1. Tornar o fluxo do caixa previsível, rápido e seguro.
2. Criar uma identidade visual corporativa, densa e adequada ao uso prolongado.
3. Preparar uma retaguarda simples para o dono, sem a complexidade de um ERP de grande rede.
4. Permitir evolução incremental sem reescrever domínio e regras de venda existentes.

## Fora do escopo inicial

- Múltiplas empresas ou filiais.
- E-commerce e aplicativo de clientes.
- Contabilidade completa.
- Integrações fiscais, bancárias e de hardware reais.
- Recursos corporativos de grandes redes.

## Estrutura da interface

### Operação de caixa

- Cabeçalho compacto com caixa, operador, conexão, horário e tema.
- Área principal aproximada de 65% para entrada e cupom e 35% para conferência, totais e pagamento.
- Cupom com item, código, descrição, quantidade, unidade, preço, desconto e total.
- Barra de ações contextuais compatível com os atalhos realmente ativos.
- Diálogos com bloqueio real da tela anterior, contenção de foco e restauração contextual.
- Enter confirma a ação principal e Esc fecha o diálogo atual.

### Retaguarda

- Navegação lateral recolhível.
- Módulos: Visão Geral, Produtos, Estoque, Compras, Padaria, Hortifruti, Financeiro, Relatórios e Configurações.
- Padrão de telas: lista, filtro, toolbar contextual e formulário.
- Funcionários não acessam funções administrativas; operações sensíveis exigem autorização do dono.

## Temas e design system

- Dicionários `Theme.Light.xaml` e `Theme.Dark.xaml` com as mesmas chaves.
- Tokens por função: superfícies, textos, bordas, ações, estados, seleção, tabela e foco.
- Uso de `DynamicResource` para troca em tempo de execução.
- Preferência persistida por estação.
- Contraste, foco e estado nunca dependem somente de cor.
- Nenhum emoji ou cor cromática espalhada pelos componentes operacionais.

## Dados, offline e segurança

- API .NET 8 e PostgreSQL no computador de retaguarda.
- SQLite em cada caixa para catálogo, sessão e fila offline.
- Sincronização por outbox, identificadores únicos e operações idempotentes.
- Perfis de dono e funcionário, senhas com hash forte e auditoria de ações sensíveis.
- Backup automático local, segunda cópia criptografada quando houver internet e teste de restauração.

## Fases

1. Corrigir foco, scanner, modais, atalhos, desconto e validações de pagamento.
2. Criar design system e temas claro/escuro.
3. Extrair componentes e implementar o novo cockpit do caixa.
4. Criar shell e protótipos navegáveis da retaguarda.
5. Evoluir API, persistência e sincronização local-first.

## Validação

- Preservar todos os testes existentes.
- Adicionar testes WPF para temas, comandos, foco e diálogos.
- Validar caixa livre, venda, pagamento, erro, cancelamento, conclusão e offline nos dois temas.
- Verificar 1280x720, 1366x768 e Full HD.
- Meta de inclusão local de produto: resposta percebida em até aproximadamente 200 ms.

## Riscos reconhecidos

- A janela atual concentra interface e quatro overlays em um único XAML.
- Atalhos globais podem operar a venda por trás de modais.
- Há grande quantidade de cores fixas e uso de `StaticResource`.
- A camada PDV ainda não possui testes próprios.
- Sincronização offline exige idempotência, auditoria e tratamento explícito de conflitos.

## Log de decisões

1. Adotar abordagem de cockpit modular, em vez de reforma cosmética ou reconstrução total.
2. Preservar a proporção operacional atual, reorganizando seus componentes internos.
3. Priorizar segurança de teclado, scanner e foco antes de animações.
4. Manter padaria e hortifruti sobre o mesmo catálogo e estoque.
5. Usar arquitetura local-first, sem dependência da nuvem para vender.
6. Evoluir incrementalmente, preservando domínio e regras existentes.
7. Manter a `main` como ponto estável e desenvolver na branch `codex/interface-erp-experimental`.
