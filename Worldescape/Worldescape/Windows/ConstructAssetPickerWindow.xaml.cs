using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Data;
using Worldescape.Service;

namespace Worldescape
{
    public partial class ConstructAssetPickerWindow : ChildWindow
    {
        #region Fields

        int _pageSize = 24;
        int _pageIndex = 0;
        int _totalPageCount = 0;

        bool _settingConstructAssets = false;

        string _pickedConstructCategory = string.Empty;

        List<ConstructAsset> _constructAssets = new List<ConstructAsset>();
        List<ConstructCategory> _constructCategories = new List<ConstructCategory>();

        RangeObservableCollection<string> _pageNumbers = new RangeObservableCollection<string>();

        Action<ConstructAsset> _assetSelected;

        readonly AssetUrlHelper _assetUriHelper;
        readonly PageNumberHelper _pageNumberHelper;

        #endregion

        #region Ctor
        public ConstructAssetPickerWindow(
           List<ConstructAsset> constructAssets,
           List<ConstructCategory> constructCategories,
           Action<ConstructAsset> assetSelected)
        {
            InitializeComponent();

            _assetUriHelper = App.ServiceProvider.GetService(typeof(AssetUrlHelper)) as AssetUrlHelper;
            _pageNumberHelper = App.ServiceProvider.GetService(typeof(PageNumberHelper)) as PageNumberHelper;

            _constructAssets = constructAssets;
            _constructCategories = constructCategories;

            _assetSelected = assetSelected;

            ShowConstructCategories();

        }
        #endregion

        #region Methods

        #region Functionality

        private void ShowConstructCategories()
        {
            Title = "Select a Category";

            var pagedData = _constructCategories;

            var _masonryPanel = new MasonryPanelWithProgressiveLoading()
            {
                Margin = new Thickness(5),
                Style = Application.Current.Resources["Panel_Style"] as Style,
                Height = 500
            };

            foreach (var item in pagedData)
            {
                var buttonConstructAsset = new Button()
                {
                    Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                    Width = 140,
                    Height = 120,
                    Margin = new Thickness(3),
                    Tag = item,
                };

                buttonConstructAsset.Click += ButtonConstructCategory_Click;
                buttonConstructAsset.Content = item.Name;

                _masonryPanel.Children.Add(buttonConstructAsset);
            }

            ContentScrollViewer.Content = _masonryPanel;

            _pageNumbers.Clear();
            PagesHolder.ItemsSource = _pageNumbers;
        }

        private void GetConstructAssets()
        {
            Title = "Select a Construct";

            _settingConstructAssets = true;

            var filteredData = string.IsNullOrEmpty(_pickedConstructCategory) ? _constructAssets : _constructAssets.Where(x => x.Category == _pickedConstructCategory);

            var pagedData = filteredData.Skip(_pageIndex * _pageSize).Take(_pageSize);

            var _masonryPanel = new MasonryPanelWithProgressiveLoading()
            {
                Margin = new Thickness(5),
                Style = Application.Current.Resources["Panel_Style"] as Style,
                Height = 500
            };

            foreach (var item in pagedData)
            {
                var uri = _assetUriHelper.BuildAssetUrl(App.Token, item.ImageUrl);

                item.ImageUrl = uri;

                var bitmap = new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));

                var img = new Image()
                {
                    Source = bitmap,
                    Stretch = Stretch.Uniform,
                    Height = 100,
                    Width = 100
                };

                var buttonConstructAsset = new Button()
                {
                    Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                    Height = 100,
                    Width = 100,
                    Margin = new Thickness(3),
                    Tag = item,
                };

                buttonConstructAsset.Click += ButtonConstructAsset_Click;
                buttonConstructAsset.Content = img;

                _masonryPanel.Children.Add(buttonConstructAsset);
            }

            ContentScrollViewer.Content = _masonryPanel;

            _settingConstructAssets = false;
        }

        private void ShowConstructAssetsCount(IEnumerable<ConstructAsset> filteredData)
        {
            _totalPageCount = filteredData.Count() / _pageSize;
            FoundConstructAssetsCountHolder.Text = $"Found { filteredData?.Count().ToString() } constructs...";
            PopulatePageNumbers(0);
        }

        private void GeneratePageNumbers()
        {
            _pageNumbers = _pageNumberHelper.GeneratePageNumbers(_totalPageCount, _pageIndex, _pageNumbers);
            PagesHolder.ItemsSource = _pageNumbers;
        }

        private void PopulatePageNumbers(int? pageIndex = null)
        {
            _pageNumbers = _pageNumberHelper.PopulatePageNumbers(_totalPageCount, pageIndex ?? _pageIndex, _pageNumbers);
            PagesHolder.ItemsSource = _pageNumbers;
        }

        #endregion

        #region Button Events

        private void ButtonPreview_Click(object sender, RoutedEventArgs e)
        {
            if (!_settingConstructAssets)
            {
                _pageIndex = _pageNumberHelper.GetPreviousPageNumber(_totalPageCount, _pageIndex);

                GetConstructAssets();
                GeneratePageNumbers();
            }
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (!_settingConstructAssets)
            {
                _pageIndex = _pageNumberHelper.GetNextPageNumber(_totalPageCount, _pageIndex);

                GetConstructAssets();
                GeneratePageNumbers();
            }
        }

        private void ButtonPageIndex_Click(object sender, RoutedEventArgs e)
        {
            if (!_settingConstructAssets)
            {
                _pageIndex = Convert.ToInt32(((Button)sender).Content);

                GetConstructAssets();
                GeneratePageNumbers();
            }
        }

        private void ButtonShowCategories_Click(object sender, RoutedEventArgs e)
        {
            ShowConstructCategories();
        }

        private void ButtonConstructCategory_Click(object sender, RoutedEventArgs e)
        {
            _pageIndex = 0;
            _pickedConstructCategory = (((Button)sender).Tag as ConstructCategory).Name;

            var filteredData = string.IsNullOrEmpty(_pickedConstructCategory) ? _constructAssets : _constructAssets.Where(x => x.Category == _pickedConstructCategory);

            ShowConstructAssetsCount(filteredData);

            GetConstructAssets();
        }

        private void ButtonConstructAsset_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var constructAsset = button.Tag as ConstructAsset;

            _assetSelected?.Invoke(constructAsset);

            Close();
        }

        #endregion

        #endregion      
    }
}

