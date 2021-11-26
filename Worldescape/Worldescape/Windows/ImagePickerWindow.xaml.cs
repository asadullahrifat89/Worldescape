using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Worldescape.Data;

namespace Worldescape
{
    public partial class ImagePickerWindow : ChildWindow
    {
        #region Fields

        Action<string> _onDataUrl;
        string _selectedDataUrl;

        #endregion

        #region Ctor

        public ImagePickerWindow(Action<string> onDataUrl)
        {
            InitializeComponent();
            _onDataUrl = onDataUrl;
        }

        #endregion

        #region Methods


        #endregion

        private void FileOpenDialogPresenter_ImageUrl_FileOpened(object sender, CSHTML5.Extensions.FileOpenDialog.FileOpenedEventArgs e)
        {
            _selectedDataUrl = e.DataURL;

            if (string.IsNullOrEmpty(_selectedDataUrl))
                return;

            var bitmapimage = new BitmapImage();
            bitmapimage.SetSource(_selectedDataUrl);
            Image_ProfileImageUrl.Source = bitmapimage;

            //var base64String = e.DataURL;
            //base64String = base64String.Substring(base64String.IndexOf(',') + 1);

            //byte[] byteBuffer = Convert.FromBase64String(base64String);

            //using (MemoryStream memoryStream = new MemoryStream(byteBuffer))
            //{
            //    var bitmapImage = new BitmapImage();
            //    bitmapImage.SetSource(memoryStream);
            //    Image_ProfileImageUrl.Source = bitmapImage;
            //}
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            //TODO: upload the image to actual server and receive url and send it back
            _onDataUrl?.Invoke(_selectedDataUrl);
            this.DialogResult = true;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

