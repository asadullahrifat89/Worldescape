using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Data;

namespace Worldescape
{
    public partial class MainPage : Page
    {
        #region Fields

        readonly ImageHelper _imageHelper;

        #endregion

        #region Ctor

        public MainPage()
        {
            InitializeComponent();
            CurrentUserHolder.DataContext = CurrentUserModel;
            _imageHelper = App.ServiceProvider.GetService(typeof(ImageHelper)) as ImageHelper;
        }

        #endregion

        #region Properties

        public CurrentUserModel CurrentUserModel { get; set; } = new CurrentUserModel();

        #endregion

        #region Methods

        public void SetCurrentUserModel()
        {
            var userName = App.User.Name;
            var profileImageUrl = App.User.ImageUrl;

            string defaultImageUrl = string.Empty;

            CurrentUserHolder.Visibility = Windows.UI.Xaml.Visibility.Visible;
            CurrentUserModel.FirstName = userName;

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
            Image_ProfileImageUrl.Source = _imageHelper.GetBitmapImage(CurrentUserModel.ProfileImageUrl);
        }

        public void SetIsBusy(bool isBusy, string busyMessage = null)
        {
            Grid_Root.IsEnabled = !isBusy;

            Grid_Root.Opacity = Grid_Root.IsEnabled ? 1 : 0.5;

            BusyMessageHolder.Text = Grid_Root.IsEnabled ? "" : busyMessage ?? "Loading...";
        }

        public void NavigateToPage(string targetUri)
        {
            // Navigate to the target page:
            Uri uri = new Uri(targetUri, UriKind.Relative);
            PageContainerFrame.Source = uri;
        }

        #endregion

        private void MenuItem_ProfileDetails_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (App.World.IsEmpty())
                NavigateToPage(Constants.Page_AccountPage);
            else
                MessageBox.Show("It is not permissible to change your account information while being connected to a world. You can leave this world, only then your can.", "Sorry!");

            MyAcountButton.IsChecked = false;
        }

        private void MenuItem_Logout_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            App.User = null;
            App.World = null;
            App.Token = null;

            NavigateToPage(Constants.Page_LoginPage);
            MyAcountButton.IsChecked = false;
        }
    }
}
