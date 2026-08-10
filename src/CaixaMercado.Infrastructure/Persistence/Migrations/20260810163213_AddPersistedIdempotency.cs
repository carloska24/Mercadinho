using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaixaMercado.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPersistedIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "requisicoes_idempotentes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    operacao = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    terminal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    hash_requisicao = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    codigo_resultado = table.Column<short>(type: "smallint", nullable: false),
                    mensagem = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    recurso_json = table.Column<string>(type: "jsonb", nullable: true),
                    criado_em_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_requisicoes_idempotentes", x => x.id);
                    table.CheckConstraint("ck_requisicoes_idempotentes_codigo", "codigo_resultado BETWEEN 0 AND 8");
                    table.CheckConstraint("ck_requisicoes_idempotentes_hash", "char_length(hash_requisicao) = 64");
                });

            migrationBuilder.CreateIndex(
                name: "ix_requisicoes_idempotentes_criado_em",
                table: "requisicoes_idempotentes",
                column: "criado_em_utc");

            migrationBuilder.CreateIndex(
                name: "ux_requisicoes_idempotentes_terminal_chave",
                table: "requisicoes_idempotentes",
                columns: new[] { "terminal_id", "chave" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "requisicoes_idempotentes");
        }
    }
}
