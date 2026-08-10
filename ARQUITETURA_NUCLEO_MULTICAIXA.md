# Arquitetura do Núcleo Multicaixa

Status: aprovado para planejamento de implementação  
Branch: `codex/nucleo-dados-multicaixa`  
Data da decisão: 10/08/2026

## 1. Contexto e objetivo

O Caixa Mercado atenderá um minimercado de bairro com pequena padaria, varejão e mercearia. A operação inicial terá dois caixas Windows, dois funcionários, o proprietário e um terceiro computador para a retaguarda e os serviços locais.

O objetivo desta evolução é transformar o protótipo atual do PDV em um núcleo operacional durável para dois caixas simultâneos, preservando a interface já validada e evitando complexidade de um ERP de grande porte. O sistema deve impedir perda ou duplicação de vendas, manter estoque único, controlar sessões de caixa e permitir ao proprietário acompanhar a loja.

## 2. Escopo do primeiro lançamento

Incluído:

- dois PDVs com identidade própria;
- usuários individuais e papéis de operador e proprietário/gerente;
- produtos por unidade e por peso, com quantidades de até três casas decimais;
- vendas persistentes, descontos autorizados, cancelamentos e pagamentos divididos;
- estoque central com histórico de movimentos;
- abertura, suprimento, sangria e fechamento de caixa;
- painel simples para produtos, estoque, vendas, caixas e diagnóstico;
- auditoria das operações sensíveis;
- backup automático e procedimento de restauração;
- preparação arquitetural para integrações fiscal, Pix e TEF.

Fora do primeiro lançamento:

- microserviços, Kubernetes ou alta disponibilidade automática;
- múltiplas filiais;
- funcionamento multimestre com venda completa durante queda do servidor local;
- CRM, folha de pagamento, contabilidade e financeiro completo;
- compras e gestão avançada de fornecedores;
- integração fiscal, Pix e TEF sem prévia homologação.

## 3. Arquitetura escolhida

Será utilizado um monólito modular com três clientes acessando uma API central pela rede local:

```text
PDV 01 WPF ─┐
            ├── API ASP.NET Core ── PostgreSQL
PDV 02 WPF ─┤       no servidor       no servidor
            │
Retaguarda ─┘
```

A API e o PostgreSQL serão instalados como serviços do Windows no computador do proprietário, que deverá permanecer ligado durante o expediente, sem suspensão automática, preferencialmente com nobreak e endereço de rede reservado. Um mini-PC dedicado poderá substituir essa máquina futuramente sem alterar a arquitetura.

Os PDVs nunca acessarão o banco diretamente. Eles conhecerão apenas o endereço da API e a identidade do terminal. Essa separação centraliza regras, autenticação, auditoria, transações e atualizações.

PostgreSQL foi escolhido por sua concorrência transacional, ausência de limites de edição e caminho de crescimento. SQL Server Express permanece uma alternativa operacional, mas não faz parte desta decisão inicial.

## 4. Módulos do sistema

O monólito será organizado em módulos com responsabilidades explícitas:

1. **Identidade e autorização:** usuários, senhas, papéis, sessões e autorizações gerenciais.
2. **Terminais:** PDV 01, PDV 02, configuração, estado e versão instalada.
3. **Catálogo:** produtos, EAN, PLU, unidade, peso, preços, histórico de preços e situação.
4. **Vendas:** carrinho persistente, itens, descontos, cliente, totais e estados.
5. **Pagamentos:** tentativas, formas, divisão, aprovação, falha, estado desconhecido e reversão.
6. **Estoque:** razão de movimentos, saldo, estoque mínimo, venda, cancelamento e ajuste.
7. **Caixa:** sessão, abertura, suprimento, sangria, recebimentos e fechamento.
8. **Auditoria:** ator, terminal, data, ação, motivo, valores anteriores e posteriores.
9. **Retaguarda:** cadastros, consultas, relatórios, diagnóstico e backup.
10. **Integrações:** outbox e contratos para fiscal, Pix, TEF e notificações futuras.

## 5. Modelo de dados essencial

