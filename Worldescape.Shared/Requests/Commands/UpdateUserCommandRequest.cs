namespace Worldescape.Shared.Requests.Commands
{
    /// <summary>
    /// A command that inserts or updates a user.
    /// </summary>
    public class UpdateUserCommandRequest : AddUserCommandRequest
    {
        /// <summary>
        /// The id of the user.
        /// </summary>
        public int Id { get; set; }
    }
}