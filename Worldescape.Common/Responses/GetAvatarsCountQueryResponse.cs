namespace Worldescape.Common
{
    public class GetAvatarsCountQueryResponse: ServiceResponse
    {
        /// <summary>
        /// Count of total Avatars returned.
        /// </summary>
        public long Count { get; set; }
    }
}
