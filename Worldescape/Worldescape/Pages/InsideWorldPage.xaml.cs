using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media.Effects;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Service;
using Worldescape.Shared;
using Image = Windows.UI.Xaml.Controls.Image;

namespace Worldescape
{
    public partial class InsideWorldPage : Page
    {
        #region Fields

        bool _isPointerCaptured;
        double _pointerX;
        double _pointerY;
        double _objectLeft;
        double _objectTop;

        bool _isLoggedIn;

        UIElement _selectedConstruct;
        UIElement _addingConstruct;
        UIElement _movingConstruct;
        UIElement _cloningConstruct;
        UIElement _selectedAvatar;

        UIElement _messageToAvatar;

        readonly IHubService HubService;

        readonly AssetUrlHelper _assetUriHelper;

        EasingFunctionBase _easingFunction = new ExponentialEase()
        {
            EasingMode = EasingMode.EaseOut,
            Exponent = 5,
        };

        List<ConstructAsset> ConstructAssets = new List<ConstructAsset>();

        List<ConstructCategory> ConstructCategories = new List<ConstructCategory>();

        List<Character> Characters = new List<Character>();

        InWorld InWorld = new InWorld();

        User User = new User();

        Avatar Avatar = null;

        Character Character = new Character();

        ObservableCollection<AvatarMessenger> AvatarMessengers = new ObservableCollection<AvatarMessenger>();

        #endregion

        #region Ctor

        public InsideWorldPage(
            IHubService hubService,
            AssetUrlHelper assetUriHelper)
        {
            InitializeComponent();

            HubService = hubService;// App.ServiceProvider.GetService(typeof(IHubService)) as IHubService;
            _assetUriHelper = assetUriHelper;

            SubscribeHub();
        }

        #endregion

        #region Methods

        #region Hub Events

        #region Construct
        private void HubService_NewBroadcastConstructMovement(int constructId, double x, double y, int z)
        {
            if (Canvas_root.Children.FirstOrDefault(c => c is Button button && button.Tag is Construct taggedConstruct && taggedConstruct.Id == constructId) is UIElement iElement)
            {
                MoveElement(uIElement: iElement, goToX: x, goToY: y, gotoZ: z);
                Console.WriteLine("<<HubService_NewBroadcastConstructMovement: OK");
            }
            else
            {
                Console.WriteLine("<<HubService_NewBroadcastConstructMovement: IGNORE");
            }
        }

