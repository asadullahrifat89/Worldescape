namespace Worldescape.Common 
{
    /// <summary>
    /// A query that fetches worlds.
    /// </summary>
    public class GetWorldsCountQueryRequest: RequestBase
    {
        /// <summary>
        /// The search string of the query.
        /// </summary>
        public string SearchString { get; set; } = string.Empty;

        /// <summary>
        /// The id of the creator for whom result should be filtered.
        /// </summary>
        public int CreatorId { get; set; }
    }
}



