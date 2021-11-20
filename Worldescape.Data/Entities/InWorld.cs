namespace Worldescape.Data
{
    /// <summary>
    /// Information of the world in which an avatar or a construct is.
    /// </summary>
    public class InWorld
    {
        /// <summary>
        /// World's id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// World's name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}