using System;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Worldescape.Common;
using Image = Windows.UI.Xaml.Controls.Image;

namespace Worldescape
{
    public class ConstructHelper
    {
        readonly ElementHelper _elementHelper;

        public ConstructHelper(ElementHelper elementHelper)
        {
            _elementHelper = elementHelper;
        }

        #region UI

        /// <summary>
        /// Return if the current app user can manipulate the provided construct.
        /// </summary>
        /// <param name="uIElement"></param>
        /// <returns></returns>
        public bool CanManipulateConstruct(UIElement uIElement)
        {
            if (uIElement != null && ((Button)uIElement).Tag is Construct c && c.Creator.Id == App.User.Id)
                return true;
            else
                return false;
        }

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
            int? z = null,
            bool disableOpacityAnimation = false)
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
                        var lastConstruct = canvas.Children.OfType<Button>().Where(x => x.Tag is Construct c).LastOrDefault().Tag as Construct;

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

            if (!disableOpacityAnimation)
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
            Creator creator = null,
            DateTime? createdOn = null)
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
                    World = inWorld ?? new InWorld() { Id = App.World.Id, Name = App.World.Name },
                    CreatedOn = createdOn ?? DateTime.Now,
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
        public void RemoveConstructFromCanvas(
            UIElement construct,
            Canvas canvas)
        {
            PerformOpacityAnimationOnConstruct(construct, 1, 0, () => // Remove
            {
                canvas.Children.Remove(construct);

                Console.WriteLine("Construct removed.");
            });
        }

        /// <summary>
        /// Align the provided construct button to the center of the provided point.
        /// </summary>
        /// <param name="pressedPoint"></param>
        /// <param name="constructButton"></param>
        /// <param name="construct"></param>
        /// <returns></returns>
        public Construct CenterAlignNewConstructButton(
            Windows.UI.Input.PointerPoint pressedPoint,
            Button constructButton,
            Construct construct,
            Canvas canvas)
        {
            var offsetX = constructButton.ActualWidth / 2;
            var offsetY = constructButton.ActualHeight / 2;

            var pointX = _elementHelper.NormalizePointerX(canvas, pressedPoint);
            var pointY = _elementHelper.NormalizePointerY(canvas, pressedPoint);

            var goToX = pointX - offsetX;
            var goToY = pointY - offsetY;

            Canvas.SetLeft(constructButton, goToX);
            Canvas.SetTop(constructButton, goToY);

            construct.Coordinate.X = goToX;
            construct.Coordinate.Y = goToY;

            return construct;
        }

        /// <summary>
        /// Returns the construct button from the provided canvas and constructId.
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="constructId"></param>
        /// <returns></returns>
        public UIElement GetConstructButtonFromCanvas(
            Canvas canvas,
            int constructId)
        {
            if (canvas == null)
                return null;
            if (constructId <= 0)
                return null;

            return canvas.Children.OfType<Button>().Where(x => x.Tag is Construct c && c.Id == constructId).FirstOrDefault();
        }

        #endregion
    }
}
