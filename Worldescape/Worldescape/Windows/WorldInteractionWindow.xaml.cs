using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Worldescape.Common;

namespace Worldescape
{
    public partial class WorldInteractionWindow : ChildWindow
    {
        readonly Action<bool> _result;
        readonly WorldHelper _worldHelper;

        public WorldInteractionWindow(
            World world,
            string title = null,
            Action<bool> result = null)
        {
            InitializeComponent();

            Title = title ?? world.Name;
            _result = result;
            _worldHelper = App.ServiceProvider.GetService(typeof(WorldHelper)) as WorldHelper;

            ContentControl_World.Content = _worldHelper.GenerateWorldButtonContent(
                world: world,
                fontSize: 15,
                pictureFrame: _worldHelper.GetWorldPictureFrame(
                    world: world,
                    margin: new Thickness(5),
                    size: 180));

        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            _result?.Invoke(true);
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _result!.Invoke(false);
            this.DialogResult = false;
        }
    }
}

