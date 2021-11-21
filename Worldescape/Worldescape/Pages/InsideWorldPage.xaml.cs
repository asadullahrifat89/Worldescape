﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media.Effects;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Service;
using Worldescape.Data;
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

        EasingFunctionBase _constructEaseOut = new ExponentialEase()
        {
            EasingMode = EasingMode.EaseOut,
            Exponent = 5,
        };

        #endregion

        #region Ctor
        public InsideWorldPage()
        {
            InitializeComponent();

            HubService = /*hubService;*/ App.ServiceProvider.GetService(typeof(IHubService)) as IHubService;
            _assetUriHelper = App.ServiceProvider.GetService(typeof(AssetUrlHelper)) as AssetUrlHelper;//assetUriHelper;

            SubscribeHub();
        }

        #endregion

        #region Properties

        List<ConstructAsset> ConstructAssets { get; set; } = new List<ConstructAsset>();

        List<ConstructCategory> ConstructCategories { get; set; } = new List<ConstructCategory>();

        List<Character> Characters { get; set; } = new List<Character>();

        InWorld InWorld { get; set; } = new InWorld();

        User User { get; set; } = new User();

        Avatar Avatar { get; set; } = null;

        Character Character { get; set; } = new Character();

        ObservableCollection<AvatarMessenger> AvatarMessengers { get; set; } = new ObservableCollection<AvatarMessenger>();

        List<Construct> MultiselectedConstructs { get; set; } = new List<Construct>();

        #endregion

        #region Methods

        #region Received Hub Events

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
                        avatarMessenger.ActivityStatus = ActivityStatus.Idle;
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

                AvatarMessengers.Add(new AvatarMessenger() { Avatar = avatar, ActivityStatus = ActivityStatus.Idle, IsLoggedIn = true });
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
                        avatarMessenger.ActivityStatus = ActivityStatus.Idle;

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
                        avatarMessenger.ActivityStatus = ActivityStatus.Idle;

                    var senderAvatarUiElement = (Button)iElement;

                    switch (mt)
                    {
                        case MessageType.Broadcast:
                            break;
                        case MessageType.Unicast:
                            {
                                AddMessageBubbleToCanvas(msg, senderAvatarUiElement); // receive message
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
                await AddConstructOnPointerPressed(e); // Canvas_root
            }
            else if (ConstructCloneButton.IsChecked.Value && _cloningConstruct != null)
            {
                await CloneConstructOnPointerPressed(e); // Canvas_root
            }
            else if (ConstructMoveButton.IsChecked.Value && _movingConstruct != null)
            {
                await MoveConstructOnPointerPressed(e); // Canvas_root
            }
            else
            {
                // Clear construct selection
                _selectedConstruct = null;
                ShowSelectedConstruct(null);

                _movingConstruct = null;
                ShowOperationalConstruct(null);

                _selectedAvatar = null;
                ShowSelectedAvatar(null);

                _messageToAvatar = null;
                ShowMessagingAvatar(null);

                HideConstructOperationButtons();
                HideAvatarOperationButtons();
                HideMessagingControls();

                ClearMultiselectedConstructs();
                HideAvatarActivityStatusHolder();
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
                await AddConstructOnPointerPressed(e); // Construct
            }
            else if (ConstructCloneButton.IsChecked.Value && _cloningConstruct != null)
            {
                await CloneConstructOnPointerPressed(e); // Construct
            }
            else if (ConstructMoveButton.IsChecked.Value && _movingConstruct != null)
            {
                await MoveConstructOnPointerPressed(e); // Construct
            }
            else if (ConstructMultiSelectButton.IsChecked.Value)
            {
                // Add the selected construct to multi selected list
                var construct = ((Button)_selectedConstruct).Tag as Construct;

                if (!MultiselectedConstructs.Contains(construct))
                {
                    MultiSelectedConstructsHolder.Children.Add(CopyUiElementImageContent(_selectedConstruct));
                    MultiselectedConstructs.Add(construct);
                }
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
                await BroadcastAvatarMovement(e);
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
            if (ConstructMultiSelectButton.IsChecked.Value && MultiselectedConstructs.Any())
            {
                var currrentPoint = e.GetCurrentPoint(Canvas_root);

                // Align avatar to clicked point
                AlignAvatarFaceDirection(currrentPoint.Position.X);

                //var maxX = Canvas_root.Children.OfType<Button>().Where(z => z.Tag is Construct c && MultiselectedConstructs.Select(x => x.Id).Contains(c.Id)).Max(x => ((Construct)x.Tag).Coordinate.X);

                //UIElement fe = Canvas_root.Children.OfType<Button>().Where(z => z.Tag is Construct c && MultiselectedConstructs.Select(x => x.Id).Contains(c.Id)).FirstOrDefault(x => ((Construct)x.Tag).Coordinate.X >= maxX);

                // var maxX = Canvas_root.Children.OfType<Button>().Where(z => z.Tag is Construct c && MultiselectedConstructs.Select(x => x.Id).Contains(c.Id)).Max(x => ((Construct)x.Tag).Coordinate.X);

                UIElement fe = Canvas_root.Children.OfType<Button>().Where(z => z.Tag is Construct c && c.Id == MultiselectedConstructs.FirstOrDefault().Id).FirstOrDefault();

                List<Tuple<int, double, double>> distWrtFi = new();

                var feConstruct = ((Button)fe).Tag as Construct;

                var fex = feConstruct.Coordinate.X;
                var fey = feConstruct.Coordinate.Y;

                foreach (Construct element in MultiselectedConstructs)
                {
                    var xDis = feConstruct.Coordinate.X - element.Coordinate.X;
                    var yDis = feConstruct.Coordinate.Y - element.Coordinate.Y;

                    distWrtFi.Add(new Tuple<int, double, double>(element.Id, xDis, yDis));
                }

                foreach (Construct element in MultiselectedConstructs)
                {
                    var nowX = element.Coordinate.X;
                    var nowY = element.Coordinate.Y;

                    _movingConstruct = Canvas_root.Children.OfType<Button>().Where(z => z.Tag is Construct).FirstOrDefault(x => ((Construct)x.Tag).Id == element.Id);

                    double goToX = currrentPoint.Position.X - ((Button)_movingConstruct).ActualWidth / 2;
                    double goToY = currrentPoint.Position.Y - ((Button)_movingConstruct).ActualHeight / 2;

                    goToX += distWrtFi.FirstOrDefault(x => x.Item1 == element.Id).Item2;
                    goToY += distWrtFi.FirstOrDefault(x => x.Item1 == element.Id).Item3;

                    var taggedObject = MoveElement(_movingConstruct, goToX, goToY);

                    var construct = taggedObject as Construct;

                    await HubService.BroadcastConstructMovement(construct.Id, construct.Coordinate.X, construct.Coordinate.Y, construct.Coordinate.Z);

                    Console.WriteLine("Construct moved.");
                }
            }
            else
            {
                if (_movingConstruct == null)
                    return;

                var taggedObject = MoveElement(_movingConstruct, e);

                var construct = taggedObject as Construct;

                // Align avatar to construct point
                AlignAvatarFaceDirection(construct.Coordinate.X);

                await HubService.BroadcastConstructMovement(construct.Id, construct.Coordinate.X, construct.Coordinate.Y, construct.Coordinate.Z);

                Console.WriteLine("Construct moved.");
            }
        }

        /// <summary>
        /// Clones the _cloningConstruct to the pressed point.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task CloneConstructOnPointerPressed(PointerRoutedEventArgs e)
        {
            if (_cloningConstruct == null)
                return;

            var constructAsset = ((Button)_cloningConstruct).Tag as Construct;

            if (constructAsset != null)
            {
                var pressedPoint = e.GetCurrentPoint(Canvas_root);

                var constructButton = GenerateConstructButton(
                    name: constructAsset.Name,
                    imageUrl: constructAsset.ImageUrl);

                // Add the construct on pressed point
                var construct = AddConstructOnCanvas(
                    construct: constructButton,
                    x: pressedPoint.Position.X,
                    y: pressedPoint.Position.Y);

                // Center the construct on pressed point
                construct = CenterAlignNewConstructButton(pressedPoint, constructButton, construct);

                // Align avatar to construct point
                AlignAvatarFaceDirection(construct.Coordinate.X);

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
            if (_addingConstruct == null)
                return;

            var button = (Button)_addingConstruct;

            var constructAsset = button.Tag as Construct;

            if (constructAsset != null)
            {
                var pressedPoint = e.GetCurrentPoint(Canvas_root);

                var constructButton = GenerateConstructButton(
                           name: constructAsset.Name,
                           imageUrl: constructAsset.ImageUrl);

                // Add the construct on pressed point
                var construct = AddConstructOnCanvas(
                    construct: constructButton,
                    x: pressedPoint.Position.X,
                    y: pressedPoint.Position.Y);

                // Center the construct on pressed point
                construct = CenterAlignNewConstructButton(pressedPoint, constructButton, construct);

                // Align avatar to construct point
                AlignAvatarFaceDirection(construct.Coordinate.X);

                await HubService.BroadcastConstruct(construct);

                Console.WriteLine("Construct added.");
            }
        }

        private Construct CenterAlignNewConstructButton(Windows.UI.Input.PointerPoint pressedPoint, Button constructButton, Construct construct)
        {
            var offsetX = constructButton.ActualWidth / 2;
            var offsetY = constructButton.ActualHeight / 2;

            var goToX = pressedPoint.Position.X - offsetX;
            var goToY = pressedPoint.Position.Y - offsetY;

            Canvas.SetLeft(constructButton, goToX);
            Canvas.SetTop(constructButton, goToY);

            construct.Coordinate.X = goToX;
            construct.Coordinate.Y = goToY;

            return construct;
        }

        #endregion

        #region Message

        /// <summary>
        /// Event fired on pointer press on a chat bubble. This starts a replay conversation with the message sender.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChatBubble_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // reply to the selected message and show Messaging controls
            if (((Button)sender).Tag is Avatar avatar)
            {
                _messageToAvatar = Canvas_root.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar taggedAvatar && taggedAvatar.Id == avatar.Id);
                _selectedAvatar = _messageToAvatar;

                ShowMessagingAvatar(_messageToAvatar);
                ShowMessagingControls();

                ShowSelectedAvatar(_selectedAvatar);
                OtherAvatarActionsHolder.Visibility = Visibility.Visible;

                //MessagingTextBox.Focus();
            }
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
                    Characters = Characters.Any() ? Characters : JsonSerializer.Deserialize<Character[]>(Service.Properties.Resources.CharacterAssets).ToList();

                    var characterPicker = new CharacterPicker(
                        characters: Characters,
                        characterSelected: async (character) =>
                        {
                            Character = character;

                            PrepareAvatarData();

                            var mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;
                            mainPage.SetCurrentUserModel();

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
        /// Activates multi selection of clicked constructs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConstructMultiSelectButton_Click(object sender, RoutedEventArgs e)
        {
            ConstructMultiSelectButton.Content = ConstructMultiSelectButton.IsChecked.Value ? "Multiselecting" : "Multiselect";

            if (ConstructMultiSelectButton.IsChecked.Value)
            {
                ShowConstructOperationButtons();
            }
            else
            {
                HideConstructOperationButtons();
            }
        }

        /// <summary>
        /// Activates crafting mode. This enables operations buttons for a construct.
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

                ConstructMultiSelectButton.IsChecked = false;
                ConstructMultiSelectButton.Content = "Multiselect";

                if (ConstructCraftButton.IsChecked.Value)
                {
                    ConstructAddButton.Visibility = Visibility.Visible;
                    ConstructMultiSelectButton.Visibility = Visibility.Visible;

                    await BroadcastAvatarActivityStatus(ActivityStatus.Crafting);
                }
                else
                {
                    ConstructAddButton.Visibility = Visibility.Collapsed;
                    ConstructMultiSelectButton.Visibility = Visibility.Collapsed;

                    HideConstructOperationButtons();

                    _movingConstruct = null;
                    _cloningConstruct = null;
                    _addingConstruct = null;

                    ShowOperationalConstruct(null);

                    await BroadcastAvatarActivityStatus(ActivityStatus.Idle);
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
                    ConstructAssets = JsonSerializer.Deserialize<ConstructAsset[]>(Service.Properties.Resources.ConstructAssets).ToList();
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
        /// Toggles moving mode for the selected construct.
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
        /// Activates cloning mode of a selected construct.
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
                if (ConstructMultiSelectButton.IsChecked.Value && MultiselectedConstructs.Any())
                {
                    foreach (var element in MultiselectedConstructs)
                    {
                        _selectedConstruct = Canvas_root.Children.OfType<Button>().Where(x => x.Tag is Construct c && c.Id == element.Id).FirstOrDefault();

                        if (_selectedConstruct != null)
                        {
                            await BroadcastConstructDelete(_selectedConstruct);
                        }
                    }

                    MultiselectedConstructs.Clear();
                }
                else
                {
                    if (_selectedConstruct != null)
                    {
                        await BroadcastConstructDelete(_selectedConstruct);
                    }
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
                if (ConstructMultiSelectButton.IsChecked.Value && MultiselectedConstructs.Any())
                {
                    foreach (var element in MultiselectedConstructs)
                    {
                        _selectedConstruct = Canvas_root.Children.OfType<Button>().Where(x => x.Tag is Construct c && c.Id == element.Id).FirstOrDefault();

                        if (_selectedConstruct != null)
                        {
                            await BroadcastConstructBringForward(_selectedConstruct);
                        }
                    }
                }
                else
                {
                    if (_selectedConstruct != null)
                    {
                        await BroadcastConstructBringForward(_selectedConstruct);
                    }
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
                if (ConstructMultiSelectButton.IsChecked.Value && MultiselectedConstructs.Any())
                {
                    foreach (var element in MultiselectedConstructs)
                    {
                        _selectedConstruct = Canvas_root.Children.OfType<Button>().Where(x => x.Tag is Construct c && c.Id == element.Id).FirstOrDefault();

                        if (_selectedConstruct != null)
                        {
                            await BroadcastConstructSendBackward(_selectedConstruct);
                        }
                    }
                }
                else
                {
                    if (_selectedConstruct != null)
                    {
                        await BroadcastConstructSendBackward(_selectedConstruct);
                    }
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
                if (ConstructMultiSelectButton.IsChecked.Value && MultiselectedConstructs.Any())
                {
                    foreach (var element in MultiselectedConstructs)
                    {
                        _selectedConstruct = Canvas_root.Children.OfType<Button>().Where(x => x.Tag is Construct c && c.Id == element.Id).FirstOrDefault();

                        if (_selectedConstruct != null)
                        {
                            await BroadcastConstructScaleUp(_selectedConstruct);
                        }
                    }
                }
                else
                {
                    if (_selectedConstruct != null)
                    {
                        await BroadcastConstructScaleUp(_selectedConstruct);
                    }
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
                if (ConstructMultiSelectButton.IsChecked.Value && MultiselectedConstructs.Any())
                {
                    foreach (var element in MultiselectedConstructs)
                    {
                        _selectedConstruct = Canvas_root.Children.OfType<Button>().Where(x => x.Tag is Construct c && c.Id == element.Id).FirstOrDefault();

                        if (_selectedConstruct != null)
                        {
                            await BroadcastConstructScaleDown(_selectedConstruct);
                        }
                    }
                }
                else
                {
                    if (_selectedConstruct != null)
                    {
                        await BroadcastConstructScaleDown(_selectedConstruct);
                    }
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
                if (ConstructMultiSelectButton.IsChecked.Value && MultiselectedConstructs.Any())
                {
                    foreach (var element in MultiselectedConstructs)
                    {
                        _selectedConstruct = Canvas_root.Children.OfType<Button>().Where(x => x.Tag is Construct c && c.Id == element.Id).FirstOrDefault();

                        if (_selectedConstruct != null)
                        {
                            await BroadcastConstructRotate(_selectedConstruct);
                        }
                    }
                }
                else
                {
                    if (_selectedConstruct != null)
                    {
                        await BroadcastConstructRotate(_selectedConstruct);
                    }
                }
            }
        }

        #endregion

        #region Avatar

        private void MyAvatarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Activates status options.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectStatusButton.IsChecked.Value)
            {
                ShowAvatarActivityStatusHolder();
            }
            else
            {
                HideAvatarActivityStatusHolder();
            }
        }

        /// <summary>
        /// Set status of the current avatar. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SetStatusMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;

            ActivityStatus[] values = Enum.GetValues(typeof(ActivityStatus)).Cast<ActivityStatus>().ToArray();
            ActivityStatus enumMember = values.FirstOrDefault(x => x.ToString() == menuItem.Content.ToString());

            await BroadcastAvatarActivityStatus(enumMember);
        }

        #endregion

        #region Message

        /// <summary>
        /// Event fired upon key press inside the chat box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MessagingTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SendUnicastMessageButton_Click(sender, e);
            }
        }

        /// <summary>
        /// Activates Messaging controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MessageAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPerformWorldEvents())
                return;

            if (_selectedAvatar == null)
                return;

            //await BroadcastAvatarActivityStatus(ActivityStatus.Messaging);

            // show messenge from and to avatars and show Messaging controls
            if (((Button)_selectedAvatar).Tag is Avatar avatar)
            {
                _messageToAvatar = Canvas_root.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar taggedAvatar && taggedAvatar.Id == avatar.Id);
                ShowMessagingAvatar(_messageToAvatar);
                ShowMessagingControls();

                //MessagingTextBox.Focus();
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

            if (((Button)_messageToAvatar).Tag is Avatar avatar && !string.IsNullOrEmpty(MessagingTextBox.Text) && !string.IsNullOrWhiteSpace(MessagingTextBox.Text))
            {
                await HubService.SendUnicastMessage(avatar.Id, MessagingTextBox.Text);

                // Add message bubble to own avatar
                if (Canvas_root.Children.FirstOrDefault(x => x is Button button && button.Tag is Avatar taggedAvatar && taggedAvatar.Id == Avatar.Id) is UIElement iElement)
                {
                    AddMessageBubbleToCanvas(MessagingTextBox.Text, iElement); // send message

                    // If activity status is not Messaging then update it
                    if (((Button)iElement).Tag is Avatar taggedAvatar && taggedAvatar.ActivityStatus != ActivityStatus.Messaging)
                    {
                        await BroadcastAvatarActivityStatus(ActivityStatus.Messaging);
                    }
                }

                MessagingTextBox.Text = string.Empty;
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

        #region Construct Images

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
                var button = CopyUiElementImageContent(uielement);
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
                var button = CopyUiElementImageContent(uielement);
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
                var img = CopyUiElementImageContent(uielement);
                SelectedAvatarHolder.Content = img;
                SelectedAvatarHolder.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Avatar Images

        private void ShowMessagingAvatar(UIElement receiverUiElement)
        {
            if (receiverUiElement == null)
            {
                MessagingToAvatarHolder.Content = null;
                MessagingFromAvatarHolder.Content = null;
            }
            else
            {
                var receiver = ((Button)receiverUiElement).Tag as Avatar;

                // If receiver avatar is forward from current avatar
                AlignAvatarFaceDirection(receiver.Coordinate.X);

                MessagingFromAvatarHolder.Content = CopyUiElementImageContent(Canvas_root.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar taggedAvatar && taggedAvatar.Id == Avatar.Id));
                MessagingToAvatarHolder.Content = CopyUiElementImageContent(receiverUiElement);
            }
        }

        private void ShowMessagingControls()
        {
            MessagingControlsHolder.Visibility = Visibility.Visible;
        }

        private void HideMessagingControls()
        {
            MessagingControlsHolder.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Functionality

        #region Hub Login

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

        private bool CanHubLogin()
        {
            var result = Avatar != null && Avatar.User != null && HubService.IsConnected();

            Console.WriteLine($"CanHubLogin: {result}");

            return result;
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
                                SetAvatarActivityStatus(avatarButton, avatar, avatar.ActivityStatus);

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
                        {
                            var oriBitmap = ((Image)((Button)iElement).Content).Source as BitmapImage;

                            var bitmap = new BitmapImage(new Uri(oriBitmap.UriSource.OriginalString, UriKind.RelativeOrAbsolute));

                            MyAvatarButton.Tag = ((Button)iElement).Tag as Avatar;
                            AvatarImageHolder.Source = bitmap;
                            MyAvatarButton.Visibility = Visibility.Visible;
                        }

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

        #region Element

        /// <summary>
        /// Copies the image content from an UIElement and returns it as an Image.
        /// </summary>
        /// <param name="uielement"></param>
        /// <returns></returns>
        private static Image CopyUiElementImageContent(UIElement uielement)
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

            var offsetX = button.ActualWidth / 2;

            var goToX = pressedPoint.Position.X - offsetX;

            // If the UIElement is Avatar then move it to an Y coordinate so that it appears on top of the clicked point, if it's a construct then move the construct to the middle point. 
            var offsetY = button.Tag is Avatar ? button.ActualHeight : button.ActualHeight / 2;

            var goToY = pressedPoint.Position.Y - offsetY;

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
            if (taggedObject is Avatar avatar)
            {
                if (ConstructCraftButton.IsChecked.Value && avatar.Id == Avatar.Id)
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

            AnimationTimeline gotoXAnimation = null;
            AnimationTimeline gotoYAnimation = null;

            if (taggedObject is Avatar) // When avatar movement
            {
                //THROEY:
                // If already on higher ground Y
                //nowY=200  
                //                   goToY=400

                // If already on lower ground Y
                //                   goToY=200
                //nowY=400

                gotoXAnimation = new DoubleAnimation()
                {
                    From = nowX,
                    To = goToX,
                    Duration = new Duration(TimeSpan.FromSeconds(timeToTravelDistance)),
                    EasingFunction = _constructEaseOut,
                };

                if (goToX < nowX) // If going backward
                {
                    button.RenderTransform = new ScaleTransform() { ScaleX = -1 };
                }
                else // If going forward
                {
                    button.RenderTransform = new ScaleTransform() { ScaleX = 1 };
                }

                var halfTime = timeToTravelDistance / 2;

                gotoYAnimation = new DoubleAnimationUsingKeyFrames();

                var gotoYAnimationKeyFrames = (DoubleAnimationUsingKeyFrames)gotoYAnimation;

                var easeOut = new ExponentialEase()
                {
                    EasingMode = EasingMode.EaseOut,
                    Exponent = 5,
                };

                var easeIn = new ExponentialEase()
                {
                    EasingMode = EasingMode.EaseIn,
                    Exponent = 5,
                };

                // Do half time animation Y
                if (nowY < goToY) // From higher ground to lower ground
                {
                    gotoYAnimationKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame()
                    {
                        KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(halfTime)),
                        Value = nowY - 100,
                        EasingFunction = easeOut,
                    });

                }
                else // From lower ground to higher ground
                {
                    var middleY = nowY - goToY;
                    gotoYAnimationKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame()
                    {
                        KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(halfTime)),
                        Value = goToY - 100,
                        EasingFunction = easeOut,
                    });
                }

                // To final animation Y
                gotoYAnimationKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(halfTime += halfTime)),
                    Value = goToY,
                    EasingFunction = easeIn,
                });

                Storyboard.SetTarget(gotoYAnimation, uIElement);
                Storyboard.SetTargetProperty(gotoYAnimation, new PropertyPath(Canvas.TopProperty));
                moveStory.Children.Add(gotoYAnimation);
            }
            else // When avatar movement
            {
                gotoXAnimation = new DoubleAnimation()
                {
                    From = nowX,
                    To = goToX,
                    Duration = new Duration(TimeSpan.FromSeconds(timeToTravelDistance)),
                    EasingFunction = _constructEaseOut,
                };

                gotoYAnimation = new DoubleAnimation()
                {
                    From = nowY,
                    To = goToY,
                    Duration = new Duration(TimeSpan.FromSeconds(timeToTravelDistance)),
                    EasingFunction = _constructEaseOut,
                };
            }

            gotoYAnimation.Completed += (object sender, EventArgs e) =>
            {
                if (taggedObject is Avatar taggedAvatar)
                {
                    if (ConstructCraftButton.IsChecked.Value && taggedAvatar.Id == Avatar.Id)
                        SetAvatarActivityStatus(button, (Avatar)taggedObject, ActivityStatus.Crafting);
                    else
                        SetAvatarActivityStatus(button, (Avatar)taggedObject, ActivityStatus.Idle);
                }
            };

            Storyboard.SetTarget(gotoXAnimation, uIElement);
            Storyboard.SetTargetProperty(gotoXAnimation, new PropertyPath(Canvas.LeftProperty));

            Storyboard.SetTarget(gotoYAnimation, uIElement);
            Storyboard.SetTargetProperty(gotoYAnimation, new PropertyPath(Canvas.TopProperty));

            moveStory.Children.Add(gotoXAnimation);
            moveStory.Children.Add(gotoYAnimation);

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

            #region Avatar Messaging

            HubService.AvatarTyping += HubService_AvatarTyping;
            HubService.NewTextMessage += HubService_NewTextMessage;
            HubService.NewImageMessage += HubService_NewImageMessage;

            #endregion

            Console.WriteLine("++ListenOnHubService: OK");
        }

        private void PrepareAvatarData()
        {
            //if (Avatar != null)
            //    return;

            InWorld = App.InWorld;
            User = App.User;

            Avatar = new Avatar()
            {
                Id = App.User.Id,
                ActivityStatus = ActivityStatus.Idle,
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
        /// Checks if world events can be performed or not. If HubService is connected to server and the user is logged in then returns true.
        /// </summary>
        /// <returns></returns>
        private bool CanPerformWorldEvents()
        {
            var result = HubService.IsConnected() && _isLoggedIn;
            Console.WriteLine("CanPerformWorldEvents: " + result);
            return result;
        }

        /// <summary>
        /// Connects the current user with server.
        /// </summary>
        /// <returns></returns>
        private async Task Connect()
        {
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

        #endregion

        #region Avatar

        /// <summary>
        /// Aligns facing direction of current avatar wrt provided x.
        /// </summary>
        /// <param name="construct"></param>
        private void AlignAvatarFaceDirection(double x)
        {
            Button senderUiElement = Canvas_root.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar taggedAvatar && taggedAvatar.Id == Avatar.Id);
            var sender = senderUiElement.Tag as Avatar;

            // If adding construct is forward from current avatar
            if (x > sender.Coordinate.X)
            {
                senderUiElement.RenderTransform = new ScaleTransform() { ScaleX = 1 };
            }
            else
            {
                senderUiElement.RenderTransform = new ScaleTransform() { ScaleX = -1 };
            }
        }

        /// <summary>
        /// Shows avatar status activity options.
        /// </summary>
        private void ShowAvatarActivityStatusHolder()
        {
            AvatarActivityStatusHolder.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides avatar activity status holder.
        /// </summary>
        private void HideAvatarActivityStatusHolder()
        {
            AvatarActivityStatusHolder.Visibility = Visibility.Collapsed;
            SelectStatusButton.IsChecked = false;
        }

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
        /// Broadcast avatar movement.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task BroadcastAvatarMovement(PointerRoutedEventArgs e)
        {
            if (Canvas_root.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar avatar && avatar.Id == Avatar.Id) is UIElement iElement)
            {
                var taggedObject = MoveElement(iElement, e);
                var movedAvatar = taggedObject as Avatar;

                var z = Canvas.GetZIndex(iElement);

                await HubService.BroadcastAvatarMovement(avatarId: Avatar.Id, x: movedAvatar.Coordinate.X, y: movedAvatar.Coordinate.Y, z: z);

                Console.WriteLine("Avatar moved.");
            }
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

                Console.WriteLine("Avatar status updated.");
            }
        }

        /// <summary>
        /// Sets the provided activityStatus to the avatar. Updates image with StatusBoundImageUrl.
        /// </summary>
        /// <param name="avatarButton"></param>
        /// <param name="avatar"></param>
        /// <param name="activityStatus"></param>
        public void SetAvatarActivityStatus(Button avatarButton, Avatar avatar, ActivityStatus activityStatus)
        {
            // Set avatar activity status
            avatar.ActivityStatus = activityStatus;

            // Update image according to status
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

            obj.RenderTransformOrigin = new Windows.Foundation.Point(0.5f, 0.5f);
            obj.RenderTransform = new ScaleTransform();

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
        /// Clears multi selected constructs.
        /// </summary>
        private void ClearMultiselectedConstructs()
        {
            ConstructMultiSelectButton.IsChecked = false;
            ConstructMultiSelectButton.Content = "Multiselect";
            MultiSelectedConstructsHolder.Children.Clear();
            MultiselectedConstructs.Clear();
        }

        /// <summary>
        /// Broadcasts construct rotation operation.
        /// </summary>
        /// <param name="_selectedConstruct"></param>
        /// <returns></returns>
        private async Task BroadcastConstructRotate(UIElement _selectedConstruct)
        {
            var button = (Button)_selectedConstruct;

            var construct = button.Tag as Construct;

            var newRotation = construct.Rotation + 5;

            construct = RotateElement(_selectedConstruct, newRotation) as Construct;

            // Align avatar to construct point
            AlignAvatarFaceDirection(construct.Coordinate.X);

            await HubService.BroadcastConstructRotation(construct.Id, construct.Rotation);

            Console.WriteLine("Construct rotated.");
        }

        /// <summary>
        /// Broadcasts construct sacle down operation.
        /// </summary>
        /// <param name="_selectedConstruct"></param>
        /// <returns></returns>
        private async Task BroadcastConstructScaleDown(UIElement _selectedConstruct)
        {
            var button = (Button)_selectedConstruct;

            var construct = button.Tag as Construct;

            if (construct.Scale == 0.25f)
            {
                return;
            }

            var newScale = construct.Scale - 0.25f;

            construct = ScaleElement(_selectedConstruct, newScale) as Construct;

            // Align avatar to construct point
            AlignAvatarFaceDirection(construct.Coordinate.X);

            await HubService.BroadcastConstructScale(construct.Id, construct.Scale);

            Console.WriteLine("Construct scaled down.");
        }

        /// <summary>
        /// Broadcasts construct scale up operation.
        /// </summary>
        /// <param name="_selectedConstruct"></param>
        /// <returns></returns>
        private async Task BroadcastConstructScaleUp(UIElement _selectedConstruct)
        {
            var button = (Button)_selectedConstruct;

            var construct = button.Tag as Construct;

            var newScale = construct.Scale + 0.25f;

            construct = ScaleElement(_selectedConstruct, newScale) as Construct;

            // Align avatar to construct point
            AlignAvatarFaceDirection(construct.Coordinate.X);

            await HubService.BroadcastConstructScale(construct.Id, construct.Scale);

            Console.WriteLine("Construct scaled up.");
        }

        /// <summary>
        /// Broadcasts construct send back operation.
        /// </summary>
        /// <param name="_selectedConstruct"></param>
        /// <returns></returns>
        private async Task BroadcastConstructSendBackward(UIElement _selectedConstruct)
        {
            var zIndex = Canvas.GetZIndex(_selectedConstruct);
            zIndex--;
            Canvas.SetZIndex(_selectedConstruct, zIndex);

            var construct = ((Button)_selectedConstruct).Tag as Construct;

            // Align avatar to construct point
            AlignAvatarFaceDirection(construct.Coordinate.X);

            await HubService.BroadcastConstructPlacement(construct.Id, zIndex);
        }

        /// <summary>
        /// Broadcasts construct bring forward operation.
        /// </summary>
        /// <param name="_selectedConstruct"></param>
        /// <returns></returns>
        private async Task BroadcastConstructBringForward(UIElement _selectedConstruct)
        {
            var zIndex = Canvas.GetZIndex(_selectedConstruct);
            zIndex++;
            Canvas.SetZIndex(_selectedConstruct, zIndex);

            var construct = ((Button)_selectedConstruct).Tag as Construct;

            // Align avatar to construct point
            AlignAvatarFaceDirection(construct.Coordinate.X);

            await HubService.BroadcastConstructPlacement(construct.Id, zIndex);
        }

        /// <summary>
        /// Broadcasts construct delete operation.
        /// </summary>
        /// <param name="_selectedConstruct"></param>
        /// <returns></returns>
        private async Task BroadcastConstructDelete(UIElement _selectedConstruct)
        {
            var construct = ((Button)_selectedConstruct).Tag as Construct;

            // Align avatar to construct point
            AlignAvatarFaceDirection(construct.Coordinate.X);

            Canvas_root.Children.Remove(_selectedConstruct);
            ShowSelectedConstruct(null);

            await HubService.RemoveConstruct(construct.Id);
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
                Style = Application.Current.Resources["MaterialDesign_GlassButton_Style"] as Style,
                Name = id.ToString(),
                Tag = new Construct()
                {
                    Id = id,
                    Name = name,
                    ImageUrl = uri,
                    Creator = creator ?? new Creator() { Id = User.Id, Name = User.Name, ImageUrl = User.ImageUrl },
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
                    Rotation = rotation,
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

            Button chatBubble = new Button()
            {
                Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                FontWeight = FontWeights.Regular,
                FontFamily = new FontFamily("Segoe UI"),
                MaxWidth = 600,
                Height = double.PositiveInfinity,
            };

            var x = taggedAvatar.Coordinate.X - (avatarButton.ActualWidth / 2);
            var y = taggedAvatar.Coordinate.Y - (avatarButton.ActualHeight / 2);

            // Prepare content
            StackPanel chatContent = new StackPanel() { Orientation = Orientation.Horizontal };

            Image avatarImage = new Image()
            {
                Source = new BitmapImage(((BitmapImage)((Image)avatarButton.Content).Source).UriSource),
                Height = 30,
                Width = 30,
                Stretch = Stretch.Uniform,
            };

            // Textblock containing the message
            var textBlock = new TextBlock()
            {
                Text = msg,
                Margin = new Thickness(5, 0, 5, 0),
                FontWeight = FontWeights.Regular,
                FontFamily = new FontFamily("Segoe UI"),
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Colors.Black),
            };

            // If own message then image on the left
            if (taggedAvatar.Id == Avatar.Id)
            {
                Button meUiElement = Canvas_root.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar taggedAvatar && taggedAvatar.Id == Avatar.Id);

                var sender = taggedAvatar;
                var receiver = ((Button)_messageToAvatar).Tag as Avatar;

                // If receiver avatar is forward from current avatar
                if (receiver.Coordinate.X > sender.Coordinate.X)
                {
                    meUiElement.RenderTransform = new ScaleTransform() { ScaleX = 1 };
                }
                else
                {
                    meUiElement.RenderTransform = new ScaleTransform() { ScaleX = -1 };
                }

                chatContent.Children.Add(avatarImage);
                chatContent.Children.Add(textBlock);
            }
            else
            {
                Button meUiElement = Canvas_root.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar meAvatar && meAvatar.Id == Avatar.Id);
                Button senderUiElement = Canvas_root.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar senderAvatar && senderAvatar.Id == taggedAvatar.Id);

                var receiver = meUiElement.Tag as Avatar;
                var sender = taggedAvatar;

                // If sender avatar is forward from current avatar
                if (sender.Coordinate.X > receiver.Coordinate.X)
                {
                    senderUiElement.RenderTransform = new ScaleTransform() { ScaleX = -1 };
                }
                else
                {
                    senderUiElement.RenderTransform = new ScaleTransform() { ScaleX = 1 };
                }

                chatBubble.Tag = taggedAvatar;
                chatBubble.PointerPressed += ChatBubble_PointerPressed;

                chatContent.Children.Add(textBlock);
                chatContent.Children.Add(avatarImage);
            }

            chatBubble.Content = chatContent;

            // Set opacity animation according to the length of the message
            DoubleAnimation opacityAnimation = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(100),
            };

            DoubleAnimation moveYAnimation = new DoubleAnimation()
            {
                From = y,
                To = y - 300,
                Duration = TimeSpan.FromSeconds(100),
                EasingFunction = new ExponentialEase()
                {
                    EasingMode = EasingMode.EaseOut,
                    Exponent = 10,
                }
            };

            // after opacity reaches zero delete this from canvas
            opacityAnimation.Completed += (s, e) =>
            {
                Canvas_root.Children.Remove(chatBubble);
            };

            Storyboard.SetTarget(opacityAnimation, chatBubble);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(OpacityProperty));

            Storyboard.SetTarget(moveYAnimation, chatBubble);
            Storyboard.SetTargetProperty(moveYAnimation, new PropertyPath(Canvas.TopProperty));

            Storyboard fadeStoryBoard = new Storyboard();
            fadeStoryBoard.Children.Add(opacityAnimation);
            fadeStoryBoard.Children.Add(moveYAnimation);

            // Add to canvas
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