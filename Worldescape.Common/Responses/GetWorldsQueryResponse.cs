using System.Collections.Generic;
using System.Linq;

namespace Worldescape.Common
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
