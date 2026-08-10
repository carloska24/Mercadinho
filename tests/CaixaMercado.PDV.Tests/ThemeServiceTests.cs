using System.Threading;
using System.Windows;
using CaixaMercado.PDV.Services;

namespace CaixaMercado.PDV.Tests;

public class ThemeServiceTests
{
    [Fact]
    public void Toggle_SubstituiSomenteTemaEPreservaOutrosRecursos()
    {
        Exception? threadException = null;

        var thread = new Thread(() =>
        {
            var preferencePath = Path.Combine(Path.GetTempPath(), $"caixa-mercado-theme-{Guid.NewGuid():N}.txt");

            try
            {
                var application = new System.Windows.Application();
                var resources = new ResourceDictionary();
                var initialTheme = LoadTheme(AppTheme.Dark);
                var sentinel = new ResourceDictionary { ["Sentinel"] = "preservado" };
                resources.MergedDictionaries.Add(initialTheme);
                resources.MergedDictionaries.Add(sentinel);

                var service = new ThemeService(resources, preferencePath);
                var notifications = 0;
                service.ThemeChanged += (_, _) => notifications++;

                service.Toggle();

                Assert.Equal(AppTheme.Light, service.CurrentTheme);
                Assert.Same(sentinel, resources.MergedDictionaries[1]);
                Assert.Contains("Theme.Light.xaml", resources.MergedDictionaries[0].Source!.OriginalString);
                Assert.Equal("Light", File.ReadAllText(preferencePath));
                Assert.Equal(1, notifications);
                application.Shutdown();
            }
            catch (Exception ex)
            {
                threadException = ex;
            }
            finally
            {
                if (File.Exists(preferencePath))
                {
                    File.Delete(preferencePath);
                }
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(threadException);
    }

    private static ResourceDictionary LoadTheme(AppTheme theme)
    {
        return new ResourceDictionary
        {
            Source = new Uri($"/CaixaMercado.PDV;component/Themes/Theme.{theme}.xaml", UriKind.Relative)
        };
    }
}
