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

        #endregion

        #region Ctor

        public MainPage()
        {
            InitializeComponent();
            CurrentUserHolder.DataContext = CurrentUserModel;
        }

        #endregion

        #region Properties

        public CurrentUserModel CurrentUserModel { get; set; } = new CurrentUserModel();

        #endregion

        #region Methods

        public void SetCurrentUserModel()
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

            App.User.ImageUrl = !profileImageUrl.IsNullOrBlank() ? profileImageUrl : defaultImageUrl;

            // If no profile picture was set
            CurrentUserModel.ProfileImageUrl = App.User.ImageUrl;
            ProfileImageUrlHolder.Source = new BitmapImage(new Uri(CurrentUserModel.ProfileImageUrl));
        }

        public void SetIsBusy(bool isBusy, string busyMessage = null)
        {
            Grid_root.IsEnabled = !isBusy;

            Grid_root.Opacity = Grid_root.IsEnabled ? 1 : 0.5;

            BusyMessageHolder.Text = Grid_root.IsEnabled ? "" : busyMessage ?? "Processing...";
        }

        public void NavigateToPage(string targetUri)
        {
            // Navigate to the target page:
            Uri uri = new Uri(targetUri, UriKind.Relative);
            PageContainerFrame.Source = uri;
        }

        #endregion
    }
}
