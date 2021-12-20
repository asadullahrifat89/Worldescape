using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Service;
using Worldescape.Common;
using Image = Windows.UI.Xaml.Controls.Image;
using Windows.UI.Input;

namespace Worldescape
{
    public partial class InsideWorldPage : Page
    {
        #region Fields

        bool _isLoggedIn;
        float zoomFactor = 0.15f;

        UIElement _selectedConstruct;
        UIElement _addingConstruct;
        UIElement _movingConstruct;
        UIElement _cloningConstruct;

        UIElement _addingPortal;

        Border _pointerImage;

        UIElement _selectedAvatar;
        UIElement _messageToAvatar;

        UIElement _selectedPortal;

        ChatMessage _replyToChatMessage;

        readonly IHubService _hubService;
        readonly AvatarHelper _avatarHelper;
        readonly ConstructHelper _constructHelper;
        readonly WorldHelper _worldHelper;
        readonly PortalHelper _portalHelper;
        readonly PaginationHelper _paginationHelper;
        readonly ElementHelper _elementHelper;
        readonly ChatBubbleHelper _chatBubbleHelper;

        readonly ConstructRepository _constructRepository;
        readonly AvatarRepository _avatarRepository;

        #endregion

        #region Ctor
        public InsideWorldPage(
            IHubService hubService,
            AvatarHelper avatarHelper,
            WorldHelper worldHelper,
            PortalHelper portalHelper,
            ConstructHelper constructHelper,
            PaginationHelper paginationHelper,
            ElementHelper elementHelper,
            ChatBubbleHelper chatBubbleHelper,
            ConstructRepository constructRepository,
            AvatarRepository avatarRepository)
        {
            InitializeComponent();

            _hubService = hubService;
            _avatarHelper = avatarHelper;
            _worldHelper = worldHelper;
            _portalHelper = portalHelper;
            _constructHelper = constructHelper;
            _paginationHelper = paginationHelper;
            _elementHelper = elementHelper;
            _chatBubbleHelper = chatBubbleHelper;
            _constructRepository = constructRepository;
            _avatarRepository = avatarRepository;

            SubscribeHub();
        }

        #endregion

        #region Properties

        List<Character> Characters { get; set; } = new List<Character>();

        Avatar Avatar { get; set; } = null;

        Character Character { get; set; } = new Character();

        ObservableCollection<AvatarMessenger> AvatarMessengers { get; set; } = new ObservableCollection<AvatarMessenger>();

        List<Construct> MultiselectedConstructs { get; set; } = new List<Construct>();

        #endregion

        #region Methods

        #region Page Events

        /// <summary>
        ///  Event fired when this page is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            SetDefault();
        }

