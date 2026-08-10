using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaixaMercado.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAtomicCheckoutCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "estoque_movimentos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_venda_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo = table.Column<short>(type: "smallint", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    criado_em_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estoque_movimentos", x => x.id);
                    table.CheckConstraint("ck_estoque_movimentos_quantidade", "quantidade > 0");
                    table.CheckConstraint("ck_estoque_movimentos_tipo", "tipo BETWEEN 1 AND 4");
                    table.ForeignKey(
                        name: "fk_estoque_movimentos_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_estoque_movimentos_venda_itens_item_venda_id",
                        column: x => x.item_venda_id,
                        principalTable: "venda_itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_estoque_movimentos_vendas_venda_id",
                        column: x => x.venda_id,
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "estoque_saldos",
                columns: table => new
                {
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    versao = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estoque_saldos", x => x.produto_id);
                    table.CheckConstraint("ck_estoque_saldos_quantidade", "quantidade >= 0");
                    table.CheckConstraint("ck_estoque_saldos_versao", "versao >= 0");
                    table.ForeignKey(
                        name: "fk_estoque_saldos_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sessoes_caixa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    filial_id = table.Column<Guid>(type: "uuid", nullable: false),
                    terminal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operador_abertura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_abertura = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aberta_em_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    operador_fechamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor_esperado_fechamento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_contado_fechamento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    fechada_em_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    versao = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sessoes_caixa", x => x.id);
                    table.CheckConstraint("ck_sessoes_caixa_fechamento", "(status = 1 AND operador_fechamento_id IS NULL AND valor_esperado_fechamento IS NULL AND valor_contado_fechamento IS NULL AND fechada_em_utc IS NULL) OR (status = 2 AND operador_fechamento_id IS NOT NULL AND valor_esperado_fechamento IS NOT NULL AND valor_contado_fechamento IS NOT NULL AND fechada_em_utc IS NOT NULL)");
                    table.CheckConstraint("ck_sessoes_caixa_status", "status IN (1, 2)");
                    table.CheckConstraint("ck_sessoes_caixa_valor_abertura", "valor_abertura >= 0");
                    table.CheckConstraint("ck_sessoes_caixa_valor_contado", "valor_contado_fechamento IS NULL OR valor_contado_fechamento >= 0");
                    table.CheckConstraint("ck_sessoes_caixa_valor_esperado", "valor_esperado_fechamento IS NULL OR valor_esperado_fechamento >= 0");
                    table.CheckConstraint("ck_sessoes_caixa_versao", "versao >= 0");
                });

            // Não presume estoque para produtos preexistentes: cria saldos explícitos em zero.
            migrationBuilder.Sql(
                """
                INSERT INTO estoque_saldos (produto_id, quantidade, versao)
                SELECT id, 0, 0 FROM produtos
                ON CONFLICT (produto_id) DO NOTHING;
                """);

            migrationBuilder.CreateTable(
                name: "eventos_auditoria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    recurso_id = table.Column<Guid>(type: "uuid", nullable: false),
                    terminal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sessao_caixa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_eventos_auditoria", x => x.id);
                    table.ForeignKey(
                        name: "fk_eventos_auditoria_sessoes_caixa_sessao_id",
                        column: x => x.sessao_caixa_id,
                        principalTable: "sessoes_caixa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "venda_pagamentos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sessao_caixa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    forma = table.Column<short>(type: "smallint", nullable: false),
                    valor_aplicado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    valor_recebido_dinheiro = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    registrado_em_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    referencia_externa = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_venda_pagamentos", x => x.id);
                    table.CheckConstraint("ck_venda_pagamentos_aprovacao_eletronica", "forma = 1 OR status <> 2 OR referencia_externa IS NOT NULL");
                    table.CheckConstraint("ck_venda_pagamentos_dinheiro", "(forma = 1 AND valor_recebido_dinheiro IS NOT NULL AND valor_recebido_dinheiro >= valor_aplicado) OR (forma <> 1 AND valor_recebido_dinheiro IS NULL)");
                    table.CheckConstraint("ck_venda_pagamentos_forma", "forma BETWEEN 1 AND 5");
                    table.CheckConstraint("ck_venda_pagamentos_status", "status BETWEEN 1 AND 5");
                    table.CheckConstraint("ck_venda_pagamentos_valor", "valor_aplicado > 0");
                    table.ForeignKey(
                        name: "fk_venda_pagamentos_sessoes_caixa_sessao_id",
                        column: x => x.sessao_caixa_id,
                        principalTable: "sessoes_caixa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_venda_pagamentos_vendas_venda_id",
                        column: x => x.venda_id,
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "caixa_movimentos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sessao_caixa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pagamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    forma = table.Column<short>(type: "smallint", nullable: false),
                    valor_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_recebido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    troco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    criado_em_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_caixa_movimentos", x => x.id);
                    table.CheckConstraint("ck_caixa_movimentos_forma", "forma BETWEEN 1 AND 5");
                    table.CheckConstraint("ck_caixa_movimentos_valores", "valor_liquido > 0 AND valor_recebido >= valor_liquido AND troco = valor_recebido - valor_liquido");
                    table.ForeignKey(
                        name: "fk_caixa_movimentos_sessoes_caixa_sessao_id",
                        column: x => x.sessao_caixa_id,
                        principalTable: "sessoes_caixa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_caixa_movimentos_venda_pagamentos_pagamento_id",
                        column: x => x.pagamento_id,
                        principalTable: "venda_pagamentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_caixa_movimentos_vendas_venda_id",
                        column: x => x.venda_id,
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_caixa_movimentos_sessao_criado_em",
                table: "caixa_movimentos",
                columns: new[] { "sessao_caixa_id", "criado_em_utc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_caixa_movimentos_venda",
                table: "caixa_movimentos",
                column: "venda_id");

            migrationBuilder.CreateIndex(
                name: "ux_caixa_movimentos_pagamento",
                table: "caixa_movimentos",
                column: "pagamento_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_estoque_movimentos_produto_criado_em",
                table: "estoque_movimentos",
                columns: new[] { "produto_id", "criado_em_utc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_estoque_movimentos_venda",
                table: "estoque_movimentos",
                column: "venda_id");

            migrationBuilder.CreateIndex(
                name: "ux_estoque_movimentos_item_tipo",
                table: "estoque_movimentos",
                columns: new[] { "item_venda_id", "tipo" },
                unique: true,
                filter: "item_venda_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_eventos_auditoria_acao_recurso",
                table: "eventos_auditoria",
                columns: new[] { "acao", "recurso_id" });

            migrationBuilder.CreateIndex(
                name: "ix_eventos_auditoria_correlation_id",
                table: "eventos_auditoria",
                column: "correlation_id",
                filter: "correlation_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_eventos_auditoria_sessao_caixa_id",
                table: "eventos_auditoria",
                column: "sessao_caixa_id");

            migrationBuilder.CreateIndex(
                name: "ix_eventos_auditoria_terminal_criado_em",
                table: "eventos_auditoria",
                columns: new[] { "terminal_id", "criado_em_utc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_sessoes_caixa_filial_aberta_em",
                table: "sessoes_caixa",
                columns: new[] { "filial_id", "aberta_em_utc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ux_sessoes_caixa_terminal_aberta",
                table: "sessoes_caixa",
                column: "terminal_id",
                unique: true,
                filter: "status = 1");

            migrationBuilder.CreateIndex(
                name: "ix_venda_pagamentos_referencia_externa",
                table: "venda_pagamentos",
                column: "referencia_externa",
                filter: "referencia_externa IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_venda_pagamentos_sessao",
                table: "venda_pagamentos",
                column: "sessao_caixa_id");

            migrationBuilder.CreateIndex(
                name: "ix_venda_pagamentos_venda",
                table: "venda_pagamentos",
                column: "venda_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "caixa_movimentos");

            migrationBuilder.DropTable(
                name: "estoque_movimentos");

            migrationBuilder.DropTable(
                name: "estoque_saldos");

            migrationBuilder.DropTable(
                name: "eventos_auditoria");

            migrationBuilder.DropTable(
                name: "venda_pagamentos");

            migrationBuilder.DropTable(
                name: "sessoes_caixa");
        }
    }
}
