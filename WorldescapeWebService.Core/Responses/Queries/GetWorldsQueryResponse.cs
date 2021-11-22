using Worldescape.Data;

namespace WorldescapeWebService.Core
{
    public class GetWorldsQueryResponse
    {
        /// <summary>
        /// Count of the worlds returned.
        /// </summary>
        public long? Count { get; set; }

        /// <summary>
        /// A collection of worlds returned.
        /// </summary>
        public IEnumerable<World> Worlds { get; set; } = Enumerable.Empty<World>();
    }
}
