using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaixaMercado.Infrastructure.Persistence.Configurations;

internal sealed class RegistroIdempotenciaConfiguration : IEntityTypeConfiguration<RegistroIdempotenciaEntity>
{
    public void Configure(EntityTypeBuilder<RegistroIdempotenciaEntity> builder)
    {
        builder.ToTable("requisicoes_idempotentes", table =>
        {
            table.HasCheckConstraint("ck_requisicoes_idempotentes_hash", "char_length(hash_requisicao) = 64");
            table.HasCheckConstraint("ck_requisicoes_idempotentes_codigo", "codigo_resultado BETWEEN 0 AND 8");
        });

        builder.HasKey(registro => registro.Id)
            .HasName("pk_requisicoes_idempotentes");

        builder.Property(registro => registro.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(registro => registro.Operacao)
            .HasColumnName("operacao")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(registro => registro.TerminalId)
            .HasColumnName("terminal_id")
            .IsRequired();

        builder.Property(registro => registro.Chave)
            .HasColumnName("chave")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(registro => registro.HashRequisicao)
            .HasColumnName("hash_requisicao")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(registro => registro.CodigoResultado)
            .HasColumnName("codigo_resultado")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(registro => registro.Mensagem)
            .HasColumnName("mensagem")
            .HasMaxLength(500);

        builder.Property(registro => registro.RecursoJson)
            .HasColumnName("recurso_json")
            .HasColumnType("jsonb");

        builder.Property(registro => registro.CriadoEmUtc)
            .HasColumnName("criado_em_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(registro => new { registro.TerminalId, registro.Chave })
            .IsUnique()
            .HasDatabaseName("ux_requisicoes_idempotentes_terminal_chave");

        builder.HasIndex(registro => registro.CriadoEmUtc)
            .HasDatabaseName("ix_requisicoes_idempotentes_criado_em");
    }
}
