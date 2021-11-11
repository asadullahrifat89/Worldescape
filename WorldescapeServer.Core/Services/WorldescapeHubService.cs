﻿using LiteDB;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Worldescape.Core;
using static Worldescape.Core.Enums;

namespace WorldescapeServer.Core;

public class WorldescapeHubService : Hub<IWorldescapeHubService>
{
    #region Fields

    private readonly ILogger<WorldescapeHubService> _logger;

    //<WorldId, InWorld> this is just for checking against signalR groups
    private static ConcurrentDictionary<int, InWorld> OnlineWorlds = new();

    #endregion

    #region Ctor

    public WorldescapeHubService(ILogger<WorldescapeHubService> logger)
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
                // Delete inactive avatars who have remained inactive for more than a minute
                var concurrentAvatars = colAvatars.Find(x => x.World.Id == avatar.World.Id && x.Session != null && x.Session.DisconnectionTime != DateTime.MinValue);

                foreach (var inAvatar in concurrentAvatars)
                {
                    TimeSpan diff = DateTime.Now - inAvatar.Session.DisconnectionTime;

                    if (diff.TotalMinutes >= TimeSpan.FromMinutes(1).TotalMinutes)
                    {
                        colAvatars.Delete(inAvatar.Id);
                    }
                }

                avatar.Session = new UserSession() { ReconnectionTime = DateTime.Now };

                // Save the new avatar
                BsonValue? id = colAvatars.Insert(avatar);

                // If not saved then return null
                if (id.IsNull || id.AsInt32 <= 0)
                {
                    return null;
                }

                // Get Worlds collection
                var colWorlds = db.GetCollection<World>("Worlds");

                // Check if a world exists or not in SignalR groups
                if (!OnlineWorlds.ContainsKey(avatar.World.Id))
                {
                    if (OnlineWorlds.TryAdd(avatar.World.Id, avatar.World))
                    {
                        // If the group doesn't exist in hub add it
                        Groups.AddToGroupAsync(Context.ConnectionId, avatar.World.Id.ToString());
                    }
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

    #endregion
}