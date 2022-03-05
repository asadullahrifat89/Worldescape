using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Worldescape.Common;
using Worldescape.Service;

namespace Worldescape
{
    public partial class WorldsPage : Page
    {
        #region Fields

        int _pageSize = 21;
        int _pageIndex = 0;
        double _masonSize = 180;
        long _totalPageCount = 0;
        bool _settingWorlds = false;

        readonly WorldHelper _worldHelper;
        readonly PaginationHelper _paginationHelper;

        readonly WorldRepository _worldRepository;

        RangeObservableCollection<PageNumber> _pageNumbers = new RangeObservableCollection<PageNumber>();

        #endregion

        #region Ctor

        public WorldsPage()
        {
            InitializeComponent();

            _worldHelper = App.ServiceProvider.GetService(typeof(WorldHelper)) as WorldHelper;
            _worldRepository = App.ServiceProvider.GetService(typeof(WorldRepository)) as WorldRepository;
            _paginationHelper = App.ServiceProvider.GetService(typeof(PaginationHelper)) as PaginationHelper;
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

        private void ToggleButton_UsersWorldsOnly_Click(object sender, RoutedEventArgs e)
        {
            TextBox_SearchWorldsText.PlaceholderText = ToggleButton_UsersWorldsOnly.IsChecked.Value ? "Search my worlds..." : "Search all worlds...";
            SearchWorlds();
        }

        private void Button_SearchWorld_Click(object sender, RoutedEventArgs e)
        {
            SearchWorlds();
        }

        private void ButtonWorld_Click(object sender, RoutedEventArgs e)
        {
            var world = ((Button)sender).Tag as World;

            var contentDialogue = new WorldInteractionWindow(world: world, title: $"Teleport to {world.Name}?", result: (result) =>
            {
                if (result)
                {
                    App.SetIsBusy(true, $"Teleporting...");
                    App.World = world;

                    App.NavigateToPage(Constants.Page_InsideWorldPage);

                    //var insideWorldPage = App.ServiceProvider.GetService(typeof(InsideWorldPage)) as InsideWorldPage;
                    //insideWorldPage.SelectCharacterAndConnect();
                }
            });

            contentDialogue.Show();
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

        private void Button_CreateWorld_Click(object sender, RoutedEventArgs e)
        {
            WorldCreationWindow WorldCreationWindow = new WorldCreationWindow((world) =>
            {
                var contentDialogue = new MessageDialogueWindow($"Teleport", "Would you like to teleport to your created world now?", (result) =>
                {
                    if (result)
                    {
                        App.World = world;
                        App.NavigateToPage(Constants.Page_InsideWorldPage);
                    }
                    else
                    {
                        SearchWorlds();
                    }
                });

                contentDialogue.Show();
            });
            WorldCreationWindow.Show();
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

            _totalPageCount = _paginationHelper.GetTotalPageCount(pageSize: _pageSize, dataCount: count);

            TextBox_FoundWorldsCount.Text = $"Found {count} worlds{(TextBox_SearchWorldsText.Text.IsNullOrBlank() ? "" : " matching " + TextBox_SearchWorldsText.Text)}...";
            PopulatePageNumbers(0);

            if (count > 0)
            {
                IEnumerable<World> worlds = await GetWorlds();

                var _masonryPanel = new MasonryPanelWithProgressiveLoading()
                {
                    Margin = new Thickness(100, 0, 100, 0),
                    MinHeight = 600
                };

                foreach (var world in worlds)
                {
                    var buttonWorld = _worldHelper.GenerateWorldButton(
                        world: world,
                        size: _masonSize,
                        imageMargin: new Thickness(10),
                        fontSize: 16);

                    buttonWorld.Click += ButtonWorld_Click;
                    buttonWorld.Margin = new Thickness(10);

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
                creatorId: ToggleButton_UsersWorldsOnly.IsChecked.Value ? App.User.Id : 0);

            if (!response.Success)
            {
                var contentDialogue = new MessageDialogueWindow(title: "Error!", message: response.Error);
                contentDialogue.Show();

                App.SetIsBusy(false);
                return 0;
            }

            return response.Result;
        }

        private async Task<IEnumerable<World>> GetWorlds()
        {
            var response = await _worldRepository.GetWorlds(
                token: App.Token,
                pageIndex: _pageIndex,
                pageSize: _pageSize,
                searchString: TextBox_SearchWorldsText.Text,
                creatorId: ToggleButton_UsersWorldsOnly.IsChecked.Value ? App.User.Id : 0);

            if (!response.Success)
            {
                var contentDialogue = new MessageDialogueWindow(title: "Error!", message: response.Error);
                contentDialogue.Show();

                App.SetIsBusy(false);
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
