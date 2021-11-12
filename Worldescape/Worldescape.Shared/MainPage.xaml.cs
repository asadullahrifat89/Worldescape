using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Worldescape
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        bool _isPointerCaptured;
        double _pointerX;
        double _pointerY;
        double _objectLeft;
        double _objectTop;

        bool _isCraftingMode;

        public MainPage()
        {
            this.InitializeComponent();
            DrawObjectsOnCanvas();
            AddCharacterOnCanvas();

            this.CraftButton.Click += CraftButton_Click;
            
        }

        private void AddCharacterOnCanvas()
        {
            var uri = "ms-appx:///Assets/Images/Avatar_Profiles/John_The_Seer/character_maleAdventurer_idle.png";

            var rect = new Rectangle()
            {
                Fill = new ImageBrush()
                {
                    ImageSource = new BitmapImage() { UriSource = new Uri(uri) },
                    Stretch = Stretch.Uniform,
                },
                Stretch = Stretch.Uniform,
                Height = 100,
                Width = 100,
            };

            Canvas.SetTop(rect, new Random().Next(500));
            Canvas.SetLeft(rect, new Random().Next(500));
            canvas_root.Children.Add(rect);
        }

        private void CraftButton_Click(object sender, RoutedEventArgs e)
        {
            _isCraftingMode = !_isCraftingMode;

            this.CraftButton.Content = _isCraftingMode ? "Crafting" : "Craft";
        }

        private string[] _objects = new string[]
        {
            "ms-appx:///Assets/Images/World_Objects/Landscape/Grass.png",
            "ms-appx:///Assets/Images/World_Objects/Landscape/Big_Tree.png",
            "ms-appx:///Assets/Images/World_Objects/Prototype/arrow_E.png",
        };

        private void DrawObjectsOnCanvas()
        {
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 10; i++)
                {
                    var uri = _objects[new Random().Next(_objects.Count())];

                    var rect = new Rectangle()
                    {
                        Fill = new ImageBrush()
                        {
                            ImageSource = new BitmapImage() { UriSource = new Uri(uri) },
                            Stretch = Stretch.Uniform,
                        },
                        MinHeight = 300,
                        MinWidth = 300,
                        //new SolidColorBrush(Colors.Green),
                    };

                    rect.AllowDrop = true;
                    rect.AllowFocusOnInteraction = true;

                    rect.DoubleTapped += Rect_DoubleTapped;

                    rect.PointerPressed += Button_PointerPressed;
                    rect.PointerMoved += Button_PointerMoved;
                    rect.PointerReleased += Button_PointerReleased;

                    Canvas.SetTop(rect, i * 100);
                    Canvas.SetLeft(rect, (i + j * 2) * 100);
                    canvas_root.Children.Add(rect);
                }
            }


        }

        private void Button_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_isCraftingMode)
            {
                UIElement uielement = (UIElement)sender;
                _objectLeft = Canvas.GetLeft(uielement);
                _objectTop = Canvas.GetTop(uielement);

                _pointerX = e.GetCurrentPoint(canvas_root).Position.X;
                _pointerY = e.GetCurrentPoint(canvas_root).Position.Y;
                uielement.CapturePointer(e.Pointer);

                _isPointerCaptured = true;
            }
        }

        private void Button_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isCraftingMode)
            {
                UIElement uielement = (UIElement)sender;

                if (_isPointerCaptured)
                {
                    // Calculate the new position of the object:
                    double deltaH = e.GetCurrentPoint(canvas_root).Position.X - _pointerX;
                    double deltaV = e.GetCurrentPoint(canvas_root).Position.Y - _pointerY;

                    _objectLeft = deltaH + _objectLeft;
                    _objectTop = deltaV + _objectTop;

                    // Update the object position:
                    Canvas.SetLeft(uielement, _objectLeft);
                    Canvas.SetTop(uielement, _objectTop);

                    // Remember the pointer position:
                    _pointerX = e.GetCurrentPoint(canvas_root).Position.X;
                    _pointerY = e.GetCurrentPoint(canvas_root).Position.Y;
                }
            }
        }

        private void Button_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_isCraftingMode)
            {
                UIElement uielement = (UIElement)sender;
                _isPointerCaptured = false;
                uielement.ReleasePointerCapture(e.Pointer);
            }
        }

        private async void Rect_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (_isCraftingMode)
            {
                //UIElement uielement = (UIElement)sender;

                //var flyout = new Flyout() { Content = new Button() { Content = new TextBlock() { Text = "Click" } } };

                //var _objectLeft = Canvas.GetLeft(uielement);
                //var _objectTop = Canvas.GetTop(uielement);

                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Save your work?";
                dialog.PrimaryButtonText = "Save";
                dialog.SecondaryButtonText = "Don't Save";
                dialog.CloseButtonText = "Cancel";
                dialog.DefaultButton = ContentDialogButton.Primary;

                var result = await dialog.ShowAsync();
            }
        }
    }
}
