using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Input;
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
        readonly ElementHelper _elementHelper;

        public PortalHelper(
            WorldHelper worldHelper,
            ElementHelper elementHelper)
        {
            _worldHelper = worldHelper;
            _elementHelper = elementHelper;
        }

        public Button GeneratePortalButton(World world)
        {
            var img = _worldHelper.GetWorldPicture(
                world: world,
                margin: new Thickness(5),
                size: 70);

            return GeneratePortalButton(
                world: world,
                size: 70,
                fontSize: 14,
                img: img);
        }

        private Button GeneratePortalButton(
            World world,
            double size,
            double fontSize,
            Border img)
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
                Style = Application.Current.Resources["MaterialDesign_GlassButton_Style"] as Style,
                Height = size,
                Width = size,
                Tag = new Portal() { World = world },
            };

            buttonWorld.Content = spContent;
            return buttonWorld;
        }

        public Portal AddPortalOnCanvas(
            UIElement portal,
            Canvas canvas,
            double x,
            double y,
            int? z = null)
        {
            Canvas.SetLeft(portal, x);
            Canvas.SetTop(portal, y);

            int indexZ = 9;

            if (z.HasValue)
            {
                indexZ = z.Value;
            }
            else
            {
                // If Z index is not proved then assign max Z index to this portal button
                if (canvas.Children != null && canvas.Children.Any())
                {
                    var children = canvas.Children.OfType<Button>();

                    if (children != null && children.Any(x => x.Tag is Construct))
                    {
                        var maxZ = children.Where(x => x.Tag is Construct).Select(z => (Construct)z.Tag).Max(x => x.Coordinate.Z);
                        indexZ = maxZ + 1;
                    }
                }
            }

            Canvas.SetZIndex(portal, indexZ);

            canvas.Children.Add(portal);

            var taggedPortal = ((Button)portal).Tag as Portal;

            taggedPortal.Coordinate.X = x;
            taggedPortal.Coordinate.Y = y;
            taggedPortal.Coordinate.Z = indexZ;

            //TODO: remove portal after one min

            return taggedPortal;
        }

        public Portal CenterAlignNewPortalButton(PointerPoint pressedPoint, Button portalButton, Portal portal, Canvas canvas)
        {
            var offsetX = portalButton.ActualWidth / 2;
            var offsetY = portalButton.ActualHeight / 2;

            var pointX = _elementHelper.NormalizePointerX(canvas, pressedPoint);
            var pointY = _elementHelper.NormalizePointerY(canvas, pressedPoint);

            var goToX = pointX - offsetX;
            var goToY = pointY - offsetY;

            Canvas.SetLeft(portalButton, goToX);
            Canvas.SetTop(portalButton, goToY);

            portal.Coordinate.X = goToX;
            portal.Coordinate.Y = goToY;

            return portal;
        }
    }
}
