using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Worldescape.Common;

namespace Worldescape.Service
{
    /// <summary>
    /// Prpvides access to all client side signalR hub methods.
    /// </summary>
    public class SignalRHubClient : ISignalRHubClient
    {
        #region Fields

        private readonly HubConnection _connection;

        // Hub Connectivity       
        public event Action ConnectionReconnecting;
        public event Action ConnectionReconnected;
        public event Action ConnectionClosed;

        // Avatar Connectivity        
        public event Action<int> AvatarDisconnected;
        public event Action<int> AvatarReconnected;
        public event Action<Avatar> AvatarLoggedIn;
        public event Action<int> AvatarLoggedOut;

        // Texting
        public event Action<ChatMessage, MessageType> NewMessage;
        public event Action<int, MessageType> AvatarTyping;

        // Avatar World Events
        public event Action<int, double, double, int> NewBroadcastAvatarMovement;
        public event Action<int, int> NewBroadcastAvatarActivityStatus;

        // Construct World Events
        public event Action<Construct> NewBroadcastConstruct;
        public event Action<int> NewRemoveConstruct;
        public event Action<int, int> NewBroadcastConstructPlacement;
        public event Action<int, float> NewBroadcastConstructRotation;
        public event Action<int, float> NewBroadcastConstructScale;
        public event Action<int, double, double, int> NewBroadcastConstructMovement;


        // Portal
        public event Action<Portal> NewBroadcastPortal;

        #endregion

        #region Ctor

