using System.Xml.Linq;

namespace CaixaMercado.PDV.Tests;

public class ModalInteractionStructureTests
{
    private static readonly XNamespace Presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

    [Fact]
    public void TodosControlesInterativosDosModais_UsamEstilosTematicos()
    {
        var document = LoadMainWindow();
        var overlays = document
            .Descendants(Presentation + "GroupBox")
            .Where(group => (string?)group.Attribute("Grid.RowSpan") == "4"
                && ((string?)group.Attribute("Visibility"))?.Contains("IsModal", StringComparison.Ordinal) == true)
            .ToArray();

        Assert.Equal(5, overlays.Length);
        Assert.All(overlays.SelectMany(overlay => overlay.Descendants(Presentation + "Button")),
            button => Assert.False(string.IsNullOrWhiteSpace((string?)button.Attribute("Style"))));
        Assert.All(overlays.SelectMany(overlay => overlay.Descendants(Presentation + "TextBox")),
            textBox => Assert.False(string.IsNullOrWhiteSpace((string?)textBox.Attribute("Style"))));
        Assert.All(overlays.SelectMany(overlay => overlay.Descendants(Presentation + "RadioButton")),
            radio => Assert.False(string.IsNullOrWhiteSpace((string?)radio.Attribute("Style"))));
    }

    [Fact]
    public void ModaisESucesso_ExpoemElementosReaisParaAutomacao()
    {
        var document = LoadMainWindow();
        var modalRoots = document.Descendants(Presentation + "GroupBox")
            .Where(group => ((string?)group.Attribute("Visibility"))?.Contains("IsModal", StringComparison.Ordinal) == true)
            .ToArray();

        Assert.Equal(5, modalRoots.Length);
        Assert.All(modalRoots, modal =>
        {
            Assert.False(string.IsNullOrWhiteSpace((string?)modal.Attribute(XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml"))));
            Assert.False(string.IsNullOrWhiteSpace((string?)modal.Attribute("AutomationProperties.Name")));
            Assert.Contains("ModalOverlayStyle", (string?)modal.Attribute("Style"));
        });

        var successTemplate = XDocument.Load(Path.Combine(ProjectDirectory(), "Resources", "VendaConcluidaState.xaml"));
        var successStatus = successTemplate.Descendants(Presentation + "Label")
            .Single(label => (string?)label.Attribute("AutomationProperties.AutomationId") == "StatusVendaConcluida");
        Assert.Equal("Assertive", (string?)successStatus.Attribute("AutomationProperties.LiveSetting"));
    }

    [Fact]
    public void CamposEBotoes_TemSelecaoLegivelEConteudoCentralizado()
    {
        var styles = File.ReadAllText(Path.Combine(ProjectDirectory(), "Resources", "Styles.xaml"));

        Assert.Contains("Property=\"SelectionOpacity\" Value=\"1\"", styles, StringComparison.Ordinal);
        Assert.Contains("Property=\"HorizontalContentAlignment\" Value=\"Center\"", styles, StringComparison.Ordinal);
        Assert.Contains("Property=\"VerticalContentAlignment\" Value=\"Center\"", styles, StringComparison.Ordinal);
    }

    [Fact]
    public void DataGrid_EhSomenteLeituraETematizaHoverFocoESelecao()
    {
        var styles = File.ReadAllText(Path.Combine(ProjectDirectory(), "Resources", "Styles.xaml"));

        Assert.Contains("Property=\"IsReadOnly\" Value=\"True\"", styles, StringComparison.Ordinal);
        Assert.Contains("DataGridRowSelectedBackgroundBrush", styles, StringComparison.Ordinal);
        Assert.Contains("DataGridRowSelectedForegroundBrush", styles, StringComparison.Ordinal);
        Assert.Contains("DataGridRowHoverBackgroundBrush", styles, StringComparison.Ordinal);
        Assert.Contains("IsKeyboardFocusWithin", styles, StringComparison.Ordinal);
    }

    [Fact]
    public void Quantidade_EnterRetornaAoEan_EDeleteEhTratadoPorContexto()
    {
        var document = LoadMainWindow();
        var quantity = document.Descendants(Presentation + "TextBox")
            .Single(element => (string?)element.Attribute(XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml")) == "TxtQuantidadeInput");

        var returnBinding = quantity.Descendants(Presentation + "KeyBinding")
            .SingleOrDefault(binding => string.Equals((string?)binding.Attribute("Key"), "Return", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(returnBinding);
        Assert.Contains("SolicitarFocoEanCommand", (string?)returnBinding!.Attribute("Command"));

        var window = document.Root!;
        Assert.Equal("MainWindow_PreviewKeyDown", (string?)window.Attribute("PreviewKeyDown"));
        Assert.Equal("MainWindow_PreviewTextInput", (string?)window.Attribute("PreviewTextInput"));
        Assert.DoesNotContain(document.Descendants(Presentation + "KeyBinding"),
            binding => string.Equals((string?)binding.Attribute("Key"), "Delete", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Checkout_NaoUsaEmojiETemSelecaoPersistenteDaFormaPagamento()
    {
        var document = LoadMainWindow();
        var source = File.ReadAllText(Path.Combine(ProjectDirectory(), "MainWindow.xaml"));

        Assert.DoesNotContain("💵", source, StringComparison.Ordinal);
        Assert.DoesNotContain("📱", source, StringComparison.Ordinal);
        Assert.DoesNotContain("💳", source, StringComparison.Ordinal);

        var choices = document.Descendants(Presentation + "RadioButton")
            .Where(button => ((string?)button.Attribute("Command"))?.Contains("SelecionarFormaPagamentoCommand", StringComparison.Ordinal) == true)
            .ToArray();

        Assert.Equal(4, choices.Length);
        Assert.All(choices, choice =>
        {
            Assert.False(string.IsNullOrWhiteSpace((string?)choice.Attribute("IsChecked")));
            Assert.Equal("FormaPagamento", (string?)choice.Attribute("GroupName"));
            Assert.Contains("Mode=TwoWay", (string?)choice.Attribute("IsChecked"));
            Assert.False(string.IsNullOrWhiteSpace((string?)choice.Attribute("AutomationProperties.Name")));
        });
    }

    private static XDocument LoadMainWindow() =>
        XDocument.Load(Path.Combine(ProjectDirectory(), "MainWindow.xaml"));

    private static string ProjectDirectory() => Path.Combine(
        FindRepositoryRoot(), "src", "CaixaMercado.PDV");

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "CaixaMercado.sln"))) return directory.FullName;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Raiz do repositório não encontrada.");
    }
}
