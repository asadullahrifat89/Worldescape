using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Internals;
using Worldescape.Shared;
using Worldescape.Shared.Entities;
using Worldescape.Shared.Models;
using Image = Windows.UI.Xaml.Controls.Image;

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

        bool _isCrafting;
        bool _isMoving;
        bool _isCloning;
        bool _isDeleting;

        Button avatar = new Button() { Style = Application.Current.Resources["MaterialDesign_HyperlinkButton_Style"] as Style };

        UIElement _interactiveConstruct;
        UIElement _addingConstruct;
        UIElement _movingConstruct;
        UIElement _cloningConstruct;

        EasingFunctionBase _easingFunction = new ExponentialEase()
        {
            EasingMode = EasingMode.EaseOut,
            Exponent = 5,
        };

        string[] _objects = new string[]
        {
            "ms-appx:///Images/World_Objects/Landscape/Grass.png",
            "ms-appx:///Images/World_Objects/Landscape/Big_Tree.png",
            "ms-appx:///Images/World_Objects/Prototype/block_W.png",
        };

        string avatarUrl = "ms-appx:///Images/Avatar_Profiles/John_The_Seer/character_maleAdventurer_idle.png";

        List<ConstructAsset> ConstructAssets = new List<ConstructAsset>();

        List<ConstructCategory> ConstructCategories = new List<ConstructCategory>();

        ChildWindow childWindow = new ChildWindow();

        #endregion

        #region Ctor

        public InsideWorldPage()
        {
            this.InitializeComponent();

            DrawRandomConstructsOnCanvas();
            DrawAvatarOnCanvas();
        }

        #endregion

        #region Methods

        #region Common

        private void DrawRandomConstructsOnCanvas()
        {
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 10; i++)
                {
                    var uri = _objects[new Random().Next(_objects.Count())];

                    Button construct = GenerateConstructButton(new Construct() { ImageUrl = uri });

                    var x = (i + j * 2) * 200;
                    var y = i * 200;

                    DrawConstructOnCanvas(construct, x, y);
                }
            }
        }

        private void DrawConstructOnCanvas(UIElement construct, double x, double y)
        {
            Canvas.SetLeft(construct, x);
            Canvas.SetTop(construct, y);

            Canvas_root.Children.Add(construct);
        }

        /// <summary>
        /// Generate a new button from the provided construct. If constructId is provided then new id is not generated.
        /// </summary>
        /// <param name="construct"></param>
        /// <param name="constructId"></param>
        /// <returns></returns>
        private Button GenerateConstructButton(Construct construct, int? constructId = null)
        {
            var uri = construct.ImageUrl;

            var bitmap = new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));

            var img = new Image() { Source = bitmap, Stretch = Stretch.None };

            // This is broadcasted and saved in database
            var id = constructId ?? UidGenerator.New();

            var obj = new Button()
            {
                BorderBrush = new SolidColorBrush(Colors.DodgerBlue),
                Style = Application.Current.Resources["MaterialDesign_ConstructButton_Style"] as Style,
                Name = id.ToString(),
                Tag = new Construct()
                {
                    Id = id,
                    Name = construct.Name,
                    ImageUrl = uri,
                    Creator = new Creator() { },
                    World = new InWorld() { }
                }
            };

            obj.Content = img;

            //obj.AllowScrollOnTouchMove = false;

            obj.PointerPressed += Construct_PointerPressed;
            obj.PointerMoved += Construct_PointerMoved;
            obj.PointerReleased += Construct_PointerReleased;

            return obj;
        }

        private static UIElement CopyConstructContent(UIElement uielement)
        {
            var oriBitmap = ((Image)((Button)uielement).Content).Source as BitmapImage;

            var bitmap = new BitmapImage(new Uri(oriBitmap.UriSource.OriginalString, UriKind.RelativeOrAbsolute));
            var img = new Image() { Source = bitmap, Stretch = Stretch.Uniform, Height = 50, Width = 100, Margin = new Thickness(10) };

            return img;
        }

        private void ShowConstructOperationButtons()
        {
            this.ConstructMoveButton.Visibility = Visibility.Visible;
            this.ConstructCloneButton.Visibility = Visibility.Visible;
            this.ConstructDeleteButton.Visibility = Visibility.Visible;
            this.ConstructBringForwardButton.Visibility = Visibility.Visible;
            this.ConstructSendBackwardButton.Visibility = Visibility.Visible;
        }

        private void HideConstructOperationButtons()
        {
            this.ConstructMoveButton.Visibility = Visibility.Collapsed;
            this.ConstructCloneButton.Visibility = Visibility.Collapsed;
            this.ConstructDeleteButton.Visibility = Visibility.Collapsed;
            this.ConstructBringForwardButton.Visibility = Visibility.Collapsed;
            this.ConstructSendBackwardButton.Visibility = Visibility.Collapsed;
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

            Storyboard moveStory = new Storyboard();

            DoubleAnimation setLeft = new DoubleAnimation()
            {
                From = nowX,
                To = goToX,
                Duration = new Duration(TimeSpan.FromSeconds(timeToTravelDistance)),
                EasingFunction = _easingFunction,
            };

            DoubleAnimation setRight = new DoubleAnimation()
            {
                From = nowY,
                To = goToY,
                Duration = new Duration(TimeSpan.FromSeconds(timeToTravelDistance)),
                EasingFunction = _easingFunction,
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
        }

        #endregion

        #region Pointer
        private void Canvas_root_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_addingConstruct != null)
            {
                DrawConstructOnCanvas(
                    construct: _addingConstruct,
                    x: e.GetCurrentPoint(this.Canvas_root).Position.X,
                    y: e.GetCurrentPoint(this.Canvas_root).Position.Y);

                _addingConstruct = null;
            }
            else if (_isCloning && _cloningConstruct != null)
            {
                var constructAsset = ((Button)_cloningConstruct).Tag as Construct;

                if (constructAsset != null)
                {
                    Button construct = GenerateConstructButton(constructAsset);

                    DrawConstructOnCanvas(
                        construct: construct,
                        x: e.GetCurrentPoint(this.Canvas_root).Position.X,
                        y: e.GetCurrentPoint(this.Canvas_root).Position.Y);
                }
            }
            else if (_isMoving && _movingConstruct != null)
            {
                MoveElement(e, _movingConstruct);
            }
            else
            {
                _interactiveConstruct = null;
                InteractiveConstructHolder.Content = null;

                HideConstructOperationButtons();
            }
        }

        private void Construct_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            UIElement uielement = (UIElement)sender;
            _interactiveConstruct = uielement;
            ShowInteractiveConstruct(uielement);

            if (_addingConstruct != null)
            {
                DrawConstructOnCanvas(
                    construct: _addingConstruct,
                    x: e.GetCurrentPoint(this.Canvas_root).Position.X,
                    y: e.GetCurrentPoint(this.Canvas_root).Position.Y);

                _addingConstruct = null;
            }
            else if (_isCloning && _cloningConstruct != null)
            {
                var constructAsset = ((Button)_cloningConstruct).Tag as Construct;

                if (constructAsset != null)
                {
                    Button construct = GenerateConstructButton(constructAsset);

                    DrawConstructOnCanvas(
                        construct: construct,
                        x: e.GetCurrentPoint(this.Canvas_root).Position.X,
                        y: e.GetCurrentPoint(this.Canvas_root).Position.Y);
                }
            }
            else if (_isMoving && _movingConstruct != null)
            {
                MoveElement(e, _movingConstruct);
            }
            else if (_isCrafting)
            {
                ShowConstructOperationButtons();

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
            if (_isCrafting)
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
            if (_isMoving)
            {
                return;
            }

            if (_isCrafting)
            {
                UIElement uielement = (UIElement)sender;
                _isPointerCaptured = false;
                uielement.ReleasePointerCapture(e.Pointer);

                _interactiveConstruct = uielement;
                ShowInteractiveConstruct(uielement);
            }
        }

        #endregion

        #region Selection Status

        private void ShowInteractiveConstruct(UIElement uielement)
        {
            var construcButton = CopyConstructContent(uielement);
            InteractiveConstructHolder.Content = construcButton;
        }

        private void ShowOperationalConstruct(UIElement uielement, string operationStatus)
        {
            var historyButton = CopyConstructContent(uielement);

            OperationalConstructHolder.Content = historyButton;
            OperationalConstructStatus.Text = operationStatus;

        }

        #endregion

        #region Button Clicks

        private void CraftButton_Click(object sender, RoutedEventArgs e)
        {
            _isCrafting = !_isCrafting;
            this.CraftButton.Content = _isCrafting ? "Crafting" : "Craft";

            _isMoving = false;
            this.ConstructMoveButton.Content = "Move";

            _isCloning = false;
            this.ConstructCloneButton.Content = "Clone";

            _isDeleting = false;
            this.ConstructDeleteButton.Content = "Delete";

            if (!_isCrafting)
            {
                this.ConstructsAddButton.Visibility = Visibility.Collapsed;

                HideConstructOperationButtons();

                _movingConstruct = null;
                _cloningConstruct = null;

                OperationalConstructHolder.Content = null;
                OperationalConstructStatus.Text = null;
            }
            else
            {
                this.ConstructsAddButton.Visibility = Visibility.Visible;
                //this.ConstructDeleteButton.Visibility = Visibility.Visible;
            }
        }

        private void ConstructsAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ConstructAssets.Any())
            {
                ConstructAssets = JsonSerializer.Deserialize<ConstructAsset[]>(Properties.Resources.ConstructAssets).ToList();
                ConstructCategories = ConstructAssets.Select(x => x.Category).Distinct().Select(z => new ConstructCategory() { ImageUrl = @$"ms-appx:///Images/World_Objects/{z}.png", Name = z }).ToList();
            }

            var constructAssetPicker = new ConstructAssetPicker(
                constructAssets: ConstructAssets,
                constructCategories: ConstructCategories,
                assetSelected: (constructAsset) =>
                {
                    //TODO: adding new construct, fill World, Creator details
                    var construct = new Construct()
                    {
                        Id = UidGenerator.New(),
                        Name = constructAsset.Name,
                        ImageUrl = constructAsset.ImageUrl,
                    };

                    Button constructButton = GenerateConstructButton(construct);
                    _addingConstruct = constructButton;
                });

            constructAssetPicker.Show();
        }

        private void ConstructMoveButton_Click(object sender, RoutedEventArgs e)
        {
            _isMoving = !_isMoving;
            ConstructMoveButton.Content = _isMoving ? "Moving" : "Move";

            if (!_isMoving)
            {
                _movingConstruct = null;
                OperationalConstructHolder.Content = null;
                OperationalConstructStatus.Text = null;
            }
            else
            {
                UIElement uielement = _interactiveConstruct;
                _movingConstruct = uielement;
                ShowOperationalConstruct(_movingConstruct, "Moving");
            }
        }

        private void ConstructCloneButton_Click(object sender, RoutedEventArgs e)
        {
            _isCloning = !_isCloning;
            ConstructCloneButton.Content = _isCloning ? "Cloning" : "Clone";

            if (!_isCloning)
            {
                _cloningConstruct = null;
                OperationalConstructHolder.Content = null;
                OperationalConstructStatus.Text = null;
            }
            else
            {
                UIElement uielement = _interactiveConstruct;
                _cloningConstruct = uielement;
                ShowOperationalConstruct(_cloningConstruct, "Cloning");
            }
        }

        private void ConstructDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            //_isDeleting = !_isDeleting;
            //ConstructDeleteButton.Content = _isDeleting ? "Deleting" : "Delete";

            //var constructName = ((Button)_interactiveConstruct).Name;

            //var constructToDelete = Canvas_root.Children.Where(x => x is Button button && button.Name == constructName).FirstOrDefault();

            if (_interactiveConstruct != null)
            {
                Canvas_root.Children.Remove(_interactiveConstruct);
                InteractiveConstructHolder.Content = null;
            }
        }

        private void ConstructBringForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_interactiveConstruct != null)
            {
                var zIndex = Canvas.GetZIndex(_interactiveConstruct);
                zIndex++;
                Canvas.SetZIndex(_interactiveConstruct, zIndex);
            }
        }

        private void ConstructSendBackwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_interactiveConstruct != null)
            {
                var zIndex = Canvas.GetZIndex(_interactiveConstruct);
                zIndex--;
                Canvas.SetZIndex(_interactiveConstruct, zIndex);
            }
        }

        private void ConstructScaleUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (_interactiveConstruct != null)
            {

            }
        }

        private void ConstructScaleDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (_interactiveConstruct != null)
            {

            }
        }

        #endregion

        #endregion
    }
}
