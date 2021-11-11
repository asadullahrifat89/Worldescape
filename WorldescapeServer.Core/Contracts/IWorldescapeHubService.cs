using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Worldescape.Core;

namespace WorldescapeServer.Core
{
    public interface IWorldescapeHubService
    {
		#region Session

		Task AvatarDisconnection(int senderId);
		Task AvatarReconnection(int senderId);
		Task AvatarLogin(Avatar client);
		Task AvatarLogout(int senderId);

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

		Task BroadcastAvatarMovement(BroadcastAvatarMovementRequest @event);
		Task BroadcastAvatarActivityStatus(BroadcastAvatarActivityStatusRequest @event);

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

		#endregion
	}
}
