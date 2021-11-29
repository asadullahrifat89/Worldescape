namespace Worldescape.Common 
{
    /// <summary>
    /// A query that fetches worlds.
    /// </summary>
    public class GetWorldsQueryRequest: RequestBase
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
        /// The search string of the query.
        /// </summary>
        public string SearchString { get; set; } = string.Empty;

        /// <summary>
        /// The id of the creator for whom result should be filtered.
        /// </summary>
        public int CreatorId { get; set; }
    }
}



