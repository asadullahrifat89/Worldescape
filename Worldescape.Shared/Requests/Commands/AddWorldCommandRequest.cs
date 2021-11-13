namespace Worldescape.Shared.Requests.Commands
{
    /// <summary>
    /// A command that inserts or updates a user.
    /// </summary>
    public class AddWorldCommandRequest
    {
        /// <summary>
        /// Name of the world.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Image url of the world.
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;
    }
}