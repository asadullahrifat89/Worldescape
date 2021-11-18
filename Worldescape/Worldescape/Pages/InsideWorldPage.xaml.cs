using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
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

        bool _isAddingConstruct;
        bool _isCraftingMode;
        bool _isMovingConstruct;
        bool _isCloningConstruct;
        //bool _isDeleting;

        bool _isLoggedIn;
        //bool _isHubSubscribed;

        UIElement _selectedConstruct;
        UIElement _addingConstruct;
        UIElement _movingConstruct;
        UIElement _cloningConstruct;

        EasingFunctionBase _easingFunction = new ExponentialEase()
        {
            EasingMode = EasingMode.EaseOut,
            Exponent = 5,
        };

        //string[] _objects = new string[]
        //{
        //    "ms-appx:///Images/World_Objects/Landscape/Grass.png",
        //    "ms-appx:///Images/World_Objects/Landscape/Big_Tree.png",
        //    "ms-appx:///Images/World_Objects/Prototype/block_W.png",
        //};

        string[] avatarUrls = new string[]
        {
            "ms-appx:///Images/Avatar_Profiles/John_The_Seer/character_maleAdventurer_idle.png",
            "ms-appx:///Images/Avatar_Profiles/Jenna_The_Adventurer/character_femaleAdventurer_idle.png",
            "ms-appx:///Images/Avatar_Profiles/Robert_The_Guardian/character_malePerson_idle.png",
            "ms-appx:///Images/Avatar_Profiles/Rodney_The_Messenger/character_femalePerson_idle.png",
            "ms-appx:///Images/Avatar_Profiles/Rob_The_Robot/character_robot_idle.png",
        };

        List<ConstructAsset> ConstructAssets = new List<ConstructAsset>();

        List<ConstructCategory> ConstructCategories = new List<ConstructCategory>();

        private readonly IHubService HubService;

        InWorld InWorld = new InWorld();

        User User = new User();

        Avatar Avatar = null;

        Character Character = new Character();

        ObservableCollection<AvatarMessenger> AvatarMessengers = new ObservableCollection<AvatarMessenger>();

        readonly AssetUrlHelper _assetUriHelper;

        #endregion

        #region Ctor

        public InsideWorldPage(IHubService hubService, AssetUrlHelper assetUriHelper)
        {
            InitializeComponent();

            HubService = hubService;// App.ServiceProvider.GetService(typeof(IHubService)) as IHubService;
            _assetUriHelper = assetUriHelper;

            SubscribeHub();

            //DemoImage.Source = new BitmapImage() { UriSource = new Uri("http://localhost:5034/api/Query/GetAsset?fileName=World_Objects%5CLandscape%5CGrass.png") };
        }

        #endregion

        #region Methods

        #region Hub Listener

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

            Console.WriteLine("++ListenOnHubService: OK");
        }

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

            //TODO: set rotation
            ScaleElement(constructBtn, construct.Scale);

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

        private void HubService_AvatarLoggedIn(Avatar obj)
        {
            // If an avatar already exists, ignore
            if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Avatar taggedAvatar && taggedAvatar.Id == obj.Id) is UIElement iElement)
            {
                Console.WriteLine("<<HubService_AvatarLoggedIn: IGNORE");
            }
            else
            {
                AddAvatarOnCanvas(obj);
                AvatarMessengers.Add(new AvatarMessenger() { Avatar = obj, ActivityStatus = ActivityStatus.Online, IsLoggedIn = true });
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

        #region Connection
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
                            this.Avatar = responseAvatar;

                            foreach (var avatar in avatars)
                            {
                                //if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Avatar taggedAvatar && taggedAvatar.Id == avatar.Id) is UIElement iElement)
                                //{
                                //    MoveElement(iElement, avatar.Coordinate.X, avatar.Coordinate.Y);
                                //}
                                //else
                                //{
                                AvatarMessengers.Add(new AvatarMessenger { Avatar = avatar, IsLoggedIn = true });
                                AddAvatarOnCanvas(avatar);
                                //}
                            }

                            ParticipantsCount.Text = AvatarMessengers.Count().ToString();
                        }

                        var constructs = result.Constructs;

                        if (constructs != null && constructs.Any())
                        {
                            Console.WriteLine("HubLogin: Constructs found: " + constructs.Count());

                            foreach (var construct in constructs)
                            {
                                // TODO: set scale and rotation

                                //if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Construct taggedConstruct && taggedConstruct.Id == construct.Id) is UIElement iElement)
                                //{
                                //    Canvas.SetZIndex(iElement, construct.Coordinate.Z);
                                //    MoveElement(iElement, construct.Coordinate.X, construct.Coordinate.Y);

                                //    
                                //}
                                //else // insert new constructs
                                //{
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


                                //}
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

        /// <summary>
        /// Event fired on pointer press on canvas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Canvas_root_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!CanPerformWorldEvents())
                return;

            if (_isAddingConstruct && _addingConstruct != null)
            {
                await AddConstructOnPointerPressed(e);
            }
            else if (_isCloningConstruct && _cloningConstruct != null)
            {
                await CloneConstructOnPointerPressed(e);
            }
            else if (_isMovingConstruct && _movingConstruct != null)
            {
                await MoveConstructOnPointerPressed(e);
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
            if (!CanPerformWorldEvents())
                return;

            UIElement uielement = (UIElement)sender;
            _selectedConstruct = uielement;
            ShowInteractiveConstruct(uielement);

            if (_isAddingConstruct && _addingConstruct != null)
            {
                await AddConstructOnPointerPressed(e);
            }
            else if (_isCloningConstruct && _cloningConstruct != null)
            {
                await CloneConstructOnPointerPressed(e);
            }
            else if (_isMovingConstruct && _movingConstruct != null)
            {
                await MoveConstructOnPointerPressed(e);
            }
            else if (_isCraftingMode)
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
            if (_isCraftingMode)
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

            if (_isCraftingMode)
            {
                // Drag drop selected construct
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
            var construct = AddConstructOnCanvas(
                construct: constructBtn,
                x: pressedPoint.Position.X,
                y: pressedPoint.Position.Y);

            // Center the construct on pressed point
            construct = MoveElement(constructBtn, e) as Construct;

            await HubService.BroadcastConstruct(construct);

            Console.WriteLine("Construct added.");
        }
        #endregion

        #region Button Events

        private void CraftButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                _isCraftingMode = !_isCraftingMode;
                CraftButton.Content = _isCraftingMode ? "Constructing" : "Construct";

                _isMovingConstruct = false;
                ConstructMoveButton.Content = "Move";

                _isCloningConstruct = false;
                ConstructCloneButton.Content = "Clone";

                //_isDeleting = false;
                ConstructDeleteButton.Content = "Delete";

                _isAddingConstruct = false;
                ConstructAddButton.Content = "Add";

                if (!_isCraftingMode)
                {
                    ConstructAddButton.Visibility = Visibility.Collapsed;

                    HideConstructOperationButtons();

                    _movingConstruct = null;
                    _cloningConstruct = null;
                    _addingConstruct = null;

                    OperationalConstructHolder.Content = null;
                    ////OperationalConstructStatus.Text = null;
                }
                else
                {
                    ConstructAddButton.Visibility = Visibility.Visible;
                    //this.ConstructDeleteButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void ConstructAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                // Turn off add mode if previously triggered
                if (_isAddingConstruct)
                {
                    _isAddingConstruct = false;
                    _addingConstruct = null;
                    ConstructAddButton.Content = "Add";

                    OperationalConstructHolder.Content = null;
                    ////OperationalConstructStatus.Text = null;

                    return;
                }

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

                        _isAddingConstruct = !_isAddingConstruct;
                        ConstructAddButton.Content = _isAddingConstruct ? "Adding" : "Add";

                        ShowOperationalConstruct(_addingConstruct, "Adding");
                    });

                constructAssetPicker.Show();
            }
        }

        private void ConstructMoveButton_Click(object sender, RoutedEventArgs e)
        {
            _isMovingConstruct = !_isMovingConstruct;
            ConstructMoveButton.Content = _isMovingConstruct ? "Moving" : "Move";

            if (!_isMovingConstruct)
            {
                _movingConstruct = null;
                OperationalConstructHolder.Content = null;
                //OperationalConstructStatus.Text = null;
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
            if (CanPerformWorldEvents())
            {
                _isCloningConstruct = !_isCloningConstruct;
                ConstructCloneButton.Content = _isCloningConstruct ? "Cloning" : "Clone";

                if (!_isCloningConstruct)
                {
                    _cloningConstruct = null;
                    OperationalConstructHolder.Content = null;
                    //OperationalConstructStatus.Text = null;
                }
                else
                {
                    UIElement uielement = _selectedConstruct;
                    _cloningConstruct = uielement;
                    ShowOperationalConstruct(_cloningConstruct, "Cloning");
                }
            }
        }

        /// <summary>
        /// Deletes a selected construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConstructDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
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

                    await HubService.RemoveConstruct(construct.Id);
                }
            }
        }

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
                }
            }
        }

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

                    Console.WriteLine("Construct scaled.");
                }
            }
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("ConnectButton_Click");

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
        }

        #endregion

        #region Construct Labels

        private void ShowInteractiveConstruct(UIElement uielement)
        {
            var button = CopyUiElementContent(uielement);
            SelectedConstructHolder.Content = button;
        }

        private void ShowOperationalConstruct(UIElement uielement, string operationStatus)
        {
            var button = CopyUiElementContent(uielement);
            OperationalConstructHolder.Content = button;
            //OperationalConstructStatus.Text = operationStatus;
        }

        #endregion

        #region Common

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
                    //Id = User.Id,
                    Email = User.Email,
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
                Coordinate = new Coordinate(new Random().Next(100), new Random().Next(100), new Random().Next(100)),
                ImageUrl = avatarUrls[new Random().Next(avatarUrls.Count())],
            };
        }

        private bool CanPerformWorldEvents()
        {
            var result = HubService.IsConnected() && _isLoggedIn;
            Console.WriteLine("CanPerformWorldEvents: " + result);
            return result;
        }

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
            Canvas.SetZIndex(avatarBtn, 999/*avatar.Coordinate.Z*/); //TODO: figure out Z index calculation later

            Canvas_root.Children.Add(avatarBtn);
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
        private static UIElement CopyUiElementContent(UIElement uielement)
        {
            var oriBitmap = ((Image)((Button)uielement).Content).Source as BitmapImage;

            var bitmap = new BitmapImage(new Uri(oriBitmap.UriSource.OriginalString, UriKind.RelativeOrAbsolute));
            var img = new Image() { Source = bitmap, Stretch = Stretch.Uniform, Height = 50, Width = 50, Margin = new Thickness(10) };

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

            ConstructScaleUpButton.Visibility = Visibility.Visible;
            ConstructScaleDownButton.Visibility = Visibility.Visible;
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

            ConstructScaleUpButton.Visibility = Visibility.Collapsed;
            ConstructScaleDownButton.Visibility = Visibility.Collapsed;
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
            var goToY = pressedPoint.Position.Y - button.ActualHeight / 2;

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
            var button = (Button)uIElement;

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

            if (button.Tag is Construct taggedConstruct)
            {
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
            else if (button.Tag is Avatar taggedAvatar)
            {
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

        private object ScaleElement(UIElement uIElement, float scale)
        {
            var button = (Button)uIElement;

            if (button.Tag is Construct construct)
            {
                #region Storyboard
                //var scaling = new ScaleTransform()
                //{
                //    CenterX = button.ActualWidth / 2,
                //    CenterY = button.ActualWidth / 2,
                //    ScaleX = construct.Scale,
                //    ScaleY = construct.Scale
                //};

                //button.RenderTransform = scaling;

                //Storyboard scaleStory = new Storyboard();
                //DoubleAnimation setScaleX = new DoubleAnimation()
                //{
                //    From = construct.Scale,
                //    To = newScale,
                //    Duration = new Duration(TimeSpan.FromSeconds(5)),
                //    EasingFunction = _easingFunction,
                //};

                //DoubleAnimation setScaleY = new DoubleAnimation()
                //{
                //    From = construct.Scale,
                //    To = newScale,
                //    Duration = new Duration(TimeSpan.FromSeconds(5)),
                //    EasingFunction = _easingFunction,
                //};

                //Storyboard.SetTarget(setScaleX, (ScaleTransform)button.RenderTransform);
                //Storyboard.SetTargetProperty(setScaleX, new PropertyPath(ScaleTransform.ScaleXProperty));

                //Storyboard.SetTarget(setScaleY, (ScaleTransform)button.RenderTransform);
                //Storyboard.SetTargetProperty(setScaleY, new PropertyPath(ScaleTransform.ScaleYProperty));

                //scaleStory.Children.Add(setScaleX);
                //scaleStory.Children.Add(setScaleY);

                //scaleStory.Begin();

                //construct.Scale = newScale; 
                #endregion

                var scaling = new ScaleTransform()
                {
                    CenterX = button.ActualWidth / 2,
                    CenterY = button.ActualWidth / 2,
                    ScaleX = scale,
                    ScaleY = scale
                };

                button.RenderTransform = scaling;

                construct.Scale = scale;

                return construct;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #endregion
    }
}