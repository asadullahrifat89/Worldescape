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
using Worldescape.Service;

namespace Worldescape
{
    public class PortalHelper
    {
        readonly WorldHelper _worldHelper;
        public PortalHelper(WorldHelper worldHelper)
        {
            _worldHelper = worldHelper;
        }

        public Button GeneratePortalButton(World world, double size, double fontSize = 14)
        {
            var img = _worldHelper.GetWorldPicture(
                world: world,
                margin: new Thickness(5),
                size: size);

            return GeneratePortalButton(
                world: world,
                size: size,
                fontSize: fontSize,
                img: img);
        }

        public Button GeneratePortalButton(World world, double size, Thickness imageMargin, double fontSize = 14)
        {
            var img = _worldHelper.GetWorldPicture(
                world: world,
                margin: imageMargin,
                size: size);

            return GeneratePortalButton(
                world: world,
                size: size,
                fontSize: fontSize,
                img: img);
        }

        private Button GeneratePortalButton(World world, double size, double fontSize, Border img)
        {
            var spContent = new StackPanel();
            spContent.Children.Add(img);
            spContent.Children.Add(new TextBlock()
            {
                FontSize = fontSize,
                FontWeight = FontWeights.SemiBold,
                TextAlignment = TextAlignment.Center,
                Text = world.Name,
                Margin = new Thickness(5),
            });

            var buttonWorld = new Button()
            {
                Style = Application.Current.Resources["MaterialDesign_Button_Style"] as Style,
                Height = size,
                Width = size,
                Margin = new Thickness(5),
                Tag = world,
            };

            buttonWorld.Content = spContent;
            return buttonWorld;
        }

        public Portal AddPortalOnCanvas(
        UIElement world,
        Canvas canvas,
        double x,
        double y,
        int? z = null,
        bool disableOpacityAnimation = false)
        {
            Canvas.SetLeft(world, x);
            Canvas.SetTop(world, y);

            int indexZ = 9;

            if (z.HasValue)
            {
                indexZ = z.Value;
            }
            else
            {
                // If Z index is not proved then assign max Z index to this construct button
                if (canvas.Children != null && canvas.Children.Any())
                {
                    var children = canvas.Children.OfType<Button>();

                    if (children != null && children.Any(x => x.Tag is Portal))
                    {
                        var maxZ = children.Where(x => x.Tag is Portal).Select(z => (Portal)z.Tag).Max(x => x.Coordinate.Z);
                        indexZ = maxZ + 1;
                    }
                }
            }

            Canvas.SetZIndex(world, indexZ);

            canvas.Children.Add(world);

            var taggedPortal = ((Button)world).Tag as Portal;

            taggedPortal.Coordinate.X = x;
            taggedPortal.Coordinate.Y = y;
            taggedPortal.Coordinate.Z = indexZ;

            //if (!disableOpacityAnimation)
            //    PerformOpacityAnimationOnConstruct(world, 0, 1); // Add

            return taggedPortal;
        }
    }
}
