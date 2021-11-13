using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Effects;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

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
            try
            {
                // Navigate to the target page:
                Uri uri = new Uri(targetUri, UriKind.Relative);
                PageContainer.Source = uri;
            }
            catch (Exception ex)
            {

                throw;
            }           
        }

        #endregion
    }
}
