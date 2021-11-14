using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Shared.Models;

namespace Worldescape.Internals
{
    public class ConstructAssetPicker : ChildWindow
    {
        int pageSize = 21;
        int pageIndex = 0;
        int totalPageCount = 0;

        List<ConstructAsset> _constructAssets = new List<ConstructAsset>();

        Action<ConstructAsset> _assetSelected;

        ScrollViewer _scrollViewer = new ScrollViewer()
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        MasonryPanelWithProgressiveLoading _masonryPanel;

        Grid _gridContent = new Grid();
        StackPanel _stackPanelFooter = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(5) };

        public ConstructAssetPicker(
            List<ConstructAsset> constructAssets,
            Action<ConstructAsset> assetSelected)
        {
            _constructAssets = constructAssets;
            _assetSelected = assetSelected;

            totalPageCount = constructAssets.Count / pageSize;

            Height = 500;
            Width = 470;
            Title = "Select a Construct";
            Style = Application.Current.Resources["MaterialDesign_ChildWindow_Style"] as Style;

            _gridContent.RowDefinitions.Add(new RowDefinition());
            _gridContent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

            Grid.SetRow(_scrollViewer, 0);

            _gridContent.Children.Add(_scrollViewer);

            Button buttonPreview = new Button()
            {
                Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                Content = "Back"
            };

            buttonPreview.Click += ButtonPreview_Click;
            _stackPanelFooter.Children.Add(buttonPreview);

            Button buttonNext = new Button()
            {
                Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                Content = "Next"
            };

            buttonNext.Click += ButtonNext_Click;
            _stackPanelFooter.Children.Add(buttonNext);

            Grid.SetRow(_stackPanelFooter, 1);
            _gridContent.Children.Add(_stackPanelFooter);

            Content = _gridContent;

            SetDataSource();
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            pageIndex++;

            if (pageIndex > totalPageCount)
            {
                pageIndex = totalPageCount;
            }

            SetDataSource();

        }

        private void ButtonPreview_Click(object sender, RoutedEventArgs e)
        {
            pageIndex--;

            if (pageIndex < 0)
            {
                pageIndex = 0;
                return;
            }

            SetDataSource();
        }

        private void SetDataSource()
        {
            var pagedAssets = _constructAssets.Skip(pageIndex * pageSize).Take(pageSize);

            _masonryPanel = new MasonryPanelWithProgressiveLoading()
            {
                Margin = new Thickness(5),                
                Style = Application.Current.Resources["Panel_Style"] as Style
            };

            foreach (var item in pagedAssets)
            {
                var uri = item.ImageUrl;

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
        }

        private void ButtonConstructAsset_Click(object sender, RoutedEventArgs e)
        {
            _assetSelected?.Invoke(((Button)sender).Tag as ConstructAsset);
            this.Close();
        }
    }
}
