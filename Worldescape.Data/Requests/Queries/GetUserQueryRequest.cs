﻿namespace Worldescape.Data
{
    /// <summary>
    /// A command that inserts or updates a user.
    /// </summary>
    public class GetUserQueryRequest: RequestBase
    {
        /// <summary>
        /// The email address of the user.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The password of the user.
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}



