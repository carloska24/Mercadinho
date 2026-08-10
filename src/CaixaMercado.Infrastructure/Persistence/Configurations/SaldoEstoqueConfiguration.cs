using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaixaMercado.Infrastructure.Persistence.Configurations;

internal sealed class SaldoEstoqueConfiguration : IEntityTypeConfiguration<SaldoEstoqueEntity>
{
    public void Configure(EntityTypeBuilder<SaldoEstoqueEntity> builder)
    {
        builder.ToTable("estoque_saldos", table =>
        {
            table.HasCheckConstraint("ck_estoque_saldos_quantidade", "quantidade >= 0");
            table.HasCheckConstraint("ck_estoque_saldos_versao", "versao >= 0");
        });

        builder.HasKey(saldo => saldo.ProdutoId)
            .HasName("pk_estoque_saldos");

        builder.Property(saldo => saldo.ProdutoId)
            .HasColumnName("produto_id")
            .ValueGeneratedNever();

        builder.Property(saldo => saldo.Quantidade)
            .HasColumnName("quantidade")
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(saldo => saldo.Versao)
            .HasColumnName("versao")
            .IsConcurrencyToken()
            .IsRequired();

        builder.HasOne<CaixaMercado.Domain.Model.Catalogo.Produto>()
            .WithOne()
            .HasForeignKey<SaldoEstoqueEntity>(saldo => saldo.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_estoque_saldos_produtos_produto_id");
    }
}
