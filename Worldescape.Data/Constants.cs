namespace Worldescape.Data
{
    public static class Constants
    {
        public static bool IsNullOrBlank(this string text)
        {
            return string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text);
        }

        public static string GetActionName(string action)
        {
            if (action.Contains("/api/Command/"))            
                action = action.Replace("/api/Command/", "");
            

            if (action.Contains("/api/Query/"))
                action = action.Replace("/api/Query/", "");

            return action;
        }

        #region Methods Invoked On Client From Server

        public const string AvatarDisconnected = "AvatarDisconnected";
        public const string AvatarReconnected = "AvatarReconnected";
        public const string AvatarLoggedIn = "AvatarLoggedIn";
        public const string AvatarLoggedOut = "AvatarLoggedOut";

        public const string BroadcastedTextMessage = "BroadcastedTextMessage";
        public const string BroadcastedPictureMessage = "BroadcastedPictureMessage";
        public const string UnicastedTextMessage = "UnicastedTextMessage";
        public const string UnicastedPictureMessage = "UnicastedPictureMessage";
        public const string AvatarTyped = "AvatarTyped";
        public const string AvatarBroadcastTyped = "AvatarBroadcastTyped";

        public const string BroadcastedAvatarMovement = "BroadcastedAvatarMovement";
        public const string BroadcastedAvatarActivityStatus = "BroadcastedAvatarActivityStatus";

        public const string BroadcastedConstruct = "BroadcastedConstruct";
        public const string BroadcastedConstructs = "BroadcastedConstructs";
        public const string RemovedConstruct = "RemovedConstruct";
        public const string RemovedConstructs = "RemovedConstructs";
        public const string BroadcastedConstructPlacement = "BroadcastedConstructPlacement";
        public const string BroadcastedConstructRotation = "BroadcastedConstructRotation";
        public const string BroadcastedConstructRotations = "BroadcastedConstructRotations";
        public const string BroadcastedConstructScale = "BroadcastedConstructScale";
        public const string BroadcastedConstructScales = "BroadcastedConstructScales";
        public const string BroadcastedConstructMovement = "BroadcastConstructMovement";

        #endregion

        #region Methods Invoked From Client To Server

        public const string Login = "Login";
        public const string Logout = "Logout";

        public const string BroadcastTextMessage = "BroadcastTextMessage";
        public const string BroadcastImageMessage = "BroadcastImageMessage";
        public const string UnicastTextMessage = "UnicastTextMessage";
        public const string UnicastImageMessage = "UnicastImageMessage";
        public const string Typing = "Typing";
        public const string BroadcastTyping = "BroadcastTyping";

        public const string BroadcastAvatarMovement = "BroadcastAvatarMovement";
        public const string BroadcastAvatarActivityStatus = "BroadcastAvatarActivityStatus";

        public const string BroadcastConstruct = "BroadcastConstruct";
        public const string BroadcastConstructs = "BroadcastConstructs";
        public const string RemoveConstruct = "RemoveConstruct";
        public const string RemoveConstructs = "RemoveConstructs";
        public const string BroadcastConstructPlacement = "BroadcastConstructPlacement";
        public const string BroadcastConstructRotation = "BroadcastConstructRotation";
        public const string BroadcastConstructRotations = "BroadcastConstructRotations";
        public const string BroadcastConstructScale = "BroadcastConstructScale";
        public const string BroadcastConstructScales = "BroadcastConstructScales";
        public const string BroadcastConstructMovement = "BroadcastConstructMovement";
        #endregion

        #region WebService Endpoints

        public const string Action_GetApiToken = "/api/Query/GetApiToken";
        public const string Action_GetUser = "/api/Query/GetUser";
        public const string Action_GetWorlds = "/api/Query/GetWorlds";
        public const string Action_GetAsset = "/api/Query/GetAsset";

        public const string Action_AddUser = "/api/Command/AddUser";
        public const string Action_UpdateUser = "/api/Command/UpdateUser";
        public const string Action_AddWorld = "/api/Command/AddWorld";
        public const string Action_UpdateWorld = "/api/Command/UpdateWorld";

        #endregion
    }
}
