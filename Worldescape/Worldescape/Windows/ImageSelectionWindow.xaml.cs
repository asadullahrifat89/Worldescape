using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Worldescape.Common;
using Worldescape.Service;

namespace Worldescape
{
    public partial class ImageSelectionWindow : ChildWindow
    {
        #region Fields
                
        string _selectedDataUrl;
        readonly Action<string> _blobId;

        readonly ImageHelper _imageHelper;
        readonly UrlHelper _urlHelper;
        readonly BlobRepository _blobRepository;

        #endregion

        #region Ctor

        public ImageSelectionWindow(Action<string> blobId, string imageUrl = null)
        {
            InitializeComponent();

            _blobId = blobId;

            _imageHelper = App.ServiceProvider.GetService(typeof(ImageHelper)) as ImageHelper;
            _urlHelper = App.ServiceProvider.GetService(typeof(UrlHelper)) as UrlHelper;
            _blobRepository = App.ServiceProvider.GetService(typeof(BlobRepository)) as BlobRepository;

            if (!imageUrl.IsNullOrBlank())
            {
                _selectedDataUrl = imageUrl;
                Image_ProfileImageUrl.Source = _imageHelper.GetBitmapImage(imageUrl.Contains("ms-appx:") ? imageUrl : _urlHelper.BuildBlobUrl(App.Token, imageUrl));
            }
        }

        #endregion

        #region Methods

        #region Button Events

        private async void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            var response = await _blobRepository.SaveBlob(App.Token, _selectedDataUrl);

            if (!response.Success)
            {
                var contentDialogue = new MessageDialogueWindow(title: "Error!", message: response.Error);
                contentDialogue.Show();
            }
            else
            {
                _blobId?.Invoke(response.Result.ToString());
                this.DialogResult = true;
            }
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void FileOpenDialogPresenter_ImageUrl_FileOpened(object sender, CSHTML5.Extensions.FileOpenDialog.FileOpenedEventArgs e)
        {
            _selectedDataUrl = e.DataURL;

            if (string.IsNullOrEmpty(_selectedDataUrl))
                return;

            Image_ProfileImageUrl.Source = _imageHelper.GetBitmapImage(_selectedDataUrl);
        }

        #endregion

        #endregion
    }
}

