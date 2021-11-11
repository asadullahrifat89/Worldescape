using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Worldescape.Common;

namespace WorldescapeWebService.Core
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
