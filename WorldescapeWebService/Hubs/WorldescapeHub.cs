using MediatR;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System.Collections.Concurrent;
using Worldescape.Data;
using Worldescape.Database;

namespace WorldescapeWebService
{
    /// <summary>
    /// Provies acess tp server side signalR hub methods, hosts concurrent avatars and constructs world wise. New ConnectionId is generated per user login.
    /// </summary>
    public class WorldescapeHub : Hub
    {
        #region Fields

        readonly ILogger<WorldescapeHub> _logger;

        //<ConnectionId, Avatar>
        static ConcurrentDictionary<string, Avatar> ConcurrentAvatars = new();

        //<ConstructId, Construct>
        static ConcurrentDictionary<int, Construct> ConcurrentConstructs = new();

        readonly DatabaseService _databaseService;
        #endregion

        #region Ctor

        public WorldescapeHub(
            ILogger<WorldescapeHub> logger,
            DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        #endregion

        #region Common

        private static string GetUsersGroup(Avatar user)
        {
            var result = user.World.Id.ToString();
            Console.WriteLine("GetUsersGroup: " + result);
            return result;
        }

        private Avatar GetCallingUser(int userId = 0)
        {
            if (userId > 0)
            {
                return ConcurrentAvatars.SingleOrDefault(c => c.Value.Id == userId).Value;
            }
            else
            {
                return ConcurrentAvatars.SingleOrDefault(c => c.Key == Context.ConnectionId).Value;
            }
        }

        private string GetUserConnectionId(int userId)
        {
            return ConcurrentAvatars.SingleOrDefault(c => c.Value.Id == userId).Key;
        }

        #endregion

        #region Hub Methods

        #region Hub Connectivity

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Avatar avatar = GetCallingUser();
            if (avatar != null)
            {
                UpdateAvatarDisconnectionTime(avatar.Id, DateTime.Now);

                var group = GetUsersGroup(avatar);

                Clients.OthersInGroup(group).SendAsync(Constants.AvatarDisconnected, avatar.Id);

                _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {avatar.Id} OnDisconnectedAsync - {DateTime.Now} World: {group}");
            }
            return base.OnDisconnectedAsync(exception);
        }

        public override Task OnConnectedAsync()
        {
            Avatar avatar = GetCallingUser();
            if (avatar != null)
            {
                UpdateAvatarReconnectionTime(avatar.Id, DateTime.Now);

                var group = GetUsersGroup(avatar);

                Clients.OthersInGroup(group).SendAsync(Constants.AvatarReconnected, avatar.Id);

                _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {avatar.Id} OnConnectedAsync- {DateTime.Now} World: {group}");
            }
            return base.OnConnectedAsync();
        }

        #endregion

        #region Avatar Connectivity

