using Microsoft.Extensions.Logging;
using System;
using Windows.UI.Xaml.Controls;

namespace Worldescape
{
    public partial class MainPage : Page
    {
        #region Fields

        private readonly ILogger<MainPage> _logger;

        #endregion

        public MainPage(ILogger<MainPage> logger)
        {
            InitializeComponent();

            _logger = logger;
        }

        #region Methods

        public void LogError(Exception error)
        {
            _logger.LogError(error, error.Message);
        }

        public void NavigateToPage(Page page)
        {
            // Set target to target page:            
            PageContainer.Content = page;
        }

        public void NavigateToPage(string targetUri)
        {
            // Navigate to the target page:
            Uri uri = new Uri(targetUri, UriKind.Relative);
            PageContainer.Source = uri;
        }

        #endregion
    }
}
