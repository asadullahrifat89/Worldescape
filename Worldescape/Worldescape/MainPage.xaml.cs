using System;
using Windows.UI.Xaml.Controls;

namespace Worldescape
{
    public partial class MainPage : Page
    {
        #region Fields



        #endregion

        public MainPage()
        {
            this.InitializeComponent();
        }

        #region Methods

        public void NavigateToPage(string targetUri)
        {
            // Navigate to the target page:
            Uri uri = new Uri(targetUri, UriKind.Relative);
            PageContainer.Source = uri;
        }

        #endregion
    }
}
