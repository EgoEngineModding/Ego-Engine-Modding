using System.Globalization;
using Avalonia;
using CommunityToolkit.Mvvm.DependencyInjection;
using EgoEngineLibrary.Frontend.Configuration;
using EgoEngineLibrary.Frontend.DependencyInjection;
using EgoErpArchiver.Configuration;
using EgoErpArchiver.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace EgoErpArchiver;

sealed class Program
{
    private static readonly string AppDataDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EgoErpArchiver");
    private static readonly string AppLogsDir = Path.Combine(AppDataDir, "logs", "log.txt");
    
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
        
        using var logger = ConfigureLogger();
        try
        {
            logger.Information("Application starting");
            ConfigureOptions(logger);
            ConfigureServices(logger);
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            logger.Fatal(e, "Application terminated unexpectedly.");
            throw;
        }
        finally
        {
            logger.Information("Application exited.");
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static Serilog.Core.Logger ConfigureLogger()
    {
        var builder = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File(AppLogsDir, rollingInterval: RollingInterval.Day);

        return builder.CreateLogger();
    }

    private static void ConfigureOptions(Serilog.ILogger logger)
    {
        Directory.CreateDirectory(AppDataDir);
        Config.Add<AppSettings>(new JsonConfigProvider(Path.Combine(AppDataDir, "settings.json"),
            ConfigJsonContext.Default.AppSettings));

        logger.Information("Loading settings");
        Config.LoadAll();
    }

    private static void ConfigureServices(Serilog.ILogger logger)
    {
        var services = new ServiceCollection();

        services.AddSingleton<ILoggerFactory, LoggerFactory>()
            .AddSingleton(typeof(ILogger<>), typeof(Logger<>))
            .AddSingleton<ILoggerProvider>(_ => new SerilogLoggerProvider(logger))
            .AddConfigOptions();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<ErpFileViewModel>();
        services.AddSingleton<ResourcesWorkspaceViewModel>();
        services.AddSingleton<TexturesWorkspaceViewModel>();
        services.AddSingleton<PackagesWorkspaceViewModel>();
        services.AddSingleton<XmlFilesWorkspaceViewModel>();
        
        var serviceProvider = services.BuildServiceProvider();
        Ioc.Default.ConfigureServices(serviceProvider);
    }
}
