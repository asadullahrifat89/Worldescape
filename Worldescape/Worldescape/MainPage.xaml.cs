﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Effects;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace Worldescape
{
    public partial class MainPage : Page
    {
        bool _isPointerCaptured;
        double _pointerX;
        double _pointerY;
        double _objectLeft;
        double _objectTop;

        bool _isCraftingMode;
        bool _isMovingMode;

        HyperlinkButton avatar = new HyperlinkButton() { BorderThickness = new Thickness(0) };

        UIElement lastInteractedConstruct;

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

        public MainPage()
        {
            this.InitializeComponent();

            MoveButton.Visibility = Visibility.Collapsed;

            DrawConstructsOnCanvas();

            DrawAvatarOnCanvas();

            // Enter construction logic here...
        }

        private void DrawConstructsOnCanvas()
        {
            try
            {
                for (int j = 0; j < 5; j++)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var uri = _objects[new Random().Next(_objects.Count())];

                        var bitmap = new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));

                        var img = new Image() { Source = bitmap, Stretch = Stretch.None };

                        var obj = new HyperlinkButton() { BorderThickness = new Thickness(0) };

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
            catch (Exception ex)
            {

            }
        }

        private void DrawAvatarOnCanvas()
        {
            try
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
            catch (Exception ex)
            {


            }
        }

        private void CraftButton_Click(object sender, RoutedEventArgs e)
        {
            _isCraftingMode = !_isCraftingMode;
            this.CraftButton.Content = _isCraftingMode ? "Crafting" : "Craft";

            _isMovingMode = false;
            this.MoveButton.Content = "Move";

            LastConstructHolder.Children.Clear();
            lastInteractedConstruct = null;

            if (_isCraftingMode)
            {
                this.MoveButton.Visibility = Visibility.Visible;

            }
            else
            {
                this.MoveButton.Visibility = Visibility.Collapsed;
            }
        }

        private void Canvas_root_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_isMovingMode && lastInteractedConstruct != null)
            {
                MoveElement(e, lastInteractedConstruct);
            }
        }

        private void Construct_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_isMovingMode)
            {
                if (lastInteractedConstruct != null)
                {
                    MoveElement(e, lastInteractedConstruct);
                }
            }
            else if (_isCraftingMode)
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

                lastInteractedConstruct = uielement;
                ShowSelectedConstruct(uielement);
            }
        }

        private void ShowSelectedConstruct(UIElement uielement)
        {
            try
            {
                var oriBitmap = ((Image)((HyperlinkButton)uielement).Content).Source as BitmapImage;

                var bitmap = new BitmapImage(new Uri(oriBitmap.UriSource.OriginalString, UriKind.RelativeOrAbsolute));
                var img = new Image() { Source = bitmap, Stretch = Stretch.Uniform, Height = 50, Width = 150, Margin = new Thickness(10) };

                var historyButton = new HyperlinkButton() { Content = img };
                historyButton.Click += HistoryButton_Click;

                LastConstructHolder.Children.Clear();
                LastConstructHolder.Children.Add(historyButton);

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            LastConstructHolder.Children.Clear();
            lastInteractedConstruct = null;
        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            _isMovingMode = !_isMovingMode;
            MoveButton.Content = _isMovingMode ? "Moving" : "Move";

            if (!_isMovingMode)
            {
                LastConstructHolder.Children.Clear();
                lastInteractedConstruct = null;
            }
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
            ChildWindow childWindow = new ChildWindow();

            var stackpanel = new StackPanel();

            stackpanel.Children.Add(new TextBox() { Margin = new Thickness(10), Height = 30 });
            stackpanel.Children.Add(new PasswordBox() { Margin = new Thickness(10), Height = 30 });

            childWindow.Content = stackpanel;

            childWindow.Show();
        }
    }
}