        private void HubService_NewBroadcastConstructScale(int constructId, float scale)
        {
            if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Construct taggedConstruct && taggedConstruct.Id == constructId) is UIElement iElement)
            {
                ScaleElement(uIElement: iElement, scale: scale);
                Console.WriteLine("<<HubService_NewBroadcastConstructScale: OK");
            }
            else
            {
                Console.WriteLine("<<HubService_NewBroadcastConstructScale: IGNORE");
            }
        }

        private void HubService_NewBroadcastConstructScales(int[] constructIds, float scale)
        {

        }

        private void HubService_NewBroadcastConstructRotation(int constructId, float rotation)
        {
            if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Construct taggedConstruct && taggedConstruct.Id == constructId) is UIElement iElement)
            {
                RotateElement(uIElement: iElement, rotation: rotation);
                Console.WriteLine("<<HubService_NewBroadcastConstructRotation: OK");
            }
            else
            {
                Console.WriteLine("<<HubService_NewBroadcastConstructRotation: IGNORE");
            }
        }

        private void HubService_NewBroadcastConstructRotations(ConcurrentDictionary<int, float> obj)
        {

        }

        private void HubService_NewBroadcastConstructPlacement(int constructId, int z)
        {
            if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Construct taggedConstruct && taggedConstruct.Id == constructId) is UIElement iElement)
            {
                Canvas.SetZIndex(iElement, z);
                Console.WriteLine("<<HubService_NewBroadcastConstructPlacement: OK");
            }
            else
            {
                Console.WriteLine("<<HubService_NewBroadcastConstructPlacement: IGNORE");
            }
        }

        private void HubService_NewRemoveConstruct(int constructId)
        {
            if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Construct taggedConstruct && taggedConstruct.Id == constructId) is UIElement iElement)
            {
                Canvas_root.Children.Remove(iElement);
                Console.WriteLine("<<HubService_NewRemoveConstruct: OK");
            }
            else
            {
                Console.WriteLine("<<HubService_NewRemoveConstruct: IGNORE");
            }
        }

        private void HubService_NewRemoveConstructs(int[] obj)
        {

        }

        private void HubService_NewBroadcastConstruct(Construct construct)
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

            ScaleElement(constructBtn, construct.Scale);
            RotateElement(constructBtn, construct.Rotation);

            Console.WriteLine("<<HubService_NewBroadcastConstruct: OK");
        }

        private void HubService_NewBroadcastConstructs(Construct[] obj)
        {

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

                    Console.WriteLine("<<HubService_AvatarReconnected: OK");
                }
                else
                {
                    Console.WriteLine("<<HubService_AvatarReconnected: IGNORE");
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

                    Console.WriteLine("<<HubService_AvatarDisconnected: OK");
                }
                else
                {
                    Console.WriteLine("<<HubService_AvatarDisconnected: IGNORE");
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

                Console.WriteLine("<<HubService_AvatarLoggedOut: OK");
            }
            else
            {
                Console.WriteLine("<<HubService_AvatarLoggedOut: IGNORE");
            }
        }

        private void HubService_AvatarLoggedIn(Avatar avatar)
        {
            // If an avatar already exists, ignore
            if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Avatar taggedAvatar && taggedAvatar.Id == avatar.Id) is UIElement iElement)
            {
                Console.WriteLine("<<HubService_AvatarLoggedIn: IGNORE");
            }
            else
            {
                var avatarButton = GenerateAvatarButton(avatar);
                AddAvatarOnCanvas(avatarButton, avatar.Coordinate.X, avatar.Coordinate.Y, avatar.Coordinate.Z);

                AvatarMessengers.Add(new AvatarMessenger() { Avatar = avatar, ActivityStatus = ActivityStatus.Online, IsLoggedIn = true });
                ParticipantsCount.Text = AvatarMessengers.Count().ToString();

                Console.WriteLine("<<HubService_AvatarLoggedIn: OK");
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

                    var avatarButton = (Button)iElement;

                    //avatarButton.SetAvatarActivityStatus((ActivityStatus)activityStatus);
                    SetAvatarActivityStatus(avatarButton, avatarButton.Tag as Avatar, (ActivityStatus)activityStatus);

                    Console.WriteLine("<<NewBroadcastAvatarActivityStatus: OK");
                }
                else
                {
                    Console.WriteLine("<<NewBroadcastAvatarActivityStatus: IGNORE");
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

                    Console.WriteLine("<<NewBroadcastAvatarMovement: OK");
                }
                else
                {
                    Console.WriteLine("<<NewBroadcastAvatarMovement: IGNORE");
                }
            }
        }
        #endregion

        #region Message

        private void HubService_AvatarTyping(int avatarId, MessageType mt)
        {

        }

        private void HubService_NewImageMessage(int avatarId, byte[] pic, MessageType mt)
        {

        }

        private void HubService_NewTextMessage(int avatarId, string msg, MessageType mt)
        {
            if (avatarId > 0)
            {
                if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Avatar taggedAvatar && taggedAvatar.Id == avatarId) is UIElement iElement)
                {
                    var avatarMessenger = AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == avatarId);

                    if (avatarMessenger != null)
                        avatarMessenger.ActivityStatus = ActivityStatus.Online;

                    var avatarButton = (Button)iElement;

                    switch (mt)
                    {
                        case MessageType.Broadcast:
                            break;
                        case MessageType.Unicast:
                            {
                                AddMessageBubbleToCanvas(msg, avatarButton);
                            }
                            break;
                        default:
                            break;
                    }

                    Console.WriteLine("<<HubService_NewTextMessage: OK");
                }
                else
                {
                    Console.WriteLine("<<HubService_NewTextMessage: IGNORE");
                }
            }
        }

        #endregion

        #region Connection

        /// <summary>
        /// Subscribe to hub and listen to hub events.
        /// </summary>
        private void SubscribeHub()
        {
            #region Hub Connectivity

            HubService.ConnectionReconnecting += HubService_ConnectionReconnecting;
            HubService.ConnectionReconnected += HubService_ConnectionReconnected;
            HubService.ConnectionClosed += HubService_ConnectionClosed;

            #endregion

            #region Avatar World Events

            HubService.NewBroadcastAvatarMovement += HubService_NewBroadcastAvatarMovement;
            HubService.NewBroadcastAvatarActivityStatus += HubService_NewBroadcastAvatarActivityStatus;

            #endregion

            #region Avatar Connectivity

            HubService.AvatarLoggedIn += HubService_AvatarLoggedIn;
            HubService.AvatarLoggedOut += HubService_AvatarLoggedOut;
            HubService.AvatarDisconnected += HubService_AvatarDisconnected;
            HubService.AvatarReconnected += HubService_AvatarReconnected;

            #endregion

            #region Construct World Events

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

            #region Avatar Messenging

            HubService.AvatarTyping += HubService_AvatarTyping;
            HubService.NewTextMessage += HubService_NewTextMessage;
            HubService.NewImageMessage += HubService_NewImageMessage;

            #endregion

            Console.WriteLine("++ListenOnHubService: OK");
        }

        private async void HubService_ConnectionClosed()
        {
            Console.WriteLine("<<HubService_ConnectionClosed");

            _isLoggedIn = false;

            if (await ConnectWithHub())
            {
                await TryLoginToHub();
            }
        }

        private async void HubService_ConnectionReconnected()
        {
            _ = await HubService.Login(Avatar);

            //IsConnected = true;
            _isLoggedIn = true;

            Console.WriteLine("<<HubService_ConnectionReconnected");
        }

        private void HubService_ConnectionReconnecting()
        {
            //IsConnected = false;
            _isLoggedIn = false;

            Console.WriteLine("<<HubService_ConnectionReconnecting");
        }
        #endregion

        #region Hub Login      

        private bool CanHubLogin()
        {
            var result = Avatar != null && Avatar.User != null && HubService.IsConnected();

            Console.WriteLine($"CanHubLogin: {result}");

            return result;
        }

        public async Task ConnectWithHubThenLogin()
        {
            if (await ConnectWithHub())
            {
                await TryLoginToHub();
            }
            else
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Would you like to try again?", "Connection failure", MessageBoxButton.OKCancel);

                if (messageBoxResult == MessageBoxResult.OK)
                {
                    await ConnectWithHubThenLogin();
                }
            }
        }

        private async Task<bool> ConnectWithHub()
        {
            try
            {
                Console.WriteLine("TryConnect: ATTEMP");

                if (HubService.IsConnected())
                {
                    Console.WriteLine("TryConnect: OK");
                    return true;
                }

                await HubService.ConnectAsync();

                Console.WriteLine("TryConnect: OK.");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("TryConnect: ERROR." + "\n" + ex.Message);
                return false;
            }
        }

        private async Task TryLoginToHub()
        {
            bool loggedIn = await LoginToHub();

            if (!loggedIn)
            {
                Console.WriteLine("TryHubLogin: FAILED");

                MessageBoxResult messageBoxResult = MessageBox.Show("Would you like to try again?", "Login failure", MessageBoxButton.OKCancel);

                if (messageBoxResult == MessageBoxResult.OK)
                {
                    await TryLoginToHub();
                }
            }
            else
            {
                Console.WriteLine("TryHubLogin: OK");
            }
        }

        private async Task<bool> LoginToHub()
        {
            try
            {
                if (CanHubLogin())
                {
                    var result = await HubService.Login(Avatar);

                    if (result != null)
                    {
                        Console.WriteLine("HubLogin: OK");

                        var avatars = result.Avatars;

                        // Clearing up canvas prior to login
                        AvatarMessengers.Clear();
                        Canvas_root.Children.Clear();

                        if (avatars != null && avatars.Any())
                        {
                            Console.WriteLine("HubLogin: avatars found: " + avatars.Count());

                            // Find current user's avatar and update current Avatar instance
                            var responseAvatar = avatars.FirstOrDefault(x => x.Id == Avatar.Id);
                            Avatar = responseAvatar;

                            foreach (var avatar in avatars)
                            {
                                var avatarButton = GenerateAvatarButton(avatar);
                                AddAvatarOnCanvas(avatarButton, avatar.Coordinate.X, avatar.Coordinate.Y, avatar.Coordinate.Z);

                                AvatarMessengers.Add(new AvatarMessenger { Avatar = avatar, IsLoggedIn = true });
                            }

                            ParticipantsCount.Text = AvatarMessengers.Count().ToString();
                        }

                        var constructs = result.Constructs;

                        if (constructs != null && constructs.Any())
                        {
                            Console.WriteLine("HubLogin: Constructs found: " + constructs.Count());

                            foreach (var construct in constructs)
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

                                ScaleElement(constructBtn, construct.Scale);
                                RotateElement(constructBtn, construct.Rotation);
                            }
                        }

                        _isLoggedIn = true;

                        // Set connected user's avatar image
                        if (Canvas_root.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar avatar && avatar.Id == Avatar.Id) is UIElement iElement)
                            AvatarImageHolder.Content = CopyUiElementContent(iElement);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("HubLogin: FAILED");

                    if (await ConnectWithHub())
                    {
                        return await LoginToHub();
                    }

                    Console.WriteLine("HubLogin: FAILED");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HubLogin: ERROR " + "\n" + ex.Message);
                return false;
            }
        }
        #endregion

        #endregion

        #region Pointer Events

        #region Canvas

        /// <summary>
        /// Event fired on pointer press on canvas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Canvas_root_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!CanPerformWorldEvents())
                return;

            if (ConstructAddButton.IsChecked.Value && _addingConstruct != null)
            {
                await AddConstructOnPointerPressed(e);
            }
            else if (ConstructCloneButton.IsChecked.Value && _cloningConstruct != null)
            {
                await CloneConstructOnPointerPressed(e);
            }
            else if (ConstructMoveButton.IsChecked.Value && _movingConstruct != null)
            {
                await MoveConstructOnPointerPressed(e);
            }
            else
            {
                // Clear construct selection
                _selectedConstruct = null;
                ShowSelectedConstruct(null);

                _selectedAvatar = null;
                ShowSelectedAvatar(null);

                _messageToAvatar = null;
                ShowMessenginAvatar(null);

                HideConstructOperationButtons();
                HideAvatarOperationButtons();
            }
        }

        #endregion

        #region Avatar

        /// <summary>
        ///  Event fired on pointer press on an avatar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Avatar_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!CanPerformWorldEvents())
                return;

            UIElement uielement = (UIElement)sender;
            _selectedAvatar = uielement;

            ShowSelectedAvatar(uielement);

            if (((Button)uielement).Tag is Avatar avatar)
            {
                // If selected own avatar
                if (avatar.Id == Avatar.Id)
                {
                    OwnAvatarActionsHolder.Visibility = Visibility.Visible;
                    OtherAvatarActionsHolder.Visibility = Visibility.Collapsed;
                }
                else
                {
                    OwnAvatarActionsHolder.Visibility = Visibility.Collapsed;
                    OtherAvatarActionsHolder.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion

        #region Construct

        /// <summary>
        /// Event fired on pointer press on a construct. The construct latches on to the pointer if press is not released.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Construct_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!CanPerformWorldEvents())
                return;

            UIElement uielement = (UIElement)sender;
            _selectedConstruct = uielement;

            ShowSelectedConstruct(uielement);

            if (ConstructAddButton.IsChecked.Value && _addingConstruct != null)
            {
                await AddConstructOnPointerPressed(e);
            }
            else if (ConstructCloneButton.IsChecked.Value && _cloningConstruct != null)
            {
                await CloneConstructOnPointerPressed(e);
            }
            else if (ConstructMoveButton.IsChecked.Value && _movingConstruct != null)
            {
                await MoveConstructOnPointerPressed(e);
            }
            else if (ConstructCraftButton.IsChecked.Value)
            {
                ShowConstructOperationButtons();

                // Drag start of a constuct
                _objectLeft = Canvas.GetLeft(uielement);
                _objectTop = Canvas.GetTop(uielement);

                var currentPoint = e.GetCurrentPoint(Canvas_root);

                // Remember the pointer position:
                _pointerX = currentPoint.Position.X;
                _pointerY = currentPoint.Position.Y;

                uielement.CapturePointer(e.Pointer);

                _isPointerCaptured = true;

                Console.WriteLine("Construct drag started.");
            }
            else
            {
                // Move avatar
                if (Canvas_root.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar avatar && avatar.Id == Avatar.Id) is UIElement iElement)
                {
                    var taggedObject = MoveElement(iElement, e);

                    var movedAvatar = taggedObject as Avatar;

                    var z = Canvas.GetZIndex(iElement);

                    await HubService.BroadcastAvatarMovement(avatarId: Avatar.Id, x: movedAvatar.Coordinate.X, y: movedAvatar.Coordinate.Y, z: z);

                    Console.WriteLine("Avatar moved.");
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
            if (ConstructCraftButton.IsChecked.Value)
            {
                UIElement uielement = (UIElement)sender;

                if (_isPointerCaptured)
                {
                    var currentPoint = e.GetCurrentPoint(Canvas_root);

                    // Calculate the new position of the object:
                    double deltaH = currentPoint.Position.X - _pointerX;
                    double deltaV = currentPoint.Position.Y - _pointerY;

                    _objectLeft = deltaH + _objectLeft;
                    _objectTop = deltaV + _objectTop;

                    // Update the object position:
                    Canvas.SetLeft(uielement, _objectLeft);
                    Canvas.SetTop(uielement, _objectTop);

                    // Remember the pointer position:
                    _pointerX = currentPoint.Position.X;
                    _pointerY = currentPoint.Position.Y;
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
            if (ConstructMoveButton.IsChecked.Value)
            {
                return;
            }

            if (ConstructCraftButton.IsChecked.Value)
            {
                // Drag drop selected construct
                UIElement uielement = (UIElement)sender;
                _isPointerCaptured = false;
                uielement.ReleasePointerCapture(e.Pointer);

                _selectedConstruct = uielement;
                ShowSelectedConstruct(uielement);

                var x = Canvas.GetLeft(uielement);
                var y = Canvas.GetTop(uielement);
                var z = Canvas.GetZIndex(uielement);

                var construct = ((Button)uielement).Tag as Construct;

                construct.Coordinate.X = x;
                construct.Coordinate.Y = y;
                construct.Coordinate.Z = z;

                await HubService.BroadcastConstructMovement(construct.Id, construct.Coordinate.X, construct.Coordinate.Y, construct.Coordinate.Z);

                Console.WriteLine("Construct dropped.");
            }
        }

        /// <summary>
        /// Moves the _movingConstruct to the pressed point.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task MoveConstructOnPointerPressed(PointerRoutedEventArgs e)
        {
            var taggedObject = MoveElement(_movingConstruct, e);

            var construct = taggedObject as Construct;

            await HubService.BroadcastConstructMovement(construct.Id, construct.Coordinate.X, construct.Coordinate.Y, construct.Coordinate.Z);

            Console.WriteLine("Construct moved.");
        }

        /// <summary>
        /// Clones the _cloningConstruct to the pressed point.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task CloneConstructOnPointerPressed(PointerRoutedEventArgs e)
        {
            var constructAsset = ((Button)_cloningConstruct).Tag as Construct;

            if (constructAsset != null)
            {
                var constructBtn = GenerateConstructButton(
                    name: constructAsset.Name,
                    imageUrl: constructAsset.ImageUrl);

                // Add the construct on pressed point
                var construct = AddConstructOnCanvas(
                    construct: constructBtn,
                    x: e.GetCurrentPoint(Canvas_root).Position.X,
                    y: e.GetCurrentPoint(Canvas_root).Position.Y);

                // Center the construct on pressed point
                construct = MoveElement(constructBtn, e) as Construct;

                await HubService.BroadcastConstruct(construct);

                Console.WriteLine("Construct cloned.");
            }
        }

        /// <summary>
        /// Adds the _addingConstruct to the pressed point.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task AddConstructOnPointerPressed(PointerRoutedEventArgs e)
        {
            var pressedPoint = e.GetCurrentPoint(Canvas_root);
            var button = (Button)_addingConstruct;

            var constructAsset = button.Tag as Construct;

            var constructBtn = GenerateConstructButton(
                   name: constructAsset.Name,
                   imageUrl: constructAsset.ImageUrl);

            // Add the construct on pressed point
            AddConstructOnCanvas(
                construct: constructBtn,
                x: pressedPoint.Position.X,
                y: pressedPoint.Position.Y);

            // Center the construct on pressed point
            var construct = MoveElement(constructBtn, e) as Construct;

            await HubService.BroadcastConstruct(construct);

            Console.WriteLine("Construct added.");
        }

        #endregion

        #endregion

        #region Button Events

        #region Connection

        /// <summary>
        /// ConnectButton click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("ConnectButton_Click");

                if (Character.IsEmpty())
                {
                    Characters = Characters.Any() ? Characters : JsonSerializer.Deserialize<Character[]>(Properties.Resources.CharacterAssets).ToList();

                    var characterPicker = new CharacterPicker(
                        characters: Characters,
                        characterSelected: async (character) =>
                        {
                            Character = character;
                            await Connect();
                        });

                    characterPicker.Show();
                }
                else
                {
                    await Connect();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Construct

        /// <summary>
        /// Actives crafting mode. This enables operations buttons for a construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConstructCraftButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                ConstructCraftButton.Content = ConstructCraftButton.IsChecked.Value ? "Crafting" : "Craft";

                ConstructMoveButton.IsChecked = false;
                ConstructMoveButton.Content = "Move";

                ConstructCloneButton.IsChecked = false;
                ConstructCloneButton.Content = "Clone";

                ConstructDeleteButton.Content = "Delete";

                ConstructAddButton.IsChecked = false;
                ConstructAddButton.Content = "Add";

                if (!ConstructCraftButton.IsChecked.Value)
                {
                    ConstructAddButton.Visibility = Visibility.Collapsed;

                    HideConstructOperationButtons();

                    _movingConstruct = null;
                    _cloningConstruct = null;
                    _addingConstruct = null;

                    ShowOperationalConstruct(null);

                    await BroadcastAvatarActivityStatus(ActivityStatus.Online);
                }
                else
                {
                    ConstructAddButton.Visibility = Visibility.Visible;

                    await BroadcastAvatarActivityStatus(ActivityStatus.Crafting);
                }
            }
        }

        /// <summary>
        /// Activates adding a construct. Shows the asset picker for picking a construct and upon selection keeps adding the construct until untoggled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConstructAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                // Turn off add mode if previously triggered
                if (_addingConstruct != null)
                {
                    _addingConstruct = null;
                    ConstructAddButton.Content = "Add";
                    ConstructAddButton.IsChecked = false;

                    return;
                }

                ConstructAddButton.Content = "Adding";

                if (!ConstructAssets.Any())
                {
                    ConstructAssets = JsonSerializer.Deserialize<ConstructAsset[]>(Properties.Resources.ConstructAssets).ToList();
                    ConstructCategories = ConstructAssets.Select(x => x.Category).Distinct().Select(z => new ConstructCategory() { ImageUrl = @$"ms-appx:///Images/World_Objects/{z}.png", Name = z }).ToList();
                }

                var constructAssetPicker = new ConstructAssetPicker(
                    constructAssets: ConstructAssets,
                    constructCategories: ConstructCategories,
                    assetUriHelper: _assetUriHelper,
                    assetSelected: (constructAsset) =>
                    {
                        var constructBtn = GenerateConstructButton(
                            name: constructAsset.Name,
                            imageUrl: constructAsset.ImageUrl);

                        _addingConstruct = constructBtn;
                        ShowOperationalConstruct(_addingConstruct);
                    });

                // If the picker was closed without a selection of an asset, set the ConstructAddButton to default
                constructAssetPicker.Closed += (s, e) =>
                {
                    if (_addingConstruct == null)
                    {
                        ConstructAddButton.Content = "Add";
                        ConstructAddButton.IsChecked = false;
                    }
                };

                constructAssetPicker.Show();
            }
        }

        /// <summary>
        /// Moves the selected construct to the clicked point.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConstructMoveButton_Click(object sender, RoutedEventArgs e)
        {
            ConstructMoveButton.Content = ConstructMoveButton.IsChecked.Value ? "Moving" : "Move";

            if (!ConstructMoveButton.IsChecked.Value)
            {
                _movingConstruct = null;
                ShowOperationalConstruct(null);
            }
            else
            {
                UIElement uielement = _selectedConstruct;
                _movingConstruct = uielement;
                ShowOperationalConstruct(_movingConstruct);
            }
        }

        /// <summary>
        /// Activates cloning of a selected construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConstructCloneButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {

                ConstructCloneButton.Content = ConstructCloneButton.IsChecked.Value ? "Cloning" : "Clone";

                if (!ConstructCloneButton.IsChecked.Value)
                {
                    _cloningConstruct = null;
                    ShowOperationalConstruct(null);
                }
                else
                {
                    UIElement uielement = _selectedConstruct;
                    _cloningConstruct = uielement;
                    ShowOperationalConstruct(_cloningConstruct);
                }
            }
        }

        /// <summary>
        /// Deletes the selected construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConstructDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                if (_selectedConstruct != null)
                {
                    var construct = ((Button)_selectedConstruct).Tag as Construct;

                    Canvas_root.Children.Remove(_selectedConstruct);
                    ShowSelectedConstruct(null);

                    await HubService.RemoveConstruct(construct.Id);
                }
            }
        }

        /// <summary>
        /// Brings the selected construct forward.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConstructBringForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                if (_selectedConstruct != null)
                {
                    var zIndex = Canvas.GetZIndex(_selectedConstruct);
                    zIndex++;
                    Canvas.SetZIndex(_selectedConstruct, zIndex);

                    var construct = ((Button)_selectedConstruct).Tag as Construct;
                    await HubService.BroadcastConstructPlacement(construct.Id, zIndex);
                }
            }
        }

        /// <summary>
        /// Sends the selected construct backwards.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConstructSendBackwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                if (_selectedConstruct != null)
                {
                    var zIndex = Canvas.GetZIndex(_selectedConstruct);
                    zIndex--;
                    Canvas.SetZIndex(_selectedConstruct, zIndex);

                    var construct = ((Button)_selectedConstruct).Tag as Construct;
                    await HubService.BroadcastConstructPlacement(construct.Id, zIndex);
                }
            }
        }

        /// <summary>
        /// Scales up the selected construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConstructScaleUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                if (_selectedConstruct != null)
                {
                    var button = (Button)_selectedConstruct;

                    var construct = button.Tag as Construct;

                    var newScale = construct.Scale + 0.25f;

                    construct = ScaleElement(_selectedConstruct, newScale) as Construct;

                    await HubService.BroadcastConstructScale(construct.Id, construct.Scale);

                    Console.WriteLine("Construct scaled up.");
                }
            }
        }

        /// <summary>
        /// Scales down the selected construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConstructScaleDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                if (_selectedConstruct != null)
                {
                    var button = (Button)_selectedConstruct;

                    var construct = button.Tag as Construct;

                    if (construct.Scale == 0.25f)
                    {
                        return;
                    }

                    var newScale = construct.Scale - 0.25f;

                    construct = ScaleElement(_selectedConstruct, newScale) as Construct;

                    await HubService.BroadcastConstructScale(construct.Id, construct.Scale);

                    Console.WriteLine("Construct scaled down.");
                }
            }
        }

        /// <summary>
        /// Rotates the selected construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConstructRotateButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                if (_selectedConstruct != null)
                {
                    var button = (Button)_selectedConstruct;

                    var construct = button.Tag as Construct;

                    var newRotation = construct.Rotation + 5;

                    construct = RotateElement(_selectedConstruct, newRotation) as Construct;

                    await HubService.BroadcastConstructRotation(construct.Id, construct.Rotation);

                    Console.WriteLine("Construct rotated.");
                }
            }
        }

        #endregion

        #region Message

        /// <summary>
        /// Activates messenging controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MessageAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPerformWorldEvents())
                return;

            if (_selectedAvatar == null)
                return;

            await BroadcastAvatarActivityStatus(ActivityStatus.Typing);

            // show messenge from and to avatars and show messenging controls
            if (((Button)_selectedAvatar).Tag is Avatar avatar)
            {
                _messageToAvatar = Canvas_root.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar taggedAvatar && taggedAvatar.Id == avatar.Id);
                ShowMessenginAvatar(_messageToAvatar);
            }
        }

        private void ShowMessenginAvatar(UIElement uIElement)
        {
            if (uIElement == null)
            {
                MessengingToAvatarHolder.Content = null;
                MessengingFromAvatarHolder.Content = null;
                MessengingControlsHolder.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessengingFromAvatarHolder.Content = CopyUiElementContent(Canvas_root.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar taggedAvatar && taggedAvatar.Id == Avatar.Id));
                MessengingToAvatarHolder.Content = CopyUiElementContent(uIElement);
                MessengingControlsHolder.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Sends unicast message to the selected avatar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SendUnicastMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPerformWorldEvents())
                return;

            if (_messageToAvatar == null)
                return;

            if (((Button)_messageToAvatar).Tag is Avatar avatar && !string.IsNullOrEmpty(MessengingTextBox.Text) && !string.IsNullOrWhiteSpace(MessengingTextBox.Text))
            {
                await HubService.SendUnicastMessage(avatar.Id, MessengingTextBox.Text);

                // Add message bubble to own avatar
                if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Avatar taggedAvatar && taggedAvatar.Id == Avatar.Id) is UIElement iElement)
                {
                    AddMessageBubbleToCanvas(MessengingTextBox.Text, iElement);

                    // If activity status is not messagin then update it
                    if (((Button)iElement).Tag is Avatar taggedAvatar && taggedAvatar.ActivityStatus != ActivityStatus.Typing)
                    {
                        await BroadcastAvatarActivityStatus(ActivityStatus.Typing);
                    }
                }

                MessengingTextBox.Text = String.Empty;
            }
        }

        /// <summary>
        /// Activates post creation window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreatePostButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPerformWorldEvents())
                return;

            if (_selectedAvatar == null)
                return;
        }

        #endregion

        #endregion

        #region Construct Labels

        /// <summary>
        /// Shows the selected construct on pointer press and release on a construct.
        /// </summary>
        /// <param name="uielement"></param>
        private void ShowSelectedConstruct(UIElement uielement)
        {
            if (uielement == null)
            {
                SelectedConstructHolder.Content = null;
                SelectedConstructHolder.Visibility = Visibility.Collapsed;
            }
            else
            {
                var button = CopyUiElementContent(uielement);
                SelectedConstructHolder.Content = button;
                SelectedConstructHolder.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Shows the operational construct when adding, moving, cloning.
        /// </summary>
        /// <param name="uielement"></param>
        private void ShowOperationalConstruct(UIElement uielement)
        {
            if (uielement == null)
            {
                OperationalConstructHolder.Content = null;
                OperationalConstructHolder.Visibility = Visibility.Collapsed;
            }
            else
            {
                var button = CopyUiElementContent(uielement);
                OperationalConstructHolder.Content = button;
                OperationalConstructHolder.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Shows the selected avatar.
        /// </summary>
        /// <param name="uielement"></param>
        private void ShowSelectedAvatar(UIElement uielement)
        {
            if (uielement == null)
            {
                SelectedAvatarHolder.Content = null;
                SelectedAvatarHolder.Visibility = Visibility.Collapsed;
            }
            else
            {
                var button = CopyUiElementContent(uielement);
                SelectedAvatarHolder.Content = button;
                SelectedAvatarHolder.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Functionality

        #region Element

        /// <summary>
        /// Copies the image content from an UIElement and returns it as an Image.
        /// </summary>
        /// <param name="uielement"></param>
        /// <returns></returns>
        private static UIElement CopyUiElementContent(UIElement uielement)
        {
            var oriBitmap = ((Image)((Button)uielement).Content).Source as BitmapImage;

            var bitmap = new BitmapImage(new Uri(oriBitmap.UriSource.OriginalString, UriKind.RelativeOrAbsolute));

            var img = new Image()
            {
                Source = bitmap,
                Stretch = Stretch.Uniform,
                Height = 50,
                Width = 50,
                Margin = new Thickness(5)
            };

            return img;
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
            var pressedPoint = e.GetCurrentPoint(Canvas_root);

            var button = (Button)uIElement;

            var goToX = pressedPoint.Position.X - button.ActualWidth / 2;

            // If the UIElement is Avatar then move it to an Y coordinate so that it appears on top of the clicked point. 
            var goToY = pressedPoint.Position.Y - (button.Tag is Avatar ? button.ActualHeight : button.ActualHeight / 2);

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
            if (uIElement == null)
                return null;

            var button = (Button)uIElement;

            var taggedObject = button.Tag;

            // Set moving status on start
            if (taggedObject is Avatar)
            {
                if (ConstructCraftButton.IsChecked.Value)
                    SetAvatarActivityStatus(button, (Avatar)taggedObject, ActivityStatus.Crafting);
                else
                    SetAvatarActivityStatus(button, (Avatar)taggedObject, ActivityStatus.Moving);
            }

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

            DoubleAnimation setXAnimation = new DoubleAnimation()
            {
                From = nowX,
                To = goToX,
                Duration = new Duration(TimeSpan.FromSeconds(timeToTravelDistance)),
                EasingFunction = _easingFunction,
            };

            DoubleAnimation setYAnimation = new DoubleAnimation()
            {
                From = nowY,
                To = goToY,
                Duration = new Duration(TimeSpan.FromSeconds(timeToTravelDistance)),
                EasingFunction = _easingFunction,
            };

            setYAnimation.Completed += (object sender, EventArgs e) =>
            {
                if (taggedObject is Avatar)
                {
                    var taggedAvatar = taggedObject as Avatar;

                    if (ConstructCraftButton.IsChecked.Value)
                        SetAvatarActivityStatus(button, (Avatar)taggedObject, ActivityStatus.Crafting);
                    else
                        SetAvatarActivityStatus(button, (Avatar)taggedObject, ActivityStatus.Online);
                }
            };

            Storyboard.SetTarget(setXAnimation, uIElement);
            Storyboard.SetTargetProperty(setXAnimation, new PropertyPath(Canvas.LeftProperty));

            Storyboard.SetTarget(setYAnimation, uIElement);
            Storyboard.SetTargetProperty(setYAnimation, new PropertyPath(Canvas.TopProperty));

            moveStory.Children.Add(setXAnimation);
            moveStory.Children.Add(setYAnimation);

            moveStory.Begin();

            if (taggedObject is Construct)
            {
                var taggedConstruct = taggedObject as Construct;

                taggedConstruct.Coordinate.X = goToX;
                taggedConstruct.Coordinate.Y = goToY;

                if (gotoZ.HasValue)
                {
                    taggedConstruct.Coordinate.Z = (int)gotoZ;
                    Canvas.SetZIndex(uIElement, (int)gotoZ);
                }
                else
                {
                    taggedConstruct.Coordinate.Z = Canvas.GetZIndex(uIElement);
                }

                taggedObject = taggedConstruct;
            }
            else if (button.Tag is Avatar)
            {
                var taggedAvatar = taggedObject as Avatar;

                taggedAvatar.Coordinate.X = goToX;
                taggedAvatar.Coordinate.Y = goToY;

                if (gotoZ.HasValue)
                {
                    taggedAvatar.Coordinate.Z = (int)gotoZ;
                    Canvas.SetZIndex(uIElement, (int)gotoZ);
                }
                else
                {
                    taggedAvatar.Coordinate.Z = Canvas.GetZIndex(uIElement);
                }

                taggedObject = taggedAvatar;
            }

            return taggedObject;
        }

        #endregion

        #region Connection

        private void SetDemoData()
        {
            if (Avatar != null)
                return;

            InWorld = App.InWorld;
            User = App.User;

            Avatar = new Avatar()
            {
                Id = App.User.Id,
                ActivityStatus = ActivityStatus.Online,
                User = new AvatarUser()
                {
                    Email = User.Email,
                    ImageUrl = User.ImageUrl,
                    Name = User.Name,
                    Phone = User.Phone,
                    ProfilePictureUrl = User.ImageUrl
                },
                Character = Character,
                World = InWorld,
                Coordinate = new Coordinate(x: (Window.Current.Bounds.Width / 2) - 50, y: (Window.Current.Bounds.Height / 2) - 100, z: new Random().Next(100, 999)),
                ImageUrl = Character.ImageUrl,
            };
        }

        /// <summary>
        /// Connects the current user with server.
        /// </summary>
        /// <returns></returns>
        private async Task Connect()
        {
            SetDemoData();

            // If a connection is already established simply login to hub
            if (CanHubLogin())
            {
                await TryLoginToHub();
            }
            else
            {
                // Otherwise open a new connection then login
                await ConnectWithHubThenLogin();
            }

            ConstructCraftingButtonsHolder.Visibility = CanPerformWorldEvents() ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Checks if world events can be performed or not. If HubService is connected to server and the user is logged in then returns true.
        /// </summary>
        /// <returns></returns>
        private bool CanPerformWorldEvents()
        {
            var result = HubService.IsConnected() && _isLoggedIn;
            Console.WriteLine("CanPerformWorldEvents: " + result);
            return result;
        }

        #endregion

        #region Avatar

        /// <summary>
        /// Adds an avatar on canvas.
        /// </summary>
        /// <param name="avatar"></param>
        private Avatar AddAvatarOnCanvas(UIElement avatar, double x, double y, int? z = null)
        {
            Canvas.SetLeft(avatar, x);
            Canvas.SetTop(avatar, y);

            if (z.HasValue)
            {
                Canvas.SetZIndex(avatar, z.Value);
            }

            Canvas_root.Children.Add(avatar);

            var taggedAvatar = ((Button)avatar).Tag as Avatar;
            taggedAvatar.Coordinate.X = x;
            taggedAvatar.Coordinate.Y = y;

            return taggedAvatar;
        }

        /// <summary>
        /// Broadcasts avatar activity status.
        /// </summary>
        /// <param name="activityStatus"></param>
        /// <returns></returns>
        private async Task BroadcastAvatarActivityStatus(ActivityStatus activityStatus)
        {
            if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Avatar taggedAvatar && taggedAvatar.Id == Avatar.Id) is UIElement iElement)
            {
                var avatarButton = (Button)iElement;
                var taggedAvatar = avatarButton.Tag as Avatar;
                SetAvatarActivityStatus(avatarButton, taggedAvatar, activityStatus);

                await HubService.BroadcastAvatarActivityStatus(taggedAvatar.Id, (int)activityStatus);
            }
        }

        /// <summary>
        /// Sets the provided activityStatus to the avatar.
        /// </summary>
        /// <param name="avatarButton"></param>
        /// <param name="avatar"></param>
        /// <param name="activityStatus"></param>
        public void SetAvatarActivityStatus(Button avatarButton, Avatar avatar, ActivityStatus activityStatus)
        {
            avatar.ActivityStatus = activityStatus;
            SetStatusBoundImageUrl(avatarButton, avatar, activityStatus);
        }

        /// <summary>
        /// Sets the StatusBoundImageUrl as content of the avatarButton according to the activityStatus.
        /// </summary>
        /// <param name="avatarButton"></param>
        /// <param name="avatar"></param>
        /// <param name="activityStatus"></param>
        private void SetStatusBoundImageUrl(Button avatarButton, Avatar avatar, ActivityStatus activityStatus)
        {
            if (avatar.Character.StatusBoundImageUrls.FirstOrDefault(x => x.Status == activityStatus) is StatusBoundImageUrl statusBoundImageUrl)
            {
                if (avatarButton.Content is Image img && img.Source is BitmapImage bitmap)
                {
                    bitmap.UriSource = new Uri(statusBoundImageUrl.ImageUrl);
                }
            }
        }

        /// <summary>
        /// Generates a new button from the provided avatar.
        /// </summary>
        /// <param name="avatar"></param>
        /// <returns></returns>
        private Button GenerateAvatarButton(Avatar avatar)
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

            Button obj = new Button()
            {
                Style = Application.Current.Resources["MaterialDesign_GlassButton_Style"] as Style,
            };

            obj.Content = img;
            obj.Tag = avatar;

            obj.PointerPressed += Avatar_PointerPressed;

            //img.Effect = new DropShadowEffect() { ShadowDepth = 10, Color = Colors.Black, BlurRadius = 10, Opacity = 0.5, Direction = -90 };
            return obj;
        }

        /// <summary>
        /// Hides avatar opearional buttons.
        /// </summary>
        private void HideAvatarOperationButtons()
        {
            OwnAvatarActionsHolder.Visibility = Visibility.Collapsed;
            OtherAvatarActionsHolder.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Construct

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

            int indexZ = 9;

            if (z.HasValue)
            {
                indexZ = z.Value;
            }
            else
            {
                // If Z index is not proved then assign max Z index to this construct button
                if (Canvas_root.Children != null && Canvas_root.Children.Any())
                {
                    if (Canvas_root.Children.Any(x => x is Button button && button.Tag is Construct))
                    {
                        var lastConstruct = ((Button)Canvas_root.Children.Where(x => x is Button button && button.Tag is Construct).LastOrDefault()).Tag as Construct;

                        if (lastConstruct != null)
                        {
                            indexZ = lastConstruct.Coordinate.Z + 1;
                        }
                    }
                }
            }

            Canvas.SetZIndex(construct, indexZ);

            Canvas_root.Children.Add(construct);

            var taggedConstruct = ((Button)construct).Tag as Construct;

            taggedConstruct.Coordinate.X = x;
            taggedConstruct.Coordinate.Y = y;
            taggedConstruct.Coordinate.Z = indexZ;

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

            // Id is broadcasted
            var id = constructId ?? UidGenerator.New();

            if (id <= 0)
            {
                throw new InvalidOperationException("Id can not be less than or equal to zero.");
            }

            var obj = new Button()
            {
                BorderBrush = new SolidColorBrush(Colors.DodgerBlue),
                Style = Application.Current.Resources["MaterialDesign_GlassButton_Style"] as Style,
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
        /// Shows construct operational buttons on the UI.
        /// </summary>
        private void ShowConstructOperationButtons()
        {
            ConstructOperationalCommandsHolder.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides construct operational buttons.
        /// </summary>
        private void HideConstructOperationButtons()
        {
            ConstructOperationalCommandsHolder.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Scales an UIElement to the provided scale. Returns the tagged object of the uIElement.
        /// </summary>
        /// <param name="uIElement"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        private object ScaleElement(UIElement uIElement, float scale)
        {
            var button = (Button)uIElement;
            button.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);

            if (button.Tag is Construct construct)
            {
                var scaleTransform = new CompositeTransform()
                {
                    ScaleX = scale,
                    ScaleY = scale,
                    Rotation = construct.Rotation,
                };

                button.RenderTransform = scaleTransform;

                construct.Scale = scale;

                return construct;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Rotates an UIElement to the provided rotation. Returns the tagged object of the uIElement.
        /// </summary>
        /// <param name="uIElement"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        private object RotateElement(UIElement uIElement, float rotation)
        {
            var button = (Button)uIElement;
            button.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);

            if (button.Tag is Construct construct)
            {
                var rotateTransform = new CompositeTransform()
                {
                    ScaleX = construct.Scale,
                    ScaleY = construct.Scale,
                    Rotation = rotation
                };

                button.RenderTransform = rotateTransform;

                construct.Rotation = rotation;

                return construct;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Message

        /// <summary>
        /// Adds message bubble to canvas on top of the avatar who sent it.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="avatar"></param>
        private void AddMessageBubbleToCanvas(string msg, UIElement avatar)
        {
            var avatarButton = avatar as Button;
            var taggedAvatar = avatarButton.Tag as Avatar;

            ContentControl chatBubble = new ContentControl()
            {
                Style = Application.Current.Resources["MaterialDesign_PopupContent_Style"] as Style,
            };

            // Prepare content
            StackPanel chatContent = new StackPanel() { Orientation = Orientation.Horizontal };

            Image avatarImage = new Image()
            {
                Source = new BitmapImage(((BitmapImage)((Image)avatarButton.Content).Source).UriSource),
                Height = 30,
                Width = 30,
                Stretch = Stretch.Uniform,
            };

            // If own message then image on the left
            if (taggedAvatar.Id == Avatar.Id)
            {
                chatContent.Children.Add(avatarImage);
                chatContent.Children.Add(new Label() { Content = msg, Margin = new Thickness(5, 0, 5, 0) });
            }
            else
            {
                chatContent.Children.Add(new Label() { Content = msg, Margin = new Thickness(5, 0, 5, 0) });
                chatContent.Children.Add(avatarImage);
            }

            chatBubble.Content = chatContent;

            // Set opacity animation
            DoubleAnimation doubleAnimation = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(60),
            };

            // after opacity reaches zero delete this from canvas
            doubleAnimation.Completed += (s, e) =>
            {
                Canvas_root.Children.Remove(chatBubble);
            };

            Storyboard.SetTarget(doubleAnimation, chatBubble);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(OpacityProperty));

            Storyboard fadeStoryBoard = new Storyboard();
            fadeStoryBoard.Children.Add(doubleAnimation);

            // Add to canvas
            var x = taggedAvatar.Coordinate.X;
            var y = taggedAvatar.Coordinate.Y - (avatarButton.ActualHeight / 2);

            Canvas.SetLeft(chatBubble, x);
            Canvas.SetTop(chatBubble, y);
            Canvas.SetZIndex(chatBubble, 999);

            // Add a shadow effect to the chat bubble
            chatBubble.Effect = new DropShadowEffect() { ShadowDepth = 4, Color = Colors.Black, BlurRadius = 10, Opacity = 0.5 };

            Canvas_root.Children.Add(chatBubble);

            fadeStoryBoard.Begin();
        }

        #endregion

        #endregion

        #endregion
    }
}