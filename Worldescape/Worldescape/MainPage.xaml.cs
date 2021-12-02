using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Worldescape.Common;
using Worldescape.Service;

namespace Worldescape
{
    public partial class MainPage : Page
    {
        #region Fields

        readonly ImageHelper _imageHelper;
        readonly UrlHelper _urlHelper;

        #endregion

        #region Ctor

        public MainPage()
        {
            InitializeComponent();
            LoggedInUserHolder.DataContext = LoggedInUserModel;

            _imageHelper = App.ServiceProvider.GetService(typeof(ImageHelper)) as ImageHelper;
            _urlHelper = App.ServiceProvider.GetService(typeof(UrlHelper)) as UrlHelper;
        }

        #endregion

        #region Properties

        public LoggedInUserModel LoggedInUserModel { get; set; } = new LoggedInUserModel();

        #endregion

        #region Methods

        #region Functionality

        private void ProfileDetails()
        {
            NavigateToPage(Constants.Page_AccountPage);
            MyAcountButton.IsChecked = false;
        }

        private void Logout()
        {
            App.User = new User();
            App.World = new World();
            App.Token = null;

            NavigateToPage(Constants.Page_LoginPage);
            MyAcountButton.IsChecked = false;
            LoggedInUserHolder.Visibility = Visibility.Collapsed;
        }

        public void SetLoggedInUserModel()
        {
            string defaultImageUrl = string.Empty;

            LoggedInUserHolder.Visibility = Visibility.Visible;
            LoggedInUserModel.FirstName = App.User.Name;

            if (App.User.ImageUrl.IsNullOrBlank())
            {
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

                App.User.ImageUrl = defaultImageUrl;
            }

            LoggedInUserModel.ProfileImageUrl = App.User.ImageUrl.Contains("ms-appx:") ? App.User.ImageUrl : _urlHelper.BuildBlobUrl(App.Token, App.User.ImageUrl);
            Image_ProfileImageUrl.Source = _imageHelper.GetBitmapImage(LoggedInUserModel.ProfileImageUrl);
        }

        /// <summary>
        /// Sets the busy indicator to busy status with a busy message.
        /// </summary>
        /// <param name="isBusy"></param>
        /// <param name="busyMessage"></param>
        public void SetIsBusy(bool isBusy, string busyMessage = null)
        {
            Grid_Root.IsEnabled = !isBusy;
            Grid_Root.Opacity = Grid_Root.IsEnabled ? 1 : 0.5;
            BusyMessageHolder.Text = Grid_Root.IsEnabled ? "" : busyMessage ?? "Loading...";
        }

        /// <summary>
        /// Navigate to the target page.
        /// </summary>
        /// <param name="targetUri"></param>
        public void NavigateToPage(string targetUri)
        {
            switch (targetUri)
            {
                case Constants.Page_WorldsPage:
                    {
                        PageContainerFrame.Content = App.ServiceProvider.GetService(typeof(WorldsPage)) as Page;
                    }
                    break;
                case Constants.Page_InsideWorldPage:
                    {
                        PageContainerFrame.Content = App.ServiceProvider.GetService(typeof(InsideWorldPage)) as Page;
                    }
                    break;
                //case Constants.Page_LoginPage:
                //    {
                //        PageContainerFrame.Content = App.ServiceProvider.GetService(typeof(LoginPage)) as Page;
                //    }
                //    break;
                //case Constants.Page_SignupPage:
                //    {
                //        PageContainerFrame.Content = App.ServiceProvider.GetService(typeof(SignupPage)) as Page;
                //    }
                //    break;
                //case Constants.Page_AccountPage:
                //    {
                //        PageContainerFrame.Content = App.ServiceProvider.GetService(typeof(AccountPage)) as Page;
                //    }
                //    break;
                default:
                    {
                        Uri uri = new Uri(targetUri, UriKind.Relative);
                        PageContainerFrame.Source = uri;
                    }
                    break;
            };
        }

        #endregion

        #region Button Events

        private void MenuItem_ProfileDetails_Click(object sender, RoutedEventArgs e)
        {
            if (App.World.IsEmpty())
            {
                ProfileDetails();
            }
            else
            {
                var contentDialogue = new ContentDialogueWindow(title: "Warning!", message: "Are you sure you want to leave?", result: (result) =>
                {
                    if (result)
                        ProfileDetails();
                });

                contentDialogue.Show();
            }
        }

        private void MenuItem_Logout_Click(object sender, RoutedEventArgs e)
        {
            if (App.World.IsEmpty())
            {
                Logout();
            }
            else
            {
                var contentDialogue = new ContentDialogueWindow(title: "Warning!", message: "Are you sure you want to leave?", result: (result) =>
                {
                    if (result)
                        Logout();
                });

                contentDialogue.Show();
            }
        }

        #endregion

        #endregion
    }
}
