using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Data;

namespace Worldescape
{
    public class WorldHelper
    {
        #region UI

        /// <summary>
        /// Gets the world image in a circular border from the provided world.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public Border GetWorldPicture(World world, double size = 40)
        {
            var bitmapImage = new BitmapImage(new Uri(world.ImageUrl));
            return PrepareRoundImage(size, bitmapImage);
        }

        /// <summary>
        /// Prepares a rounded border to contain the provided bitmap image.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="bitmapImage"></param>
        /// <returns></returns>
        private Border PrepareRoundImage(double size, BitmapImage bitmapImage)
        {
            var imageBorder = new Border()
            {
                Height = size,
                Width = size,
                CornerRadius = new CornerRadius(40),
                ClipToBounds = true,
                Background = new SolidColorBrush(Colors.Transparent),
            };

            Image userImage = new Image()
            {
                Source = bitmapImage,
                Height = size,
                Width = size,
                Stretch = Stretch.UniformToFill,
            };

            imageBorder.Child = userImage;
            return imageBorder;
        }

        #endregion
    }
}
