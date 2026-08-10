using System.Windows;
using CaixaMercado.PDV.Services;

namespace CaixaMercado.PDV;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public ThemeService ThemeService { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        ThemeService = new ThemeService(Resources);
        base.OnStartup(e);
    }
}

