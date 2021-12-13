using System;
using System.Linq;
using System.Windows.Media.Effects;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Common;
using Image = Windows.UI.Xaml.Controls.Image;

namespace Worldescape
{
    public class ChatBubbleHelper
    {
        readonly AvatarHelper _avatarHelper;

        public ChatBubbleHelper(AvatarHelper avatarHelper)
        {
            _avatarHelper = avatarHelper;
        }

        /// <summary>
        /// Adds a chat bubble to canvas on top of the avatar who sent it.
        /// </summary>
        /// <param name="chatMessage"></param>
        /// <param name="fromAvatar"></param>
        /// <param name="messageType"></param>
        /// <param name="toAvatar"></param>
        /// <param name="canvas"></param>
        /// <param name="loggedInAvatar"></param>
        /// <returns></returns>
        public Button AddChatBubbleToCanvas(
            ChatMessage chatMessage,
            UIElement fromAvatar,
            MessageType messageType,
            UIElement toAvatar,
            Canvas canvas,
            Avatar loggedInAvatar,
            ChatMessage replyToChatMessage = null)
        {
            var fromAvatarButton = fromAvatar as Button;
            var fromTaggedAvatar = fromAvatarButton?.Tag as Avatar;

            var toAvatarButton = toAvatar as Button;
            var toTaggedAvatar = toAvatarButton?.Tag as Avatar;

            Button btnChatBubble = new Button()
            {
                Style = Application.Current.Resources["MaterialDesign_ChatButton_Style"] as Style,
                FontWeight = FontWeights.Regular,
                FontFamily = new FontFamily("Segoe UI"),
                MaxWidth = 600,
                Height = double.PositiveInfinity,
            };

            var x = fromTaggedAvatar.Coordinate.X - (fromAvatarButton.ActualWidth / 2);
            var y = fromTaggedAvatar.Coordinate.Y - (fromAvatarButton.ActualHeight / 2);

            StackPanel spContent = new StackPanel();

            // If sent message then image on the left
            if (fromTaggedAvatar.Id == loggedInAvatar.Id)
            {
                if (toAvatar != null)
                    _avatarHelper.AlignAvatarFaceDirectionWrtX(
                        x: toTaggedAvatar.Coordinate.X,
                        canvas: canvas,
                        avatarId: loggedInAvatar.Id);

                AddReplyMessageToChatBubble(
                    replyToChatMessage: replyToChatMessage,
                    taggedAvatar: toTaggedAvatar,
                    spContent: spContent,
                    loggedInAvatar: loggedInAvatar);

                AddMessageToChatBubble(
                    chatMessage: chatMessage,
                    messageType: messageType,
                    spContent: spContent,
                    taggedAvatar: fromTaggedAvatar,
                    loggedInAvatar: loggedInAvatar);
            }
            else // If received message then image on the right
            {
                Button meAvatarButton = canvas.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar meAvatar && meAvatar.Id == loggedInAvatar.Id);
                var receiver = meAvatarButton.Tag as Avatar;

                // This is later used when replying to the sender.
                btnChatBubble.Tag = chatMessage;

                _avatarHelper.AlignAvatarFaceDirectionWrtX(
                      x: receiver.Coordinate.X,
                      canvas: canvas,
                      avatarId: fromTaggedAvatar.Id);

                AddReplyMessageToChatBubble(
                    replyToChatMessage: replyToChatMessage,
                    taggedAvatar: receiver,
                    spContent: spContent,
                    loggedInAvatar: loggedInAvatar);

                AddMessageToChatBubble(
                    chatMessage: chatMessage,
                    messageType: messageType,
                    spContent: spContent,
                    taggedAvatar: fromTaggedAvatar,
                    loggedInAvatar: loggedInAvatar);
            }

            btnChatBubble.Content = spContent;

            // Set opacity animation according to the length of the message
            DoubleAnimation opacityAnimation = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMinutes(2)//TimeSpan.FromSeconds(msg.Length * 2).Add(TimeSpan.FromMilliseconds(300)),
            };

            DoubleAnimation moveYAnimation = new DoubleAnimation()
            {
                From = y,
                To = y - 400,
                Duration = TimeSpan.FromSeconds(100),
                EasingFunction = new ExponentialEase()
                {
                    EasingMode = EasingMode.EaseOut,
                    Exponent = 5,
                }
            };

            // after opacity reaches zero delete this from canvas
            opacityAnimation.Completed += (s, e) =>
            {
                canvas.Children.Remove(btnChatBubble);
            };

