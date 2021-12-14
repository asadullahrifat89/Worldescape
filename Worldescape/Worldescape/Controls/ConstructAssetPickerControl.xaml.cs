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
        double _constructCategoryMasonrySize = 130;        

        int _pageSize = 20;
        int _pageIndex = 0;
        long _totalPageCount = 0;

        bool _settingConstructAssets = false;
        public bool FirstHit = false;

        string _constructCategoryFilter = string.Empty;
        string _constructSubCategoryFilter = string.Empty;

        readonly List<ConstructAsset> ConstructAssets = new List<ConstructAsset>();
        readonly List<ConstructCategory> ConstructCategories = new List<ConstructCategory>();
        readonly List<ConstructSubCategory> ConstructSubCategories = new List<ConstructSubCategory>();

        readonly UrlHelper _urlHelper;
        readonly PaginationHelper _paginationHelper;

        ConstructCategory _pickedConstructCategory = new ConstructCategory() /*{ Name = "All" }*/;
        ConstructSubCategory _pickedConstructSubCategory = new ConstructSubCategory();

        RangeObservableCollection<PageNumber> _pageNumbers = new RangeObservableCollection<PageNumber>();

        #endregion

        #region Ctor
        public ConstructAssetPickerControl()
        {
            InitializeComponent();
            _urlHelper = App.ServiceProvider.GetService(typeof(UrlHelper)) as UrlHelper;
            _paginationHelper = App.ServiceProvider.GetService(typeof(PaginationHelper)) as PaginationHelper;

            ConstructAssets = JsonSerializer.Deserialize<ConstructAsset[]>(Service.Properties.Resources.ConstructAssets).ToList();

            ConstructCategories = ConstructAssets.Select(x => x.Category).Distinct().Select(z => new ConstructCategory()
            {
                Id = z,
                Name = Constants.CamelToName(z)
            }).ToList();

            ConstructSubCategories = ConstructAssets.Select(x => x.SubCategory).Distinct().Select(z => new ConstructSubCategory()
            {
                Id = z,
                Category = z.Split('\\')[0],
                Name = Constants.CamelToName(z.Split('\\')[1]),
            }).ToList();

            //ShowConstructCategories();
        }

        #endregion

        #region Methods

        #region Functionality

        private int GetFilteredConstructAssetsCount()
        {
            int count = 0;

            count = ConstructAssets.Count(x => (!_constructCategoryFilter.IsNullOrBlank() ? x.Category == _constructCategoryFilter : !x.Category.IsNullOrBlank())
             && (!_constructSubCategoryFilter.IsNullOrBlank() ? x.SubCategory == _constructSubCategoryFilter : !x.SubCategory.IsNullOrBlank())
             && (!TextBox_SearchConstructAssets.Text.IsNullOrBlank() ? x.Name.ToLowerInvariant().Contains(TextBox_SearchConstructAssets.Text.ToLowerInvariant()) : !x.Name.IsNullOrBlank()));

            return count;
        }

        private IEnumerable<ConstructAsset> GetFilteredConstructAssets()
        {
            var filteredData = new List<ConstructAsset>();

            filteredData.AddRange(ConstructAssets.Where(x =>
            (!_constructCategoryFilter.IsNullOrBlank() ? x.Category == _constructCategoryFilter : !x.Category.IsNullOrBlank())
            && (!_constructSubCategoryFilter.IsNullOrBlank() ? x.SubCategory == _constructSubCategoryFilter : !x.SubCategory.IsNullOrBlank())
            && (!TextBox_SearchConstructAssets.Text.IsNullOrBlank() ? x.Name.ToLowerInvariant().Contains(TextBox_SearchConstructAssets.Text.ToLowerInvariant()) : !x.Name.IsNullOrBlank())));

            return filteredData;
        }

        private void SearchConstructAssets()
        {
            _pageIndex = 0;
            ShowConstructAssetsCount();
            GetConstructAssets();
        }

        public void ShowConstructCategories()
        {
            var pagedData = ConstructCategories;

            var _masonryPanel = new MasonryPanelWithProgressiveLoading()
            {
                Margin = new Thickness(5),
                Style = Application.Current.Resources["Panel_Style"] as Style,
                // Height = _masonryPanelHeight
            };

            //Add all the categories
            foreach (var constructCategory in pagedData)
            {
                AddConstructCategoryMasonry(_masonryPanel, constructCategory);
            }

            ContentScrollViewer.Content = _masonryPanel;

            _pageNumbers.Clear();
            PagesHolder.ItemsSource = _pageNumbers;

            TextBox_SearchConstructAssets.PlaceholderText = $"{ pagedData.Count() } categories...";
        }

        private void AddConstructCategoryMasonry(MasonryPanelWithProgressiveLoading _masonryPanel, ConstructCategory constructCategory)
        {
            var buttonName = constructCategory.Name.Replace(" ", "");

            var button_Category = new Button()
            {
                Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                Width = _constructCategoryMasonrySize + 30,
                Height = _constructCategoryMasonrySize - 40,
                Margin = new Thickness(3),
                Tag = constructCategory,
                FontSize = 16,
                Content = constructCategory.Name,
                Name = buttonName,
            };

            button_Category.Click += ButtonConstructCategory_Click;
            _masonryPanel.Children.Add(button_Category);
        }

        private void ShowConstructSubCategories()
        {
            var pagedData = _constructCategoryFilter.IsNullOrBlank() ? ConstructSubCategories : ConstructSubCategories.Where(x => x.Category == _constructCategoryFilter);

            var _masonryPanel = new MasonryPanelWithProgressiveLoading()
            {
                Margin = new Thickness(5),
                Style = Application.Current.Resources["Panel_Style"] as Style,
                // Height = _masonryPanelHeight
            };

            //Add all the categories
            foreach (var ConstructSubCategory in pagedData)
            {
                AddConstructSubCategoryMasonry(_masonryPanel, ConstructSubCategory);
            }

            ContentScrollViewer.Content = _masonryPanel;

            _pageNumbers.Clear();
            PagesHolder.ItemsSource = _pageNumbers;

            TextBox_SearchConstructAssets.PlaceholderText = $"{ pagedData.Count() } sub categories in {_pickedConstructCategory?.Name.ToLowerInvariant()}";
        }

        private void AddConstructSubCategoryMasonry(MasonryPanelWithProgressiveLoading _masonryPanel, ConstructSubCategory constructSubCategory)
        {
            var buttonName = constructSubCategory.Name.Replace(" ", "");

            var button_Category = new Button()
            {
                Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                Width = _constructCategoryMasonrySize + 30,
                Height = _constructCategoryMasonrySize - 40,
                Margin = new Thickness(3),
                Tag = constructSubCategory,
                FontSize = 16,
                Content = new TextBlock() { Text = constructSubCategory.Name, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.SemiBold, FontSize = 16 },
                Name = buttonName,
            };

            button_Category.Click += ButtonConstructSubCategory_Click;
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
            var count = GetFilteredConstructAssetsCount();

            _totalPageCount = _paginationHelper.GetTotalPageCount(pageSize: _pageSize, dataCount: count);

            var can = _pickedConstructCategory?.Name.ToLowerInvariant();
            var ccn = _pickedConstructSubCategory?.Name.ToLowerInvariant();
            var match = TextBox_SearchConstructAssets.Text.IsNullOrBlank() ? "" : " matching " + TextBox_SearchConstructAssets.Text;

            TextBox_SearchConstructAssets.PlaceholderText = $"{ count } constructs in {can}/{ccn}{match}";
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

        private void Button_SearchConstructAssets_Click(object sender, RoutedEventArgs e)
        {
            SearchConstructAssets();
        }

        private void Button_ConstructCategory_Click(object sender, RoutedEventArgs e)
        {
            ShowConstructSubCategories();
        }

        private void ButtonPreview_Click(object sender, RoutedEventArgs e)
        {
            if (!_settingConstructAssets)
            {
                _pageIndex = _paginationHelper.GetPreviousPageNumber(_pageIndex);

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
            var categoryId = _pickedConstructCategory.Id;

            //if (categoryId.Equals("All"))
            //{
            //    _constructSubCategoryFilter = null;
            //    _pickedConstructSubCategory = null;
            //    Button_ConstructCategory.Content = "";
            //    Button_ConstructCategory.Visibility = Visibility.Collapsed;

            //    _pageIndex = 0;
            //    ShowConstructAssetsCount();
            //    GetConstructAssets();
            //}
            //else
            //{
            _constructCategoryFilter = categoryId;

            Button_ConstructCategory.Content = _pickedConstructCategory.Name;
            Button_ConstructCategory.Visibility = Visibility.Visible;

            ShowConstructSubCategories();
            //}
        }

        private void ButtonConstructSubCategory_Click(object sender, RoutedEventArgs e)
        {
            _pickedConstructSubCategory = ((Button)sender).Tag as ConstructSubCategory;
            var subCategoryId = _pickedConstructSubCategory.Id;

            _constructSubCategoryFilter = subCategoryId;

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

        private void TextBox_SearchConstructAssets_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
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
