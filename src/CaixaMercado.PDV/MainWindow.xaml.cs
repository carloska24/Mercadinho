using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using CaixaMercado.PDV.Services;
using CaixaMercado.PDV.ViewModels;

namespace CaixaMercado.PDV;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;
    private ThemeService? _themeService;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        Closed += MainWindow_Closed;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            _viewModel = vm;
            vm.RequestFocusEan += FocarCampoEan;
            vm.RequestFocusQuantidade += FocarCampoQuantidade;
            vm.RequestFocusPagamento += FocarCampoPagamento;
            vm.PropertyChanged += MainViewModel_PropertyChanged;
        }

        _themeService = (System.Windows.Application.Current as App)?.ThemeService;
        if (_themeService != null)
        {
            _themeService.ThemeChanged += ThemeService_ThemeChanged;
            AtualizarBotaoTema();
        }
        FocarCampoEan();
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        if (_viewModel == null) return;

        _viewModel.RequestFocusEan -= FocarCampoEan;
        _viewModel.RequestFocusQuantidade -= FocarCampoQuantidade;
        _viewModel.RequestFocusPagamento -= FocarCampoPagamento;
        _viewModel.PropertyChanged -= MainViewModel_PropertyChanged;

        if (_themeService != null)
        {
            _themeService.ThemeChanged -= ThemeService_ThemeChanged;
        }
    }

    private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsVendaConcluidaVisivel)
            && _viewModel?.IsVendaConcluidaVisivel == true)
        {
            Dispatcher.BeginInvoke(AnunciarVendaConcluida);
        }

        if (e.PropertyName is nameof(MainViewModel.IsModalPagamentoAberto)
            or nameof(MainViewModel.IsModalConsultaAberta)
            or nameof(MainViewModel.IsModalDescontoAberto)
            or nameof(MainViewModel.IsModalConfirmarCancelarAberto)
            or nameof(MainViewModel.IsModalClienteAberto))
        {
            FocarModalAtivo();
        }
    }

    private void AnunciarVendaConcluida()
    {
        VendaConcluidaControl.ApplyTemplate();
        if (VendaConcluidaControl.Template.FindName("StatusVendaConcluida", VendaConcluidaControl) is not UIElement status) return;

        var peer = UIElementAutomationPeer.FromElement(status)
            ?? UIElementAutomationPeer.CreatePeerForElement(status);
        peer?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
    }

    private void FocarCampoEan()
    {
        if (_viewModel?.TemModalAberto == true || _viewModel?.IsVendaConcluidaVisivel == true) return;

        Dispatcher.BeginInvoke(new Action(() =>
        {
            TxtEanInput.Focus();
            Keyboard.Focus(TxtEanInput);
        }));
    }

    private void FocarModalAtivo()
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (_viewModel == null) return;

            if (_viewModel.IsModalPagamentoAberto)
            {
                FocarCampoModal(TxtValorPago, selecionarConteudo: true);
            }
            else if (_viewModel.IsModalConsultaAberta)
            {
                FocarCampoModal(TxtFiltroConsulta, selecionarConteudo: true);
            }
            else if (_viewModel.IsModalClienteAberto)
            {
                FocarCampoModal(TxtClienteNome, selecionarConteudo: true);
            }
            else if (_viewModel.IsModalDescontoAberto)
            {
                FocarCampoModal(TxtValorDesconto, selecionarConteudo: true);
            }
            else if (_viewModel.IsModalConfirmarCancelarAberto)
            {
                FocarCampoModal(BtnNaoCancelarVenda);
            }
            else
            {
                FocarCampoEan();
            }
        }));
    }

    private void FocarCampoQuantidade()
    {
        if (_viewModel?.TemModalAberto == true) return;

        Dispatcher.BeginInvoke(new Action(() =>
        {
            TxtQuantidadeInput.Focus();
            Keyboard.Focus(TxtQuantidadeInput);
            TxtQuantidadeInput.SelectAll();
        }));
    }

    private void FocarCampoPagamento()
    {
        Dispatcher.BeginInvoke(new Action(() => FocarCampoModal(TxtValorPago, selecionarConteudo: true)));
    }

    private static void FocarCampoModal(Control controle, bool selecionarConteudo = false)
    {
        controle.Focus();
        Keyboard.Focus(controle);

        if (selecionarConteudo && controle is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }

    private void AlternarTema_Click(object sender, RoutedEventArgs e)
    {
        _themeService?.Toggle();
        FocarCampoEan();
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete || _viewModel == null) return;

        // Em campos editáveis, Delete deve apagar texto — nunca remover item da venda.
        if (Keyboard.FocusedElement is TextBox) return;
        if (_viewModel.TemModalAberto || _viewModel.IsVendaConcluidaVisivel) return;

        if (_viewModel.RemoverItemCommand.CanExecute(null))
        {
            _viewModel.RemoverItemCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void MainWindow_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (_viewModel == null || _viewModel.TemModalAberto || _viewModel.IsVendaConcluidaVisivel) return;
        if (Keyboard.FocusedElement is TextBox) return;
        if (string.IsNullOrEmpty(e.Text) || e.Text.Any(character => !char.IsDigit(character))) return;

        // Recupera leituras do scanner mesmo após seleção de linha ou foco em um botão.
        TxtEanInput.Focus();
        Keyboard.Focus(TxtEanInput);
        TxtEanInput.CaretIndex = TxtEanInput.Text.Length;
        TxtEanInput.SelectedText = e.Text;
        TxtEanInput.CaretIndex = TxtEanInput.Text.Length;
        e.Handled = true;
    }

    private void GridProdutosConsulta_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || _viewModel == null) return;

        // O DataGrid consome ENTER quando uma célula está focada. Interceptamos antes
        // para cumprir a ação anunciada no modal: adicionar o produto selecionado.
        if (_viewModel.AdicionarProdutoConsultaCommand.CanExecute(null))
        {
            _viewModel.AdicionarProdutoConsultaCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void ThemeService_ThemeChanged(object? sender, EventArgs e)
    {
        AtualizarBotaoTema();
    }

    private void AtualizarBotaoTema()
    {
        if (_themeService == null) return;

        BtnAlternarTema.Content = _themeService.CurrentTheme == AppTheme.Dark
            ? "TEMA: ESCURO"
            : "TEMA: CLARO";

        AutomationProperties.SetItemStatus(
            BtnAlternarTema,
            _themeService.CurrentTheme == AppTheme.Dark ? "Tema atual: Escuro" : "Tema atual: Claro");
    }
}
