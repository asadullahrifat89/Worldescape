using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
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
using Worldescape.Data;
using Image = Windows.UI.Xaml.Controls.Image;

namespace Worldescape
{
    public class ConstructHelper
    {
        /// <summary>
        /// Performs opacity animation on the provided construct. Returns the callback onCompleted upon animation completion.
        /// </summary>
        /// <param name="construct"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="onCompleted"></param>
        public void PerformOpacityAnimationOnConstruct(
            UIElement construct,
            double from,
            double to,
            Action onCompleted = null)
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(500),
            };

            opacityAnimation.Completed += (s, e) =>
            {
                onCompleted?.Invoke();
            };

            Storyboard.SetTarget(opacityAnimation, construct);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));

            Storyboard fadeStoryBoard = new Storyboard();
            fadeStoryBoard.Children.Add(opacityAnimation);

            fadeStoryBoard.Begin();
        }

        /// <summary>
        /// Adds a construct to the canvas.
        /// </summary>
        /// <param name="construct"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Construct AddConstructOnCanvas(
            UIElement construct,
            Canvas canvas,
            double x,
            double y,
            int? z = null)
        {
            Canvas.SetLeft(construct, x);
            Canvas.SetTop(construct, y);

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
                    if (canvas.Children.Any(x => x is Button button && button.Tag is Construct))
                    {
                        var lastConstruct = ((Button)canvas.Children.Where(x => x is Button button && button.Tag is Construct).LastOrDefault()).Tag as Construct;

                        if (lastConstruct != null)
                        {
                            indexZ = lastConstruct.Coordinate.Z + 1;
                        }
                    }
                }
            }

            Canvas.SetZIndex(construct, indexZ);

            canvas.Children.Add(construct);

            var taggedConstruct = ((Button)construct).Tag as Construct;

            taggedConstruct.Coordinate.X = x;
            taggedConstruct.Coordinate.Y = y;
            taggedConstruct.Coordinate.Z = indexZ;

            PerformOpacityAnimationOnConstruct(construct, 0, 1); // Add

            return taggedConstruct;
        }

        /// <summary>
        /// Generate a new button from the provided construct. If constructId is not provided then new id is generated. If inWorld, creator are not provided then current world and user are tagged.
        /// </summary>
        /// <param name="construct"></param>
        /// <param name="constructId"></param>
        /// <returns></returns>
        public Button GenerateConstructButton(
            string name,
            string imageUrl,
            int? constructId = null,
            InWorld inWorld = null,
            Creator creator = null)
        {
            var uri = imageUrl;

            var bitmap = new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));

            var img = new Image() { Source = bitmap, Stretch = Stretch.None };

            // Id is broadcasted
            var id = constructId ?? UidGenerator.New();

            if (id <= 0)
            {
                throw new InvalidOperationException("Id can not be less than or equal to zero.");
            }

            var obj = new Button()
            {
                BorderBrush = new SolidColorBrush(Colors.DodgerBlue),
                Style = Application.Current.Resources["MaterialDesign_GlassButton_Style"] as Style,
                Name = id.ToString(),
                Tag = new Construct()
                {
                    Id = id,
                    Name = name,
                    ImageUrl = uri,
                    Creator = creator ?? new Creator() { Id = App.User.Id, Name = App.User.Name, ImageUrl = App.User.ImageUrl },
                    World = inWorld ?? new InWorld() { Id = App.InWorld.Id, Name = App.InWorld.Name }
                }
            };

            obj.Content = img;

            //obj.AllowScrollOnTouchMove = false;            

            return obj;
        }

        /// <summary>
        /// Removes the provided construct from canvas.
        /// </summary>
        /// <param name="construct"></param>
        public void RemoveConstructFromCanvas(UIElement construct, Canvas canvas)
        {
            PerformOpacityAnimationOnConstruct(construct, 1, 0, () => // Remove
            {
                canvas.Children.Remove(construct);

                Console.WriteLine("Construct removed.");
            });
        }
    }
}
