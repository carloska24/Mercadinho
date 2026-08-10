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

        AssertButtonHasCommand(document, presentation, "Produto");
        AssertButtonHasCommand(document, presentation, "Cliente");
        AssertButtonHasCommand(document, presentation, "Qtd");
        AssertButtonHasCommand(document, presentation, "Desconto");
        AssertButtonHasCommand(document, presentation, "Canc Item");
        AssertButtonHasCommand(document, presentation, "Consultar");
        AssertButtonHasCommand(document, presentation, "Pagamento");
        AssertButtonHasCommand(document, presentation, "Cancelar");
        AssertButtonHasCommand(document, presentation, "Remover");
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

    private static void AssertButtonHasCommand(XDocument document, XNamespace presentation, string label)
    {
        var button = document
            .Descendants(presentation + "Button")
            .FirstOrDefault(candidate => candidate
                .Descendants(presentation + "TextBlock")
                .Any(text => string.Equals((string?)text.Attribute("Text"), label, StringComparison.Ordinal)));

        Assert.NotNull(button);
        Assert.False(string.IsNullOrWhiteSpace((string?)button!.Attribute("Command")));
        Assert.False(
            string.IsNullOrWhiteSpace((string?)button.Attribute("AutomationProperties.Name")),
            $"O botão '{label}' precisa de um nome acessível para operação e testes automatizados.");
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
