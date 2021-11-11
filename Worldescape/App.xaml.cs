using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using Worldescape.Common;

namespace Worldescape;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        ServiceCollection services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        DispatcherUnhandledException += (sender, args) =>
        {
            args.Handled = true;
        };

        var mainWindow = _serviceProvider.GetService<MainWindow>();
        mainWindow?.Show();
    }

    private void ConfigureServices(ServiceCollection services)
    {

#if DEBUG
        var appsettings = "appsettings.Development.json";
#else
        var appsettings = "appsettings.Production.json";
#endif

        var configuration = new ConfigurationBuilder().AddJsonFile(appsettings).Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Services
        services.AddSingleton<IWorldescapeHubService, WorldescapeHubService>();

        // Extensions
        services.AddHttpService();

        // Views
        services.AddSingleton<MainWindow>();
    }

    private void OnExit(object sender, ExitEventArgs e)
    {

    }
}

