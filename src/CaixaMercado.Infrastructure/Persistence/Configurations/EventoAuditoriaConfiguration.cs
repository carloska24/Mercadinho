using CaixaMercado.Domain.Model.Auditoria;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaixaMercado.Infrastructure.Persistence.Configurations;

internal sealed class EventoAuditoriaConfiguration : IEntityTypeConfiguration<EventoAuditoria>
{
    public void Configure(EntityTypeBuilder<EventoAuditoria> builder)
    {
        builder.ToTable("eventos_auditoria");
        builder.HasKey(evento => evento.Id).HasName("pk_eventos_auditoria");
        builder.Property(evento => evento.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(evento => evento.Acao).HasColumnName("acao").HasMaxLength(80).IsRequired();
        builder.Property(evento => evento.RecursoId).HasColumnName("recurso_id").IsRequired();
        builder.Property(evento => evento.TerminalId).HasColumnName("terminal_id").IsRequired();
        builder.Property(evento => evento.SessaoCaixaId).HasColumnName("sessao_caixa_id").IsRequired();
        builder.Property(evento => evento.OperadorId).HasColumnName("operador_id").IsRequired();
        builder.Property(evento => evento.CriadoEmUtc).HasColumnName("criado_em_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(evento => evento.CorrelationId).HasColumnName("correlation_id").HasMaxLength(100);

        builder.HasOne<CaixaMercado.Domain.Model.Caixas.SessaoCaixa>().WithMany()
            .HasForeignKey(evento => evento.SessaoCaixaId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_eventos_auditoria_sessoes_caixa_sessao_id");
        builder.HasIndex(evento => new { evento.TerminalId, evento.CriadoEmUtc })
            .IsDescending(false, true).HasDatabaseName("ix_eventos_auditoria_terminal_criado_em");
        builder.HasIndex(evento => new { evento.Acao, evento.RecursoId })
            .HasDatabaseName("ix_eventos_auditoria_acao_recurso");
        builder.HasIndex(evento => evento.CorrelationId)
            .HasFilter("correlation_id IS NOT NULL")
            .HasDatabaseName("ix_eventos_auditoria_correlation_id");
    }
}
