using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Worldescape.Data;
using Worldescape.Service;

namespace Worldescape
{
    public partial class ImagePickerWindow : ChildWindow
    {
        #region Fields

        Action<string> _newDataUrl;
        string _selectedDataUrl;

        readonly ImageHelper _imageHelper;
        readonly UrlHelper _urlHelper;
        readonly HttpServiceHelper _httpServiceHelper;

        #endregion

        #region Ctor

        public ImagePickerWindow(Action<string> newDataUrl, string oldDataUrl = null)
        {
            InitializeComponent();

            _newDataUrl = newDataUrl;

            _imageHelper = App.ServiceProvider.GetService(typeof(ImageHelper)) as ImageHelper;
            _urlHelper = App.ServiceProvider.GetService(typeof(UrlHelper)) as UrlHelper;
            _httpServiceHelper = App.ServiceProvider.GetService(typeof(HttpServiceHelper)) as HttpServiceHelper;

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

        private async void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            var command = new SaveBlobCommandRequest()
            {
                Id = UidGenerator.New(),
                DataUrl = _selectedDataUrl,
                Token = App.Token
            };

            var response = await _httpServiceHelper.SendPostRequest<SaveBlobCommandResponse>(
              actionUri: Constants.Action_SaveBlob,
              payload: command);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK || !response.ExternalError.IsNullOrBlank())
            {
                MessageBox.Show(response.ExternalError.ToString());
            }
            else
            {
                _newDataUrl?.Invoke(_urlHelper.BuildBlobUrl(App.Token, response.Id));
                this.DialogResult = true;
            }
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

