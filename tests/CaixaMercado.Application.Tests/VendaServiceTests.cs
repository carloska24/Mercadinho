using CaixaMercado.Application.Services;
using CaixaMercado.Domain.Enums;
using Xunit;

namespace CaixaMercado.Application.Tests;

public class VendaServiceTests
{
    [Fact]
    public void AdicionarItem_ComEanValido_DeveAdicionarItemAVenda()
    {
        var service = new VendaService();
        var item = service.AdicionarItem("7891234567890", 2);

        Assert.NotNull(item);
        Assert.Equal("ARROZ TIPO 1 TIO JOÃO 5KG", item.DescricaoProduto);
        Assert.Equal(2, item.Quantidade);
        Assert.Equal(24.90m, item.PrecoUnitario);
        Assert.Equal(49.80m, item.Total);

        var venda = service.ObterVendaAtual();
        Assert.Single(venda.Itens);
        Assert.Equal(49.80m, venda.Total);
    }

    [Fact]
    public void FinalizarVenda_ComDinheiroSuficiente_DeveFinalizarECalcularTroco()
    {
        var service = new VendaService();
        service.AdicionarItem("7891234567890", 1); // 24.90

        var sucesso = service.FinalizarVenda(TipoPagamento.Dinheiro, 30.00m, out decimal troco, out string erro);

        Assert.True(sucesso);
        Assert.Empty(erro);
        Assert.Equal(5.10m, troco);
        Assert.Equal(StatusVenda.Finalizada, service.ObterVendaAtual().Status);
    }
}
