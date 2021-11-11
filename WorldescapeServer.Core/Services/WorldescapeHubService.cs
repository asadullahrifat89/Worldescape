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

        private static int GetUsersGroup(Avatar newUser)
        {
            return newUser.World.Id;
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

                Clients.OthersInGroup(GetUsersGroup(user).ToString()).AvatarDisconnection(user.Id);
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

                Clients.OthersInGroup(GetUsersGroup(user).ToString()).AvatarReconnection(user.Id);
                _logger.LogInformation($"<> {user.Id} OnConnectedAsync- {DateTime.Now}");
            }
            return base.OnConnectedAsync();
        }

        #endregion

        #region ConnectedAvatars

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

        #region ConcurrentConstructs
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
