﻿using System;
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
using Worldescape.Common;
using Image = Windows.UI.Xaml.Controls.Image;
using Windows.UI.Input;
using System.Windows.Input;

namespace Worldescape
{
    public partial class InsideWorldPage : Page
    {
        #region Fields

        bool _isLoggedIn;

        UIElement _selectedConstruct;
        UIElement _addingConstruct;
        UIElement _movingConstruct;
        UIElement _cloningConstruct;
        UIElement _selectedAvatar;

        UIElement _messageToAvatar;

        readonly IHubService HubService;
        readonly MainPage _mainPage;
        readonly AvatarHelper _avatarHelper;
        readonly ConstructHelper _constructHelper;
        readonly WorldHelper _worldHelper;
        readonly HttpServiceHelper _httpServiceHelper;
        readonly PaginationHelper _paginationHelper;
        readonly ElementHelper _elementHelper;

        readonly ConstructAssetPickerControl ConstructAssetPickerControl;

        #endregion

        #region Ctor
        public InsideWorldPage()
        {
            InitializeComponent();

            HubService = App.ServiceProvider.GetService(typeof(IHubService)) as IHubService;

            _mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;
            _avatarHelper = App.ServiceProvider.GetService(typeof(AvatarHelper)) as AvatarHelper;
            _worldHelper = App.ServiceProvider.GetService(typeof(WorldHelper)) as WorldHelper;
            _constructHelper = App.ServiceProvider.GetService(typeof(ConstructHelper)) as ConstructHelper;
            _httpServiceHelper = App.ServiceProvider.GetService(typeof(HttpServiceHelper)) as HttpServiceHelper;
            _paginationHelper = App.ServiceProvider.GetService(typeof(PaginationHelper)) as PaginationHelper;
            _elementHelper = App.ServiceProvider.GetService(typeof(ElementHelper)) as ElementHelper;

            ConstructAssetPickerControl = App.ServiceProvider.GetService(typeof(ConstructAssetPickerControl)) as ConstructAssetPickerControl;

            // This is be visible after character select and Hub login
            Grid_Root.Visibility = Visibility.Collapsed;

            SubscribeHub();
            SubscribeConstructAssetPicker();
            SelectCharacterAndConnect();
        }

        #endregion

        #region Properties

        List<ConstructAsset> ConstructAssets { get; set; } = new List<ConstructAsset>();

        List<ConstructCategory> ConstructCategories { get; set; } = new List<ConstructCategory>();

        List<Character> Characters { get; set; } = new List<Character>();

        Avatar Avatar { get; set; } = null;

        Character Character { get; set; } = new Character();

        ObservableCollection<AvatarMessenger> AvatarMessengers { get; set; } = new ObservableCollection<AvatarMessenger>();

        List<Construct> MultiselectedConstructs { get; set; } = new List<Construct>();

        #endregion

        #region Methods

        #region Page Events

        /// <summary>
        /// Event fired when this page is unloaded. Logs out the current user from hub, disconnects from hub and unsubscribes from all hub events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (HubService != null)
            {
                await LogoutFromHubThenDisconnect();
                UnsubscribeHub();
                UnsubscribeConstructAssetPicker();
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
        private async void Canvas_Root_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!CanPerformWorldEvents())
                return;

            if (Button_ConstructAdd.IsChecked.Value && _addingConstruct != null)
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
                ShowMessagingAvatar(null);

                HideConstructOperationButtons();
                HideAvatarOperationButtons();
                HideMessagingControls();

                ClearMultiselectedConstructs();
                HideAvatarActivityStatusHolder();

                //if (ToggleButton_PanCanvas.IsChecked.Value)
                //{
                //    Canvas_RootContainer.Cursor = Cursors.SizeAll;

