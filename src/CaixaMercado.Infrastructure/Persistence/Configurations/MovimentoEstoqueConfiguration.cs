using CaixaMercado.Domain.Model.Estoque;
using CaixaMercado.Domain.Model.Vendas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaixaMercado.Infrastructure.Persistence.Configurations;

internal sealed class MovimentoEstoqueConfiguration : IEntityTypeConfiguration<MovimentoEstoque>
{
    public void Configure(EntityTypeBuilder<MovimentoEstoque> builder)
    {
        builder.ToTable("estoque_movimentos", table =>
        {
            table.HasCheckConstraint("ck_estoque_movimentos_tipo", "tipo BETWEEN 1 AND 4");
            table.HasCheckConstraint("ck_estoque_movimentos_quantidade", "quantidade > 0");
        });

        builder.HasKey(movimento => movimento.Id).HasName("pk_estoque_movimentos");
        builder.Property(movimento => movimento.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(movimento => movimento.ProdutoId).HasColumnName("produto_id").IsRequired();
        builder.Property(movimento => movimento.VendaId).HasColumnName("venda_id");
        builder.Property(movimento => movimento.ItemVendaId).HasColumnName("item_venda_id");
        builder.Property(movimento => movimento.Tipo).HasColumnName("tipo").HasConversion<short>().IsRequired();
        builder.Property(movimento => movimento.Quantidade).HasColumnName("quantidade").HasPrecision(18, 3).IsRequired();
        builder.Property(movimento => movimento.CriadoEmUtc).HasColumnName("criado_em_utc").HasColumnType("timestamp with time zone").IsRequired();

        builder.HasOne<CaixaMercado.Domain.Model.Catalogo.Produto>()
            .WithMany()
            .HasForeignKey(movimento => movimento.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_estoque_movimentos_produtos_produto_id");
        builder.HasOne<Venda>()
            .WithMany()
            .HasForeignKey(movimento => movimento.VendaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_estoque_movimentos_vendas_venda_id");
        builder.HasOne<ItemVenda>()
            .WithMany()
            .HasForeignKey(movimento => movimento.ItemVendaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_estoque_movimentos_venda_itens_item_venda_id");

        builder.HasIndex(movimento => new { movimento.ItemVendaId, movimento.Tipo })
            .IsUnique()
            .HasFilter("item_venda_id IS NOT NULL")
            .HasDatabaseName("ux_estoque_movimentos_item_tipo");
        builder.HasIndex(movimento => new { movimento.ProdutoId, movimento.CriadoEmUtc })
            .IsDescending(false, true)
            .HasDatabaseName("ix_estoque_movimentos_produto_criado_em");
        builder.HasIndex(movimento => movimento.VendaId)
            .HasDatabaseName("ix_estoque_movimentos_venda");
    }
}
