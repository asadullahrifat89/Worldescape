using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Worldescape.Shared;

namespace Worldescape
{
    public class HubService : IHubService
    {
        #region Fields

        private readonly ILogger<HubService> _logger;

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
        public event Action<int, string, MessageType> NewTextMessage;
        public event Action<int, byte[], MessageType> NewImageMessage;
        public event Action<int, MessageType> AvatarTyping;

        // Avatar World Events
        public event Action<int, double, double, int> NewBroadcastAvatarMovement;
        public event Action<int, int> NewBroadcastAvatarActivityStatus;

        // Construct World Events
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
            _connection.On<int, string>(Constants.BroadcastedTextMessage, (senderId, message) => NewTextMessage?.Invoke(senderId, message, MessageType.Broadcast));
            _connection.On<int, byte[]>(Constants.BroadcastedPictureMessage, (senderId, img) => NewImageMessage?.Invoke(senderId, img, MessageType.Broadcast));
            _connection.On<int, string>(Constants.UnicastedTextMessage, (senderId, message) => NewTextMessage?.Invoke(senderId, message, MessageType.Unicast));
            _connection.On<int, byte[]>(Constants.UnicastedPictureMessage, (senderId, img) => NewImageMessage?.Invoke(senderId, img, MessageType.Unicast));
            _connection.On<int>(Constants.AvatarTyped, (senderId) => AvatarTyping?.Invoke(senderId, MessageType.Unicast));
            _connection.On<int>(Constants.AvatarBroadcastTyped, (senderId) => AvatarTyping?.Invoke(senderId, MessageType.Broadcast));

            // Avatar
            _connection.On<int, double, double, int>(Constants.BroadcastedAvatarMovement, (avatarId, x, y, z) => NewBroadcastAvatarMovement?.Invoke(avatarId, x, y, z));
            _connection.On<int, int>(Constants.BroadcastedAvatarActivityStatus, (avatarId, activityStatus) => NewBroadcastAvatarActivityStatus?.Invoke(avatarId, activityStatus));

            // Construct
            _connection.On<Construct>(Constants.BroadcastedConstruct, (construct) => NewBroadcastConstruct?.Invoke(construct));
            _connection.On<Construct[]>(Constants.BroadcastedConstructs, (constructs) => NewBroadcastConstructs?.Invoke(constructs));
            _connection.On<int>(Constants.RemovedConstruct, (constructId) => NewRemoveConstruct?.Invoke(constructId));
            _connection.On<int[]>(Constants.RemovedConstructs, (constructIds) => NewRemoveConstructs?.Invoke(constructIds));
            _connection.On<int, int>(Constants.BroadcastedConstructPlacement, (constructId, z) => NewBroadcastConstructPlacement?.Invoke(constructId, z));
            _connection.On<int, float>(Constants.BroadcastedConstructRotation, (constructId, rotation) => NewBroadcastConstructRotation?.Invoke(constructId, rotation));
            _connection.On<ConcurrentDictionary<int, float>>(Constants.BroadcastedConstructRotations, (constructIds) => NewBroadcastConstructRotations?.Invoke(constructIds));
            _connection.On<int, float>(Constants.BroadcastedConstructScale, (constructId, scale) => NewBroadcastConstructScale?.Invoke(constructId, scale));
            _connection.On<int[], float>(Constants.BroadcastedConstructScales, (constructIds, scale) => NewBroadcastConstructScales?.Invoke(constructIds, scale));
            _connection.On<int, double, double, int>(Constants.BroadcastedConstructMovement, (constructId, x, y, z) => NewBroadcastConstructMovement?.Invoke(constructId, x, y, z));

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

        public async Task<HubLoginResponse> Login(Avatar newUser)
        {
            Console.WriteLine(">>HubService: LoginAsync");
            return await _connection.InvokeAsync<HubLoginResponse>(Constants.Login, newUser);
        }

        public async Task Logout()
        {
            Console.WriteLine(">>HubService: LogoutAsync");
            await _connection.SendAsync(Constants.Logout);
        }

        #endregion

        #region Texting

        public async Task SendBroadcastMessage(string msg)
        {
            Console.WriteLine(">>HubService: SendBroadcastMessageAsync");
            await _connection.SendAsync(Constants.BroadcastTextMessage, msg);
        }

        public async Task SendBroadcastMessage(byte[] img)
        {
            Console.WriteLine(">>HubService: SendBroadcastMessageAsync");
            await _connection.SendAsync(Constants.BroadcastImageMessage, img);
        }

        public async Task SendUnicastMessage(int recepientId, string msg)
        {
            Console.WriteLine(">>HubService: SendUnicastMessageAsync");
            await _connection.SendAsync(Constants.UnicastTextMessage, recepientId, msg);
        }

        public async Task SendUnicastMessage(int recepientId, byte[] img)
        {
            Console.WriteLine(">>HubService: SendUnicastMessageAsync");
            await _connection.SendAsync(Constants.UnicastImageMessage, recepientId, img);
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

        public async Task BroadcastConstructs(Construct[] constructs)
        {
            Console.WriteLine(">>HubService: BroadcastConstructsAsync");
            await _connection.SendAsync(Constants.BroadcastConstructs, constructs);
        }

        public async Task RemoveConstruct(int constructId)
        {
            Console.WriteLine(">>HubService: RemoveConstructAsync");
            await _connection.SendAsync(Constants.RemoveConstruct, constructId);
        }

        public async Task RemoveConstructs(int[] constructIds)
        {
            Console.WriteLine(">>HubService: RemoveConstructsAsync");
            await _connection.SendAsync(Constants.RemoveConstructs, constructIds);
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

        public async Task BroadcastConstructRotations(ConcurrentDictionary<int, float> constructIds)
        {
            Console.WriteLine(">>HubService: BroadcastConstructRotationsAsync");
            await _connection.SendAsync(Constants.BroadcastConstructRotations, constructIds);
        }

        public async Task BroadcastConstructScale(int constructId, float scale)
        {
            Console.WriteLine(">>HubService: BroadcastConstructScaleAsync");
            await _connection.SendAsync(Constants.BroadcastConstructScale, constructId, scale);
        }

        public async Task BroadcastConstructScales(int[] constructIds, float scale)
        {
            Console.WriteLine(">>HubService: BroadcastConstructScalesAsync");
            await _connection.SendAsync(Constants.BroadcastConstructScales, constructIds, scale);
        }

        public async Task BroadcastConstructMovement(int constructId, double x, double y, int z)
        {
            Console.WriteLine(">>HubService: BroadcastConstructMovementAsync");
            await _connection.SendAsync(Constants.BroadcastConstructMovement, constructId, x, y, z);
        }

        #endregion
    }
}
