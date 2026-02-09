using Avalonia;
using System.Globalization;
using CommunityToolkit.Mvvm.DependencyInjection;
using EgoEngineLibrary.Frontend.Configuration;
using EgoEngineLibrary.Frontend.DependencyInjection;
using EgoErpArchiver.Configuration;
using EgoErpArchiver.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace EgoErpArchiver;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var culture = new CultureInfo("en-US");

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        ConfigureServices();
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static void ConfigureServices()
    {
        var services = new ServiceCollection();

        var appConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EgoErpArchiver");
        Directory.CreateDirectory(appConfigDir);
        services.AddConfig<SettingsConfig>(new JsonConfigProvider(Path.Combine(appConfigDir, "settings.json"),
            ConfigJsonContext.Default.SettingsConfig));

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<SettingsViewModel>();
        
        var serviceProvider = services.BuildServiceProvider();
        Ioc.Default.ConfigureServices(serviceProvider);
    }
}
