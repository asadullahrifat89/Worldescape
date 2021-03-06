using System;

namespace Worldescape.Common
{
    /// <summary>
    /// A registered user in real world. ImageUrl saves the profile picture.
    /// </summary>
    public class User : EntityBase
    {
        /// <summary>
        /// The first name of the user.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// The last name of the user.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

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



