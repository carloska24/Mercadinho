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

    private string _eanInput = string.Empty;
    private decimal _quantidadeInput = 1m;
    private ItemVenda? _itemSelecionado;
    private string _ultimoItemDescricao = "Nenhum item registrado";
    private decimal _ultimoItemTotal = 0m;
    private string _mensagemStatus = "CAIXA LIVRE — AGUARDANDO PRODUTO";
    private string _clienteNome = "CONSUMIDOR NÃO IDENTIFICADO";
    private bool _isModalPagamentoAberto;
    private decimal _valorPagoInput;
    private decimal _trocoCalculado;
    private TipoPagamento _tipoPagamentoSelecionado = TipoPagamento.Dinheiro;
    private string _mensagemErroModal = string.Empty;

    public MainViewModel() : this(new VendaService())
    {
    }

    public MainViewModel(IVendaService vendaService)
    {
        _vendaService = vendaService ?? throw new ArgumentNullException(nameof(vendaService));
        Itens = new ObservableCollection<ItemVenda>();

        // Inicializar Comandos
        AdicionarItemCommand = new RelayCommand(ExecutarAdicionarItem);
        RemoverItemCommand = new RelayCommand(ExecutarRemoverItem, () => ItemSelecionado != null || Itens.Count > 0);
        AbrirPagamentoCommand = new RelayCommand(ExecutarAbrirPagamento, () => Itens.Count > 0);
        ConfirmarPagamentoCommand = new RelayCommand(ExecutarConfirmarPagamento);
        FecharModalPagamentoCommand = new RelayCommand(ExecutarFecharModalPagamento);
        CancelarVendaCommand = new RelayCommand(ExecutarCancelarVenda);
        ConsultarProdutoCommand = new RelayCommand(ExecutarConsultarProduto);
        FocoEanCommand = new RelayCommand(ExecutarFocoEan);

        AtualizarDadosVenda();
    }

    public Venda VendaAtual => _vendaService.ObterVendaAtual();
    public Caixa CaixaAtual => _vendaService.ObterCaixaAtual();
    public ObservableCollection<ItemVenda> Itens { get; }

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

    // Comandos
    public ICommand AdicionarItemCommand { get; }
    public ICommand RemoverItemCommand { get; }
    public ICommand AbrirPagamentoCommand { get; }
    public ICommand ConfirmarPagamentoCommand { get; }
    public ICommand FecharModalPagamentoCommand { get; }
    public ICommand CancelarVendaCommand { get; }
    public ICommand ConsultarProdutoCommand { get; }
    public ICommand FocoEanCommand { get; }

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
                MensagemStatus = $"ITEM ADICIONADO: {item.DescricaoProduto}";
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
        IsModalPagamentoAberto = true;
    }

    private void ExecutarConfirmarPagamento()
    {
        if (_vendaService.FinalizarVenda(TipoPagamentoSelecionado, ValorPagoInput, out decimal troco, out string erro))
        {
            TrocoCalculado = troco;
            IsModalPagamentoAberto = false;
            MensagemStatus = $"VENDA #{VendaAtual.Numero} FINALIZADA! TROCO: R$ {troco:N2}";
            
            // Iniciar nova venda
            _vendaService.NovaVenda();
            UltimoItemDescricao = "Venda finalizada com sucesso!";
            UltimoItemTotal = 0m;
            AtualizarDadosVenda();
        }
        else
        {
            MensagemErroModal = erro;
        }
    }

    private void ExecutarFecharModalPagamento()
    {
        IsModalPagamentoAberto = false;
    }

    private void ExecutarCancelarVenda()
    {
        if (Itens.Count == 0) return;

        _vendaService.CancelarVenda();
        _vendaService.NovaVenda();
        MensagemStatus = "VENDA CANCELADA — NOVO ATENDIMENTO INICIADO";
        UltimoItemDescricao = "Venda cancelada pelo operador";
        UltimoItemTotal = 0m;
        AtualizarDadosVenda();
    }

    private void ExecutarConsultarProduto()
    {
        MensagemStatus = "CONSULTA DE PRODUTOS — DIGITE O CÓDIGO/EAN NO CAMPO PRINCIPAL";
    }

    private void ExecutarFocoEan()
    {
        // Sinalizador para View se necessário
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

        OnPropertyChanged(nameof(VendaAtual));
        OnPropertyChanged(nameof(CaixaAtual));

        (AbrirPagamentoCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (RemoverItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }
}
