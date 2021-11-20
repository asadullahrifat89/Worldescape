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
        private readonly HttpCommunicationService _httpCommunicationService;

        public SignUpModel SignUpModel { get; set; } = new SignUpModel();

        public SignupPage()
        {
            InitializeComponent();
            SignUpModelHolder.DataContext = SignUpModel;
            _httpCommunicationService = App.ServiceProvider.GetService(typeof(HttpCommunicationService)) as HttpCommunicationService;
        }

        private void Control_BindingValidationError(object sender, ValidationErrorEventArgs e)
        {
            CheckIfModelValid();
        }

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

        private async void Button_Signup_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIfModelValid())
            {
                Control_BindingValidationError(sender, null);
                return;
            }

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

            var response = await _httpCommunicationService.SendToHttpAsync<ServiceResponse>(
                httpMethod: HttpMethod.Post,
                baseUri: _httpCommunicationService.GetWebServiceUrl(),
                actionUri: Constants.Action_AddUser,
                payload: command);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                if (!response.ExternalError.IsNullOrBlank())
                {
                    MessageBox.Show(response.ExternalError.ToString());
                }
            }
        }

        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            var mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;
            mainPage.NavigateToPage("/LoginPage");
        }

        private void Control_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckIfModelValid();
        }
    }
}
