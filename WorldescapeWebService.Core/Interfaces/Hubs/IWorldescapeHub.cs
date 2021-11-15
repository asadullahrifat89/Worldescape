using System.Collections.Concurrent;
using Worldescape.Shared.Entities;

namespace WorldescapeWebService.Core.Contracts.Services;

public interface IWorldescapeHub
{
    #region Session
       
    Task AvatarLogin(Avatar avatar);
    Task AvatarLogout(int senderId);

    Task AvatarDisconnection(int senderId);
    Task AvatarReconnection(int senderId);

    #endregion

    #region Texting

    Task BroadcastTextMessage(int senderId, string message);
    Task BroadcastPictureMessage(int senderId, byte[] img);
    Task UnicastTextMessage(int senderId, string message);
    Task UnicastPictureMessage(int senderId, byte[] img);
    Task AvatarTyping(int senderId);
    Task AvatarBroadcastTyping(int senderId);

    #endregion

    #region Avatar

    Task BroadcastAvatarMovement(int avatarId, double x, double y, int z);
    Task BroadcastAvatarActivityStatus(int avatarId, int activityStatus);

    #endregion

    #region Construct

    Task BroadcastConstruct(Construct construct);
    Task BroadcastConstructs(Construct[] constructs);
    Task RemoveConstruct(int constructId);
    Task RemoveConstructs(int[] constructIds);
    Task BroadcastConstructPlacement(int constructId, int z);
    Task BroadcastConstructRotation(int constructId, float rotation);
    Task BroadcastConstructRotations(ConcurrentDictionary<int, float> constructIds);
    Task BroadcastConstructScale(int constructId, float scale);
    Task BroadcastConstructScales(int[] constructIds, float scale);
    Task BroadcastConstructMovement(int constructId, double x, double y, int z);

    #endregion
}

