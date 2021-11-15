using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Worldescape.Shared;

namespace Worldescape
{
    public class HubService : IHubService
    {
        #region Fields

        private readonly HubConnection _connection;

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

        public HubService()
        {
#if DEBUG
            var url = Properties.Resources.DevHubService;
            //var url = Properties.Resources.ProdHubService;
#else
            var url = Properties.Resources.ProdHubService;
#endif
            _connection = new HubConnectionBuilder().WithUrl(url).WithAutomaticReconnect().Build();

            // Session
            _connection.On<Avatar>("AvatarLogin", (avatar) => AvatarLoggedIn?.Invoke(avatar));
            _connection.On<int>("AvatarLogout", (senderId) => AvatarLoggedOut?.Invoke(senderId));
            _connection.On<int>("AvatarDisconnection", (senderId) => AvatarDisconnected?.Invoke(senderId));
            _connection.On<int>("AvatarReconnection", (senderId) => AvatarReconnected?.Invoke(senderId));

            // Texting
            _connection.On<int, string>("BroadcastTextMessage", (senderId, message) => NewTextMessage?.Invoke(senderId, message, MessageType.Broadcast));
            _connection.On<int, byte[]>("BroadcastPictureMessage", (senderId, img) => NewImageMessage?.Invoke(senderId, img, MessageType.Broadcast));
            _connection.On<int, string>("UnicastTextMessage", (senderId, message) => NewTextMessage?.Invoke(senderId, message, MessageType.Unicast));
            _connection.On<int, byte[]>("UnicastPictureMessage", (senderId, img) => NewImageMessage?.Invoke(senderId, img, MessageType.Unicast));
            _connection.On<int>("AvatarTyping", (senderId) => AvatarTyping?.Invoke(senderId, MessageType.Unicast));
            _connection.On<int>("AvatarBroadcastTyping", (senderId) => AvatarTyping?.Invoke(senderId, MessageType.Broadcast));

            // Avatar
            _connection.On<int, double, double, int>("BroadcastAvatarMovement", (avatarId, x, y, z) => NewBroadcastAvatarMovement?.Invoke(avatarId, x, y, z));
            _connection.On<int, int>("BroadcastAvatarActivityStatus", (avatarId, activityStatus) => NewBroadcastAvatarActivityStatus?.Invoke(avatarId, activityStatus));

            // Construct
            _connection.On<Construct>("BroadcastConstruct", (construct) => NewBroadcastConstruct?.Invoke(construct));
            _connection.On<Construct[]>("BroadcastConstructs", (constructs) => NewBroadcastConstructs?.Invoke(constructs));
            _connection.On<int>("RemoveConstruct", (constructId) => NewRemoveConstruct?.Invoke(constructId));
            _connection.On<int[]>("RemoveConstructs", (constructIds) => NewRemoveConstructs?.Invoke(constructIds));
            _connection.On<int, int>("BroadcastConstructPlacement", (constructId, z) => NewBroadcastConstructPlacement?.Invoke(constructId, z));
            _connection.On<int, float>("BroadcastConstructRotation", (constructId, rotation) => NewBroadcastConstructRotation?.Invoke(constructId, rotation));
            _connection.On<ConcurrentDictionary<int, float>>("BroadcastConstructRotations", (constructIds) => NewBroadcastConstructRotations?.Invoke(constructIds));
            _connection.On<int, float>("BroadcastConstructScale", (constructId, scale) => NewBroadcastConstructScale?.Invoke(constructId, scale));
            _connection.On<int[], float>("BroadcastConstructScales", (constructIds, scale) => NewBroadcastConstructScales?.Invoke(constructIds, scale));
            _connection.On<int, double, double, int>("BroadcastConstructMovement", (constructId, x, y, z) => NewBroadcastConstructMovement?.Invoke(constructId, x, y, z));

            // Connection
            _connection.Reconnecting += Connection_Reconnecting;
            _connection.Reconnected += Connection_Reconnected;
            _connection.Closed += Connection_Closed;

            //ServicePointManager.DefaultConnectionLimit = 10;
        }

        #endregion

        #region Connection

        public bool IsConnected()
        {
            Console.WriteLine(">>IsConnected: " + _connection?.State);
            return _connection.State == HubConnectionState.Connected || _connection.State == HubConnectionState.Connecting || _connection.State == HubConnectionState.Reconnecting;
        }

        public async Task ConnectAsync()
        {
            Console.WriteLine(">>HubService: ConnectAsync");
            await _connection.StartAsync();
        }

        public async Task DisconnectAsync()
        {
            Console.WriteLine(">>HubService: DisconnectAsync");
            await _connection.StopAsync();
        }

        private Task Connection_Closed(Exception arg)
        {
            Console.WriteLine("<<HubService: Connection_Closed");
            ConnectionClosed?.Invoke();
            return Task.Delay(100);
        }

        private Task Connection_Reconnected(string arg)
        {
            Console.WriteLine("<<HubService: Connection_Reconnected");
            ConnectionReconnected?.Invoke();
            return Task.Delay(100);
        }

