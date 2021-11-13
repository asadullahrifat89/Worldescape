using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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

        }

        private void Button_SignUp_Click(object sender, RoutedEventArgs e)
        {
            var mainPage = App._serviceProvider.GetService(typeof(MainPage)) as MainPage;
            mainPage.NavigateToPage("/SignupPage");
        }
    }
}