Entidades mínimas:

- `Usuario`, `Papel` e `Permissao`;
- `TerminalPdv`;
- `Produto`, `CodigoProduto` e `HistoricoPreco`;
- `Venda` e `VendaItem`;
- `TentativaPagamento` e `Pagamento`;
- `MovimentoEstoque`;
- `SessaoCaixa` e `MovimentoCaixa`;
- `EventoAuditoria`;
- `OutboxMessage` e registro de idempotência.

As entidades usarão UUID como identidade técnica. Datas serão armazenadas em UTC e valores monetários em `decimal`/`numeric` com precisão definida. Quantidades pesáveis aceitarão três casas decimais.

Cada item vendido guardará uma fotografia da descrição, unidade, preço e demais dados relevantes no momento da venda. Alterações posteriores no cadastro não modificarão o histórico.

Estoque será representado por movimentos append-only. O saldo poderá ser materializado para desempenho, mas sempre deverá ser reconciliável pela razão de movimentos. Ajustes exigirão motivo, usuário e autorização apropriada.

## 6. Ciclo da venda e consistência

Estados previstos:

```text
Aberta → AguardandoPagamento → Paga → FiscalPendente → Finalizada
   └──────────────→ Cancelada / Falhou / RequerRevisao
```

O carrinho será criado e persistido no servidor. Inclusões, remoções e descontos serão recalculados pela API; o PDV nunca será a fonte final dos totais.

Na finalização, uma única transação deverá:

1. validar que a venda continua aberta e atualizada;
2. validar sessão, usuário e permissões;
3. registrar o pagamento aprovado;
4. registrar os movimentos de estoque;
5. registrar os movimentos de caixa;
6. atribuir número comercial único;
7. registrar auditoria;
8. confirmar o novo estado da venda.

Se qualquer parte interna falhar, nada será confirmado. Integrações externas não serão simuladas como uma transação distribuída; utilizarão máquina de estados e outbox persistente.

## 7. Concorrência e idempotência

Cada comando mutável importante terá uma chave de idempotência. A repetição da mesma chave e do mesmo conteúdo retornará o resultado anterior. A mesma chave com conteúdo diferente será rejeitada e auditada.

`VendaId` e `PaymentAttemptId` serão gerados como UUID. O número visível da venda será atribuído pelo servidor com restrição de unicidade, nunca por contador em memória no PDV.

Atualizações de venda e estoque usarão concorrência otimista ou bloqueio curto dentro da transação. Se os dois caixas tentarem vender simultaneamente o último item, somente uma operação poderá confirmar. A outra receberá um erro operacional claro e não gerará pagamento nem saldo negativo, salvo futura política expressamente configurada.

Timeout de rede não será interpretado automaticamente como sucesso ou falha. O PDV consultará o resultado usando a mesma chave antes de permitir nova tentativa.

## 8. Pagamentos

A arquitetura aceitará dinheiro, Pix, débito, crédito e pagamentos divididos. A escolha de uma forma de pagamento não equivale à aprovação.

Estados mínimos de tentativa:

- criada;
- processando;
- aprovada;
- recusada;
- resultado desconhecido;
- revertida.

Uma venda não será finalizada com meio eletrônico sem confirmação do provedor. Retornos duplicados ou fora de ordem serão tratados idempotentemente. Cancelamentos após pagamento produzirão reversão e auditoria; registros financeiros nunca serão apagados.

Dados sensíveis de cartão, PAN ou CVV não serão armazenados. Serão mantidos apenas identificadores, NSU, autorização, bandeira e status permitidos pelo provedor.

## 9. Sessão de caixa e permissões

Cada operador usará conta individual. Haverá dois papéis iniciais:

- **Operador:** vender, consultar, identificar cliente, receber e executar operações dentro de limites definidos;
- **Proprietário/Gerente:** cadastrar produtos, alterar preços, ajustar estoque, autorizar descontos e cancelamentos, executar sangrias e acessar relatórios.

