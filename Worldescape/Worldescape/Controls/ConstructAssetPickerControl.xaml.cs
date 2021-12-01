using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Common;
using Worldescape.Service;

namespace Worldescape
{
    public partial class ConstructAssetPickerControl : UserControl
    {
        #region Fields

        public event EventHandler<ConstructAsset> AssetSelected;

        double _constructAssetMasonrySize = 130;
        double _constructCategoryMasonrySize = 120;
        //double _masonryPanelHeight = 350;

        int _pageSize = 20;
        int _pageIndex = 0;
        long _totalPageCount = 0;

        bool _settingConstructAssets = false;

        string _constructCategoryNameFilter = string.Empty;

        readonly List<ConstructAsset> ConstructAssets = new List<ConstructAsset>();
        readonly List<ConstructCategory> ConstructCategories = new List<ConstructCategory>();

        readonly UrlHelper _urlHelper;
        readonly PaginationHelper _paginationHelper;

        ConstructCategory _pickedConstructCategory = new ConstructCategory() { Name = "All" };
        RangeObservableCollection<PageNumber> _pageNumbers = new RangeObservableCollection<PageNumber>();

        #endregion

        #region Ctor
        public ConstructAssetPickerControl()
        {
            InitializeComponent();

            _urlHelper = App.ServiceProvider.GetService(typeof(UrlHelper)) as UrlHelper;
            _paginationHelper = App.ServiceProvider.GetService(typeof(PaginationHelper)) as PaginationHelper;

            ConstructAssets = JsonSerializer.Deserialize<ConstructAsset[]>(Service.Properties.Resources.ConstructAssets).ToList();
            ConstructCategories = ConstructAssets.Select(x => x.Category).Distinct().Select(z => new ConstructCategory() { ImageUrl = @$"ms-appx:///Images/World_Objects/{z}.png", Name = z }).ToList();

            ShowConstructCategories();
        }

        #endregion

        #region Methods

        #region Functionality

        private IEnumerable<ConstructAsset> GetFilteredConstructAssets()
        {
            var filteredData = new List<ConstructAsset>();

            filteredData.AddRange(ConstructAssets.Where(x =>
            (!_constructCategoryNameFilter.IsNullOrBlank() ? x.Category == _constructCategoryNameFilter : !x.Category.IsNullOrBlank())
            && (!TextBoxSearchConstructAssets.Text.IsNullOrBlank() ? x.Name.ToLowerInvariant().Contains(TextBoxSearchConstructAssets.Text.ToLowerInvariant()) : !x.Name.IsNullOrBlank())));

            return filteredData;
        }

        private void SearchConstructAssets()
        {
            _pageIndex = 0;
            ShowConstructAssetsCount();
            GetConstructAssets();
        }

        private void ShowConstructCategories()
        {
            var pagedData = ConstructCategories;

            var _masonryPanel = new MasonryPanelWithProgressiveLoading()
            {
                Margin = new Thickness(5),
                Style = Application.Current.Resources["Panel_Style"] as Style,
                // Height = _masonryPanelHeight
            };

            var allConstructCategory = new ConstructCategory() { Name = "All" };

            // Add an All button first
            AddConstructCategoryMasonry(_masonryPanel, allConstructCategory);

            //Add all teh categories
            foreach (var constructCategory in pagedData)
            {
                AddConstructCategoryMasonry(_masonryPanel, constructCategory);
            }

            ContentScrollViewer.Content = _masonryPanel;

            _pageNumbers.Clear();
            PagesHolder.ItemsSource = _pageNumbers;
        }

        private void AddConstructCategoryMasonry(MasonryPanelWithProgressiveLoading _masonryPanel, ConstructCategory constructCategory)
        {
            var buttonName = constructCategory.Name.Replace(" ", "");

            var button_Category = new Button()
            {
                Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                Width = _constructCategoryMasonrySize,
                Height = _constructCategoryMasonrySize,
                Margin = new Thickness(3),
                Tag = constructCategory,
                FontSize = 16,
                Content = constructCategory.Name,
                Name = buttonName,
            };

            button_Category.Click += ButtonConstructCategory_Click;
            _masonryPanel.Children.Add(button_Category);
        }