        /// <summary>
        /// Event fired when this page is unloaded. Logs out the current user from hub, disconnects from hub and unsubscribes from all hub events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Page_Unloaded(
            object sender,
            RoutedEventArgs e)
        {
            if (_hubService != null)
            {
                await LogoutFromHubThenDisconnect();
                Character = new Character();
            }
        }

        #endregion

        #region Pointer Events

        #region Canvas

        /// <summary>
        /// Event fired on pointer press on canvas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Canvas_Root_PointerPressed(
            object sender,
            PointerRoutedEventArgs e)
        {
            if (!CanPerformWorldEvents())
                return;

            if (_addingPortal != null)
            {
                await AddPortalOnPointerPressed(e);
            }
            else if (_addingConstruct != null)
            {
                await AddConstructOnPointerPressed(e); // Canvas_Root
            }
            else if (Button_ConstructClone.IsChecked.Value && _cloningConstruct != null)
            {
                await CloneConstructOnPointerPressed(e); // Canvas_Root
            }
            else if (Button_ConstructMove.IsChecked.Value && _movingConstruct != null)
            {
                await MoveConstructOnPointerPressed(e); // Canvas_Root
            }
            else
            {
                // Clear construct selection
                _selectedConstruct = null;
                ShowSelectedConstruct(null); // Canvas

                _movingConstruct = null;
                ShowOperationalConstruct(null);

                _selectedAvatar = null;
                ShowSelectedAvatar(null);

                _messageToAvatar = null;
                ShowMessagingAvatars();

                HideConstructOperationButtons();
                HideAvatarOperationButtons();
                HideMessagingControls();
                HidePortalActionsHolder();

                ClearMultiselectedConstructs();
                HideAvatarActivityStatusHolder();
            }
        }

        /// <summary>
        /// Event fired on pointer movement on canvas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_Root_PointerMoved(
            object sender,
            PointerRoutedEventArgs e)
        {
            MoveAttachedPointerElement(e);
        }

        #endregion

        #region Avatar

        /// <summary>
        ///  Event fired on pointer press on an avatar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Avatar_PointerPressed(
            object sender,
            PointerRoutedEventArgs e)
        {
            if (!CanPerformWorldEvents())
                return;

            UIElement uielement = (UIElement)sender;
            _selectedAvatar = uielement;

            ShowSelectedAvatar(uielement);

            if (uielement is Button button && button.Tag is Avatar avatar)
            {
                // If selected own avatar
                if (avatar.Id == Avatar.Id)
                {
                    // Show commands for self
                    PopElementContextCommands(e: e, uIElement: OwnAvatarActionsHolder);

                    ShowOwnAvatarActionsHolder();
                    HideOtherAvatarActions();
                }
                else
                {
                    // Show commands for other
                    PopElementContextCommands(e: e, uIElement: OtherAvatarActionsHolder);

                    ShowOtherAvatarActionsHolder();
                    HideOwnAvatarActions();

                    Button_SelectStatus.IsChecked = false;
                    // Hide activity status menu items
                    HideAvatarActivityStatusHolder();
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
        private async void Construct_PointerPressed(
            object sender,
            PointerRoutedEventArgs e)
        {
            if (!CanPerformWorldEvents())
                return;

            UIElement uielement = (UIElement)sender;
            _selectedConstruct = uielement;

            ShowSelectedConstruct(uielement); // Construct

            if (_addingPortal != null)
            {
                await AddPortalOnPointerPressed(e);
            }
            else if (_addingConstruct != null)
            {
                await AddConstructOnPointerPressed(e); // Construct
            }
            else if (Button_ConstructClone.IsChecked.Value && _cloningConstruct != null)
            {
                await CloneConstructOnPointerPressed(e); // Construct
            }
            else if (Button_ConstructMove.IsChecked.Value && _movingConstruct != null)
            {
                await MoveConstructOnPointerPressed(e); // Construct
            }
            else if (Button_ConstructMultiSelect.IsChecked.Value)
            {
                if (!CanManipulateConstruct())
                    return;

                // Add the selected construct to multi selected list
                var construct = ((Button)_selectedConstruct).Tag as Construct;

                if (!MultiselectedConstructs.Contains(construct))
                {
                    MultiSelectedConstructsHolder.Children.Add(GetImageFromUiElement(_selectedConstruct));
                    MultiselectedConstructs.Add(construct);
                }

                ShowConstructOperationButtons(e); // Multi select
            }
            else if (Button_ConstructCraft.IsChecked.Value)
            {
                if (!CanManipulateConstruct())
                {
                    HideConstructOperationButtons();
                    return;
                }

                ShowConstructOperationButtons(e); // Construct selection

                DragStart(Canvas_Root, e, uielement);

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
        private void Construct_PointerMoved(
            object sender,
            PointerRoutedEventArgs e)
        {
            if (Button_ConstructCraft.IsChecked.Value)
            {
                if (!CanManipulateConstruct())
                    return;

                UIElement uielement = (UIElement)sender;
                DragElement(Canvas_Root, e, uielement);
            }
        }

        /// <summary>
        /// Event fired when pointer is released from a construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Construct_PointerReleased(
            object sender,
            PointerRoutedEventArgs e)
        {
            if (Button_ConstructMove.IsChecked.Value)
                return;

            if (Button_ConstructCraft.IsChecked.Value)
            {
                if (!CanManipulateConstruct())
                    return;

                // Drag drop selected construct
                UIElement uielement = (UIElement)sender;
                DragRelease(e, uielement);

                _selectedConstruct = uielement;
                ShowSelectedConstruct(uielement); // Construct

                var x = Canvas.GetLeft(uielement);
                var y = Canvas.GetTop(uielement);
                var z = Canvas.GetZIndex(uielement);

                var construct = ((Button)uielement).Tag as Construct;

                construct.Coordinate.X = x;
                construct.Coordinate.Y = y;
                construct.Coordinate.Z = z;

                await _hubService.BroadcastConstructMovement(
                    constructId: construct.Id,
                    x: construct.Coordinate.X,
                    y: construct.Coordinate.Y,
                    z: construct.Coordinate.Z);

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
            if (Button_ConstructMultiSelect.IsChecked.Value && MultiselectedConstructs.Any())
            {
                await MoveConstructs(e);
            }
            else
            {
                await MoveConstruct(e);
            }

            ShowConstructOperationButtons(e); // Move
        }

        /// <summary>
        /// Clones the _cloningConstruct to the pressed point.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task CloneConstructOnPointerPressed(PointerRoutedEventArgs e)
        {
            if (Button_ConstructMultiSelect.IsChecked.Value && MultiselectedConstructs.Any())
            {
                await CloneConstructs(e);
            }
            else
            {
                await CloneConstruct(e);
            }

            ShowConstructOperationButtons(e); // Clone
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
                var pressedPoint = e.GetCurrentPoint(Canvas_Root);

                var pointX = NormalizePointerX(pressedPoint);
                var pointY = NormalizePointerY(pressedPoint);

                var constructButton = GenerateConstructButton(
                           name: constructAsset.Name,
                           imageUrl: constructAsset.ImageUrl);

                // Add the construct on pressed point
                var construct = AddConstructOnCanvas(
                    construct: constructButton,
                    x: pointX,
                    y: pointY);

                // Center the construct on pressed point
                construct = CenterAlignNewConstructButton(
                    pressedPoint: pressedPoint,
                    constructButton: constructButton,
                    construct: construct);

                // Align avatar to construct point
                AlignAvatarFaceDirectionWrtX(gotoX: construct.Coordinate.X);

                await _hubService.BroadcastConstruct(construct);

                Console.WriteLine("Construct added.");

                // Turn off add mode
                _addingConstruct = null;
                ShowOperationalConstruct(null);
                ReleaseAttachedPointerElement();
            }
        }

        #endregion

        #region Message

        /// <summary>
        /// Event fired on pointer press on a chat bubble. This starts a replay conversation with the message sender.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChatBubble_PointerPressed(
            object sender,
            PointerRoutedEventArgs e)
        {
            // reply to the selected message and show Messaging controls
            if (((Button)sender).Tag is ChatMessage chatMessage)
            {
                _replyToChatMessage = chatMessage;

                // Turn of broadcast mode as replying to this user's conversation.
                Button_MessageAll.IsChecked = false;

                _messageToAvatar = _avatarHelper.GetAvatarButtonFromCanvas(canvas: Canvas_Root, avatarId: chatMessage.SenderId);
                _selectedAvatar = _messageToAvatar;

                ShowMessagingAvatars(_messageToAvatar);
                ShowMessagingControls();

                ShowSelectedAvatar(_selectedAvatar);
                SetAvatarDetailsOnSideCard();
            }
        }

        #endregion

        #region Portal

        private void Portal_PointerPressed(
            object sender,
            PointerRoutedEventArgs e)
        {
            _selectedPortal = (UIElement)sender;

            PopElementContextCommands(e: e, uIElement: PortalActionsHolder);
            ShowPortalActionsHolder();
        }

        private async Task AddPortalOnPointerPressed(PointerRoutedEventArgs e)
        {
            if (_addingPortal == null)
                return;

            var button = (Button)_addingPortal;

            var pressedPoint = e.GetCurrentPoint(Canvas_Root);

            var pointX = NormalizePointerX(pressedPoint);
            var pointY = NormalizePointerY(pressedPoint);

            // Add the portal on pressed point
            var portal = AddPortalOnCanvas(
                portal: button,
                x: pointX,
                y: pointY);

            // Center the portal on pressed point
            portal = CenterAlignNewPortalButton(
                pressedPoint: pressedPoint,
                portalButton: button,
                portal: portal);

            // Align avatar to portal point
            AlignAvatarFaceDirectionWrtX(gotoX: portal.Coordinate.X);

            await _hubService.BroadcastPortal(portal);

            Console.WriteLine("Portal added.");

            // Turn off add mode
            _addingPortal = null;
            ReleaseAttachedPointerElement();

            HideAvatarOperationButtons();
        }

        #endregion

        #endregion

        #region Button Events

        #region World

        private void Button_ShowAvatars_Click(object sender, RoutedEventArgs e)
        {
            if (Button_ShowAvatars.IsChecked.Value)
            {
                ContentControl_ActiveAvatarsContainer.Visibility = Visibility.Visible;
                PopulateAvatarsInAvatarsContainer();
            }
            else
            {
                ContentControl_ActiveAvatarsContainer.Visibility = Visibility.Collapsed;
            }
        }

        private void Button_World_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_ZoomInCanvas_Click(object sender, RoutedEventArgs e)
        {
            Canvas_CompositeTransform.ScaleX += zoomFactor;
            Canvas_CompositeTransform.ScaleY += zoomFactor;
        }

        private void Button_ZoomOutCanvas_Click(object sender, RoutedEventArgs e)
        {
            if (Canvas_CompositeTransform.ScaleX == 0.25f)
            {
                return;
            }

            Canvas_CompositeTransform.ScaleX -= zoomFactor;
            Canvas_CompositeTransform.ScaleY -= zoomFactor;
        }

        #endregion

        #region Connection

        /// <summary>
        /// Logs out and disconnects the current user from Hub.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Leave_Click(object sender, RoutedEventArgs e)
        {
            LeaveWorld();
        }

        #endregion

        #region Construct

        /// <summary>
        /// Activates crafting mode. This enables operations buttons for a construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_ConstructCraft_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                Button_ConstructMove.IsChecked = false;

                Button_ConstructClone.IsChecked = false;

                Button_ConstructAdd.IsChecked = false;

                //ToggleButton_PanCanvas.IsChecked = false;

                HideConstructAssetsControl();

                Button_ConstructMultiSelect.IsChecked = false;

                ReleaseAttachedPointerElement();

                if (Button_ConstructCraft.IsChecked.Value)
                {
                    Button_ConstructAdd.Visibility = Visibility.Visible;
                    Button_ConstructMultiSelect.Visibility = Visibility.Visible;

                    await BroadcastAvatarActivityStatus(ActivityStatus.Crafting);
                }
                else
                {
                    Button_ConstructAdd.Visibility = Visibility.Collapsed;
                    Button_ConstructMultiSelect.Visibility = Visibility.Collapsed;

                    HideConstructOperationButtons();

                    _movingConstruct = null;
                    _cloningConstruct = null;
                    _addingConstruct = null;

                    ShowOperationalConstruct(null);

                    await BroadcastAvatarActivityStatus(ActivityStatus.Online);
                }
            }
        }

        /// <summary>
        /// Activates adding a construct. Shows the asset picker for picking a construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ConstructAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                if (Button_ConstructAdd.IsChecked.Value)
                {
                    ShowConstructAssetsControl();
                }
                else
                {
                    HideConstructAssetsControl();
                    //ShowOperationalConstruct(null);
                    //ReleaseAssignedPointerElement();
                }
            }
        }

        /// <summary>
        /// Activates multi selection of clicked constructs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ConstructMultiSelect_Click(object sender, RoutedEventArgs e)
        {
            MultiSelectedConstructsHolder.Children.Clear();
            MultiselectedConstructs.Clear();

            ReleaseAttachedPointerElement();
            Button_ConstructMove.IsChecked = false;
            Button_ConstructClone.IsChecked = false;

            if (!Button_ConstructMultiSelect.IsChecked.Value)
            {
                HideConstructOperationButtons();
                Button_ConstructClone.IsChecked = false;
                Button_ConstructMove.IsChecked = false;
                ShowOperationalConstruct(null);
            }
        }

        /// <summary>
        /// Toggles moving mode for the selected construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ConstructMove_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                if (!Button_ConstructMove.IsChecked.Value)
                {
                    _movingConstruct = null;
                    ShowOperationalConstruct(null);
                    ReleaseAttachedPointerElement();
                }
                else
                {
                    UIElement uielement = _selectedConstruct;
                    _movingConstruct = uielement;
                    ShowOperationalConstruct(_movingConstruct, "Moving...");

                    AttachPointerElement(uielement);
                }
            }
        }

        /// <summary>
        /// Activates cloning mode of a selected construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ConstructClone_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                if (!Button_ConstructClone.IsChecked.Value)
                {
                    _cloningConstruct = null;
                    ShowOperationalConstruct(null);
                    ReleaseAttachedPointerElement();
                }
                else
                {
                    UIElement uielement = _selectedConstruct;
                    _cloningConstruct = uielement;
                    ShowOperationalConstruct(_cloningConstruct, "Cloning...");

                    AttachPointerElement(uielement);
                }
            }
        }

        /// <summary>
        /// Deletes the selected construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_ConstructDelete_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                if (Button_ConstructMultiSelect.IsChecked.Value && MultiselectedConstructs.Any())
                {
                    foreach (var element in MultiselectedConstructs)
                    {
                        _selectedConstruct = _constructHelper.GetConstructButtonFromCanvas(canvas: Canvas_Root, constructId: element.Id);

                        if (_selectedConstruct != null)
                        {
                            await BroadcastConstructDelete(_selectedConstruct);
                        }
                    }

                    ClearMultiselectedConstructs();
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
        private async void Button_ConstructBringForward_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                if (Button_ConstructMultiSelect.IsChecked.Value && MultiselectedConstructs.Any())
                {
                    foreach (var element in MultiselectedConstructs)
                    {
                        _selectedConstruct = _constructHelper.GetConstructButtonFromCanvas(canvas: Canvas_Root, constructId: element.Id);

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
        private async void Button_ConstructSendBackward_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                if (Button_ConstructMultiSelect.IsChecked.Value && MultiselectedConstructs.Any())
                {
                    foreach (var element in MultiselectedConstructs)
                    {
                        _selectedConstruct = _constructHelper.GetConstructButtonFromCanvas(canvas: Canvas_Root, constructId: element.Id);

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
        private async void Button_ConstructScaleUp_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                if (Button_ConstructMultiSelect.IsChecked.Value && MultiselectedConstructs.Any())
                {
                    foreach (var element in MultiselectedConstructs)
                    {
                        _selectedConstruct = _constructHelper.GetConstructButtonFromCanvas(canvas: Canvas_Root, constructId: element.Id);

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
        private async void Button_ConstructScaleDown_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                if (Button_ConstructMultiSelect.IsChecked.Value && MultiselectedConstructs.Any())
                {
                    foreach (var element in MultiselectedConstructs)
                    {
                        _selectedConstruct = _constructHelper.GetConstructButtonFromCanvas(canvas: Canvas_Root, constructId: element.Id);

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
        /// Rotates the selected construct forward.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_ConstructRotateForward_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                if (Button_ConstructMultiSelect.IsChecked.Value && MultiselectedConstructs.Any())
                {
                    foreach (var element in MultiselectedConstructs)
                    {
                        _selectedConstruct = _constructHelper.GetConstructButtonFromCanvas(canvas: Canvas_Root, constructId: element.Id);

                        if (_selectedConstruct != null)
                        {
                            await BroadcastConstructRotate(selectedConstruct: _selectedConstruct);
                        }
                    }
                }
                else
                {
                    if (_selectedConstruct != null)
                    {
                        await BroadcastConstructRotate(selectedConstruct: _selectedConstruct);
                    }
                }
            }
        }

        /// <summary>
        /// Rotates the selected construct backward.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_ConstructRotateBackward_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                if (Button_ConstructMultiSelect.IsChecked.Value && MultiselectedConstructs.Any())
                {
                    foreach (var element in MultiselectedConstructs)
                    {
                        _selectedConstruct = _constructHelper.GetConstructButtonFromCanvas(canvas: Canvas_Root, constructId: element.Id);

                        if (_selectedConstruct != null)
                        {
                            await BroadcastConstructRotate(selectedConstruct: _selectedConstruct, isBacward: true);
                        }
                    }
                }
                else
                {
                    if (_selectedConstruct != null)
                    {
                        await BroadcastConstructRotate(selectedConstruct: _selectedConstruct, isBacward: true);
                    }
                }
            }
        }

        #endregion

        #region Avatar

        private void Button_CreatePortal_Click(object sender, RoutedEventArgs e)
        {
            WorldSelectionWindow WorldSelectionWindow = new WorldSelectionWindow();
            WorldSelectionWindow.WorldSelected += (sender, world) =>
            {
                Button btnPortal = GeneratePortalButton(world);
                _addingPortal = btnPortal;
                AttachPointerElement(btnPortal);
            };
            WorldSelectionWindow.Show();
        }

        private void ActiveAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is Avatar avatar)
            {
                ScrollIntoView(avatar);
            }
        }

        /// <summary>
        /// Sets selected avatar details on side card.
        /// </summary>
        private void SetAvatarDetailsOnSideCard()
        {
            if (_selectedAvatar == null)
                return;

            if (((Button)_selectedAvatar).Tag is Avatar avatar)
            {
                var twoImages = new Grid() { };

                var characterPicture = GetAvatarCharacterPicture(avatar: avatar);
                var userPicture = GetAvatarUserPicture(avatar: avatar, size: 100);

                characterPicture.VerticalAlignment = VerticalAlignment.Bottom;
                characterPicture.HorizontalAlignment = HorizontalAlignment.Right;

                twoImages.Children.Add(userPicture);
                twoImages.Children.Add(characterPicture);

                DetailsImageHolder.Content = twoImages;
                DetailsNameHolder.Text = avatar.Name;
                DetailsDateHolder.Text = avatar.CreatedOn.ToShortTimeString();
            }
        }

        /// <summary>
        /// Scrolls current avatar into view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MyAvatar_Click(object sender, RoutedEventArgs e)
        {
            ScrollIntoView(Avatar);

            var avatarUIElement = _avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, Avatar.Id) as Button;

            var pointX = Canvas.GetLeft(avatarUIElement) + avatarUIElement.ActualWidth / 2 + 3;
            var pointY = Canvas.GetTop(avatarUIElement) + avatarUIElement.ActualHeight;

            Canvas.SetLeft(OwnAvatarActionsHolder, pointX);
            Canvas.SetTop(OwnAvatarActionsHolder, pointY);

            ShowOwnAvatarActionsHolder();
            HideOtherAvatarActions();
        }

        /// <summary>
        /// Activates status options.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_SelectStatus_Click(object sender, RoutedEventArgs e)
        {
            if (Button_SelectStatus.IsChecked.Value)
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
        private async void MenuItem_SetStatus_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;

            ActivityStatus[] values = Enum.GetValues(typeof(ActivityStatus)).Cast<ActivityStatus>().ToArray();
            ActivityStatus enumMember = values.FirstOrDefault(x => x.ToString() == menuItem.Content.ToString());

            await BroadcastAvatarActivityStatus(enumMember);
        }

        #endregion

        #region Message

        /// <summary>
        /// Activates unicast messaging controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MessageAvatar_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPerformWorldEvents())
                return;

            // Turn off broadcast mode
            Button_MessageAll.IsChecked = false;

            if (_selectedAvatar == null)
                return;

            // Show messenge from and to avatars and show messaging controls
            if (((Button)_selectedAvatar).Tag is Avatar avatar)
            {
                _messageToAvatar = _avatarHelper.GetAvatarButtonFromCanvas(canvas: Canvas_Root, avatarId: avatar.Id);
                ShowMessagingAvatars(_messageToAvatar);
                ShowMessagingControls();
                SetAvatarDetailsOnSideCard();

                MessagingTextBox.Focus();
                HideOtherAvatarActions();
            }
        }

        /// <summary>
        /// Activcates broadcast messaging controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MessageAll_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPerformWorldEvents())
                return;

            _messageToAvatar = null;
            ShowMessagingAvatars();
            ShowMessagingControls();

            MessagingTextBox.Focus();
        }

        /// <summary>
        /// Event fired upon key press inside the chat box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MessagingTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (Button_MessageAll.IsChecked.Value)
                    await SendBroadcastMessage();
                else
                    await SendUnicastMessage();
            }
        }

        /// <summary>
        /// Sends unicast message to the selected avatar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_SendUnicastMessage_Click(object sender, RoutedEventArgs e)
        {
            await SendUnicastMessage();
        }

        /// <summary>
        ///  Sends broadcast message to all avatars.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_SendBroadcastMessage_Click(object sender, RoutedEventArgs e)
        {
            await SendBroadcastMessage();
        }

        /// <summary>
        /// Activates post creation window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_CreatePost_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPerformWorldEvents())
                return;

            if (_selectedAvatar == null)
                return;
        }

        #endregion

        #region Details

        /// <summary>
        /// Shows the side card.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ObjectDetails_Click(object sender, RoutedEventArgs e)
        {
            // If nothing is selected then no point in showing the side card.
            if (_selectedAvatar == null && _selectedConstruct == null)
                return;

            SideCard.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides the side card.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_CloseSideCard_Click(object sender, RoutedEventArgs e)
        {
            SideCard.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Portal

        private void Button_Teleport_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPortal == null)
                return;

            var portal = ((Button)_selectedPortal).Tag as Portal;

            var contentDialogue = new MessageDialogueWindow(title: "Teleport!", message: $"Would you like to go to {portal.World.Name}?", result: async (result) =>
            {
                if (result)
                {
                    if (await LogoutFromHub())
                    {
                        SetDefault();
                        App.World = portal.World;
                        SetAvatarData();

                        App.SetIsBusy(true, "Teleporting to world...");

                        if (await LoginToHub())
                        {
                            App.SetIsBusy(false);
                        }
                    }
                }

                HidePortalActionsHolder();
            });
            contentDialogue.Show();
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
                Button_ObjectDetails.Visibility = Visibility.Collapsed;
            }
            else
            {
                Button_ObjectDetails.Visibility = Visibility.Visible;

                var image = GetImageFromUiElement(uielement: uielement, size: 70);

                SelectedConstructHolder.Content = image;
                SelectedConstructHolder.Visibility = Visibility.Visible;

                SetConstructDetailsOnSideCard();
            }
        }

        /// <summary>
        /// Shows the operational construct when adding, moving, cloning.
        /// </summary>
        /// <param name="uielement"></param>
        private void ShowOperationalConstruct(UIElement uielement, string message = null)
        {
            if (uielement == null)
            {
                OperationalConstructHolder.Content = null;
                OperationalConstructHolder.Visibility = Visibility.Collapsed;
            }
            else
            {
                var image = GetImageFromUiElement(uielement: uielement, size: 70);

                StackPanel spContent = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                spContent.Children.Add(image);

                if (!message.IsNullOrBlank())
                    spContent.Children.Add(new Label()
                    {
                        Content = message,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center
                    });

                OperationalConstructHolder.Content = spContent;
                OperationalConstructHolder.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Shows the selected avatar with character and user images.
        /// </summary>
        /// <param name="uielement"></param>
        private void ShowSelectedAvatar(UIElement uielement)
        {
            if (uielement == null)
            {
                SelectedAvatarHolder.Content = null;
                SelectedAvatarHolder.Visibility = Visibility.Collapsed;
                Button_ObjectDetails.Visibility = Visibility.Collapsed;
            }
            else
            {
                Button_ObjectDetails.Visibility = Visibility.Visible;

                var taggedAvatar = ((Button)uielement).Tag as Avatar;

                var userImage = GetAvatarUserPicture(avatar: taggedAvatar, size: 80);
                var avatarImage = GetAvatarCharacterPicture(avatar: taggedAvatar, size: 30);
                avatarImage.HorizontalAlignment = HorizontalAlignment.Right;
                avatarImage.VerticalAlignment = VerticalAlignment.Bottom;

                Grid content = new Grid();

                content.Children.Add(userImage);
                content.Children.Add(avatarImage);

                SelectedAvatarHolder.Content = content;
                SelectedAvatarHolder.Visibility = Visibility.Visible;

                SetAvatarDetailsOnSideCard();
            }
        }

        #endregion

        #region Avatar Images

        /// <summary>
        /// Show avatar character images in and around message controls.
        /// </summary>
        /// <param name="receiverUiElement"></param>
        private void ShowMessagingAvatars(UIElement receiverUiElement = null)
        {
            MessagingFromAvatarHolder.Content = _avatarHelper.GetAvatarUserPictureFrame(Avatar);

            if (receiverUiElement == null)
            {
                MessagingToAvatarHolder.Content = null;
                _replyToChatMessage = null;
            }
            else
            {
                var receiver = ((Button)receiverUiElement).Tag as Avatar;
                AlignAvatarFaceDirectionWrtX(receiver.Coordinate.X);
                MessagingToAvatarHolder.Content = _avatarHelper.GetAvatarUserPictureFrame(receiver);
            }
        }

        /// <summary>
        /// Shows messaging controls.
        /// </summary>
        private void ShowMessagingControls()
        {
            MessagingControlsHolder.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides messaging controls.
        /// </summary>
        private void HideMessagingControls()
        {
            MessagingControlsHolder.Visibility = Visibility.Collapsed;
            Button_MessageAll.IsChecked = false;
        }

        #endregion

        #region Hub Events

        #region Construct
        private void HubService_NewBroadcastConstructMovement(int constructId, double x, double y, int z)
        {
            if (_constructHelper.GetConstructButtonFromCanvas(canvas: Canvas_Root, constructId: constructId) is UIElement iElement)
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
            if (_constructHelper.GetConstructButtonFromCanvas(canvas: Canvas_Root, constructId: constructId) is UIElement iElement)
            {
                ScaleElement(uIElement: iElement, scale: scale);
                Console.WriteLine("<<HubService_NewBroadcastConstructScale: OK");
            }
            else
            {
                Console.WriteLine("<<HubService_NewBroadcastConstructScale: IGNORE");
            }
        }

        //private void HubService_NewBroadcastConstructScales(int[] constructIds, float scale)
        //{

        //}

        private void HubService_NewBroadcastConstructRotation(int constructId, float rotation)
        {
            if (_constructHelper.GetConstructButtonFromCanvas(canvas: Canvas_Root, constructId: constructId) is UIElement iElement)
            {
                RotateElement(uIElement: iElement, rotation: rotation);
                Console.WriteLine("<<HubService_NewBroadcastConstructRotation: OK");
            }
            else
            {
                Console.WriteLine("<<HubService_NewBroadcastConstructRotation: IGNORE");
            }
        }

        //private void HubService_NewBroadcastConstructRotations(ConcurrentDictionary<int, float> obj)
        //{

        //}

        private void HubService_NewBroadcastConstructPlacement(int constructId, int z)
        {
            if (_constructHelper.GetConstructButtonFromCanvas(canvas: Canvas_Root, constructId: constructId) is UIElement iElement)
            {
                Canvas.SetZIndex(element: iElement, value: z);
                Console.WriteLine("<<HubService_NewBroadcastConstructPlacement: OK");
            }
            else
            {
                Console.WriteLine("<<HubService_NewBroadcastConstructPlacement: IGNORE");
            }
        }

        private void HubService_NewRemoveConstruct(int constructId)
        {
            if (_constructHelper.GetConstructButtonFromCanvas(canvas: Canvas_Root, constructId: constructId) is UIElement constructUiElement)
            {
                RemoveConstructFromCanvas(constructUiElement);
                Console.WriteLine("<<HubService_NewRemoveConstruct: OK");
            }
            else
            {
                Console.WriteLine("<<HubService_NewRemoveConstruct: IGNORE");
            }
        }

        //private void HubService_NewRemoveConstructs(int[] obj)
        //{

        //}

        private void HubService_NewBroadcastConstruct(Construct construct)
        {
            var btnConstruct = GenerateConstructButton(
                name: construct.Name,
                imageUrl: construct.ImageUrl,
                constructId: construct.Id,
                inWorld: construct.World,
                creator: construct.Creator,
                createdOn: construct.CreatedOn);

            AddConstructOnCanvas(
                construct: btnConstruct,
                x: construct.Coordinate.X,
                y: construct.Coordinate.Y,
                z: construct.Coordinate.Z);

            ScaleElement(uIElement: btnConstruct, scale: construct.Scale);
            RotateElement(uIElement: btnConstruct, rotation: construct.Rotation);

            Console.WriteLine("<<HubService_NewBroadcastConstruct: OK");
        }

        //private void HubService_NewBroadcastConstructs(Construct[] obj)
        //{

        //}

        #endregion

        #region Avatar Connection

        private void HubService_AvatarReconnected(int avatarId)
        {
            if (avatarId > 0)
            {
                if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, avatarId) is UIElement iElement)
                {
                    var avatarMessenger = AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == avatarId);

                    if (avatarMessenger != null)
                    {
                        //avatarMessenger.ActivityStatus = ActivityStatus.Idle;
                        avatarMessenger.IsLoggedIn = true;
                    }

                    SetAvatarActivityStatus(
                        avatarButton: (Button)iElement,
                        avatar: (Avatar)((Button)iElement).Tag,
                        activityStatus: ActivityStatus.Online);

                    Console.WriteLine("<<HubService_AvatarReconnected: OK");
                }
                else
                {
                    Console.WriteLine("<<HubService_AvatarReconnected: IGNORE");
                }
            }
        }

        private void HubService_AvatarDisconnected(int avatarId)
        {
            if (avatarId > 0)
            {
                if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, avatarId) is UIElement iElement)
                {
                    var avatarMessenger = AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == avatarId);

                    if (avatarMessenger != null)
                    {
                        //avatarMessenger.ActivityStatus = ActivityStatus.Away;
                        avatarMessenger.IsLoggedIn = false;
                    }

                    SetAvatarActivityStatus(
                        avatarButton: (Button)iElement,
                        avatar: (Avatar)((Button)iElement).Tag,
                        activityStatus: ActivityStatus.Away);

                    Console.WriteLine("<<HubService_AvatarDisconnected: OK");
                }
                else
                {
                    Console.WriteLine("<<HubService_AvatarDisconnected: IGNORE");
                }
            }
        }

        private void HubService_AvatarLoggedOut(int avatarId)
        {
            if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, avatarId) is UIElement iElement)
            {
                var avatarMessenger = AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == avatarId);

                if (avatarMessenger != null)
                {
                    AvatarMessengers.Remove(avatarMessenger);
                    AvatarsCount.Text = AvatarMessengers.Count().ToString();
                }

                Canvas_Root.Children.Remove(iElement);

                PopulateAvatarsInAvatarsContainer();

                Console.WriteLine("<<HubService_AvatarLoggedOut: OK");
            }
            else
            {
                Console.WriteLine("<<HubService_AvatarLoggedOut: IGNORE");
            }
        }

        private void HubService_AvatarLoggedIn(Avatar avatar)
        {
            // If an avatar already exists, remove that
            if (_avatarHelper.GetAvatarButtonFromCanvas(canvas: Canvas_Root, avatarId: avatar.Id) is UIElement iElement)
            {
                Canvas_Root.Children.Remove(iElement);
                AvatarMessengers.Remove(AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == avatar.Id));
            }

            var avatarButton = GenerateAvatarButton(avatar);

            AddAvatarOnCanvas(
                avatar: avatarButton,
                x: avatar.Coordinate.X,
                y: avatar.Coordinate.Y);

            AvatarMessengers.Add(new AvatarMessenger()
            {
                Avatar = avatar,
                //ActivityStatus = avatar.ActivityStatus,
                IsLoggedIn = true
            });

            AvatarsCount.Text = AvatarMessengers.Count().ToString();
            PopulateAvatarsInAvatarsContainer();
            Console.WriteLine("<<HubService_AvatarLoggedIn: OK");
        }

        #endregion

        #region Avatar

        private void HubService_NewBroadcastAvatarActivityStatus(int avatarId, int activityStatus)
        {
            if (avatarId > 0)
            {
                if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, avatarId) is UIElement iElement)
                {
                    //var avatarMessenger = AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == avatarId);

                    //if (avatarMessenger != null)
                    //    avatarMessenger.ActivityStatus = (ActivityStatus)activityStatus;

                    var avatarButton = (Button)iElement;

                    SetAvatarActivityStatus(
                        avatarButton: avatarButton,
                        avatar: avatarButton.Tag as Avatar,
                        activityStatus: (ActivityStatus)activityStatus);

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
                if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, avatarId) is UIElement iElement)
                {
                    //var avatarMessenger = AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == avatarId);

                    //if (avatarMessenger != null)
                    //    avatarMessenger.ActivityStatus = ActivityStatus.Idle;

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

        private void HubService_NewMessage(ChatMessage chatMessage, MessageType mt)
        {
            var senderAvatarId = chatMessage.SenderId;
            var msg = chatMessage.Message;

            if (senderAvatarId > 0)
            {
                if (_avatarHelper.GetAvatarButtonFromCanvas(canvas: Canvas_Root, avatarId: senderAvatarId) is UIElement iElement)
                {
                    var senderAvatarUiElement = (Button)iElement;

                    AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == senderAvatarId)?.Chatter.Add(chatMessage);

                    switch (mt)
                    {
                        case MessageType.Broadcast:
                            {
                                AddChatBubbleToCanvas(chatMessage: chatMessage, fromAvatar: senderAvatarUiElement, messageType: mt); // receive broadcast message
                            }
                            break;
                        case MessageType.Unicast:
                            {
                                var replyToMessage = AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == this.Avatar.Id)?.Chatter.FirstOrDefault(x => x.Id == chatMessage.ReplyToMessageId);

                                AddChatBubbleToCanvas(chatMessage: chatMessage, fromAvatar: senderAvatarUiElement, messageType: mt, replyToChatMessage: replyToMessage); // receive unicast message
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

        #region Hub Connection       

        private async void HubService_ConnectionClosed()
        {
            Console.WriteLine("<<HubService_ConnectionClosed");

            _isLoggedIn = false;

            if (App.World.IsEmpty())
                return;

            if (await ConnectWithHub())
            {
                await TryLoginToHub();
            }
        }

        private async void HubService_ConnectionReconnected()
        {
            _ = await _hubService.Login(Avatar);
            _isLoggedIn = true;
            Console.WriteLine("<<HubService_ConnectionReconnected");
        }

        private void HubService_ConnectionReconnecting()
        {
            _isLoggedIn = false;
            Console.WriteLine("<<HubService_ConnectionReconnecting");
        }

        #endregion

        #region Portal

        private void HubService_NewBroadcastPortal(Portal portal)
        {
            var btnPortal = GeneratePortalButton(world: portal.World);

            AddPortalOnCanvas(
                portal: btnPortal,
                x: portal.Coordinate.X,
                y: portal.Coordinate.Y,
                z: portal.Coordinate.Z);

            Console.WriteLine("<<HubService_NewBroadcastPortal: OK");
        }

        #endregion

        #endregion

        #region Functionality

        #region Page

        /// <summary>
        /// Sets the page to it's defaults.
        /// </summary>
        private void SetDefault()
        {            
            this.SetRandomBackground();

            Visibility = Visibility.Collapsed;

            Button_ConstructMove.IsChecked = false;

            Button_ConstructClone.IsChecked = false;

            Button_ConstructAdd.IsChecked = false;

            Button_ConstructCraft.IsChecked = false;

            Button_ConstructMultiSelect.IsChecked = false;

            Button_ConstructAdd.Visibility = Visibility.Collapsed;
            Button_ConstructMultiSelect.Visibility = Visibility.Collapsed;
            ClearMultiselectedConstructs();

            HideConstructAssetsControl();
            HideConstructOperationButtons();
            HideMessagingControls();
            HidePortalActionsHolder();

            _movingConstruct = null;
            _cloningConstruct = null;
            _addingConstruct = null;
            _addingPortal = null;

            ShowOperationalConstruct(null);

            Canvas_Root.Children.Clear();
        }

        #endregion

        #region World

        /// <summary>
        /// Adds random clouds to the canvas.
        /// </summary>
        /// <param name="drawOver"></param>
        private async void PopulateClouds()
        {
            await _worldHelper.PopulateClouds(
                  canvas: Canvas_Root);

            await _worldHelper.PopulateClouds(
                canvas: Canvas_Root,
                drawOver: true);
        }

        /// <summary>
        /// Shows current world in UI.
        /// </summary>
        private void SetCurrentWorld()
        {
            if (!App.World.IsEmpty())
            {
                Button_World.Tag = App.World;
                Button_World.Visibility = Visibility.Visible;
                WorldImageHolder.Content = GetWorldPicture(App.World);
                WorldNameHolder.Text = App.World.Name;

                Console.WriteLine($"ShowCurrentWorld:OK");
            }
        }

        /// <summary>
        /// Returns the provided world's image in a circular border.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private Border GetWorldPicture(
            World world,
            double size = 40)
        {
            return _worldHelper.GetWorldPicture(world: world, size: size);
        }

        #endregion

        #region Portal

        private void ShowPortalActionsHolder()
        {
            PortalActionsHolder.Visibility = Visibility.Visible;
        }

        private void HidePortalActionsHolder()
        {
            PortalActionsHolder.Visibility = Visibility.Collapsed;
            _selectedPortal = null;
        }

        private Button GeneratePortalButton(World world)
        {
            var btn = _portalHelper.GeneratePortalButton(world);
            btn.PointerPressed += Portal_PointerPressed;
            return btn;
        }

        private Portal AddPortalOnCanvas(
            UIElement portal,
            double x,
            double y,
            int? z = null)
        {
            return _portalHelper.AddPortalOnCanvas(
                portal: portal,
                canvas: Canvas_Root,
                x: x,
                y: y,
                z: z);
        }

        private Portal CenterAlignNewPortalButton(
            PointerPoint pressedPoint,
            Button portalButton,
            Portal portal)
        {
            return _portalHelper.CenterAlignNewPortalButton(
                pressedPoint: pressedPoint,
                portalButton: portalButton,
                portal: portal,
                canvas: Canvas_Root);
        }

        #endregion

        #region Hub Login

        private bool CanHubLogin()
        {
            var result = Avatar != null && Avatar.User != null && _hubService.IsConnected();

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
                var contentDialogue = new MessageDialogueWindow(title: "Connection failure!", message: "Would you like to try again?", result: async (result) =>
                {
                    if (result)
                        await ConnectWithHubThenLogin();
                });
                contentDialogue.Show();
            }
        }

        private async Task<bool> ConnectWithHub()
        {
            try
            {
                if (App.World.IsEmpty())
                    return false;

                Console.WriteLine("TryConnect: ATTEMP");

                if (_hubService.IsConnected())
                {
                    Console.WriteLine("TryConnect: OK");
                    return true;
                }

                await _hubService.ConnectAsync();

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

                var contentDialogue = new MessageDialogueWindow(title: "Login failure!", message: "Would you like to try again?", result: async (result) =>
                {
                    if (result)
                        await TryLoginToHub();
                });
                contentDialogue.Show();
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
                    var avatar = await _hubService.Login(Avatar);

                    if (avatar != null && !avatar.IsEmpty())
                    {
                        Console.WriteLine("LoginToHub: OK");

                        // Logged in user's avatar
                        Avatar = avatar;

                        // Clearing up canvas prior to login
                        AvatarMessengers.Clear();

                        App.SetIsBusy(true, "Preparing world...");

                        // Get constructs and avatars
                        await FetchConstructs();
                        await FetchAvatars();

                        _isLoggedIn = true;

                        // Set connected user's avatar image
                        SetCurrentUserAvatar();
                        SetCurrentWorld();

                        App.SetIsBusy(false);
                        Visibility = Visibility.Visible;

                        ScrollIntoView(Avatar);

                        PopulateClouds();

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("LoginToHub: FAILED");

                    if (await ConnectWithHub())
                    {
                        return await LoginToHub();
                    }

                    Console.WriteLine("LoginToHub: FAILED");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoginToHub: ERROR " + "\n" + ex.Message);
                return false;
            }
        }

        #endregion

        #region Hub Logout

        /// <summary>
        /// Logout and disconnect current user from Hub.
        /// </summary>
        /// <returns></returns>
        private async Task LogoutFromHubThenDisconnect()
        {
            // If logged in then log out and disconnect from hub
            if (await LogoutFromHub())
            {
                await _hubService.DisconnectAsync();
            }
        }

        /// <summary>
        /// Logout current user from Hub.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> LogoutFromHub()
        {
            try
            {
                if (_isLoggedIn && _hubService.IsConnected())
                {
                    await _hubService.Logout();
                    App.World = new World();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("LogoutFromHub: ERROR " + "\n" + ex.Message);
                return false;
            }
        }

        #endregion

        #region Element

        /// <summary>
        /// Attach an image with pointer movement.
        /// </summary>
        /// <param name="uiElement"></param>
        private void AttachPointerElement(UIElement uiElement)
        {
            if (uiElement is Button button)
            {
                if (button.Tag is Construct construct)
                {
                    if (button.Content is Image image && image.Source is BitmapImage bitmapImage)
                    {
                        var bitmap = new BitmapImage(new Uri(bitmapImage.UriSource.OriginalString, UriKind.RelativeOrAbsolute));

                        var img = new Image()
                        {
                            Source = bitmap,
                            Stretch = image.Stretch,
                        };

                        ScaleElement(img, construct.Scale);
                        RotateElement(img, construct.Rotation);

                        _pointerImage = new Border();

                        _pointerImage.Child = img;
                        _pointerImage.Opacity = 0.8;
                        _pointerImage.Tag = false;
                    }
                }
                else if (button.Tag is Portal portal)
                {
                    var img = GetWorldPicture(portal.World, 140);

                    _pointerImage = img;
                    _pointerImage.Opacity = 0.8;
                    _pointerImage.Tag = false;
                }
            }
        }

        /// <summary>
        /// Move attached image with pointer.
        /// </summary>
        /// <param name="e"></param>
        private void MoveAttachedPointerElement(PointerRoutedEventArgs e)
        {
            if (_pointerImage != null)
            {
                if ((bool)_pointerImage.Tag == false)
                {
                    Canvas_Root.Children.Add(_pointerImage);
                    Canvas.SetZIndex(_pointerImage, 999);
                    _pointerImage.Tag = true;
                }

                var pressedPoint = e.GetCurrentPoint(Canvas_Root);

                var offsetX = _pointerImage.ActualWidth / 2;
                var offsetY = _pointerImage.ActualHeight / 2;/*+ 3*/

                var pointX = NormalizePointerX(pressedPoint);
                var pointY = NormalizePointerY(pressedPoint);

                var goToX = pointX - offsetX;
                var goToY = pointY - offsetY;

                Canvas.SetLeft(_pointerImage, goToX);
                Canvas.SetTop(_pointerImage, goToY);
            }
        }

        /// <summary>
        /// Releases the attached image from pointer movement.
        /// </summary>
        private void ReleaseAttachedPointerElement()
        {
            if (_pointerImage != null)
            {
                Canvas_Root.Children.Remove(_pointerImage);
                _pointerImage = null;
            }
        }

        /// <summary>
        /// Starts dragging an UIElement.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="uielement"></param>
        private void DragStart(
            Canvas canvas,
            PointerRoutedEventArgs e,
            UIElement uielement)
        {
            _elementHelper.DragStart(canvas, e, uielement);
        }

        /// <summary>
        /// Drags an UIElement.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="uielement"></param>
        private void DragElement(
            Canvas canvas,
            PointerRoutedEventArgs e,
            UIElement uielement)
        {
            _elementHelper.DragElement(canvas, e, uielement);
        }

        /// <summary>
        /// Stops dragging an UIElement.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="uielement"></param>
        private void DragRelease(
            PointerRoutedEventArgs e,
            UIElement uielement)
        {
            _elementHelper.DragRelease(e, uielement);
        }

        /// <summary>
        /// Gets the image from the provided UiElement.
        /// </summary>
        /// <param name="uielement"></param>
        /// <returns></returns>
        private Image GetImageFromUiElement(
            UIElement uielement,
            double size = 50,
            Stretch stretch = Stretch.Uniform)
        {
            if (uielement == null)
                return null;

            if (uielement is Button button && button.Content is Image image && image.Source is BitmapImage bitmapImage)
            {
                var bitmap = new BitmapImage(new Uri(bitmapImage.UriSource.OriginalString, UriKind.RelativeOrAbsolute));

                var img = new Image()
                {
                    Source = bitmap,
                    Stretch = stretch,
                    Height = size,
                    Width = size,
                };

                return img;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Moves an UIElement to a new coordinate with the provided PointerRoutedEventArgs in canvas. Returns the tagged object of the uIElement.
        /// </summary>
        /// <param name="uIElement"></param>
        /// <param name="goToX"></param>
        /// <param name="goToY"></param>
        /// <returns></returns>
        private object MoveElement(
            UIElement uIElement,
            PointerRoutedEventArgs e)
        {
            return _elementHelper.MoveElement(
                canvas: Canvas_Root,
                uIElement: uIElement,
                e: e);
        }

        /// <summary>
        /// Moves an UIElement to the provided goToX and goToY coordinate in canvas. Returns the tagged object of the uIElement.
        /// </summary>
        /// <param name="uIElement"></param>
        /// <param name="goToX"></param>
        /// <param name="goToY"></param>
        /// <returns></returns>
        private object MoveElement(
            UIElement uIElement,
            double goToX,
            double goToY,
            int? gotoZ = null)
        {
            return _elementHelper.MoveElement(
                uIElement: uIElement,
                goToX: goToX,
                goToY: goToY,
                gotoZ: gotoZ,
                isCrafting: Button_ConstructCraft.IsChecked.Value);
        }

        /// <summary>
        /// Makes the provided UIElement appear on the pointer coordinate.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="uIElement"></param>
        private void PopElementContextCommands(
            PointerRoutedEventArgs e,
            UIElement uIElement,
            double offsetX = 0,
            double offSetY = 0)
        {
            var pressedPoint = e.GetCurrentPoint(Canvas_RootHost);

            var pointX = pressedPoint.Position.X + 3 + offsetX;
            var pointY = pressedPoint.Position.Y + 3 + offSetY;

            Canvas.SetLeft(uIElement, pointX);
            Canvas.SetTop(uIElement, pointY);
        }

        #endregion

        #region Connection

        /// <summary>
        /// Prompts character selection and establishes communication to hub.
        /// </summary>
        /// <returns></returns>
        public async void SelectCharacterAndConnect()
        {
            try
            {
                if (Character.IsEmpty())
                {
                    Characters = Characters.Any() ? Characters : JsonSerializer.Deserialize<Character[]>(Service.Properties.Resources.CharacterAssets).ToList();

                    var characterPicker = new CharacterSelectionWindow(characters: Characters, characterSelected: async (character) =>
                    {
                        Character = character;
                        SetAvatarData();
                        App.SetLoggedInUserModel();

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

        /// <summary>
        /// Leave this world. If user is connected to hub and logged in them logs the user out and disconnects from the hub.
        /// </summary>
        /// <returns></returns>
        private void LeaveWorld()
        {
            var contentDialogue = new MessageDialogueWindow(title: "Leaving!", message: "Are you sure you want to leave this world?", result: (result) =>
            {
                if (result)
                    App.NavigateToPage(Constants.Page_WorldsPage);
            });

            contentDialogue.Show();
        }

        /// <summary>
        /// Subscribe to hub and start listening to hub events.
        /// </summary>
        private void SubscribeHub()
        {
            #region Hub Connectivity Events

            _hubService.ConnectionReconnecting += HubService_ConnectionReconnecting;
            _hubService.ConnectionReconnected += HubService_ConnectionReconnected;
            _hubService.ConnectionClosed += HubService_ConnectionClosed;

            #endregion

            #region Avatar Events

            _hubService.NewBroadcastAvatarMovement += HubService_NewBroadcastAvatarMovement;
            _hubService.NewBroadcastAvatarActivityStatus += HubService_NewBroadcastAvatarActivityStatus;

            #endregion

            #region Avatar Connectivity Events

            _hubService.AvatarLoggedIn += HubService_AvatarLoggedIn;
            _hubService.AvatarLoggedOut += HubService_AvatarLoggedOut;
            _hubService.AvatarDisconnected += HubService_AvatarDisconnected;
            _hubService.AvatarReconnected += HubService_AvatarReconnected;

            #endregion

            #region Construct Events

            _hubService.NewBroadcastConstruct += HubService_NewBroadcastConstruct;
            //HubService.NewBroadcastConstructs += HubService_NewBroadcastConstructs;

            _hubService.NewRemoveConstruct += HubService_NewRemoveConstruct;
            //HubService.NewRemoveConstructs += HubService_NewRemoveConstructs;

            _hubService.NewBroadcastConstructPlacement += HubService_NewBroadcastConstructPlacement;

            _hubService.NewBroadcastConstructRotation += HubService_NewBroadcastConstructRotation;
            //HubService.NewBroadcastConstructRotations += HubService_NewBroadcastConstructRotations;

            _hubService.NewBroadcastConstructScale += HubService_NewBroadcastConstructScale;
            //HubService.NewBroadcastConstructScales += HubService_NewBroadcastConstructScales;

            _hubService.NewBroadcastConstructMovement += HubService_NewBroadcastConstructMovement;

            #endregion

            #region Avatar Messaging Events

            _hubService.AvatarTyping += HubService_AvatarTyping;
            _hubService.NewMessage += HubService_NewMessage;
            //_hubService.NewImageMessage += HubService_NewImageMessage;

            #endregion

            #region Portal Events

            _hubService.NewBroadcastPortal += HubService_NewBroadcastPortal;

            #endregion

            Console.WriteLine("++SubscribeHub: OK");
        }

        /// <summary>
        /// Unsubscribe from hub and stop listening to hub events.
        /// </summary>
        private void UnsubscribeHub()
        {
            #region Hub Connectivity

            _hubService.ConnectionReconnecting -= HubService_ConnectionReconnecting;
            _hubService.ConnectionReconnected -= HubService_ConnectionReconnected;
            _hubService.ConnectionClosed -= HubService_ConnectionClosed;

            #endregion

            #region Avatar Events

            _hubService.NewBroadcastAvatarMovement -= HubService_NewBroadcastAvatarMovement;
            _hubService.NewBroadcastAvatarActivityStatus -= HubService_NewBroadcastAvatarActivityStatus;

            #endregion

            #region Avatar Connectivity Events

            _hubService.AvatarLoggedIn -= HubService_AvatarLoggedIn;
            _hubService.AvatarLoggedOut -= HubService_AvatarLoggedOut;
            _hubService.AvatarDisconnected -= HubService_AvatarDisconnected;
            _hubService.AvatarReconnected -= HubService_AvatarReconnected;

            #endregion

            #region Construct Events

            _hubService.NewBroadcastConstruct -= HubService_NewBroadcastConstruct;
            //HubService.NewBroadcastConstructs -= HubService_NewBroadcastConstructs;

            _hubService.NewRemoveConstruct -= HubService_NewRemoveConstruct;
            //HubService.NewRemoveConstructs -= HubService_NewRemoveConstructs;

            _hubService.NewBroadcastConstructPlacement -= HubService_NewBroadcastConstructPlacement;

            _hubService.NewBroadcastConstructRotation -= HubService_NewBroadcastConstructRotation;
            //HubService.NewBroadcastConstructRotations -= HubService_NewBroadcastConstructRotations;

            _hubService.NewBroadcastConstructScale -= HubService_NewBroadcastConstructScale;
            //HubService.NewBroadcastConstructScales -= HubService_NewBroadcastConstructScales;

            _hubService.NewBroadcastConstructMovement -= HubService_NewBroadcastConstructMovement;

            #endregion

            #region Avatar Messaging Events

            _hubService.AvatarTyping -= HubService_AvatarTyping;
            _hubService.NewMessage -= HubService_NewMessage;
            //_hubService.NewImageMessage -= HubService_NewImageMessage;

            #endregion

            #region Portal Events

            _hubService.NewBroadcastPortal -= HubService_NewBroadcastPortal;

            #endregion

            Console.WriteLine("++UnsubscribeHub: OK");
        }

        /// <summary>
        /// Set avatar and world information for logged in user for the current inside world session.
        /// </summary>
        private void SetAvatarData()
        {
            Avatar = new Avatar()
            {
                Id = App.User.Id,
                Name = App.User.Name,
                ActivityStatus = ActivityStatus.Online,
                User = new AvatarUser()
                {
                    Email = App.User.Email,
                    ImageUrl = App.User.ImageUrl,
                    Name = App.User.Name
                },
                Character = Character,
                World = new InWorld { Id = App.World.Id, Name = App.World.Name },
                Coordinate = new Coordinate(x: (Window.Current.Bounds.Width / 2) - 50, y: (Window.Current.Bounds.Height / 2) - 100, z: new Random().Next(100, 900)),
                ImageUrl = Character.ImageUrl,
            };
        }

        /// <summary>
        /// Checks if world events can be performed or not. If HubService is connected to server and the user is logged in then returns true.
        /// </summary>
        /// <returns></returns>
        private bool CanPerformWorldEvents()
        {
            var result = _hubService.IsConnected() && _isLoggedIn;
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
        /// Shows actions for other avatar.
        /// </summary>
        private void ShowOtherAvatarActionsHolder()
        {
            OtherAvatarActionsHolder.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Shows own avatar actions.
        /// </summary>
        private void ShowOwnAvatarActionsHolder()
        {
            OwnAvatarActionsHolder.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Shows current user's avatar in UI.
        /// </summary>
        private void SetCurrentUserAvatar()
        {
            if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, Avatar.Id) is UIElement iElement)
            {
                var avatar = _avatarHelper.GetTaggedAvatar(iElement);

                Button_MyAvatar.Tag = avatar;

                var brAvatarCharPic = GetAvatarCharacterPicture(avatar);
                brAvatarCharPic.Margin = new Thickness(5, 0, 5, 0);
                var tbAvatarName = new TextBlock() { Text = avatar.Character.Name.Replace("_", " "), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) };
                var spContent = new StackPanel() { Orientation = Orientation.Horizontal };

                spContent.Children.Add(brAvatarCharPic);
                spContent.Children.Add(tbAvatarName);

                Button_MyAvatar.Content = spContent;
                CurrentAvatarHolder.Visibility = Visibility.Visible;
                Console.WriteLine($"ShowCurrentUserAvatar:OK");
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
            Button_SelectStatus.IsChecked = false;
        }

        /// <summary>
        /// Hides command buttons for self and non self avatars.
        /// </summary>
        private void HideAvatarOperationButtons()
        {
            HideOwnAvatarActions();
            HideOtherAvatarActions();
        }

        /// <summary>
        /// Hides command buttons for self avatar.
        /// </summary>
        private void HideOwnAvatarActions()
        {
            OwnAvatarActionsHolder.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Scroll the provided avatar into view.
        /// </summary>
        /// <param name="avatar"></param>
        private void ScrollIntoView(Avatar avatar)
        {
            if (_avatarHelper.GetAvatarButtonFromCanvas(canvas: Canvas_Root, avatarId: avatar.Id) is UIElement iElement)
            {
                CanvasScrollViewer.ScrollIntoView(
                    element: (Button)iElement,
                    horizontalMargin: Window.Current.Bounds.Width / 2,
                    verticalMargin: (Window.Current.Bounds.Height / 2) - 50,
                    duration: new Duration(TimeSpan.FromSeconds(0)));

                Console.WriteLine("ScrollIntoView:OK");
            }
        }

        /// <summary>
        /// Populate masonry panle to show active avatars in current world.
        /// </summary>
        private void PopulateAvatarsInAvatarsContainer()
        {
            if (AvatarMessengers.Count > 0)
            {
                var _masonryPanel = new MasonryPanelWithProgressiveLoading()
                {
                    Margin = new Thickness(5)
                };

                foreach (Avatar avatar in AvatarMessengers.Select(x => x.Avatar))
                {
                    var userImage = GetAvatarUserPicture(avatar);
                    userImage.Margin = new Thickness(5, 0, 5, 0);

                    var activeAvatarButton = new Button()
                    {
                        Tag = avatar,
                        Style = Application.Current.TryFindResource("MaterialDesign_Button_Style_NoDropShadow") as Style,
                        Margin = new Thickness(5),
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                    };

                    var content = new StackPanel() { Orientation = Orientation.Horizontal };
                    content.Children.Add(userImage);
                    content.Children.Add(new TextBlock()
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Text = avatar.Name,
                        FontWeight = FontWeights.SemiBold,
                        FontSize = 16,
                        Margin = new Thickness(5, 0, 5, 0),
                        TextWrapping = TextWrapping.Wrap,
                    });

                    activeAvatarButton.Content = content;
                    activeAvatarButton.Click += ActiveAvatarButton_Click;

                    _masonryPanel.Children.Add(activeAvatarButton);
                }

                ScrollViewer_ActiveAvatarsContainer.Content = _masonryPanel;
            }
        }

        /// <summary>
        /// Get avatars for the current world.
        /// </summary>
        /// <param name="avatars"></param>
        private async Task FetchAvatars()
        {
            var count = await GetAvatarsCount();

            // If any avatars exist for this world start fetching asynchronously
            if (count > 0)
            {
                var pageSize = 15;

                var totalPageCount = _paginationHelper.GetTotalPageCount(pageSize: pageSize, dataCount: count);

                var fetchedAvatars = new List<Avatar>();

                for (int pageIndex = 0; pageIndex < totalPageCount; pageIndex++)
                {
                    var avatars = await GetAvatars(pageSize, pageIndex);
                    if (avatars != null && avatars.Any())
                    {
                        fetchedAvatars.AddRange(avatars);
                    }
                }

                if (fetchedAvatars.Any())
                {
                    Console.WriteLine("FetchAvatars: avatars found: " + fetchedAvatars.Count());

                    Parallel.ForEach(fetchedAvatars, avatar =>
                    {
                        var avatarButton = GenerateAvatarButton(avatar: avatar);

                        SetAvatarActivityStatus(
                            avatarButton: avatarButton,
                            avatar: avatar,
                            activityStatus: avatar.ActivityStatus);

                        AddAvatarOnCanvas(
                            avatar: avatarButton,
                            x: avatar.Coordinate.X,
                            y: avatar.Coordinate.Y);

                        AvatarMessengers.Add(new AvatarMessenger { Avatar = avatar, IsLoggedIn = true });
                    });

                    Console.WriteLine("FetchAvatars: completed loading avatars in canvas.");

                    AvatarsCount.Text = AvatarMessengers.Count().ToString();
                }
            }

            PopulateAvatarsInAvatarsContainer();
        }

        /// <summary>
        /// Get avatar count from server for the current world.
        /// </summary>
        /// <returns></returns>
        private async Task<long> GetAvatarsCount()
        {
            // Get Avatars count for this world
            var response = await _avatarRepository.GetAvatarsCount(
                token: App.Token,
                worldId: App.World.Id);

            if (!response.Success)
            {
                var contentDialogue = new MessageDialogueWindow(title: "Error!", message: response.Error);
                contentDialogue.Show();

                App.SetIsBusy(false);
                return 0;
            }

            return response.Result;
        }

        /// <summary>
        /// Get avatars from server by pagination for the current world.
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Avatar>> GetAvatars(
            int pageSize,
            int pageIndex)
        {
            // Get Avatars in small packets
            var response = await _avatarRepository.GetAvatars(
                token: App.Token,
                worldId: App.World.Id,
                pageIndex: pageIndex,
                pageSize: pageSize);

            if (!response.Success)
            {
                var contentDialogue = new MessageDialogueWindow(title: "Error!", message: response.Error);
                contentDialogue.Show();

                App.SetIsBusy(false);
                return Enumerable.Empty<Avatar>();
            }

            var avatars = response.Result as IEnumerable<Avatar>;

            return avatars;
        }

        /// <summary>
        /// Gets the user image as a circular border from the provided avatar.
        /// </summary>
        /// <param name="avatar"></param>
        /// <returns></returns>
        private Border GetAvatarUserPicture(
            Avatar avatar,
            double size = 40)
        {
            return _avatarHelper.GetAvatarUserPictureFrame(
                avatar: avatar,
                size: size);
        }

        /// <summary>
        /// Gets the character image as a circular border from the provided avatar.
        /// </summary>
        /// <param name="avatar"></param>
        /// <returns></returns>
        private Border GetAvatarCharacterPicture(
            Avatar avatar,
            double size = 40)
        {
            return _avatarHelper.GetAvatarCharacterPictureFrame(
                avatar: avatar,
                size: size,
                background: Colors.BlanchedAlmond);
        }

        /// <summary>
        /// Aligns facing direction of logged in user's avatar wrt provided x.
        /// </summary>
        /// <param name="gotoX"></param>
        private void AlignAvatarFaceDirectionWrtX(double gotoX)
        {
            _avatarHelper.AlignAvatarCharacterDirectionWrtX(
                goToX: gotoX,
                canvas: Canvas_Root,
                avatarId: Avatar.Id);
        }

        /// <summary>
        /// Adds an avatar on canvas.
        /// </summary>
        /// <param name="avatar"></param>
        private Avatar AddAvatarOnCanvas(
            UIElement avatar,
            double x,
            double y)
        {
            return _avatarHelper.AddAvatarOnCanvas(
                avatar: avatar,
                canvas: Canvas_Root,
                x: x,
                y: y);
        }

        /// <summary>
        /// Broadcast avatar movement.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task BroadcastAvatarMovement(PointerRoutedEventArgs e)
        {
            if (_avatarHelper.GetAvatarButtonFromCanvas(canvas: Canvas_Root, avatarId: Avatar.Id) is UIElement iElement)
            {
                var taggedObject = MoveElement(iElement, e);
                var movedAvatar = taggedObject as Avatar;

                var z = Canvas.GetZIndex(iElement);

                await _hubService.BroadcastAvatarMovement(
                    avatarId: Avatar.Id,
                    x: movedAvatar.Coordinate.X,
                    y: movedAvatar.Coordinate.Y,
                    z: z);

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
            if (_avatarHelper.GetAvatarButtonFromCanvas(canvas: Canvas_Root, avatarId: Avatar.Id) is UIElement iElement)
            {
                var avatarButton = (Button)iElement;
                var taggedAvatar = avatarButton.Tag as Avatar;

                SetAvatarActivityStatus(
                    avatarButton: avatarButton,
                    avatar: taggedAvatar,
                    activityStatus: activityStatus);

                await _hubService.BroadcastAvatarActivityStatus(
                    avatarId: taggedAvatar.Id,
                    activityStatus: (int)activityStatus);

                Console.WriteLine("Avatar status updated.");
            }
        }

        /// <summary>
        /// Sets the provided activityStatus to the avatar. Updates image with StatusBoundImageUrl.
        /// </summary>
        /// <param name="avatarButton"></param>
        /// <param name="avatar"></param>
        /// <param name="activityStatus"></param>
        public void SetAvatarActivityStatus(
            Button avatarButton,
            Avatar avatar,
            ActivityStatus activityStatus)
        {
            _avatarHelper.SetAvatarActivityStatus(
                avatarButton: avatarButton,
                avatar: avatar,
                activityStatus: activityStatus);
        }

        /// <summary>
        /// Generates a new button from the provided avatar.
        /// </summary>
        /// <param name="avatar"></param>
        /// <returns></returns>
        private Button GenerateAvatarButton(Avatar avatar)
        {
            var obj = _avatarHelper.GenerateAvatarButton(avatar);
            obj.PointerPressed += Avatar_PointerPressed;
            return obj;
        }

        #endregion

        #region Construct

        /// <summary>
        /// Get constructs for the current world.
        /// </summary>
        /// <returns></returns>
        private async Task FetchConstructs()
        {
            var count = await GetConstructsCount();

            // If any constructs exist for this world start fetching asynchronously
            if (count > 0)
            {
                var pageSize = 20;

                var totalPageCount = _paginationHelper.GetTotalPageCount(pageSize: pageSize, dataCount: count);

                var fetchedConstructs = new List<Construct>();

                for (int pageIndex = 0; pageIndex < totalPageCount; pageIndex++)
                {
                    var constructs = await GetConstructs(pageSize, pageIndex);

                    if (constructs != null && constructs.Any())
                    {
                        fetchedConstructs.AddRange(constructs);
                    }
                }

                if (fetchedConstructs.Any())
                {
                    var children = Canvas_Root.Children.OfType<Button>().Where(x => x.Tag is Construct);

                    Console.WriteLine($"FetchConstructs: Found {fetchedConstructs.Count} constructs.");

                    Parallel.ForEach(fetchedConstructs, construct =>
                    {
                        // If a construct already exists then update that, this can happen as after avatar login, new constructs can start appearing thru HubService
                        if (children != null && children.Count() > 0 && children.Any(x => ((Construct)x.Tag).Id == construct.Id))
                        {
                            if (children.FirstOrDefault(x => ((Construct)x.Tag).Id == construct.Id) is Button btnConstruct)
                            {
                                btnConstruct.Tag = construct;

                                Canvas.SetLeft(btnConstruct, construct.Coordinate.X);
                                Canvas.SetTop(btnConstruct, construct.Coordinate.Y);
                                Canvas.SetZIndex(btnConstruct, construct.Coordinate.Z);

                                ScaleElement(btnConstruct, construct.Scale);
                                RotateElement(btnConstruct, construct.Rotation);
                            }
                        }
                        else // If construct doesn't exist then add that
                        {
                            var btnConstruct = GenerateConstructButton(
                               name: construct.Name,
                               imageUrl: construct.ImageUrl,
                               constructId: construct.Id,
                               inWorld: construct.World,
                               creator: construct.Creator,
                               createdOn: construct.CreatedOn);

                            AddConstructOnCanvas(
                                construct: btnConstruct,
                                x: construct.Coordinate.X,
                                y: construct.Coordinate.Y,
                                z: construct.Coordinate.Z,
                                disableOpacityAnimation: true);

                            ScaleElement(btnConstruct, construct.Scale);
                            RotateElement(btnConstruct, construct.Rotation);
                        }
                    });
                }

                Console.WriteLine("FetchConstructs: Completed loading constructs in canvas.");
            }
        }

        /// <summary>
        ///  Get constructs count from server for the current world.
        /// </summary>
        /// <returns></returns>
        private async Task<long> GetConstructsCount()
        {
            var response = await _constructRepository.GetConstructsCount(
                token: App.Token,
                worldId: App.World.Id);

            if (!response.Success)
            {
                var contentDialogue = new MessageDialogueWindow(title: "Error!", message: response.Error);
                contentDialogue.Show();

                App.SetIsBusy(false);
                return 0;
            }

            return response.Result;
        }

        /// <summary>
        /// Get constructs from server by pagination for the current world.
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Construct>> GetConstructs(
            int pageSize,
            int pageIndex)
        {
            // Get constructs in small packets
            var response = await _constructRepository.GetConstructs(
                token: App.Token,
                pageIndex: pageIndex,
                pageSize: pageSize,
                worldId: App.World.Id);

            if (!response.Success)
            {
                var contentDialogue = new MessageDialogueWindow(title: "Error!", message: response.Error);
                contentDialogue.Show();

                App.SetIsBusy(false);
                return Enumerable.Empty<Construct>();
            }

            var constructs = response.Result as IEnumerable<Construct>;

            return constructs;
        }

        /// <summary>
        /// Shows the ConstructAssetSelectionControl.
        /// </summary>
        private void ShowConstructAssetsControl()
        {
            ConstructAssetSelectionControl.Visibility = Visibility.Visible;

            if (!ConstructAssetSelectionControl.FirstHit)
            {
                ConstructAssetSelectionControl.ShowConstructCategories();
                ConstructAssetSelectionControl.FirstHit = true;
            }
        }

        /// <summary>
        /// Hides the ConstructAssetSelectionControl.
        /// </summary>
        private void HideConstructAssetsControl()
        {
            ConstructAssetSelectionControl.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Subscription method for AssetSelected event of ConstructAssetSelectionControl.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="constructAsset"></param>
        private void ConstructAssetSelectionControl_AssetSelected(
            object sender,
            ConstructAsset constructAsset)
        {
            ReleaseAttachedPointerElement();

            var btnConstruct = GenerateConstructButton(
                name: constructAsset.Name,
                imageUrl: constructAsset.ImageUrl);

            _addingConstruct = btnConstruct;
            ShowOperationalConstruct(_addingConstruct, "Adding...");

            AttachPointerElement(btnConstruct);
        }

        /// <summary>
        /// Aligns a construct button to the center point of the pressed pointer location.
        /// </summary>
        /// <param name="pressedPoint"></param>
        /// <param name="constructButton"></param>
        /// <param name="construct"></param>
        /// <returns></returns>
        private Construct CenterAlignNewConstructButton(
            PointerPoint pressedPoint,
            Button constructButton,
            Construct construct)
        {
            return _constructHelper.CenterAlignNewConstructButton(
                pressedPoint: pressedPoint,
                constructButton: constructButton,
                construct: construct,
                canvas: Canvas_Root);
        }

        /// <summary>
        /// Checks if the selected construct can be manipulated by the current user. A user can only manipulate construct he added.
        /// </summary>
        /// <returns></returns>
        private bool CanManipulateConstruct()
        {
            return _constructHelper.CanManipulateConstruct(_selectedConstruct);
        }

        /// <summary>
        /// Removes the provided construct from canvas.
        /// </summary>
        /// <param name="construct"></param>
        private void RemoveConstructFromCanvas(UIElement construct)
        {
            _constructHelper.RemoveConstructFromCanvas(construct, Canvas_Root);
        }

        /// <summary>
        /// Clears multi selected constructs.
        /// </summary>
        private void ClearMultiselectedConstructs()
        {
            Button_ConstructMultiSelect.IsChecked = false;
            MultiSelectedConstructsHolder.Children.Clear();
            MultiselectedConstructs.Clear();

            Button_ConstructClone.IsChecked = false;
            Button_ConstructMove.IsChecked = false;
        }

        /// <summary>
        /// Broadcasts construct rotation operation.
        /// </summary>
        /// <param name="selectedConstruct"></param>
        /// <returns></returns>
        private async Task BroadcastConstructRotate(
            UIElement selectedConstruct,
            bool isBacward = false)
        {
            var button = (Button)selectedConstruct;

            var construct = button.Tag as Construct;

            var newRotation = isBacward ? construct.Rotation - 5 : construct.Rotation + 5;

            construct = RotateElement(uIElement: selectedConstruct, rotation: newRotation) as Construct;

            // Align avatar to construct point
            AlignAvatarFaceDirectionWrtX(gotoX: construct.Coordinate.X);

            await _hubService.BroadcastConstructRotation(constructId: construct.Id, rotation: construct.Rotation);

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

            construct = ScaleElement(uIElement: _selectedConstruct, scale: newScale) as Construct;

            // Align avatar to construct point
            AlignAvatarFaceDirectionWrtX(gotoX: construct.Coordinate.X);

            await _hubService.BroadcastConstructScale(constructId: construct.Id, scale: construct.Scale);

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

            construct = ScaleElement(uIElement: _selectedConstruct, scale: newScale) as Construct;

            // Align avatar to construct point
            AlignAvatarFaceDirectionWrtX(gotoX: construct.Coordinate.X);

            await _hubService.BroadcastConstructScale(constructId: construct.Id, scale: construct.Scale);

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
            AlignAvatarFaceDirectionWrtX(gotoX: construct.Coordinate.X);

            await _hubService.BroadcastConstructPlacement(construct.Id, zIndex);
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
            AlignAvatarFaceDirectionWrtX(gotoX: construct.Coordinate.X);

            await _hubService.BroadcastConstructPlacement(construct.Id, zIndex);
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
            AlignAvatarFaceDirectionWrtX(gotoX: construct.Coordinate.X);

            RemoveConstructFromCanvas(_selectedConstruct);

            ShowSelectedConstruct(null); // Construct delete

            await _hubService.RemoveConstruct(construct.Id);
        }

        /// <summary>
        /// Adds a construct to the canvas.
        /// </summary>
        /// <param name="construct"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private Construct AddConstructOnCanvas(
            UIElement construct,
            double x,
            double y,
            int? z = null,
            bool disableOpacityAnimation = false)
        {
            return _constructHelper.AddConstructOnCanvas(
                construct: construct,
                canvas: Canvas_Root,
                x: x,
                y: y,
                z: z,
                disableOpacityAnimation: disableOpacityAnimation);
        }

        /// <summary>
        /// Generate a new button from the provided construct. If constructId is not provided then new id is generated. If inWorld, creator are not provided then current world and user are tagged.
        /// </summary>
        /// <param name="construct"></param>
        /// <param name="constructId"></param>
        /// <returns></returns>
        private Button GenerateConstructButton(
            string name,
            string imageUrl,
            int? constructId = null,
            InWorld inWorld = null,
            Creator creator = null,
            DateTime? createdOn = null)
        {
            var btn = _constructHelper.GenerateConstructButton(
                name: name,
                imageUrl: imageUrl,
                constructId: constructId,
                inWorld: inWorld,
                creator: creator,
                createdOn: createdOn);

            btn.PointerPressed += Construct_PointerPressed;
            btn.PointerMoved += Construct_PointerMoved;
            btn.PointerReleased += Construct_PointerReleased;

            return btn;
        }

        /// <summary>
        /// Shows construct operational buttons on the UI.
        /// </summary>
        private void ShowConstructOperationButtons(PointerRoutedEventArgs e)
        {
            if (_selectedConstruct != null && _selectedConstruct is Button button && button.Tag is Construct)
            {
                PopElementContextCommands(e: e, uIElement: ConstructOperationalCommandsHolder, offsetX: button.ActualWidth / 2, offSetY: button.ActualHeight / 4);
                ConstructOperationalCommandsHolder.Visibility = Visibility.Visible;
            }
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
        private object ScaleElement(
            UIElement uIElement,
            float scale)
        {
            return _elementHelper.ScaleElement(uIElement, scale);
        }

        /// <summary>
        /// Rotates an UIElement to the provided rotation. Returns the tagged object of the uIElement.
        /// </summary>
        /// <param name="uIElement"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        private object RotateElement(
            UIElement uIElement,
            float rotation)
        {
            return _elementHelper.RotateElement(uIElement, rotation);
        }

        /// <summary>
        /// Moves multi-selected constructs to the pointer position.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task MoveConstructs(PointerRoutedEventArgs e)
        {
            var pressedPoint = e.GetCurrentPoint(Canvas_Root);

            var pointX = NormalizePointerX(pressedPoint);
            var pointY = NormalizePointerY(pressedPoint);

            // Align avatar to clicked point
            AlignAvatarFaceDirectionWrtX(pointX);

            UIElement fe = Canvas_Root.Children.OfType<Button>().Where(z => z.Tag is Construct c && c.Id == MultiselectedConstructs.FirstOrDefault().Id).FirstOrDefault();

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

                _movingConstruct = Canvas_Root.Children.OfType<Button>().Where(z => z.Tag is Construct).FirstOrDefault(x => ((Construct)x.Tag).Id == element.Id);

                double goToX = pointX - ((Button)_movingConstruct).ActualWidth / 2;
                double goToY = pointY - ((Button)_movingConstruct).ActualHeight / 2;

                goToX += distWrtFi.FirstOrDefault(x => x.Item1 == element.Id).Item2;
                goToY += distWrtFi.FirstOrDefault(x => x.Item1 == element.Id).Item3;

                var taggedObject = MoveElement(
                    uIElement: _movingConstruct,
                    goToX: goToX,
                    goToY: goToY);

                var construct = taggedObject as Construct;

                await _hubService.BroadcastConstructMovement(
                    constructId: construct.Id,
                    x: construct.Coordinate.X,
                    y: construct.Coordinate.Y,
                    z: construct.Coordinate.Z);

                Console.WriteLine("Construct moved.");
            }
        }

        /// <summary>
        /// Moves selected construct to the pointer position.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task MoveConstruct(PointerRoutedEventArgs e)
        {
            if (_movingConstruct == null)
                return;

            var taggedObject = MoveElement(_movingConstruct, e);

            var construct = taggedObject as Construct;

            // Align avatar to construct point
            AlignAvatarFaceDirectionWrtX(gotoX: construct.Coordinate.X);

            await _hubService.BroadcastConstructMovement(
                construct.Id,
                construct.Coordinate.X,
                construct.Coordinate.Y,
                construct.Coordinate.Z);

            Console.WriteLine("Construct moved.");
        }

        /// <summary>
        /// Clones multi-selected constructs to the pointer position.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task CloneConstructs(PointerRoutedEventArgs e)
        {
            var pressedPoint = e.GetCurrentPoint(Canvas_Root);

            var pointX = NormalizePointerX(pressedPoint);
            var pointY = NormalizePointerY(pressedPoint);

            // Align avatar to clicked point
            AlignAvatarFaceDirectionWrtX(pointX);

            UIElement fe = Canvas_Root.Children.OfType<Button>().Where(z => z.Tag is Construct c && c.Id == MultiselectedConstructs.FirstOrDefault().Id).FirstOrDefault();

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

                _cloningConstruct = Canvas_Root.Children.OfType<Button>().Where(z => z.Tag is Construct).FirstOrDefault(x => ((Construct)x.Tag).Id == element.Id);

                double goToX = pointX - ((Button)_cloningConstruct).ActualWidth / 2;
                double goToY = pointY - ((Button)_cloningConstruct).ActualHeight / 2;

                goToX += distWrtFi.FirstOrDefault(x => x.Item1 == element.Id).Item2;
                goToY += distWrtFi.FirstOrDefault(x => x.Item1 == element.Id).Item3;

                var constructSource = ((Button)_cloningConstruct).Tag as Construct;

                var construct = CloneConstruct(
                   cloningConstruct: constructSource,
                   pressedPoint: pressedPoint,
                   pointX: goToX,
                   pointY: goToY,
                   disableCenterAlignToPointerPoint: true);

                await _hubService.BroadcastConstruct(construct);
                Console.WriteLine("Construct cloned.");
            }
        }

        /// <summary>
        /// Clones selected construct to the pointer position.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task CloneConstruct(PointerRoutedEventArgs e)
        {
            if (_cloningConstruct == null)
                return;

            var pressedPoint = e.GetCurrentPoint(Canvas_Root);

            var pointX = NormalizePointerX(pressedPoint);
            var pointY = NormalizePointerY(pressedPoint);

            var constructSource = ((Button)_cloningConstruct).Tag as Construct;

            var construct = CloneConstruct(
                cloningConstruct: constructSource,
                pressedPoint: pressedPoint,
                pointX: pointX,
                pointY: pointY);

            // Align avatar to construct point
            AlignAvatarFaceDirectionWrtX(gotoX: construct.Coordinate.X);

            await _hubService.BroadcastConstruct(construct);
            Console.WriteLine("Construct cloned.");
        }

        /// <summary>
        /// Clones the provided construct to pointX and pointY.
        /// </summary>
        /// <param name="cloningConstruct"></param>
        /// <param name="pressedPoint"></param>
        /// <param name="pointX"></param>
        /// <param name="pointY"></param>
        /// <returns></returns>
        private Construct CloneConstruct(
            Construct cloningConstruct,
            PointerPoint pressedPoint,
            double pointX,
            double pointY,
            bool disableCenterAlignToPointerPoint = false)
        {
            var constructButton = GenerateConstructButton(
                name: cloningConstruct.Name,
                imageUrl: cloningConstruct.ImageUrl);

            // Add the construct on pressed point
            var construct = AddConstructOnCanvas(
                construct: constructButton,
                x: pointX,
                y: pointY);

            // Clone the scaling and rotation factors
            construct = ScaleElement(constructButton, cloningConstruct.Scale) as Construct;
            construct = RotateElement(constructButton, cloningConstruct.Rotation) as Construct;

            // Center the construct on pressed point
            if (!disableCenterAlignToPointerPoint)
                construct = CenterAlignNewConstructButton(
                    pressedPoint: pressedPoint,
                    constructButton: constructButton,
                    construct: construct);

            return construct;
        }

        /// <summary>
        /// Normalizes the X position of the provided PointerPoint w.r.t X ScaleTransform factor of Canvas_Root.
        /// </summary>
        /// <param name="pressedPoint"></param>
        /// <returns></returns>
        private double NormalizePointerX(PointerPoint pressedPoint)
        {
            return _elementHelper.NormalizePointerX(Canvas_Root, pressedPoint);
        }

        /// <summary>
        ///  Normalizes the Y position of the provided PointerPoint w.r.t Y ScaleTransform factor of Canvas_Root.
        /// </summary>
        /// <param name="pressedPoint"></param>
        /// <returns></returns>
        private double NormalizePointerY(PointerPoint pressedPoint)
        {
            return _elementHelper.NormalizePointerY(Canvas_Root, pressedPoint);
        }

        #endregion

        #region Message

        /// <summary>
        /// Hides command buttons for non self avatar.
        /// </summary>
        private void HideOtherAvatarActions()
        {
            OtherAvatarActionsHolder.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Send unicast message to selected avatar.
        /// </summary>
        /// <returns></returns>
        private async Task SendUnicastMessage()
        {
            if (!CanPerformWorldEvents())
                return;

            if (_messageToAvatar == null)
                return;

            //Check if a valid message was typed and the recepient exists in canvas
            if (((Button)_messageToAvatar).Tag is Avatar recipientAvatar && !MessagingTextBox.Text.IsNullOrBlank() && Canvas_Root.Children.OfType<Button>().Any(x => x.Tag is Avatar avatar1 && avatar1.Id == recipientAvatar.Id))
            {
                var chatMessage = new ChatMessage()
                {
                    SenderId = Avatar.Id,
                    RecipientId = recipientAvatar.Id,
                    Message = MessagingTextBox.Text,
                    ReplyToMessageId = _replyToChatMessage != null ? _replyToChatMessage.Id : 0
                };

                await _hubService.SendUnicastMessage(chatMessage);

                // Add message bubble to own avatar
                if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, Avatar.Id) is UIElement selfAvatarUiElement)
                {
                    AddChatBubbleToCanvas(chatMessage: chatMessage, fromAvatar: selfAvatarUiElement, messageType: MessageType.Unicast, replyToChatMessage: _replyToChatMessage); // send unicast message

                    // Add message to own chatter
                    AddChatMessageToOwnChatter(chatMessage);

                    // If activity status is not Messaging then update it
                    if (((Button)selfAvatarUiElement).Tag is Avatar taggedAvatar && taggedAvatar.ActivityStatus != ActivityStatus.Messaging)
                    {
                        await BroadcastAvatarActivityStatus(ActivityStatus.Messaging);
                    }
                }

                MessagingTextBox.Text = string.Empty;
                _replyToChatMessage = null;
            }
        }

        private void AddChatMessageToOwnChatter(ChatMessage chatMessage)
        {
            AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == this.Avatar.Id)?.Chatter.Add(chatMessage);
        }

        /// <summary>
        /// Send broadcast message to all avatars.
        /// </summary>
        /// <returns></returns>
        private async Task SendBroadcastMessage()
        {
            if (!CanPerformWorldEvents())
                return;

            //Check if a valid message was typed
            if (!MessagingTextBox.Text.IsNullOrBlank())
            {
                var chatMessage = new ChatMessage()
                {
                    Message = MessagingTextBox.Text,
                    SenderId = Avatar.Id
                };

                await _hubService.SendBroadcastMessage(chatMessage);

                // Add message bubble to own avatar
                if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, Avatar.Id) is UIElement selfAvatarUiElement)
                {
                    AddChatBubbleToCanvas(chatMessage: chatMessage, fromAvatar: selfAvatarUiElement, messageType: MessageType.Broadcast); // send broadcast message

                    // Add message to own chatter
                    AddChatMessageToOwnChatter(chatMessage);

                    // If activity status is not Greeting then update it
                    if (((Button)selfAvatarUiElement).Tag is Avatar taggedAvatar && taggedAvatar.ActivityStatus != ActivityStatus.Greeting)
                    {
                        await BroadcastAvatarActivityStatus(ActivityStatus.Greeting);
                    }
                }

                MessagingTextBox.Text = string.Empty;
            }
        }

        /// <summary>
        /// Adds a chat bubble to canvas on top of the avatar who sent it.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="fromAvatar"></param>
        /// <param name="messageType"></param>
        private void AddChatBubbleToCanvas(
            ChatMessage chatMessage,
            UIElement fromAvatar,
            MessageType messageType,
            ChatMessage replyToChatMessage = null)
        {
            var avatarButton = fromAvatar as Button;
            var taggedAvatar = avatarButton.Tag as Avatar;

            var brChatBubble = _chatBubbleHelper.AddChatBubbleToCanvas(
                 chatMessage: chatMessage,
                 fromAvatar: fromAvatar,
                 messageType: messageType,
                 toAvatar: _messageToAvatar,
                 canvas: Canvas_Root,
                 loggedInAvatar: Avatar,
                 replyToChatMessage: replyToChatMessage);

            if (taggedAvatar.Id != Avatar.Id)
            {
                brChatBubble.PointerPressed += ChatBubble_PointerPressed;
            }
        }

        #endregion

        #region Details

        /// <summary>
        /// Sets selected construct details on side card.
        /// </summary>
        private void SetConstructDetailsOnSideCard()
        {
            if (_selectedConstruct == null)
                return;

            if (((Button)_selectedConstruct).Tag is Construct construct)
            {
                DetailsImageHolder.Content = GetImageFromUiElement(_selectedConstruct);
                DetailsNameHolder.Text = construct.Name;
                DetailsDateHolder.Text = $"{construct.CreatedOn.ToShortDateString()} - {construct.CreatedOn.ToShortTimeString()} by {construct.Creator.Name.Split(' ').FirstOrDefault()}";
            }
        }

        #endregion

        #endregion

        #endregion
    }
}