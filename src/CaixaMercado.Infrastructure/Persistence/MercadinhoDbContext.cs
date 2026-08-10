using CaixaMercado.Domain.Model.Catalogo;
using CaixaMercado.Domain.Model.Vendas;
using Microsoft.EntityFrameworkCore;

namespace CaixaMercado.Infrastructure.Persistence;

public sealed class MercadinhoDbContext(DbContextOptions<MercadinhoDbContext> options) : DbContext(options)
{
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>("venda_numero_seq")
            .StartsAt(1001)
            .IncrementsBy(1);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MercadinhoDbContext).Assembly);
    }
}
