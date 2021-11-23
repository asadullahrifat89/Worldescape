namespace Worldescape.Data
{
    public class GetWorldsCountQueryResponse: ServiceResponse
    {
        /// <summary>
        /// Count of total Constructs returned.
        /// </summary>
        public long Count { get; set; }
    }
}
