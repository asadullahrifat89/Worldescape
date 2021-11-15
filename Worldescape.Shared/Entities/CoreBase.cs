using System;

namespace Worldescape.Shared
{
    public class CoreBase
    {
        /// <summary>
        /// Id of an entity.
        /// </summary>
        public int Id { get; set; }

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
            return Id <= 0 && string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(ImageUrl);
        }
    }
}