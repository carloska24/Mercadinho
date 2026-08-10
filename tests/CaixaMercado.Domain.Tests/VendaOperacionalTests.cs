using CaixaMercado.Domain.Model.Catalogo;
using CaixaMercado.Domain.Model.Vendas;

namespace CaixaMercado.Domain.Tests;

public class VendaOperacionalTests
{
    [Fact]
    public void AdicionarItem_UnidadeComQuantidadeFracionaria_DeveRejeitar()
    {
        var venda = CriarVenda();
        Assert.Throws<ArgumentException>(() => venda.AdicionarItem(CriarProduto(UnidadeMedida.Unidade), 1.5m));
    }

    [Fact]
    public void AdicionarItem_QuilogramaComTresCasas_DeveAceitar()
    {
        var venda = CriarVenda();
        var item = venda.AdicionarItem(CriarProduto(UnidadeMedida.Quilograma, preco: 9.90m), 1.235m);
        Assert.Equal(1.235m, item.Quantidade);
        Assert.Equal(12.22650m, item.Total);
    }

    [Fact]
    public void AdicionarItem_RepetidoComMesmoSnapshot_DeveConsolidar()
    {
        var venda = CriarVenda();
        var produto = CriarProduto(UnidadeMedida.Unidade);
        var primeiro = venda.AdicionarItem(produto, 1m);
        var segundo = venda.AdicionarItem(produto, 2m);
        Assert.Same(primeiro, segundo);
        Assert.Single(venda.Itens);
        Assert.Equal(3m, primeiro.Quantidade);
        Assert.Equal(2, venda.Versao);
    }

    [Fact]
    public void Item_DevePreservarSnapshotDoProduto()
    {
        var venda = CriarVenda();
        var item = venda.AdicionarItem(CriarProduto(UnidadeMedida.Unidade, "Pão francês", 1.25m), 2m);
        Assert.Equal("Pão francês", item.Produto.Descricao);
        Assert.Equal(1.25m, item.Produto.PrecoUnitario);
        Assert.Equal(2.50m, item.Total);
    }

    [Fact]
    public void AplicarDescontoAcimaDoSubtotal_DeveRejeitar()
    {
        var venda = CriarVenda();
        venda.AdicionarItem(CriarProduto(UnidadeMedida.Unidade, preco: 10m), 1m);
        Assert.Throws<ArgumentOutOfRangeException>(() => venda.AplicarDesconto(10.01m));
    }

    [Fact]
    public void RemoverItem_QuandoSubtotalCaiAbaixoDoDesconto_DeveLimitarDesconto()
    {
        var venda = CriarVenda();
        var primeiro = venda.AdicionarItem(CriarProduto(UnidadeMedida.Unidade, preco: 10m), 1m);
        venda.AdicionarItem(new Produto(Guid.NewGuid(), "COD-02", "7890000000002", "102", "Outro",
            UnidadeMedida.Unidade, 2m, false), 1m);
        venda.AplicarDesconto(11m);

        venda.RemoverItem(primeiro.Id);

        Assert.Equal(2m, venda.Subtotal);
        Assert.Equal(2m, venda.Desconto);
        Assert.Equal(0m, venda.Total);
    }

    [Fact]
    public void IniciarPagamento_VendaVazia_DeveRejeitar()
    {
        Assert.Throws<InvalidOperationException>(() => CriarVenda().IniciarPagamento());
    }

    [Fact]
    public void VendaAguardandoPagamento_NaoDeveAceitarMutacaoDeItens()
    {
        var venda = CriarVenda();
        var produto = CriarProduto(UnidadeMedida.Unidade);
        venda.AdicionarItem(produto, 1m);
        venda.IniciarPagamento();
        Assert.Throws<InvalidOperationException>(() => venda.AdicionarItem(produto, 1m));
        Assert.Throws<InvalidOperationException>(() => venda.AplicarDesconto(1m));
    }

    [Fact]
    public void Cancelar_DevePreservarItensEIncrementarVersao()
    {
        var venda = CriarVenda();
        venda.AdicionarItem(CriarProduto(UnidadeMedida.Unidade), 1m);
        var versaoAnterior = venda.Versao;
        venda.Cancelar();
        Assert.Equal(StatusVendaOperacional.Cancelada, venda.Status);
        Assert.Single(venda.Itens);
        Assert.Equal(versaoAnterior + 1, venda.Versao);
    }

    private static Venda CriarVenda() => Venda.Abrir(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        Guid.NewGuid(), Guid.NewGuid(), new DateTimeOffset(2026, 8, 10, 12, 0, 0, TimeSpan.Zero));

    private static Produto CriarProduto(UnidadeMedida unidade, string descricao = "Produto teste", decimal preco = 5m) =>
        new(Guid.NewGuid(), "COD-01", "7890000000001", "101", descricao, unidade, preco,
            produtoPesavel: unidade == UnidadeMedida.Quilograma);
}
