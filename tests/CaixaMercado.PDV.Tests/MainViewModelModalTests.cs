using CaixaMercado.Application.Services;
using CaixaMercado.Domain.Enums;
using CaixaMercado.PDV.ViewModels;

namespace CaixaMercado.PDV.Tests;

public class MainViewModelModalTests
{
    [Fact]
    public void PagamentoInvalido_MantemModalAberto_SemSolicitarFocoNoEan()
    {
        var viewModel = new MainViewModel(new VendaService());
        viewModel.EanInput = "7891234567890";
        viewModel.AdicionarItemCommand.Execute(null);
        viewModel.AbrirPagamentoCommand.Execute(null);
        viewModel.ValorPagoInput = 0m;

        var solicitacoesDeFocoNoEan = 0;
        var solicitacoesDeFocoNoPagamento = 0;
        viewModel.RequestFocusEan += () => solicitacoesDeFocoNoEan++;
        viewModel.RequestFocusPagamento += () => solicitacoesDeFocoNoPagamento++;

        viewModel.ConfirmarPagamentoCommand.Execute(null);

        Assert.True(viewModel.IsModalPagamentoAberto);
        Assert.True(viewModel.HasMensagemErroModal);
        Assert.Equal(0, solicitacoesDeFocoNoEan);
        Assert.Equal(1, solicitacoesDeFocoNoPagamento);
    }

    [Fact]
    public void F2DurantePagamento_NaoAbreConsultaSobreModal()
    {
        var viewModel = CriarVendaComPagamentoAberto();

        viewModel.AbrirConsultaCommand.Execute(null);

        Assert.True(viewModel.IsModalPagamentoAberto);
        Assert.False(viewModel.IsModalConsultaAberta);
        Assert.Equal(TipoPagamento.Pix, viewModel.TipoPagamentoSelecionado);
    }

    [Fact]
    public void EscapeDurantePagamento_FechaPagamentoSemSolicitarCancelamentoDaVenda()
    {
        var viewModel = CriarVendaComPagamentoAberto();

        viewModel.CancelarVendaCommand.Execute(null);

        Assert.False(viewModel.IsModalPagamentoAberto);
        Assert.False(viewModel.IsModalConfirmarCancelarAberto);
        Assert.NotEmpty(viewModel.Itens);
    }

    [Fact]
    public void AcaoPrincipalDuranteConsulta_AdicionaProdutoSelecionadoEFechaModal()
    {
        var viewModel = new MainViewModel(new VendaService());
        viewModel.AbrirConsultaCommand.Execute(null);

        viewModel.ExecutarAcaoPrincipalCommand.Execute(null);

        Assert.False(viewModel.IsModalConsultaAberta);
        Assert.Single(viewModel.Itens);
    }

    [Fact]
    public void FormaPagamento_SoPodeSerSelecionadaComPagamentoAberto()
    {
        var viewModel = new MainViewModel(new VendaService());

        Assert.False(viewModel.SelecionarFormaPagamentoCommand.CanExecute(TipoPagamento.CartaoDebito));

        viewModel.EanInput = "7891234567890";
        viewModel.AdicionarItemCommand.Execute(null);
        viewModel.AbrirPagamentoCommand.Execute(null);

        Assert.True(viewModel.SelecionarFormaPagamentoCommand.CanExecute(TipoPagamento.CartaoDebito));

        viewModel.SelecionarFormaPagamentoCommand.Execute(TipoPagamento.CartaoDebito);

        Assert.Equal(TipoPagamento.CartaoDebito, viewModel.TipoPagamentoSelecionado);
    }

    [Fact]
    public void DescontoValorFixo_EInversoDoDescontoPercentual()
    {
        var viewModel = new MainViewModel(new VendaService());

        Assert.True(viewModel.IsDescontoValorFixo);

        viewModel.IsDescontoPercentual = true;

        Assert.False(viewModel.IsDescontoValorFixo);

        viewModel.IsDescontoValorFixo = true;

        Assert.False(viewModel.IsDescontoPercentual);
    }

    [Fact]
    public void RemoverItemDuranteModal_NaoAlteraVenda()
    {
        var viewModel = CriarVendaComPagamentoAberto();
        var quantidadeAntes = viewModel.Itens.Count;

        viewModel.RemoverItemCommand.Execute(null);

        Assert.Equal(quantidadeAntes, viewModel.Itens.Count);
    }

    [Fact]
    public void EnterDuranteConfirmacaoDeCancelamento_NaoCancelaVenda()
    {
        var viewModel = new MainViewModel(new VendaService());
        viewModel.EanInput = "7891234567890";
        viewModel.AdicionarItemCommand.Execute(null);
        viewModel.CancelarVendaCommand.Execute(null);

        viewModel.ExecutarAcaoPrincipalCommand.Execute(null);

        Assert.NotEmpty(viewModel.Itens);
        Assert.False(viewModel.IsModalConfirmarCancelarAberto);
    }

    [Fact]
    public void F3ForaDoPagamento_IdentificaCliente()
    {
        var viewModel = new MainViewModel(new VendaService());

        viewModel.AtalhoF3Command.Execute(null);

        Assert.True(viewModel.IsModalClienteAberto);

        viewModel.ClienteNomeInput = "Maria da Silva";
        viewModel.ConfirmarClienteCommand.Execute(null);

        Assert.False(viewModel.IsModalClienteAberto);
        Assert.Equal("Maria da Silva", viewModel.ClienteNome);
    }

