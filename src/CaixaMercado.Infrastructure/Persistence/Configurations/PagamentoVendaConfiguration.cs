using CaixaMercado.Domain.Model.Vendas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaixaMercado.Infrastructure.Persistence.Configurations;

internal sealed class PagamentoVendaConfiguration : IEntityTypeConfiguration<PagamentoVenda>
{
    public void Configure(EntityTypeBuilder<PagamentoVenda> builder)
    {
        builder.ToTable("venda_pagamentos", table =>
        {
            table.HasCheckConstraint("ck_venda_pagamentos_forma", "forma BETWEEN 1 AND 5");
            table.HasCheckConstraint("ck_venda_pagamentos_status", "status BETWEEN 1 AND 5");
            table.HasCheckConstraint("ck_venda_pagamentos_valor", "valor_aplicado > 0");
            table.HasCheckConstraint("ck_venda_pagamentos_dinheiro", "(forma = 1 AND valor_recebido_dinheiro IS NOT NULL AND valor_recebido_dinheiro >= valor_aplicado) OR (forma <> 1 AND valor_recebido_dinheiro IS NULL)");
            table.HasCheckConstraint("ck_venda_pagamentos_aprovacao_eletronica", "forma = 1 OR status <> 2 OR referencia_externa IS NOT NULL");
        });

        builder.HasKey(pagamento => pagamento.Id).HasName("pk_venda_pagamentos");
        builder.Property(pagamento => pagamento.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(pagamento => pagamento.VendaId).HasColumnName("venda_id").IsRequired();
        builder.Property(pagamento => pagamento.SessaoCaixaId).HasColumnName("sessao_caixa_id").IsRequired();
        builder.Property(pagamento => pagamento.Forma).HasColumnName("forma").HasConversion<short>().IsRequired();
        builder.Property(pagamento => pagamento.ValorAplicado).HasColumnName("valor_aplicado").HasPrecision(18, 2).IsRequired();
        builder.Property(pagamento => pagamento.Status).HasColumnName("status").HasConversion<short>().IsRequired();
        builder.Property(pagamento => pagamento.ValorRecebidoDinheiro).HasColumnName("valor_recebido_dinheiro").HasPrecision(18, 2);
        builder.Property(pagamento => pagamento.RegistradoEmUtc).HasColumnName("registrado_em_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(pagamento => pagamento.ReferenciaExterna).HasColumnName("referencia_externa").HasMaxLength(100);
        builder.Ignore(pagamento => pagamento.Troco);

        builder.HasOne<Venda>().WithMany().HasForeignKey(pagamento => pagamento.VendaId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_venda_pagamentos_vendas_venda_id");
        builder.HasOne<CaixaMercado.Domain.Model.Caixas.SessaoCaixa>().WithMany()
            .HasForeignKey(pagamento => pagamento.SessaoCaixaId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_venda_pagamentos_sessoes_caixa_sessao_id");

        builder.HasIndex(pagamento => pagamento.VendaId).HasDatabaseName("ix_venda_pagamentos_venda");
        builder.HasIndex(pagamento => pagamento.SessaoCaixaId).HasDatabaseName("ix_venda_pagamentos_sessao");
        builder.HasIndex(pagamento => pagamento.ReferenciaExterna)
            .HasFilter("referencia_externa IS NOT NULL")
            .HasDatabaseName("ix_venda_pagamentos_referencia_externa");
    }
}
