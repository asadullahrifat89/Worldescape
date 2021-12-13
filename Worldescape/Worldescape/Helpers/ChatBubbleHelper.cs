using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
                Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
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
                {
                    var receiver = ((Button)toAvatar).Tag as Avatar;
                    _avatarHelper.AlignAvatarFaceDirectionWrtX(x: receiver.Coordinate.X, canvas: canvas, avatarId: loggedInAvatar.Id);
                }

                if (replyToChatMessage != null)
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
                StackPanel spUserImageAndMessage = new StackPanel() { Orientation = Orientation.Horizontal };

                var buttons = canvas.Children.OfType<Button>();

                Button meUiElement = buttons.FirstOrDefault(x => x.Tag is Avatar meAvatar && meAvatar.Id == loggedInAvatar.Id);
                Button senderUiElement = buttons.FirstOrDefault(x => x.Tag is Avatar senderAvatar && senderAvatar.Id == fromTaggedAvatar.Id);

                var receiverAvatar = meUiElement.Tag as Avatar;
                var senderAvatar = fromTaggedAvatar;

                // If sender avatar is forward from current avatar
                if (senderAvatar.Coordinate.X > receiverAvatar.Coordinate.X)
                    senderUiElement.RenderTransform = new ScaleTransform() { ScaleX = -1 };
                else
                    senderUiElement.RenderTransform = new ScaleTransform() { ScaleX = 1 };

                // This is later used when replying to the sender.
                btnChatBubble.Tag = chatMessage;

                if (replyToChatMessage != null)
                    AddReplyMessageToChatBubble(
                        replyToChatMessage: replyToChatMessage,
                        taggedAvatar: receiverAvatar,
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
            StackPanel spUserImageAndMessage = new StackPanel() { Orientation = Orientation.Horizontal };

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
                spUserImageAndMessage.HorizontalAlignment = HorizontalAlignment.Left;
                spUserImageAndMessage.Children.Add(brUserImage);
                // Add icon of message type
                AddMessageTypeIconText(messageType, spUserImageAndMessage);
                spUserImageAndMessage.Children.Add(tbMsg);
            }
            else
            {
                spUserImageAndMessage.HorizontalAlignment = HorizontalAlignment.Right;
                spUserImageAndMessage.Children.Add(tbMsg);
                // Add icon of message type
                AddMessageTypeIconText(messageType, spUserImageAndMessage);
                spUserImageAndMessage.Children.Add(brUserImage);
            }
           
            spContent.Children.Add(spUserImageAndMessage);
        }

        private void AddReplyMessageToChatBubble(
            ChatMessage replyToChatMessage,
            Avatar taggedAvatar,
            StackPanel spContent,
            Avatar loggedInAvatar)
        {
            if (taggedAvatar != null)
            {
                StackPanel spUserImageAndMessageReply = new StackPanel() { Orientation = Orientation.Horizontal };

                Border brUserImageReply = _avatarHelper.GetAvatarUserPicture(taggedAvatar, 20);
                brUserImageReply.VerticalAlignment = VerticalAlignment.Top;

                // Textblock containing the reply message
                var tbReplyMsg = new TextBlock()
                {
                    Text = replyToChatMessage.Message,
                    Margin = new Thickness(5, 0, 5, 0),
                    FontWeight = FontWeights.Regular,
                    FontFamily = new FontFamily("Segoe UI"),
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Colors.Black),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                };

                if (loggedInAvatar.Id == taggedAvatar.Id)
                {
                    spUserImageAndMessageReply.HorizontalAlignment = HorizontalAlignment.Left;
                    spUserImageAndMessageReply.Children.Add(brUserImageReply);
                    spUserImageAndMessageReply.Children.Add(tbReplyMsg);
                }
                else
                {
                    spUserImageAndMessageReply.HorizontalAlignment = HorizontalAlignment.Right;
                    spUserImageAndMessageReply.Children.Add(brUserImageReply);
                    spUserImageAndMessageReply.Children.Add(tbReplyMsg);
                }

                spContent.Children.Add(spUserImageAndMessageReply);
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
