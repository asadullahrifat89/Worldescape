using System.Net.Http;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Worldescape.Data;
using Worldescape.Service;

namespace Worldescape
{
    public partial class LoginPage : Page
    {
        #region Fields

        private readonly HttpServiceHelper _httpServiceHelper;

        #endregion

        public LoginPage()
        {
            InitializeComponent();
            LoginModelHolder.DataContext = LoginModel;
            _httpServiceHelper = App.ServiceProvider.GetService(typeof(HttpServiceHelper)) as HttpServiceHelper;
            CheckIfModelValid();
        }

        #region Properties

        public LoginModel LoginModel { get; set; } = new LoginModel();

        #endregion

        private bool CheckIfModelValid()
        {
            if (!LoginModel.Email.IsNullOrBlank() && !LoginModel.Password.IsNullOrBlank())
                Button_Login.IsEnabled = true;
            else
                Button_Login.IsEnabled = false;

            return Button_Login.IsEnabled;
        }

        private void Control_BindingValidationError(object sender, ValidationErrorEventArgs e)
        {
            CheckIfModelValid();
        }

        private void Control_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckIfModelValid();
        }

        private async void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIfModelValid())
                return;

            var response = await _httpServiceHelper.SendGetRequest<StringResponse>(
               actionUri: Constants.Action_GetApiToken,
               payload: new GetApiTokenQueryRequest { Password = LoginModel.Password, Email = LoginModel.Email, });

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK || !response.ExternalError.IsNullOrBlank())
            {
                MessageBox.Show(response.ExternalError.ToString());
            }
            else
            {
                var token = response.Response;

                if (token.IsNullOrBlank())
                {
                    MessageBox.Show("Failed to login.");
                    return;
                }

                App.Token = token;

                var user = await _httpServiceHelper.SendGetRequest<User>(
                   actionUri: Constants.Action_GetUser,
                   payload: new GetUserQueryRequest() { Token = token, Email = LoginModel.Email, Password = LoginModel.Password });

                if (user == null || user.IsEmpty())
                {
                    MessageBox.Show("Failed to login.");
                    return;
                }

                App.User = user;
                App.InWorld = new InWorld() { Id = 786, Name = "Test World" }; // TODO: for the time time being demo world

                var mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;
                mainPage.NavigateToPage("/InsideWorldPage");
            }
        }

        private void Button_SignUp_Click(object sender, RoutedEventArgs e)
        {
            var mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;
            mainPage.NavigateToPage("/SignupPage");
        }
    }
}
