using System;

namespace Worldescape.Common.Entities
{
    /// <summary>
    /// A registered user in real world.
    /// </summary>
    public class User : CoreBase
    {
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
        /// The gender of the user.
        /// </summary>
        public Gender Gender { get; set; }

        /// <summary>
        /// The user's birthday.
        /// </summary>
        public DateTime DateOfBirth { get; set; }
    }

    public enum Gender
    {
        Male,
        Female,
        Other,
    }
}



