using System.Windows;
using Worldescape.Views;

namespace Worldescape
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SignInPage _signInPage;
        public MainWindow(SignInPage signInPage)
        {
            InitializeComponent();
            _signInPage = signInPage;

            this.Frame_ContentFrame.Content = _signInPage;
        }
    }
}
