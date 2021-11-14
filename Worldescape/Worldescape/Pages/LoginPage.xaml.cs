using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Worldescape.Pages
{
    public partial class LoginPage : Page
    {

        public LoginPage()
        {
            this.InitializeComponent();
        }

        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            var mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;

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