        [HubMethodName(Constants.Login)]
        public async Task<HubLoginResponse> Login(Avatar avatar)
        {
            // If an existing avatar doesn't exist
            if (!ConcurrentAvatars.Any(x => x.Value.Id == avatar.Id))
            {
                var minValue = DateTime.MinValue;

                // Delete inactive avatars who have remained inactive for more than a minute
                //var concurrentAvatars = OnlineAvatars.Values.Where(x => x.World.Id == avatar.World.Id && x.Session != null && x.Session.DisconnectionTime != minValue);

                //foreach (var inAvatar in concurrentAvatars)
                //{
                //    TimeSpan diff = DateTime.Now - inAvatar.Session.DisconnectionTime;

                //    if (diff.TotalMinutes >= TimeSpan.FromMinutes(1).TotalMinutes)
                //    {
                //        string connectionId = GetUserConnectionId(avatar.Id);
                //        OnlineAvatars.TryRemove(connectionId, out Avatar removed);
                //    }
                //}

                avatar.Session = new UserSession() { ReconnectionTime = DateTime.UtcNow };
                avatar.ConnectionId = Context.ConnectionId;

                // Save the new avatar
                if (!ConcurrentAvatars.TryAdd(Context.ConnectionId, avatar))
                {
                    return null;
                }

                var group = avatar.World.Id.ToString();
                await Groups.AddToGroupAsync(Context.ConnectionId, group);

                await Clients.OthersInGroup(group).SendAsync(Constants.AvatarLoggedIn, avatar);

                _logger.LogInformation($"++ ConnectionId: {Context.ConnectionId} AvatarId: {avatar.Id} Login-> World {avatar.World.Id} - {DateTime.Now} World: {group}");

                // Find all avatars from the calling avatar's world
                var avatars = ConcurrentAvatars.Where(x => x.Value.World.Id == avatar.World.Id)?.Select(z => z.Value).ToArray();

                // Return the curated avatars and constructs
                return new HubLoginResponse()
                {
                    Avatars = avatars ?? new Avatar[] { }
                };
            }
            else
            {
                // Remove old instance
                ConcurrentAvatars.TryRemove(ConcurrentAvatars.FirstOrDefault(x => x.Value.Id == avatar.Id));

                avatar.Session = new UserSession() { ReconnectionTime = DateTime.UtcNow };
                avatar.ConnectionId = Context.ConnectionId;

                // Add new instance            
                ConcurrentAvatars.TryAdd(Context.ConnectionId, avatar);

                var group = GetUsersGroup(avatar);
                await Groups.AddToGroupAsync(Context.ConnectionId, group);

                await Clients.OthersInGroup(group).SendAsync(Constants.AvatarLoggedIn, avatar);

                _logger.LogInformation($"++ ConnectionId: {Context.ConnectionId} AvatarId: {avatar.Id} Login-> World {avatar.World.Id} - {DateTime.Now} World: {group}");

                // Find all avatars from the calling avatar's world
                var avatars = ConcurrentAvatars.Where(x => x.Value.World.Id == avatar.World.Id)?.Select(z => z.Value).ToArray();

                // Return the curated avatars and constructs
                return new HubLoginResponse()
                {
                    Avatars = avatars ?? new Avatar[] { }
                };
            }
        }

        [HubMethodName(Constants.Logout)]
        public void Logout()
        {
            Avatar avatar = GetCallingUser();

            if (avatar != null && !avatar.IsEmpty())
            {
                if (ConcurrentAvatars.Any(x => x.Value.Id == avatar.Id))
                {
                    string connectionId = ConcurrentAvatars.SingleOrDefault((c) => c.Value.Id == avatar.Id).Key;

                    ConcurrentAvatars.TryRemove(connectionId, out Avatar a);

                    var group = avatar.World.Id.ToString();
                    Clients.OthersInGroup(group).SendAsync(Constants.AvatarLoggedOut, avatar.Id);

                    _logger.LogInformation($"-- ConnectionId: {Context.ConnectionId} AvatarId: {avatar.Id} Logout-> WorldId {avatar.World.Id} - {DateTime.Now} World: {group}");
                }
            }
        }

        #endregion

        #region Messagin

        [HubMethodName(Constants.BroadcastTextMessage)]
        public async void BroadcastTextMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Avatar sender = GetCallingUser();

                var group = GetUsersGroup(sender);
                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedTextMessage, sender.Id, message);

