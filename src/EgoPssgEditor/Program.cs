using Avalonia;
using System.Globalization;
using CommunityToolkit.Mvvm.DependencyInjection;
using EgoEngineLibrary.Frontend.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace EgoPssgEditor;

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

        services.AddSingleton<IDialogService, AvaloniaDialogService>();
        
        var serviceProvider = services.BuildServiceProvider();
        DialogService.Instance = serviceProvider.GetRequiredService<IDialogService>();

        Ioc.Default.ConfigureServices(serviceProvider);
    }
}
