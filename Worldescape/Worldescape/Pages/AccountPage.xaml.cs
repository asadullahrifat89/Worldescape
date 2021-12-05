using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Worldescape.Common;
using Worldescape.Service;

namespace Worldescape
{
    public partial class AccountPage : Page
    {
        #region Fields
                
        readonly ImageHelper _imageHelper;
        readonly UrlHelper _urlHelper;

        readonly UserRepository _userRepository;

        readonly MainPage _mainPage;

        #endregion

        #region Ctor

        public AccountPage(
            //ImageHelper imageHelper,
            //UrlHelper urlHelper,
            //MainPage mainPage
            )
        {
            InitializeComponent();

            //_imageHelper = imageHelper;
            //_urlHelper = urlHelper;
            //_mainPage = mainPage;

            AccountModelHolder.DataContext = AccountModel;

            _imageHelper = App.ServiceProvider.GetService(typeof(ImageHelper)) as ImageHelper;
            _urlHelper = App.ServiceProvider.GetService(typeof(UrlHelper)) as UrlHelper;
            _userRepository = App.ServiceProvider.GetService(typeof(UserRepository)) as UserRepository;
            _mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;
        }

        private void LoadUserDetails()
        {
            AccountModel.Id = App.User.Id;
            AccountModel.DateOfBirth = App.User.DateOfBirth;
            AccountModel.Email = App.User.Email;
            AccountModel.FirstName = App.User.FirstName;
            AccountModel.LastName = App.User.LastName;
            AccountModel.Password = App.User.Password;
            //PasswordBox_Pasword.Password = AccountModel.Password;

            switch (App.User.Gender)
            {
                case Gender.Male:
                    RadioButton_Male.IsChecked = true;
                    break;
                case Gender.Female:
                    RadioButton_Female.IsChecked = true;
                    break;
                case Gender.Other:
                    RadioButton_Other.IsChecked = true;
                    break;
                default:
                    break;
            }

            AccountModel.ImageUrl = App.User.ImageUrl; // If not set then default is set after logging in
            Image_ProfileImageUrl.Source = _imageHelper.GetBitmapImage(App.User.ImageUrl.Contains("ms-appx:") ? App.User.ImageUrl : _urlHelper.BuildBlobUrl(App.Token, App.User.ImageUrl));
            TextBlock_Name.Text = App.User.Name;
        }

        #endregion

        #region Properties

        public AccountModel AccountModel { get; set; } = new AccountModel();

        #endregion

        #region Methods

        #region Functionality

        private bool CheckIfModelValid()
        {
            //AccountModel.Password = PasswordBox_Pasword.Password;

            if (!AccountModel.FirstName.IsNullOrBlank()
                && !AccountModel.LastName.IsNullOrBlank()
                && !AccountModel.Email.IsNullOrBlank()
                && !AccountModel.Password.IsNullOrBlank() && AccountModel.Password.Length <= 12
                && AccountModel.DateOfBirth != null && AccountModel.DateOfBirth != DateTime.MinValue
                && (RadioButton_Male.IsChecked.GetValueOrDefault() || RadioButton_Female.IsChecked.GetValueOrDefault() || RadioButton_Other.IsChecked.GetValueOrDefault()))
                Button_UpdateAccount.IsEnabled = true;
            else
                Button_UpdateAccount.IsEnabled = false;

            return Button_UpdateAccount.IsEnabled;
        }

        private void NavigateToLoginPage()
        {
            _mainPage.NavigateToPage(Constants.Page_LoginPage);
        }

        private void NavigateToWorldsPage()
        {
            _mainPage.NavigateToPage(Constants.Page_WorldsPage);
        }

        private async Task UpdateUser()
        {
            _mainPage.SetIsBusy(true, "Saving your account...");

            var response = await _userRepository.UpdateUser(
               token: App.Token,
               id: AccountModel.Id,
               email: AccountModel.Email,
               password: AccountModel.Password,
               dateofbirth: AccountModel.DateOfBirth,
               gender: RadioButton_Male.IsChecked.Value ? Gender.Male : RadioButton_Female.IsChecked.Value ? Gender.Female : RadioButton_Other.IsChecked.Value ? Gender.Other : Gender.Other,
               firstname: AccountModel.FirstName,
               lastname: AccountModel.LastName,
               imageUrl: AccountModel.ImageUrl);

            if (!response.Success)
            {
                var contentDialogue = new ContentDialogueWindow(title: "Error!", message: response.Error);
                contentDialogue.Show();

                _mainPage.SetIsBusy(false);
            }
            else
            {
                NavigateToLoginPage();
                _mainPage.SetIsBusy(false);
            }
        }

        #endregion

        #region Page Events

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserDetails();
            CheckIfModelValid();
        }

        #endregion

        #region UX Events

        private void Control_BindingValidationError(object sender, ValidationErrorEventArgs e)
        {
            CheckIfModelValid();
        }

        private void Control_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckIfModelValid();
        }

        #endregion

        #region Button Events

        private void Button_UploadImageUrl_Click(object sender, RoutedEventArgs e)
        {
            var imagePickerWindow = new ImagePickerWindow(blobId: (blobId) =>
            {
                AccountModel.ImageUrl = blobId;
                Image_ProfileImageUrl.Source = _imageHelper.GetBitmapImage(_urlHelper.BuildBlobUrl(App.Token, blobId));
            }, imageUrl: AccountModel.ImageUrl);

            imagePickerWindow.Show();
        }

        private void Button_RemoveImageUrl_Click(object sender, RoutedEventArgs e)
        {
            string defaultImageUrl = null;

            // Gender wise default image
            switch (App.User.Gender)
            {
                case Gender.Male:
                    defaultImageUrl = "ms-appx:///Assets/Images/Defaults/ProfileImage_Male.png";
                    break;
                case Gender.Female:
                    defaultImageUrl = "ms-appx:///Assets/Images/Defaults/ProfileImage_Female.png";
                    break;
                case Gender.Other:
                    defaultImageUrl = "ms-appx:///Assets/Images/Defaults/ProfileImage_Other.png";
                    break;
                default:
                    break;
            }

            AccountModel.ImageUrl = defaultImageUrl;
            Image_ProfileImageUrl.Source = _imageHelper.GetBitmapImage(AccountModel.ImageUrl);
        }

        private async void Button_UpdateAccount_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIfModelValid())
                return;

            await UpdateUser();
        }

        private void Button_Discard_Click(object sender, RoutedEventArgs e)
        {
            // If user is already logged in then go to worlds page
            if (!App.User.IsEmpty())
            {
                NavigateToWorldsPage();
            }
            else
            {
                NavigateToLoginPage();
            }
        }

        #endregion

        #endregion      
    }
}