Cada PDV poderá ter somente uma sessão de caixa aberta. A sessão registrará valor de abertura, vendas, recebimentos por forma, suprimentos, sangrias, cancelamentos e valor de fechamento.

O fechamento será preferencialmente cego: o operador informa os valores contados antes de ver o esperado. Divergências ficam registradas e não podem ser apagadas. Operações sensíveis solicitarão credencial gerencial e motivo, sem trocar permanentemente o operador da venda.

## 10. Painel do proprietário

A retaguarda mostrará apenas o necessário para a loja:

- faturamento e quantidade de vendas do dia;
- vendas por caixa, operador e forma de pagamento;
- descontos, cancelamentos, ticket médio e divergências;
- sessões abertas e fechamentos;
- estoque atual, estoque baixo e movimentos;
- produtos mais vendidos;
- estado dos PDVs, API, banco, filas e backups.

O proprietário poderá cadastrar produtos, preços, códigos, unidades, estoque mínimo e situação. Relatórios operacionais poderão ser exportados em CSV e PDF.

## 11. Disponibilidade e modo degradado

Queda da internet não impedirá a operação local. Integrações autorizadas a aguardar serão registradas em fila persistente, com tentativas, último erro e ação manual.

Na primeira versão, queda da API ou do servidor local preservará o carrinho exibido, mas bloqueará novas finalizações. O sistema nunca marcará Pix ou cartão como aprovado por presunção. Venda completa durante queda do servidor será estudada somente em uma fase futura, pois exigiria banco local, sincronização, reconciliação e política especial de estoque.

Metas iniciais:

- disponibilidade local de 99,5% durante o expediente;
- scanner até item visível: p95 de até 250 ms na LAN;
- persistência de operação local: p95 de até 500 ms;
- finalização interna: até 1 segundo, excluindo provedores externos;
- RTO de até 30 minutos;
- RPO entre 5 e 15 minutos, a validar com a operação.

## 12. Backup, restauração e observabilidade

Uma cópia no mesmo disco não será considerada backup. A estratégia será:

- backup completo noturno;
- cópias incrementais ou WAL conforme o RPO aprovado;
- cópia automática em mídia ou máquina distinta;
- cópia externa criptografada;
- retenção sugerida de 7 diários, 4 semanais e 12 mensais;
- verificação diária de integridade;
- restauração de teste mensal;
- exercício completo trimestral.

O painel alertará backup atrasado, pouco espaço em disco, serviço indisponível e filas paradas. Logs estruturados conterão `CorrelationId`, venda, tentativa de pagamento, PDV, usuário e versão, sem senhas ou dados sensíveis.

Haverá um procedimento impresso de recuperação do servidor: instalar versão compatível, restaurar, validar totais e última venda e liberar os terminais.

## 13. API, contratos e implantação

A API será dividida por recursos de autenticação, terminais, produtos, vendas, pagamentos, estoque, caixa, auditoria e relatórios. Operações assíncronas usarão `async/await`, cancelamento e timeouts explícitos.

Erros usarão contratos estáveis e legíveis: produto inexistente, estoque insuficiente, conflito de versão, caixa fechado, permissão negada, pagamento pendente e servidor indisponível.

O servidor iniciará API e PostgreSQL automaticamente após reinicialização. Cada PDV terá `ApiBaseUrl`, `TerminalId`, filial e timeouts, mas nenhuma senha do banco.

Migrações serão versionadas, executadas sob lock único e precedidas por backup verificado. A atualização seguirá expand-and-contract sempre que necessário, mantendo temporariamente compatibilidade entre servidor atual e cliente anterior. Alterações manuais no schema de produção serão proibidas.

## 14. Estratégia de testes

### Unidade

- totais, arredondamento, desconto e troco;
- estados de venda e pagamento;
- permissões e limites;
- regras de estoque;
- idempotência.

### Integração com PostgreSQL real

- constraints e mapeamentos;
- commit e rollback;
- migrações desde banco vazio e versão anterior;
- concorrência entre dois PDVs;
- repetição e conflito de idempotência;
- cancelamento e recomposição do estoque.

### Ponta a ponta

