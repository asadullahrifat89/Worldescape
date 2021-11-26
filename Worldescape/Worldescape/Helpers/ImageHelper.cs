using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Media.Imaging;

namespace Worldescape
{
    public class ImageHelper
    {
        public BitmapImage GetBitmapImage(string dataUrl)
        {
            var bitmapimage = new BitmapImage();

            if (dataUrl.Contains("ms-appx:"))
            {
                bitmapimage.UriSource = new Uri(dataUrl);
            }
            else
            {
                bitmapimage.SetSource(dataUrl);
            }

            return bitmapimage;
        }
    }
}
