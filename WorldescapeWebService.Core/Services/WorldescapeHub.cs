using LiteDB;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Worldescape.Shared.Entities;
using Worldescape.Shared.Requests;
using WorldescapeWebService.Core.Contracts.Services;

namespace WorldescapeWebService.Core.Services;

public class WorldescapeHub : Hub<IWorldescapeHub>
{
    #region Fields

    private readonly ILogger<WorldescapeHub> _logger;

    //<WorldId, InWorld> this is just for checking against signalR groups
    private static ConcurrentDictionary<int, InWorld> OnlineWorlds = new();

    #endregion

    #region Ctor

    public WorldescapeHub(ILogger<WorldescapeHub> logger)
    {
        _logger = logger;
    }

    #endregion

    #region Common

    private static string GetUsersGroup(Avatar newUser)
    {
        return newUser.World.Id.ToString();
    }

    private Avatar GetCallingUser(int userId = 0)
    {
        if (userId > 0)
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"Worldescape.db"))
            {
                // Get Avatars collection
                var col = db.GetCollection<Avatar>("Avatars");

                var result = col.FindOne(x => x.Id == userId);

                return result;
            }
        }
        else
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"Worldescape.db"))
            {
                // Get Avatars collection
                var col = db.GetCollection<Avatar>("Avatars");

                var result = col.FindOne(x => x.ConnectionId == Context.ConnectionId);

                return result;
            }
        }
    }

    private string GetUserConnectionId(int userId)
    {
        // Open database (or create if doesn't exist)
        using (var db = new LiteDatabase(@"Worldescape.db"))
        {
            // Get Avatars collection
            var col = db.GetCollection<Avatar>("Avatars");

            var result = col.FindOne(x => x.Id == userId);

            return result.ConnectionId;
        }
    }

    #endregion

    #region Connection

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Avatar user = GetCallingUser();
        if (user != null)
        {
            UpdateAvatarDisconnectionTime(user.Id, DateTime.Now);

            Clients.OthersInGroup(GetUsersGroup(user)).AvatarDisconnection(user.Id);
            _logger.LogInformation($"<> {user.Id} OnDisconnectedAsync - {DateTime.Now}");
        }
        return base.OnDisconnectedAsync(exception);
    }

    public override Task OnConnectedAsync()
    {
        Avatar user = GetCallingUser();
        if (user != null)
        {
            UpdateAvatarReconnectionTime(user.Id, DateTime.Now);

            Clients.OthersInGroup(GetUsersGroup(user)).AvatarReconnection(user.Id);
            _logger.LogInformation($"<> {user.Id} OnConnectedAsync- {DateTime.Now}");
        }
        return base.OnConnectedAsync();
    }

    #endregion

    #region Session

    public Tuple<Avatar[], Construct[]> Login(Avatar avatar) // Get a list of online avatars and constructs
    {
        // Open database (or create if doesn't exist)
        using (var db = new LiteDatabase(@"Worldescape.db"))
        {
            // Get Avatars collection
            var colAvatars = db.GetCollection<Avatar>("Avatars");

            // If an existing avatar doesn't exist
            if (!colAvatars.Exists(x => x.Id == avatar.Id))
            {
                var minValue = DateTime.MinValue;

                // Delete inactive avatars who have remained inactive for more than a minute
                var concurrentAvatars = colAvatars.Find(x => x.World.Id == avatar.World.Id && x.Session != null && x.Session.DisconnectionTime != minValue);

                foreach (var inAvatar in concurrentAvatars)
                {
                    TimeSpan diff = DateTime.Now - inAvatar.Session.DisconnectionTime;

                    // TODO: Must fix auto delete of avatar
                    if (diff.TotalMinutes >= TimeSpan.FromMinutes(1).TotalMinutes)
                    {
                        colAvatars.Delete(inAvatar.Id);
                    }
                }

                avatar.Session = new UserSession() { ReconnectionTime = DateTime.UtcNow };
                avatar.ConnectionId = Context.ConnectionId;

                // Save the new avatar
                BsonValue? id = colAvatars.Insert(avatar);

                // If not saved then return null
                if (id.IsNull || id.AsInt32 <= 0)
                {
                    return null;
                }

                // Check if a world exists or not in SignalR groups
                if (!OnlineWorlds.ContainsKey(avatar.World.Id))
                {
                    // If the group doesn't exist in hub add it
                    Groups.AddToGroupAsync(Context.ConnectionId, avatar.World.Id.ToString());

                    OnlineWorlds.TryAdd(avatar.World.Id, avatar.World);
                }

                Clients.OthersInGroup(GetUsersGroup(avatar)).AvatarLogin(avatar);

                _logger.LogInformation($"++ {avatar.Id} Login-> World {avatar.World.Id} - {DateTime.Now}");

                // Get Constructs collection
                var colConstructs = db.GetCollection<Construct>("Constructs");

                // Find all constructs from the calling avatar's world
                var constructs = colConstructs.Find(x => x.World.Id == avatar.World.Id)?.ToArray();

                // Find all avatars from the calling avatar's world
                var avatars = colAvatars.Find(x => x.World.Id == avatar.World.Id)?.ToArray();

                // Return the curated avatars and constructs
                return new Tuple<Avatar[], Construct[]>(avatars ?? new Avatar[] { }, constructs ?? new Construct[] { });
            }

            return null;
        }
    }

    public void Logout()
    {
        Avatar avatar = GetCallingUser();

        if (avatar != null && !avatar.IsEmpty())
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"Worldescape.db"))
            {
                // Get Avatars collection
                var col = db.GetCollection<Avatar>("Avatars");

                if (col.Exists(x => x.Id == avatar.Id))
                {
                    col.Delete(avatar.Id);

                    Clients.OthersInGroup(avatar.World.Id.ToString()).AvatarLogout(avatar.Id);

                    _logger.LogInformation($"-- {avatar.Id} Logout-> World {avatar.World.Id} - {DateTime.Now}");
                }
            }
        }
    }

    #endregion

    #region Texting
    public void BroadcastTextMessage(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            Avatar sender = GetCallingUser();

            Clients.OthersInGroup(GetUsersGroup(sender)).BroadcastTextMessage(sender.Id, message);

            _logger.LogInformation($"<> {sender.Id} BroadcastTextMessage - {DateTime.Now}");
        }
    }

    public void BroadcastImageMessage(byte[] img)
    {
        if (img != null)
        {
            Avatar sender = GetCallingUser();

            Clients.OthersInGroup(GetUsersGroup(sender)).BroadcastPictureMessage(sender.Id, img);

            _logger.LogInformation($"<> {sender.Id} BroadcastImageMessage - {DateTime.Now}");
        }
    }

    public void UnicastTextMessage(int recepientId, string message)
    {
        Avatar sender = GetCallingUser();
        string recipientConnectionId = GetUserConnectionId(recepientId);

        if (sender != null
            && recepientId != sender.Id
            && !string.IsNullOrEmpty(message))
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"Worldescape.db"))
            {
                // Get Avatars collection
                var col = db.GetCollection<Avatar>("Avatars");

                if (col.Exists(x => x.Id == recepientId && x.ConnectionId == recipientConnectionId))
                {
                    Clients.Client(recipientConnectionId).UnicastTextMessage(sender.Id, message);

                    _logger.LogInformation($"<> {sender.Id} UnicastTextMessage - {DateTime.Now}");
                }
            }
        }
    }

    public void UnicastImageMessage(int recepientId, byte[] img)
    {
        Avatar sender = GetCallingUser();
        string recipientConnectionId = GetUserConnectionId(recepientId);

        if (sender != null
            && recepientId != sender.Id
            && img != null)
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"Worldescape.db"))
            {
                // Get Avatars collection
                var col = db.GetCollection<Avatar>("Avatars");

                if (col.Exists(x => x.Id == recepientId && x.ConnectionId == recipientConnectionId))
                {
                    Clients.Client(recipientConnectionId).UnicastPictureMessage(sender.Id, img);

                    _logger.LogInformation($"<> {sender.Id} UnicastImageMessage - {DateTime.Now}");
                }
            }
        }
    }

    public void Typing(int recepientId)
    {
        if (recepientId <= 0)
        {
            return;
        }

        Avatar sender = GetCallingUser();
        string recipientConnectionId = GetUserConnectionId(recepientId);

        // Open database (or create if doesn't exist)
        using (var db = new LiteDatabase(@"Worldescape.db"))
        {
            // Get Avatars collection
            var col = db.GetCollection<Avatar>("Avatars");

            if (col.Exists(x => x.Id == recepientId && x.ConnectionId == recipientConnectionId))
            {
                Clients.Client(recipientConnectionId).AvatarTyping(sender.Id);

                _logger.LogInformation($"<> {sender.Id} Typing - {DateTime.Now}");
            }
        }
    }

    public void BroadcastTyping()
    {
        Avatar sender = GetCallingUser();

        Clients.OthersInGroup(GetUsersGroup(sender)).AvatarBroadcastTyping(sender.Id);
        _logger.LogInformation($"<> {sender.Id} BroadcastTyping - {DateTime.Now}");
    }

    #endregion

    #region Avatar

    public void BroadcastAvatarMovement(BroadcastAvatarMovementRequest request)
    {
        if (request.AvatarId > 0 && request.Coordinate != null)
        {
            Clients.OthersInGroup(GetUsersGroup(GetCallingUser(request.AvatarId))).BroadcastAvatarMovement(request);

            UpdateAvatarMovement(request.AvatarId, request.Coordinate);

            _logger.LogInformation($"<> {request.AvatarId} BroadcastAvatarMovement - {DateTime.Now}");
        }
    }

    public void BroadcastAvatarActivityStatus(BroadcastAvatarActivityStatusRequest request)
    {
        if (request.AvatarId > 0)
        {
            Clients.OthersInGroup(GetUsersGroup(GetCallingUser(request.AvatarId))).BroadcastAvatarActivityStatus(request);

            UpdateAvatarActivityStatus(request.AvatarId, request.ActivityStatus);

            _logger.LogInformation($"<> {request.AvatarId} BroadcastAvatarActivityStatus - {DateTime.Now}");
        }
    }

    #endregion

    #region Construct

    public void BroadcastConstruct(Construct construct)
    {
        if (construct.Id > 0)
        {
            Clients.OthersInGroup(GetUsersGroup(GetCallingUser())).BroadcastConstruct(construct);
            AddOrUpdateConstructInConstructs(construct);
            _logger.LogInformation($"<> {construct.Id} BroadcastConstruct - {DateTime.Now}");
        }
    }

    public void BroadcastConstructs(Construct[] constructs)
    {
        if (constructs != null && constructs.Any())
        {
            Clients.OthersInGroup(GetUsersGroup(GetCallingUser())).BroadcastConstructs(constructs);
            foreach (var construct in constructs)
            {
                AddOrUpdateConstructInConstructs(construct);
            }
            _logger.LogInformation($"<> {constructs.Count()} BroadcastConstructs - {DateTime.Now}");
        }
    }

    public void RemoveConstruct(int constructId)
    {
        if (constructId > 0)
        {
            Clients.OthersInGroup(GetUsersGroup(GetCallingUser())).RemoveConstruct(constructId);
            RemoveConstructFromConstructs(constructId);
            _logger.LogInformation($"<> {constructId} RemoveConstruct - {DateTime.Now}");
        }
    }

    public void RemoveConstructs(int[] constructIds)
    {
        if (constructIds != null && constructIds.Any())
        {
            Clients.OthersInGroup(GetUsersGroup(GetCallingUser())).RemoveConstructs(constructIds);

            foreach (var constructId in constructIds)
            {
                RemoveConstructFromConstructs(constructId);
            }

            _logger.LogInformation($"<> {constructIds.Count()} RemoveConstructs - {DateTime.Now}");
        }
    }

    public void BroadcastConstructPlacement(int constructId, int z)
    {
        if (constructId > 0)
        {
            Clients.OthersInGroup(GetUsersGroup(GetCallingUser())).BroadcastConstructPlacement(constructId, z);
            UpdateConstructPlacementInConstructs(constructId, z);
            _logger.LogInformation($"<> {constructId} BroadcastConstructPlacement - {DateTime.Now}");
        }
    }

    public void BroadcastConstructRotation(int constructId, float rotation)
    {
        if (constructId > 0)
        {
            Clients.OthersInGroup(GetUsersGroup(GetCallingUser())).BroadcastConstructRotation(constructId, rotation);
            UpdateConstructRotationInConstructs(constructId, rotation);
            _logger.LogInformation($"<> {constructId} BroadcastConstructRotation - {DateTime.Now}");
        }
    }

    public void BroadcastConstructRotations(ConcurrentDictionary<int, float> constructIds)
    {
        if (constructIds != null)
        {
            Clients.OthersInGroup(GetUsersGroup(GetCallingUser())).BroadcastConstructRotations(constructIds);

            foreach (var constructId in constructIds)
            {
                UpdateConstructRotationInConstructs(constructId.Key, constructId.Value);
            }

            _logger.LogInformation($"<> {constructIds.Count()} BroadcastConstructRotations - {DateTime.Now}");
        }
    }

    public void BroadcastConstructScale(int constructId, float scale)
    {
        if (constructId > 0)
        {
            Clients.OthersInGroup(GetUsersGroup(GetCallingUser())).BroadcastConstructScale(constructId, scale);
            UpdateConstructScaleInConstructs(constructId, scale);
            _logger.LogInformation($"<> {constructId} BroadcastConstructScale - {DateTime.Now}");
        }
    }

    public void BroadcastConstructScales(int[] constructIds, float scale)
    {
        if (constructIds != null && constructIds.Any())
        {
            Clients.OthersInGroup(GetUsersGroup(GetCallingUser())).BroadcastConstructScales(constructIds, scale);

            foreach (var constructId in constructIds)
            {
                UpdateConstructScaleInConstructs(constructId, scale);
            }

            _logger.LogInformation($"<> {constructIds.Count()} BroadcastConstructScales - {DateTime.Now}");
        }
    }

    public void BroadcastConstructMovement(int constructId, double x, double y, int z)
    {
        if (constructId > 0)
        {
            Clients.OthersInGroup(GetUsersGroup(GetCallingUser())).BroadcastConstructMovement(constructId, x, y, z);
            UpdateConstructMovementInConstructs(constructId, x, y, z);
            _logger.LogInformation($"<> {constructId} BroadcastConstructMovement - {DateTime.Now}");
        }
    }

    #endregion

    #region Connected Avatars

    private void UpdateAvatarReconnectionTime(int avatarId, DateTime reconnectionTime)
    {
        var connectionId = GetUserConnectionId(avatarId);

        // Open database (or create if doesn't exist)
        using (var db = new LiteDatabase(@"Worldescape.db"))
        {
            // Get Avatars collection
            var col = db.GetCollection<Avatar>("Avatars");

            if (col.Exists(x => x.Id == avatarId && x.ConnectionId == connectionId))
            {
                var result = col.FindOne(x => x.Id == avatarId && x.ConnectionId == connectionId);

                result.Session.ReconnectionTime = reconnectionTime;

                col.Update(result);
            }
        }
    }

    private void UpdateAvatarDisconnectionTime(int avatarId, DateTime disconnectionTime)
    {
        var connectionId = GetUserConnectionId(avatarId);

        // Open database (or create if doesn't exist)
        using (var db = new LiteDatabase(@"Worldescape.db"))
        {
            // Get Avatars collection
            var col = db.GetCollection<Avatar>("Avatars");

            if (col.Exists(x => x.Id == avatarId && x.ConnectionId == connectionId))
            {
                var result = col.FindOne(x => x.Id == avatarId && x.ConnectionId == connectionId);

                result.Session.DisconnectionTime = disconnectionTime;

                col.Update(result);
            }
        }
    }

    private void UpdateAvatarActivityStatus(int avatarId, ActivityStatus activityStatus)
    {
        var connectionId = GetUserConnectionId(avatarId);

        // Open database (or create if doesn't exist)
        using (var db = new LiteDatabase(@"Worldescape.db"))
        {
            // Get Avatars collection
            var col = db.GetCollection<Avatar>("Avatars");

            if (col.Exists(x => x.Id == avatarId && x.ConnectionId == connectionId))
            {
                var result = col.FindOne(x => x.Id == avatarId && x.ConnectionId == connectionId);

                result.ActivityStatus = activityStatus;

                col.Update(result);
            }
        }
    }

    private void UpdateAvatarMovement(int avatarId, Coordinate coordinate)
    {
        var connectionId = GetUserConnectionId(avatarId);

        // Open database (or create if doesn't exist)
        using (var db = new LiteDatabase(@"Worldescape.db"))
        {
            // Get Avatars collection
            var col = db.GetCollection<Avatar>("Avatars");

            if (col.Exists(x => x.Id == avatarId && x.ConnectionId == connectionId))
            {
                var result = col.FindOne(x => x.Id == avatarId && x.ConnectionId == connectionId);

                result.Coordinate = coordinate;

                col.Update(result);
            }
        }
    }

    #endregion

    #region Connected Constructs

    private void UpdateConstructPlacementInConstructs(int constructId, int z)
    {
        // Open database (or create if doesn't exist)
        using (var db = new LiteDatabase(@"Worldescape.db"))
        {
            // Get Constructs collection
            var col = db.GetCollection<Construct>("Constructs");

            if (col.Exists(x => x.Id == constructId))
            {
                var result = col.FindOne(x => x.Id == constructId);

                result.Coordinate.Z = z;

                col.Update(result);
            }
        }
    }

    private void UpdateConstructRotationInConstructs(int constructId, float rotation)
    {
        // Open database (or create if doesn't exist)
        using (var db = new LiteDatabase(@"Worldescape.db"))
        {
            // Get Constructs collection
            var col = db.GetCollection<Construct>("Constructs");

            if (col.Exists(x => x.Id == constructId))
            {
                var result = col.FindOne(x => x.Id == constructId);

                result.Rotation = rotation;

                col.Update(result);
            }
        }
    }

    private void UpdateConstructScaleInConstructs(int constructId, float scale)
    {
        // Open database (or create if doesn't exist)
        using (var db = new LiteDatabase(@"Worldescape.db"))
        {
            // Get Constructs collection
            var col = db.GetCollection<Construct>("Constructs");

            if (col.Exists(x => x.Id == constructId))
            {
                var result = col.FindOne(x => x.Id == constructId);

                result.Scale = scale;

                col.Update(result);
            }
        }
    }

    private void RemoveConstructFromConstructs(int constructId)
    {
        // Open database (or create if doesn't exist)
        using (var db = new LiteDatabase(@"Worldescape.db"))
        {
            // Get Constructs collection
            var col = db.GetCollection<Construct>("Constructs");

            if (col.Exists(x => x.Id == constructId))
            {
                col.Delete(constructId);
            }
        }
    }

    private void AddOrUpdateConstructInConstructs(Construct construct)
    {
        // Open database (or create if doesn't exist)
        using (var db = new LiteDatabase(@"Worldescape.db"))
        {
            // Get Constructs collection
            var col = db.GetCollection<Construct>("Constructs");

            if (col.Exists(x => x.Id == construct.Id))
            {
                var result = col.FindOne(x => x.Id == construct.Id);

                result = construct;

                col.Update(result);
            }
            else
            {
                col.Insert(construct);
            }
        }
    }

    private void UpdateConstructMovementInConstructs(int constructId, double x, double y, int z)
    {
        // Open database (or create if doesn't exist)
        using (var db = new LiteDatabase(@"Worldescape.db"))
        {
            // Get Constructs collection
            var col = db.GetCollection<Construct>("Constructs");

            if (col.Exists(x => x.Id == constructId))
            {
                var result = col.FindOne(x => x.Id == constructId);

                result.Coordinate.X = x;
                result.Coordinate.Y = y;
                result.Coordinate.Z = z;

                col.Update(result);
            }
        }
    }

    #endregion
}
