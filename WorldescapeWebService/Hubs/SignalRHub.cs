using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Worldescape.Common;
using Worldescape.Database;

namespace WorldescapeWebService
{
    /// <summary>
    /// Provides access to server side signalR hub methods. New ConnectionId is generated per user login.
    /// </summary>
    public class SignalRHub : Hub
    {
        #region Fields

        readonly ILogger<SignalRHub> _logger;
        readonly DatabaseService _databaseService;

        #endregion

        #region Ctor

        public SignalRHub(
            ILogger<SignalRHub> logger,
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

        private async Task<Avatar> GetCallingUser(int userId = 0)
        {
            if (userId > 0)
            {
                var avatar = await _databaseService.FindById<Avatar>(userId);
                return avatar;
            }
            else
            {
                var avatar = await _databaseService.FindOne(Builders<Avatar>.Filter.Eq(x => x.Session.ConnectionId, Context.ConnectionId));
                return avatar;
            }
        }

        private async Task<string> GetUserConnectionId(int userId)
        {
            var avatar = await _databaseService.FindById<Avatar>(userId);
            return avatar.Session.ConnectionId;
        }

        #endregion

        #region Hub Methods

        #region Hub Connectivity

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Avatar avatar = await GetCallingUser();
            if (avatar != null)
            {
                await UpdateAvatarDisconnectionTime(avatar.Id, DateTime.Now);
                await UpdateAvatarActivityStatus(avatar.Id, (int)ActivityStatus.Away);

                var group = GetUsersGroup(avatar);

                await Clients.OthersInGroup(group).SendAsync(Constants.AvatarDisconnected, avatar.Id);

                _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {avatar.Id} OnDisconnectedAsync - {DateTime.Now} World: {group}");
            }
            /*return*/
            await base.OnDisconnectedAsync(exception);
        }

        public override async Task OnConnectedAsync()
        {
            Avatar avatar = await GetCallingUser();
            if (avatar != null)
            {
                await UpdateAvatarReconnectionTime(avatar.Id, DateTime.Now);
                await UpdateAvatarActivityStatus(avatar.Id, (int)ActivityStatus.Idle);

                var group = GetUsersGroup(avatar);

                await Clients.OthersInGroup(group).SendAsync(Constants.AvatarReconnected, avatar.Id);

                _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {avatar.Id} OnConnectedAsync- {DateTime.Now} World: {group}");
            }
            /*return*/
            await base.OnConnectedAsync();
        }

        #endregion

        #region Avatar Connectivity

        [HubMethodName(Constants.Login)]
        public async Task<HubLoginResponse> Login(Avatar avatar)
        {
            // If an existing avatar
            if (await AvatarExists(avatar))
            {
                var existingAvatar = await GetAvatar(avatar.Id);

                // If user is logging in to a world previously logged in, then make him appear where he left off
                if (avatar.World.Id == existingAvatar.World.Id)
                {
                    avatar.Coordinate = existingAvatar.Coordinate;
                    avatar.ActivityStatus = existingAvatar.ActivityStatus;
                }

                avatar.Session = new AvatarSession()
                {
                    ReconnectionTime = DateTime.UtcNow,
                    ConnectionId = Context.ConnectionId
                };

                // Remove old instance
                await RemoveAvatar(avatar);

                // Update world population count from already logged in user's world
                await UpdateWorldPopulationCount(existingAvatar.World.Id);

                // Add new instance            
                await AddAvatar(avatar);

                // Update world population count for the world the user is logging in
                await UpdateWorldPopulationCount(avatar.World.Id);

                var group = GetUsersGroup(avatar);
                await Groups.AddToGroupAsync(Context.ConnectionId, group);

                await Clients.OthersInGroup(group).SendAsync(Constants.AvatarLoggedIn, avatar);

                _logger.LogInformation($"++ ConnectionId: {Context.ConnectionId} AvatarId: {avatar.Id} Login-> World {avatar.World.Id} - {DateTime.Now} World: {group}");
            }
            else
            {
                avatar.Session = new AvatarSession()
                {
                    ReconnectionTime = DateTime.UtcNow,
                    ConnectionId = Context.ConnectionId
                };

                // Save the new avatar                
                await AddAvatar(avatar);

                // Update world population count
                await UpdateWorldPopulationCount(avatar.World.Id);

                var group = avatar.World.Id.ToString();
                await Groups.AddToGroupAsync(Context.ConnectionId, group);

                await Clients.OthersInGroup(group).SendAsync(Constants.AvatarLoggedIn, avatar);
                _logger.LogInformation($"++ ConnectionId: {Context.ConnectionId} AvatarId: {avatar.Id} Login-> World {avatar.World.Id} - {DateTime.Now} World: {group}");
            }

            // Find own avatar
            var newSelf = await GetAvatar(avatar.Id);

            // Return the curated avatar
            return new HubLoginResponse()
            {
                Avatar = newSelf
            };
        }

