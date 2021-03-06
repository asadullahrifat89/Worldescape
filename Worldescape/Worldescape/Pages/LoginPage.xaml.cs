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

        readonly ApiTokenRepository _apiTokenRepository;
        readonly UserRepository _userRepository;

        #endregion

        #region Ctor

        public LoginPage()
        {
            InitializeComponent();

            LoginModelHolder.DataContext = LoginModel;

            _apiTokenRepository = App.ServiceProvider.GetService(typeof(ApiTokenRepository)) as ApiTokenRepository;
            _userRepository = App.ServiceProvider.GetService(typeof(UserRepository)) as UserRepository;
        }

        #endregion

        #region Properties

        public LoginModel LoginModel { get; set; } = new LoginModel();

        #endregion

        #region Methods

        #region Page Events
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CheckIfModelValid();            
        }
        #endregion

        #region Button Events

        private async void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            await Login();
        }

        private void Button_SignUp_Click(object sender, RoutedEventArgs e)
        {
            App.NavigateToPage(Constants.Page_SignupPage);
        }

        #endregion

        #region UX Events

        private void PasswordBox_Pasword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            CheckIfModelValid();
        }

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
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                await Login();
            }
        }

        #endregion

        #region Functionality

        private bool CheckIfModelValid()
        {
            if (!LoginModel.Email.IsNullOrBlank() && !LoginModel.Password.IsNullOrBlank() && LoginModel.Password.Length <= 12)
                Button_Login.IsEnabled = true;
            else
                Button_Login.IsEnabled = false;

            return Button_Login.IsEnabled;
        }

        private async Task Login()
        {
            if (!CheckIfModelValid())
                return;

            App.SetIsBusy(true);

            var apiTokenResponse = await _apiTokenRepository.GetApiToken(
                email: LoginModel.Email,
                password: LoginModel.Password);

            if (!apiTokenResponse.Success)
            {
                var contentDialogue = new MessageDialogueWindow(title: "Error!", message: apiTokenResponse.Error);
                contentDialogue.Show();

                App.SetIsBusy(false);
            }
            else
            {
                var token = apiTokenResponse.Result;

                if (token.IsNullOrBlank())
                {
                    var contentDialogue = new MessageDialogueWindow(title: "Error!", message: "Failed to login.");
                    contentDialogue.Show();

                    App.SetIsBusy(false);
                    return;
                }

                App.Token = token;

                var loginResponse = await _userRepository.GetUser(
                    token: token,
                    email: LoginModel.Email,
                    password: LoginModel.Password);

                if (!loginResponse.Success)
                {
                    var contentDialogue = new MessageDialogueWindow(title: "Error!", message: loginResponse.Error);
                    contentDialogue.Show();

                    App.SetIsBusy(false);
                }
                else
                {
                    App.User = loginResponse.Result;

                    App.SetLoggedInUserModel();
                    App.NavigateToPage(Constants.Page_WorldsPage);
                    App.SetIsBusy(false);
                }
            }
        }

        #endregion

        #endregion
    }
}
