namespace Worldescape.Data
{
    /// <summary>
    /// A command that updates a world.
    /// </summary>
    public class UpdateWorldCommandRequest : AddWorldCommandRequest
    {
        /// <summary>
        /// The id of the world.
        /// </summary>
        public int Id { get; set; }
    }
}