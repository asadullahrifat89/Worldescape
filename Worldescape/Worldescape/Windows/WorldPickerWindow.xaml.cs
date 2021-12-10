﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Worldescape.Common;
using Worldescape.Service;

namespace Worldescape
{
    public partial class WorldPickerWindow : ChildWindow
    {
        #region Fields

        int _pageSize = 18;
        int _pageIndex = 0;
        long _totalPageCount = 0;

        bool _settingWorlds = false;

        readonly WorldHelper _worldHelper;
        readonly PaginationHelper _paginationHelper;

        readonly WorldRepository _worldRepository;

        RangeObservableCollection<PageNumber> _pageNumbers = new RangeObservableCollection<PageNumber>();

        public event EventHandler<World> WorldSelected;

        #endregion

        #region Ctor

        public WorldPickerWindow()
        {
            InitializeComponent();

            _worldHelper = App.ServiceProvider.GetService(typeof(WorldHelper)) as WorldHelper;
            _paginationHelper = App.ServiceProvider.GetService(typeof(PaginationHelper)) as PaginationHelper;
            _worldRepository = App.ServiceProvider.GetService(typeof(WorldRepository)) as WorldRepository;

            SearchWorlds();
        }

        #endregion

        #region Methods

        #region Page Events

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SearchWorlds();
        }

        #endregion

        #region Button Events

        private void Button_SearchWorld_Click(object sender, RoutedEventArgs e)
        {
            SearchWorlds();
        }

        private void ButtonWorld_Click(object sender, RoutedEventArgs e)
        {
            var world = ((Button)sender).Tag as World;            
            WorldSelected?.Invoke(this, world);
            this.DialogResult = true;
        }

        private async void ButtonPreview_Click(object sender, RoutedEventArgs e)
        {
            if (!_settingWorlds)
            {
                _pageIndex = _paginationHelper.GetPreviousPageNumber(_pageIndex);
                await FetchWorlds();
                GeneratePageNumbers();
            }
        }

        private async void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (!_settingWorlds)
            {
                _pageIndex = _paginationHelper.GetNextPageNumber(_totalPageCount, _pageIndex);
                await FetchWorlds();
                GeneratePageNumbers();
            }
        }

        private async void ButtonPageIndex_Click(object sender, RoutedEventArgs e)
        {
            if (!_settingWorlds)
            {
                _pageIndex = Convert.ToInt32(((Button)sender).Content);
                await FetchWorlds();
                GeneratePageNumbers();
            }
        }

        #endregion

        #region UX Events

        private void TextBox_SearchWorldsText_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SearchWorlds();
            }
        }

        #endregion

        #region Functionality

        private async void SearchWorlds()
        {
            await FetchWorlds();
        }

        private async Task FetchWorlds()
        {
            _settingWorlds = true;

            var count = await GetWorldsCount();

            _totalPageCount = _paginationHelper.GetTotalPageCount(_pageSize, count);

            TextBox_FoundWorldsCount.Text = $"Found {count} worlds{(TextBox_SearchWorldsText.Text.IsNullOrBlank() ? "" : " matching " + TextBox_SearchWorldsText.Text)}...";
            PopulatePageNumbers(0);

            if (count > 0)
            {
                IEnumerable<World> worlds = await GetWorlds();

                var _masonryPanel = new MasonryPanelWithProgressiveLoading()
                {                    
                    Style = Application.Current.Resources["Panel_Style"] as Style,
                    Height = 500
                };

                var size = 100;

                foreach (var world in worlds)
                {
                    var img = _worldHelper.GetWorldPicture(world: world, size: size);

                    img.Margin = new Thickness(5, 10, 5, 10);

                    var stackPanel = new StackPanel() { Margin = new Thickness(10) };
                    stackPanel.Children.Add(img);
                    stackPanel.Children.Add(new TextBlock()
                    {
                        FontSize = 14,
                        FontWeight = FontWeights.SemiBold,
                        TextAlignment = TextAlignment.Center,
                        Text = world.Name,
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
            }

            _settingWorlds = false;
        }

        private async Task<long> GetWorldsCount()
        {
            var response = await _worldRepository.GetWorldsCount(
                token: App.Token,
                searchString: TextBox_SearchWorldsText.Text,
                creatorId: App.User.Id);

            if (!response.Success)
            {
                var contentDialogue = new ContentDialogueWindow(title: "Error!", message: response.Error);
                contentDialogue.Show();
                return 0;
            }

            return (long)response.Result;
        }

        private async Task<IEnumerable<World>> GetWorlds()
        {
            var response = await _worldRepository.GetWorlds(
                                   token: App.Token,
                                   pageIndex: _pageIndex,
                                   pageSize: _pageSize,
                                   searchString: TextBox_SearchWorldsText.Text,
                                   creatorId: App.User.Id);

            if (!response.Success)
            {
                var contentDialogue = new ContentDialogueWindow(title: "Error!", message: response.Error);
                contentDialogue.Show();

                return System.Linq.Enumerable.Empty<World>();
            }

            var worlds = response.Result as IEnumerable<World>;
            return worlds;
        }

        private void GeneratePageNumbers()
        {
            _pageNumbers = _paginationHelper.GeneratePageNumbers(_totalPageCount, _pageIndex, _pageNumbers);
            PagesHolder.ItemsSource = _pageNumbers;
        }

        private void PopulatePageNumbers(int? pageIndex = null)
        {
            _pageNumbers = _paginationHelper.PopulatePageNumbers(_totalPageCount, pageIndex ?? _pageIndex, _pageNumbers);
            PagesHolder.ItemsSource = _pageNumbers;
        }

        #endregion

        #endregion        
    }
}

