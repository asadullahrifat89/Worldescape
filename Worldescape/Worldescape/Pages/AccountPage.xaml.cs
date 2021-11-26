using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Data;
using Worldescape.Service;

namespace Worldescape
{
    public partial class AccountPage : Page
    {
        #region Fields

        private readonly HttpServiceHelper _httpServiceHelper;
        private readonly MainPage _mainPage;

        #endregion

        #region Ctor

        public AccountPage()
        {
            InitializeComponent();
            AccountModelHolder.DataContext = AccountModel;
            _httpServiceHelper = App.ServiceProvider.GetService(typeof(HttpServiceHelper)) as HttpServiceHelper;
            _mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;

            LoadUserDetails();
            CheckIfModelValid();
        }

        private void LoadUserDetails()
        {
            AccountModel.Id = App.User.Id;
            AccountModel.DateOfBirth = App.User.DateOfBirth;
            AccountModel.Email = App.User.Email;
            AccountModel.FirstName = App.User.FirstName;
            AccountModel.LastName = App.User.LastName;
            AccountModel.Password = App.User.Password;

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

            AccountModel.ImageUrl = App.User.ImageUrl;
            Image_ProfileImageUrl.Source = new BitmapImage(new Uri(App.User.ImageUrl));
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
            if (!AccountModel.FirstName.IsNullOrBlank()
                && !AccountModel.LastName.IsNullOrBlank()
                && !AccountModel.Email.IsNullOrBlank()
                && !AccountModel.Password.IsNullOrBlank()
                && AccountModel.DateOfBirth != null
                && AccountModel.DateOfBirth != DateTime.MinValue
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

        private async Task UpdateUser()
        {
            _mainPage.SetIsBusy(true, "Saving your account...");
            var command = new UpdateUserCommandRequest
            {
                Token = App.Token,
                //Phone = AccountModel.Phone,
                Id = AccountModel.Id,
                Email = AccountModel.Email,
                Password = AccountModel.Password,
                DateOfBirth = AccountModel.DateOfBirth,
                Gender = RadioButton_Male.IsChecked.Value ? Gender.Male : RadioButton_Female.IsChecked.Value ? Gender.Female : RadioButton_Other.IsChecked.Value ? Gender.Other : Gender.Other,
                FirstName = AccountModel.FirstName,
                LastName = AccountModel.LastName,
                Name = AccountModel.FirstName + " " + AccountModel.LastName,
                ImageUrl = AccountModel.ImageUrl,
            };

            var response = await _httpServiceHelper.SendPostRequest<ServiceResponse>(
               actionUri: Constants.Action_UpdateUser,
               payload: command);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK || !response.ExternalError.IsNullOrBlank())
            {
                MessageBox.Show(response.ExternalError.ToString());
                _mainPage.SetIsBusy(false);
            }
            else
            {
                NavigateToLoginPage();
                _mainPage.SetIsBusy(false);
            }
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

        private void FileOpenDialogPresenter_ImageUrl_FileOpened(object sender, CSHTML5.Extensions.FileOpenDialog.FileOpenedEventArgs e)
        {
            string dataURL = e.DataURL;
            
            if (string.IsNullOrEmpty(dataURL))
                return;

            var bitmapimage = new BitmapImage();
            bitmapimage.SetSource(dataURL);
            Image_ProfileImageUrl.Source = bitmapimage;

            //var base64String = e.DataURL;
            //base64String = base64String.Substring(base64String.IndexOf(',') + 1);

            //byte[] byteBuffer = Convert.FromBase64String(base64String);

            //using (MemoryStream memoryStream = new MemoryStream(byteBuffer))
            //{
            //    var bitmapImage = new BitmapImage();
            //    bitmapImage.SetSource(memoryStream);
            //    Image_ProfileImageUrl.Source = bitmapImage;
            //}
        }

        private async void Button_UpdateAccount_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIfModelValid())
                return;

            await UpdateUser();
        }

        private void Button_Discard_Click(object sender, RoutedEventArgs e)
        {
            NavigateToLoginPage();
        }


        #endregion

        #endregion


    }
}
