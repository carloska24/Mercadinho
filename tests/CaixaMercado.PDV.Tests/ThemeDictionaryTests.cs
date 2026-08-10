using System.Xml.Linq;

namespace CaixaMercado.PDV.Tests;

public class ThemeDictionaryTests
{
    private static readonly string[] RequiredKeys =
    {
        "WindowBackgroundBrush",
        "HeaderBackgroundBrush",
        "CardBackgroundBrush",
        "CardBorderBrush",
        "SaleAreaBackgroundBrush",
        "InputBackgroundBrush",
        "InputBorderBrush",
        "SurfaceRaisedBrush",
        "SurfaceSunkenBrush",
        "OverlayBackgroundBrush",
        "PrimaryTextBrush",
        "SecondaryTextBrush",
        "MutedTextBrush",
        "OnAccentTextBrush",
        "OnWarningTextBrush",
        "SuccessForegroundBrush",
        "DangerForegroundBrush",
        "AccentGreenBrush",
        "AccentGreenHoverBrush",
        "SuccessActionBackgroundBrush",
        "SuccessActionHoverBrush",
        "SuccessActionPressedBrush",
        "BlueActionHoverBrush",
        "BlueActionPressedBrush",
        "WarningActionHoverBrush",
        "WarningActionPressedBrush",
        "DangerActionBackgroundBrush",
        "DangerActionHoverBrush",
        "DangerActionPressedBrush",
        "AccentYellowBrush",
        "AccentBlueBrush",
        "AccentCyanBrush",
        "AccentRedBrush",
        "FocusBrush",
        "SuccessContainerBrush",
        "DangerContainerBrush",
        "ButtonNeutralBrush",
        "ButtonNeutralHoverBrush",
        "ButtonNeutralPressedBrush",
        "DisabledBackgroundBrush",
        "DisabledForegroundBrush",
        "InputSelectionBrush",
        "InputSelectionTextBrush",
        "DataGridHeaderBackgroundBrush",
        "DataGridHeaderForegroundBrush",
        "DataGridRowBackgroundBrush",
        "DataGridRowAlternateBackgroundBrush",
        "DataGridRowSelectedBackgroundBrush",
        "DataGridRowSelectedForegroundBrush",
        "DataGridRowHoverBackgroundBrush",
        "DataGridRowHoverForegroundBrush",
        "DataGridGridLinesBrush"
    };

    [Fact]
    public void TemasClaroEEscuro_PossuemAsMesmasChavesObrigatorias()
    {
        var repositoryRoot = FindRepositoryRoot();
        var darkKeys = ReadResourceKeys(Path.Combine(repositoryRoot, "src", "CaixaMercado.PDV", "Themes", "Theme.Dark.xaml"));
        var lightKeys = ReadResourceKeys(Path.Combine(repositoryRoot, "src", "CaixaMercado.PDV", "Themes", "Theme.Light.xaml"));

        Assert.Equal(darkKeys, lightKeys);
        Assert.All(RequiredKeys, key => Assert.Contains(key, darkKeys));
    }

    [Fact]
    public void XamlOperacional_NaoContemCoresFixasDeInterface()
    {
        var repositoryRoot = FindRepositoryRoot();
        var files = new[]
        {
            Path.Combine(repositoryRoot, "src", "CaixaMercado.PDV", "MainWindow.xaml"),
            Path.Combine(repositoryRoot, "src", "CaixaMercado.PDV", "Resources", "Styles.xaml")
        };

        foreach (var file in files)
        {
            Assert.DoesNotMatch("#[0-9A-Fa-f]{6,8}", File.ReadAllText(file));
        }
    }

