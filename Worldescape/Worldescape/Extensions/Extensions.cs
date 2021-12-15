using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Worldescape
{
    public static class PageExtensions
    {
        public static Page SetRandomBackground(this Page page)
        {
            Color color = App.BackgroundColors[new Random().Next(0, App.BackgroundColors.Count())];
            page.Background = new SolidColorBrush(color);

            return page;
        }
    }
}
