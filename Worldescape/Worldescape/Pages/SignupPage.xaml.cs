using System;
using System.Threading.Tasks;
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
        private readonly MainPage _mainPage;

        #endregion

        #region Ctor

        public SignupPage()
        {
            InitializeComponent();
            SignUpModelHolder.DataContext = SignUpModel;
            _httpServiceHelper = App.ServiceProvider.GetService(typeof(HttpServiceHelper)) as HttpServiceHelper;
            _mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;
            CheckIfModelValid();
        }

        #endregion

        #region Properties

        public SignupModel SignUpModel { get; set; } = new SignupModel();

        #endregion

        #region Methods

        #region Functionality

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

        private void NavigateToLoginPage()
        {
            _mainPage.NavigateToPage("/LoginPage");
        }

        private async Task SignUp()
        {
            _mainPage.SetIsBusy(true, "Creating your account...");
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
        
        private async void Button_Signup_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIfModelValid())
                return;

            await SignUp();
        }

        

        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            NavigateToLoginPage();
        } 

        #endregion

        #endregion
    }
}
