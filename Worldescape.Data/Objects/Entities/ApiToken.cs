using MongoDB.Bson.Serialization.Attributes;

namespace Worldescape.Data
{
    public class ApiToken
    {
        /// <summary>
        /// Id of the token.
        /// </summary>
        [BsonId]
        public int Id { get; set; } = UidGenerator.New();

        /// <summary>
        /// Id of the user to which this token is being generated.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The token generated for the user.
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }
}



