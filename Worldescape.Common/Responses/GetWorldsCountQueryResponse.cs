namespace Worldescape.Common
{
    public class GetWorldsCountQueryResponse: ServiceResponse
    {
        /// <summary>
        /// Count of total Constructs returned.
        /// </summary>
        public long Count { get; set; }
    }
}
