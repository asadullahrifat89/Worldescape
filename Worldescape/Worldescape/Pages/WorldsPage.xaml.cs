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

        int _pageSize = 12;
        int _pageIndex = 0;
        long _totalPageCount = 0;

        bool _settingWorlds = false;

        readonly MainPage _mainPage;
        readonly HttpServiceHelper _httpServiceHelper;
        readonly WorldHelper _worldHelper;
        readonly PageNumberHelper _pageNumberHelper;

        RangeObservableCollection<string> _pageNumbers = new RangeObservableCollection<string>();

        #endregion

        #region Ctor

        public WorldsPage()
        {
            this.InitializeComponent();

            _httpServiceHelper = App.ServiceProvider.GetService(typeof(HttpServiceHelper)) as HttpServiceHelper;
            _mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;
            _worldHelper = App.ServiceProvider.GetService(typeof(WorldHelper)) as WorldHelper;
            _pageNumberHelper = App.ServiceProvider.GetService(typeof(PageNumberHelper)) as PageNumberHelper;

            LoadWorlds();
        }

        #endregion

        #region Methods

        #region Functionality

        private async Task LoadWorlds()
        {
            var count = await GetWorldsCount();

            if (count > 0)
            {
                await GetWorlds();
            }
        }

        private async Task GetWorlds()
        {
            _settingWorlds = true;

            var response = await _httpServiceHelper.SendGetRequest<GetWorldsQueryResponse>(
                actionUri: Constants.Action_GetWorlds,
                payload: new GetWorldsQueryRequest() { Token = App.Token, PageIndex = _pageIndex, PageSize = _pageSize, SearchString = SearchWorldsTextHolder.Text });

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK || !response.ExternalError.IsNullOrBlank())
            {
                MessageBox.Show(response.ExternalError.ToString());
            }

            var worlds = response.Worlds;

            var _masonryPanel = new MasonryPanelWithProgressiveLoading()
            {
                Margin = new Thickness(20, 0, 20, 0),
                Style = Application.Current.Resources["Panel_Style"] as Style,
                MinHeight = 600
            };

            var size = 220;

            foreach (var world in worlds)
            {
                var img = _worldHelper.GetWorldPicture(world: world, size: size);

                img.Margin = new Thickness(10, 15, 10, 10);

                var stackPanel = new StackPanel() { Margin = new Thickness(10) };
                stackPanel.Children.Add(img);
                stackPanel.Children.Add(new TextBlock()
                {
                    FontSize = 20,
                    FontWeight = FontWeights.SemiBold,
                    TextAlignment = TextAlignment.Center,
                    Text = world.Name,
                    Margin = new Thickness(5),
                });
                stackPanel.Children.Add(new TextBlock()
                {
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    TextAlignment = TextAlignment.Center,
                    Text = "By " + world.Creator.Name,
                    Margin = new Thickness(5),
                });

                var buttonWorld = new Button()
                {
                    Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                    Height = size,
                    Width = size,
                    Margin = new Thickness(5),
                    Tag = world,
                };

                buttonWorld.Click += ButtonWorld_Click;
                buttonWorld.Content = stackPanel;

                _masonryPanel.Children.Add(buttonWorld);
            }

            ContentScrollViewer.Content = _masonryPanel;

            _settingWorlds = false;
        }

        private async Task<long> GetWorldsCount()
        {
            // Get constructs count for this world
            var countResponse = await _httpServiceHelper.SendGetRequest<GetWorldsCountQueryResponse>(
                actionUri: Constants.Action_GetWorldsCount,
                payload: new GetWorldsCountQueryRequest() { Token = App.Token, SearchString = SearchWorldsTextHolder.Text });

            if (countResponse.HttpStatusCode != System.Net.HttpStatusCode.OK || !countResponse.ExternalError.IsNullOrBlank())
            {
                MessageBox.Show(countResponse.ExternalError.ToString());
            }

            _totalPageCount = countResponse.Count < _pageSize ? 1 : (long)Math.Ceiling(countResponse.Count / (decimal)_pageSize);

            FoundWorldsCountHolder.Text = $"Found {countResponse.Count} worlds...";

            PopulatePageNumbers();

            return countResponse.Count;
        }

        private void GeneratePageNumbers()
        {
            _pageNumbers = _pageNumberHelper.GeneratePageNumbers(_totalPageCount, _pageIndex, _pageNumbers);
            PagesHolder.ItemsSource = _pageNumbers;
        }

        private void PopulatePageNumbers()
        {
            _pageNumbers = _pageNumberHelper.PopulatePageNumbers(_totalPageCount, _pageIndex, _pageNumbers);
            PagesHolder.ItemsSource = _pageNumbers;
        }

        #endregion

        #region Button Events

        private async void ButtonSearchWorld_Click(object sender, RoutedEventArgs e)
        {
            await LoadWorlds();
        }

        private void ButtonWorld_Click(object sender, RoutedEventArgs e)
        {
            var world = ((Button)sender).Tag as World;
            var result = MessageBox.Show("Would you like to go to this world?", $"Go to {world.Name}", MessageBoxButton.OKCancel);

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
                _pageIndex--;

                if (_pageIndex < 0)
                {
                    _pageIndex = 0;
                    return;
                }

                await GetWorlds();

                GeneratePageNumbers();
            }
        }

        private async void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (!_settingWorlds)
            {
                _pageIndex++;

                if (_pageIndex > _totalPageCount)
                {
                    _pageIndex = (int)_totalPageCount;
                }

                await GetWorlds();

                GeneratePageNumbers();
            }
        }

        private async void ButtonPageIndex_Click(object sender, RoutedEventArgs e)
        {
            if (!_settingWorlds)
            {
                _pageIndex = Convert.ToInt32(((Button)sender).Content);

                await GetWorlds();

                GeneratePageNumbers();
            }
        }

        private void ButtonCreateWorld_Click(object sender, RoutedEventArgs e)
        {
            WorldCreatorWindow worldCreatorWindow = new WorldCreatorWindow(async (world) =>
            {
                var result = MessageBox.Show("Would you like to teleport to your created world now?", "Teleport", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    App.World = world;
                    _mainPage.NavigateToPage(Constants.Page_InsideWorldPage);
                }
                else
                {
                    await LoadWorlds();
                }
            });
            worldCreatorWindow.Show();
        }

        #endregion

        #region UX Events

        private async void SearchWorldsTextHolder_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                await LoadWorlds();
            }
        }

        #endregion

        #endregion
    }
}
