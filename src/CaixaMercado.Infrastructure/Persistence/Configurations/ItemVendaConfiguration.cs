using CaixaMercado.Domain.Model.Vendas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaixaMercado.Infrastructure.Persistence.Configurations;

internal sealed class ItemVendaConfiguration : IEntityTypeConfiguration<ItemVenda>
{
    public void Configure(EntityTypeBuilder<ItemVenda> builder)
    {
        builder.ToTable("venda_itens", table =>
        {
            table.HasCheckConstraint("ck_venda_itens_sequencial", "sequencial > 0");
            table.HasCheckConstraint("ck_venda_itens_quantidade", "quantidade > 0");
            table.HasCheckConstraint("ck_venda_itens_desconto", "desconto >= 0");
            table.HasCheckConstraint("ck_venda_itens_preco_unitario", "produto_preco_unitario >= 0");
            table.HasCheckConstraint("ck_venda_itens_unidade_medida", "produto_unidade_medida IN (1, 2)");
        });

        builder.HasKey(item => item.Id)
            .HasName("pk_venda_itens");

        builder.Property(item => item.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property<Guid>("venda_id")
            .HasColumnName("venda_id")
            .IsRequired();

        builder.Property(item => item.Sequencial)
            .HasColumnName("sequencial")
            .IsRequired();

        builder.Property(item => item.Quantidade)
            .HasColumnName("quantidade")
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(item => item.Desconto)
            .HasColumnName("desconto")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Ignore(item => item.ValorBruto);
        builder.Ignore(item => item.Total);

        builder.ComplexProperty(item => item.Produto, snapshot =>
        {
            snapshot.Property(produto => produto.ProdutoId)
                .HasColumnName("produto_id")
                .IsRequired();

            snapshot.Property(produto => produto.CodigoInterno)
                .HasColumnName("produto_codigo_interno")
                .HasMaxLength(32)
                .IsRequired();

            snapshot.Property(produto => produto.Ean)
                .HasColumnName("produto_ean")
                .HasMaxLength(32);

            snapshot.Property(produto => produto.Plu)
                .HasColumnName("produto_plu")
                .HasMaxLength(16);

            snapshot.Property(produto => produto.Descricao)
                .HasColumnName("produto_descricao")
                .HasMaxLength(160)
                .IsRequired();

            snapshot.Property(produto => produto.UnidadeMedida)
                .HasColumnName("produto_unidade_medida")
                .HasConversion<short>()
                .IsRequired();

            snapshot.Property(produto => produto.PrecoUnitario)
                .HasColumnName("produto_preco_unitario")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        builder.HasIndex("venda_id", nameof(ItemVenda.Sequencial))
            .IsUnique()
            .HasDatabaseName("ux_venda_itens_venda_sequencial");

    }
}
