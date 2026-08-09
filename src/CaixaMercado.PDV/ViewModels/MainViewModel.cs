using System.Collections.ObjectModel;
using System.Windows.Input;
using CaixaMercado.Application.Interfaces;
using CaixaMercado.Application.Services;
using CaixaMercado.Domain.Entities;
using CaixaMercado.Domain.Enums;
using CaixaMercado.PDV.Common;

namespace CaixaMercado.PDV.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IVendaService _vendaService;

    // Foco para a View
    public event Action? RequestFocusEan;

    private string _eanInput = string.Empty;
    private decimal _quantidadeInput = 1m;
    private ItemVenda? _itemSelecionado;
    private string _ultimoItemDescricao = "Nenhum item registrado";
    private decimal _ultimoItemTotal = 0m;
    private string _mensagemStatus = "CAIXA LIVRE — AGUARDANDO PRODUTO";
    private string _clienteNome = "CONSUMIDOR NÃO IDENTIFICADO";

    // Modal Pagamento (F9)
    private bool _isModalPagamentoAberto;
    private decimal _valorPagoInput;
    private decimal _trocoCalculado;
    private TipoPagamento _tipoPagamentoSelecionado = TipoPagamento.Dinheiro;
    private string _mensagemErroModal = string.Empty;

    // Modal Consulta Produtos (F2)
    private bool _isModalConsultaAberta;
    private string _filtroConsulta = string.Empty;
    private Produto? _produtoConsultaSelecionado;

    // Modal Desconto (F6)
    private bool _isModalDescontoAberto;
    private decimal _valorDescontoInput;
    private bool _isDescontoPercentual;

    // Modal Confirmar Cancelamento de Venda (ESC)
    private bool _isModalConfirmarCancelarAberto;

    public MainViewModel() : this(new VendaService())
    {
    }

    public MainViewModel(IVendaService vendaService)
    {
        _vendaService = vendaService ?? throw new ArgumentNullException(nameof(vendaService));
        Itens = new ObservableCollection<ItemVenda>();
        ProdutosConsulta = new ObservableCollection<Produto>();

        // Comandos Principais
        AdicionarItemCommand = new RelayCommand(ExecutarAdicionarItem);
        RemoverItemCommand = new RelayCommand(ExecutarRemoverItem, () => ItemSelecionado != null || Itens.Count > 0);
        AbrirPagamentoCommand = new RelayCommand(ExecutarAbrirPagamento, () => Itens.Count > 0);
        ConfirmarPagamentoCommand = new RelayCommand(ExecutarConfirmarPagamento);
        FecharModalPagamentoCommand = new RelayCommand(ExecutarFecharModalPagamento);
        CancelarVendaCommand = new RelayCommand(ExecutarSolicitarCancelarVenda);
        ConfirmarCancelarVendaCommand = new RelayCommand(ExecutarConfirmarCancelarVenda);
        FecharModalCancelarVendaCommand = new RelayCommand(() => { IsModalConfirmarCancelarAberto = false; SolicitarFocoEan(); });

        // Comandos de Consulta F2
        AbrirConsultaCommand = new RelayCommand(ExecutarAbrirConsulta);
        FecharModalConsultaCommand = new RelayCommand(() => { IsModalConsultaAberta = false; SolicitarFocoEan(); });
        FiltrarProdutosCommand = new RelayCommand(ExecutarFiltrarProdutos);
        AdicionarProdutoConsultaCommand = new RelayCommand(ExecutarAdicionarProdutoConsulta, () => ProdutoConsultaSelecionado != null);

        // Comandos de Desconto F6
        AbrirDescontoCommand = new RelayCommand(ExecutarAbrirDesconto, () => Itens.Count > 0);
        ConfirmarDescontoCommand = new RelayCommand(ExecutarConfirmarDesconto);
        FecharModalDescontoCommand = new RelayCommand(() => { IsModalDescontoAberto = false; SolicitarFocoEan(); });

        // Atalhos de Forma de Pagamento F1-F4 na Modal
        SelecionarFormaPagamentoCommand = new RelayCommand((param) =>
        {
            if (param is TipoPagamento tipo)
            {
                TipoPagamentoSelecionado = tipo;
            }
            else if (param is string tipoStr && Enum.TryParse<TipoPagamento>(tipoStr, out var parsed))
            {
                TipoPagamentoSelecionado = parsed;
            }
        });

        SolicitarFocoEanCommand = new RelayCommand(SolicitarFocoEan);

        AtualizarDadosVenda();
        ExecutarFiltrarProdutos();
    }

    public Venda VendaAtual => _vendaService.ObterVendaAtual();
    public Caixa CaixaAtual => _vendaService.ObterCaixaAtual();
    public ObservableCollection<ItemVenda> Itens { get; }
    public ObservableCollection<Produto> ProdutosConsulta { get; }

    public string EanInput
    {
        get => _eanInput;
        set => SetProperty(ref _eanInput, value);
    }

    public decimal QuantidadeInput
    {
        get => _quantidadeInput;
        set => SetProperty(ref _quantidadeInput, value);
    }

    public ItemVenda? ItemSelecionado
    {
        get => _itemSelecionado;
        set
        {
            if (SetProperty(ref _itemSelecionado, value))
            {
                (RemoverItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string UltimoItemDescricao
    {
        get => _ultimoItemDescricao;
        set => SetProperty(ref _ultimoItemDescricao, value);
    }

    public decimal UltimoItemTotal
    {
        get => _ultimoItemTotal;
        set => SetProperty(ref _ultimoItemTotal, value);
    }

    public string MensagemStatus
    {
        get => _mensagemStatus;
        set => SetProperty(ref _mensagemStatus, value);
    }

    public string ClienteNome
    {
        get => _clienteNome;
        set => SetProperty(ref _clienteNome, value);
    }

    // Modais
    public bool IsModalPagamentoAberto
    {
        get => _isModalPagamentoAberto;
        set => SetProperty(ref _isModalPagamentoAberto, value);
    }

    public decimal ValorPagoInput
    {
        get => _valorPagoInput;
        set
        {
            if (SetProperty(ref _valorPagoInput, value))
            {
                CalcularTrocoModal();
            }
        }
    }

    public decimal TrocoCalculado
    {
        get => _trocoCalculado;
        set => SetProperty(ref _trocoCalculado, value);
    }

    public TipoPagamento TipoPagamentoSelecionado
    {
        get => _tipoPagamentoSelecionado;
        set => SetProperty(ref _tipoPagamentoSelecionado, value);
    }

    public string MensagemErroModal
    {
        get => _mensagemErroModal;
        set
        {
            if (SetProperty(ref _mensagemErroModal, value))
            {
                OnPropertyChanged(nameof(HasMensagemErroModal));
            }
        }
    }

    public bool HasMensagemErroModal => !string.IsNullOrEmpty(MensagemErroModal);

    // Modal F2 Consulta
    public bool IsModalConsultaAberta
    {
        get => _isModalConsultaAberta;
        set => SetProperty(ref _isModalConsultaAberta, value);
    }

    public string FiltroConsulta
    {
        get => _filtroConsulta;
        set
        {
            if (SetProperty(ref _filtroConsulta, value))
            {
                ExecutarFiltrarProdutos();
            }
        }
    }

    public Produto? ProdutoConsultaSelecionado
    {
        get => _produtoConsultaSelecionado;
        set
        {
            if (SetProperty(ref _produtoConsultaSelecionado, value))
            {
                (AdicionarProdutoConsultaCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    // Modal F6 Desconto
    public bool IsModalDescontoAberto
    {
        get => _isModalDescontoAberto;
        set => SetProperty(ref _isModalDescontoAberto, value);
    }

    public decimal ValorDescontoInput
    {
        get => _valorDescontoInput;
        set => SetProperty(ref _valorDescontoInput, value);
    }

    public bool IsDescontoPercentual
    {
        get => _isDescontoPercentual;
        set => SetProperty(ref _isDescontoPercentual, value);
    }

    // Modal ESC Confirmar Cancelar Venda
    public bool IsModalConfirmarCancelarAberto
    {
        get => _isModalConfirmarCancelarAberto;
        set => SetProperty(ref _isModalConfirmarCancelarAberto, value);
    }

    // Comandos
    public ICommand AdicionarItemCommand { get; }
    public ICommand RemoverItemCommand { get; }
    public ICommand AbrirPagamentoCommand { get; }
    public ICommand ConfirmarPagamentoCommand { get; }
    public ICommand FecharModalPagamentoCommand { get; }
    public ICommand CancelarVendaCommand { get; }
    public ICommand ConfirmarCancelarVendaCommand { get; }
    public ICommand FecharModalCancelarVendaCommand { get; }
    public ICommand AbrirConsultaCommand { get; }
    public ICommand FecharModalConsultaCommand { get; }
    public ICommand FiltrarProdutosCommand { get; }
    public ICommand AdicionarProdutoConsultaCommand { get; }
    public ICommand AbrirDescontoCommand { get; }
    public ICommand ConfirmarDescontoCommand { get; }
    public ICommand FecharModalDescontoCommand { get; }
    public ICommand SelecionarFormaPagamentoCommand { get; }
    public ICommand SolicitarFocoEanCommand { get; }

    private void ExecutarAdicionarItem()
    {
        if (string.IsNullOrWhiteSpace(EanInput)) return;

        try
        {
            var item = _vendaService.AdicionarItem(EanInput.Trim(), QuantidadeInput);
            if (item != null)
            {
                UltimoItemDescricao = item.DescricaoProduto;
                UltimoItemTotal = item.Total;
                MensagemStatus = $"VENDA EM ANDAMENTO — {item.DescricaoProduto}";
                EanInput = string.Empty;
                QuantidadeInput = 1m;
                AtualizarDadosVenda();
            }
            else
            {
                MensagemStatus = $"PRODUTO NÃO ENCONTRADO PARA O CÓDIGO '{EanInput}'";
                EanInput = string.Empty;
            }
        }
        catch (Exception ex)
        {
            MensagemStatus = $"ERRO: {ex.Message}";
        }
        finally
        {
            SolicitarFocoEan();
        }
    }

    private void ExecutarRemoverItem()
    {
        var seqRemover = ItemSelecionado?.Sequencial ?? Itens.LastOrDefault()?.Sequencial ?? 0;
        if (seqRemover > 0)
        {
            if (_vendaService.RemoverItem(seqRemover))
            {
                MensagemStatus = $"ITEM #{seqRemover} REMOVIDO COM SUCESSO";
                AtualizarDadosVenda();
            }
        }
        SolicitarFocoEan();
    }

    private void ExecutarAbrirPagamento()
    {
        if (Itens.Count == 0)
        {
            MensagemStatus = "IMPOSSÍVEL ABRIR PAGAMENTO SEM ITENS NA VENDA";
            return;
        }

        ValorPagoInput = VendaAtual.Total;
        TrocoCalculado = 0m;
        MensagemErroModal = string.Empty;
        MensagemStatus = "AGUARDANDO PAGAMENTO — F1 DINHEIRO | F2 PIX | F3 DÉBITO | F4 CRÉDITO";
        IsModalPagamentoAberto = true;
    }

    private void ExecutarConfirmarPagamento()
    {
        if (_vendaService.FinalizarVenda(TipoPagamentoSelecionado, ValorPagoInput, out decimal troco, out string erro))
        {
            TrocoCalculado = troco;
            IsModalPagamentoAberto = false;
            MensagemStatus = $"VENDA #{VendaAtual.Numero} FINALIZADA COM SUCESSO! TROCO: R$ {troco:N2}";

            _vendaService.NovaVenda();
            UltimoItemDescricao = "Venda finalizada com sucesso!";
            UltimoItemTotal = 0m;
            AtualizarDadosVenda();
        }
        else
        {
            MensagemErroModal = erro;
        }
        SolicitarFocoEan();
    }

    private void ExecutarFecharModalPagamento()
    {
        IsModalPagamentoAberto = false;
        SolicitarFocoEan();
    }

    private void ExecutarSolicitarCancelarVenda()
    {
        if (Itens.Count == 0) return;

        IsModalConfirmarCancelarAberto = true;
    }

    private void ExecutarConfirmarCancelarVenda()
    {
        IsModalConfirmarCancelarAberto = false;
        _vendaService.CancelarVenda();
        _vendaService.NovaVenda();
        MensagemStatus = "VENDA CANCELADA — CAIXA LIVRE";
        UltimoItemDescricao = "Venda cancelada pelo operador";
        UltimoItemTotal = 0m;
        AtualizarDadosVenda();
        SolicitarFocoEan();
    }

    private void ExecutarAbrirConsulta()
    {
        FiltroConsulta = string.Empty;
        ExecutarFiltrarProdutos();
        IsModalConsultaAberta = true;
    }

    private void ExecutarFiltrarProdutos()
    {
        ProdutosConsulta.Clear();
        var lista = _vendaService.PesquisarProdutos(FiltroConsulta);
        foreach (var prod in lista)
        {
            ProdutosConsulta.Add(prod);
        }
        ProdutoConsultaSelecionado = ProdutosConsulta.FirstOrDefault();
    }

    private void ExecutarAdicionarProdutoConsulta()
    {
        if (ProdutoConsultaSelecionado == null) return;

        EanInput = ProdutoConsultaSelecionado.EAN;
        IsModalConsultaAberta = false;
        ExecutarAdicionarItem();
    }

    private void ExecutarAbrirDesconto()
    {
        if (Itens.Count == 0) return;
        ValorDescontoInput = 0m;
        IsDescontoPercentual = false;
        IsModalDescontoAberto = true;
    }

    private void ExecutarConfirmarDesconto()
    {
        if (ValorDescontoInput <= 0)
        {
            IsModalDescontoAberto = false;
            SolicitarFocoEan();
            return;
        }

        if (IsDescontoPercentual)
        {
            _vendaService.AplicarDescontoPercentualVenda(ValorDescontoInput);
        }
        else
        {
            _vendaService.AplicarDescontoVenda(ValorDescontoInput);
        }

        IsModalDescontoAberto = false;
        MensagemStatus = $"DESCONTO DE {(IsDescontoPercentual ? $"{ValorDescontoInput}%" : $"R$ {ValorDescontoInput:N2}")} APLICADO";
        AtualizarDadosVenda();
        SolicitarFocoEan();
    }

    private void SolicitarFocoEan()
    {
        RequestFocusEan?.Invoke();
    }

    private void CalcularTrocoModal()
    {
        TrocoCalculado = Math.Max(0m, ValorPagoInput - VendaAtual.Total);
    }

    private void AtualizarDadosVenda()
    {
        Itens.Clear();
        foreach (var item in VendaAtual.Itens)
        {
            Itens.Add(item);
        }

        if (Itens.Count == 0 && VendaAtual.Status == StatusVenda.EmAberto)
        {
            MensagemStatus = "CAIXA LIVRE — AGUARDANDO PRODUTO";
        }

        OnPropertyChanged(nameof(VendaAtual));
        OnPropertyChanged(nameof(CaixaAtual));
        OnPropertyChanged(nameof(HasItens));
        OnPropertyChanged(nameof(HasNoItens));

        (AbrirPagamentoCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (AbrirDescontoCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (RemoverItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    public bool HasItens => Itens.Count > 0;
    public bool HasNoItens => Itens.Count == 0;
}
