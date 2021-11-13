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
        List<ConstructAsset> _constructAssets = new List<ConstructAsset>();

        ScrollViewer scrollViewer = new ScrollViewer()
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        MasonryPanelWithProgressiveLoading wrapPanel = new MasonryPanelWithProgressiveLoading()
        {
            Margin = new Thickness(5),
            Style = Application.Current.Resources["Panel_Style"] as Style
        };

        public ConstructAssetPicker(List<ConstructAsset> constructAssets)
        {
            _constructAssets = constructAssets;

            Height = 500;
            Width = 470;
            Title = "Select a Construct";
            Style = Application.Current.Resources["MaterialDesign_ChildWindow_Style"] as Style;

            scrollViewer.Content = wrapPanel;

            Content = scrollViewer;

            var pagedAssets = _constructAssets.Take(50);

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
                    Margin = new Thickness(3)
                };

                buttonConstructAsset.Click += ButtonConstructAsset_Click;

                buttonConstructAsset.Content = img;

                wrapPanel.Children.Add(buttonConstructAsset);
            }
        }

        private void ButtonConstructAsset_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
