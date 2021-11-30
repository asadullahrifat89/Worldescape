using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Common;

namespace Worldescape
{
    public partial class CharacterPickerWindow : ChildWindow
    {
        #region Fields
        
        List<Character> _characters = new List<Character>();
        Action<Character> _characterSelected; 

        #endregion

        #region Ctor

        public CharacterPickerWindow(
           List<Character> characters,
           Action<Character> characterSelected)
        {
            InitializeComponent();

            _characters = characters;
            _characterSelected = characterSelected;

            ShowCharacters();
        } 

        #endregion

        #region Methods

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
                var bitmap = new BitmapImage(new Uri(item.ImageUrl, UriKind.RelativeOrAbsolute));

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

            ContentScrollViewer.Content = _masonryPanel;
        }

        private void ButtonCharacter_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var Character = button.Tag as Character;

            _characterSelected?.Invoke(Character);
            Close();
        }

        #endregion
    }
}

