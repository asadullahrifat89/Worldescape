using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Contracts.Services;
using Worldescape.Internals;
using Worldescape.Shared;
using Worldescape.Shared.Entities;
using Worldescape.Shared.Models;
using Worldescape.Shared.Requests;
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

        bool _isCraftingConstruct;
        bool _isMovingConstruct;
        bool _isCloningConstruct;
        //bool _isDeleting;

        bool IsConnected;
        bool IsLoggedIn;

        Button avatarBtn = new Button() { Style = Application.Current.Resources["MaterialDesign_HyperlinkButton_Style"] as Style };

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

        IWorldescapeHubService HubService;

        InWorld InWorld = new InWorld();

        User User = new User();

        Avatar Avatar = new Avatar();

        Character Character = new Character();

        SynchronizationContext synchronizationContext;

        ObservableCollection<AvatarMessenger> AvatarMessengers = new ObservableCollection<AvatarMessenger>();

        #endregion

        #region Ctor

        public InsideWorldPage()
        {
            this.InitializeComponent();

            synchronizationContext = SynchronizationContext.Current;

            //DrawRandomConstructsOnCanvas();

            InWorld = new InWorld() { Id = 1, Name = "Test World" };
            User = new User() { Id = UidGenerator.New(), Name = "Test User" };
            Avatar = new Avatar()
            {
                Id = UidGenerator.New(),
                ActivityStatus = ActivityStatus.Online,
                User = new AvatarUser()
                {
                    Email = User.Email,
                    Id = User.Id,
                    ImageUrl = User.ImageUrl,
                    Name = User.Name,
                    Phone = User.Phone,
                    ProfilePictureUrl = User.ImageUrl
                },
                Character = new AvatarCharacter()
                {
                    Id = Character.Id,
                    Name = Character.Name,
                    ImageUrl = Character.ImageUrl,
                },
                World = InWorld,
                Coordinate = new Coordinate(new Random().Next(500), new Random().Next(500), new Random().Next(500)),
                ImageUrl = avatarUrl,
            };

            HubService = App.ServiceProvider.GetService(typeof(IWorldescapeHubService)) as IWorldescapeHubService;

            ListenOnHubService();

            TryConnectAndHubLogin();
        }

        #endregion

        #region Methods

        #region Common

        private void DrawAvatarOnCanvas(Avatar avatar)
        {
            var uri = avatar.ImageUrl;

            var bitmap = new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));

            var img = new Image()
            {
                Source = bitmap,
                Stretch = Stretch.Uniform,
                Height = 100,
                Width = 100,
            };

            avatarBtn.Content = img;
            avatarBtn.Tag = avatar;

            //avatar.Effect = new DropShadowEffect() { ShadowDepth = 3, Color = Colors.Black, BlurRadius = 10, Opacity = 0.3 };

            Canvas.SetLeft(avatarBtn, avatar.Coordinate.X);
            Canvas.SetTop(avatarBtn, avatar.Coordinate.Y);
            Canvas.SetZIndex(avatarBtn, avatar.Coordinate.Z);

            this.Canvas_root.Children.Add(avatarBtn);
        }

        /// <summary>
        /// Adds a construct to the canvas.
        /// </summary>
        /// <param name="construct"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private void DrawConstructOnCanvas(UIElement construct, double x, double y, int? z = null)
        {
            Canvas.SetLeft(construct, x);
            Canvas.SetTop(construct, y);

            if (z.HasValue)
            {
                Canvas.SetZIndex(construct, (int)z);
            }

            Canvas_root.Children.Add(construct);

            var taggedConstruct = ((Button)construct).Tag as Construct;

            taggedConstruct.Coordinate.X = x;
            taggedConstruct.Coordinate.Y = y;

            if (z.HasValue)
            {
                taggedConstruct.Coordinate.Z = (int)z;
            }
        }

        /// <summary>
        /// Generate a new button from the provided construct. If constructId is not provided then new id is generated.
        /// </summary>
        /// <param name="construct"></param>
        /// <param name="constructId"></param>
        /// <returns></returns>
        private Button GenerateConstructButton(string name, string imageUrl, int? constructId = null)
        {
            var uri = imageUrl;

            var bitmap = new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));

            var img = new Image() { Source = bitmap, Stretch = Stretch.None };

            // This is broadcasted and saved in database
            var id = constructId ?? UidGenerator.New();

            //TODO: adding new construct, fill World, Creator details

            var obj = new Button()
            {
                BorderBrush = new SolidColorBrush(Colors.DodgerBlue),
                Style = Application.Current.Resources["MaterialDesign_ConstructButton_Style"] as Style,
                Name = id.ToString(),
                Tag = new Construct()
                {
                    Id = id,
                    Name = name,
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

        private void MoveElement(PointerRoutedEventArgs e, UIElement uIElement)
        {
            var goToX = e.GetCurrentPoint(this.Canvas_root).Position.X;
            var goToY = e.GetCurrentPoint(this.Canvas_root).Position.Y;

            MoveElement(uIElement, goToX, goToY);
        }

        private void MoveElement(UIElement uIElement, double goToX, double goToY)
        {
            var nowX = Canvas.GetLeft(uIElement);
            var nowY = Canvas.GetTop(uIElement);

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


        //private void DrawRandomConstructsOnCanvas()
        //{
        //    for (int j = 0; j < 5; j++)
        //    {
        //        for (int i = 0; i < 10; i++)
        //        {
        //            var uri = _objects[new Random().Next(_objects.Count())];

        //            Button constructBtn = GenerateConstructButton(
        //                name: Guid.NewGuid().ToString(),
        //                imageUrl: uri);

        //            var x = (i + j * 2) * 200;
        //            var y = i * 200;

        //            DrawConstructOnCanvas(constructBtn, x, y);
        //        }
        //    }
        //}

        #endregion

        #region Hub

        private void ListenOnHubService()
        {
            #region Connection

            HubService.ConnectionReconnecting += HubService_ConnectionReconnecting;
            HubService.ConnectionReconnected += HubService_ConnectionReconnected;
            HubService.ConnectionClosed += HubService_ConnectionClosed;

            #endregion

            #region Avatar

            HubService.NewBroadcastAvatarMovement += HubService_NewBroadcastAvatarMovement;
            HubService.NewBroadcastAvatarActivityStatus += HubService_NewBroadcastAvatarActivityStatus;

            #endregion

            #region Session

            HubService.AvatarLoggedIn += HubService_AvatarLoggedIn;
            HubService.AvatarLoggedOut += HubService_AvatarLoggedOut;
            HubService.AvatarDisconnected += HubService_AvatarDisconnected;
            HubService.AvatarReconnected += HubService_AvatarReconnected;

            #endregion
        }

        private void HubService_AvatarReconnected(int obj)
        {
            if (obj > 0)
            {
                if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Avatar taggedAvatar && taggedAvatar.Id == obj) is UIElement iElement)
                {
                    var avatarMessenger = AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == obj);

                    if (avatarMessenger != null)
                    {
                        avatarMessenger.ActivityStatus = ActivityStatus.Online;
                        avatarMessenger.IsLoggedIn = true;
                    }
                }
            }
        }

        private void HubService_AvatarDisconnected(int obj)
        {
            if (obj > 0)
            {
                if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Avatar taggedAvatar && taggedAvatar.Id == obj) is UIElement iElement)
                {
                    var avatarMessenger = AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == obj);

                    if (avatarMessenger != null)
                    {
                        avatarMessenger.ActivityStatus = ActivityStatus.Offline;
                        avatarMessenger.IsLoggedIn = false;
                    }
                }
            }
        }

        private void HubService_AvatarLoggedOut(int obj)
        {
            if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Avatar taggedAvatar && taggedAvatar.Id == obj) is UIElement iElement)
            {
                var avatarMessenger = AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == obj);

                if (avatarMessenger != null)
                {
                    AvatarMessengers.Remove(avatarMessenger);
                }

                Canvas_root.Children.Remove(iElement);
            }

        }

        private void HubService_AvatarLoggedIn(Avatar obj)
        {
            // Check if the avatar already exists in current world
            var avatarMessenger = AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == obj.Id);

            // If not then add a new avatar
            if (avatarMessenger == null)
            {
                DrawAvatarOnCanvas(obj);

                AvatarMessengers.Add(new AvatarMessenger() { Avatar = obj, ActivityStatus = ActivityStatus.Online, IsLoggedIn = true });
            }
        }

        private void HubService_NewBroadcastAvatarActivityStatus(BroadcastAvatarActivityStatusRequest obj)
        {
            if (obj != null)
            {
                if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Avatar taggedAvatar && taggedAvatar.Id == obj.AvatarId) is UIElement iElement)
                {
                    var avatarMessenger = AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == obj.AvatarId);
                    if (avatarMessenger != null)
                        avatarMessenger.ActivityStatus = obj.ActivityStatus;
                }
            }
        }

        private void HubService_NewBroadcastAvatarMovement(BroadcastAvatarMovementRequest obj)
        {
            if (obj != null)
            {
                if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Avatar taggedAvatar && taggedAvatar.Id == obj.AvatarId) is UIElement iElement)
                {
                    var avatarMessenger = AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == obj.AvatarId);
                    if (avatarMessenger != null)
                        avatarMessenger.ActivityStatus = ActivityStatus.Online;

                    MoveElement(uIElement: iElement, goToX: obj.Coordinate.X, goToY: obj.Coordinate.Y);
                }
            }
        }

        private async void HubService_ConnectionClosed()
        {
            IsConnected = false;
            IsLoggedIn = false;

            if (await TryConnect())
            {
                await TryHubLogin();
            }
        }

        private async void HubService_ConnectionReconnected()
        {
            _ = await HubService.LoginAsync(Avatar);

            IsConnected = true;
            IsLoggedIn = true;
        }

        private void HubService_ConnectionReconnecting()
        {
            IsConnected = false;
            IsLoggedIn = false;
        }

        private bool CanHubLogin()
        {
            return Avatar != null && !Avatar.IsEmpty() && Avatar.User != null && IsConnected;
        }

        private bool CanPerformWorldEvents()
        {
            return IsConnected && IsLoggedIn;
        }

        public async Task TryConnectAndHubLogin()
        {
            if (await TryConnect())
            {
                await TryHubLogin();
            }
            else
            {
                //synchronizationContext.Post(async (_) =>
                //{
                //    ConsentContentDialog contentDialog = new("Connection failure", "Would you like to try again?");
                //    contentDialog.Selected += async (sender, e) =>
                //    {
                //        await TryConnect_TryLoginToHubService();
                //    };

                //    _ = await contentDialog.ShowAsync();
                //}, null);
            }
        }

        private async Task<bool> TryConnect()
        {
            try
            {
                if (IsConnected)
                {
                    return true;
                }

                await HubService.ConnectAsync();
                IsConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private async Task TryHubLogin()
        {
            bool joined = await HubLogin();

            if (joined)
            {
                DrawAvatarOnCanvas(Avatar);
            }
            else
            {

            }
        }

        private async Task<bool> HubLogin()
        {
            try
            {
                if (CanHubLogin())
                {
                    var result = await HubService.LoginAsync(Avatar);

                    if (result != null)
                    {
                        var avatars = result.Item1;

                        if (avatars != null && avatars.Any())
                        {
                            foreach (var avatar in avatars.Where(x => !AvatarMessengers.Select(z => z.Avatar.Id).Contains(x.Id)))
                            {
                                AvatarMessengers.Add(new AvatarMessenger { Avatar = avatar, IsLoggedIn = true });
                                DrawAvatarOnCanvas(avatar);
                            }
                        }

                        var constructs = result.Item2;

                        if (constructs != null && constructs.Any())
                        {
                            foreach (var construct in constructs)
                            {
                                if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Construct taggedConstruct && taggedConstruct.Id == construct.Id) is UIElement iElement)
                                {
                                    Canvas.SetZIndex(iElement, construct.Coordinate.Z);
                                    MoveElement(iElement, construct.Coordinate.X, construct.Coordinate.Y);

                                    // TODO: set scale and rotation
                                }
                                else // insert new constructs
                                {
                                    Button constructBtn = GenerateConstructButton(
                                      name: construct.Name,
                                      imageUrl: construct.ImageUrl,
                                      constructId: construct.Id);

                                    DrawConstructOnCanvas(
                                        construct: constructBtn,
                                        x: construct.Coordinate.X,
                                        y: construct.Coordinate.Y,
                                        z: construct.Coordinate.Z);
                                }
                            }
                        }

                        IsLoggedIn = true;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
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
            else if (_isCloningConstruct && _cloningConstruct != null)
            {
                var constructAsset = ((Button)_cloningConstruct).Tag as Construct;

                if (constructAsset != null)
                {
                    Button constructBtn = GenerateConstructButton(
                        name: constructAsset.Name,
                        imageUrl: constructAsset.ImageUrl);

                    DrawConstructOnCanvas(
                        construct: constructBtn,
                        x: e.GetCurrentPoint(this.Canvas_root).Position.X,
                        y: e.GetCurrentPoint(this.Canvas_root).Position.Y);
                }
            }
            else if (_isMovingConstruct && _movingConstruct != null)
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
            else if (_isCloningConstruct && _cloningConstruct != null)
            {
                var constructAsset = ((Button)_cloningConstruct).Tag as Construct;

                if (constructAsset != null)
                {
                    Button constructBtn = GenerateConstructButton(
                        name: constructAsset.Name,
                        imageUrl: constructAsset.ImageUrl);

                    DrawConstructOnCanvas(
                        construct: constructBtn,
                        x: e.GetCurrentPoint(this.Canvas_root).Position.X,
                        y: e.GetCurrentPoint(this.Canvas_root).Position.Y);
                }
            }
            else if (_isMovingConstruct && _movingConstruct != null)
            {
                MoveElement(e, _movingConstruct);
            }
            else if (_isCraftingConstruct)
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
                MoveElement(e, avatarBtn);
            }
        }

        private void Construct_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isCraftingConstruct)
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
            if (_isMovingConstruct)
            {
                return;
            }

            if (_isCraftingConstruct)
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
            _isCraftingConstruct = !_isCraftingConstruct;
            this.CraftButton.Content = _isCraftingConstruct ? "Crafting" : "Craft";

            _isMovingConstruct = false;
            this.ConstructMoveButton.Content = "Move";

            _isCloningConstruct = false;
            this.ConstructCloneButton.Content = "Clone";

            //_isDeleting = false;
            this.ConstructDeleteButton.Content = "Delete";

            if (!_isCraftingConstruct)
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
                    Button constructBtn = GenerateConstructButton(
                        name: constructAsset.Name,
                        imageUrl: constructAsset.ImageUrl);

                    _addingConstruct = constructBtn;
                });

            constructAssetPicker.Show();
        }

        private void ConstructMoveButton_Click(object sender, RoutedEventArgs e)
        {
            _isMovingConstruct = !_isMovingConstruct;
            ConstructMoveButton.Content = _isMovingConstruct ? "Moving" : "Move";

            if (!_isMovingConstruct)
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
            _isCloningConstruct = !_isCloningConstruct;
            ConstructCloneButton.Content = _isCloningConstruct ? "Cloning" : "Clone";

            if (!_isCloningConstruct)
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
