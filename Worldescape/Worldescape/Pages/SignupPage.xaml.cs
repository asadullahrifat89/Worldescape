using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Worldescape.Data;

namespace Worldescape
{
    public partial class SignupPage : Page
    {
        public SignupPage()
        {
            InitializeComponent();
        }

        private void Button_Signup_Click(object sender, RoutedEventArgs e)
        {
            User user = new User() 
            {
            
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
