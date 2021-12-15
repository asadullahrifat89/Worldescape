using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Common;

namespace Worldescape
{
    public class WorldHelper
    {
        readonly double[] _cloudScales = { 1, 1.25, 1.35, 0.75, 0.50 };

        public WorldHelper()
        {

        }

        #region UI

        public Button GenerateWorldButton(
            World world,
            double size,
            double fontSize = 14)
        {
            var img = GetWorldPictureFrame(
                world: world,
                margin: new Thickness(5),
                size: size);

            return GenerateWorldButton(
                world: world,
                size: size,
                fontSize: fontSize,
                img: img);
        }

        public Button GenerateWorldButton(
            World world,
            double size,
            Thickness imageMargin,
            double fontSize = 14)
        {
            var img = GetWorldPictureFrame(
                world: world,
                margin: imageMargin,
                size: size);

            return GenerateWorldButton(
                world: world,
                size: size,
                fontSize: fontSize,
                img: img);
        }

        private Button GenerateWorldButton(
            World world,
            double size,
            double fontSize,
            Border img)
        {
            var spContent = new StackPanel();
            spContent.Children.Add(img);

            var spText = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            spText.Children.Add(new TextBlock()
            {
                FontSize = fontSize,
                FontWeight = FontWeights.SemiBold,
                TextAlignment = TextAlignment.Left,
                Text = world.Name,
                Margin = new Thickness(5),
            });

            if (world.PopulationCount > 0)
            {
                var format = world.PopulationCount >= 1000 ? "{0:#,0, K}" : "{0:#,0}";
                string populationCount = string.Format(format, world.PopulationCount.ToString());

                spText.Children.Add(new TextBlock()
                {
                    FontSize = fontSize,
                    Foreground = new SolidColorBrush(Color.FromRgb(99, 89, 148)),
                    FontWeight = FontWeights.Normal,
                    TextAlignment = TextAlignment.Left,
                    Text = "👣",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(1),
                });

                spText.Children.Add(new TextBlock()
                {
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.FromRgb(99, 89, 148)),
                    FontWeight = FontWeights.SemiBold,
                    TextAlignment = TextAlignment.Left,
                    Text = populationCount,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(1),
                });
            }

            spContent.Children.Add(spText);

            var buttonWorld = new Button()
            {
                Style = Application.Current.Resources["MaterialDesign_GlassButton_Style"] as Style,
                Height = size,
                Width = size,
                Margin = new Thickness(5),
                Tag = world,
            };

            buttonWorld.Content = spContent;
            return buttonWorld;
        }

        /// <summary>
        /// Gets the world image in a circular border from the provided world.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public Border GetWorldPicture(World world, double size = 40)
        {
            var bitmapImage = new BitmapImage(new Uri(world.ImageUrl));
            return PrepareRoundImage(size, bitmapImage);
        }

        /// <summary>
        ///  Gets the world image in a circular border from the provided world with the margin.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="margin"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public Border GetWorldPictureFrame(World world, Thickness margin, double size = 40)
        {
            var bitmapImage = new BitmapImage(new Uri(world.ImageUrl));
            var border = PrepareRoundImage(size, bitmapImage);
            border.Margin = margin;
            return border;
        }

        /// <summary>
        /// Prepares a rounded border to contain the provided bitmap image.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="bitmapImage"></param>
        /// <returns></returns>
        private Border PrepareRoundImage(double size, BitmapImage bitmapImage)
        {
            var imageBorder = new Border()
            {
                Height = size,
                Width = size,
                CornerRadius = new CornerRadius(40),
                ClipToBounds = true,
                Background = new SolidColorBrush(Colors.Transparent),
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
        /// Adds random clouds to the provided Canvas. If drawOver = true then adds the clouds on top of existing canvas elements.
        /// </summary>
        /// <param name="drawOver"></param>
        public async Task PopulateClouds(Canvas canvas, bool drawOver = false)
        {
            double canvasSize = Constants.Canvas_Size;

            for (int i = 0; i < 17; i++)
            {
                var cloudImage = $"ms-appx:///Assets/Images/Defaults/cloud-{new Random().Next(minValue: 0, maxValue: 2)}.png";

                var bitmap = new BitmapImage(new Uri(cloudImage, UriKind.RelativeOrAbsolute));

                var img = new Image() { Source = bitmap, Stretch = Stretch.None };

                Canvas.SetTop(img, new Random().Next(0, (int)canvasSize));

                var cloudScale = _cloudScales[new Random().Next(0, _cloudScales.Count())];

                if (drawOver)
                {
                    Canvas.SetZIndex(img, 999);
                    img.RenderTransform = new ScaleTransform() { ScaleX = -1 * cloudScale, ScaleY = 1 * cloudScale };
                }
                else
                {
                    img.RenderTransform = new ScaleTransform() { ScaleX = 1 * cloudScale, ScaleY = 1 * cloudScale };
                }

                float distance = (float)canvasSize - 300;
                float unitPixel = 200f;
                float timeToTravelunitPixel = new Random().Next(minValue: 1, maxValue: 5);

                float timeToTravelDistance = distance / unitPixel * timeToTravelunitPixel;

                var gotoXAnimation = new DoubleAnimation()
                {
                    From = 0,
                    To = distance,
                    Duration = new Duration(TimeSpan.FromSeconds(timeToTravelDistance)),
                    RepeatBehavior = RepeatBehavior.Forever,
                };

                Storyboard.SetTarget(gotoXAnimation, img);
                Storyboard.SetTargetProperty(gotoXAnimation, new PropertyPath(Canvas.LeftProperty));

                Storyboard moveStory = new Storyboard();
                moveStory.Children.Add(gotoXAnimation);

                canvas.Children.Add(img);

                moveStory.Begin();

                await Task.Delay(1000);
            }
        }

        #endregion
    }
}
