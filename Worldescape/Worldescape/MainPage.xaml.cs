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
            InitializeComponent();
        }

        #region Methods

        public void NavigateToPage(Page page)
        {
            // Set target to target page:            
            PageContainer.Content = page;
        }

        public void NavigateToPage(string targetUri)
        {
            // Navigate to the target page:
            Uri uri = new Uri(targetUri, UriKind.Relative);
            PageContainer.Source = uri;
        }

        #endregion
    }
}
