using CaixaMercado.Domain.Model.Vendas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaixaMercado.Infrastructure.Persistence.Configurations;

internal sealed class VendaConfiguration : IEntityTypeConfiguration<Venda>
{
    public void Configure(EntityTypeBuilder<Venda> builder)
    {
        builder.ToTable("vendas", table =>
        {
            table.HasCheckConstraint("ck_vendas_numero", "numero IS NULL OR numero > 0");
            table.HasCheckConstraint("ck_vendas_status", "status BETWEEN 1 AND 8");
            table.HasCheckConstraint("ck_vendas_desconto", "desconto >= 0");
            table.HasCheckConstraint("ck_vendas_versao", "versao >= 0");
        });

        builder.HasKey(venda => venda.Id)
            .HasName("pk_vendas");

        builder.Property(venda => venda.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(venda => venda.Numero)
            .HasColumnName("numero")
            .HasDefaultValueSql("nextval('venda_numero_seq')")
            .ValueGeneratedOnAdd();

        builder.Property(venda => venda.FilialId)
            .HasColumnName("filial_id")
            .IsRequired();

        builder.Property(venda => venda.TerminalId)
            .HasColumnName("terminal_id")
            .IsRequired();

        builder.Property(venda => venda.SessaoCaixaId)
            .HasColumnName("sessao_caixa_id")
            .IsRequired();

        builder.Property(venda => venda.OperadorId)
            .HasColumnName("operador_id")
            .IsRequired();

        builder.Property(venda => venda.CriadaEmUtc)
            .HasColumnName("criada_em_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(venda => venda.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(venda => venda.Versao)
            .HasColumnName("versao")
            .IsConcurrencyToken()
            .IsRequired();

        builder.Property(venda => venda.Desconto)
            .HasColumnName("desconto")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Ignore(venda => venda.ValorBruto);
        builder.Ignore(venda => venda.Subtotal);
        builder.Ignore(venda => venda.Total);
        builder.Ignore(venda => venda.QuantidadeTotal);

        builder.HasMany(venda => venda.Itens)
            .WithOne()
            .HasForeignKey("venda_id")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Navigation(venda => venda.Itens)
            .HasField("_itens")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(venda => new { venda.FilialId, venda.Numero })
            .IsUnique()
            .HasFilter("numero IS NOT NULL")
            .HasDatabaseName("ux_vendas_filial_numero");

        builder.HasIndex(venda => new { venda.TerminalId, venda.CriadaEmUtc })
            .IsDescending(false, true)
            .HasDatabaseName("ix_vendas_terminal_criada_em");

        builder.HasIndex(venda => new { venda.FilialId, venda.Status, venda.CriadaEmUtc })
            .IsDescending(false, false, true)
            .HasDatabaseName("ix_vendas_filial_status_criada_em");

        builder.HasIndex(venda => new { venda.OperadorId, venda.CriadaEmUtc })
            .IsDescending(false, true)
            .HasDatabaseName("ix_vendas_operador_criada_em");
    }
}
