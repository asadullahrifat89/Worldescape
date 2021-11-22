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

        int pageSize = 24;
        int pageIndex = 0;
        int totalPageCount = 0;

        bool _settingConstructAssets = false;

        string _pickedConstructCategory = string.Empty;

        List<ConstructAsset> _constructAssets = new List<ConstructAsset>();
        List<ConstructCategory> _constructCategories = new List<ConstructCategory>();

        Action<ConstructAsset> _assetSelected;

        readonly AssetUrlHelper _assetUriHelper;

        #endregion

        #region Ctor
        public ConstructAssetPickerWindow(
           List<ConstructAsset> constructAssets,
           List<ConstructCategory> constructCategories,
           Action<ConstructAsset> assetSelected,
           AssetUrlHelper assetUriHelper)
        {
            InitializeComponent();

            _assetUriHelper = assetUriHelper;
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
        }

        private void ShowConstructAssets()
        {
            Title = "Select a Construct";

            _settingConstructAssets = true;

            var filteredData = string.IsNullOrEmpty(_pickedConstructCategory) ? _constructAssets : _constructAssets.Where(x => x.Category == _pickedConstructCategory);

            totalPageCount = filteredData.Count() / pageSize;

            var pagedData = filteredData.Skip(pageIndex * pageSize).Take(pageSize);

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

                var img = new Image() { Source = bitmap, Stretch = Stretch.Uniform, Height = 100, Width = 100 };

                var buttonConstructAsset = new Button()
                {
                    Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                    Width = 100,
                    Height = 100,
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

        #endregion

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        #endregion

        #region Button Events

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (!_settingConstructAssets)
            {
                pageIndex++;

                if (pageIndex > totalPageCount)
                {
                    pageIndex = totalPageCount;
                }

                ShowConstructAssets();
            }
        }

        private void ButtonPreview_Click(object sender, RoutedEventArgs e)
        {
            if (!_settingConstructAssets)
            {
                pageIndex--;

                if (pageIndex < 0)
                {
                    pageIndex = 0;
                    return;
                }

                ShowConstructAssets();
            }
        }

        private void ButtonShowCategories_Click(object sender, RoutedEventArgs e)
        {
            ShowConstructCategories();
        }

        private void ButtonConstructCategory_Click(object sender, RoutedEventArgs e)
        {
            pageIndex = 0;
            _pickedConstructCategory = (((Button)sender).Tag as ConstructCategory).Name;
            ShowConstructAssets();
        }

        private void ButtonConstructAsset_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var constructAsset = button.Tag as ConstructAsset;

            _assetSelected?.Invoke(constructAsset);
            
            Close();
        }

        #endregion
    }
}

