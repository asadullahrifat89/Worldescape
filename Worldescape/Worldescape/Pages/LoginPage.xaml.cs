using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Worldescape.Common;
using Worldescape.Service;

namespace Worldescape
{
    public partial class LoginPage : Page
    {
        #region Fields

        private readonly HttpServiceHelper _httpServiceHelper;
        private readonly MainPage _mainPage;

        #endregion

        #region Ctor

        public LoginPage()
        {
            InitializeComponent();
            LoginModelHolder.DataContext = LoginModel;
            _httpServiceHelper = App.ServiceProvider.GetService(typeof(HttpServiceHelper)) as HttpServiceHelper;
            _mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;
            CheckIfModelValid();
        }

        #endregion

        #region Properties

        public LoginModel LoginModel { get; set; } = new LoginModel();

        #endregion

        #region Methods

        #region Functionality

        private bool CheckIfModelValid()
        {
            if (!LoginModel.Email.IsNullOrBlank() && !LoginModel.Password.IsNullOrBlank())
                Button_Login.IsEnabled = true;
            else
                Button_Login.IsEnabled = false;

            return Button_Login.IsEnabled;
        }

        private async Task Login()
        {
            if (!CheckIfModelValid())
                return;

            _mainPage.SetIsBusy(true);

            var response = await _httpServiceHelper.SendGetRequest<StringResponse>(
               actionUri: Constants.Action_GetApiToken,
               payload: new GetApiTokenQueryRequest { Password = LoginModel.Password, Email = LoginModel.Email, });

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK || !response.ExternalError.IsNullOrBlank())
            {
                var contentDialogue = new ContentDialogueWindow(title: "Error!", message: response.ExternalError.ToString());
                contentDialogue.Show();

                _mainPage.SetIsBusy(false);
            }
            else
            {
                var token = response.Response;

                if (token.IsNullOrBlank())
                {
                    var contentDialogue = new ContentDialogueWindow(title: "Error!", message: "Failed to login.");
                    contentDialogue.Show();

                    _mainPage.SetIsBusy(false);
                    return;
                }

                App.Token = token;

                var user = await _httpServiceHelper.SendGetRequest<User>(
                   actionUri: Constants.Action_GetUser,
                   payload: new GetUserQueryRequest() { Token = token, Email = LoginModel.Email, Password = LoginModel.Password });

                if (user == null || user.IsEmpty())
                {
                    var contentDialogue = new ContentDialogueWindow(title: "Error!", message: "Failed to login.");
                    contentDialogue.Show();

                    _mainPage.SetIsBusy(false);
                    return;
                }

                App.User = user;

                _mainPage.SetLoggedInUserModel();
                _mainPage.NavigateToPage(Constants.Page_WorldsPage);
                _mainPage.SetIsBusy(false);
            }
        }

        #endregion

        #region Button Events

        private async void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            await Login();
        }

        private void Button_SignUp_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.NavigateToPage(Constants.Page_SignupPage);
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

        private async void PasswordBox_Pasword_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            CheckIfModelValid();

            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                await Login();
            }
        }

        #endregion 

        #endregion
    }
}
