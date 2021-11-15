using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.UI.Xaml;
using Worldescape.Contracts.Services;
using Worldescape.Extensions;
using Worldescape.Pages;
using Worldescape.Services;
using Worldescape.Shared.Entities;

namespace Worldescape
{
    public sealed partial class App : Application
    {
        public static IServiceProvider ServiceProvider;

        public static User User = new User();

        public static InWorld InWorld = new InWorld();

        public App()
        {
            InitializeComponent();

            Startup += App_Startup;

            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            //var mainPage = new MainPage();
            var mainPage = ServiceProvider.GetService(typeof(MainPage)) as MainPage;
            Window.Current.Content = mainPage;

            mainPage.NavigateToPage("/LoginPage");
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            UnhandledException += App_UnhandledException;
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Extensions
            services.AddHttpService();

            // Core Services
            services.AddSingleton<IHubService, HubService>();

            // Pages
            services.AddSingleton<MainPage>();
            //services.AddSingleton<InsideWorldPage>();
        }

        private void App_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.Message);

            var mainPage = ServiceProvider.GetService(typeof(MainPage)) as MainPage;
            if (mainPage != null)
            {
                mainPage.LogError(e.ExceptionObject);
            }
            e.Handled = true;
        }
    }
}
