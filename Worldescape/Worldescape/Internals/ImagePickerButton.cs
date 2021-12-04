using CSHTML5.Extensions.FileOpenDialog;
using System;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Worldescape
{
    public class ImagePickerButton : Button
    {
        public event EventHandler<FileOpenedEventArgs> FileOpened;

        public ImagePickerButton()
        {            
            Style = Application.Current.TryFindResource("MaterialDesign_RoundButton_Style") as Style;
         
            var content = new Grid();
            content.Children.Add(new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe Fluent Icons"),
                FontWeight = FontWeights.Normal,
                Text = "\ue898"
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
