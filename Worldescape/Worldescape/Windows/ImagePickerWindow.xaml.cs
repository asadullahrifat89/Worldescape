using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

            var command = new SaveBlobCommandRequest()
            {
                Id = UidGenerator.New(),
                DataUrl = _selectedDataUrl,
            };

            _newDataUrl?.Invoke(_selectedDataUrl);
            this.DialogResult = true;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

