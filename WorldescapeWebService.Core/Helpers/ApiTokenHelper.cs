using LiteDB;
using Worldescape.Shared.Entities;

namespace WorldescapeWebService.Core.Helpers
{
    public class ApiTokenHelper
    {
        public bool BeValidApiToken(string token)
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"Worldescape.db"))
            {
                // Get ApiTokens collection
                var col = db.GetCollection<ApiToken>("ApiTokens");

                // Use LINQ to query documents (with no index)
                var result = col.FindOne(x => x.Token == token);

                // If no api token was found then invalid key
                if (result == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Gets the user information from a token. Returns null if not found.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public User GetUserFromApiToken(string token)
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"Worldescape.db"))
            {
                // Get ApiTokens collection
                var colApiTokens = db.GetCollection<ApiToken>("ApiTokens");

                // Use LINQ to query documents (with no index)
                var apiToken = colApiTokens.FindOne(x => x.Token == token);

                // If no api token was found then invalid key
                if (apiToken == null)
                {
                    return null;
                }
                else
                {
                    // Get Users collection
                    var colUser = db.GetCollection<User>("Users");

                    return colUser.FindById(apiToken.UserId);
                }
            }
        }
    }
}
