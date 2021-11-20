using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Worldescape.Data;
using Worldescape.Service;

namespace Worldescape
{
    public partial class SignupPage : Page
    {
        private readonly IHttpService _httpService;
        public SignupPage()
        {
            InitializeComponent();
            _httpService = App.ServiceProvider.GetService(typeof(IHttpService)) as IHttpService;
        }

        private void Button_Signup_Click(object sender, RoutedEventArgs e)
        {
            var command = new AddUserCommandRequest
            {
                Email = TextBox_Email.Text,
                Password = PasswordBox_Pasword.Password,
                DateOfBirth = DatePicker_DateOfBirth.SelectedDate.Value,
                Gender = RadioButton_Male.IsChecked.Value ? Gender.Male : RadioButton_Female.IsChecked.Value ? Gender.Female : Gender.Other,
                FirstName = TextBox_FirstName.Text,
                LastName = TextBox_LastName.Text,
                Name = TextBox_FirstName.Text + " " + TextBox_LastName.Text,
            };

            //TODO: call server and signup a user
        }

        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            var mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;
            mainPage.NavigateToPage("/LoginPage");
        }
    }
}
