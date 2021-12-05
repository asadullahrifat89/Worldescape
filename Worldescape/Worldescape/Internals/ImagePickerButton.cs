using CSHTML5.Extensions.FileOpenDialog;
using System;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Worldescape
{
    public class ImagePickerButton : Button
    {
        public event EventHandler<FileOpenedEventArgs> FileOpened;

        public ImagePickerButton()
        {
            Style = Application.Current.TryFindResource("MaterialDesign_RoundButton_Style") as Style;

            var content = new Grid();

            content.Children.Add(new Image()
            {
                Source = new BitmapImage(new Uri("ms-appx:///Worldescape/Assets/Icons/file_upload_black_24dp.svg")),
                Height = 50,
            });

            var fileOpener = new FileOpenDialogPresenter()
            {
                Filter = "(.jpg;*.png)|*.jpg;*.png",
                Opacity = 0,
                ResultKind = ResultKind.DataURL,
            };

            fileOpener.FileOpened += FileOpened;
            content.Children.Add(fileOpener);

            Content = content;

        }
    }
}
