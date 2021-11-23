using System.Collections.Generic;
using System.Linq;
using Worldescape.Data;

namespace Worldescape.Data
{
    public class GetWorldsQueryResponse : ServiceResponse
    {
        /// <summary>
        /// Count of total worlds returned.
        /// </summary>
        public long? Count { get; set; }

        /// <summary>
        /// A collection of worlds returned.
        /// </summary>
        public IEnumerable<World> Worlds { get; set; } = Enumerable.Empty<World>();
    }
}
