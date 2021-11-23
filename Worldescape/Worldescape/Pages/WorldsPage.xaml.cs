using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Worldescape.Service;

namespace Worldescape
{
    public partial class WorldsPage : Page
    {
        readonly MainPage _mainPage;
        readonly HttpServiceHelper _httpServiceHelper;

        #region Ctor

        public WorldsPage()
        {
            this.InitializeComponent();

            _httpServiceHelper = App.ServiceProvider.GetService(typeof(HttpServiceHelper)) as HttpServiceHelper;
            _mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;
        }

        #endregion

        #region Methods

        #region Functionality



        #endregion

        #region Button Events

        private void ButtonPreview_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonCreateWorld_Click(object sender, RoutedEventArgs e)
        {

        } 

        #endregion

        #endregion
    }
}
