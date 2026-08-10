using System.Xml.Linq;

namespace CaixaMercado.PDV.Tests;

public class ShortcutWiringTests
{
    [Fact]
    public void BarraInferior_TemComandosEAtalhosParaTodasAsAcoesExibidas()
    {
        var mainWindowPath = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "CaixaMercado.PDV",
            "MainWindow.xaml");

        var document = XDocument.Load(mainWindowPath);
        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        var inputKeys = document
            .Descendants(presentation + "KeyBinding")
            .Select(element => (string?)element.Attribute("Key"))
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var key in new[] { "F2", "F3", "F4", "F6", "F7", "F8", "F9", "Escape" })
        {
            Assert.Contains(key, inputKeys);
        }

        AssertButtonHasCommand(document, presentation, "F2 Produto");
        AssertButtonHasCommand(document, presentation, "F3 Cliente");
        AssertButtonHasCommand(document, presentation, "F4 Quantidade");
        AssertButtonHasCommand(document, presentation, "F6 Desconto");
        AssertButtonHasCommand(document, presentation, "F7 Cancelar item");
        AssertButtonHasCommand(document, presentation, "F8 Consultar");
        AssertButtonHasCommand(document, presentation, "F9 Pagamento");
        AssertButtonHasCommand(document, presentation, "Esc Cancelar venda");
        AssertButtonHasCommand(document, presentation, "Delete Remover item");
    }

    [Fact]
    public void BotaoPagamentoPrincipal_ConteudoHerdaForegroundDosEstadosDoBotao()
    {
        var mainWindowPath = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "CaixaMercado.PDV",
            "MainWindow.xaml");

        var document = XDocument.Load(mainWindowPath);
        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        const string inheritedForeground =
            "{Binding Foreground, RelativeSource={RelativeSource AncestorType=Button}}";

        var paymentButton = document
            .Descendants(presentation + "Button")
            .Single(button => (string?)button.Attribute("Grid.Row") == "5"
                && ((string?)button.Attribute("Command"))?.Contains("AbrirPagamentoCommand", StringComparison.Ordinal) == true);

        var icon = paymentButton.Descendants(presentation + "Path").Single();
        var label = paymentButton.Descendants(presentation + "TextBlock").Single();

        Assert.Equal(inheritedForeground, (string?)icon.Attribute("Fill"));
        Assert.Equal(inheritedForeground, (string?)label.Attribute("Foreground"));
    }

    private static void AssertButtonHasCommand(XDocument document, XNamespace presentation, string accessibleName)
    {
        var button = document
            .Descendants(presentation + "Button")
            .FirstOrDefault(candidate => string.Equals(
                (string?)candidate.Attribute("AutomationProperties.Name"),
                accessibleName,
                StringComparison.Ordinal));

        Assert.NotNull(button);
        Assert.False(string.IsNullOrWhiteSpace((string?)button!.Attribute("Command")));
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
