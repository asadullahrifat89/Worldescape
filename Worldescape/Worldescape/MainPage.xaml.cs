using Microsoft.Extensions.Logging;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
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

        public void SetCurrentUserModel(string avatarImageUrl = null)
        {
            var firstName = App.User.FirstName;
            var profileImageUrl = App.User.ImageUrl;

            string defaultImageUrl = string.Empty;

            CurrentUserHolder.Visibility = Windows.UI.Xaml.Visibility.Visible;
            CurrentUserModel.FirstName = firstName;

            // Gender wise default image
            switch (App.User.Gender)
            {
                case Gender.Male:
                    defaultImageUrl = "ms-appx:///Images/Defaults/ProfileImage_Male.png";
                    break;
                case Gender.Female:
                    defaultImageUrl = "ms-appx:///Images/Defaults/ProfileImage_Female.png";
                    break;
                case Gender.Other:
                    defaultImageUrl = "ms-appx:///Images/Defaults/ProfileImage_Other.png";
                    break;
                default:
                    break;
            }

            // If no profile picture was set
            CurrentUserModel.ProfileImageUrl = !profileImageUrl.IsNullOrBlank() ? profileImageUrl : defaultImageUrl;
            CurrentUserModel.AvatarImageUrl = !avatarImageUrl.IsNullOrBlank() ? avatarImageUrl : defaultImageUrl;

            AvatarImageUrlHolder.Source = new BitmapImage(new Uri(CurrentUserModel.AvatarImageUrl));
            ProfileImageUrlHolder.Source = new BitmapImage(new Uri(CurrentUserModel.ProfileImageUrl));
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
