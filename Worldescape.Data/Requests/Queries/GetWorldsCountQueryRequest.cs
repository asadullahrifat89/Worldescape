namespace Worldescape.Data 
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
    }
}



