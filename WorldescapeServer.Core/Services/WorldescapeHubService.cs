using LiteDB;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Worldescape.Core;
using static Worldescape.Core.Enums;

namespace WorldescapeServer.Core
{
    public class WorldescapeHubService : Hub<IWorldescapeHubService>
    {
        #region Fields

        private readonly ILogger<WorldescapeHubService> _logger;

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

        public void BroadcastAvatarMovement(BroadcastAvatarMovementRequest @event)
        {
            if (@event.AvatarId > 0 && @event.Coordinate != null)
            {
                Clients.OthersInGroup(GetUsersGroup(GetCallingUser(@event.AvatarId))).BroadcastAvatarMovement(@event);

                UpdateAvatarMovement(@event.AvatarId, @event.Coordinate);

                _logger.LogInformation($"<> {@event.AvatarId} BroadcastAvatarMovement - {DateTime.Now}");
            }
        }

        public void BroadcastAvatarActivityStatus(BroadcastAvatarActivityStatusRequest @event)
        {
            if (@event.AvatarId > 0)
            {
                Clients.OthersInGroup(GetUsersGroup(GetCallingUser(@event.AvatarId))).BroadcastAvatarActivityStatus(@event);

                UpdateAvatarActivityStatus(@event.AvatarId, @event.ActivityStatus);

                _logger.LogInformation($"<> {@event.AvatarId} BroadcastAvatarActivityStatus - {DateTime.Now}");
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
}
