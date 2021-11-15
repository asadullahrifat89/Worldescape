using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Worldescape.Contracts.Services;
using Worldescape.Shared.Entities;
using Worldescape.Shared.Models;
using Worldescape.Shared.Requests;

namespace Worldescape.Services
{
    public class WorldescapeHubService : IWorldescapeHubService
    {
        #region Fields

        private HubConnection connection;

        // Connection
        public event Action<int> AvatarDisconnected;
        public event Action<int> AvatarReconnected;
        public event Action ConnectionReconnecting;
        public event Action ConnectionReconnected;
        public event Action ConnectionClosed;

        // Session        
        public event Action<Avatar> AvatarLoggedIn;
        public event Action<int> AvatarLoggedOut;

        // Texting
        public event Action<int, string, MessageType> NewTextMessage;
        public event Action<int, byte[], MessageType> NewImageMessage;
        public event Action<int, MessageType> AvatarTyping;

        // Avatar
        public event Action<int, double, double, int> NewBroadcastAvatarMovement;
        public event Action<int, int> NewBroadcastAvatarActivityStatus;

        // Construct
        public event Action<Construct> NewBroadcastConstruct;
        public event Action<Construct[]> NewBroadcastConstructs;
        public event Action<int> NewRemoveConstruct;
        public event Action<int[]> NewRemoveConstructs;
        public event Action<int, int> NewBroadcastConstructPlacement;
        public event Action<int, float> NewBroadcastConstructRotation;
        public event Action<ConcurrentDictionary<int, float>> NewBroadcastConstructRotations;
        public event Action<int, float> NewBroadcastConstructScale;
        public event Action<int[], float> NewBroadcastConstructScales;
        public event Action<int, double, double, int> NewBroadcastConstructMovement;

        #endregion

        #region Ctor

        public WorldescapeHubService()
        {
#if DEBUG
            var url = Properties.Resources.DevHubService;
#else
            var url = Properties.Resources.ProdHubService;
#endif
            connection = new HubConnectionBuilder().WithUrl(url).WithAutomaticReconnect().Build();

            connection.On<Avatar>("AvatarLogin", (u) => AvatarLoggedIn?.Invoke(u));
            connection.On<int>("AvatarLogout", (n) => AvatarLoggedOut?.Invoke(n));
            connection.On<int>("AvatarDisconnection", (n) => AvatarDisconnected?.Invoke(n));
            connection.On<int>("AvatarReconnection", (n) => AvatarReconnected?.Invoke(n));
            connection.On<int, string>("BroadcastTextMessage", (n, m) => NewTextMessage?.Invoke(n, m, MessageType.Broadcast));
            connection.On<int, byte[]>("BroadcastPictureMessage", (n, m) => NewImageMessage?.Invoke(n, m, MessageType.Broadcast));
            connection.On<int, string>("UnicastTextMessage", (n, m) => NewTextMessage?.Invoke(n, m, MessageType.Unicast));
            connection.On<int, byte[]>("UnicastPictureMessage", (n, m) => NewImageMessage?.Invoke(n, m, MessageType.Unicast));
            connection.On<int>("AvatarTyping", (p) => AvatarTyping?.Invoke(p, MessageType.Unicast));
            connection.On<int>("AvatarBroadcastTyping", (p) => AvatarTyping?.Invoke(p, MessageType.Broadcast));

            // Avatar
            connection.On<int, double, double, int>("BroadcastAvatarMovement", (avatarId, x, y, z) => NewBroadcastAvatarMovement?.Invoke(avatarId, x, y, z));
            connection.On<int, int>("BroadcastAvatarActivityStatus", (avatarId, activityStatus) => NewBroadcastAvatarActivityStatus?.Invoke(avatarId, activityStatus));

            // construct
            connection.On<Construct>("BroadcastConstruct", (construct) => NewBroadcastConstruct?.Invoke(construct));
            connection.On<Construct[]>("BroadcastConstructs", (constructs) => NewBroadcastConstructs?.Invoke(constructs));
            connection.On<int>("RemoveConstruct", (constructId) => NewRemoveConstruct?.Invoke(constructId));
            connection.On<int[]>("RemoveConstructs", (constructIds) => NewRemoveConstructs?.Invoke(constructIds));
            connection.On<int, int>("BroadcastConstructPlacement", (constructId, z) => NewBroadcastConstructPlacement?.Invoke(constructId, z));
            connection.On<int, float>("BroadcastConstructRotation", (constructId, rotation) => NewBroadcastConstructRotation?.Invoke(constructId, rotation));
            connection.On<ConcurrentDictionary<int, float>>("BroadcastConstructRotations", (constructIds) => NewBroadcastConstructRotations?.Invoke(constructIds));
            connection.On<int, float>("BroadcastConstructScale", (constructId, scale) => NewBroadcastConstructScale?.Invoke(constructId, scale));
            connection.On<int[], float>("BroadcastConstructScales", (constructIds, scale) => NewBroadcastConstructScales?.Invoke(constructIds, scale));
            connection.On<int, double, double, int>("BroadcastConstructMovement", (constructId, x, y, z) => NewBroadcastConstructMovement?.Invoke(constructId, x, y, z));

            connection.Reconnecting += Connection_Reconnecting;
            connection.Reconnected += Connection_Reconnected;
            connection.Closed += Connection_Closed;

            ServicePointManager.DefaultConnectionLimit = 10;
        }

