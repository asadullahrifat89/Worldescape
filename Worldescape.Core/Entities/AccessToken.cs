using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Worldescape.Core
{
    internal class AccessToken
    {
        /// <summary>
        /// Id of the user to which this token is being generated.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The token generated for the user.
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }
}
