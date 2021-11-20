using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Data;

namespace Worldescape.Service
{
    public class CharacterPicker : ChildWindow
    {

        List<Character> _characters = new List<Character>();

        Action<Character> _assetSelected;

        ScrollViewer _scrollViewer = new ScrollViewer()
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        Grid _gridContent = new Grid();
        StackPanel _stackPanelFooter = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(5) };

        public CharacterPicker(
            List<Character> characters,
            Action<Character> characterSelected)
        {
            _characters = characters;

            _assetSelected = characterSelected;

            Height = 600;
            Width = 610;
            Style = Application.Current.Resources["MaterialDesign_ChildWindow_Style"] as Style;

            _gridContent.Children.Add(_scrollViewer);

            Content = _gridContent;

            ShowCharacters();
        }

        private void ShowCharacters()
        {
            Title = "Select a Character";

            var _masonryPanel = new MasonryPanelWithProgressiveLoading()
            {
                Margin = new Thickness(5),
                Style = Application.Current.Resources["Panel_Style"] as Style,
                Height = 500
            };

            foreach (var item in _characters)
            {
                var uri = item.ImageUrl;

                item.ImageUrl = uri;

                var bitmap = new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));

                var img = new Image() { Source = bitmap, Stretch = Stretch.Uniform, Height = 100, Width = 100 };

                var buttonCharacter = new Button()
                {
                    Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                    Width = 100,
                    Height = 100,
                    Margin = new Thickness(3),
                    Tag = item,         
                };

                buttonCharacter.Click += ButtonCharacter_Click;
                buttonCharacter.Content = img;

                _masonryPanel.Children.Add(buttonCharacter);
            }

            _scrollViewer.Content = _masonryPanel;
        }

        private void ButtonCharacter_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var Character = button.Tag as Character;

            _assetSelected?.Invoke(Character);
            Close();
        }
    }
}