                //    // Canvas_Root drag start
                //    UIElement uielement = (UIElement)sender;
                //    DragStart(Canvas_RootContainer, e, uielement);
                //}
            }
        }

        //private void Canvas_Root_PointerMoved(object sender, PointerRoutedEventArgs e)
        //{
        //    if (ToggleButton_PanCanvas.IsChecked.Value)
        //    {
        //        UIElement uielement = (UIElement)sender;
        //        DragElement(Canvas_RootContainer, e, uielement);
        //    }
        //}

        //private void Canvas_Root_PointerReleased(object sender, PointerRoutedEventArgs e)
        //{
        //    if (ToggleButton_PanCanvas.IsChecked.Value)
        //    {
        //        Canvas_RootContainer.Cursor = Cursors.Arrow;
        //        // Drag stop
        //        UIElement uielement = (UIElement)sender;
        //        DragRelease(e, uielement);
        //    }
        //}

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
                    Button_SelectStatus.IsChecked = false;
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
        private async void Construct_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!CanPerformWorldEvents())
                return;

            UIElement uielement = (UIElement)sender;
            _selectedConstruct = uielement;

            ShowSelectedConstruct(uielement); // Construct

            if (Button_ConstructAdd.IsChecked.Value && _addingConstruct != null)
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
            }
            else if (Button_ConstructCraft.IsChecked.Value)
            {
                if (!CanManipulateConstruct())
                {
                    HideConstructOperationButtons();
                    return;
                }

                ShowConstructOperationButtons();

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
        private void Construct_PointerMoved(object sender, PointerRoutedEventArgs e)
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
        private async void Construct_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (Button_ConstructMove.IsChecked.Value)
            {
                return;
            }

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

                await HubService.BroadcastConstructMovement(
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

                // Clone the scaling and rotation factors
                construct = ScaleElement(constructButton, constructAsset.Scale) as Construct;
                construct = RotateElement(constructButton, constructAsset.Rotation) as Construct;

                // Center the construct on pressed point
                construct = CenterAlignNewConstructButton(
                    pressedPoint: pressedPoint,
                    constructButton: constructButton,
                    construct: construct);

                // Align avatar to construct point
                AlignUsersAvatarFaceDirection(x: construct.Coordinate.X);

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
                AlignUsersAvatarFaceDirection(x: construct.Coordinate.X);

                await HubService.BroadcastConstruct(construct);

                Console.WriteLine("Construct added.");

                // Turn off add mode
                _addingConstruct = null;
            }
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
                _messageToAvatar = _avatarHelper.GetAvatarButtonFromCanvas(canvas: Canvas_Root, avatarId: avatar.Id);
                _selectedAvatar = _messageToAvatar;

                ShowMessagingAvatar(_messageToAvatar);
                ShowMessagingControls();

                ShowSelectedAvatar(_selectedAvatar);
                SetAvatarDetailsOnSideCard();
                OtherAvatarActionsHolder.Visibility = Visibility.Visible;

                MessagingTextBox.Focus();
            }
        }

        #endregion

        #endregion

        #region Button Events

        #region World

        private void ToggleButton_ShowAvatars_Click(object sender, RoutedEventArgs e)
        {
            if (ToggleButton_ShowAvatars.IsChecked.Value)
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

        private void ToggleButton_ZoomInCanvas_Click(object sender, RoutedEventArgs e)
        {
            Canvas_RootScaleTransform.ScaleX += 0.25;
            Canvas_RootScaleTransform.ScaleY += 0.25;
        }

        private void ToggleButton_ZoomOutCanvas_Click(object sender, RoutedEventArgs e)
        {
            Canvas_RootScaleTransform.ScaleX -= 0.25;
            Canvas_RootScaleTransform.ScaleY -= 0.25;
        }

        #endregion

        #region Connection

        /// <summary>
        /// Initiates connection with Hub and login of the user into the Hub.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Connect_Click(object sender, RoutedEventArgs e)
        {
            SelectCharacterAndConnect();
        }

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

                    await BroadcastAvatarActivityStatus(ActivityStatus.Idle);
                }
            }
        }

        /// <summary>
        /// Activates adding a construct. Shows the asset picker for picking a construct and upon selection keeps adding the construct until untoggled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ConstructAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                // Turn off add mode if previously triggered
                if (_addingConstruct != null)
                {
                    _addingConstruct = null;
                    //Button_ConstructAdd.IsChecked = false;
                    //return;
                }

                if (Button_ConstructAdd.IsChecked.Value)
                {
                    ShowConstructAssetsControl();
                }
                else
                {
                    HideConstructAssetsControl();
                }

                //ConstructAssetPickerControl.ShowConstructCategories();

                //if (!ConstructAssets.Any())
                //{
                //    ConstructAssets = JsonSerializer.Deserialize<ConstructAsset[]>(Service.Properties.Resources.ConstructAssets).ToList();
                //    ConstructCategories = ConstructAssets.Select(x => x.Category).Distinct().Select(z => new ConstructCategory() { ImageUrl = @$"ms-appx:///Images/World_Objects/{z}.png", Name = z }).ToList();
                //}

                //var constructAssetPicker = new ConstructAssetPickerWindow(
                //    constructAssets: ConstructAssets,
                //    constructCategories: ConstructCategories,
                //    assetSelected: (constructAsset) =>
                //    {
                //        var constructBtn = GenerateConstructButton(
                //            name: constructAsset.Name,
                //            imageUrl: constructAsset.ImageUrl);

                //        _addingConstruct = constructBtn;
                //        ShowOperationalConstruct(_addingConstruct);
                //    });

                // If the picker was closed without a selection of an asset, set the Button_ConstructAdd to default
                //constructAssetPicker.Closed += (s, e) =>
                //{
                //    if (_addingConstruct == null)
                //    {
                //        Button_ConstructAdd.IsChecked = false;
                //    }
                //};

                //constructAssetPicker.Show();
            }
        }

        /// <summary>
        /// Activates multi selection of clicked constructs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ConstructMultiSelect_Click(object sender, RoutedEventArgs e)
        {
            //Button_ConstructMultiSelect.Content = Button_ConstructMultiSelect.IsChecked.Value ? "Selecting" : "Select";

            if (Button_ConstructMultiSelect.IsChecked.Value)
            {
                ShowConstructOperationButtons();
            }
            else
            {
                HideConstructOperationButtons();
            }
        }

        /// <summary>
        /// Toggles moving mode for the selected construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ConstructMove_Click(object sender, RoutedEventArgs e)
        {
            //Button_ConstructMove.Content = Button_ConstructMove.IsChecked.Value ? "Moving" : "Move";

            if (!Button_ConstructMove.IsChecked.Value)
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
        private void Button_ConstructClone_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                // Button_ConstructClone.Content = Button_ConstructClone.IsChecked.Value ? "Cloning" : "Clone";

                if (!Button_ConstructClone.IsChecked.Value)
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
        /// Rotates the selected construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_ConstructRotate_Click(object sender, RoutedEventArgs e)
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
        /// Event fired upon key press inside the chat box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MessagingTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                await SendUnicastMessage();
            }
        }

        /// <summary>
        /// Activates Messaging controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MessageAvatar_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPerformWorldEvents())
                return;

            if (_selectedAvatar == null)
                return;

            // show messenge from and to avatars and show Messaging controls
            if (((Button)_selectedAvatar).Tag is Avatar avatar)
            {
                _messageToAvatar = _avatarHelper.GetAvatarButtonFromCanvas(canvas: Canvas_Root, avatarId: avatar.Id);
                ShowMessagingAvatar(_messageToAvatar);
                ShowMessagingControls();
                SetAvatarDetailsOnSideCard();

                MessagingTextBox.Focus();
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

                var image = GetImageFromUiElement(uielement, 70);

                SelectedConstructHolder.Content = image;
                SelectedConstructHolder.Visibility = Visibility.Visible;

                SetConstructDetailsOnSideCard();
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
                var image = GetImageFromUiElement(uielement);
                OperationalConstructHolder.Content = image;
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
                AlignUsersAvatarFaceDirection(receiver.Coordinate.X);

                MessagingFromAvatarHolder.Content = _avatarHelper.GetAvatarUserPicture(Avatar);// GetImageFromUiElement(_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, Avatar.Id));
                MessagingToAvatarHolder.Content = _avatarHelper.GetAvatarUserPicture(receiver);//GetImageFromUiElement(receiverUiElement);
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
        }

        #endregion

        #region Hub Events

        #region Construct
        private void HubService_NewBroadcastConstructMovement(int constructId, double x, double y, int z)
        {
            if (Canvas_Root.Children.FirstOrDefault(c => c is Button button && button.Tag is Construct taggedConstruct && taggedConstruct.Id == constructId) is UIElement iElement)
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
            if (Canvas_Root.Children.FirstOrDefault(x => x is Button button && button.Tag is Construct taggedConstruct && taggedConstruct.Id == constructId) is UIElement iElement)
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
            if (Canvas_Root.Children.FirstOrDefault(x => x is Button button && button.Tag is Construct taggedConstruct && taggedConstruct.Id == constructId) is UIElement iElement)
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
            if (Canvas_Root.Children.FirstOrDefault(x => x is Button button && button.Tag is Construct taggedConstruct && taggedConstruct.Id == constructId) is UIElement iElement)
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
            if (Canvas_Root.Children.FirstOrDefault(x => x is Button button && button.Tag is Construct taggedConstruct && taggedConstruct.Id == constructId) is UIElement constructUiElement)
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
            var constructBtn = GenerateConstructButton(
                name: construct.Name,
                imageUrl: construct.ImageUrl,
                constructId: construct.Id,
                inWorld: construct.World,
                creator: construct.Creator,
                createdOn: construct.CreatedOn);

            AddConstructOnCanvas(
                construct: constructBtn,
                x: construct.Coordinate.X,
                y: construct.Coordinate.Y,
                z: construct.Coordinate.Z);

            ScaleElement(uIElement: constructBtn, scale: construct.Scale);
            RotateElement(uIElement: constructBtn, rotation: construct.Rotation);

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
                        avatarMessenger.ActivityStatus = ActivityStatus.Idle;
                        avatarMessenger.IsLoggedIn = true;
                    }

                    SetAvatarActivityStatus(
                        avatarButton: (Button)iElement,
                        avatar: (Avatar)(((Button)iElement).Tag),
                        activityStatus: ActivityStatus.Idle);

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
                        avatarMessenger.ActivityStatus = ActivityStatus.Away;
                        avatarMessenger.IsLoggedIn = false;
                    }

                    SetAvatarActivityStatus(
                        avatarButton: (Button)iElement,
                        avatar: (Avatar)(((Button)iElement).Tag),
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
            if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, avatar.Id) is UIElement iElement)
            {
                Canvas_Root.Children.Remove(iElement);
                AvatarMessengers.Remove(AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == avatar.Id));
            }

            var avatarButton = GenerateAvatarButton(avatar);
            AddAvatarOnCanvas(avatarButton, avatar.Coordinate.X, avatar.Coordinate.Y, avatar.Coordinate.Z);

            AvatarMessengers.Add(new AvatarMessenger() { Avatar = avatar, ActivityStatus = avatar.ActivityStatus, IsLoggedIn = true });
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
                    var avatarMessenger = AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == avatarId);

                    if (avatarMessenger != null)
                        avatarMessenger.ActivityStatus = (ActivityStatus)activityStatus;

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
                if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, avatarId) is UIElement iElement)
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
                                AddChatBubbleToCanvas(msg, senderAvatarUiElement); // receive message
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
            _ = await HubService.Login(Avatar);
            _isLoggedIn = true;
            Console.WriteLine("<<HubService_ConnectionReconnected");
        }

        private void HubService_ConnectionReconnecting()
        {
            _isLoggedIn = false;
            Console.WriteLine("<<HubService_ConnectionReconnecting");
        }

        #endregion

        #endregion

        #region Functionality

        #region World

        /// <summary>
        /// Adds random clouds to the canvas. If drawOver= true then adds the clouds on top of existing canvas elements.
        /// </summary>
        /// <param name="drawOver"></param>
        private async void PopulateClouds(bool drawOver = false)
        {
            await _worldHelper.PopulateClouds(canvas: Canvas_Root, drawOver: drawOver);
        }

        /// <summary>
        /// Shows current world.
        /// </summary>
        private void ShowCurrentWorld()
        {
            if (!App.World.IsEmpty())
            {
                Button_World.Tag = App.World;
                Button_World.Visibility = Visibility.Visible;
                WorldImageHolder.Content = GetWorldPicture(App.World);
                WorldNameHolder.Text = App.World.Name;
            }
        }

        #endregion

        #region Hub Login

        public async Task ConnectWithHubThenLogin()
        {
            if (await ConnectWithHub())
            {
                await TryLoginToHub();
            }
            else
            {
                var contentDialogue = new ContentDialogueWindow(title: "Connection failure!", message: "Would you like to try again?", result: async (result) =>
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

                var contentDialogue = new ContentDialogueWindow(title: "Login failure!", message: "Would you like to try again?", result: async (result) =>
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
                    var result = await HubService.Login(Avatar);

                    if (result != null && result.Avatar != null && !result.Avatar.IsEmpty())
                    {
                        Console.WriteLine("LoginToHub: OK");

                        var avatar = result.Avatar;

                        // Logged in user's avatar
                        Avatar = avatar;

                        // Clearing up canvas prior to login
                        AvatarMessengers.Clear();
                        Canvas_Root.Children.Clear();

                        _mainPage.SetIsBusy(true, "Preparing world...");

                        // Get avatars and constructs
                        await FetchAvatars();
                        PopulateClouds();
                        await FetchConstructs();
                        PopulateClouds(true);

                        ScrollIntoView(Avatar);
                        _isLoggedIn = true;

                        // Set connected user's avatar image
                        ShowCurrentUserAvatar();
                        ShowCurrentWorld();

                        _mainPage.SetIsBusy(false);
                        Grid_Root.Visibility = Visibility.Visible;

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

        #region Hub Logout

        private async Task LogoutFromHubThenDisconnect()
        {
            // If logged in then log out and disconnect from hub
            if (_isLoggedIn)
            {
                _mainPage.SetIsBusy(true);

                await HubService.Logout();

                if (HubService.IsConnected())
                {
                    await HubService.DisconnectAsync();
                }

                App.World = new World();

                _mainPage.SetIsBusy(false);
            }
        }

        #endregion

        #region Element

        /// <summary>
        /// Starts dragging an UIElement.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="uielement"></param>
        private void DragStart(Canvas canvas, PointerRoutedEventArgs e, UIElement uielement)
        {
            _elementHelper.DragStart(canvas, e, uielement);
        }

        /// <summary>
        /// Drags an UIElement.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="uielement"></param>
        private void DragElement(Canvas canvas, PointerRoutedEventArgs e, UIElement uielement)
        {
            _elementHelper.DragElement(canvas, e, uielement);
        }

        /// <summary>
        /// Stops dragging an UIElement.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="uielement"></param>
        private void DragRelease(PointerRoutedEventArgs e, UIElement uielement)
        {
            _elementHelper.DragRelease(e, uielement);
        }

        /// <summary>
        /// Gets the image from the provided UiElement.
        /// </summary>
        /// <param name="uielement"></param>
        /// <returns></returns>
        private Image GetImageFromUiElement(UIElement uielement, double size = 50, Stretch stretch = Stretch.Uniform)
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
        private object MoveElement(UIElement uIElement, PointerRoutedEventArgs e)
        {
            return _elementHelper.MoveElement(canvas: Canvas_Root, uIElement: uIElement, e: e);
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
            return _elementHelper.MoveElement(uIElement: uIElement, goToX: goToX, goToY: goToY, gotoZ: gotoZ, isCrafting: Button_ConstructCraft.IsChecked.Value);
        }

        #endregion

        #region Connection

        /// <summary>
        /// Prompts character selection and establishes communication to hub.
        /// </summary>
        /// <returns></returns>
        private async void SelectCharacterAndConnect()
        {
            try
            {
                Console.WriteLine("Button_Connect_Click");

                if (Character.IsEmpty())
                {
                    Characters = Characters.Any() ? Characters : JsonSerializer.Deserialize<Character[]>(Service.Properties.Resources.CharacterAssets).ToList();

                    var characterPicker = new CharacterPickerWindow(
                        characters: Characters,
                        characterSelected: async (character) =>
                        {
                            Character = character;
                            SetAvatarData();
                            _mainPage.SetLoggedInUserModel();

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
            var contentDialogue = new ContentDialogueWindow(title: "Leaving!", message: "Are you sure you want to leave this world?", result: (result) =>
            {
                if (result)
                    _mainPage.NavigateToPage(Constants.Page_WorldsPage);
            });

            contentDialogue.Show();
        }

        /// <summary>
        /// Subscribe to hub and start listening to hub events.
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
            //HubService.NewBroadcastConstructs += HubService_NewBroadcastConstructs;

            HubService.NewRemoveConstruct += HubService_NewRemoveConstruct;
            //HubService.NewRemoveConstructs += HubService_NewRemoveConstructs;

            HubService.NewBroadcastConstructPlacement += HubService_NewBroadcastConstructPlacement;

            HubService.NewBroadcastConstructRotation += HubService_NewBroadcastConstructRotation;
            //HubService.NewBroadcastConstructRotations += HubService_NewBroadcastConstructRotations;

            HubService.NewBroadcastConstructScale += HubService_NewBroadcastConstructScale;
            //HubService.NewBroadcastConstructScales += HubService_NewBroadcastConstructScales;

            HubService.NewBroadcastConstructMovement += HubService_NewBroadcastConstructMovement;

            #endregion

            #region Avatar Messaging

            HubService.AvatarTyping += HubService_AvatarTyping;
            HubService.NewTextMessage += HubService_NewTextMessage;
            HubService.NewImageMessage += HubService_NewImageMessage;

            #endregion

            Console.WriteLine("++SubscribeHub: OK");
        }

        /// <summary>
        /// Unsubscribe from hub and stop listening to hub events.
        /// </summary>
        private void UnsubscribeHub()
        {
            #region Hub Connectivity

            HubService.ConnectionReconnecting -= HubService_ConnectionReconnecting;
            HubService.ConnectionReconnected -= HubService_ConnectionReconnected;
            HubService.ConnectionClosed -= HubService_ConnectionClosed;

            #endregion

            #region Avatar World Events

            HubService.NewBroadcastAvatarMovement -= HubService_NewBroadcastAvatarMovement;
            HubService.NewBroadcastAvatarActivityStatus -= HubService_NewBroadcastAvatarActivityStatus;

            #endregion

            #region Avatar Connectivity

            HubService.AvatarLoggedIn -= HubService_AvatarLoggedIn;
            HubService.AvatarLoggedOut -= HubService_AvatarLoggedOut;
            HubService.AvatarDisconnected -= HubService_AvatarDisconnected;
            HubService.AvatarReconnected -= HubService_AvatarReconnected;

            #endregion

            #region Construct World Events

            HubService.NewBroadcastConstruct -= HubService_NewBroadcastConstruct;
            //HubService.NewBroadcastConstructs -= HubService_NewBroadcastConstructs;

            HubService.NewRemoveConstruct -= HubService_NewRemoveConstruct;
            //HubService.NewRemoveConstructs -= HubService_NewRemoveConstructs;

            HubService.NewBroadcastConstructPlacement -= HubService_NewBroadcastConstructPlacement;

            HubService.NewBroadcastConstructRotation -= HubService_NewBroadcastConstructRotation;
            //HubService.NewBroadcastConstructRotations -= HubService_NewBroadcastConstructRotations;

            HubService.NewBroadcastConstructScale -= HubService_NewBroadcastConstructScale;
            //HubService.NewBroadcastConstructScales -= HubService_NewBroadcastConstructScales;

            HubService.NewBroadcastConstructMovement -= HubService_NewBroadcastConstructMovement;

            #endregion

            #region Avatar Messaging

            HubService.AvatarTyping -= HubService_AvatarTyping;
            HubService.NewTextMessage -= HubService_NewTextMessage;
            HubService.NewImageMessage -= HubService_NewImageMessage;

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
                ActivityStatus = ActivityStatus.Idle,
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

        #region World

        /// <summary>
        /// Returns the provided world's image in a circular border.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private Border GetWorldPicture(World world, double size = 40)
        {
            return _worldHelper.GetWorldPicture(world: world, size: size);
        }

        #endregion

        #region Avatar

        /// <summary>
        /// Scroll the provided avatar into view.
        /// </summary>
        /// <param name="avatar"></param>
        private void ScrollIntoView(Avatar avatar)
        {
            if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, avatar.Id) is UIElement iElement)
            {
                CanvasScrollViewer.ScrollIntoView(
                    element: (Button)iElement,
                    horizontalMargin: (Window.Current.Bounds.Width / 2),
                    verticalMargin: (Window.Current.Bounds.Height / 2) - 50,
                    duration: new Duration(TimeSpan.FromSeconds(0)));
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
                    Margin = new Thickness(5),
                    Style = Application.Current.Resources["Panel_Style"] as Style,
                    //Height = 400
                };

                foreach (Avatar avatar in AvatarMessengers.Select(x => x.Avatar))
                {
                    var userImage = GetAvatarUserPicture(avatar);
                    userImage.Margin = new Thickness(5, 0, 5, 0);

                    var activeAvatarButton = new Button()
                    {
                        Tag = avatar,
                        Style = App.Current.TryFindResource("MaterialDesign_Button_Style_NoDropShadow") as Style,
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
                var pageSize = 10;

                var totalPageCount = _paginationHelper.GetTotalPageCount(pageSize, count);

                var tasks = new List<Task>();

                for (int pageIndex = 0; pageIndex < totalPageCount; pageIndex++)
                {
                    tasks.Add(GetAvatars(pageSize, pageIndex));
                }

                await Task.WhenAll(tasks.ToArray());
            }

            PopulateAvatarsInAvatarsContainer();
        }

        private async Task GetAvatars(int pageSize, int pageIndex)
        {
            // Get Avatars in small packets
            var response = await _httpServiceHelper.SendGetRequest<GetAvatarsQueryResponse>(
                actionUri: Constants.Action_GetAvatars,
                payload: new GetAvatarsQueryRequest() { Token = App.Token, PageIndex = pageIndex, PageSize = pageSize, WorldId = App.World.Id });

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK || !response.ExternalError.IsNullOrBlank())
            {
                var contentDialogue = new ContentDialogueWindow(title: "Error!", message: response.ExternalError.ToString());
                contentDialogue.Show();

                _mainPage.SetIsBusy(false);
            }

            var avatars = response.Avatars;

            if (avatars != null && avatars.Any())
            {
                Console.WriteLine("LoginToHub: avatars found: " + avatars.Count());

                foreach (var avatar in avatars)
                {
                    var avatarButton = GenerateAvatarButton(avatar);

                    SetAvatarActivityStatus(
                        avatarButton: avatarButton,
                        avatar: avatar,
                        activityStatus: avatar.ActivityStatus);

                    AddAvatarOnCanvas(
                        avatar: avatarButton,
                        x: avatar.Coordinate.X,
                        y: avatar.Coordinate.Y,
                        z: avatar.Coordinate.Z);

                    AvatarMessengers.Add(new AvatarMessenger { Avatar = avatar, IsLoggedIn = true });
                }

                AvatarsCount.Text = AvatarMessengers.Count().ToString();
            }
        }

        private async Task<long> GetAvatarsCount()
        {
            // Get Avatars count for this world
            var countResponse = await _httpServiceHelper.SendGetRequest<GetAvatarsCountQueryResponse>(
                actionUri: Constants.Action_GetAvatarsCount,
                payload: new GetAvatarsCountQueryRequest() { Token = App.Token, WorldId = App.World.Id });

            if (countResponse.HttpStatusCode != System.Net.HttpStatusCode.OK || !countResponse.ExternalError.IsNullOrBlank())
            {
                var contentDialogue = new ContentDialogueWindow(title: "Error!", message: countResponse.ExternalError.ToString());
                contentDialogue.Show();

                _mainPage.SetIsBusy(false);
            }

            return countResponse.Count;
        }

        /// <summary>
        /// Gets the user image as a circular border from the provided avatar.
        /// </summary>
        /// <param name="avatar"></param>
        /// <returns></returns>
        private Border GetAvatarUserPicture(Avatar avatar, double size = 40)
        {
            return _avatarHelper.GetAvatarUserPicture(avatar, size);
        }

        /// <summary>
        /// Gets the character image as a circular border from the provided avatar.
        /// </summary>
        /// <param name="avatar"></param>
        /// <returns></returns>
        private Border GetAvatarCharacterPicture(Avatar avatar, double size = 40)
        {
            return _avatarHelper.GetAvatarCharacterPicture(avatar: avatar, size: size, background: Colors.BlanchedAlmond);
        }

        /// <summary>
        /// Shows current user's avatar.
        /// </summary>
        private void ShowCurrentUserAvatar()
        {
            if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, Avatar.Id) is UIElement iElement)
            {
                var avatar = _avatarHelper.GetTaggedAvatar(iElement);

                Button_MyAvatar.Tag = avatar;
                AvatarImageHolder.Content = GetAvatarCharacterPicture(avatar);
                CurrentAvatarHolder.Visibility = Visibility.Visible;
                AvatarNameHolder.Text = avatar.Character.Name.Replace("_", " ");
            }
        }

        /// <summary>
        /// Aligns facing direction of current avatar wrt provided x.
        /// </summary>
        /// <param name="construct"></param>
        private void AlignUsersAvatarFaceDirection(double x)
        {
            _avatarHelper.AlignAvatarFaceDirection(x, Canvas_Root, Avatar.Id);
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
        /// Adds an avatar on canvas.
        /// </summary>
        /// <param name="avatar"></param>
        private Avatar AddAvatarOnCanvas(UIElement avatar, double x, double y, int? z = null)
        {
            return _avatarHelper.AddAvatarOnCanvas(avatar, Canvas_Root, x, y, z);
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
            if (_avatarHelper.GetAvatarButtonFromCanvas(canvas: Canvas_Root, avatarId: Avatar.Id) is UIElement iElement)
            {
                var avatarButton = (Button)iElement;
                var taggedAvatar = avatarButton.Tag as Avatar;

                SetAvatarActivityStatus(
                    avatarButton: avatarButton,
                    avatar: taggedAvatar,
                    activityStatus: activityStatus);

                await HubService.BroadcastAvatarActivityStatus(avatarId: taggedAvatar.Id, activityStatus: (int)activityStatus);

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
        /// Get constructs for the current world.
        /// </summary>
        /// <returns></returns>
        private async Task FetchConstructs()
        {
            var count = await GetConstructsCount();

            // If any constructs exist for this world start fetching asynchronously
            if (count > 0)
            {
                var pageSize = 10;

                var totalPageCount = _paginationHelper.GetTotalPageCount(pageSize: pageSize, dataCount: count);

                var tasks = new List<Task>();

                for (int pageIndex = 0; pageIndex < totalPageCount; pageIndex++)
                {
                    tasks.Add(GetConstructs(pageSize, pageIndex));
                }

                await Task.WhenAll(tasks.ToArray());

                Console.WriteLine("LoginToHub: Completed fetching constructs.");
            }
        }

        private async Task<long> GetConstructsCount()
        {
            // Get constructs count for this world
            var countResponse = await _httpServiceHelper.SendGetRequest<GetConstructsCountQueryResponse>(
                actionUri: Constants.Action_GetConstructsCount,
                payload: new GetConstructsCountQueryRequest() { Token = App.Token, WorldId = App.World.Id });

            if (countResponse.HttpStatusCode != System.Net.HttpStatusCode.OK || !countResponse.ExternalError.IsNullOrBlank())
            {
                var contentDialogue = new ContentDialogueWindow(title: "Error!", message: countResponse.ExternalError.ToString());
                contentDialogue.Show();

                _mainPage.SetIsBusy(false);
            }

            return countResponse.Count;
        }

        private async Task GetConstructs(int pageSize, int pageIndex)
        {
            // Get constructs in small packets
            var response = await _httpServiceHelper.SendGetRequest<GetConstructsQueryResponse>(
                actionUri: Constants.Action_GetConstructs,
                payload: new GetConstructsQueryRequest() { Token = App.Token, PageIndex = pageIndex, PageSize = pageSize, WorldId = App.World.Id });

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK || !response.ExternalError.IsNullOrBlank())
            {
                var contentDialogue = new ContentDialogueWindow(title: "Error!", message: response.ExternalError.ToString());
                contentDialogue.Show();

                _mainPage.SetIsBusy(false);
            }

            var constructs = response.Constructs;

            if (constructs != null && constructs.Any())
            {
                var children = Canvas_Root.Children.OfType<Button>().Where(x => x.Tag is Construct);

                foreach (var construct in constructs)
                {
                    // If a construct already exists then update that, this can happen as after avatar login, new constructs can start appearing thru HubService
                    if (children != null && children.Count() > 0 && children.Any(x => ((Construct)x.Tag).Id == construct.Id))
                    {
                        if (children.FirstOrDefault(x => ((Construct)x.Tag).Id == construct.Id) is Button constructBtn)
                        {
                            constructBtn.Tag = construct;

                            Canvas.SetLeft(constructBtn, construct.Coordinate.X);
                            Canvas.SetTop(constructBtn, construct.Coordinate.Y);
                            Canvas.SetZIndex(constructBtn, construct.Coordinate.Z);

                            ScaleElement(constructBtn, construct.Scale);
                            RotateElement(constructBtn, construct.Rotation);
                        }
                    }
                    else // If construct doesn't exist then add that
                    {
                        var constructBtn = GenerateConstructButton(
                           name: construct.Name,
                           imageUrl: construct.ImageUrl,
                           constructId: construct.Id,
                           inWorld: construct.World,
                           creator: construct.Creator,
                           createdOn: construct.CreatedOn);

                        AddConstructOnCanvas(
                            construct: constructBtn,
                            x: construct.Coordinate.X,
                            y: construct.Coordinate.Y,
                            z: construct.Coordinate.Z,
                            disableOpacityAnimation: true);

                        ScaleElement(constructBtn, construct.Scale);
                        RotateElement(constructBtn, construct.Rotation);
                    }
                }
            }
        }

        private void ShowConstructAssetsControl()
        {
            if (ContentControl_ConstructAssetsControlContainer.Content == null)
                ContentControl_ConstructAssetsControlContainer.Content = ConstructAssetPickerControl;

            ContentControl_ConstructAssetsContainer.Visibility = Visibility.Visible;
        }

        private void HideConstructAssetsControl()
        {
            ContentControl_ConstructAssetsContainer.Visibility = Visibility.Collapsed;
        }

        private void UnsubscribeConstructAssetPicker()
        {
            ConstructAssetPickerControl.AssetSelected -= ConstructAssetPickerControl_AssetSelected;
        }

        /// <summary>
        /// Subscribes to AssetSelected event of ConstructAssetPickerControl.
        /// </summary>
        private void SubscribeConstructAssetPicker()
        {
            ConstructAssetPickerControl.AssetSelected += ConstructAssetPickerControl_AssetSelected;
        }

        /// <summary>
        /// Subscription method for AssetSelected event of ConstructAssetPickerControl.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="constructAsset"></param>
        private void ConstructAssetPickerControl_AssetSelected(object sender, ConstructAsset constructAsset)
        {
            var constructBtn = GenerateConstructButton(
                name: constructAsset.Name,
                imageUrl: constructAsset.ImageUrl);

            _addingConstruct = constructBtn;
            ShowOperationalConstruct(_addingConstruct);
        }

        /// <summary>
        /// Aligns a construct button to the center point of the pressed pointer location.
        /// </summary>
        /// <param name="pressedPoint"></param>
        /// <param name="constructButton"></param>
        /// <param name="construct"></param>
        /// <returns></returns>
        private Construct CenterAlignNewConstructButton(PointerPoint pressedPoint, Button constructButton, Construct construct)
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

            construct = RotateElement(uIElement: _selectedConstruct, rotation: newRotation) as Construct;

            // Align avatar to construct point
            AlignUsersAvatarFaceDirection(x: construct.Coordinate.X);

            await HubService.BroadcastConstructRotation(constructId: construct.Id, rotation: construct.Rotation);

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
            AlignUsersAvatarFaceDirection(x: construct.Coordinate.X);

            await HubService.BroadcastConstructScale(constructId: construct.Id, scale: construct.Scale);

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
            AlignUsersAvatarFaceDirection(x: construct.Coordinate.X);

            await HubService.BroadcastConstructScale(constructId: construct.Id, scale: construct.Scale);

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
            AlignUsersAvatarFaceDirection(x: construct.Coordinate.X);

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
            AlignUsersAvatarFaceDirection(x: construct.Coordinate.X);

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
            AlignUsersAvatarFaceDirection(x: construct.Coordinate.X);

            RemoveConstructFromCanvas(_selectedConstruct);

            ShowSelectedConstruct(null); // Construct delete

            await HubService.RemoveConstruct(construct.Id);
        }

        /// <summary>
        /// Adds a construct to the canvas.
        /// </summary>
        /// <param name="construct"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private Construct AddConstructOnCanvas(UIElement construct, double x, double y, int? z = null, bool disableOpacityAnimation = false)
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
        private Button GenerateConstructButton(string name, string imageUrl, int? constructId = null, InWorld inWorld = null, Creator creator = null, DateTime? createdOn = null)
        {
            var obj = _constructHelper.GenerateConstructButton(
                name: name,
                imageUrl: imageUrl,
                constructId: constructId,
                inWorld: inWorld,
                creator: creator,
                createdOn: createdOn);

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
            return _elementHelper.ScaleElement(uIElement, scale);
        }

        /// <summary>
        /// Rotates an UIElement to the provided rotation. Returns the tagged object of the uIElement.
        /// </summary>
        /// <param name="uIElement"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        private object RotateElement(UIElement uIElement, float rotation)
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

            //var pointX = currrentPoint.Position.X;
            //var pointY = currrentPoint.Position.Y;

            //pointX = pointX / ((ScaleTransform)Canvas_Root.RenderTransform).ScaleX;
            //pointY = pointY / ((ScaleTransform)Canvas_Root.RenderTransform).ScaleY;

            // Align avatar to clicked point
            AlignUsersAvatarFaceDirection(pointX);

            //var maxX = Canvas_Root.Children.OfType<Button>().Where(z => z.Tag is Construct c && MultiselectedConstructs.Select(x => x.Id).Contains(c.Id)).Max(x => ((Construct)x.Tag).Coordinate.X);

            //UIElement fe = Canvas_Root.Children.OfType<Button>().Where(z => z.Tag is Construct c && MultiselectedConstructs.Select(x => x.Id).Contains(c.Id)).FirstOrDefault(x => ((Construct)x.Tag).Coordinate.X >= maxX);

            // var maxX = Canvas_Root.Children.OfType<Button>().Where(z => z.Tag is Construct c && MultiselectedConstructs.Select(x => x.Id).Contains(c.Id)).Max(x => ((Construct)x.Tag).Coordinate.X);

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

                var taggedObject = MoveElement(_movingConstruct, goToX, goToY);

                var construct = taggedObject as Construct;

                await HubService.BroadcastConstructMovement(construct.Id, construct.Coordinate.X, construct.Coordinate.Y, construct.Coordinate.Z);

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
            AlignUsersAvatarFaceDirection(x: construct.Coordinate.X);

            await HubService.BroadcastConstructMovement(construct.Id, construct.Coordinate.X, construct.Coordinate.Y, construct.Coordinate.Z);

            Console.WriteLine("Construct moved.");
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
            if (((Button)_messageToAvatar).Tag is Avatar avatar && !MessagingTextBox.Text.IsNullOrBlank() && Canvas_Root.Children.OfType<Button>().Any(x => x.Tag is Avatar avatar1 && avatar1.Id == avatar.Id))
            {
                await HubService.SendUnicastMessage(avatar.Id, MessagingTextBox.Text);

                // Add message bubble to own avatar
                if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, Avatar.Id) is UIElement iElement)
                {
                    AddChatBubbleToCanvas(MessagingTextBox.Text, iElement); // send message

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
        /// Adds a chat bubble to canvas on top of the avatar who sent it.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="avatar"></param>
        private void AddChatBubbleToCanvas(string msg, UIElement avatar)
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
            Border userImageHolder = GetAvatarUserPicture(taggedAvatar);

            // Textblock containing the message
            var messageHolder = new TextBlock()
            {
                Text = msg,
                Margin = new Thickness(5, 0, 5, 0),
                FontWeight = FontWeights.Regular,
                FontFamily = new FontFamily("Segoe UI"),
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Colors.Black),
                VerticalAlignment = VerticalAlignment.Center,
            };

            // If sent message then image on the left
            if (taggedAvatar.Id == Avatar.Id)
            {
                var receiver = ((Button)_messageToAvatar).Tag as Avatar;

                // If receiver avatar is forward from current avatar
                AlignUsersAvatarFaceDirection(receiver.Coordinate.X);

                chatContent.Children.Add(userImageHolder);
                chatContent.Children.Add(messageHolder);
            }
            else // If received message then image on the right
            {
                Button meUiElement = Canvas_Root.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar meAvatar && meAvatar.Id == Avatar.Id);
                Button senderUiElement = Canvas_Root.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar senderAvatar && senderAvatar.Id == taggedAvatar.Id);

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

                chatContent.Children.Add(messageHolder);
                chatContent.Children.Add(userImageHolder);
            }

            chatBubble.Content = chatContent;

            // Set opacity animation according to the length of the message
            DoubleAnimation opacityAnimation = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(msg.Length * 2),
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
                Canvas_Root.Children.Remove(chatBubble);
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

            Canvas_Root.Children.Add(chatBubble);

            fadeStoryBoard.Begin();
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