        [HubMethodName(Constants.Logout)]
        public async Task Logout()
        {
            Avatar avatar = await GetCallingUser();

            if (avatar != null && !avatar.IsEmpty())
            {
                await RemoveAvatar(avatar);

                // Update world population count
                await UpdateWorldPopulationCount(avatar.World.Id);

                var group = avatar.World.Id.ToString();
                await Clients.OthersInGroup(group).SendAsync(Constants.AvatarLoggedOut, avatar.Id);

                _logger.LogInformation($"-- ConnectionId: {Context.ConnectionId} AvatarId: {avatar.Id} Logout-> WorldId {avatar.World.Id} - {DateTime.Now} World: {group}");
            }
        }

        #endregion

        #region Messaging

        [HubMethodName(Constants.BroadcastMessage)]
        public async Task BroadcastTextMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Avatar sender = await GetCallingUser();

                var group = GetUsersGroup(sender);
                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedMessage, sender.Id, message);

                _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {sender.Id} BroadcastTextMessage - {DateTime.Now} World: {group}");
            }
        }


        [HubMethodName(Constants.BroadcastImageMessage)]
        public async Task BroadcastImageMessage(byte[] img)
        {
            if (img != null)
            {
                Avatar sender = await GetCallingUser();

                var group = GetUsersGroup(sender);
                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedPictureMessage, sender.Id, img);

                _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {sender.Id} BroadcastImageMessage - {DateTime.Now} World: {group}");
            }
        }


        [HubMethodName(Constants.UnicastMessage)]
        public async Task UnicastTextMessage(int recepientId, string message)
        {
            Avatar sender = await GetCallingUser();
            string recipientConnectionId = await GetUserConnectionId(recepientId);

            if (sender != null
                && recepientId != sender.Id
                && !string.IsNullOrEmpty(message))
            {
                if (await AvatarExists(recepientId, recipientConnectionId))
                {
                    await Clients.Client(recipientConnectionId).SendAsync(Constants.UnicastedMessage, sender.Id, message);

                    _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {sender.Id} UnicastTextMessage - {DateTime.Now} World: {sender.World.Id}");
                }
            }
        }


        [HubMethodName(Constants.UnicastImageMessage)]
        public async Task UnicastImageMessage(int recepientId, byte[] img)
        {
            Avatar sender = await GetCallingUser();
            string recipientConnectionId = await GetUserConnectionId(recepientId);

            if (sender != null
                && recepientId != sender.Id
                && img != null)
            {
                if (await AvatarExists(recepientId, recipientConnectionId))
                {
                    await Clients.Client(recipientConnectionId).SendAsync(Constants.UnicastedPictureMessage, sender.Id, img);

                    _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {sender.Id} UnicastImageMessage - {DateTime.Now} World: {sender.World.Id}");
                }
            }
        }


        [HubMethodName(Constants.Typing)]
        public async Task Typing(int recepientId)
        {
            if (recepientId <= 0)
            {
                return;
            }

            Avatar sender = await GetCallingUser();
            string recipientConnectionId = await GetUserConnectionId(recepientId);

            if (await AvatarExists(recepientId, recipientConnectionId))
            {
                await Clients.Client(recipientConnectionId).SendAsync(Constants.AvatarTyped, sender.Id);

                _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {sender.Id} Typing - {DateTime.Now} World: {sender.World.Id}");
            }
        }


        [HubMethodName(Constants.BroadcastTyping)]
        public async Task BroadcastTyping()
        {
            Avatar sender = await GetCallingUser();

            var group = GetUsersGroup(sender);

            await Clients.OthersInGroup(group).SendAsync(Constants.AvatarBroadcastTyped, sender.Id);

            _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {sender.Id} BroadcastTyping - {DateTime.Now} World: {group}");
        }

        #endregion

        #region Avatar World Events

        [HubMethodName(Constants.BroadcastAvatarMovement)]
        public async Task BroadcastAvatarMovement(int avatarId, double x, double y, int z)
        {
            if (avatarId > 0)
            {
                var group = GetUsersGroup(await GetCallingUser(avatarId));

                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedAvatarMovement, avatarId, x, y, z);

                await UpdateAvatarMovement(avatarId, x, y, z);

                _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {avatarId} BroadcastAvatarMovement - {DateTime.Now} World: {group}");
            }
        }


        [HubMethodName(Constants.BroadcastAvatarActivityStatus)]
        public async Task BroadcastAvatarActivityStatus(int avatarId, int activityStatus)
        {
            if (avatarId > 0)
            {
                var group = await GetCallingUser(avatarId);
                await Clients.OthersInGroup(GetUsersGroup(group)).SendAsync(Constants.BroadcastedAvatarActivityStatus, avatarId, activityStatus);

                await UpdateAvatarActivityStatus(avatarId, activityStatus);

                _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {avatarId} BroadcastAvatarActivityStatus - {DateTime.Now} World: {group}");
            }
        }

        #endregion

        #region Construct World Events

        [HubMethodName(Constants.BroadcastConstruct)]
        public async Task BroadcastConstruct(Construct construct)
        {
            if (construct.Id > 0)
            {
                var group = GetUsersGroup(await GetCallingUser());
                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedConstruct, construct);

                await AddOrUpdateConstructInConstructs(construct);
                _logger.LogInformation($"<> {construct.Id} BroadcastConstruct - {DateTime.Now} World: {group}");
            }
        }


        //[HubMethodName(Constants.BroadcastConstructs)]
        //public async void BroadcastConstructs(Construct[] constructs)
        //{
        //    if (constructs != null && constructs.Any())
        //    {
        //        var group = GetUsersGroup(GetCallingUser());

        //        await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedConstructs, constructs);

        //        foreach (var construct in constructs)
        //        {
        //            AddOrUpdateConstructInConstructs(construct);
        //        }
        //        _logger.LogInformation($"<> {constructs.Count()} BroadcastConstructs - {DateTime.Now} World: {group}");
        //    }
        //}


        [HubMethodName(Constants.RemoveConstruct)]
        public async Task RemoveConstruct(int constructId)
        {
            if (constructId > 0)
            {
                var group = GetUsersGroup(await GetCallingUser());

                await Clients.OthersInGroup(group).SendAsync(Constants.RemovedConstruct, constructId);

                await RemoveConstructFromConstructs(constructId);
                _logger.LogInformation($"<> Construct: {constructId} RemoveConstruct - {DateTime.Now} World: {group}");
            }
        }


        //[HubMethodName(Constants.RemoveConstructs)]
        //public async void RemoveConstructs(int[] constructIds)
        //{
        //    if (constructIds != null && constructIds.Any())
        //    {
        //        var group = GetUsersGroup(GetCallingUser());

        //        await Clients.OthersInGroup(group).SendAsync(Constants.RemovedConstructs, constructIds);

        //        foreach (var constructId in constructIds)
        //        {
        //            RemoveConstructFromConstructs(constructId);
        //        }

        //        _logger.LogInformation($"<> {constructIds.Count()} RemoveConstructs - {DateTime.Now} World: {group}");
        //    }
        //}


        [HubMethodName(Constants.BroadcastConstructPlacement)]
        public async Task BroadcastConstructPlacement(int constructId, int z)
        {
            if (constructId > 0)
            {
                var group = GetUsersGroup(await GetCallingUser());

                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedConstructPlacement, constructId, z);

                await UpdateConstructPlacementInConstructs(constructId, z);
                _logger.LogInformation($"<> Construct: {constructId} BroadcastConstructPlacement - {DateTime.Now} World: {group}");
            }
        }


        [HubMethodName(Constants.BroadcastConstructRotation)]
        public async Task BroadcastConstructRotation(int constructId, float rotation)
        {
            if (constructId > 0)
            {
                var group = GetUsersGroup(await GetCallingUser());
                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedConstructRotation, constructId, rotation);

                await UpdateConstructRotationInConstructs(constructId, rotation);
                _logger.LogInformation($"<> Construct: {constructId} BroadcastConstructRotation - {DateTime.Now} World: {group}");
            }
        }


        //[HubMethodName(Constants.BroadcastConstructRotations)]
        //public async void BroadcastConstructRotations(ConcurrentDictionary<int, float> constructIds)
        //{
        //    if (constructIds != null)
        //    {
        //        var group = GetUsersGroup(GetCallingUser());
        //        await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedConstructRotations, constructIds);

        //        foreach (var constructId in constructIds)
        //        {
        //            UpdateConstructRotationInConstructs(constructId.Key, constructId.Value);
        //        }

        //        _logger.LogInformation($"<> {constructIds.Count()} BroadcastConstructRotations - {DateTime.Now} World: {group}");
        //    }
        //}


        [HubMethodName(Constants.BroadcastConstructScale)]
        public async Task BroadcastConstructScale(int constructId, float scale)
        {
            if (constructId > 0)
            {
                var group = GetUsersGroup(await GetCallingUser());

                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedConstructScale, constructId, scale);

                await UpdateConstructScaleInConstructs(constructId, scale);
                _logger.LogInformation($"<> Construct: {constructId} BroadcastConstructScale - {DateTime.Now} World: {group}");
            }
        }


        //[HubMethodName(Constants.BroadcastConstructScales)]
        //public async void BroadcastConstructScales(int[] constructIds, float scale)
        //{
        //    if (constructIds != null && constructIds.Any())
        //    {
        //        var group = GetUsersGroup(GetCallingUser());

        //        await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedConstructScales, constructIds, scale);

        //        foreach (var constructId in constructIds)
        //        {
        //            UpdateConstructScaleInConstructs(constructId, scale);
        //        }

        //        _logger.LogInformation($"<> {constructIds.Count()} BroadcastConstructScales - {DateTime.Now} World: {group}");
        //    }
        //}


        [HubMethodName(Constants.BroadcastConstructMovement)]
        public async Task BroadcastConstructMovement(int constructId, double x, double y, int z)
        {
            if (constructId > 0)
            {
                var group = GetUsersGroup(await GetCallingUser());

                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedConstructMovement, constructId, x, y, z);

                await UpdateConstructMovementInConstructs(constructId, x, y, z);
                _logger.LogInformation($"<> Construct: Construct: {constructId} BroadcastConstructMovement - {DateTime.Now} World: {group}");
            }
        }

        #endregion

        #region Portal World Events

        [HubMethodName(Constants.BroadcastPortal)]
        public async Task BroadcastPortal(Portal portal)
        {
            if (portal.Id > 0)
            {
                var group = GetUsersGroup(await GetCallingUser());
                await Clients.OthersInGroup(group).SendAsync(Constants.BroadcastedPortal, portal);

                //await AddOrUpdatePortalInPortals(portal);
                _logger.LogInformation($"<> {portal.Id} BroadcastPortal - {DateTime.Now} World: {group}");
            }
        }

        #endregion

        #endregion

        #region Concurrent Avatars

        private async Task<Avatar> GetAvatar(int avatarId)
        {
            return await _databaseService.FindById<Avatar>(avatarId);
        }

        private async Task<bool> AvatarExists(int avatarId, string connectionId)
        {
            return await _databaseService.Exists(Builders<Avatar>.Filter.And(Builders<Avatar>.Filter.Eq(x => x.Id, avatarId), Builders<Avatar>.Filter.Eq(x => x.Session.ConnectionId, connectionId)));
        }

        private async Task<bool> AvatarExists(Avatar avatar)
        {
            return await _databaseService.ExistsById<Avatar>(avatar.Id);
        }

        private async Task AddAvatar(Avatar avatar)
        {
            if (await _databaseService.ExistsById<Avatar>(avatar.Id))
            {
                await _databaseService.UpsertById(avatar, avatar.Id);
            }
            else
            {
                await _databaseService.InsertDocument(avatar);
            }
        }

        private async Task RemoveAvatar(Avatar avatar)
        {
            if (await _databaseService.ExistsById<Avatar>(avatar.Id))
            {
                await _databaseService.DeleteById<Avatar>(avatar.Id);
            }
        }

        private async Task UpdateAvatarReconnectionTime(int avatarId, DateTime reconnectionTime)
        {
            if (await _databaseService.FindById<Avatar>(avatarId) is Avatar avatar)
            {
                var update = Builders<Avatar>.Update.Set(x => x.Session.ReconnectionTime, reconnectionTime);
                await _databaseService.UpdateById(update, avatar.Id);
            }
        }

        private async Task UpdateAvatarDisconnectionTime(int avatarId, DateTime disconnectionTime)
        {
            if (await _databaseService.FindById<Avatar>(avatarId) is Avatar avatar)
            {
                var update = Builders<Avatar>.Update.Set(x => x.Session.DisconnectionTime, disconnectionTime);
                await _databaseService.UpdateById(update, avatar.Id);
            }
        }

        private async Task UpdateAvatarActivityStatus(int avatarId, int activityStatus)
        {
            if (await _databaseService.FindById<Avatar>(avatarId) is Avatar avatar)
            {
                var update = Builders<Avatar>.Update.Set(x => x.ActivityStatus, (ActivityStatus)activityStatus);
                await _databaseService.UpdateById(update, avatar.Id);
            }
        }

        private async Task UpdateAvatarMovement(int avatarId, double x, double y, int z)
        {
            if (await _databaseService.FindById<Avatar>(avatarId) is Avatar avatar)
            {
                var update = Builders<Avatar>.Update.Set(x => x.Coordinate.X, x).Set(x => x.Coordinate.Y, y).Set(x => x.Coordinate.Z, z);
                await _databaseService.UpdateById(update, avatar.Id);
            }
        }

        #endregion

        #region Concurrent Constructs

        private async Task AddOrUpdateConstructInConstructs(Construct construct)
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

        private async Task RemoveConstructFromConstructs(int constructId)
        {
            if (await _databaseService.ExistsById<Construct>(constructId))
            {
                await _databaseService.DeleteById<Construct>(constructId);
            }
        }

        private async Task UpdateConstructPlacementInConstructs(int constructId, int z)
        {
            if (await _databaseService.FindById<Construct>(constructId) is Construct construct)
            {
                var update = Builders<Construct>.Update.Set(x => x.Coordinate.Z, z);
                await _databaseService.UpdateById(update, construct.Id);
            }
        }

        private async Task UpdateConstructRotationInConstructs(int constructId, float rotation)
        {
            if (await _databaseService.FindById<Construct>(constructId) is Construct construct)
            {
                var update = Builders<Construct>.Update.Set(x => x.Rotation, rotation);
                await _databaseService.UpdateById(update, construct.Id);
            }
        }

        private async Task UpdateConstructScaleInConstructs(int constructId, float scale)
        {
            if (await _databaseService.FindById<Construct>(constructId) is Construct construct)
            {
                var update = Builders<Construct>.Update.Set(x => x.Scale, scale);
                await _databaseService.UpdateById(update, construct.Id);
            }
        }

        private async Task UpdateConstructMovementInConstructs(int constructId, double x, double y, int z)
        {
            if (await _databaseService.FindById<Construct>(constructId) is Construct construct)
            {
                var update = Builders<Construct>.Update.Set(x => x.Coordinate.X, x).Set(x => x.Coordinate.Y, y).Set(x => x.Coordinate.Z, z);
                await _databaseService.UpdateById(update, construct.Id);
            }
        }

        #endregion

        #region Concurrent Worlds

        private async Task UpdateWorldPopulationCount(int worldId)
        {
            if (await _databaseService.FindById<World>(worldId) is World world)
            {
                var count = await _databaseService.CountDocuments(Builders<Avatar>.Filter.Eq(x => x.World.Id, worldId));

                var update = Builders<World>.Update.Set(x => x.PopulationCount, count);
                await _databaseService.UpdateById(update, worldId);
            }
        }

        #endregion
    }
}