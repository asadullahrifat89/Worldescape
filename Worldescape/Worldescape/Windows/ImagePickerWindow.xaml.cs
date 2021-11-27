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

        Action<string> _newDataUrl;
        string _selectedDataUrl;

        readonly ImageHelper _imageHelper;

        #endregion

        #region Ctor

        public ImagePickerWindow(Action<string> newDataUrl, string oldDataUrl = null)
        {
            InitializeComponent();
            _newDataUrl = newDataUrl;
            _imageHelper = App.ServiceProvider.GetService(typeof(ImageHelper)) as ImageHelper;

            if (!oldDataUrl.IsNullOrBlank())
            {
                _selectedDataUrl = oldDataUrl;
                Image_ProfileImageUrl.Source = _imageHelper.GetBitmapImage(_selectedDataUrl);
            }
        }

        #endregion

        #region Methods


        #endregion

        private void FileOpenDialogPresenter_ImageUrl_FileOpened(object sender, CSHTML5.Extensions.FileOpenDialog.FileOpenedEventArgs e)
        {
            _selectedDataUrl = e.DataURL;

            if (string.IsNullOrEmpty(_selectedDataUrl))
                return;

            Image_ProfileImageUrl.Source = _imageHelper.GetBitmapImage(_selectedDataUrl);

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

            _newDataUrl?.Invoke(_selectedDataUrl);
            this.DialogResult = true;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

