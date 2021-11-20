using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Worldescape.Data;

namespace Worldescape
{
    public partial class LoginPage : Page
    {

        public LoginPage()
        {
            InitializeComponent();
        }

        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            var mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;

            App.User = new User() { Id = UidGenerator.New(), Name = TextBox_Email.Text };
            App.InWorld = new InWorld() { Id = 786, Name = "Test World" };

            var insideWorldPage = App.ServiceProvider.GetService(typeof(InsideWorldPage)) as InsideWorldPage;            
            mainPage.NavigateToPage(insideWorldPage);

            //mainPage.NavigateToPage("/InsideWorldPage");
        }

        private void Button_SignUp_Click(object sender, RoutedEventArgs e)
        {
            var mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;
            mainPage.NavigateToPage("/SignupPage");
        }
    }
}
