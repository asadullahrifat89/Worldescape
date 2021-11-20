using System.Net.Http;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Worldescape.Data;

namespace Worldescape
{
    public partial class LoginPage : Page
    {
        #region Fields

        private readonly HttpCommunicationService _httpCommunicationService;

        #endregion

        public LoginPage()
        {
            InitializeComponent();
            LoginModelHolder.DataContext = LoginModel;
            _httpCommunicationService = App.ServiceProvider.GetService(typeof(HttpCommunicationService)) as HttpCommunicationService;
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

            var command = new GetApiTokenQueryRequest
            {
                Password = LoginModel.Password,
                Email = LoginModel.Email,
            };

            var response = await _httpCommunicationService.SendToHttpAsync<ServiceResponse>(
               httpMethod: HttpMethod.Post,
               baseUri: _httpCommunicationService.GetWebServiceUrl(),
               actionUri: Constants.Action_AddUser,
               payload: command);

            //    var mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;

            //    App.User = new User() { Id = UidGenerator.New(), Name = TextBox_Email.Text };
            //    App.InWorld = new InWorld() { Id = 786, Name = "Test World" };

            //    var insideWorldPage = App.ServiceProvider.GetService(typeof(InsideWorldPage)) as InsideWorldPage;            
            //    mainPage.NavigateToPage(insideWorldPage);

            //    //mainPage.NavigateToPage("/InsideWorldPage");
        }

        private void Button_SignUp_Click(object sender, RoutedEventArgs e)
        {
            var mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;
            mainPage.NavigateToPage("/SignupPage");
        }
    }
}