            Storyboard.SetTarget(opacityAnimation, btnChatBubble);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Button.OpacityProperty));

            Storyboard.SetTarget(moveYAnimation, btnChatBubble);
            Storyboard.SetTargetProperty(moveYAnimation, new PropertyPath(Canvas.TopProperty));

            Storyboard fadeStoryBoard = new Storyboard();
            fadeStoryBoard.Children.Add(opacityAnimation);
            fadeStoryBoard.Children.Add(moveYAnimation);

            // Add to canvas
            Canvas.SetLeft(btnChatBubble, x);
            Canvas.SetTop(btnChatBubble, y);
            Canvas.SetZIndex(btnChatBubble, 999);

            // Add a shadow effect to the chat bubble
            btnChatBubble.Effect = new DropShadowEffect() { ShadowDepth = 4, Color = Colors.Black, BlurRadius = 10, Opacity = 0.5 };

            canvas.Children.Add(btnChatBubble);

            fadeStoryBoard.Begin();

            return btnChatBubble;
        }

        private void AddMessageToChatBubble(
            ChatMessage chatMessage,
            Avatar taggedAvatar,
            MessageType messageType,
            StackPanel spContent,
            Avatar loggedInAvatar)
        {
            StackPanel spPlaceholder = new StackPanel() { Orientation = Orientation.Horizontal };

            Border brUserImage = _avatarHelper.GetAvatarUserPicture(taggedAvatar);
            brUserImage.VerticalAlignment = VerticalAlignment.Top;

            // Textblock containing the message
            var tbMsg = new TextBlock()
            {
                Text = chatMessage.Message,
                Margin = new Thickness(5, 0, 5, 0),
                FontWeight = FontWeights.Regular,
                FontFamily = new FontFamily("Segoe UI"),
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Colors.Black),
                VerticalAlignment = VerticalAlignment.Center,
            };

            if (loggedInAvatar.Id == taggedAvatar.Id)
            {
                spPlaceholder.HorizontalAlignment = HorizontalAlignment.Left;
                spPlaceholder.Children.Add(brUserImage);
                AddMessageTypeIconText(messageType: messageType, spUserImageAndMessage: spPlaceholder);
                spPlaceholder.Children.Add(tbMsg);
            }
            else
            {
                spPlaceholder.HorizontalAlignment = HorizontalAlignment.Right;
                spPlaceholder.Children.Add(brUserImage);
                spPlaceholder.Children.Add(tbMsg);
                AddMessageTypeIconText(messageType: messageType, spUserImageAndMessage: spPlaceholder);
            }

            spContent.Children.Add(spPlaceholder);
        }

        private void AddReplyMessageToChatBubble(
            ChatMessage replyToChatMessage,
            Avatar taggedAvatar,
            StackPanel spContent,
            Avatar loggedInAvatar)
        {
            if (taggedAvatar != null && replyToChatMessage != null)
            {
                StackPanel spPlaceholder = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(-5, 5, 5, 5) };

                Border brUserImage = _avatarHelper.GetAvatarUserPicture(taggedAvatar, 30);
                brUserImage.VerticalAlignment = VerticalAlignment.Top;

                // Textblock containing the reply message
                var tbMsg = new TextBlock()
                {
                    Text = replyToChatMessage.Message,
                    Margin = new Thickness(5, 0, 5, 0),
                    FontWeight = FontWeights.Regular,
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 14,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Colors.Black),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                };

                //if (loggedInAvatar.Id == taggedAvatar.Id)
                //{
                //    spPlaceholder.HorizontalAlignment = HorizontalAlignment.Left;
                //    spPlaceholder.Children.Add(brUserImage);
                //    spPlaceholder.Children.Add(tbMsg);
                //}
                //else
                //{
                spPlaceholder.HorizontalAlignment = HorizontalAlignment.Left;
                spPlaceholder.Children.Add(brUserImage);
                spPlaceholder.Children.Add(tbMsg);
                //}

                spContent.Children.Add(spPlaceholder);
            }
        }

        /// <summary>
        /// Add an icon to the message content according to message type.
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="spUserImageAndMessage"></param>
        private void AddMessageTypeIconText(MessageType messageType, StackPanel spUserImageAndMessage)
        {
            var icon = new Image() { Margin = new Thickness(5, 5, 5, 0) };

            switch (messageType)
            {
                case MessageType.Broadcast:
                    {
                        icon.Source = new BitmapImage(new Uri("ms-appx:///Worldescape/Assets/Icons/quickreply_black_24dp.svg"));
                    }
                    break;
                case MessageType.Unicast:
                    {
                        icon.Source = new BitmapImage(new Uri("ms-appx:///Worldescape/Assets/Icons/chat_black_24dp.svg"));
                    }
                    break;
                default:
                    break;
            }

            spUserImageAndMessage.Children.Add(icon);
        }
    }
}
