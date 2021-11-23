using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Worldescape.Data;
using Worldescape.Service;

namespace Worldescape
{
    public partial class WorldCreatorWindow : ChildWindow
    {
        Action<World> _wordSaved;
        World _world;

        readonly MainPage _mainPage;
        readonly HttpServiceHelper _httpServiceHelper;

        public WorldCreatorWindow(Action<World> worldSaved, World world = null)
        {
            InitializeComponent();
            _wordSaved = worldSaved;
            _world = world ?? new World();
            WorldNameHolder.Text = _world.Name;

            Title = _world.IsEmpty() ? "Create world" : "Update world";

            _httpServiceHelper = App.ServiceProvider.GetService(typeof(HttpServiceHelper)) as HttpServiceHelper;
            _mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;
        }

        private async void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (!WorldNameHolder.Text.IsNullOrBlank())
            {                
                if (_world.IsEmpty())
                {
                    await AddWorld();
                }
                else
                {
                    await UpdateWorld();
                }
            }
        }

        private async Task UpdateWorld()
        {
            _mainPage.SetIsBusy(true, "Saving your world...");

            var command = new UpdateWorldCommandRequest
            {
                Token = App.Token,
                Name = WorldNameHolder.Text,
                Id = _world.Id,
            };

            var world = await _httpServiceHelper.SendPostRequest<World>(
               actionUri: Constants.Action_UpdateWorld,
               payload: command);

            if (world != null && world.Id > 0)
            {
                _mainPage.SetIsBusy(false);

                _wordSaved?.Invoke(_world);
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Failed to save your world. This shouldn't be happening.", "Failed..");
                _mainPage.SetIsBusy(false);
            }
        }

        private async Task AddWorld()
        {
            _mainPage.SetIsBusy(true, "Creating your world...");

            var command = new AddWorldCommandRequest
            {
                Token = App.Token,
                Name = WorldNameHolder.Text,
            };

            var world = await _httpServiceHelper.SendPostRequest<World>(
               actionUri: Constants.Action_AddWorld,
               payload: command);

            if (world != null && world.Id > 0)
            {
                _mainPage.SetIsBusy(false);

                _wordSaved?.Invoke(_world);
                this.DialogResult = true;

            }
            else
            {
                MessageBox.Show("Failed to save your world. This shouldn't be happening.", "Failed..");
                _mainPage.SetIsBusy(false);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

