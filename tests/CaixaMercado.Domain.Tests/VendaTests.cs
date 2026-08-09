using CaixaMercado.Domain.Entities;
using CaixaMercado.Domain.Enums;
using Xunit;

namespace CaixaMercado.Domain.Tests;

public class VendaTests
{
    [Fact]
    public void Venda_Nova_DeveIniciarEmAbertoEReferenciarItensVazios()
    {
        var venda = new Venda();

        Assert.Equal(StatusVenda.EmAberto, venda.Status);
        Assert.Empty(venda.Itens);
        Assert.Equal(0m, venda.Subtotal);
        Assert.Equal(0m, venda.Total);
    }

    [Fact]
    public void Venda_CalculoSubtotalETotal_DeveSomarItensEAbaterDescontoCorrectamente()
    {
        var venda = new Venda();
        venda.Itens.Add(new ItemVenda
        {
            Sequencial = 1,
            DescricaoProduto = "Arroz Tipo 1 5kg",
            Quantidade = 2,
            PrecoUnitario = 22.90m
        });
        venda.Itens.Add(new ItemVenda
        {
            Sequencial = 2,
            DescricaoProduto = "Feijão Carioca 1kg",
            Quantidade = 3,
            PrecoUnitario = 7.50m
        });

        // 2 * 22.90 = 45.80
        // 3 * 7.50 = 22.50
        // Subtotal = 68.30
        Assert.Equal(68.30m, venda.Subtotal);
        Assert.Equal(68.30m, venda.Total);

        venda.Desconto = 5.00m;
        Assert.Equal(63.30m, venda.Total);
    }
}
