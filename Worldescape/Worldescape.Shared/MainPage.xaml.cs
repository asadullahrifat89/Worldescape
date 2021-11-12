using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
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

        Rectangle avatar = new Rectangle();

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

            var bitmap = new BitmapImage(new Uri(uri));

            avatar = new Rectangle()
            {
                Fill = new ImageBrush()
                {
                    ImageSource = bitmap,
                    Stretch = Stretch.Uniform,
                },
                Stretch = Stretch.Uniform,
                Height = 100,
                Width = 100,
            };

            //avatar.TranslationTransition = new Vector3Transition();

            Canvas.SetTop(avatar, new Random().Next(500));
            Canvas.SetLeft(avatar, new Random().Next(500));
            this.Canvas_root.Children.Add(avatar);
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

                    var bitmap = new BitmapImage(new Uri(uri));

                    var rect = new Rectangle()
                    {
                        Fill = new ImageBrush()
                        {
                            ImageSource = bitmap,
                            Stretch = Stretch.Uniform,
                        },
                        Height = 350,
                        Width = 350,
                    };

                    rect.AllowDrop = true;
                    rect.AllowFocusOnInteraction = true;

                    rect.DoubleTapped += Rect_DoubleTapped;

                    rect.PointerPressed += Button_PointerPressed;
                    rect.PointerMoved += Button_PointerMoved;
                    rect.PointerReleased += Button_PointerReleased;

                    Canvas.SetTop(rect, i * 100);
                    Canvas.SetLeft(rect, (i + j * 2) * 100);

                    Canvas_root.Children.Add(rect);
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

                _pointerX = e.GetCurrentPoint(this.Canvas_root).Position.X;
                _pointerY = e.GetCurrentPoint(this.Canvas_root).Position.Y;
                uielement.CapturePointer(e.Pointer);

                _isPointerCaptured = true;
            }
            else
            {
                var nowX = Canvas.GetLeft(avatar);
                var nowY = Canvas.GetTop(avatar);

                var goToX = e.GetCurrentPoint(this.Canvas_root).Position.X;
                var goToY = e.GetCurrentPoint(this.Canvas_root).Position.Y;

                float distance = Vector3.Distance(
                    new Vector3(
                        (float)nowX,
                        (float)nowY,
                        0),
                    new Vector3(
                        (float)goToX,
                        (float)goToY,
                        0));

                float unitPixel = 200f;
                float timeToTravelunitPixel = 0.5f;

                float timeToTravelDistance = distance / unitPixel * timeToTravelunitPixel;

                var nowZ = Canvas.GetZIndex(avatar);

                #region XamlFlair                

                //XamlFlair.CompoundSettings compoundSettings = new XamlFlair.CompoundSettings();
                //compoundSettings.Sequence = new List<XamlFlair.AnimationSettings>()
                //{
                //    new XamlFlair.AnimationSettings()
                //    {
                //        Kind = XamlFlair.AnimationKind.TranslateXTo,
                //        Duration = timeToTravelDistance,
                //        OffsetX = new XamlFlair.Offset() { OffsetValue = goToX },
                //        Easing = XamlFlair.EasingType.Quartic,
                //        EasingMode = EasingMode.EaseOut,
                //    },
                //    new XamlFlair.AnimationSettings()
                //    {
                //        Kind = XamlFlair.AnimationKind.TranslateXTo,
                //        Duration = timeToTravelDistance,
                //        OffsetX = new XamlFlair.Offset() { OffsetValue = goToY },
                //        Easing = XamlFlair.EasingType.Quartic,
                //        EasingMode = EasingMode.EaseOut,
                //    },
                //};

                //compoundSettings.Event = "PointerReleased";
                //XamlFlair.Animations.SetPrimary(avatar, compoundSettings);


                #endregion              

                #region Storyboard

                EasingFunctionBase easingFunction = new ExponentialEase
                {
                    EasingMode = EasingMode.EaseOut,
                    Exponent = 5,
                };

                Storyboard moveStory = new Storyboard();

                DoubleAnimation setLeft = new DoubleAnimation()
                {
                    From = nowX,
                    To = goToX,
                    //Duration = new Duration(TimeSpan.FromSeconds(timeToTravelDistance)),
                    //EasingFunction = easingFunction,
                };

                DoubleAnimation setRight = new DoubleAnimation()
                {
                    From = nowY,
                    To = goToY,
                    //Duration = new Duration(TimeSpan.FromSeconds(timeToTravelDistance)),
                    //EasingFunction = easingFunction,
                };

                Storyboard.SetTarget(setLeft, avatar);
                Storyboard.SetTargetProperty(setLeft, "(Canvas.Left)");

                Storyboard.SetTarget(setRight, avatar);
                Storyboard.SetTargetProperty(setRight, "(Canvas.Top)");

                moveStory.Children.Add(setLeft);
                moveStory.Children.Add(setRight);

                moveStory.Begin();

                #endregion

                Console.WriteLine("Avatar moved.");
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
                    double deltaH = e.GetCurrentPoint(this.Canvas_root).Position.X - _pointerX;
                    double deltaV = e.GetCurrentPoint(this.Canvas_root).Position.Y - _pointerY;

                    _objectLeft = deltaH + _objectLeft;
                    _objectTop = deltaV + _objectTop;

                    // Update the object position:
                    Canvas.SetLeft(uielement, _objectLeft);
                    Canvas.SetTop(uielement, _objectTop);

                    // Remember the pointer position:
                    _pointerX = e.GetCurrentPoint(this.Canvas_root).Position.X;
                    _pointerY = e.GetCurrentPoint(this.Canvas_root).Position.Y;
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
