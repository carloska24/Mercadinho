using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CaixaMercado.PDV.ViewModels;

namespace CaixaMercado.PDV;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;

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
            vm.PropertyChanged += MainViewModel_PropertyChanged;
        }
        FocarCampoEan();
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        if (_viewModel == null) return;

        _viewModel.RequestFocusEan -= FocarCampoEan;
        _viewModel.PropertyChanged -= MainViewModel_PropertyChanged;
    }

    private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainViewModel.IsModalPagamentoAberto)
            or nameof(MainViewModel.IsModalConsultaAberta)
            or nameof(MainViewModel.IsModalDescontoAberto)
            or nameof(MainViewModel.IsModalConfirmarCancelarAberto))
        {
            FocarModalAtivo();
        }
    }

    private void FocarCampoEan()
    {
        if (_viewModel?.TemModalAberto == true) return;

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

    private static void FocarCampoModal(Control controle, bool selecionarConteudo = false)
    {
        controle.Focus();
        Keyboard.Focus(controle);

        if (selecionarConteudo && controle is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }
}