        private void GetConstructAssets()
        {
            _settingConstructAssets = true;

            var filteredData = GetFilteredConstructAssets();

            var pagedData = filteredData.Skip(_pageIndex * _pageSize).Take(_pageSize);

            var _masonryPanel = new MasonryPanelWithProgressiveLoading()
            {
                Margin = new Thickness(5),
                Style = Application.Current.Resources["Panel_Style"] as Style,
                //Height = _masonryPanelHeight
            };

            foreach (var item in pagedData)
            {
                AddConstructAssetMasonry(_masonryPanel, item);
            }

            ContentScrollViewer.Content = _masonryPanel;

            _settingConstructAssets = false;
        }

        private void AddConstructAssetMasonry(MasonryPanelWithProgressiveLoading _masonryPanel, ConstructAsset constructAsset)
        {
            var buttonName = constructAsset.Name.Replace(" ", "");

            var uri = _urlHelper.BuildAssetUrl(App.Token, constructAsset.ImageUrl);

            constructAsset.ImageUrl = uri;

            var bitmap = new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));

            var img = new Image()
            {
                Source = bitmap,
                Stretch = Stretch.Uniform,
                Height = _constructAssetMasonrySize - 10,
                Width = _constructAssetMasonrySize - 10,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            var txt = new TextBlock()
            {
                Text = constructAsset.Name,
                FontWeight = FontWeights.SemiBold,
                Foreground = Application.Current.Resources["MaterialDesign_DefaultAccentColor"] as SolidColorBrush,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14
            };

            StackPanel content = new StackPanel();
            content.Children.Add(img);
            content.Children.Add(txt);

            var buttonConstructAsset = new Button()
            {
                Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                Height = _constructAssetMasonrySize,
                Width = _constructAssetMasonrySize,
                Margin = new Thickness(3),
                Tag = constructAsset,
                Content = content,
                Name = buttonName,
            };
            buttonConstructAsset.Click += ButtonConstructAsset_Click;
            _masonryPanel.Children.Add(buttonConstructAsset);
        }

        private void ShowConstructAssetsCount()
        {
            var filteredData = GetFilteredConstructAssets();

            _totalPageCount = _paginationHelper.GetTotalPageCount(_pageSize, filteredData.Count());

            FoundConstructAssetsCountHolder.Text = $"Found { filteredData?.Count().ToString() } constructs in {_pickedConstructCategory.Name.ToLowerInvariant()}{(TextBoxSearchConstructAssets.Text.IsNullOrBlank() ? "" : " matching " + TextBoxSearchConstructAssets.Text)}...";
            PopulatePageNumbers(0);
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

        #region Button Events

        private void ButtonSearchConstructAssets_Click(object sender, RoutedEventArgs e)
        {
            SearchConstructAssets();
        }

        private void ButtonPreview_Click(object sender, RoutedEventArgs e)
        {
            if (!_settingConstructAssets)
            {
                _pageIndex = _paginationHelper.GetPreviousPageNumber(_totalPageCount, _pageIndex);

                GetConstructAssets();
                GeneratePageNumbers();
            }
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (!_settingConstructAssets)
            {
                _pageIndex = _paginationHelper.GetNextPageNumber(_totalPageCount, _pageIndex);

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
            _pickedConstructCategory = ((Button)sender).Tag as ConstructCategory;
            var categoryName = _pickedConstructCategory.Name;

            _constructCategoryNameFilter = !categoryName.Equals("All") ? categoryName : null;

            _pageIndex = 0;
            ShowConstructAssetsCount();
            GetConstructAssets();
        }

        private void ButtonConstructAsset_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var constructAsset = button.Tag as ConstructAsset;

            AssetSelected?.Invoke(this, constructAsset);
        }

        #endregion

        #region UX Events

        private void TextBoxSearchConstructAssets_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SearchConstructAssets();
            }
        }

        #endregion

        #endregion
    }
}
