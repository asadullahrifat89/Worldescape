using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.UI.Xaml;
using Worldescape.Service;
using Worldescape.Common;
using Windows.UI;

namespace Worldescape
{
    public sealed partial class App : Application
    {
        #region Ctor

        public App()
        {
            InitializeComponent();

            Startup += App_Startup;

            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            var _mainPage = ServiceProvider.GetService(typeof(MainPage)) as MainPage;
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

        public static readonly Color[] BackgroundColors = new Color[]
        {
            Color.FromRgb(235, 157, 96), // Sandy Brown
            Color.FromRgb(224, 180, 221), // Pink Lavender
            Color.FromRgb(203, 167, 163), // Tuscany
            Color.FromRgb(37, 35, 88), // Space Cadet
            Color.FromRgb(106, 101, 107), // Dim Gray
            Color.FromRgb(157, 192, 142), // Dark Sea Green
            Color.FromRgb(255, 196, 0), // Mikado Yellow
            Color.FromRgb(189, 28, 108), // Maroon X 11
        };

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
            services.AddSingleton<PortalHelper>();
            services.AddSingleton<ChatBubbleHelper>();

            // Repositories
            services.AddSingleton<ApiTokenRepository>();
            services.AddSingleton<ConstructRepository>();
            services.AddSingleton<AvatarRepository>();
            services.AddSingleton<WorldRepository>();
            services.AddSingleton<UserRepository>();
            services.AddSingleton<BlobRepository>();

            // Pages
            services.AddSingleton<MainPage>(); // just the main page
        }

        private void App_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.Message);
            e.Handled = true;
        }

        public static void NavigateToPage(string targetUri)
        {
            var _mainPage = ServiceProvider.GetService(typeof(MainPage)) as MainPage;
            _mainPage.NavigateToPage(targetUri);
        }

        public static void SetIsBusy(bool isBusy, string busyMessage = null)
        {
            var _mainPage = ServiceProvider.GetService(typeof(MainPage)) as MainPage;
            _mainPage.SetIsBusy(isBusy, busyMessage);
        }

        public static void SetLoggedInUserModel()
        {
            var _mainPage = ServiceProvider.GetService(typeof(MainPage)) as MainPage;
            _mainPage.SetLoggedInUserModel();
        }
        #endregion
    }
}
