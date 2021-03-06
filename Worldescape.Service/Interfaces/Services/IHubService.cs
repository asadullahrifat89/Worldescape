using System;
using System.Threading.Tasks;
using Worldescape.Common;

namespace Worldescape.Service
{
    public interface IHubService
    {
        #region Fields

        // Connection
        event Action<int> AvatarDisconnected;
        event Action<int> AvatarReconnected;
        event Action ConnectionReconnecting;
        event Action ConnectionReconnected;
        event Action ConnectionClosed;

        // Session
        event Action<Avatar> AvatarLoggedIn;
        event Action<int> AvatarLoggedOut;

        // Texting
        event Action<ChatMessage, MessageType> NewMessage;
        event Action<int, MessageType> AvatarTyping;

        // Avatar
        event Action<int, double, double, int> NewBroadcastAvatarMovement;
        event Action<int, int> NewBroadcastAvatarActivityStatus;

        // Construct
        event Action<Construct> NewBroadcastConstruct;
        event Action<int> NewRemoveConstruct;
        event Action<int, int> NewBroadcastConstructPlacement;
        event Action<int, float> NewBroadcastConstructRotation;
        event Action<int, float> NewBroadcastConstructScale;
        event Action<int, double, double, int> NewBroadcastConstructMovement;

        // Portal
        event Action<Portal> NewBroadcastPortal;

        #endregion

        #region Connection

        bool IsConnected();

        Task ConnectAsync();

        Task DisconnectAsync();

        #endregion

        #region Session

        Task<Avatar> Login(Avatar avatar);

        Task Logout();

        #endregion

        #region Texting

        Task SendBroadcastMessage(ChatMessage chatMessage);

        Task SendUnicastMessage(ChatMessage chatMessage);


        Task Typing(int recepientId);

        Task BroadcastTyping();

        #endregion

        #region Avatar

        Task BroadcastAvatarMovement(int avatarId, double x, double y, int z);

        Task BroadcastAvatarActivityStatus(int avatarId, int activityStatus);

        #endregion

        #region Construct

        Task BroadcastConstruct(Construct construct);

        Task RemoveConstruct(int constructId);

        //Task RemoveConstructs(int[] constructIds);

        Task BroadcastConstructPlacement(int constructId, int z);

        Task BroadcastConstructRotation(int constructId, float rotation);

        //Task BroadcastConstructRotations(ConcurrentDictionary<int, float> constructIds);

        Task BroadcastConstructScale(int constructId, float scale);

        //Task BroadcastConstructScales(int[] constructIds, float scale);

        Task BroadcastConstructMovement(int constructId, double x, double y, int z);

        #endregion

        #region Portal

        Task BroadcastPortal(Portal portal);

        #endregion
    }
}
