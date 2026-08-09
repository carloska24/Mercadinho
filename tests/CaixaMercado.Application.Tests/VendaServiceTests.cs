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

    [Fact]
    public void AdicionarItem_ComMesmoProdutoMultiplasVezes_DeveConsolidarQuantidade()
    {
        var service = new VendaService();
        service.AdicionarItem("7891234567891", 1); // Feijão 1kg (7.80)
        service.AdicionarItem("7891234567891", 2); // Feijão 1kg (7.80)

        var venda = service.ObterVendaAtual();
        Assert.Single(venda.Itens);
        Assert.Equal(3m, venda.Itens[0].Quantidade);
        Assert.Equal(23.40m, venda.Total);
    }

    [Fact]
    public void AplicarDescontoPercentualVenda_DeveAbaterPercentualDoSubtotal()
    {
        var service = new VendaService();
        service.AdicionarItem("7891234567890", 2); // 49.80

        var resultado = service.AplicarDescontoPercentualVenda(10m); // 10% = 4.98

        Assert.True(resultado);
        Assert.Equal(4.98m, service.ObterVendaAtual().Desconto);
        Assert.Equal(44.82m, service.ObterVendaAtual().Total);
    }

    [Fact]
    public void PesquisarProdutos_ComTermoDeFiltro_DeveRetornarProdutosCorrespondentes()
    {
        var service = new VendaService();
        var resultado = service.PesquisarProdutos("ARROZ");

        Assert.NotEmpty(resultado);
        Assert.Contains(resultado, p => p.Descricao.Contains("ARROZ"));
    }
}
