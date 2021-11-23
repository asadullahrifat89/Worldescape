namespace Worldescape.Data 
{
    /// <summary>
    /// A query that fetches Constructs.
    /// </summary>
    public class GetConstructsCountQueryRequest: RequestBase
    {
        /// <summary>
        /// The world id of which the constructs are to be returned.
        /// </summary>
        public int WorldId { get; set; }
    }
}



