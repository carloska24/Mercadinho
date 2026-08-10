using CaixaMercado.Domain.Model.Caixas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaixaMercado.Infrastructure.Persistence.Configurations;

internal sealed class SessaoCaixaConfiguration : IEntityTypeConfiguration<SessaoCaixa>
{
    public void Configure(EntityTypeBuilder<SessaoCaixa> builder)
    {
        builder.ToTable("sessoes_caixa", table =>
        {
            table.HasCheckConstraint("ck_sessoes_caixa_status", "status IN (1, 2)");
            table.HasCheckConstraint("ck_sessoes_caixa_valor_abertura", "valor_abertura >= 0");
            table.HasCheckConstraint("ck_sessoes_caixa_valor_esperado", "valor_esperado_fechamento IS NULL OR valor_esperado_fechamento >= 0");
            table.HasCheckConstraint("ck_sessoes_caixa_valor_contado", "valor_contado_fechamento IS NULL OR valor_contado_fechamento >= 0");
            table.HasCheckConstraint("ck_sessoes_caixa_versao", "versao >= 0");
            table.HasCheckConstraint("ck_sessoes_caixa_fechamento", "(status = 1 AND operador_fechamento_id IS NULL AND valor_esperado_fechamento IS NULL AND valor_contado_fechamento IS NULL AND fechada_em_utc IS NULL) OR (status = 2 AND operador_fechamento_id IS NOT NULL AND valor_esperado_fechamento IS NOT NULL AND valor_contado_fechamento IS NOT NULL AND fechada_em_utc IS NOT NULL)");
        });

        builder.HasKey(sessao => sessao.Id).HasName("pk_sessoes_caixa");
        builder.Property(sessao => sessao.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(sessao => sessao.FilialId).HasColumnName("filial_id").IsRequired();
        builder.Property(sessao => sessao.TerminalId).HasColumnName("terminal_id").IsRequired();
        builder.Property(sessao => sessao.OperadorAberturaId).HasColumnName("operador_abertura_id").IsRequired();
        builder.Property(sessao => sessao.ValorAbertura).HasColumnName("valor_abertura").HasPrecision(18, 2).IsRequired();
        builder.Property(sessao => sessao.AbertaEmUtc).HasColumnName("aberta_em_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(sessao => sessao.Status).HasColumnName("status").HasConversion<short>().IsRequired();
        builder.Property(sessao => sessao.OperadorFechamentoId).HasColumnName("operador_fechamento_id");
        builder.Property(sessao => sessao.ValorEsperadoFechamento).HasColumnName("valor_esperado_fechamento").HasPrecision(18, 2);
        builder.Property(sessao => sessao.ValorContadoFechamento).HasColumnName("valor_contado_fechamento").HasPrecision(18, 2);
        builder.Property(sessao => sessao.FechadaEmUtc).HasColumnName("fechada_em_utc").HasColumnType("timestamp with time zone");
        builder.Property(sessao => sessao.Versao).HasColumnName("versao").IsConcurrencyToken().IsRequired();
        builder.Ignore(sessao => sessao.DiferencaFechamento);

        builder.HasIndex(sessao => sessao.TerminalId)
            .IsUnique()
            .HasFilter("status = 1")
            .HasDatabaseName("ux_sessoes_caixa_terminal_aberta");
        builder.HasIndex(sessao => new { sessao.FilialId, sessao.AbertaEmUtc })
            .IsDescending(false, true)
            .HasDatabaseName("ix_sessoes_caixa_filial_aberta_em");
    }
}
