using Microsoft.Extensions.Logging;
using System;
using Windows.UI.Xaml.Controls;
using Worldescape.Data;

namespace Worldescape
{
    public partial class MainPage : Page
    {
        #region Fields

        private readonly ILogger<MainPage> _logger;

        #endregion

        #region Ctor

        public MainPage(ILogger<MainPage> logger)
        {
            InitializeComponent();
            _logger = logger;

            CurrentUserHolder.DataContext = CurrentUserModel;
        }

        #endregion

        #region Properties

        public CurrentUserModel CurrentUserModel { get; set; } = new CurrentUserModel();

        #endregion

        #region Methods

        public void SetCurrentUserModel(string firstName, string profileImageUrl = null, string avatarImageUrl = null)
        {
            CurrentUserModel.FirstName = firstName;
            if (!profileImageUrl.IsNullOrBlank())
            {
                CurrentUserModel.ProfileImageUrl = profileImageUrl; 
            }
            if (!avatarImageUrl.IsNullOrBlank())
            {
                CurrentUserModel.AvatarImageUrl = avatarImageUrl; 
            }
            CurrentUserHolder.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

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