        #endregion

        #region Connection

        public async Task ConnectAsync()
        {
            await connection.StartAsync();
        }

        public async Task DisconnectAsync()
        {
            await connection.StopAsync();
        }

        private Task Connection_Closed(Exception arg)
        {
            ConnectionClosed?.Invoke();
            return Task.Delay(100);
        }

        private Task Connection_Reconnected(string arg)
        {
            ConnectionReconnected?.Invoke();
            return Task.Delay(100);
        }

        private Task Connection_Reconnecting(Exception arg)
        {
            ConnectionReconnecting?.Invoke();
            return Task.Delay(100);
        }
        #endregion

        #region Session

        public async Task<Tuple<Avatar[], Construct[]>> LoginAsync(Avatar newUser)
        {
            return await connection.InvokeAsync<Tuple<Avatar[], Construct[]>>("Login", newUser);
        }

        public async Task LogoutAsync()
        {
            await connection.SendAsync("Logout");
        }

        #endregion

        #region Texting

        public async Task SendBroadcastMessageAsync(string msg)
        {
            await connection.SendAsync("BroadcastTextMessage", msg);
        }

        public async Task SendBroadcastMessageAsync(byte[] img)
        {
            await connection.SendAsync("BroadcastImageMessage", img);
        }

        public async Task SendUnicastMessageAsync(int recepientId, string msg)
        {
            await connection.SendAsync("UnicastTextMessage", recepientId, msg);
        }

        public async Task SendUnicastMessageAsync(int recepientId, byte[] img)
        {
            await connection.SendAsync("UnicastImageMessage", recepientId, img);
        }

        public async Task TypingAsync(int recepientId)
        {
            await connection.SendAsync("Typing", recepientId);
        }

        public async Task BroadcastTypingAsync()
        {
            await connection.SendAsync("BroadcastTyping");
        }

        #endregion

        #region Avatar

        public async Task BroadcastAvatarMovementAsync(int avatarId, double x, double y, int z)
        {
            await connection.SendAsync("BroadcastAvatarMovement", avatarId, x, y, z);
        }

        public async Task BroadcastAvatarActivityStatusAsync(int avatarId, int activityStatus)
        {
            await connection.SendAsync("BroadcastAvatarActivityStatus", avatarId, activityStatus);
        }

        #endregion

        #region Construct

        public async Task BroadcastConstructAsync(Construct construct)
        {
            await connection.SendAsync("BroadcastConstruct", construct);
        }

        public async Task BroadcastConstructsAsync(Construct[] constructs)
        {
            await connection.SendAsync("BroadcastConstructs", constructs);
        }

        public async Task RemoveConstructAsync(int constructId)
        {
            await connection.SendAsync("RemoveConstruct", constructId);
        }

        public async Task RemoveConstructsAsync(int[] constructIds)
        {
            await connection.SendAsync("RemoveConstructs", constructIds);
        }

        public async Task BroadcastConstructPlacementAsync(int constructId, int z)
        {
            await connection.SendAsync("BroadcastConstructPlacement", constructId, z);
        }

        public async Task BroadcastConstructRotationAsync(int constructId, float rotation)
        {
            await connection.SendAsync("BroadcastConstructRotation", constructId, rotation);
        }

        public async Task BroadcastConstructRotationsAsync(ConcurrentDictionary<int, float> constructIds)
        {
            await connection.SendAsync("BroadcastConstructRotations", constructIds);
        }

        public async Task BroadcastConstructScaleAsync(int constructId, float scale)
        {
            await connection.SendAsync("BroadcastConstructScale", constructId, scale);
        }

        public async Task BroadcastConstructScalesAsync(int[] constructIds, float scale)
        {
            await connection.SendAsync("BroadcastConstructScales", constructIds, scale);
        }

        public async Task BroadcastConstructMovementAsync(int constructId, double x, double y, int z)
        {
            await connection.SendAsync("BroadcastConstructMovement", constructId, x, y, z);
        }

        #endregion
    }
}