        private Task Connection_Reconnecting(Exception arg)
        {
            Console.WriteLine("<<HubService: Connection_Reconnecting");
            ConnectionReconnecting?.Invoke();
            return Task.Delay(100);
        }
        #endregion

        #region Session

        public async Task<HubLoginResponse> Login(Avatar newUser)
        {
            Console.WriteLine(">>HubService: LoginAsync");
            return await _connection.InvokeAsync<HubLoginResponse>("Login", newUser);
        }

        public async Task Logout()
        {
            Console.WriteLine(">>HubService: LogoutAsync");
            await _connection.SendAsync("Logout");
        }

        #endregion

        #region Texting

        public async Task SendBroadcastMessage(string msg)
        {
            Console.WriteLine(">>HubService: SendBroadcastMessageAsync");
            await _connection.SendAsync("BroadcastTextMessage", msg);
        }

        public async Task SendBroadcastMessage(byte[] img)
        {
            Console.WriteLine(">>HubService: SendBroadcastMessageAsync");
            await _connection.SendAsync("BroadcastImageMessage", img);
        }

        public async Task SendUnicastMessage(int recepientId, string msg)
        {
            Console.WriteLine(">>HubService: SendUnicastMessageAsync");
            await _connection.SendAsync("UnicastTextMessage", recepientId, msg);
        }

        public async Task SendUnicastMessage(int recepientId, byte[] img)
        {
            Console.WriteLine(">>HubService: SendUnicastMessageAsync");
            await _connection.SendAsync("UnicastImageMessage", recepientId, img);
        }

        public async Task Typing(int recepientId)
        {
            Console.WriteLine(">>HubService: TypingAsync");
            await _connection.SendAsync("Typing", recepientId);
        }

        public async Task BroadcastTyping()
        {
            Console.WriteLine(">>HubService: BroadcastTypingAsync");
            await _connection.SendAsync("BroadcastTyping");
        }

        #endregion

        #region Avatar

        public async Task BroadcastAvatarMovement(int avatarId, double x, double y, int z)
        {
            Console.WriteLine(">>HubService: BroadcastAvatarMovementAsync");
            await _connection.SendAsync("BroadcastAvatarMovement", avatarId, x, y, z);
        }

        public async Task BroadcastAvatarActivityStatus(int avatarId, int activityStatus)
        {
            Console.WriteLine(">>HubService: BroadcastAvatarActivityStatusAsync");
            await _connection.SendAsync("BroadcastAvatarActivityStatus", avatarId, activityStatus);
        }

        #endregion

        #region Construct

        public async Task BroadcastConstruct(Construct construct)
        {
            Console.WriteLine(">>HubService: BroadcastConstructAsync");
            await _connection.SendAsync("BroadcastConstruct", construct);
        }

        public async Task BroadcastConstructs(Construct[] constructs)
        {
            Console.WriteLine(">>HubService: BroadcastConstructsAsync");
            await _connection.SendAsync("BroadcastConstructs", constructs);
        }

        public async Task RemoveConstruct(int constructId)
        {
            Console.WriteLine(">>HubService: RemoveConstructAsync");
            await _connection.SendAsync("RemoveConstruct", constructId);
        }

        public async Task RemoveConstructs(int[] constructIds)
        {
            Console.WriteLine(">>HubService: RemoveConstructsAsync");
            await _connection.SendAsync("RemoveConstructs", constructIds);
        }

        public async Task BroadcastConstructPlacement(int constructId, int z)
        {
            Console.WriteLine(">>HubService: BroadcastConstructPlacementAsync");
            await _connection.SendAsync("BroadcastConstructPlacement", constructId, z);
        }

        public async Task BroadcastConstructRotation(int constructId, float rotation)
        {
            Console.WriteLine(">>HubService: BroadcastConstructRotationAsync");
            await _connection.SendAsync("BroadcastConstructRotation", constructId, rotation);
        }

        public async Task BroadcastConstructRotations(ConcurrentDictionary<int, float> constructIds)
        {
            Console.WriteLine(">>HubService: BroadcastConstructRotationsAsync");
            await _connection.SendAsync("BroadcastConstructRotations", constructIds);
        }

        public async Task BroadcastConstructScale(int constructId, float scale)
        {
            Console.WriteLine(">>HubService: BroadcastConstructScaleAsync");
            await _connection.SendAsync("BroadcastConstructScale", constructId, scale);
        }

        public async Task BroadcastConstructScales(int[] constructIds, float scale)
        {
            Console.WriteLine(">>HubService: BroadcastConstructScalesAsync");
            await _connection.SendAsync("BroadcastConstructScales", constructIds, scale);
        }

        public async Task BroadcastConstructMovement(int constructId, double x, double y, int z)
        {
            Console.WriteLine(">>HubService: BroadcastConstructMovementAsync");
            await _connection.SendAsync("BroadcastConstructMovement", constructId, x, y, z);
        }

        #endregion
    }
}
