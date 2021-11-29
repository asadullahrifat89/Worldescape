using MongoDB.Bson.Serialization.Attributes;

namespace Worldescape.Common
{
    public class Blob
    {
        /// <summary>
        /// Id of the blob.
        /// </summary>
        [BsonId]
        public int Id { get; set; } = UidGenerator.New();        

        /// <summary>
        /// The data url of the blob.
        /// </summary>
        public string DataUrl { get; set; } = string.Empty;
    }
}



