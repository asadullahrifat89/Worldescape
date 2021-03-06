using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Worldescape.Common;
using Worldescape.Service;

namespace Worldescape
{
    public partial class WorldCreationWindow : ChildWindow
    {
        readonly World _world;
        readonly Action<World> _wordSaved;        
        readonly WorldRepository _worldRepository;

        public WorldCreationWindow(
            Action<World> worldSaved,
            World world = null)
        {
            InitializeComponent();
            _wordSaved = worldSaved;
            _world = world ?? new World();
            WorldNameHolder.Text = _world.Name;

            Title = _world.IsEmpty() ? "Create world" : "Update world";

            _worldRepository = App.ServiceProvider.GetService(typeof(WorldRepository)) as WorldRepository;
        }

        private async void Button_OK_Click(object sender, RoutedEventArgs e)
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
            App.SetIsBusy(true, "Saving your world...");

            var response = await _worldRepository.UpdateWorld(
               token: App.Token,
               name: WorldNameHolder.Text,
               id: _world.Id);

            if (!response.Success)
            {
                var contentDialogue = new MessageDialogueWindow(title: "Failed!", message: response.Error);
                contentDialogue.Show();

                App.SetIsBusy(false);
            }
            else
            {
                var world = response.Result;

                if (world != null && world.Id > 0)
                {
                    App.SetIsBusy(false);

                    _wordSaved?.Invoke(world);
                    this.DialogResult = true;
                }
            }
        }

        private async Task AddWorld()
        {
            App.SetIsBusy(true, "Creating your world...");

            var defaultWorldImageUrl = $"ms-appx:///Assets/Images/Defaults/World_{new Random().Next(0, 9)}.png";

            var response = await _worldRepository.AddWorld(
                token: App.Token,
                name: WorldNameHolder.Text,
                imageUrl: defaultWorldImageUrl);

            if (!response.Success)
            {
                var contentDialogue = new MessageDialogueWindow(title: "Failed!", message: response.Error);
                contentDialogue.Show();

                App.SetIsBusy(false);
            }
            else
            {
                var world = response.Result;

                if (world != null && world.Id > 0)
                {
                    App.SetIsBusy(false);

                    _wordSaved?.Invoke(world);
                    this.DialogResult = true;
                }
            }
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