                _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {sender.Id} BroadcastTextMessage - {DateTime.Now} World: {group}");
            }
        }


        [HubMethodName(Constants.BroadcastImageMessage)]
        public async void BroadcastImageMessage(byte[] img)
        {
            if (img != null)
            {
                Avatar sender = GetCallingUser();

                var group = GetUsersGroup(sender);
                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedPictureMessage, sender.Id, img);

                _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {sender.Id} BroadcastImageMessage - {DateTime.Now} World: {group}");
            }
        }


        [HubMethodName(Constants.UnicastTextMessage)]
        public async void UnicastTextMessage(int recepientId, string message)
        {
            Avatar sender = GetCallingUser();
            string recipientConnectionId = GetUserConnectionId(recepientId);

            if (sender != null
                && recepientId != sender.Id
                && !string.IsNullOrEmpty(message))
            {
                if (ConcurrentAvatars.Any(x => x.Value.Id == recepientId && x.Value.ConnectionId == recipientConnectionId))
                {
                    await Clients.Client(recipientConnectionId).SendAsync(Constants.UnicastedTextMessage, sender.Id, message);

                    _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {sender.Id} UnicastTextMessage - {DateTime.Now} World: {sender.World.Id}");
                }
            }
        }


        [HubMethodName(Constants.UnicastImageMessage)]
        public async void UnicastImageMessage(int recepientId, byte[] img)
        {
            Avatar sender = GetCallingUser();
            string recipientConnectionId = GetUserConnectionId(recepientId);

            if (sender != null
                && recepientId != sender.Id
                && img != null)
            {
                if (ConcurrentAvatars.Any(x => x.Value.Id == recepientId && x.Value.ConnectionId == recipientConnectionId))
                {
                    await Clients.Client(recipientConnectionId).SendAsync(Constants.UnicastedPictureMessage, sender.Id, img);

                    _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {sender.Id} UnicastImageMessage - {DateTime.Now} World: {sender.World.Id}");
                }
            }
        }


        [HubMethodName(Constants.Typing)]
        public async void Typing(int recepientId)
        {
            if (recepientId <= 0)
            {
                return;
            }

            Avatar sender = GetCallingUser();
            string recipientConnectionId = GetUserConnectionId(recepientId);

            if (ConcurrentAvatars.Any(x => x.Value.Id == recepientId && x.Value.ConnectionId == recipientConnectionId))
            {
                await Clients.Client(recipientConnectionId).SendAsync(Constants.AvatarTyped, sender.Id);

                _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {sender.Id} Typing - {DateTime.Now} World: {sender.World.Id}");
            }
        }


        [HubMethodName(Constants.BroadcastTyping)]
        public void BroadcastTyping()
        {
            Avatar sender = GetCallingUser();

            var group = GetUsersGroup(sender);

            Clients.OthersInGroup(group).SendAsync(Constants.AvatarBroadcastTyped, sender.Id);

            _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {sender.Id} BroadcastTyping - {DateTime.Now} World: {group}");
        }

        #endregion

        #region Avatar World Events

        [HubMethodName(Constants.BroadcastAvatarMovement)]
        public async void BroadcastAvatarMovement(int avatarId, double x, double y, int z)
        {
            if (avatarId > 0)
            {
                var group = GetUsersGroup(GetCallingUser(avatarId));

                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedAvatarMovement, avatarId, x, y, z);

                UpdateAvatarMovement(avatarId, x, y, z);

                _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {avatarId} BroadcastAvatarMovement - {DateTime.Now} World: {group}");
            }
        }


        [HubMethodName(Constants.BroadcastAvatarActivityStatus)]
        public async void BroadcastAvatarActivityStatus(int avatarId, int activityStatus)
        {
            if (avatarId > 0)
            {
                var group = GetCallingUser(avatarId);
                await Clients.OthersInGroup(GetUsersGroup(group)).SendAsync(Constants.BroadcastedAvatarActivityStatus, avatarId, activityStatus);

                UpdateAvatarActivityStatus(avatarId, activityStatus);

                _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {avatarId} BroadcastAvatarActivityStatus - {DateTime.Now} World: {group}");
            }
        }

        #endregion

        #region Construct World Events

        [HubMethodName(Constants.BroadcastConstruct)]
        public async void BroadcastConstruct(Construct construct)
        {
            if (construct.Id > 0)
            {
                var group = GetUsersGroup(GetCallingUser());
                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedConstruct, construct);

                AddOrUpdateConstructInConstructs(construct);
                _logger.LogInformation($"<> {construct.Id} BroadcastConstruct - {DateTime.Now} World: {group}");
            }
        }


        [HubMethodName(Constants.BroadcastConstructs)]
        public async void BroadcastConstructs(Construct[] constructs)
        {
            if (constructs != null && constructs.Any())
            {
                var group = GetUsersGroup(GetCallingUser());

                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedConstructs, constructs);

                foreach (var construct in constructs)
                {
                    AddOrUpdateConstructInConstructs(construct);
                }
                _logger.LogInformation($"<> {constructs.Count()} BroadcastConstructs - {DateTime.Now} World: {group}");
            }
        }


        [HubMethodName(Constants.RemoveConstruct)]
        public async void RemoveConstruct(int constructId)
        {
            if (constructId > 0)
            {
                var group = GetUsersGroup(GetCallingUser());

                await Clients.OthersInGroup(group).SendAsync(Constants.RemovedConstruct, constructId);

                RemoveConstructFromConstructs(constructId);
                _logger.LogInformation($"<> Construct: {constructId} RemoveConstruct - {DateTime.Now} World: {group}");
            }
        }


        [HubMethodName(Constants.RemoveConstructs)]
        public async void RemoveConstructs(int[] constructIds)
        {
            if (constructIds != null && constructIds.Any())
            {
                var group = GetUsersGroup(GetCallingUser());

                await Clients.OthersInGroup(group).SendAsync(Constants.RemovedConstructs, constructIds);

                foreach (var constructId in constructIds)
                {
                    RemoveConstructFromConstructs(constructId);
                }

                _logger.LogInformation($"<> {constructIds.Count()} RemoveConstructs - {DateTime.Now} World: {group}");
            }
        }


        [HubMethodName(Constants.BroadcastConstructPlacement)]
        public async void BroadcastConstructPlacement(int constructId, int z)
        {
            if (constructId > 0)
            {
                var group = GetUsersGroup(GetCallingUser());

                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedConstructPlacement, constructId, z);

                UpdateConstructPlacementInConstructs(constructId, z);
                _logger.LogInformation($"<> Construct: {constructId} BroadcastConstructPlacement - {DateTime.Now} World: {group}");
            }
        }


        [HubMethodName(Constants.BroadcastConstructRotation)]
        public async void BroadcastConstructRotation(int constructId, float rotation)
        {
            if (constructId > 0)
            {
                var group = GetUsersGroup(GetCallingUser());
                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedConstructRotation, constructId, rotation);

                UpdateConstructRotationInConstructs(constructId, rotation);
                _logger.LogInformation($"<> Construct: {constructId} BroadcastConstructRotation - {DateTime.Now} World: {group}");
            }
        }


        [HubMethodName(Constants.BroadcastConstructRotations)]
        public async void BroadcastConstructRotations(ConcurrentDictionary<int, float> constructIds)
        {
            if (constructIds != null)
            {
                var group = GetUsersGroup(GetCallingUser());
                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedConstructRotations, constructIds);

                foreach (var constructId in constructIds)
                {
                    UpdateConstructRotationInConstructs(constructId.Key, constructId.Value);
                }

                _logger.LogInformation($"<> {constructIds.Count()} BroadcastConstructRotations - {DateTime.Now} World: {group}");
            }
        }


        [HubMethodName(Constants.BroadcastConstructScale)]
        public async void BroadcastConstructScale(int constructId, float scale)
        {
            if (constructId > 0)
            {
                var group = GetUsersGroup(GetCallingUser());

                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedConstructScale, constructId, scale);

                UpdateConstructScaleInConstructs(constructId, scale);
                _logger.LogInformation($"<> Construct: {constructId} BroadcastConstructScale - {DateTime.Now} World: {group}");
            }
        }


        [HubMethodName(Constants.BroadcastConstructScales)]
        public async void BroadcastConstructScales(int[] constructIds, float scale)
        {
            if (constructIds != null && constructIds.Any())
            {
                var group = GetUsersGroup(GetCallingUser());

                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedConstructScales, constructIds, scale);

                foreach (var constructId in constructIds)
                {
                    UpdateConstructScaleInConstructs(constructId, scale);
                }

                _logger.LogInformation($"<> {constructIds.Count()} BroadcastConstructScales - {DateTime.Now} World: {group}");
            }
        }


        [HubMethodName(Constants.BroadcastConstructMovement)]
        public async void BroadcastConstructMovement(int constructId, double x, double y, int z)
        {
            if (constructId > 0)
            {
                var group = GetUsersGroup(GetCallingUser());

                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedConstructMovement, constructId, x, y, z);

                UpdateConstructMovementInConstructs(constructId, x, y, z);
                _logger.LogInformation($"<> Construct: Construct: {constructId} BroadcastConstructMovement - {DateTime.Now} World: {group}");
            }
        }

        #endregion

        #endregion

        #region Concurrent Avatars

        private void UpdateAvatarReconnectionTime(int avatarId, DateTime reconnectionTime)
        {
            var connectionId = GetUserConnectionId(avatarId);

            if (ConcurrentAvatars.ContainsKey(connectionId))
            {
                var conUpdated = ConcurrentAvatars[connectionId];
                conUpdated.Session.ReconnectionTime = reconnectionTime;
                ConcurrentAvatars.TryUpdate(key: connectionId, newValue: conUpdated, comparisonValue: ConcurrentAvatars[connectionId]);
            }
        }

        private void UpdateAvatarDisconnectionTime(int avatarId, DateTime disconnectionTime)
        {
            var connectionId = GetUserConnectionId(avatarId);

            if (ConcurrentAvatars.ContainsKey(connectionId))
            {
                var conUpdated = ConcurrentAvatars[connectionId];
                conUpdated.Session.DisconnectionTime = disconnectionTime;
                ConcurrentAvatars.TryUpdate(key: connectionId, newValue: conUpdated, comparisonValue: ConcurrentAvatars[connectionId]);
            }
        }

        private void UpdateAvatarActivityStatus(int avatarId, int activityStatus)
        {
            var connectionId = GetUserConnectionId(avatarId);

            if (ConcurrentAvatars.ContainsKey(connectionId))
            {
                var conUpdated = ConcurrentAvatars[connectionId];
                conUpdated.ActivityStatus = (ActivityStatus)activityStatus;
                ConcurrentAvatars.TryUpdate(key: connectionId, newValue: conUpdated, comparisonValue: ConcurrentAvatars[connectionId]);
            }
        }

        private void UpdateAvatarMovement(int avatarId, double x, double y, int z)
        {
            var connectionId = GetUserConnectionId(avatarId);

            if (ConcurrentAvatars.ContainsKey(connectionId))
            {
                var conUpdated = ConcurrentAvatars[connectionId];
                conUpdated.Coordinate.X = x;
                conUpdated.Coordinate.Y = y;
                conUpdated.Coordinate.Z = z;

                ConcurrentAvatars.TryUpdate(key: connectionId, newValue: conUpdated, comparisonValue: ConcurrentAvatars[connectionId]);
            }
        }

        #endregion

        #region Concurrent Constructs

        private async void UpdateConstructPlacementInConstructs(int constructId, int z)
        {
            if (await _databaseService.FindById<Construct>(constructId) is Construct construct)
            {
                var update = Builders<Construct>.Update.Set(x => x.Coordinate.Z, z);
                await _databaseService.UpdateById(update, construct.Id);
            }
        }

        private async void UpdateConstructRotationInConstructs(int constructId, float rotation)
        {
            if (await _databaseService.FindById<Construct>(constructId) is Construct construct)
            {
                var update = Builders<Construct>.Update.Set(x => x.Rotation, rotation);
                await _databaseService.UpdateById(update, construct.Id);
            }
        }

        private async void UpdateConstructScaleInConstructs(int constructId, float scale)
        {
            if (await _databaseService.FindById<Construct>(constructId) is Construct construct)
            {
                var update = Builders<Construct>.Update.Set(x => x.Scale, scale);
                await _databaseService.UpdateById(update, construct.Id);
            }
        }

        private async void RemoveConstructFromConstructs(int constructId)
        {
            if (await _databaseService.ExistsById<Construct>(constructId))
            {
                await _databaseService.DeleteById<Construct>(constructId);
            }
        }

        private async void AddOrUpdateConstructInConstructs(Construct construct)
        {
            if (await _databaseService.ExistsById<Construct>(construct.Id))
            {
                await _databaseService.UpsertById(construct, construct.Id);
            }
            else
            {
                await _databaseService.InsertDocument(construct);
            }
        }

        private async void UpdateConstructMovementInConstructs(int constructId, double x, double y, int z)
        {
            if (await _databaseService.FindById<Construct>(constructId) is Construct construct)
            {
                var update = Builders<Construct>.Update.Set(x => x.Coordinate.X, x).Set(x => x.Coordinate.Y, y).Set(x => x.Coordinate.Z, z);
                await _databaseService.UpdateById(update, construct.Id);
            }
        }

        #endregion
    }
}