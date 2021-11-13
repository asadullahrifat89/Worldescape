using Worldescape.Shared.Entities;

namespace WorldescapeWebService.Core.Responses.Queries
{
    public class GetWorldsQueryResponse
    {
        /// <summary>
        /// Count of the worlds returned.
        /// </summary>
        public int? Count { get; set; }

        /// <summary>
        /// A collection of worlds returned.
        /// </summary>
        public IEnumerable<World> Worlds { get; set; } = Enumerable.Empty<World>();
    }
}
