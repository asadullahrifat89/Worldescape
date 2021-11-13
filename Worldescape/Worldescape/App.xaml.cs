using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Windows.UI.Xaml;
using Worldescape.Interaction.Contracts.Services;
using Worldescape.Interaction.Extensions;
using Worldescape.Interaction.Services;

namespace Worldescape
{
    public sealed partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            this.InitializeComponent();

            // Enter construction logic here...

            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            var mainPage = new MainPage();
            Window.Current.Content = mainPage;
        }

		private void ConfigureServices(ServiceCollection services)
		{
			//var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
			//services.AddSingleton<IConfiguration>(configuration);

			// Extensions
			services.AddHttpService();

            // Core Services
            services.AddSingleton<IWorldescapeHubService, WorldescapeHubService>();
        }
	}
}
