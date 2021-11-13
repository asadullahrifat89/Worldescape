using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.UI.Xaml;
using Worldescape.Contracts.Services;
using Worldescape.Extensions;
using Worldescape.Services;

namespace Worldescape
{
    public sealed partial class App : Application
    {
        public static IServiceProvider _serviceProvider;

        public App()
        {
            this.InitializeComponent();

            // Enter construction logic here...

            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            var mainPage = App._serviceProvider.GetService(typeof(MainPage)) as MainPage;

            //var mainPage = new MainPage();
            Window.Current.Content = mainPage;

            mainPage.NavigateToPage("/LoginPage");
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Extensions
            services.AddHttpService();

            // Core Services
            services.AddSingleton<IWorldescapeHubService, WorldescapeHubService>();


            services.AddSingleton<MainPage>();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.UnhandledException += App_UnhandledException;
        }

        private void App_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.Message + Environment.NewLine + e.ExceptionObject.StackTrace);
            e.Handled = true;
        }
    }
}
