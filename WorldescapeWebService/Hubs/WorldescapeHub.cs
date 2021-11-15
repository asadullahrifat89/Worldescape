﻿using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Worldescape.Shared;

namespace WorldescapeWebService;

public class WorldescapeHub : Hub/*Hub<IWorldescapeHub>*/
{
    #region Fields

    private readonly ILogger<WorldescapeHub> _logger;

    //<WorldId, InWorld> this is just for checking against signalR groups
    private static ConcurrentDictionary<int, InWorld> OnlineWorlds = new();

    //<ConnectionId, Avatar>
    private static ConcurrentDictionary<string, Avatar> OnlineAvatars = new();

    //<ConstructId, Construct>
    private static ConcurrentDictionary<int, Construct> OnlineConstructs = new();

    #endregion

    #region Ctor

    public WorldescapeHub(ILogger<WorldescapeHub> logger)
    {
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
            return OnlineAvatars.SingleOrDefault(c => c.Value.Id == userId).Value;
        }
        else
        {
            return OnlineAvatars.SingleOrDefault(c => c.Key == Context.ConnectionId).Value;
        }
    }

    private string GetUserConnectionId(int userId)
    {
        return OnlineAvatars.SingleOrDefault(c => c.Value.Id == userId).Key;
    }

    #endregion

    #region Connection

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Avatar avatar = GetCallingUser();
        if (avatar != null)
        {
            UpdateAvatarDisconnectionTime(avatar.Id, DateTime.Now);

            var group = GetUsersGroup(avatar);

            //Clients.OthersInGroup(group).AvatarDisconnection(avatar.Id);
            Clients.OthersInGroup(group).SendAsync("AvatarDisconnected", avatar.Id);

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

            //Clients.OthersInGroup(group).AvatarReconnection(avatar.Id);
            Clients.OthersInGroup(group).SendAsync("AvatarReconnected", avatar.Id);

            _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {avatar.Id} OnConnectedAsync- {DateTime.Now} World: {group}");
        }
        return base.OnConnectedAsync();
    }

    #endregion

    #region Session

    public HubLoginResponse Login(Avatar avatar)
    {
        // If an existing avatar doesn't exist
        if (!OnlineAvatars.Any(x => x.Value.Id == avatar.Id))
        {
            var minValue = DateTime.MinValue;

            // Delete inactive avatars who have remained inactive for more than a minute
            var concurrentAvatars = OnlineAvatars.Values.Where(x => x.World.Id == avatar.World.Id && x.Session != null && x.Session.DisconnectionTime != minValue);

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
            if (!OnlineAvatars.TryAdd(Context.ConnectionId, avatar))
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

            var group = GetUsersGroup(avatar);

            //Clients.OthersInGroup(group).AvatarLogin(avatar);
            Clients.OthersInGroup(group).SendAsync("AvatarLoggedIn", avatar);

            _logger.LogInformation($"++ ConnectionId: {Context.ConnectionId} AvatarId: {avatar.Id} Login-> World {avatar.World.Id} - {DateTime.Now} World: {group}");


            // Find all constructs from the calling avatar's world
            var constructs = OnlineConstructs.Where(x => x.Value.World.Id == avatar.World.Id)?.Select(z => z.Value).ToArray();

            // Find all avatars from the calling avatar's world
            var avatars = OnlineAvatars.Where(x => x.Value.World.Id == avatar.World.Id)?.Select(z => z.Value).ToArray();


            // Return the curated avatars and constructs
            return new HubLoginResponse() { Avatars = avatars ?? new Avatar[] { }, Constructs = constructs ?? new Construct[] { } };
        }
        else
        {
            // Remove old instance
            OnlineAvatars.TryRemove(OnlineAvatars.FirstOrDefault(x => x.Value.Id == avatar.Id));

            avatar.Session = new UserSession() { ReconnectionTime = DateTime.UtcNow };
            avatar.ConnectionId = Context.ConnectionId;

            // Add new instance            
            OnlineAvatars.TryAdd(Context.ConnectionId, avatar);

            var group = GetUsersGroup(avatar);

            //Clients.OthersInGroup(group).AvatarLogin(avatar);
            Clients.OthersInGroup(group).SendAsync("AvatarLoggedIn", avatar);

            _logger.LogInformation($"++ ConnectionId: {Context.ConnectionId} AvatarId: {avatar.Id} Login-> World {avatar.World.Id} - {DateTime.Now} World: {group}");

            // Find all constructs from the calling avatar's world
            var constructs = OnlineConstructs.Where(x => x.Value.World.Id == avatar.World.Id)?.Select(z => z.Value).ToArray();

            // Find all avatars from the calling avatar's world
            var avatars = OnlineAvatars.Where(x => x.Value.World.Id == avatar.World.Id)?.Select(z => z.Value).ToArray();

            // Return the curated avatars and constructs
            return new HubLoginResponse() { Avatars = avatars ?? new Avatar[] { }, Constructs = constructs ?? new Construct[] { } };
        }
    }

    public void Logout()
    {
        Avatar avatar = GetCallingUser();

        if (avatar != null && !avatar.IsEmpty())
        {
            if (OnlineAvatars.Any(x => x.Value.Id == avatar.Id))
            {
                string connectionId = OnlineAvatars.SingleOrDefault((c) => c.Value.Id == avatar.Id).Key;

                OnlineAvatars.TryRemove(connectionId, out Avatar a);

                var group = avatar.World.Id.ToString();
                //Clients.OthersInGroup(group).AvatarLogout(avatar.Id);
                Clients.OthersInGroup(group).SendAsync("AvatarLoggedOut", avatar.Id);

                _logger.LogInformation($"-- ConnectionId: {Context.ConnectionId} AvatarId: {avatar.Id} Logout-> WorldId {avatar.World.Id} - {DateTime.Now} World: {group}");
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

            var group = GetUsersGroup(sender);
            //Clients.OthersInGroup(group).BroadcastTextMessage(sender.Id, message);
            Clients.OthersInGroup(group).SendAsync("BroadcastedTextMessage", sender.Id, message);

            _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {sender.Id} BroadcastTextMessage - {DateTime.Now} World: {group}");
        }
    }

    public void BroadcastImageMessage(byte[] img)
    {
        if (img != null)
        {
            Avatar sender = GetCallingUser();

            var group = GetUsersGroup(sender);
            //Clients.OthersInGroup(group).BroadcastPictureMessage(sender.Id, img);
            Clients.OthersInGroup(group).SendAsync("BroadcastedPictureMessage", sender.Id, img);

            _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {sender.Id} BroadcastImageMessage - {DateTime.Now} World: {group}");
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
            if (OnlineAvatars.Any(x => x.Value.Id == recepientId && x.Value.ConnectionId == recipientConnectionId))
            {
                //Clients.Client(recipientConnectionId).UnicastTextMessage(sender.Id, message);
                Clients.Client(recipientConnectionId).SendAsync("UnicastedTextMessage", sender.Id, message);

                _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {sender.Id} UnicastTextMessage - {DateTime.Now} World: {sender.World.Id}");
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
            if (OnlineAvatars.Any(x => x.Value.Id == recepientId && x.Value.ConnectionId == recipientConnectionId))
            {
                //Clients.Client(recipientConnectionId).UnicastPictureMessage(sender.Id, img);
                Clients.Client(recipientConnectionId).SendAsync("UnicastedPictureMessage", sender.Id, img);

                _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {sender.Id} UnicastImageMessage - {DateTime.Now} World: {sender.World.Id}");
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

        if (OnlineAvatars.Any(x => x.Value.Id == recepientId && x.Value.ConnectionId == recipientConnectionId))
        {
            //Clients.Client(recipientConnectionId).AvatarTyping(sender.Id);
            Clients.Client(recipientConnectionId).SendAsync("AvatarTyped", sender.Id);

            _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {sender.Id} Typing - {DateTime.Now} World: {sender.World.Id}");
        }
    }

    public void BroadcastTyping()
    {
        Avatar sender = GetCallingUser();

        var group = GetUsersGroup(sender);
        //Clients.OthersInGroup(group).AvatarBroadcastTyping(sender.Id);
        Clients.OthersInGroup(group).SendAsync("AvatarBroadcastTyped", sender.Id);

        _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {sender.Id} BroadcastTyping - {DateTime.Now} World: {group}");
    }

    #endregion

    #region Avatar

    public void BroadcastAvatarMovement(int avatarId, double x, double y, int z)
    {
        if (avatarId > 0)
        {
            var group = GetUsersGroup(GetCallingUser(avatarId));

            //Clients.OthersInGroup(group).BroadcastAvatarMovement(avatarId, x, y, z);
            Clients.OthersInGroup(group).SendAsync("BroadcastedAvatarMovement", avatarId, x, y, z);

            UpdateAvatarMovement(avatarId, x, y, z);

            _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {avatarId} BroadcastAvatarMovement - {DateTime.Now} World: {group}");
        }
    }

    public void BroadcastAvatarActivityStatus(int avatarId, int activityStatus)
    {
        if (avatarId > 0)
        {
            var group = GetCallingUser(avatarId);
            //Clients.OthersInGroup(GetUsersGroup(group)).BroadcastAvatarActivityStatus(avatarId, activityStatus);
            Clients.OthersInGroup(GetUsersGroup(group)).SendAsync("BroadcastedAvatarActivityStatus", avatarId, activityStatus);

            UpdateAvatarActivityStatus(avatarId, activityStatus);

            _logger.LogInformation($"<> ConnectionId: {Context.ConnectionId} AvatarId: {avatarId} BroadcastAvatarActivityStatus - {DateTime.Now} World: {group}");
        }
    }

    #endregion

    #region Construct

    public void BroadcastConstruct(Construct construct)
    {
        if (construct.Id > 0)
        {
            var group = GetUsersGroup(GetCallingUser());
            //Clients.OthersInGroup(group).BroadcastConstruct(construct);
            Clients.OthersInGroup(group).SendAsync("BroadcastedConstruct", construct);

            AddOrUpdateConstructInConstructs(construct);
            _logger.LogInformation($"<> {construct.Id} BroadcastConstruct - {DateTime.Now} World: {group}");
        }
    }

    public void BroadcastConstructs(Construct[] constructs)
    {
        if (constructs != null && constructs.Any())
        {
            var group = GetUsersGroup(GetCallingUser());

            //Clients.OthersInGroup(group).BroadcastConstructs(constructs);
            Clients.OthersInGroup(group).SendAsync("BroadcastedConstructs", constructs);

            foreach (var construct in constructs)
            {
                AddOrUpdateConstructInConstructs(construct);
            }
            _logger.LogInformation($"<> {constructs.Count()} BroadcastConstructs - {DateTime.Now} World: {group}");
        }
    }

    public void RemoveConstruct(int constructId)
    {
        if (constructId > 0)
        {
            var group = GetUsersGroup(GetCallingUser());

            //Clients.OthersInGroup(group).RemoveConstruct(constructId);
            Clients.OthersInGroup(group).SendAsync("RemovedConstruct", constructId);

            RemoveConstructFromConstructs(constructId);
            _logger.LogInformation($"<> Construct: {constructId} RemoveConstruct - {DateTime.Now} World: {group}");
        }
    }

    public void RemoveConstructs(int[] constructIds)
    {
        if (constructIds != null && constructIds.Any())
        {
            var group = GetUsersGroup(GetCallingUser());

            //Clients.OthersInGroup(group).RemoveConstructs(constructIds);
            Clients.OthersInGroup(group).SendAsync("RemovedConstructs", constructIds);

            foreach (var constructId in constructIds)
            {
                RemoveConstructFromConstructs(constructId);
            }

            _logger.LogInformation($"<> {constructIds.Count()} RemoveConstructs - {DateTime.Now} World: {group}");
        }
    }

    public void BroadcastConstructPlacement(int constructId, int z)
    {
        if (constructId > 0)
        {
            var group = GetUsersGroup(GetCallingUser());

            //Clients.OthersInGroup(group).BroadcastConstructPlacement(constructId, z);
            Clients.OthersInGroup(group).SendAsync("BroadcastedConstructPlacement", constructId, z);

            UpdateConstructPlacementInConstructs(constructId, z);
            _logger.LogInformation($"<> Construct: {constructId} BroadcastConstructPlacement - {DateTime.Now} World: {group}");
        }
    }

    public void BroadcastConstructRotation(int constructId, float rotation)
    {
        if (constructId > 0)
        {
            var group = GetUsersGroup(GetCallingUser());
            //Clients.OthersInGroup(group).BroadcastConstructRotation(constructId, rotation);
            Clients.OthersInGroup(group).SendAsync("BroadcastedConstructRotation", constructId, rotation);

            UpdateConstructRotationInConstructs(constructId, rotation);
            _logger.LogInformation($"<> Construct: {constructId} BroadcastConstructRotation - {DateTime.Now} World: {group}");
        }
    }

    public void BroadcastConstructRotations(ConcurrentDictionary<int, float> constructIds)
    {
        if (constructIds != null)
        {
            var group = GetUsersGroup(GetCallingUser());
            //Clients.OthersInGroup(group).BroadcastConstructRotations(constructIds);
            Clients.OthersInGroup(group).SendAsync("BroadcastedConstructRotations", constructIds);

            foreach (var constructId in constructIds)
            {
                UpdateConstructRotationInConstructs(constructId.Key, constructId.Value);
            }

            _logger.LogInformation($"<> {constructIds.Count()} BroadcastConstructRotations - {DateTime.Now} World: {group}");
        }
    }

    public void BroadcastConstructScale(int constructId, float scale)
    {
        if (constructId > 0)
        {
            var group = GetUsersGroup(GetCallingUser());

            //Clients.OthersInGroup(group).BroadcastConstructScale(constructId, scale);
            Clients.OthersInGroup(group).SendAsync("BroadcastedConstructScale", constructId, scale);

            UpdateConstructScaleInConstructs(constructId, scale);
            _logger.LogInformation($"<> Construct: {constructId} BroadcastConstructScale - {DateTime.Now} World: {group}");
        }
    }

    public void BroadcastConstructScales(int[] constructIds, float scale)
    {
        if (constructIds != null && constructIds.Any())
        {
            var group = GetUsersGroup(GetCallingUser());

            //Clients.OthersInGroup(group).BroadcastConstructScales(constructIds, scale);
            Clients.OthersInGroup(group).SendAsync("BroadcastedConstructScales", constructIds, scale);

            foreach (var constructId in constructIds)
            {
                UpdateConstructScaleInConstructs(constructId, scale);
            }

            _logger.LogInformation($"<> {constructIds.Count()} BroadcastConstructScales - {DateTime.Now} World: {group}");
        }
    }

    public void BroadcastConstructMovement(int constructId, double x, double y, int z)
    {
        if (constructId > 0)
        {
            var group = GetUsersGroup(GetCallingUser());

            //Clients.OthersInGroup(group).BroadcastConstructMovement(constructId, x, y, z);
            Clients.OthersInGroup(group).SendAsync("BroadcastedConstructMovement", constructId, x, y, z);

            UpdateConstructMovementInConstructs(constructId, x, y, z);
            _logger.LogInformation($"<> Construct: Construct: {constructId} BroadcastConstructMovement - {DateTime.Now} World: {group}");
        }
    }

    #endregion

    #region Connected Avatars

    private void UpdateAvatarReconnectionTime(int avatarId, DateTime reconnectionTime)
    {
        var connectionId = GetUserConnectionId(avatarId);

        if (OnlineAvatars.ContainsKey(connectionId))
        {
            var conUpdated = OnlineAvatars[connectionId];
            conUpdated.Session.ReconnectionTime = reconnectionTime;
            OnlineAvatars.TryUpdate(key: connectionId, newValue: conUpdated, comparisonValue: OnlineAvatars[connectionId]);
        }
    }

    private void UpdateAvatarDisconnectionTime(int avatarId, DateTime disconnectionTime)
    {
        var connectionId = GetUserConnectionId(avatarId);

        if (OnlineAvatars.ContainsKey(connectionId))
        {
            var conUpdated = OnlineAvatars[connectionId];
            conUpdated.Session.DisconnectionTime = disconnectionTime;
            OnlineAvatars.TryUpdate(key: connectionId, newValue: conUpdated, comparisonValue: OnlineAvatars[connectionId]);
        }
    }

    private void UpdateAvatarActivityStatus(int avatarId, int activityStatus)
    {
        var connectionId = GetUserConnectionId(avatarId);

        if (OnlineAvatars.ContainsKey(connectionId))
        {
            var conUpdated = OnlineAvatars[connectionId];
            conUpdated.ActivityStatus = (ActivityStatus)activityStatus;
            OnlineAvatars.TryUpdate(key: connectionId, newValue: conUpdated, comparisonValue: OnlineAvatars[connectionId]);
        }
    }

    private void UpdateAvatarMovement(int avatarId, double x, double y, int z)
    {
        var connectionId = GetUserConnectionId(avatarId);

        if (OnlineAvatars.ContainsKey(connectionId))
        {
            var conUpdated = OnlineAvatars[connectionId];
            conUpdated.Coordinate.X = x;
            conUpdated.Coordinate.Y = y;
            conUpdated.Coordinate.Z = z;

            OnlineAvatars.TryUpdate(key: connectionId, newValue: conUpdated, comparisonValue: OnlineAvatars[connectionId]);
        }
    }

    #endregion

    #region Connected Constructs

    private void UpdateConstructPlacementInConstructs(int constructId, int z)
    {
        if (OnlineConstructs.ContainsKey(constructId))
        {
            var conUpdated = OnlineConstructs[constructId];
            conUpdated.Coordinate.Z = z;
            OnlineConstructs.TryUpdate(key: constructId, newValue: conUpdated, comparisonValue: OnlineConstructs[constructId]);
        }
    }

    private void UpdateConstructRotationInConstructs(int constructId, float rotation)
    {
        if (OnlineConstructs.ContainsKey(constructId))
        {
            var conUpdated = OnlineConstructs[constructId];
            conUpdated.Rotation = rotation;
            OnlineConstructs.TryUpdate(key: constructId, newValue: conUpdated, comparisonValue: OnlineConstructs[constructId]);
        }
    }

    private void UpdateConstructScaleInConstructs(int constructId, float scale)
    {
        if (OnlineConstructs.ContainsKey(constructId))
        {
            var conUpdated = OnlineConstructs[constructId];
            conUpdated.Scale = scale;
            OnlineConstructs.TryUpdate(key: constructId, newValue: conUpdated, comparisonValue: OnlineConstructs[constructId]);
        }
    }

    private void RemoveConstructFromConstructs(int constructId)
    {
        if (OnlineConstructs.ContainsKey(constructId))
        {
            OnlineConstructs.TryRemove(constructId, out Construct c);
        }
    }

    private void AddOrUpdateConstructInConstructs(Construct construct)
    {
        if (OnlineConstructs.ContainsKey(construct.Id))
        {
            OnlineConstructs.TryRemove(new KeyValuePair<int, Construct>(key: construct.Id, value: construct));
        }

        OnlineConstructs.TryAdd(key: construct.Id, value: construct);
    }

    private void UpdateConstructMovementInConstructs(int constructId, double x, double y, int z)
    {
        if (OnlineConstructs.ContainsKey(constructId))
        {
            var construct = OnlineConstructs[constructId];

            construct.Coordinate.X = x;
            construct.Coordinate.Y = y;
            construct.Coordinate.Z = z;

            OnlineConstructs.TryUpdate(key: constructId, newValue: construct, comparisonValue: OnlineConstructs[constructId]);
        }
    }

    #endregion
}