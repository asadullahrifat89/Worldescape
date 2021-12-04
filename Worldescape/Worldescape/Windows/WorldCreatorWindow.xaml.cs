﻿using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Worldescape.Common;
using Worldescape.Service;

namespace Worldescape
{
    public partial class WorldCreatorWindow : ChildWindow
    {
        Action<World> _wordSaved;
        World _world;

        readonly MainPage _mainPage;
        readonly HttpServiceHelper _httpServiceHelper;
        readonly WorldRepository _worldRepository;

        public WorldCreatorWindow(
            Action<World> worldSaved,
            World world = null)
        {
            InitializeComponent();
            _wordSaved = worldSaved;
            _world = world ?? new World();
            WorldNameHolder.Text = _world.Name;

            Title = _world.IsEmpty() ? "Create world" : "Update world";

            _httpServiceHelper = App.ServiceProvider.GetService(typeof(HttpServiceHelper)) as HttpServiceHelper;
            _worldRepository = App.ServiceProvider.GetService( typeof(WorldRepository)) as WorldRepository;
            _mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;
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

                _wordSaved?.Invoke(world);
                this.DialogResult = true;
            }
            else
            {
                var contentDialogue = new ContentDialogueWindow(title: "Failed!", message: "Failed to save your world. This shouldn't be happening. Try again.");
                contentDialogue.Show();

                _mainPage.SetIsBusy(false);
            }
        }

        private async Task AddWorld()
        {
            _mainPage.SetIsBusy(true, "Creating your world...");

            var defaultWorldImageUrl = $"ms-appx:///Images/Defaults/World_{new Random().Next(0, 9)}.png";

            var response = await _worldRepository.AddWorld(
                token: App.Token,
                name: WorldNameHolder.Text,
                imageUrl: defaultWorldImageUrl);

            if (!response.Success) 
            {
                var contentDialogue = new ContentDialogueWindow(title: "Failed!", message: response.Error);
                contentDialogue.Show();

                _mainPage.SetIsBusy(false);
            }
            else
            {
                var world = response.Result as World;

                if (world != null && world.Id > 0)
                {
                    _mainPage.SetIsBusy(false);

                    _wordSaved?.Invoke(world);
                    this.DialogResult = true;
                }
            }

            //var command = new AddWorldCommandRequest
            //{
            //    Token = App.Token,
            //    Name = WorldNameHolder.Text,
            //    ImageUrl = defaultWorldImageUrl
            //};

            //var world = await _httpServiceHelper.SendPostRequest<World>(
            //   actionUri: Constants.Action_AddWorld,
            //   payload: command);

            //if (world != null && world.Id > 0)
            //{
            //    _mainPage.SetIsBusy(false);

            //    _wordSaved?.Invoke(world);
            //    this.DialogResult = true;

            //}
            //else
            //{
            //    var contentDialogue = new ContentDialogueWindow(title: "Failed!", message: "Failed to create your world. This shouldn't be happening. Try again.");
            //    contentDialogue.Show();

            //    _mainPage.SetIsBusy(false);
            //}
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

