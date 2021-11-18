using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Shared;

namespace Worldescape
{
    public class AvatarButton : Button
    {
        public BitmapImage BitmapImage { get; set; } = new BitmapImage();

        public AvatarButton(Avatar avatar)
        {
            this.Style = Application.Current.Resources["MaterialDesign_HyperlinkButton_Style"] as Style;
            this.Tag = avatar;

            var uri = avatar.ImageUrl;

            BitmapImage = new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));

            var img = new Image()
            {
                Source = BitmapImage,
                Stretch = Stretch.Uniform,
                Height = 100,
                Width = 100,
            };

            //Effect = new DropShadowEffect() { ShadowDepth = 3, Color = Colors.Black, BlurRadius = 10, Opacity = 0.3 };

            this.Content = img;
        }

        /// <summary>
        /// Sets the provided activityStatus to the avatar.
        /// </summary>
        /// <param name="activityStatus"></param>
        public void SetAvatarActivityStatus(ActivityStatus activityStatus)
        {
            var avatar = this.Tag as Avatar;
            avatar.ActivityStatus = activityStatus;
            SetStatusBoundImageUrl(avatar, activityStatus);
        }

        /// <summary>
        /// Sets the StatusBoundImageUrl as content of the avatarButton according to the activityStatus.
        /// </summary>
        /// <param name="avatar"></param>
        /// <param name="activityStatus"></param>
        private void SetStatusBoundImageUrl(Avatar avatar, ActivityStatus activityStatus)
        {
            if (avatar.Character.StatusBoundImageUrls.FirstOrDefault(x => x.Status == activityStatus) is StatusBoundImageUrl statusBoundImageUrl)
            {
                BitmapImage.UriSource = new Uri(statusBoundImageUrl.ImageUrl);
            }
        }
    }
}
