namespace Worldescape.Data
{
    public class GetConstructsCountQueryResponse: ServiceResponse
    {
        /// <summary>
        /// Count of total Constructs returned.
        /// </summary>
        public long? Count { get; set; }
    }
}
