using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.UI.Xaml;
using Worldescape.Service;
using Worldescape.Common;

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
                        
            _mainPage = ServiceProvider.GetService(typeof(MainPage)) as MainPage;
            Window.Current.Content = _mainPage;

            _mainPage.NavigateToPage(Constants.Page_LoginPage);
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            UnhandledException += App_UnhandledException;
        }

        #endregion

        #region Properties

        public static IServiceProvider ServiceProvider { get; set; }

        public static User User { get; set; } = new User();

        public static World World { get; set; } = new World();

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
            services.AddSingleton<UrlHelper>();
            services.AddSingleton<HttpServiceHelper>();
            services.AddSingleton<AvatarHelper>();
            services.AddSingleton<ConstructHelper>();
            services.AddSingleton<WorldHelper>();
            services.AddSingleton<PaginationHelper>();
            services.AddSingleton<ImageHelper>();
            services.AddSingleton<ElementHelper>();

            // Repositories
            services.AddSingleton<ApiTokenRepository>();
            services.AddSingleton<ConstructRepository>();
            services.AddSingleton<AvatarRepository>();
            services.AddSingleton<WorldRepository>();
            services.AddSingleton<UserRepository>();

            // Pages
            services.AddSingleton<MainPage>();

            services.AddSingleton<WorldsPage>();
            services.AddSingleton<InsideWorldPage>();

            //services.AddSingleton<LoginPage>();
            //services.AddSingleton<SignupPage>();
            //services.AddSingleton<AccountPage>();            

            // Controls
            services.AddSingleton<ConstructAssetPickerControl>();
        }

        private void App_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.Message);            
            e.Handled = true;
        }

        #endregion
    }
}
