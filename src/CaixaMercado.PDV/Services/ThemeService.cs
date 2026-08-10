using System.IO;

namespace CaixaMercado.PDV.Services;

public sealed class ThemeService
{
    private const string ThemeDictionaryMarker = "Themes/Theme.";
    private readonly System.Windows.ResourceDictionary _applicationResources;
    private readonly string _preferenceFilePath;

    public ThemeService(System.Windows.ResourceDictionary applicationResources, string? preferenceFilePath = null)
    {
        _applicationResources = applicationResources ?? throw new ArgumentNullException(nameof(applicationResources));
        _preferenceFilePath = preferenceFilePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CaixaMercado",
            "theme.txt");

        CurrentTheme = LoadPreference();
        Apply(CurrentTheme, persist: false);
    }

    public AppTheme CurrentTheme { get; private set; }

    public event EventHandler? ThemeChanged;

    public void Toggle()
    {
        Apply(CurrentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark);
    }

    public void Apply(AppTheme theme, bool persist = true)
    {
        var dictionaries = _applicationResources.MergedDictionaries;
        var currentDictionary = dictionaries.FirstOrDefault(IsThemeDictionary);
        var targetDictionary = new System.Windows.ResourceDictionary
        {
            Source = new Uri($"/CaixaMercado.PDV;component/Themes/Theme.{theme}.xaml", UriKind.Relative)
        };

        if (currentDictionary == null)
        {
            dictionaries.Insert(0, targetDictionary);
        }
        else
        {
            var index = dictionaries.IndexOf(currentDictionary);
            dictionaries[index] = targetDictionary;
        }

        CurrentTheme = theme;

        if (persist)
        {
            SavePreference();
        }

        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    private static bool IsThemeDictionary(System.Windows.ResourceDictionary dictionary)
    {
        return dictionary.Source?.OriginalString.Contains(
            ThemeDictionaryMarker,
            StringComparison.OrdinalIgnoreCase) == true;
    }

    private AppTheme LoadPreference()
    {
        try
        {
            if (File.Exists(_preferenceFilePath)
                && Enum.TryParse<AppTheme>(File.ReadAllText(_preferenceFilePath).Trim(), true, out var theme))
            {
                return theme;
            }
        }
        catch (IOException)
        {
            // Preferência indisponível não pode impedir a abertura do caixa.
        }
        catch (UnauthorizedAccessException)
        {
            // Estações com perfil restrito usam o tema padrão.
        }

        return AppTheme.Dark;
    }

    private void SavePreference()
    {
        try
        {
            var directory = Path.GetDirectoryName(_preferenceFilePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_preferenceFilePath, CurrentTheme.ToString());
        }
        catch (IOException)
        {
            // A troca atual continua válida mesmo se a preferência não puder ser salva.
        }
        catch (UnauthorizedAccessException)
        {
            // A troca atual continua válida em perfis sem permissão de escrita.
        }
    }
}
