using System.Collections.Generic;
using System.Linq;

namespace Worldescape.Common
{
    public class GetAvatarsQueryResponse : ServiceResponse
    {
        /// <summary>
        /// Count of total Avatars returned.
        /// </summary>
        public long? Count { get; set; }

        /// <summary>
        /// A collection of Avatars returned.
        /// </summary>
        public IEnumerable<Avatar> Avatars { get; set; } = Enumerable.Empty<Avatar>();
    }
}
