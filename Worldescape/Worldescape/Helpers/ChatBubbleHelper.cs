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
        /// <param name="msg"></param>
        /// <param name="fromAvatar"></param>
        /// <param name="messageType"></param>
        /// <param name="toAvatar"></param>
        /// <param name="canvas"></param>
        /// <param name="loggedInAvatar"></param>
        /// <returns></returns>
        public Button AddChatBubbleToCanvas(
            string msg,
            UIElement fromAvatar,
            MessageType messageType,
            UIElement toAvatar,
            Canvas canvas,
            Avatar loggedInAvatar)
        {
            var avatarButton = fromAvatar as Button;
            var taggedAvatar = avatarButton.Tag as Avatar;

            Button btnChatBubble = new Button()
            {
                Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                FontWeight = FontWeights.Regular,
                FontFamily = new FontFamily("Segoe UI"),
                MaxWidth = 600,
                Height = double.PositiveInfinity,
            };

            var x = taggedAvatar.Coordinate.X - (avatarButton.ActualWidth / 2);
            var y = taggedAvatar.Coordinate.Y - (avatarButton.ActualHeight / 2);

            TextBlock tbMsg = null;

            // Textblock containing the message
            tbMsg = new TextBlock()
            {
                Text = msg,
                Margin = new Thickness(5, 0, 5, 0),
                FontWeight = FontWeights.Regular,
                FontFamily = new FontFamily("Segoe UI"),
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Colors.Black),
                VerticalAlignment = VerticalAlignment.Center,
            };

            StackPanel spUserImageAndMessage = new StackPanel() { Orientation = Orientation.Horizontal };
            Border brUserImage = _avatarHelper.GetAvatarUserPicture(taggedAvatar);
            brUserImage.VerticalAlignment = VerticalAlignment.Top;

            // If sent message then image on the left
            if (taggedAvatar.Id == loggedInAvatar.Id)
            {
                if (toAvatar != null)
                {
                    var receiver = ((Button)toAvatar).Tag as Avatar;
                    _avatarHelper.AlignAvatarFaceDirectionWrtX(x: receiver.Coordinate.X, canvas: canvas, avatarId: loggedInAvatar.Id);
                }

                spUserImageAndMessage.Children.Add(brUserImage);

                // Add icon of message type
                AddMessageTypeIconText(messageType, spUserImageAndMessage);
                spUserImageAndMessage.Children.Add(tbMsg);
            }
            else // If received message then image on the right
            {
                Button meUiElement = canvas.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar meAvatar && meAvatar.Id == loggedInAvatar.Id);
                Button senderUiElement = canvas.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar senderAvatar && senderAvatar.Id == taggedAvatar.Id);

                var receiver = meUiElement.Tag as Avatar;
                var sender = taggedAvatar;

                // If sender avatar is forward from current avatar
                if (sender.Coordinate.X > receiver.Coordinate.X)
                    senderUiElement.RenderTransform = new ScaleTransform() { ScaleX = -1 };
                else
                    senderUiElement.RenderTransform = new ScaleTransform() { ScaleX = 1 };

                btnChatBubble.Tag = taggedAvatar;                

                spUserImageAndMessage.Children.Add(tbMsg);

                // Add icon of message type
                AddMessageTypeIconText(messageType, spUserImageAndMessage);
                spUserImageAndMessage.Children.Add(brUserImage);
            }

            btnChatBubble.Content = spUserImageAndMessage;

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
