using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Worldescape.Common
{
    public class EntityBase
    {
        /// <summary>
        /// Id of an entity. Auto generated upon instance declaration.
        /// </summary>
        [BsonId]
        public int Id { get; set; } = UidGenerator.New();

        /// <summary>
        /// Name of an entity.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Image url of an entity.
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// The datetime on which this entity was created on.
        /// </summary>
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        /// <summary>
        /// The datetime on which this entity was updated on.
        /// </summary>
        public DateTime? UpdatedOn { get; set; } = DateTime.Now;

        public bool IsEmpty()
        {
            return Name.IsNullOrBlank();
        }
    }
}