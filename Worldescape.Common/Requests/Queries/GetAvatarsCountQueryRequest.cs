namespace Worldescape.Common 
{
    /// <summary>
    /// A query that fetches Avatars.
    /// </summary>
    public class GetAvatarsCountQueryRequest: RequestBase
    {
        /// <summary>
        /// The world id of which the Avatars are to be returned.
        /// </summary>
        public int WorldId { get; set; }
    }
}



