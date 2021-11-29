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
using Windows.UI.Input;

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

        EasingFunctionBase _constructEaseOut = new ExponentialEase()
        {
            EasingMode = EasingMode.EaseOut,
            Exponent = 5,
        };

        readonly MainPage _mainPage;
        readonly AvatarHelper _avatarHelper;
        readonly ConstructHelper _constructHelper;
        readonly WorldHelper _worldHelper;
        readonly HttpServiceHelper _httpServiceHelper;
        readonly PageNumberHelper _pageNumberHelper;

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
            _pageNumberHelper = App.ServiceProvider.GetService(typeof(PageNumberHelper)) as PageNumberHelper;

            SubscribeHub();
            CommenseConnection();
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

                // Drag start of a constuct
                _objectLeft = Canvas.GetLeft(uielement);
                _objectTop = Canvas.GetTop(uielement);

                var currentPoint = e.GetCurrentPoint(Canvas_Root);

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
            if (Button_ConstructCraft.IsChecked.Value)
            {
                if (!CanManipulateConstruct())
                    return;

                UIElement uielement = (UIElement)sender;

                if (_isPointerCaptured)
                {
                    var currentPoint = e.GetCurrentPoint(Canvas_Root);

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
                _isPointerCaptured = false;
                uielement.ReleasePointerCapture(e.Pointer);

                _selectedConstruct = uielement;
                ShowSelectedConstruct(uielement); // Construct

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
            if (Button_ConstructMultiSelect.IsChecked.Value && MultiselectedConstructs.Any())
            {
                var currrentPoint = e.GetCurrentPoint(Canvas_Root);

                // Align avatar to clicked point
                AlignAvatarFaceDirection(currrentPoint.Position.X);

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
                var pressedPoint = e.GetCurrentPoint(Canvas_Root);

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
                var pressedPoint = e.GetCurrentPoint(Canvas_Root);

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
                _messageToAvatar = _avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, avatar.Id);
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

        private void Button_World_Click(object sender, RoutedEventArgs e)
        {

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
            CommenseConnection();
        }

        /// <summary>
        /// Logs out and disconnects the current user from Hub.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Leave_Click(object sender, RoutedEventArgs e)
        {
            await LeaveWorld();
        }

        #endregion

        #region Construct

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
        /// Activates crafting mode. This enables operations buttons for a construct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_ConstructCraft_Click(object sender, RoutedEventArgs e)
        {
            if (CanPerformWorldEvents())
            {
                //Button_ConstructCraft.Content = Button_ConstructCraft.IsChecked.Value ? "Constructing" : "Construct";

                Button_ConstructMove.IsChecked = false;
                //Button_ConstructMove.Content = "Move";

                Button_ConstructClone.IsChecked = false;
                //Button_ConstructClone.Content = "Clone";

                Button_ConstructAdd.IsChecked = false;
                //Button_ConstructAdd.Content = "Add";

                Button_ConstructMultiSelect.IsChecked = false;
                //Button_ConstructMultiSelect.Content = "Select";

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
                    //Button_ConstructAdd.Content = "Add";
                    Button_ConstructAdd.IsChecked = false;

                    return;
                }

                //Button_ConstructAdd.Content = "Adding";

                if (!ConstructAssets.Any())
                {
                    ConstructAssets = JsonSerializer.Deserialize<ConstructAsset[]>(Service.Properties.Resources.ConstructAssets).ToList();
                    ConstructCategories = ConstructAssets.Select(x => x.Category).Distinct().Select(z => new ConstructCategory() { ImageUrl = @$"ms-appx:///Images/World_Objects/{z}.png", Name = z }).ToList();
                }

                var constructAssetPicker = new ConstructAssetPickerWindow(
                    constructAssets: ConstructAssets,
                    constructCategories: ConstructCategories,
                    assetSelected: (constructAsset) =>
                    {
                        var constructBtn = GenerateConstructButton(
                            name: constructAsset.Name,
                            imageUrl: constructAsset.ImageUrl);

                        _addingConstruct = constructBtn;
                        ShowOperationalConstruct(_addingConstruct);
                    });

                // If the picker was closed without a selection of an asset, set the Button_ConstructAdd to default
                constructAssetPicker.Closed += (s, e) =>
                {
                    if (_addingConstruct == null)
                    {
                        //Button_ConstructAdd.Content = "Add";
                        Button_ConstructAdd.IsChecked = false;
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
                        _selectedConstruct = _constructHelper.GetConstructButtonFromCanvas(Canvas_Root, element.Id);

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
                        _selectedConstruct = _constructHelper.GetConstructButtonFromCanvas(Canvas_Root, element.Id);

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
                        _selectedConstruct = _constructHelper.GetConstructButtonFromCanvas(Canvas_Root, element.Id);

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
                        _selectedConstruct = _constructHelper.GetConstructButtonFromCanvas(Canvas_Root, element.Id);

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
                        _selectedConstruct = _constructHelper.GetConstructButtonFromCanvas(Canvas_Root, element.Id);

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
                        _selectedConstruct = _constructHelper.GetConstructButtonFromCanvas(Canvas_Root, element.Id);

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
                var userPicture = GetAvatarUserPicture(avatar, 100);

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
            if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, Avatar.Id) is UIElement iElement)
            {
                CanvasScrollViewer.ScrollIntoView((Button)iElement);
            }
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
        private void MessagingTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Button_SendUnicastMessage_Click(sender, e);
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
                _messageToAvatar = _avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, avatar.Id);
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
            if (!CanPerformWorldEvents())
                return;

            if (_messageToAvatar == null)
                return;

            if (((Button)_messageToAvatar).Tag is Avatar avatar && !MessagingTextBox.Text.IsNullOrBlank())
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

                var userImage = GetAvatarUserPicture(taggedAvatar);
                var avatarImage = GetAvatarCharacterPicture(taggedAvatar);

                StackPanel stackPanelContent = new StackPanel() { Orientation = Orientation.Vertical };

                stackPanelContent.Children.Add(userImage);
                stackPanelContent.Children.Add(avatarImage);

                SelectedAvatarHolder.Content = stackPanelContent;
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
                AlignAvatarFaceDirection(receiver.Coordinate.X);

                MessagingFromAvatarHolder.Content = GetImageFromUiElement(_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, Avatar.Id));
                MessagingToAvatarHolder.Content = GetImageFromUiElement(receiverUiElement);
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

            ScaleElement(constructBtn, construct.Scale);
            RotateElement(constructBtn, construct.Rotation);

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

        private void HubService_AvatarLoggedOut(int avatarId)
        {
            if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, avatarId) is UIElement iElement)
            {
                var avatarMessenger = AvatarMessengers.FirstOrDefault(x => x.Avatar.Id == avatarId);

                if (avatarMessenger != null)
                {
                    AvatarMessengers.Remove(avatarMessenger);
                    ParticipantsCount.Text = AvatarMessengers.Count().ToString();
                }

                Canvas_Root.Children.Remove(iElement);

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
            if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, avatar.Id) is UIElement iElement)
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
                if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, avatarId) is UIElement iElement)
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
                    _mainPage.SetIsBusy(true);
                    var result = await HubService.Login(Avatar);

                    if (result != null)
                    {
                        Console.WriteLine("LoginToHub: OK");

                        var avatars = result.Avatars;

                        // Clearing up canvas prior to login
                        AvatarMessengers.Clear();
                        Canvas_Root.Children.Clear();

                        AddAvatarsToCanvasAfterHubLogin(avatars);

                        _mainPage.SetIsBusy(false);

                        await GetConstructs();

                        _isLoggedIn = true;

                        // Set connected user's avatar image
                        ShowCurrentUserAvatar();
                        ShowCurrentWorld();
                        return true;
                    }
                    else
                    {
                        _mainPage.SetIsBusy(false);
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
        /// Gets the image from the provided UiElement.
        /// </summary>
        /// <param name="uielement"></param>
        /// <returns></returns>
        private Image GetImageFromUiElement(UIElement uielement, double size = 50, Stretch stretch = Stretch.Uniform)
        {
            var oriBitmap = ((Image)((Button)uielement).Content).Source as BitmapImage;

            var bitmap = new BitmapImage(new Uri(oriBitmap.UriSource.OriginalString, UriKind.RelativeOrAbsolute));

            var img = new Image()
            {
                Source = bitmap,
                Stretch = stretch,
                Height = size,
                Width = size,
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
            var pressedPoint = e.GetCurrentPoint(Canvas_Root);

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

            // Set moving status on start, if own avatar and if crafting mode is set then set crafting status
            if (taggedObject is Avatar avatar)
            {
                if (Button_ConstructCraft.IsChecked.Value && avatar.Id == Avatar.Id)
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
                    if (Button_ConstructCraft.IsChecked.Value && taggedAvatar.Id == Avatar.Id)
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
        /// Prompts character selection and establishes communication to hub.
        /// </summary>
        /// <returns></returns>
        private async void CommenseConnection()
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
        private async Task LeaveWorld()
        {
            var result = MessageBox.Show("Are you sure you want to leave this world?", "Leaving...", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.Cancel)
                return;

            await LogoutFromHubThenDisconnect();

            _mainPage.NavigateToPage(Constants.Page_WorldsPage);
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

        #region World
        private Border GetWorldPicture(World world, double size = 40)
        {
            return _worldHelper.GetWorldPicture(world: world, size: size);
        }

        #endregion

        #region Avatar

        private void AddAvatarsToCanvasAfterHubLogin(Avatar[] avatars)
        {
            if (avatars != null && avatars.Any())
            {
                Console.WriteLine("LoginToHub: avatars found: " + avatars.Count());

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
        private void AlignAvatarFaceDirection(double x)
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
            if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, Avatar.Id) is UIElement iElement)
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
            if (_avatarHelper.GetAvatarButtonFromCanvas(Canvas_Root, Avatar.Id) is UIElement iElement)
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
            _avatarHelper.SetAvatarActivityStatus(avatarButton, avatar, activityStatus);
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
        private async Task GetConstructs()
        {
            // Get constructs count for this world
            var countResponse = await _httpServiceHelper.SendGetRequest<GetConstructsCountQueryResponse>(
                actionUri: Constants.Action_GetConstructsCount,
                payload: new GetConstructsCountQueryRequest() { Token = App.Token, WorldId = App.World.Id });

            if (countResponse.HttpStatusCode != System.Net.HttpStatusCode.OK || !countResponse.ExternalError.IsNullOrBlank())
            {
                MessageBox.Show(countResponse.ExternalError.ToString());
                _mainPage.SetIsBusy(false);
            }

            // If any constructs exist for this world start fetching asynchronously
            if (countResponse.Count > 0)
            {
                var pageSize = 20;

                var totalPageCount = _pageNumberHelper.GetTotalPageCount(pageSize, countResponse.Count);

                for (int pageIndex = 0; pageIndex < totalPageCount; pageIndex++)
                {
                    // Get constructs in small packets
                    var response = await _httpServiceHelper.SendGetRequest<GetConstructsQueryResponse>(
                        actionUri: Constants.Action_GetConstructs,
                        payload: new GetConstructsQueryRequest() { Token = App.Token, PageIndex = pageIndex, PageSize = pageSize, WorldId = App.World.Id });

                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK || !response.ExternalError.IsNullOrBlank())
                    {
                        MessageBox.Show(response.ExternalError.ToString());
                        _mainPage.SetIsBusy(false);
                    }

                    var constructs = response.Constructs;

                    if (constructs != null && constructs.Any())
                    {
                        foreach (var construct in constructs)
                        {
                            // If a construct already exists then update that, this can happen as after avatar login, new constructs can start appearing thru HubService
                            if (Canvas_Root.Children.OfType<Button>().Count(x => x.Tag is Construct) > 0
                                && Canvas_Root.Children.OfType<Button>().Where(x => x.Tag is Construct).Any(x => ((Construct)x.Tag).Id == construct.Id))
                            {
                                if (Canvas_Root.Children.OfType<Button>().Where(x => x.Tag is Construct).FirstOrDefault(x => ((Construct)x.Tag).Id == construct.Id) is Button constructBtn)
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
                                    z: construct.Coordinate.Z);

                                ScaleElement(constructBtn, construct.Scale);
                                RotateElement(constructBtn, construct.Rotation);
                            }
                        }
                    }
                }

                Console.WriteLine("LoginToHub: Completed fetching constructs.");
            }
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
            return _constructHelper.CenterAlignNewConstructButton(pressedPoint, constructButton, construct);
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
            //Button_ConstructMultiSelect.Content = "Select";
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
        private Construct AddConstructOnCanvas(UIElement construct, double x, double y, int? z = null)
        {
            return _constructHelper.AddConstructOnCanvas(construct, Canvas_Root, x, y, z);
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
                AlignAvatarFaceDirection(receiver.Coordinate.X);

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

        #endregion

        #endregion
    }
}