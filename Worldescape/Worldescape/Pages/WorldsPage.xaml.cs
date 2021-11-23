﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Worldescape.Data;
using Worldescape.Service;

namespace Worldescape
{
    public partial class WorldsPage : Page
    {
        #region Fields

        int pageSize = 24;
        int pageIndex = 0;
        long totalPageCount = 0;

        bool _settingWorlds = false;

        readonly MainPage _mainPage;
        readonly HttpServiceHelper _httpServiceHelper;

        #endregion

        #region Ctor

        public WorldsPage()
        {
            this.InitializeComponent();

            _httpServiceHelper = App.ServiceProvider.GetService(typeof(HttpServiceHelper)) as HttpServiceHelper;
            _mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;

            ShowWorlds();
        }

        #endregion

        #region Methods

        #region Functionality

        private async Task ShowWorlds()
        {
            var countResponse = await GetWorldsCount();

            if (countResponse.Count > 0)
            {
                await GetWorlds();
            }
        }

        private async Task GetWorlds()
        {
            _settingWorlds = true;

            var response = await _httpServiceHelper.SendGetRequest<GetWorldsQueryResponse>(
                actionUri: Constants.Action_GetWorlds,
                payload: new GetWorldsQueryRequest() { Token = App.Token, PageIndex = pageIndex, PageSize = pageSize, SearchString = SearchWorldsTextHolder.Text });

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK || !response.ExternalError.IsNullOrBlank())
            {
                MessageBox.Show(response.ExternalError.ToString());
            }

            var worlds = response.Worlds;

            WorldsHolder.Children.Clear();

            foreach (var world in worlds)
            {
                var uri = world.ImageUrl;

                //world.ImageUrl = uri;

                var bitmap = new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));

                var img = new Image()
                {
                    Source = bitmap,
                    Stretch = Stretch.Uniform,
                    Height = 200,
                    Width = 300
                };

                var buttonWorld = new Button()
                {
                    Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                    Height = 200,
                    Width = 300,
                    Margin = new Thickness(5),
                    ClipToBounds = true,
                    Tag = world,
                };

                StackPanel stackPanel = new StackPanel();
                stackPanel.Children.Add(img);
                stackPanel.Children.Add(new TextBlock() { FontSize = 16, FontWeight = FontWeights.SemiBold, Text = world.Name });

                buttonWorld.Click += ButtonWorld_Click;
                buttonWorld.Content = stackPanel;

                WorldsHolder.Children.Add(buttonWorld);
            }
        }

        private async Task<GetWorldsCountQueryResponse> GetWorldsCount()
        {


            // Get constructs count for this world
            var countResponse = await _httpServiceHelper.SendGetRequest<GetWorldsCountQueryResponse>(
             actionUri: Constants.Action_GetWorldsCount,
             payload: new GetWorldsCountQueryRequest() { Token = App.Token, SearchString = SearchWorldsTextHolder.Text });

            if (countResponse.HttpStatusCode != System.Net.HttpStatusCode.OK || !countResponse.ExternalError.IsNullOrBlank())
            {
                MessageBox.Show(countResponse.ExternalError.ToString());
            }

            totalPageCount = countResponse.Count < pageSize ? 1 : countResponse.Count / pageSize;

            FoundWorldsCountHolder.Text = $"Found {countResponse.Count} worlds...";

            // TODO: create page numbers below

            return countResponse;
        }

        #endregion

        #region Button Events

        private void ButtonWorld_Click(object sender, RoutedEventArgs e)
        {
            var world = ((Button)sender).Tag as World;
            var result = MessageBox.Show("Would you like to teleport to this world?", $"Join {world.Name}", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                App.World = world;
                _mainPage.NavigateToPage(Constants.Page_InsideWorldPage);
            }
        }

        private async void ButtonPreview_Click(object sender, RoutedEventArgs e)
        {
            if (!_settingWorlds)
            {
                pageIndex--;

                if (pageIndex < 0)
                {
                    pageIndex = 0;
                    return;
                }

                await GetWorlds();
            }
        }

        private async void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (!_settingWorlds)
            {
                pageIndex++;

                if (pageIndex > totalPageCount)
                {
                    pageIndex = (int)totalPageCount;
                }

                await GetWorlds();
            }
        }

        private void ButtonCreateWorld_Click(object sender, RoutedEventArgs e)
        {
            WorldCreatorWindow worldCreatorWindow = new WorldCreatorWindow((world) =>
            {
                App.World = world;
                _mainPage.NavigateToPage(Constants.Page_InsideWorldPage);
            });
            worldCreatorWindow.Show();
        }

        #endregion

        #endregion
    }
}
