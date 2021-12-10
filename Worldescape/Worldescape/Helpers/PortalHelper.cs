﻿using System;
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

        /// <summary>
        /// Generates a portal button with a rounded world image and the world's name below it.
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public Button GeneratePortalButton(World world)
        {
            var img = _worldHelper.GetWorldPicture(
                world: world,
                margin: new Thickness(5),
                size: 70);

            return GeneratePortalButton(
                world: world,
                fontSize: 15,
                img: img);
        }

        private Button GeneratePortalButton(
            World world,
            double fontSize,
            Border img)
        {
            var spContent = new StackPanel() { Margin = new Thickness(5) };

            spContent.Children.Add(img);
            spContent.Children.Add(new TextBlock()
            {
                FontSize = fontSize,
                FontWeight = FontWeights.SemiBold,
                TextAlignment = TextAlignment.Center,
                Text = world.Name,
                Foreground = new SolidColorBrush(Colors.White)
            });

            var buttonWorld = new Button()
            {
                Style = Application.Current.Resources["MaterialDesign_GlassButton_Style"] as Style,
                Tag = new Portal() { World = world },
            };

            buttonWorld.Content = spContent;
            return buttonWorld;
        }

        /// <summary>
        /// Adds the portal button to the provided canvas.
        /// </summary>
        /// <param name="portal"></param>
        /// <param name="canvas"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
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

            // Set opacity animation
            DoubleAnimation opacityAnimation = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMinutes(4)
            };

            // after opacity reaches zero delete this from canvas
            opacityAnimation.Completed += (s, e) =>
            {
                canvas.Children.Remove(portal);
            };

            Storyboard.SetTarget(opacityAnimation, portal);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Button.OpacityProperty));

            canvas.Children.Add(portal);

            var taggedPortal = ((Button)portal).Tag as Portal;

            taggedPortal.Coordinate.X = x;
            taggedPortal.Coordinate.Y = y;
            taggedPortal.Coordinate.Z = indexZ;

            Storyboard fadeStoryBoard = new Storyboard();
            fadeStoryBoard.Children.Add(opacityAnimation);
            fadeStoryBoard.Begin();

            return taggedPortal;
        }

        /// <summary>
        /// Center aligns the portal button wrt the provided PointerPoint.
        /// </summary>
        /// <param name="pressedPoint"></param>
        /// <param name="portalButton"></param>
        /// <param name="portal"></param>
        /// <param name="canvas"></param>
        /// <returns></returns>
        public Portal CenterAlignNewPortalButton(
            PointerPoint pressedPoint,
            Button portalButton,
            Portal portal,
            Canvas canvas)
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
