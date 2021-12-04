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

        readonly HttpServiceHelper _httpServiceHelper;
        readonly UserRepository _userRepository;
        readonly MainPage _mainPage;

        #endregion

        #region Ctor

        public LoginPage(
            //HttpServiceHelper httpServiceHelper,
            //MainPage mainPage
            )
        {
            InitializeComponent();

            //_httpServiceHelper = httpServiceHelper;
            //_mainPage = mainPage;

            LoginModelHolder.DataContext = LoginModel;

            _httpServiceHelper = App.ServiceProvider.GetService(typeof(HttpServiceHelper)) as HttpServiceHelper;
            _userRepository = App.ServiceProvider.GetService(typeof(UserRepository)) as UserRepository;
            _mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;

        }

        #endregion

        #region Properties

        public LoginModel LoginModel { get; set; } = new LoginModel();

        #endregion

        #region Methods

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

                var loginResponse = await _userRepository.GetUser(
                    token: token,
                    email: LoginModel.Email,
                    password: LoginModel.Password);

                if (!loginResponse.Success)
                {
                    var contentDialogue = new ContentDialogueWindow(title: "Error!", message: loginResponse.Error);
                    contentDialogue.Show();

                    _mainPage.SetIsBusy(false);
                    return;
                }
                else
                {
                    App.User = loginResponse.Result as User;

                    _mainPage.SetLoggedInUserModel();
                    _mainPage.NavigateToPage(Constants.Page_WorldsPage);
                    _mainPage.SetIsBusy(false);
                }
            }
        }

        #endregion

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
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                await Login();
            }
        }

        #endregion

        #endregion       
    }
}