    [Theory]
    [InlineData("Theme.Dark.xaml")]
    [InlineData("Theme.Light.xaml")]
    public void Tema_AtendeContrasteMinimoNosPrincipaisControles(string themeFile)
    {
        var repositoryRoot = FindRepositoryRoot();
        var brushes = ReadBrushColors(Path.Combine(repositoryRoot, "src", "CaixaMercado.PDV", "Themes", themeFile));

        AssertContrast(brushes, "PrimaryTextBrush", "WindowBackgroundBrush", 4.5);
        AssertContrast(brushes, "PrimaryTextBrush", "CardBackgroundBrush", 4.5);
        AssertContrast(brushes, "PrimaryTextBrush", "ButtonNeutralBrush", 4.5);
        AssertContrast(brushes, "SecondaryTextBrush", "CardBackgroundBrush", 4.5);
        AssertContrast(brushes, "DisabledForegroundBrush", "DisabledBackgroundBrush", 4.5);
        AssertContrast(brushes, "InputSelectionTextBrush", "InputSelectionBrush", 4.5);
        AssertContrast(brushes, "OnAccentTextBrush", "AccentBlueBrush", 4.5);
        AssertContrast(brushes, "OnAccentTextBrush", "SuccessActionBackgroundBrush", 4.5);
        AssertContrast(brushes, "OnWarningTextBrush", "AccentYellowBrush", 4.5);
        AssertContrast(brushes, "OnAccentTextBrush", "DangerActionBackgroundBrush", 4.5);
        AssertContrast(brushes, "DangerForegroundBrush", "CardBackgroundBrush", 4.5);
        AssertContrast(brushes, "SuccessForegroundBrush", "SuccessContainerBrush", 4.5);
        AssertContrast(brushes, "MutedTextBrush", "CardBackgroundBrush", 4.5);
        AssertContrast(brushes, "DataGridRowSelectedForegroundBrush", "DataGridRowSelectedBackgroundBrush", 4.5);
        AssertContrast(brushes, "DataGridRowHoverForegroundBrush", "DataGridRowHoverBackgroundBrush", 4.5);
        AssertContrast(brushes, "OnAccentTextBrush", "BlueActionHoverBrush", 4.5);
        AssertContrast(brushes, "OnAccentTextBrush", "BlueActionPressedBrush", 4.5);
        AssertContrast(brushes, "OnAccentTextBrush", "SuccessActionHoverBrush", 4.5);
        AssertContrast(brushes, "OnAccentTextBrush", "SuccessActionPressedBrush", 4.5);
        AssertContrast(brushes, "OnWarningTextBrush", "WarningActionHoverBrush", 4.5);
        AssertContrast(brushes, "OnWarningTextBrush", "WarningActionPressedBrush", 4.5);
        AssertContrast(brushes, "OnAccentTextBrush", "DangerActionHoverBrush", 4.5);
        AssertContrast(brushes, "OnAccentTextBrush", "DangerActionPressedBrush", 4.5);
        AssertContrast(brushes, "MutedTextBrush", "SurfaceRaisedBrush", 4.5);
    }

    [Fact]
    public void IndicadoresDeStatus_NaoUsamVerdeFixoIncompativelComTemaClaro()
    {
        var repositoryRoot = FindRepositoryRoot();
        var files = new[]
        {
            Path.Combine(repositoryRoot, "src", "CaixaMercado.PDV", "Resources", "EmptyState.xaml"),
            Path.Combine(repositoryRoot, "src", "CaixaMercado.PDV", "Resources", "VendaConcluidaState.xaml")
        };

        Assert.All(files, file => Assert.DoesNotContain("Fill=\"#34D399\"", File.ReadAllText(file), StringComparison.OrdinalIgnoreCase));
    }

    private static string[] ReadResourceKeys(string filePath)
    {
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";

        return XDocument.Load(filePath)
            .Root!
            .Elements()
            .Select(element => (string?)element.Attribute(xaml + "Key"))
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Cast<string>()
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToArray();
    }

    private static Dictionary<string, string> ReadBrushColors(string filePath)
    {
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";

        return XDocument.Load(filePath)
            .Root!
            .Elements()
            .Where(element => element.Name.LocalName == "SolidColorBrush")
            .ToDictionary(
                element => (string)element.Attribute(xaml + "Key")!,
                element => (string)element.Attribute("Color")!,
                StringComparer.Ordinal);
    }

    private static void AssertContrast(
        IReadOnlyDictionary<string, string> brushes,
        string foregroundKey,
        string backgroundKey,
        double minimum)
    {
        var ratio = ContrastRatio(brushes[foregroundKey], brushes[backgroundKey]);
        Assert.True(
            ratio >= minimum,
            $"Contraste {foregroundKey}/{backgroundKey} = {ratio:N2}; mínimo esperado = {minimum:N1}.");
    }

    private static double ContrastRatio(string foreground, string background)
    {
        var foregroundLuminance = RelativeLuminance(foreground);
        var backgroundLuminance = RelativeLuminance(background);
        var lighter = Math.Max(foregroundLuminance, backgroundLuminance);
        var darker = Math.Min(foregroundLuminance, backgroundLuminance);
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double RelativeLuminance(string color)
    {
        var hex = color.TrimStart('#');
        if (hex.Length == 8)
        {
            hex = hex[2..];
        }

        var channels = new[]
        {
            Convert.ToInt32(hex[0..2], 16) / 255d,
            Convert.ToInt32(hex[2..4], 16) / 255d,
            Convert.ToInt32(hex[4..6], 16) / 255d
        };

        return 0.2126 * Linearize(channels[0])
            + 0.7152 * Linearize(channels[1])
            + 0.0722 * Linearize(channels[2]);
    }

    private static double Linearize(double channel)
    {
        return channel <= 0.04045
            ? channel / 12.92
            : Math.Pow((channel + 0.055) / 1.055, 2.4);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "CaixaMercado.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Raiz do repositório não encontrada.");
    }
}
