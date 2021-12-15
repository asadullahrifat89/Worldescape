using System;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Common;
using Worldescape.Service;

namespace Worldescape
{
    public class AvatarHelper
    {
        #region Fields

        readonly UrlHelper _urlHelper;

        #endregion

        #region Ctor

        public AvatarHelper()
        {
            _urlHelper = App.ServiceProvider.GetService(typeof(UrlHelper)) as UrlHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the user image in a circular border from the provided avatar.
        /// </summary>
        /// <param name="avatar"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public Border GetAvatarUserPictureFrame(
            Avatar avatar,
            double size = 40,
            Color? background = null)
        {
            var bitmapImage = new BitmapImage(new Uri(avatar.User.ImageUrl.Contains("ms-appx:") ? avatar.User.ImageUrl : _urlHelper.BuildBlobUrl(App.Token, avatar.User.ImageUrl)));
            return PrepareRoundPictureFrame(
                size: size,
                bitmapImage: bitmapImage,
                background: background);
        }

        /// <summary>
        /// Gets the character image in a circular border from the provided avatar.
        /// </summary>
        /// <param name="avatar"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public Border GetAvatarCharacterPictureFrame(
            Avatar avatar,
            double size = 40,
            Color? background = null)
        {
            var bitmapImage = new BitmapImage(new Uri(avatar.Character.ImageUrl));
            return PrepareRoundPictureFrame(
                size: size,
                bitmapImage: bitmapImage,
                background: background);
        }

        /// <summary>
        /// Prepares a rounded border to contain the provided bitmap image.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="bitmapImage"></param>
        /// <returns></returns>
        private Border PrepareRoundPictureFrame(
            double size,
            BitmapImage bitmapImage,
            Color? background = null)
        {
            var imageBorder = new Border()
            {
                Height = size,
                Width = size,
                CornerRadius = new CornerRadius(40),
                ClipToBounds = true,
                Background = background != null ? new SolidColorBrush(background.Value) : new SolidColorBrush(Colors.Transparent),
            };

            Image userImage = new Image()
            {
                Source = bitmapImage,
                Height = size,
                Width = size,
                Stretch = Stretch.UniformToFill,
                Name = "AvatarUserImage",
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
                if (GetAvatarCharacterImage(avatarButton) is Image img && img.Source is BitmapImage bitmap)
                {
                    bitmap.UriSource = new Uri(statusBoundImageUrl.ImageUrl);
                }
            }
        }

        /// <summary>
        /// Get avatar character image from avatar button.
        /// </summary>
        /// <param name="avatarButton"></param>
        /// <returns></returns>
        private Image GetAvatarCharacterImage(Button avatarButton)
        {
            if (avatarButton.Content is StackPanel spContent && spContent.Children.OfType<Image>().FirstOrDefault(x => x.Name == "AvatarCharacterImage") is Image img)
            {
                return img;
            }
            else
            {
                return new Image();
            }
        }

        /// <summary>
        /// Aligns facing direction of avatar chatacter wrt provided gotoX.
        /// </summary>
        /// <param name="goToX"></param>
        /// <param name="canvas"></param>
        /// <param name="avatarId"></param>
        public void AlignAvatarCharacterDirectionWrtX(
            double goToX,
            Canvas canvas,
            int avatarId)
        {
            Button avatarButton = GetAvatarButtonFromCanvas(canvas: canvas, avatarId: avatarId) as Button;
            var avatar = avatarButton.Tag as Avatar;
            var nowX = avatar.Coordinate.X;

            AlignAvatarButtonWrtX(
                goToX: goToX,
                nowX: nowX,
                avatarButton: avatarButton);            
        }

        /// <summary>
        /// Aligns avatar button wrt provided gotoX.
        /// </summary>
        /// <param name="goToX"></param>
        /// <param name="nowX"></param>
        /// <param name="avatarButton"></param>
        public void AlignAvatarButtonWrtX(
            double goToX,
            double nowX,
            Button avatarButton)
        {
            // If going backward
            if (goToX < nowX)
            {
                GetAvatarCharacterImage(avatarButton).RenderTransform = new ScaleTransform() { ScaleX = -1 };

            }
            else // If going forward
            {
                GetAvatarCharacterImage(avatarButton).RenderTransform = new ScaleTransform() { ScaleX = 1 };
            }
        }


        /// <summary>
        /// Generates a new button from the provided avatar.
        /// </summary>
        /// <param name="avatar"></param>
        /// <returns></returns>
        public Button GenerateAvatarButton(Avatar avatar)
        {
            var imgUser = GetAvatarUserPictureFrame(avatar);

            var imgCharacter = new Image()
            {
                Source = new BitmapImage(new Uri(avatar.ImageUrl, UriKind.RelativeOrAbsolute)),
                Stretch = Stretch.Uniform,
                Height = 100,
                Width = 100,
                Name = "AvatarCharacterImage",
            };

            imgCharacter.RenderTransformOrigin = new Windows.Foundation.Point(0.5f, 0.5f);
            imgCharacter.RenderTransform = new ScaleTransform();

            Button btnAvatar = new Button()
            {
                Style = Application.Current.Resources["MaterialDesign_HyperlinkButton_Style"] as Style,
            };

            ToolTipService.SetToolTip(btnAvatar, avatar.Name);

            var spContent = new StackPanel();
            spContent.Children.Add(imgUser);
            spContent.Children.Add(imgCharacter);

            btnAvatar.Content = spContent;
            btnAvatar.Tag = avatar;

            return btnAvatar;
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

            return canvas.Children.OfType<Button>().Where(x => x.Tag is Avatar).FirstOrDefault(x => x.Tag is Avatar avatar && avatar.Id == avatarId);
        }

        #endregion
    }
}