        public SignalRHubClient(HttpServiceHelper httpServiceHelper)
        {
            string url = httpServiceHelper.GetHubServiceUrl();

            _connection = new HubConnectionBuilder().WithUrl(url, options =>
            {
                options.SkipNegotiation = true;
                options.Transports = HttpTransportType.WebSockets;
                options.CloseTimeout = TimeSpan.MaxValue;

            }).WithAutomaticReconnect().Build();

            // Session
            _connection.On<Avatar>(Constants.AvatarLoggedIn, (avatar) => AvatarLoggedIn?.Invoke(avatar));
            _connection.On<int>(Constants.AvatarLoggedOut, (senderId) => AvatarLoggedOut?.Invoke(senderId));
            _connection.On<int>(Constants.AvatarDisconnected, (senderId) => AvatarDisconnected?.Invoke(senderId));
            _connection.On<int>(Constants.AvatarReconnected, (senderId) => AvatarReconnected?.Invoke(senderId));

            // Texting
            _connection.On<ChatMessage>(Constants.BroadcastedMessage, (message) => NewMessage?.Invoke(message, MessageType.Broadcast));
            _connection.On<ChatMessage>(Constants.UnicastedMessage, (message) => NewMessage?.Invoke(message, MessageType.Unicast));

            _connection.On<int>(Constants.AvatarTyped, (senderId) => AvatarTyping?.Invoke(senderId, MessageType.Unicast));
            _connection.On<int>(Constants.AvatarBroadcastTyped, (senderId) => AvatarTyping?.Invoke(senderId, MessageType.Broadcast));

            // Avatar
            _connection.On<int, double, double, int>(Constants.BroadcastedAvatarMovement, (avatarId, x, y, z) => NewBroadcastAvatarMovement?.Invoke(avatarId, x, y, z));
            _connection.On<int, int>(Constants.BroadcastedAvatarActivityStatus, (avatarId, activityStatus) => NewBroadcastAvatarActivityStatus?.Invoke(avatarId, activityStatus));

            // Construct
            _connection.On<Construct>(Constants.BroadcastedConstruct, (construct) => NewBroadcastConstruct?.Invoke(construct));
            _connection.On<int>(Constants.RemovedConstruct, (constructId) => NewRemoveConstruct?.Invoke(constructId));
            _connection.On<int, int>(Constants.BroadcastedConstructPlacement, (constructId, z) => NewBroadcastConstructPlacement?.Invoke(constructId, z));
            _connection.On<int, float>(Constants.BroadcastedConstructRotation, (constructId, rotation) => NewBroadcastConstructRotation?.Invoke(constructId, rotation));
            _connection.On<int, float>(Constants.BroadcastedConstructScale, (constructId, scale) => NewBroadcastConstructScale?.Invoke(constructId, scale));
            _connection.On<int, double, double, int>(Constants.BroadcastedConstructMovement, (constructId, x, y, z) => NewBroadcastConstructMovement?.Invoke(constructId, x, y, z));

            // Portal
            _connection.On<Portal>(Constants.BroadcastedPortal, (portal) => NewBroadcastPortal?.Invoke(portal));

            // Connection
            _connection.Reconnecting += Connection_Reconnecting;
            _connection.Reconnected += Connection_Reconnected;
            _connection.Closed += Connection_Closed;

            ServicePointManager.DefaultConnectionLimit = 10;
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

        public async Task<Avatar> Login(Avatar newUser)
        {
            Console.WriteLine(">>HubService: LoginAsync");
            return await _connection.InvokeAsync<Avatar>(Constants.Login, newUser);
        }

        public async Task Logout()
        {
            Console.WriteLine(">>HubService: LogoutAsync");
            await _connection.SendAsync(Constants.Logout);
        }

        #endregion

        #region Texting

        public async Task SendBroadcastMessage(ChatMessage chatMessage)
        {
            Console.WriteLine(">>HubService: SendBroadcastMessageAsync");
            await _connection.SendAsync(Constants.BroadcastMessage, chatMessage);
        }

        public async Task SendUnicastMessage(ChatMessage chatMessage)
        {
            Console.WriteLine(">>HubService: SendUnicastMessageAsync");
            await _connection.SendAsync(Constants.UnicastMessage, chatMessage);
        }

        public async Task Typing(int recepientId)
        {
            Console.WriteLine(">>HubService: TypingAsync");
            await _connection.SendAsync(Constants.Typing, recepientId);
        }

        public async Task BroadcastTyping()
        {
            Console.WriteLine(">>HubService: BroadcastTypingAsync");
            await _connection.SendAsync(Constants.BroadcastTyping);
        }

        #endregion

        #region Avatar

        public async Task BroadcastAvatarMovement(int avatarId, double x, double y, int z)
        {
            Console.WriteLine(">>HubService: BroadcastAvatarMovementAsync");
            await _connection.SendAsync(Constants.BroadcastAvatarMovement, avatarId, x, y, z);
        }

        public async Task BroadcastAvatarActivityStatus(int avatarId, int activityStatus)
        {
            Console.WriteLine(">>HubService: BroadcastAvatarActivityStatusAsync");
            await _connection.SendAsync(Constants.BroadcastAvatarActivityStatus, avatarId, activityStatus);
        }

        #endregion

        #region Construct

        public async Task BroadcastConstruct(Construct construct)
        {
            Console.WriteLine(">>HubService: BroadcastConstructAsync");
            await _connection.SendAsync(Constants.BroadcastConstruct, construct);
        }

        public async Task RemoveConstruct(int constructId)
        {
            Console.WriteLine(">>HubService: RemoveConstructAsync");
            await _connection.SendAsync(Constants.RemoveConstruct, constructId);
        }

        public async Task BroadcastConstructPlacement(int constructId, int z)
        {
            Console.WriteLine(">>HubService: BroadcastConstructPlacementAsync");
            await _connection.SendAsync(Constants.BroadcastConstructPlacement, constructId, z);
        }

        public async Task BroadcastConstructRotation(int constructId, float rotation)
        {
            Console.WriteLine(">>HubService: BroadcastConstructRotationAsync");
            await _connection.SendAsync(Constants.BroadcastConstructRotation, constructId, rotation);
        }

        public async Task BroadcastConstructScale(int constructId, float scale)
        {
            Console.WriteLine(">>HubService: BroadcastConstructScaleAsync");
            await _connection.SendAsync(Constants.BroadcastConstructScale, constructId, scale);
        }

        public async Task BroadcastConstructMovement(int constructId, double x, double y, int z)
        {
            Console.WriteLine(">>HubService: BroadcastConstructMovementAsync");
            await _connection.SendAsync(Constants.BroadcastConstructMovement, constructId, x, y, z);
        }

        #endregion

        #region Portal

        public async Task BroadcastPortal(Portal portal)
        {
            Console.WriteLine(">>HubService: BroadcastPortalAsync");
            await _connection.SendAsync(Constants.BroadcastPortal, portal);
        }

        #endregion
    }
}
