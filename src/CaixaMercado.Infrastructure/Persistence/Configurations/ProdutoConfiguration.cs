using CaixaMercado.Domain.Model.Catalogo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaixaMercado.Infrastructure.Persistence.Configurations;

internal sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("produtos", table =>
        {
            table.HasCheckConstraint("ck_produtos_unidade_medida", "unidade_medida IN (1, 2)");
            table.HasCheckConstraint("ck_produtos_preco_venda", "preco_venda >= 0");
            table.HasCheckConstraint(
                "ck_produtos_pesavel_unidade",
                "NOT produto_pesavel OR unidade_medida = 2");
        });

        builder.HasKey(produto => produto.Id)
            .HasName("pk_produtos");

        builder.Property(produto => produto.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(produto => produto.CodigoInterno)
            .HasColumnName("codigo_interno")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(produto => produto.Ean)
            .HasColumnName("ean")
            .HasMaxLength(32);

        builder.Property(produto => produto.Plu)
            .HasColumnName("plu")
            .HasMaxLength(16);

        builder.Property(produto => produto.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(produto => produto.UnidadeMedida)
            .HasColumnName("unidade_medida")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(produto => produto.PrecoVenda)
            .HasColumnName("preco_venda")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(produto => produto.ProdutoPesavel)
            .HasColumnName("produto_pesavel")
            .IsRequired();

        builder.Property(produto => produto.Ativo)
            .HasColumnName("ativo")
            .IsRequired();

        builder.HasIndex(produto => produto.CodigoInterno)
            .IsUnique()
            .HasDatabaseName("ux_produtos_codigo_interno");

        builder.HasIndex(produto => produto.Ean)
            .IsUnique()
            .HasFilter("ean IS NOT NULL")
            .HasDatabaseName("ux_produtos_ean");

        builder.HasIndex(produto => produto.Plu)
            .IsUnique()
            .HasFilter("plu IS NOT NULL")
            .HasDatabaseName("ux_produtos_plu");

        builder.HasIndex(produto => new { produto.Ativo, produto.Descricao })
            .HasDatabaseName("ix_produtos_ativo_descricao");
    }
}