    [Fact]
    public void F3DurantePagamento_SelecionaDebitoSemAbrirCliente()
    {
        var viewModel = CriarVendaComPagamentoAberto();

        viewModel.AtalhoF3Command.Execute(null);

        Assert.Equal(TipoPagamento.CartaoDebito, viewModel.TipoPagamentoSelecionado);
        Assert.False(viewModel.IsModalClienteAberto);
    }

    [Fact]
    public void F4ForaDoPagamento_SolicitaFocoNaQuantidade()
    {
        var viewModel = new MainViewModel(new VendaService());
        var solicitacoesDeFoco = 0;
        viewModel.RequestFocusQuantidade += () => solicitacoesDeFoco++;

        viewModel.AtalhoF4Command.Execute(null);

        Assert.Equal(1, solicitacoesDeFoco);
    }

    [Fact]
    public void F4DurantePagamento_SelecionaCreditoSemAlterarFoco()
    {
        var viewModel = CriarVendaComPagamentoAberto();
        var solicitacoesDeFoco = 0;
        viewModel.RequestFocusQuantidade += () => solicitacoesDeFoco++;

        viewModel.AtalhoF4Command.Execute(null);

        Assert.Equal(TipoPagamento.CartaoCredito, viewModel.TipoPagamentoSelecionado);
        Assert.Equal(0, solicitacoesDeFoco);
    }

    [Fact]
    public void F8AbreConsultaSomenteForaDeOutroModal()
    {
        var viewModel = new MainViewModel(new VendaService());

        viewModel.AtalhoF8Command.Execute(null);

        Assert.True(viewModel.IsModalConsultaAberta);

        viewModel.FecharModalConsultaCommand.Execute(null);
        viewModel.EanInput = "7891234567890";
        viewModel.AdicionarItemCommand.Execute(null);
        viewModel.AbrirPagamentoCommand.Execute(null);
        viewModel.AtalhoF8Command.Execute(null);

        Assert.True(viewModel.IsModalPagamentoAberto);
        Assert.False(viewModel.IsModalConsultaAberta);
    }

    [Fact]
    public void RemoverUltimoItem_LimpaCartaoDeUltimoItem()
    {
        var viewModel = new MainViewModel(new VendaService());
        viewModel.EanInput = "7891234567890";
        viewModel.AdicionarItemCommand.Execute(null);

        viewModel.RemoverItemCommand.Execute(null);

        Assert.Empty(viewModel.Itens);
        Assert.Equal("Nenhum item registrado", viewModel.UltimoItemDescricao);
        Assert.Equal(0m, viewModel.UltimoItemTotal);
    }

    [Fact]
    public void F2DuranteIdentificacaoDeCliente_NaoEmpilhaConsulta()
    {
        var viewModel = new MainViewModel(new VendaService());
        viewModel.AtalhoF3Command.Execute(null);

        viewModel.AbrirConsultaCommand.Execute(null);

        Assert.True(viewModel.IsModalClienteAberto);
        Assert.False(viewModel.IsModalConsultaAberta);
        Assert.Equal(1, ContarModaisAbertos(viewModel));
    }

    [Fact]
    public void FormaPagamento_NotificaEstadoVisualExclusivo()
    {
        var viewModel = CriarVendaComPagamentoAberto();

        viewModel.SelecionarFormaPagamentoCommand.Execute(TipoPagamento.Pix);

        Assert.False(viewModel.IsPagamentoDinheiroSelecionado);
        Assert.True(viewModel.IsPagamentoPixSelecionado);
        Assert.False(viewModel.IsPagamentoDebitoSelecionado);
        Assert.False(viewModel.IsPagamentoCreditoSelecionado);

        viewModel.IsPagamentoDinheiroSelecionado = true;

        Assert.Equal(TipoPagamento.Dinheiro, viewModel.TipoPagamentoSelecionado);
        Assert.True(viewModel.IsPagamentoDinheiroSelecionado);
        Assert.False(viewModel.IsPagamentoPixSelecionado);
    }

    [Fact]
    public void Pagamento_ReabertoReiniciaSelecaoEmDinheiro()
    {
        var viewModel = CriarVendaComPagamentoAberto();
        viewModel.SelecionarFormaPagamentoCommand.Execute(TipoPagamento.Pix);

        viewModel.FecharModalPagamentoCommand.Execute(null);
        viewModel.AbrirPagamentoCommand.Execute(null);

        Assert.True(viewModel.IsPagamentoDinheiroSelecionado);
        Assert.False(viewModel.IsPagamentoPixSelecionado);
        Assert.Equal(TipoPagamento.Dinheiro, viewModel.TipoPagamentoSelecionado);
    }

    private static int ContarModaisAbertos(MainViewModel viewModel) => new[]
    {
        viewModel.IsModalPagamentoAberto,
        viewModel.IsModalConsultaAberta,
        viewModel.IsModalDescontoAberto,
        viewModel.IsModalConfirmarCancelarAberto,
        viewModel.IsModalClienteAberto
    }.Count(aberto => aberto);

    private static MainViewModel CriarVendaComPagamentoAberto()
    {
        var viewModel = new MainViewModel(new VendaService());
        viewModel.EanInput = "7891234567890";
        viewModel.AdicionarItemCommand.Execute(null);
        viewModel.AbrirPagamentoCommand.Execute(null);
        return viewModel;
    }
}
