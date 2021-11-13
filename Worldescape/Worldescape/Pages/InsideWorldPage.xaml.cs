using System;
using System.Linq;
using System.Numerics;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace Worldescape.Pages
{
    public partial class InsideWorldPage : Page
    {
        #region Fields

        bool _isPointerCaptured;
        double _pointerX;
        double _pointerY;
        double _objectLeft;
        double _objectTop;

        bool _isCraftingMode;
        bool _isMovingMode;

        Button avatar = new Button() { Style = Application.Current.Resources["MaterialDesign_HyperlinkButton_Style"] as Style };

        UIElement interactiveConstruct;
        UIElement movingConstruct;

        EasingFunctionBase easingFunction = new ExponentialEase()
        {
            EasingMode = EasingMode.EaseOut,
            Exponent = 5,
        };

        string[] _objects = new string[]
        {
            "ms-appx:///Images/World_Objects/Landscape/Grass.png",
            "ms-appx:///Images/World_Objects/Landscape/Big_Tree.png",
            "ms-appx:///Images/World_Objects/Prototype/arrow_E.png",
        };

        string avatarUrl = "ms-appx:///Images/Avatar_Profiles/John_The_Seer/character_maleAdventurer_idle.png";

        #endregion

        public InsideWorldPage()
        {
            this.InitializeComponent();

            MoveButton.Visibility = Visibility.Collapsed;

            DrawConstructsOnCanvas();
            DrawAvatarOnCanvas();
        }

        #region Methods

        private void DrawConstructsOnCanvas()
        {
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 10; i++)
                {
                    var uri = _objects[new Random().Next(_objects.Count())];

                    var bitmap = new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));

                    var img = new Image() { Source = bitmap, Stretch = Stretch.None };

                    var obj = new Button()
                    {
                        BorderBrush = new SolidColorBrush(Colors.DodgerBlue),
                        Style = Application.Current.Resources["MaterialDesign_ConstructButton_Style"] as Style
                    };

                    obj.Content = img;

                    //obj.AllowDrop = true;
                    obj.AllowScrollOnTouchMove = false;

                    obj.PointerPressed += Construct_PointerPressed;
                    obj.PointerMoved += Construct_PointerMoved;
                    obj.PointerReleased += Construct_PointerReleased;

                    Canvas.SetTop(obj, i * 100);
                    Canvas.SetLeft(obj, (i + j * 2) * 100);

                    Canvas_root.Children.Add(obj);
                }
            }
        }

        private void DrawAvatarOnCanvas()
        {
            var uri = avatarUrl;

            var bitmap = new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));

            var img = new Image()
            {
                Source = bitmap,
                Stretch = Stretch.Uniform,
                Height = 100,
                Width = 100,
            };

            avatar.Content = img;

            //avatar.Effect = new DropShadowEffect() { ShadowDepth = 3, Color = Colors.Black, BlurRadius = 10, Opacity = 0.3 };

            Canvas.SetTop(avatar, new Random().Next(500));
            Canvas.SetLeft(avatar, new Random().Next(500));
            this.Canvas_root.Children.Add(avatar);
        }

        private void CraftButton_Click(object sender, RoutedEventArgs e)
        {
            _isCraftingMode = !_isCraftingMode;
            this.CraftButton.Content = _isCraftingMode ? "Crafting" : "Craft";

            _isMovingMode = false;
            this.MoveButton.Content = "Move";

            if (!_isCraftingMode)
            {
                this.MoveButton.Visibility = Visibility.Collapsed;
                movingConstruct = null;
                MovingConstructHolder.Children.Clear();
            }
        }

        private void Canvas_root_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_isMovingMode && movingConstruct != null)
            {
                MoveElement(e, movingConstruct);
            }
        }

        private void Construct_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

            UIElement uielement = (UIElement)sender;
            interactiveConstruct = uielement;
            ShowInteractiveConstruct(uielement);

            if (_isMovingMode)
            {
                if (movingConstruct != null)
                {
                    MoveElement(e, movingConstruct);
                }
            }
            else if (_isCraftingMode)
            {
                this.MoveButton.Visibility = Visibility.Visible;

                //UIElement uielement = (UIElement)sender;
                _objectLeft = Canvas.GetLeft(uielement);
                _objectTop = Canvas.GetTop(uielement);

                _pointerX = e.GetCurrentPoint(this.Canvas_root).Position.X;
                _pointerY = e.GetCurrentPoint(this.Canvas_root).Position.Y;
                uielement.CapturePointer(e.Pointer);

                _isPointerCaptured = true;
            }
            else
            {
                MoveElement(e, avatar);
            }
        }

        private void Construct_PointerMoved(object sender, PointerRoutedEventArgs e)
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

        private void Construct_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_isMovingMode)
            {
                return;
            }

            if (_isCraftingMode)
            {
                UIElement uielement = (UIElement)sender;
                _isPointerCaptured = false;
                uielement.ReleasePointerCapture(e.Pointer);

                interactiveConstruct = uielement;
                ShowInteractiveConstruct(uielement);
            }
        }

        private void ShowInteractiveConstruct(UIElement uielement)
        {
            Button historyButton = CopyConstructContent(uielement);

            InteractiveConstructHolder.Children.Clear();
            InteractiveConstructHolder.Children.Add(historyButton);
        }

        private static Button CopyConstructContent(UIElement uielement)
        {
            var oriBitmap = ((Image)((Button)uielement).Content).Source as BitmapImage;

            var bitmap = new BitmapImage(new Uri(oriBitmap.UriSource.OriginalString, UriKind.RelativeOrAbsolute));
            var img = new Image() { Source = bitmap, Stretch = Stretch.Uniform, Height = 50, Width = 100, Margin = new Thickness(10) };

            var historyButton = new Button() { Content = img, Style = Application.Current.Resources["MaterialDesign_HyperlinkButton_Style"] as Style };
            return historyButton;
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            InteractiveConstructHolder.Children.Clear();
            interactiveConstruct = null;
        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            _isMovingMode = !_isMovingMode;
            MoveButton.Content = _isMovingMode ? "Moving" : "Move";

            if (!_isMovingMode)
            {
                movingConstruct = null;
                MovingConstructHolder.Children.Clear();
            }
            else
            {
                UIElement uielement = interactiveConstruct;
                movingConstruct = uielement;
                ShowMovingConstruct(movingConstruct);
            }
        }

        private void ShowMovingConstruct(UIElement uielement)
        {
            Button historyButton = CopyConstructContent(uielement);

            MovingConstructHolder.Children.Clear();
            MovingConstructHolder.Children.Add(historyButton);
        }

        private void MoveElement(PointerRoutedEventArgs e, UIElement uIElement)
        {
            var nowX = Canvas.GetLeft(uIElement);
            var nowY = Canvas.GetTop(uIElement);

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

            #region Storyboard

            Storyboard moveStory = new Storyboard();

            DoubleAnimation setLeft = new DoubleAnimation()
            {
                From = nowX,
                To = goToX,
                Duration = new Duration(TimeSpan.FromSeconds(timeToTravelDistance)),
                EasingFunction = easingFunction,
            };

            DoubleAnimation setRight = new DoubleAnimation()
            {
                From = nowY,
                To = goToY,
                Duration = new Duration(TimeSpan.FromSeconds(timeToTravelDistance)),
                EasingFunction = easingFunction,
            };

            setRight.Completed += (object sender, EventArgs e) =>
            {
                //TODO: set idle logic here
            };

            Storyboard.SetTarget(setLeft, uIElement);
            Storyboard.SetTargetProperty(setLeft, new PropertyPath(Canvas.LeftProperty));

            Storyboard.SetTarget(setRight, uIElement);
            Storyboard.SetTargetProperty(setRight, new PropertyPath(Canvas.TopProperty));

            moveStory.Children.Add(setLeft);
            moveStory.Children.Add(setRight);

            moveStory.Begin();

            #endregion
        }

        private void ChildWindowButton_Click(object sender, RoutedEventArgs e)
        {
            ChildWindow childWindow = new ChildWindow()
            {
                Height = 500,
                Width = 500,
                Title = "Login",
                Style = Application.Current.Resources["MaterialDesign_ChildWindow_Style"] as Style
            };

            var stackpanel = new StackPanel();

            stackpanel.Children.Add(new TextBlock()
            {
                Text = "Email",
                FontSize = 14,
                Margin = new Thickness(10, 0, 10, 0),
                Foreground = new SolidColorBrush(Colors.DarkGray),
                FontWeight = FontWeights.SemiBold
            });
            stackpanel.Children.Add(new TextBox()
            {
                Margin = new Thickness(10),
                FontSize = 14,
                Style = Application.Current.Resources["MaterialDesign_TextBox_Style"] as Style
            });

            stackpanel.Children.Add(new TextBlock()
            {
                Text = "Password",
                FontSize = 14,
                Margin = new Thickness(10, 0, 10, 0),
                Foreground = new SolidColorBrush(Colors.DarkGray),
                FontWeight = FontWeights.SemiBold
            });
            stackpanel.Children.Add(new PasswordBox()
            {
                Margin = new Thickness(10),
                FontSize = 14,
                Style = Application.Current.Resources["MaterialDesign_PasswordBox_Style"] as Style
            });

            childWindow.Content = stackpanel;

            childWindow.Show();
        }

        #endregion
    }
}
