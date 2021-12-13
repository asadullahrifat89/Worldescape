using MongoDB.Bson.Serialization.Attributes;

namespace Worldescape.Common
{
    public class Portal
    {
        /// <summary>
        /// Id of an entity. Auto generated upon instance declaration.
        /// </summary>
        [BsonId]
        public int Id { get; set; } = UidGenerator.New();

        /// <summary>
        /// The coordinate of this portal in the world.
        /// </summary>
        public Coordinate Coordinate { get; set; } = new Coordinate();

        /// <summary>
        /// The world to which this portals takes.
        /// </summary>
        public World World { get; set; }
    }
}
