namespace Worldescape.Common
{
    /// <summary>
    /// A query that fetches Avatars.
    /// </summary>
    public class GetAvatarsQueryRequest : RequestBase
    {
        /// <summary>
        /// The page index of the query.
        /// </summary>
        public int PageIndex { get; set; } = 0;

        /// <summary>
        /// The page size of the query.
        /// </summary>
        public int PageSize { get; set; } = 0;

        /// <summary>
        /// The world id of which the Avatars are to be returned.
        /// </summary>
        public int WorldId { get; set; }
    }
}
