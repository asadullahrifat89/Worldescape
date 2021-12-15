using System;
using System.Numerics;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Worldescape.Common;

namespace Worldescape
{
    /// <summary>
    /// A helper class that provides functions for element transformation.
    /// </summary>
    public class ElementHelper
    {
        #region Fields

        bool _isPointerCaptured;
        double _pointerX;
        double _pointerY;
        double _objectLeft;
        double _objectTop;

        readonly AvatarHelper _avatarHelper;
        readonly EasingFunctionBase _constructEaseOut = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 5, };

        #endregion

        #region Ctor

        public ElementHelper(AvatarHelper avatarHelper)
        {
            _avatarHelper = avatarHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Normalizes the X position of the provided PointerPoint w.r.t X ScaleTransform factor of the provided Canvas.
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="pressedPoint"></param>
        /// <returns></returns>
        public double NormalizePointerX(
            Canvas canvas,
            PointerPoint pressedPoint)
        {
            var pointX = pressedPoint.Position.X;
            pointX = pointX / ((CompositeTransform)canvas.RenderTransform).ScaleX;
            return pointX;
        }

        /// <summary>
        /// Normalizes the Y position of the provided PointerPoint w.r.t Y ScaleTransform factor of the provided Canvas.
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="pressedPoint"></param>
        /// <returns></returns>
        public double NormalizePointerY(
            Canvas canvas,
            PointerPoint pressedPoint)
        {
            var pointY = pressedPoint.Position.Y;
            pointY = pointY / ((CompositeTransform)canvas.RenderTransform).ScaleY;
            return pointY;
        }

        /// <summary>
        /// Moves an UIElement to a new coordinate with the provided PointerRoutedEventArgs in canvas. Returns the tagged object of the uIElement.
        /// </summary>
        /// <param name="uIElement"></param>
        /// <param name="goToX"></param>
        /// <param name="goToY"></param>
        /// <returns></returns>
        public object MoveElement(
            Canvas canvas,
            UIElement uIElement,
            PointerRoutedEventArgs e)
        {
            var pressedPoint = e.GetCurrentPoint(canvas);

            var pointX = NormalizePointerX(canvas, pressedPoint);
            var pointY = NormalizePointerY(canvas, pressedPoint);

            var button = (Button)uIElement;

            var offsetX = button.ActualWidth / 2;
            // If the UIElement is Avatar then move it to an Y coordinate so that it appears on top of the clicked point, if it's a construct then move the construct to the middle point. 
            var offsetY = button.Tag is Avatar ? button.ActualHeight : button.ActualHeight / 2;

            var goToX = pointX - offsetX;
            var goToY = pointY - offsetY;

            var taggedObject = MoveElement(uIElement, goToX, goToY);

            return taggedObject;
        }

        /// <summary>
        /// Moves an UIElement to the provided goToX and goToY coordinate in canvas. Returns the tagged object of the uIElement.
        /// </summary>
        /// <param name="uIElement"></param>
        /// <param name="goToX"></param>
        /// <param name="goToY"></param>
        /// <returns></returns>
        public object MoveElement(
            UIElement uIElement,
            double goToX,
            double goToY,
            int? gotoZ = null,
            bool isCrafting = false)
        {
            if (uIElement == null)
                return null;

            var button = (Button)uIElement;

            var taggedObject = button.Tag;

            // Set moving status on start, if own avatar and if crafting mode is set then set crafting status
            if (taggedObject is Avatar avatar)
            {
                if (isCrafting && avatar.Id == App.User.Id)
                    _avatarHelper.SetAvatarActivityStatus(button, (Avatar)taggedObject, ActivityStatus.Crafting);
                else
                    _avatarHelper.SetAvatarActivityStatus(button, (Avatar)taggedObject, ActivityStatus.Moving);
            }

            var nowX = Canvas.GetLeft(uIElement);
            var nowY = Canvas.GetTop(uIElement);

            float distance = Vector3.Distance(
                new Vector3(
                    (float)nowX,
                    (float)nowY,
                    0),
                new Vector3(
                    (float)goToX,
                    (float)goToY,
                    0));

            float unitPixel = 200f;
            float timeToTravelunitPixel = 0.5f;

            float timeToTravelDistance = distance / unitPixel * timeToTravelunitPixel;

            if (timeToTravelDistance > 2)
                timeToTravelDistance = 2;

            Storyboard moveStory = new Storyboard();

            AnimationTimeline gotoXAnimation = null;
            AnimationTimeline gotoYAnimation = null;

            if (taggedObject is Avatar) // When avatar movement
            {
                //THEORY:
                // If already on higher ground Y
                //nowY=200  
                //                   goToY=400

                // If already on lower ground Y
                //                   goToY=200
                //nowY=400

                gotoXAnimation = new DoubleAnimation()
                {
                    From = nowX,
                    To = goToX,
                    Duration = new Duration(TimeSpan.FromSeconds(timeToTravelDistance)),
                    EasingFunction = _constructEaseOut,
                };

                _avatarHelper.AlignAvatarButtonWrtX(
                    goToX: goToX,
                    nowX: nowX,
                    avatarButton: button);

                var halfTime = timeToTravelDistance / 2;

                gotoYAnimation = new DoubleAnimationUsingKeyFrames();

                var gotoYAnimationKeyFrames = (DoubleAnimationUsingKeyFrames)gotoYAnimation;

                var easeOut = new ExponentialEase()
                {
                    EasingMode = EasingMode.EaseOut,
                    Exponent = 5,
                };

                var easeIn = new ExponentialEase()
                {
                    EasingMode = EasingMode.EaseIn,
                    Exponent = 5,
                };

                // Do half time animation Y
                if (nowY < goToY) // From higher ground to lower ground
                {
                    gotoYAnimationKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame()
                    {
                        KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(halfTime)),
                        Value = nowY - 100,
                        EasingFunction = easeOut,
                    });

                }
                else // From lower ground to higher ground
                {
                    var middleY = nowY - goToY;
                    gotoYAnimationKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame()
                    {
                        KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(halfTime)),
                        Value = goToY - 100,
                        EasingFunction = easeOut,
                    });
                }

                // To final animation Y
                gotoYAnimationKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(halfTime += halfTime)),
                    Value = goToY,
                    EasingFunction = easeIn,
                });

                Storyboard.SetTarget(gotoYAnimation, uIElement);
                Storyboard.SetTargetProperty(gotoYAnimation, new PropertyPath(Canvas.TopProperty));
                moveStory.Children.Add(gotoYAnimation);
            }
            else // When avatar movement
            {
                gotoXAnimation = new DoubleAnimation()
                {
                    From = nowX,
                    To = goToX,
                    Duration = new Duration(TimeSpan.FromMilliseconds(550)),
                    EasingFunction = _constructEaseOut,
                };

                gotoYAnimation = new DoubleAnimation()
                {
                    From = nowY,
                    To = goToY,
                    Duration = new Duration(TimeSpan.FromMilliseconds(550)),
                    EasingFunction = _constructEaseOut,
                };
            }

            gotoYAnimation.Completed += (object sender, EventArgs e) =>
            {
                if (taggedObject is Avatar taggedAvatar)
                {
                    if (isCrafting && taggedAvatar.Id == App.User.Id)
                        _avatarHelper.SetAvatarActivityStatus(button, (Avatar)taggedObject, ActivityStatus.Crafting);
                    else
                        _avatarHelper.SetAvatarActivityStatus(button, (Avatar)taggedObject, ActivityStatus.Idle);
                }
            };

            Storyboard.SetTarget(gotoXAnimation, uIElement);
            Storyboard.SetTargetProperty(gotoXAnimation, new PropertyPath(Canvas.LeftProperty));

            Storyboard.SetTarget(gotoYAnimation, uIElement);
            Storyboard.SetTargetProperty(gotoYAnimation, new PropertyPath(Canvas.TopProperty));

            moveStory.Children.Add(gotoXAnimation);
            moveStory.Children.Add(gotoYAnimation);

            moveStory.Begin();

            if (taggedObject is Construct)
            {
                var taggedConstruct = taggedObject as Construct;

                taggedConstruct.Coordinate.X = goToX;
                taggedConstruct.Coordinate.Y = goToY;

                if (gotoZ.HasValue)
                {
                    taggedConstruct.Coordinate.Z = (int)gotoZ;
                    Canvas.SetZIndex(uIElement, (int)gotoZ);
                }
                else
                {
                    taggedConstruct.Coordinate.Z = Canvas.GetZIndex(uIElement);
                }

                taggedObject = taggedConstruct;
            }
            else if (button.Tag is Avatar)
            {
                var taggedAvatar = taggedObject as Avatar;

                taggedAvatar.Coordinate.X = goToX;
                taggedAvatar.Coordinate.Y = goToY;

                if (gotoZ.HasValue)
                {
                    taggedAvatar.Coordinate.Z = (int)gotoZ;
                    Canvas.SetZIndex(uIElement, (int)gotoZ);
                }
                else
                {
                    taggedAvatar.Coordinate.Z = Canvas.GetZIndex(uIElement);
                }

                taggedObject = taggedAvatar;
            }

            return taggedObject;
        }

        /// <summary>
        /// Scales an UIElement to the provided scale. Returns the tagged object of the uIElement.
        /// </summary>
        /// <param name="uIElement"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public object ScaleElement(
            UIElement uIElement,
            float scale)
        {
            uIElement.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);

            CompositeTransform composite = null;

            // If an exiting transformation already exits
            if (uIElement.RenderTransform != null && uIElement.RenderTransform is CompositeTransform cc)
                composite = cc;

            var scaleTransform = new CompositeTransform()
            {
                ScaleX = scale,
                ScaleY = scale,
                Rotation = composite?.Rotation ?? 0,
            };

            uIElement.RenderTransform = scaleTransform;

            if (uIElement is Button button && button.Tag is Construct construct)
            {
                construct.Scale = scale;
                return construct;
            }
            else
                return null;
        }

        /// <summary>
        /// Rotates an UIElement to the provided rotation. Returns the tagged object of the uIElement.
        /// </summary>
        /// <param name="uIElement"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public object RotateElement(
            UIElement uIElement,
            float rotation)
        {
            uIElement.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);

            CompositeTransform composite = null;

            // If an exiting transformation already exits
            if (uIElement.RenderTransform != null && uIElement.RenderTransform is CompositeTransform cc)
                composite = cc;

            var scaleTransform = new CompositeTransform()
            {
                ScaleX = composite?.ScaleX ?? 1,
                ScaleY = composite?.ScaleY ?? 1,
                Rotation = rotation,
            };

            uIElement.RenderTransform = scaleTransform;

            if (uIElement is Button button && button.Tag is Construct construct)
            {
                construct.Rotation = rotation;
                return construct;
            }
            else
                return null;
        }

        /// <summary>
        /// Starts dragging an UIElement.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="uielement"></param>
        public void DragStart(
            Canvas canvas,
            PointerRoutedEventArgs e,
            UIElement uielement)
        {
            // Drag start of a constuct
            _objectLeft = Canvas.GetLeft(uielement);
            _objectTop = Canvas.GetTop(uielement);

            var currentPoint = e.GetCurrentPoint(canvas);

            // Remember the pointer position:
            _pointerX = currentPoint.Position.X;
            _pointerY = currentPoint.Position.Y;

            uielement.CapturePointer(e.Pointer);

            _isPointerCaptured = true;
        }

        /// <summary>
        /// Drags an UIElement.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="uielement"></param>
        public void DragElement(
            Canvas canvas,
            PointerRoutedEventArgs e,
            UIElement uielement)
        {
            if (_isPointerCaptured)
            {
                var currentPoint = e.GetCurrentPoint(canvas);

                // Calculate the new position of the object:
                double deltaH = currentPoint.Position.X - _pointerX;
                double deltaV = currentPoint.Position.Y - _pointerY;

                _objectLeft = deltaH + _objectLeft;
                _objectTop = deltaV + _objectTop;

                // Update the object position:
                Canvas.SetLeft(uielement, _objectLeft);
                Canvas.SetTop(uielement, _objectTop);

                // Remember the pointer position:
                _pointerX = currentPoint.Position.X;
                _pointerY = currentPoint.Position.Y;
            }
        }

        /// <summary>
        /// Stops dragging an UIElement.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="uielement"></param>
        public void DragRelease(
            PointerRoutedEventArgs e,
            UIElement uielement)
        {
            _isPointerCaptured = false;
            uielement.ReleasePointerCapture(e.Pointer);
        }

        #endregion
    }
}
