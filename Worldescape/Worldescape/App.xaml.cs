using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.UI.Xaml;
using Worldescape.Service;
using Worldescape.Data;

namespace Worldescape
{
    public sealed partial class App : Application
    {
        private readonly MainPage _mainPage;

        #region Ctor

        public App()
        {
            InitializeComponent();

            Startup += App_Startup;

            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            //var mainPage = new MainPage();
            _mainPage = ServiceProvider.GetService(typeof(MainPage)) as MainPage;
            Window.Current.Content = _mainPage;

            _mainPage.NavigateToPage("/LoginPage");
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            UnhandledException += App_UnhandledException;
        }

        #endregion

        #region Properties

        public static IServiceProvider ServiceProvider { get; set; }

        public static User User { get; set; } = new User();

        public static InWorld InWorld { get; set; } = new InWorld();

        public static string Token { get; set; } = string.Empty;

        #endregion

        #region Methods

        private void ConfigureServices(ServiceCollection services)
        {
            // Extensions
            services.AddHttpService();

            // Core Services            
            services.AddSingleton<IHubService, HubService>();

            // Helpers
            services.AddSingleton<AssetUrlHelper>();
            services.AddSingleton<HttpServiceHelper>();
            services.AddSingleton<AvatarHelper>();
            services.AddSingleton<ConstructHelper>();

            // Pages
            services.AddSingleton<MainPage>();
            //services.AddSingleton<InsideWorldPage>();
            //services.AddSingleton<LoginPage>();
            //services.AddSingleton<SignupPage>();
        }

        private void App_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {

#if DEBUG
            MessageBox.Show(e.ExceptionObject.ToString());
#else
            Console.WriteLine(e.ExceptionObject.Message);            
            e.Handled = true;
#endif
        }

        #endregion
    }
}
