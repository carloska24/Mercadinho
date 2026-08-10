using CaixaMercado.Domain.Model.Caixas;
using CaixaMercado.Domain.Model.Vendas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaixaMercado.Infrastructure.Persistence.Configurations;

internal sealed class MovimentoCaixaConfiguration : IEntityTypeConfiguration<MovimentoCaixa>
{
    public void Configure(EntityTypeBuilder<MovimentoCaixa> builder)
    {
        builder.ToTable("caixa_movimentos", table =>
        {
            table.HasCheckConstraint("ck_caixa_movimentos_forma", "forma BETWEEN 1 AND 5");
            table.HasCheckConstraint("ck_caixa_movimentos_valores", "valor_liquido > 0 AND valor_recebido >= valor_liquido AND troco = valor_recebido - valor_liquido");
        });

        builder.HasKey(movimento => movimento.Id).HasName("pk_caixa_movimentos");
        builder.Property(movimento => movimento.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(movimento => movimento.SessaoCaixaId).HasColumnName("sessao_caixa_id").IsRequired();
        builder.Property(movimento => movimento.VendaId).HasColumnName("venda_id").IsRequired();
        builder.Property(movimento => movimento.PagamentoId).HasColumnName("pagamento_id").IsRequired();
        builder.Property(movimento => movimento.Forma).HasColumnName("forma").HasConversion<short>().IsRequired();
        builder.Property(movimento => movimento.ValorLiquido).HasColumnName("valor_liquido").HasPrecision(18, 2).IsRequired();
        builder.Property(movimento => movimento.ValorRecebido).HasColumnName("valor_recebido").HasPrecision(18, 2).IsRequired();
        builder.Property(movimento => movimento.Troco).HasColumnName("troco").HasPrecision(18, 2).IsRequired();
        builder.Property(movimento => movimento.CriadoEmUtc).HasColumnName("criado_em_utc").HasColumnType("timestamp with time zone").IsRequired();

        builder.HasOne<SessaoCaixa>().WithMany().HasForeignKey(movimento => movimento.SessaoCaixaId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_caixa_movimentos_sessoes_caixa_sessao_id");
        builder.HasOne<Venda>().WithMany().HasForeignKey(movimento => movimento.VendaId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_caixa_movimentos_vendas_venda_id");
        builder.HasOne<PagamentoVenda>().WithOne().HasForeignKey<MovimentoCaixa>(movimento => movimento.PagamentoId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_caixa_movimentos_venda_pagamentos_pagamento_id");

        builder.HasIndex(movimento => movimento.PagamentoId).IsUnique()
            .HasDatabaseName("ux_caixa_movimentos_pagamento");
        builder.HasIndex(movimento => new { movimento.SessaoCaixaId, movimento.CriadoEmUtc })
            .IsDescending(false, true).HasDatabaseName("ix_caixa_movimentos_sessao_criado_em");
        builder.HasIndex(movimento => movimento.VendaId).HasDatabaseName("ix_caixa_movimentos_venda");
    }
}
