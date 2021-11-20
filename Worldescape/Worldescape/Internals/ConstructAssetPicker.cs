using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Data;

namespace Worldescape.Service
{
    public class ConstructAssetPicker : ChildWindow
    {
        int pageSize = 21;
        int pageIndex = 0;
        int totalPageCount = 0;

        bool _settingConstructAssets = false;

        string _pickedConstructCategory = string.Empty;

        List<ConstructAsset> _constructAssets = new List<ConstructAsset>();
        List<ConstructCategory> _constructCategories = new List<ConstructCategory>();

        Action<ConstructAsset> _assetSelected;

        ScrollViewer _scrollViewer = new ScrollViewer()
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        Grid _gridContent = new Grid();
        StackPanel _stackPanelFooter = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(5) };

        readonly AssetUrlHelper _assetUriHelper;

        public ConstructAssetPicker(
            List<ConstructAsset> constructAssets,
            List<ConstructCategory> constructCategories,
            Action<ConstructAsset> assetSelected,
            AssetUrlHelper assetUriHelper)
        {
            _assetUriHelper = assetUriHelper;
            _constructAssets = constructAssets;
            _constructCategories = constructCategories;

            _assetSelected = assetSelected;

            Height = 600;
            Width = 610;
            Style = Application.Current.Resources["MaterialDesign_ChildWindow_Style"] as Style;

            _gridContent.RowDefinitions.Add(new RowDefinition());
            _gridContent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

            Grid.SetRow(_scrollViewer, 0);

            _gridContent.Children.Add(_scrollViewer);

            Button buttonShowCategories = new Button()
            {
                Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                Content = "Categories",
                Width = 120,
                Margin = new Thickness(5)
            };

            buttonShowCategories.Click += ButtonShowCategories_Click;
            _stackPanelFooter.Children.Add(buttonShowCategories);

            Button buttonPreview = new Button()
            {
                Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                Content = "Previous",
                Width = 120,
                Margin = new Thickness(5)
            };

            buttonPreview.Click += ButtonPreview_Click;
            _stackPanelFooter.Children.Add(buttonPreview);

            Button buttonNext = new Button()
            {
                Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                Content = "Next",
                Width = 120,
                Margin = new Thickness(5)
            };

            buttonNext.Click += ButtonNext_Click;
            _stackPanelFooter.Children.Add(buttonNext);

            Grid.SetRow(_stackPanelFooter, 1);
            _gridContent.Children.Add(_stackPanelFooter);

            Content = _gridContent;

            ShowConstructCategories();
        }

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

            _scrollViewer.Content = _masonryPanel;
        }

        private void ButtonConstructCategory_Click(object sender, RoutedEventArgs e)
        {
            pageIndex = 0;
            _pickedConstructCategory = (((Button)sender).Tag as ConstructCategory).Name;
            ShowConstructAssets();
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
                var uri = _assetUriHelper.BuildAssetUrl(item.ImageUrl);

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

            _scrollViewer.Content = _masonryPanel;

            _settingConstructAssets = false;
        }

        private void ButtonConstructAsset_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var constructAsset = button.Tag as ConstructAsset;

            _assetSelected?.Invoke(constructAsset);
            Close();
        }
    }
}
