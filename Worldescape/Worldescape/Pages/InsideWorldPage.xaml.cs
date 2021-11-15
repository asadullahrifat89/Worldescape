using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
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

        private readonly ILogger<InsideWorldPage> _logger;

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

        UIElement _avatar;

        UIElement _selectedConstruct;
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

        string[] avatarUrls = new string[]
        {
            "ms-appx:///Images/Avatar_Profiles/John_The_Seer/character_maleAdventurer_idle.png",
            "ms-appx:///Images/Avatar_Profiles/Jenna_The_Adventurer/character_femaleAdventurer_idle.png",
            "ms-appx:///Images/Avatar_Profiles/Rob_The_Robot/character_robot_idle.png",
            "ms-appx:///Images/Avatar_Profiles/Robert_The_Guardian/character_malePerson_idle.png",
            "ms-appx:///Images/Avatar_Profiles/Rodney_The_Messenger/character_femaleAdventurer_idle.png",
        };

        List<ConstructAsset> ConstructAssets = new List<ConstructAsset>();

        List<ConstructCategory> ConstructCategories = new List<ConstructCategory>();

        IHubService HubService;

        InWorld InWorld = new InWorld();

        User User = new User();

        Avatar Avatar = new Avatar();

        Character Character = new Character();

        ObservableCollection<AvatarMessenger> AvatarMessengers = new ObservableCollection<AvatarMessenger>();

        #endregion

        #region Ctor

        public InsideWorldPage(ILogger<InsideWorldPage> logger, IHubService hubService)
        {
            InitializeComponent();

            _logger = logger;
            HubService = hubService;

            DemoWorld();
            ListenOnHubService();
            TryConnectAndHubLogin();
        }

        private void DemoWorld()
        {
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
                ImageUrl = avatarUrls[new Random().Next(avatarUrls.Count())],
            };
        }

        #endregion

        #region Methods

        #region Hub Listener

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

            #region Construct

            HubService.NewBroadcastConstruct += HubService_NewBroadcastConstruct;
            HubService.NewBroadcastConstructs += HubService_NewBroadcastConstructs;
            HubService.NewRemoveConstruct += HubService_NewRemoveConstruct;
            HubService.NewRemoveConstructs += HubService_NewRemoveConstructs;
            HubService.NewBroadcastConstructPlacement += HubService_NewBroadcastConstructPlacement;
            HubService.NewBroadcastConstructRotation += HubService_NewBroadcastConstructRotation;
            HubService.NewBroadcastConstructRotations += HubService_NewBroadcastConstructRotations;
            HubService.NewBroadcastConstructScale += HubService_NewBroadcastConstructScale;
            HubService.NewBroadcastConstructScales += HubService_NewBroadcastConstructScales;
            HubService.NewBroadcastConstructMovement += HubService_NewBroadcastConstructMovement;

            #endregion
        }

        #region Construct
        private void HubService_NewBroadcastConstructMovement(int arg1, double arg2, double arg3, int arg4)
        {
            if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Construct taggedConstruct && taggedConstruct.Id == arg1) is UIElement iElement)
            {
                MoveElement(iElement, arg2, arg3, arg4);
            }
        }

        private void HubService_NewBroadcastConstructScales(int[] arg1, float arg2)
        {

        }

        private void HubService_NewBroadcastConstructScale(int arg1, float arg2)
        {

        }

        private void HubService_NewBroadcastConstructRotations(ConcurrentDictionary<int, float> obj)
        {

        }

        private void HubService_NewBroadcastConstructRotation(int arg1, float arg2)
        {

        }

        private void HubService_NewBroadcastConstructPlacement(int arg1, int arg2)
        {
            if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Construct taggedConstruct && taggedConstruct.Id == arg1) is UIElement iElement)
            {
                Canvas.SetZIndex(iElement, arg2);
            }
        }

        private void HubService_NewRemoveConstructs(int[] obj)
        {

        }

        private void HubService_NewRemoveConstruct(int obj)
        {
            if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Construct taggedConstruct && taggedConstruct.Id == obj) is UIElement iElement)
            {
                Canvas_root.Children.Remove(iElement);
            }
        }

        private void HubService_NewBroadcastConstructs(Construct[] obj)
        {

        }

        private void HubService_NewBroadcastConstruct(Construct obj)
        {
            var constructBtn = GenerateConstructButton(
                name: obj.Name,
                imageUrl: obj.ImageUrl,
                constructId: obj.Id,
                inWorld: obj.World,
                creator: obj.Creator);

            AddConstructOnCanvas(
                construct: constructBtn,
                x: obj.Coordinate.X,
                y: obj.Coordinate.Y,
                z: obj.Coordinate.Z);
        }
        #endregion

        #region Session
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
                    ParticipantsCount.Text = AvatarMessengers.Count().ToString();
                }

                Canvas_root.Children.Remove(iElement);
            }
        }

        private void HubService_AvatarLoggedIn(Avatar obj)
        {
            // Check if the avatar already exists in current world
            var iElement = Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Avatar taggedAvatar && taggedAvatar.Id == obj.Id);

            // If not then add a new avatar
            if (iElement == null)
            {
                AddAvatarOnCanvas(obj);
                AvatarMessengers.Add(new AvatarMessenger() { Avatar = obj, ActivityStatus = ActivityStatus.Online, IsLoggedIn = true });
                ParticipantsCount.Text = AvatarMessengers.Count().ToString();
            }
        }
        #endregion

        #region Avatar
        private void HubService_NewBroadcastAvatarActivityStatus(int avatarId, int activityStatus)
        {
            if (avatarId > 0)
            {
                if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Avatar taggedAvatar && taggedAvatar.Id == avatarId) is UIElement iElement)
                {
                    var avatarMessenger = AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == avatarId);
                    if (avatarMessenger != null)
                        avatarMessenger.ActivityStatus = (ActivityStatus)activityStatus;
                }
            }
        }

        private void HubService_NewBroadcastAvatarMovement(int avatarId, double x, double y, int z)
        {
            if (avatarId > 0)
            {
                if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Avatar taggedAvatar && taggedAvatar.Id == avatarId) is UIElement iElement)
                {
                    var avatarMessenger = AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == avatarId);
                    if (avatarMessenger != null)
                        avatarMessenger.ActivityStatus = ActivityStatus.Online;

                    MoveElement(uIElement: iElement, goToX: x, goToY: y);
                }
            }
        }
        #endregion

        #region Connection
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
        #endregion

        #region Hub Login
        private bool CanPerformWorldEvents()
        {
            return IsConnected && IsLoggedIn;
        }

        private bool CanHubLogin()
        {
            return Avatar != null && !Avatar.IsEmpty() && Avatar.User != null && IsConnected;
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

            //if (joined)
            //{
            //    DrawAvatarOnCanvas(Avatar);
            //}
            //else
            //{

            //}
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
                                AddAvatarOnCanvas(avatar);
                            }

                            ParticipantsCount.Text = AvatarMessengers.Count().ToString();
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
                                    var constructBtn = GenerateConstructButton(
                                      name: construct.Name,
                                      imageUrl: construct.ImageUrl,
                                      constructId: construct.Id,
                                      inWorld: construct.World,
                                      creator: construct.Creator);

                                    AddConstructOnCanvas(
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

        #endregion

        #region Pointer Events

        /// <summary>
        /// Event fired on pointer press on canvas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Canvas_root_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_addingConstruct != null)
            {
                var construct = AddConstructOnCanvas(
                    construct: _addingConstruct,
                    x: e.GetCurrentPoint(Canvas_root).Position.X,
                    y: e.GetCurrentPoint(Canvas_root).Position.Y);

                _addingConstruct = null;

                await HubService.BroadcastConstructAsync(construct);
            }
            else if (_isCloningConstruct && _cloningConstruct != null)
            {
                var constructAsset = ((Button)_cloningConstruct).Tag as Construct;

                if (constructAsset != null)
                {
                    var constructBtn = GenerateConstructButton(
                        name: constructAsset.Name,
                        imageUrl: constructAsset.ImageUrl);

                    var construct = AddConstructOnCanvas(
                        construct: constructBtn,
                        x: e.GetCurrentPoint(Canvas_root).Position.X,
                        y: e.GetCurrentPoint(Canvas_root).Position.Y);

                    await HubService.BroadcastConstructAsync(construct);
                }
            }
            else if (_isMovingConstruct && _movingConstruct != null)
            {
                var taggedObject = MoveElement(_movingConstruct, e);

                var construct = taggedObject as Construct;

                await HubService.BroadcastConstructMovementAsync(construct.Id, construct.Coordinate.X, construct.Coordinate.Y, construct.Coordinate.Z);
            }
            else
            {
                // Clear construct selection
                _selectedConstruct = null;
                SelectedConstructHolder.Content = null;

                HideConstructOperationButtons();
            }
        }

        /// <summary>
        /// Event fired when pointer is pointer is pressed on a construct. The construct latches on to the pointer if press is not released.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Construct_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            UIElement uielement = (UIElement)sender;
            _selectedConstruct = uielement;
            ShowInteractiveConstruct(uielement);

            if (_addingConstruct != null)
            {
                var construct = AddConstructOnCanvas(
                    construct: _addingConstruct,
                    x: e.GetCurrentPoint(Canvas_root).Position.X,
                    y: e.GetCurrentPoint(Canvas_root).Position.Y);

                _addingConstruct = null;

                await HubService.BroadcastConstructAsync(construct);
            }
            else if (_isCloningConstruct && _cloningConstruct != null)
            {
                var constructAsset = ((Button)_cloningConstruct).Tag as Construct;

                if (constructAsset != null)
                {
                    var constructBtn = GenerateConstructButton(
                        name: constructAsset.Name,
                        imageUrl: constructAsset.ImageUrl);

                    var construct = AddConstructOnCanvas(
                        construct: constructBtn,
                        x: e.GetCurrentPoint(Canvas_root).Position.X,
                        y: e.GetCurrentPoint(Canvas_root).Position.Y);

                    await HubService.BroadcastConstructAsync(construct);
                }
            }
            else if (_isMovingConstruct && _movingConstruct != null)
            {
                var taggedObject = MoveElement(_movingConstruct, e);

                var construct = taggedObject as Construct;

                await HubService.BroadcastConstructMovementAsync(construct.Id, construct.Coordinate.X, construct.Coordinate.Y, construct.Coordinate.Z);
            }
            else if (_isCraftingConstruct)
            {
                ShowConstructOperationButtons();

                // Drag start of a constuct
                _objectLeft = Canvas.GetLeft(uielement);
                _objectTop = Canvas.GetTop(uielement);

                // Remember the pointer position:
                _pointerX = e.GetCurrentPoint(Canvas_root).Position.X;
                _pointerY = e.GetCurrentPoint(Canvas_root).Position.Y;

                uielement.CapturePointer(e.Pointer);

                _isPointerCaptured = true;
            }
            else
            {
                // Move avatar
                if (Canvas_root.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar avatar && avatar.Id == Avatar.Id) is UIElement iElement)
                {
                    var taggedObject = MoveElement(iElement, e);

                    var movedAvatar = taggedObject as Avatar;

                    var z = Canvas.GetZIndex(iElement);

                    await HubService.BroadcastAvatarMovementAsync(avatarId: movedAvatar.Id, x: movedAvatar.Coordinate.X, y: movedAvatar.Coordinate.Y, z: z);
                }
            }
        }

        /// <summary>
        /// Event fired when pointer is moved within a construct. The construct latches on to the pointer if press is not released.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Construct_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isCraftingConstruct)
            {
                UIElement uielement = (UIElement)sender;

                if (_isPointerCaptured)
                {
                    // Calculate the new position of the object:
                    double deltaH = e.GetCurrentPoint(Canvas_root).Position.X - _pointerX;
                    double deltaV = e.GetCurrentPoint(Canvas_root).Position.Y - _pointerY;

                    _objectLeft = deltaH + _objectLeft;
                    _objectTop = deltaV + _objectTop;

                    // Update the object position:
                    Canvas.SetLeft(uielement, _objectLeft);
                    Canvas.SetTop(uielement, _objectTop);

                    // Remember the pointer position:
                    _pointerX = e.GetCurrentPoint(Canvas_root).Position.X;
                    _pointerY = e.GetCurrentPoint(Canvas_root).Position.Y;
                }
            }
        }

        /// <summary>
        /// Event fired when pointer is released from a construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Construct_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_isMovingConstruct)
            {
                return;
            }

            if (_isCraftingConstruct)
            {
                // Drag release of a construct
                UIElement uielement = (UIElement)sender;
                _isPointerCaptured = false;
                uielement.ReleasePointerCapture(e.Pointer);

                _selectedConstruct = uielement;
                ShowInteractiveConstruct(uielement);

                var x = Canvas.GetLeft(uielement);
                var y = Canvas.GetTop(uielement);
                var z = Canvas.GetZIndex(uielement);

                var construct = ((Button)uielement).Tag as Construct;

                construct.Coordinate.X = x;
                construct.Coordinate.Y = y;
                construct.Coordinate.Z = z;

                await HubService.BroadcastConstructMovementAsync(construct.Id, construct.Coordinate.X, construct.Coordinate.Y, construct.Coordinate.Z);
            }
        }

        #endregion

        #region Button Events

        private void CraftButton_Click(object sender, RoutedEventArgs e)
        {
            _isCraftingConstruct = !_isCraftingConstruct;
            CraftButton.Content = _isCraftingConstruct ? "Crafting" : "Craft";

            _isMovingConstruct = false;
            ConstructMoveButton.Content = "Move";

            _isCloningConstruct = false;
            ConstructCloneButton.Content = "Clone";

            //_isDeleting = false;
            ConstructDeleteButton.Content = "Delete";

            if (!_isCraftingConstruct)
            {
                ConstructsAddButton.Visibility = Visibility.Collapsed;

                HideConstructOperationButtons();

                _movingConstruct = null;
                _cloningConstruct = null;

                OperationalConstructHolder.Content = null;
                OperationalConstructStatus.Text = null;
            }
            else
            {
                ConstructsAddButton.Visibility = Visibility.Visible;
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
                    var constructBtn = GenerateConstructButton(
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
                UIElement uielement = _selectedConstruct;
                _movingConstruct = uielement;
                ShowOperationalConstruct(_movingConstruct, "Moving");
            }
        }

        /// <summary>
        /// Starts cloning a selected construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                UIElement uielement = _selectedConstruct;
                _cloningConstruct = uielement;
                ShowOperationalConstruct(_cloningConstruct, "Cloning");
            }
        }


        /// <summary>
        /// Deletes a selected construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConstructDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            //_isDeleting = !_isDeleting;
            //ConstructDeleteButton.Content = _isDeleting ? "Deleting" : "Delete";

            //var constructName = ((Button)_interactiveConstruct).Name;

            //var constructToDelete = Canvas_root.Children.Where(x => x is Button button && button.Name == constructName).FirstOrDefault();

            if (_selectedConstruct != null)
            {
                var construct = ((Button)_selectedConstruct).Tag as Construct;

                Canvas_root.Children.Remove(_selectedConstruct);
                SelectedConstructHolder.Content = null;

                await HubService.RemoveConstructAsync(construct.Id);
            }
        }

        private async void ConstructBringForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedConstruct != null)
            {
                var zIndex = Canvas.GetZIndex(_selectedConstruct);
                zIndex++;
                Canvas.SetZIndex(_selectedConstruct, zIndex);

                var construct = ((Button)_selectedConstruct).Tag as Construct;
                await HubService.BroadcastConstructPlacementAsync(construct.Id, zIndex);
            }
        }

        private async void ConstructSendBackwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedConstruct != null)
            {
                var zIndex = Canvas.GetZIndex(_selectedConstruct);
                zIndex--;
                Canvas.SetZIndex(_selectedConstruct, zIndex);

                var construct = ((Button)_selectedConstruct).Tag as Construct;
                await HubService.BroadcastConstructPlacementAsync(construct.Id, zIndex);
            }
        }

        private void ConstructScaleUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedConstruct != null)
            {

            }
        }

        private void ConstructScaleDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedConstruct != null)
            {

            }
        }

        #endregion

        #region Construct Labels

        private void ShowInteractiveConstruct(UIElement uielement)
        {
            var construcButton = CopyConstructContent(uielement);
            SelectedConstructHolder.Content = construcButton;
        }

        private void ShowOperationalConstruct(UIElement uielement, string operationStatus)
        {
            var historyButton = CopyConstructContent(uielement);

            OperationalConstructHolder.Content = historyButton;
            OperationalConstructStatus.Text = operationStatus;

        }

        #endregion

        #region Common

        /// <summary>
        /// Adds an avatar on canvas.
        /// </summary>
        /// <param name="avatar"></param>
        private void AddAvatarOnCanvas(Avatar avatar)
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

            Button avatarBtn = new Button() { Style = Application.Current.Resources["MaterialDesign_HyperlinkButton_Style"] as Style };

            avatarBtn.Content = img;
            avatarBtn.Tag = avatar;

            //avatar.Effect = new DropShadowEffect() { ShadowDepth = 3, Color = Colors.Black, BlurRadius = 10, Opacity = 0.3 };

            Canvas.SetLeft(avatarBtn, avatar.Coordinate.X);
            Canvas.SetTop(avatarBtn, avatar.Coordinate.Y);
            Canvas.SetZIndex(avatarBtn, avatar.Coordinate.Z);

            Canvas_root.Children.Add(avatarBtn);
            _avatar = avatarBtn;
        }

        /// <summary>
        /// Adds a construct to the canvas.
        /// </summary>
        /// <param name="construct"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private Construct AddConstructOnCanvas(UIElement construct, double x, double y, int? z = null)
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

            return taggedConstruct;
        }

        /// <summary>
        /// Generate a new button from the provided construct. If constructId is not provided then new id is generated. If inWorld, creator are not provided then current world and user are tagged.
        /// </summary>
        /// <param name="construct"></param>
        /// <param name="constructId"></param>
        /// <returns></returns>
        private Button GenerateConstructButton(string name, string imageUrl, int? constructId = null, InWorld inWorld = null, Creator creator = null)
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
                    Creator = creator ?? new Creator() { Id = User.Id, Name = User.Name },
                    World = inWorld ?? new InWorld() { Id = InWorld.Id, Name = InWorld.Name }
                }
            };

            obj.Content = img;

            //obj.AllowScrollOnTouchMove = false;

            obj.PointerPressed += Construct_PointerPressed;
            obj.PointerMoved += Construct_PointerMoved;
            obj.PointerReleased += Construct_PointerReleased;

            return obj;
        }

        /// <summary>
        /// Copies the image content from an UIElement and returns it as an Image.
        /// </summary>
        /// <param name="uielement"></param>
        /// <returns></returns>
        private static UIElement CopyConstructContent(UIElement uielement)
        {
            var oriBitmap = ((Image)((Button)uielement).Content).Source as BitmapImage;

            var bitmap = new BitmapImage(new Uri(oriBitmap.UriSource.OriginalString, UriKind.RelativeOrAbsolute));
            var img = new Image() { Source = bitmap, Stretch = Stretch.Uniform, Height = 50, Width = 100, Margin = new Thickness(10) };

            return img;
        }

        /// <summary>
        /// Shows construct operational buttons on the UI.
        /// </summary>
        private void ShowConstructOperationButtons()
        {
            ConstructMoveButton.Visibility = Visibility.Visible;
            ConstructCloneButton.Visibility = Visibility.Visible;
            ConstructDeleteButton.Visibility = Visibility.Visible;
            ConstructBringForwardButton.Visibility = Visibility.Visible;
            ConstructSendBackwardButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides construct operational buttons on the UI.
        /// </summary>
        private void HideConstructOperationButtons()
        {
            ConstructMoveButton.Visibility = Visibility.Collapsed;
            ConstructCloneButton.Visibility = Visibility.Collapsed;
            ConstructDeleteButton.Visibility = Visibility.Collapsed;
            ConstructBringForwardButton.Visibility = Visibility.Collapsed;
            ConstructSendBackwardButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Moves an UIElement to a new coordinate with the provided PointerRoutedEventArgs in canvas. Returns the tagged object of the uIElement.
        /// </summary>
        /// <param name="uIElement"></param>
        /// <param name="goToX"></param>
        /// <param name="goToY"></param>
        /// <returns></returns>
        private object MoveElement(UIElement uIElement, PointerRoutedEventArgs e)
        {
            var goToX = e.GetCurrentPoint(Canvas_root).Position.X;
            var goToY = e.GetCurrentPoint(Canvas_root).Position.Y;
            //var gotoZ = Canvas.GetZIndex(uIElement);

            var taggedObject = MoveElement(uIElement, goToX, goToY);

            return taggedObject;
        }

        /// <summary>
        /// Moves an UIElement to the provided goToX and goToY coordinate in canvas. Returns the tagged object of the uIElement.
        /// </summary>
        /// <param name="uIElement"></param>
        /// <param name="goToX"></param>
        /// <param name="goToY"></param>
        /// <returns></returns>
        private object MoveElement(UIElement uIElement, double goToX, double goToY, int? gotoZ = null)
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

            object taggedObject = null;

            if (((Button)uIElement).Tag is Construct taggedConstruct)
            {
                taggedConstruct.Coordinate.X = goToX;
                taggedConstruct.Coordinate.Y = goToY;

                if (gotoZ.HasValue)
                {
                    taggedConstruct.Coordinate.Z = (int)gotoZ;
                    Canvas.SetZIndex(uIElement, (int)gotoZ);
                }

                taggedObject = taggedConstruct;
            }
            else if (((Button)uIElement).Tag is Avatar taggedAvatar)
            {
                taggedAvatar.Coordinate.X = goToX;
                taggedAvatar.Coordinate.Y = goToY;

                if (gotoZ.HasValue)
                {
                    taggedAvatar.Coordinate.Z = (int)gotoZ;
                    Canvas.SetZIndex(uIElement, (int)gotoZ);
                }

                taggedObject = taggedAvatar;
            }

            return taggedObject;
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

        #endregion
    }
}