- dois processos disputando o último SKU;
- duplo Enter na finalização;
- queda do PDV antes e depois do commit;
- reinício do servidor;
- timeout com resultado posterior;
- fechamento dos dois caixas;
- backup e restauração;
- scanner, EAN, PLU e produtos pesáveis;
- teclado, foco, modais e atalhos;
- temas claro e escuro nas resoluções suportadas.

Integrações fiscal, Pix e TEF terão simuladores para aprovação, recusa, timeout, callback duplicado, resposta fora de ordem e reversão.

## 15. Implantação gradual

1. Construir o núcleo durável em homologação.
2. Validar banco, migrações, concorrência, backup e restauração.
3. Adaptar o PDV atual gradualmente à API, preservando a interface validada.
4. Simular dois caixas completos.
5. Pilotar um único caixa com conferência paralela por três a cinco dias.
6. Liberar o segundo caixa após ausência comprovada de perda ou duplicação.
7. Homologar e ativar integrações externas progressivamente.

## 16. Critérios de liberação

O sistema somente poderá operar como fonte oficial da loja quando estiver comprovado que:

- os PDVs possuem identidades distintas;
- vendas, pagamentos, estoque e caixa são persistentes;
- a finalização é atômica e idempotente;
- o teste concorrente do último item passa;
- nenhum pagamento eletrônico finaliza sem aprovação;
- usuários, permissões e auditoria estão ativos;
- backup externo e restauração foram testados;
- reinícios não perdem nem duplicam vendas;
- fechamento e divergência de caixa funcionam;
- fiscal e pagamentos foram homologados quando entrarem no escopo;
- existe procedimento de suporte e reversão da atualização.

Qualquer venda perdida ou duplicada, pagamento sem venda, venda sem pagamento, divergência inexplicada ou backup inválido interrompe o piloto.

## 17. Sequência inicial de implementação

1. Criar contratos e configuração de testes de integração.
2. Evoluir os agregados de domínio e suas invariantes.
3. Criar o `DbContext`, mapeamentos e primeira migração PostgreSQL.
4. Implementar catálogo, terminais e vendas persistentes.
5. Implementar estoque transacional e testes concorrentes.
6. Implementar sessões e movimentos de caixa.
7. Implementar pagamentos e idempotência.
8. Expor endpoints da API, autenticação e health checks.
9. Trocar o serviço em memória do PDV por cliente da API de forma incremental.
10. Criar a retaguarda essencial, backup, diagnóstico e implantação piloto.

## 18. Registro de decisões

| Decisão | Escolha aprovada | Motivo |
|---|---|---|
| Topologia | API e banco centrais na LAN | Consistência simples para dois caixas |
| Banco | PostgreSQL | Concorrência, licença e crescimento |
| Arquitetura | Monólito modular | Menor custo operacional sem perder separação |
| Acesso a dados | Somente pela API | Segurança, regras e versionamento centralizados |
| Queda da internet | Operação local continua | Serviços principais estão na LAN |
| Queda do servidor | Preserva carrinho e bloqueia finalização | Evita duplicidade e inconsistência |
| Offline completo | Adiado | Complexidade desproporcional ao MVP |
| Estoque | Razão de movimentos | Auditoria e reconciliação |
| Identificadores | UUID + número visível do servidor | Evita colisão entre PDVs |
| Requisições mutáveis | Idempotentes | Protege contra retry e duplo Enter |
| Interface | Preservar e adaptar gradualmente | Reduz regressão no PDV já validado |
| Implantação | Homologação, piloto em um caixa e expansão | Controle de risco operacional |

## 19. Pontos a validar antes da produção

Estas decisões não bloqueiam o início do núcleo, mas deverão ser fechadas antes do piloto real:

- destino externo do backup;
- RPO final entre 5 e 15 minutos;
- equipamento servidor definitivo e nobreak;
- política de estoque negativo;
- limites de desconto e cancelamento por papel;
- equipamentos de balança, scanner e impressão;
- provedor fiscal, Pix e TEF aplicável à loja;
- prazo de retenção fiscal e de auditoria.

