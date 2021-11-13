using System;
using Worldescape.Shared.Entities;
using Worldescape.Shared.Responses;

namespace Worldescape.Shared.Requests.Commands
{
    /// <summary>
    /// A command that inserts or updates a user.
    /// </summary>
    public class AddUserCommandRequest
    {
        /// <summary>
        /// Name of the user.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The email address of the user.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The phone number of the user.
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// The password of the user.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Image url of the user.
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// The gender of the user.
        /// </summary>
        public Gender Gender { get; set; }

        /// <summary>
        /// The user's birthday.
        /// </summary>
        public DateTime DateOfBirth { get; set; }
    }
}



