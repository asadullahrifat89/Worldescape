using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Text;
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
        long _totalPageCount = 0;

        bool _settingConstructAssets = false;

        string _constructCategoryNameFilter = string.Empty;
        ConstructCategory _pickedConstructCategory = new ConstructCategory() { Name = "All" };

        List<ConstructAsset> _constructAssets = new List<ConstructAsset>();
        List<ConstructCategory> _constructCategories = new List<ConstructCategory>();

        RangeObservableCollection<string> _pageNumbers = new RangeObservableCollection<string>();

        Action<ConstructAsset> _assetSelected;

        readonly UrlHelper _urlHelper;
        readonly PageNumberHelper _pageNumberHelper;

        #endregion

        #region Ctor
        public ConstructAssetPickerWindow(
           List<ConstructAsset> constructAssets,
           List<ConstructCategory> constructCategories,
           Action<ConstructAsset> assetSelected)
        {
            InitializeComponent();

            _urlHelper = App.ServiceProvider.GetService(typeof(UrlHelper)) as UrlHelper;
            _pageNumberHelper = App.ServiceProvider.GetService(typeof(PageNumberHelper)) as PageNumberHelper;

            _constructAssets = constructAssets;
            _constructCategories = constructCategories;

            _assetSelected = assetSelected;

            ShowConstructCategories();

        }
        #endregion

        #region Methods

        #region Functionality

        private IEnumerable<ConstructAsset> GetFilteredConstructAssets()
        {
            var filteredData = new List<ConstructAsset>();

            filteredData.AddRange(_constructAssets.Where(x =>
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
            Title = "Select a Category";

            var pagedData = _constructCategories;

            var _masonryPanel = new MasonryPanelWithProgressiveLoading()
            {
                Margin = new Thickness(5),
                Style = Application.Current.Resources["Panel_Style"] as Style,
                Height = 500
            };

            // Add an All button first
            var button_All = new Button()
            {
                Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                Width = 150,
                Height = 120,
                Margin = new Thickness(3),
                Tag = new ConstructCategory() { Name = "All" },
                FontSize = 18
            };

            button_All.Click += ButtonConstructCategory_Click;
            button_All.Content = "All";

            _masonryPanel.Children.Add(button_All);

            //Add all teh categories
            foreach (var item in pagedData)
            {
                var button_Category = new Button()
                {
                    Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                    Width = 150,
                    Height = 120,
                    Margin = new Thickness(3),
                    Tag = item,
                    FontSize = 18
                };

                button_Category.Click += ButtonConstructCategory_Click;
                button_Category.Content = item.Name;

                _masonryPanel.Children.Add(button_Category);
            }

            ContentScrollViewer.Content = _masonryPanel;

            _pageNumbers.Clear();
            PagesHolder.ItemsSource = _pageNumbers;
        }

        private void GetConstructAssets()
        {
            Title = "Select a Construct";

            _settingConstructAssets = true;

            var filteredData = GetFilteredConstructAssets();

            var pagedData = filteredData.Skip(_pageIndex * _pageSize).Take(_pageSize);

            var _masonryPanel = new MasonryPanelWithProgressiveLoading()
            {
                Margin = new Thickness(5),
                Style = Application.Current.Resources["Panel_Style"] as Style,
                Height = 500
            };

            foreach (var item in pagedData)
            {
                var uri = _urlHelper.BuildAssetUrl(App.Token, item.ImageUrl);

                item.ImageUrl = uri;

                var bitmap = new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));

                var img = new Image()
                {
                    Source = bitmap,
                    Stretch = Stretch.Uniform,
                    Height = 160,
                    Width = 160,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };

                var txt = new TextBlock()
                {
                    Text = item.Name,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Application.Current.Resources["MaterialDesign_DefaultAccentColor"] as SolidColorBrush,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 14
                };

                var buttonConstructAsset = new Button()
                {
                    Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                    Height = 170,
                    Width = 170,
                    Margin = new Thickness(3),
                    Tag = item,
                };

                StackPanel stackPanel = new StackPanel();
                stackPanel.Children.Add(img);
                stackPanel.Children.Add(txt);

                buttonConstructAsset.Click += ButtonConstructAsset_Click;
                buttonConstructAsset.Content = stackPanel;

                _masonryPanel.Children.Add(buttonConstructAsset);
            }

            ContentScrollViewer.Content = _masonryPanel;

            _settingConstructAssets = false;
        }

        private void ShowConstructAssetsCount()
        {
            var filteredData = GetFilteredConstructAssets();

            _totalPageCount = _pageNumberHelper.GetTotalPageCount(_pageSize, filteredData.Count());

            FoundConstructAssetsCountHolder.Text = $"Found { filteredData?.Count().ToString() } constructs in {_pickedConstructCategory.Name.ToLowerInvariant()}{(TextBoxSearchConstructAssets.Text.IsNullOrBlank() ? "" : " matching " + TextBoxSearchConstructAssets.Text)}...";
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

        private void ButtonSearchConstructAssets_Click(object sender, RoutedEventArgs e)
        {
            SearchConstructAssets();
        }

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

            _assetSelected?.Invoke(constructAsset);

            Close();
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

