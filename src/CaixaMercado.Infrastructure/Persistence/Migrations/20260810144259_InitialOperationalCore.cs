using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaixaMercado.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialOperationalCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "venda_numero_seq",
                startValue: 1001L);

            migrationBuilder.CreateTable(
                name: "produtos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_interno = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ean = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    plu = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    descricao = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    unidade_medida = table.Column<short>(type: "smallint", nullable: false),
                    preco_venda = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    produto_pesavel = table.Column<bool>(type: "boolean", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_produtos", x => x.id);
                    table.CheckConstraint("ck_produtos_pesavel_unidade", "NOT produto_pesavel OR unidade_medida = 2");
                    table.CheckConstraint("ck_produtos_preco_venda", "preco_venda >= 0");
                    table.CheckConstraint("ck_produtos_unidade_medida", "unidade_medida IN (1, 2)");
                });

            migrationBuilder.CreateTable(
                name: "vendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<long>(type: "bigint", nullable: true, defaultValueSql: "nextval('venda_numero_seq')"),
                    filial_id = table.Column<Guid>(type: "uuid", nullable: false),
                    terminal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sessao_caixa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criada_em_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    versao = table.Column<long>(type: "bigint", nullable: false),
                    desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vendas", x => x.id);
                    table.CheckConstraint("ck_vendas_desconto", "desconto >= 0");
                    table.CheckConstraint("ck_vendas_numero", "numero IS NULL OR numero > 0");
                    table.CheckConstraint("ck_vendas_status", "status BETWEEN 1 AND 8");
                    table.CheckConstraint("ck_vendas_versao", "versao >= 0");
                });

            migrationBuilder.CreateTable(
                name: "venda_itens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencial = table.Column<int>(type: "integer", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_codigo_interno = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    produto_descricao = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    produto_ean = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    produto_plu = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    produto_preco_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_unidade_medida = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_venda_itens", x => x.id);
                    table.CheckConstraint("ck_venda_itens_desconto", "desconto >= 0");
                    table.CheckConstraint("ck_venda_itens_preco_unitario", "produto_preco_unitario >= 0");
                    table.CheckConstraint("ck_venda_itens_quantidade", "quantidade > 0");
                    table.CheckConstraint("ck_venda_itens_sequencial", "sequencial > 0");
                    table.CheckConstraint("ck_venda_itens_unidade_medida", "produto_unidade_medida IN (1, 2)");
                    table.ForeignKey(
                        name: "FK_venda_itens_vendas_venda_id",
                        column: x => x.venda_id,
                        principalTable: "vendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_produtos_ativo_descricao",
                table: "produtos",
                columns: new[] { "ativo", "descricao" });

            migrationBuilder.CreateIndex(
                name: "ux_produtos_codigo_interno",
                table: "produtos",
                column: "codigo_interno",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_produtos_ean",
                table: "produtos",
                column: "ean",
                unique: true,
                filter: "ean IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_produtos_plu",
                table: "produtos",
                column: "plu",
                unique: true,
                filter: "plu IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_venda_itens_venda_sequencial",
                table: "venda_itens",
                columns: new[] { "venda_id", "sequencial" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vendas_filial_status_criada_em",
                table: "vendas",
                columns: new[] { "filial_id", "status", "criada_em_utc" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "ix_vendas_operador_criada_em",
                table: "vendas",
                columns: new[] { "operador_id", "criada_em_utc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_vendas_terminal_criada_em",
                table: "vendas",
                columns: new[] { "terminal_id", "criada_em_utc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ux_vendas_filial_numero",
                table: "vendas",
                columns: new[] { "filial_id", "numero" },
                unique: true,
                filter: "numero IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "produtos");

            migrationBuilder.DropTable(
                name: "venda_itens");

            migrationBuilder.DropTable(
                name: "vendas");

            migrationBuilder.DropSequence(
                name: "venda_numero_seq");
        }
    }
}
