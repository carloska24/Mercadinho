using System.Windows;
using System.Windows.Input;
using CaixaMercado.PDV.ViewModels;

namespace CaixaMercado.PDV;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        FocarCampoEan();
    }

    private void FocarCampoEan()
    {
        TxtEanInput.Focus();
        Keyboard.Focus(TxtEanInput);
    }
}