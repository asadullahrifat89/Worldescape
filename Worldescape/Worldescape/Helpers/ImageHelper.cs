using System;
using Windows.UI.Xaml.Media.Imaging;

namespace Worldescape
{
    public class ImageHelper
    {
        /// <summary>
        /// Returns a BitmapImage from the provided dataUrl.
        /// </summary>
        /// <param name="dataUrl"></param>
        /// <returns></returns>
        public BitmapImage GetBitmapImage(string dataUrl)
        {
            var bitmapimage = new BitmapImage();

            if (dataUrl.Contains("data:image/"))
            {
                bitmapimage.SetSource(dataUrl);
            }
            else
            {
                bitmapimage.UriSource = new Uri(dataUrl);
            }

            return bitmapimage;
        }
    }
}
