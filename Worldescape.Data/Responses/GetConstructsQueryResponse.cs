using System.Collections.Generic;
using System.Linq;
using Worldescape.Data;

namespace Worldescape.Data
{
    public class GetConstructsQueryResponse
    {
        /// <summary>
        /// Count of total Constructs returned.
        /// </summary>
        public long? Count { get; set; }

        /// <summary>
        /// A collection of Constructs returned.
        /// </summary>
        public IEnumerable<Construct> Constructs { get; set; } = Enumerable.Empty<Construct>();
    }
}
