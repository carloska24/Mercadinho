using CaixaMercado.Domain.Model.Auditoria;
using CaixaMercado.Domain.Model.Catalogo;
using CaixaMercado.Domain.Model.Caixas;
using CaixaMercado.Domain.Model.Estoque;
using CaixaMercado.Domain.Model.Vendas;
using Microsoft.EntityFrameworkCore;

namespace CaixaMercado.Infrastructure.Persistence;

public sealed class MercadinhoDbContext(DbContextOptions<MercadinhoDbContext> options) : DbContext(options)
{
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();
    public DbSet<SessaoCaixa> SessoesCaixa => Set<SessaoCaixa>();
    public DbSet<MovimentoEstoque> MovimentosEstoque => Set<MovimentoEstoque>();
    public DbSet<PagamentoVenda> PagamentosVenda => Set<PagamentoVenda>();
    public DbSet<MovimentoCaixa> MovimentosCaixa => Set<MovimentoCaixa>();
    public DbSet<EventoAuditoria> EventosAuditoria => Set<EventoAuditoria>();
    internal DbSet<SaldoEstoqueEntity> SaldosEstoque => Set<SaldoEstoqueEntity>();
    internal DbSet<RegistroIdempotenciaEntity> RegistrosIdempotencia => Set<RegistroIdempotenciaEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>("venda_numero_seq")
            .StartsAt(1001)
            .IncrementsBy(1);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MercadinhoDbContext).Assembly);
    }
}
