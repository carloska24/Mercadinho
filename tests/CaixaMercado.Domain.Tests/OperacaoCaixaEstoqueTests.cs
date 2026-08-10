using CaixaMercado.Domain.Model.Caixas;
using CaixaMercado.Domain.Model.Catalogo;
using CaixaMercado.Domain.Model.Vendas;

namespace CaixaMercado.Domain.Tests;

public sealed class OperacaoCaixaEstoqueTests
{
    private static readonly DateTimeOffset Agora = new(2026, 8, 10, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    public void SessaoCaixa_NaoPodeSerFechadaDuasVezes()
    {
        var sessao = SessaoCaixa.Abrir(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m, Agora);
        sessao.Fechar(Guid.NewGuid(), 150m, 149.50m, Agora.AddHours(8));

        Assert.Equal(-0.50m, sessao.DiferencaFechamento);
        Assert.Throws<InvalidOperationException>(() =>
            sessao.Fechar(Guid.NewGuid(), 150m, 150m, Agora.AddHours(9)));
    }

    [Fact]
    public void SessaoCaixa_RegistrarVendaIncrementaVersaoESomenteAceitaAberta()
    {
        var sessao = SessaoCaixa.Abrir(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m, Agora);
        sessao.RegistrarVenda();
        Assert.Equal(1, sessao.Versao);
        sessao.Fechar(Guid.NewGuid(), 100m, 100m, Agora.AddHours(8));
        Assert.Throws<InvalidOperationException>(sessao.RegistrarVenda);
    }

    [Fact]
    public void Venda_FinalizadaUmaVez_RejeitaNovaFinalizacao()
    {
        var venda = NovaVenda(9.90m);
        var pagamento = Dinheiro(venda, 9.90m, 20m);

        venda.Finalizar(new[] { pagamento });

        Assert.Equal(StatusVendaOperacional.Finalizada, venda.Status);
        Assert.Throws<InvalidOperationException>(() => venda.Finalizar(new[] { pagamento }));
    }

    [Fact]
    public void PagamentoDinheiro_RecebidoVinteParaTotalNoveENoventa_CalculaTroco()
    {
        var venda = NovaVenda(9.90m);
        var pagamento = Dinheiro(venda, 9.90m, 20m);

        Assert.Equal(10.10m, pagamento.Troco);
    }

    [Fact]
    public void PagamentoDinheiro_RecebidoInsuficiente_ERejeitado()
    {
        var venda = NovaVenda(9.90m);
        Assert.Throws<ArgumentOutOfRangeException>(() => Dinheiro(venda, 9.90m, 9m));
    }

    [Theory]
    [InlineData(FormaPagamentoOperacional.Pix)]
    [InlineData(FormaPagamentoOperacional.CartaoDebito)]
    [InlineData(FormaPagamentoOperacional.CartaoCredito)]
    public void PagamentoEletronico_AprovadoSemReferencia_ERejeitado(FormaPagamentoOperacional forma)
    {
        var venda = NovaVenda(9.90m);
        Assert.Throws<ArgumentException>(() => new PagamentoVenda(Guid.NewGuid(), venda.Id,
            venda.SessaoCaixaId, forma, 9.90m, StatusPagamentoOperacional.Aprovado, Agora));
    }

    private static Venda NovaVenda(decimal preco)
    {
        var venda = Venda.Abrir(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Agora);
        venda.AdicionarItem(new Produto(Guid.NewGuid(), "001", "789", null, "PRODUTO",
            UnidadeMedida.Unidade, preco, false), 1m);
        return venda;
    }

    private static PagamentoVenda Dinheiro(Venda venda, decimal aplicado, decimal recebido) =>
        new(Guid.NewGuid(), venda.Id, venda.SessaoCaixaId, FormaPagamentoOperacional.Dinheiro,
            aplicado, StatusPagamentoOperacional.Aprovado, Agora, recebido);
}
