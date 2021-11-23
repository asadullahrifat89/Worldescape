using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Data;

namespace Worldescape
{
    public class AvatarHelper
    {
        /// <summary>
        /// Gets the user image in a circular border from the provided avatar.
        /// </summary>
        /// <param name="avatar"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public Border GetAvatarUserPicture(Avatar avatar, double size = 40)
        {
            var bitmapImage = new BitmapImage(new Uri(avatar.User.ImageUrl));

            var imageBorder = new Border()
            {
                Height = size,
                Width = size,
                CornerRadius = new CornerRadius(40),
                ClipToBounds = true,
            };

            Image userImage = new Image()
            {
                Source = bitmapImage,
                Height = size,
                Width = size,
                Stretch = Stretch.UniformToFill,
            };

            imageBorder.Child = userImage;
            return imageBorder;
        }

        /// <summary>
        /// Gets the character image in a circular border from the provided avatar.
        /// </summary>
        /// <param name="avatar"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public Border GetAvatarCharacterPicture(Avatar avatar, double size = 40)
        {
            var bitmapImage = new BitmapImage(new Uri(avatar.Character.ImageUrl));

            var imageBorder = new Border()
            {
                Height = size,
                Width = size,
                CornerRadius = new CornerRadius(40),
                ClipToBounds = true,
            };

            Image userImage = new Image()
            {
                Source = bitmapImage,
                Height = size,
                Width = size,
                Stretch = Stretch.UniformToFill,
            };

            imageBorder.Child = userImage;
            return imageBorder;
        }

        /// <summary>
        /// Returns the tagged avatar object with the provided UIElement.
        /// </summary>
        /// <param name="uIElement"></param>
        /// <returns></returns>
        public Avatar GetTaggedAvatar(UIElement uIElement)
        {
            if (uIElement == null)
                return null;

            var avatar = ((Button)uIElement).Tag as Avatar;

            return avatar;
        }

        /// <summary>
        /// Aligns facing direction of current avatar wrt provided x.
        /// </summary>
        /// <param name="construct"></param>
        public void AlignAvatarFaceDirection(
            double x,
            Canvas canvas,
            int avatarId)
        {
            Button senderUiElement = canvas.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar taggedAvatar && taggedAvatar.Id == avatarId);
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
        /// Adds an avatar on canvas.
        /// </summary>
        /// <param name="avatar"></param>
        public Avatar AddAvatarOnCanvas(
            UIElement avatar,
            Canvas canvas,
            double x,
            double y,
            int? z = null)
        {
            Canvas.SetLeft(avatar, x);
            Canvas.SetTop(avatar, y);

            if (z.HasValue)
            {
                Canvas.SetZIndex(avatar, z.Value);
            }

            canvas.Children.Add(avatar);

            var taggedAvatar = ((Button)avatar).Tag as Avatar;
            taggedAvatar.Coordinate.X = x;
            taggedAvatar.Coordinate.Y = y;

            return taggedAvatar;
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
        public Button GenerateAvatarButton(Avatar avatar)
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

            //img.Effect = new DropShadowEffect() { ShadowDepth = 10, Color = Colors.Black, BlurRadius = 10, Opacity = 0.5, Direction = -90 };
            return obj;
        }

        /// <summary>
        /// Returns the avatar button from the provided canvas and avatarId.
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="avatarId"></param>
        /// <returns></returns>
        public UIElement GetAvatarButtonFromCanvas(Canvas canvas, int avatarId)
        {
            if (avatarId == 0)
                return null;
            if (canvas == null)
                return null;

            return canvas.Children.OfType<Button>().FirstOrDefault(x => x.Tag is Avatar avatar && avatar.Id == avatarId);
        }
    }
}
