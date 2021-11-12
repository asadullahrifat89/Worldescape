using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Worldescape.App.Core.ObjectElements;

namespace Worldescape.App.Core.Contracts.Services
{
    public interface IWorldescapeHubService
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
        event Action<int, string, MessageType> NewTextMessage;
        event Action<int, byte[], MessageType> NewImageMessage;
        event Action<int, MessageType> AvatarTyping;

        // Avatar
        event Action<BroadcastAvatarMovementRequest> NewBroadcastAvatarMovement;
        event Action<BroadcastAvatarActivityStatusRequest> NewBroadcastAvatarActivityStatus;

        // Construct
        event Action<Construct> NewBroadcastConstruct;
        event Action<Construct[]> NewBroadcastConstructs;
        event Action<int> NewRemoveConstruct;
        event Action<int[]> NewRemoveConstructs;
        event Action<int, int> NewBroadcastConstructPlacement;
        event Action<int, float> NewBroadcastConstructRotation;
        event Action<ConcurrentDictionary<int, float>> NewBroadcastConstructRotations;
        event Action<int, float> NewBroadcastConstructScale;
        event Action<int[], float> NewBroadcastConstructScales;

        #endregion

        #region Connection

        Task ConnectAsync();

        Task DisconnectAsync();

        #endregion

        #region Session

        Task<Tuple<Avatar[], Construct[]>> LoginAsync(Avatar avatar);

        Task LogoutAsync();

        #endregion

        #region Texting

        Task SendBroadcastMessageAsync(string msg);

        Task SendBroadcastMessageAsync(byte[] img);

        Task SendUnicastMessageAsync(int recepientId, string msg);

        Task SendUnicastMessageAsync(int recepientId, byte[] img);

        Task TypingAsync(int recepientId);

        Task BroadcastTypingAsync();

        #endregion

        #region Avatar

        Task BroadcastAvatarMovementAsync(BroadcastAvatarMovementRequest @event);

        Task BroadcastAvatarActivityStatusAsync(BroadcastAvatarActivityStatusRequest @event);

        #endregion

        #region Construct

        Task BroadcastConstructAsync(Construct construct);

        Task BroadcastConstructsAsync(Construct[] constructs);

        Task RemoveConstructAsync(int constructId);

        Task RemoveConstructsAsync(int[] constructIds);

        Task BroadcastConstructPlacementAsync(int constructId, int z);

        Task BroadcastConstructRotationAsync(int constructId, float rotation);

        Task BroadcastConstructRotationsAsync(ConcurrentDictionary<int, float> constructIds);

        Task BroadcastConstructScaleAsync(int constructId, float scale);

        Task BroadcastConstructScalesAsync(int[] constructIds, float scale);

        #endregion
    }
}
