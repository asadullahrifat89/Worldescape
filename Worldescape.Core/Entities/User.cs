namespace Worldescape.Core
{
    /// <summary>
    /// A registered user in real world.
    /// </summary>
    public class User : CoreBase
    {
		public string Email { get; set; } = string.Empty;

		public string Phone { get; set; } = string.Empty;

		public string ProfilePictureUrl { get; set; } = string.Empty;

		public string Pasword { get; set; } = string.Empty;
    }
}
