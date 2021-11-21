using System;
using System.Net.Http;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Worldescape.Data;
using Worldescape.Service;

namespace Worldescape
{
    public partial class SignupPage : Page
    {
        #region Fields

        private readonly HttpServiceHelper _httpServiceHelper;

        #endregion

        #region Ctor

        public SignupPage()
        {
            InitializeComponent();
            SignUpModelHolder.DataContext = SignUpModel;
            _httpServiceHelper = App.ServiceProvider.GetService(typeof(HttpServiceHelper)) as HttpServiceHelper;
            CheckIfModelValid();
        }

        #endregion

        #region Properties

        public SignupModel SignUpModel { get; set; } = new SignupModel();

        #endregion

        #region Methods

        private bool CheckIfModelValid()
        {
            if (!SignUpModel.FirstName.IsNullOrBlank()
                && !SignUpModel.LastName.IsNullOrBlank()
                && !SignUpModel.Email.IsNullOrBlank()
                && !SignUpModel.Password.IsNullOrBlank()
                && SignUpModel.DateOfBirth != null
                && SignUpModel.DateOfBirth != DateTime.MinValue
                && (RadioButton_Male.IsChecked.GetValueOrDefault() || RadioButton_Female.IsChecked.GetValueOrDefault() || RadioButton_Other.IsChecked.GetValueOrDefault()))
                Button_Signup.IsEnabled = true;
            else
                Button_Signup.IsEnabled = false;

            return Button_Signup.IsEnabled;
        }

        private void Control_BindingValidationError(object sender, ValidationErrorEventArgs e)
        {
            CheckIfModelValid();
        }

        private void Control_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckIfModelValid();
        }

        private async void Button_Signup_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIfModelValid())
                return;

            var command = new AddUserCommandRequest
            {
                Email = SignUpModel.Email,
                Password = SignUpModel.Password,
                DateOfBirth = SignUpModel.DateOfBirth,
                Gender = RadioButton_Male.IsChecked.Value ? Gender.Male : RadioButton_Female.IsChecked.Value ? Gender.Female : RadioButton_Other.IsChecked.Value ? Gender.Other : Gender.Other,
                FirstName = SignUpModel.FirstName,
                LastName = SignUpModel.LastName,
                Name = SignUpModel.FirstName + " " + SignUpModel.LastName,
            };

            var response = await _httpServiceHelper.SendPostRequest<ServiceResponse>(
               actionUri: Constants.Action_AddUser,
               payload: command);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK || !response.ExternalError.IsNullOrBlank())
            {
                MessageBox.Show(response.ExternalError.ToString());
            }
            else
            {
                NavigateToLoginPage();
            }
        }

        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            NavigateToLoginPage();
        }

        private static void NavigateToLoginPage()
        {
            var mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;
            mainPage.NavigateToPage("/LoginPage");
        }

        #endregion
    }
}
