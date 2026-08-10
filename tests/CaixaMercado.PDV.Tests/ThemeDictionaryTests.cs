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
        "AccentGreenBrush",
        "AccentGreenHoverBrush",
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
        "DataGridHeaderBackgroundBrush",
        "DataGridHeaderForegroundBrush",
        "DataGridRowBackgroundBrush",
        "DataGridRowAlternateBackgroundBrush",
        "DataGridRowSelectedBackgroundBrush",
        "DataGridRowSelectedForegroundBrush",
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
