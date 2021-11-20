using MongoDB.Driver;
using Worldescape.Data;
using Worldescape.Database;

namespace WorldescapeWebService.Core
{
    public class ApiTokenHelper
    {
        private readonly DatabaseService _databaseService;

        public ApiTokenHelper(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<bool> BeValidApiToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            var result = await _databaseService.Exists(Builders<ApiToken>.Filter.Eq(x => x.Token, token));

            return result;

            //// Open database (or create if doesn't exist)
            //using (var db = new LiteDatabase(@"Worldescape.db"))
            //{
            //    // Get ApiTokens collection
            //    var col = db.GetCollection<ApiToken>("ApiTokens");

            //    // Use LINQ to query documents (with no index)
            //    var result = col.FindOne(x => x.Token == token);

            //    // If no api token was found then invalid key
            //    if (result == null)
            //    {
            //        return false;
            //    }
            //    else
            //    {
            //        return true;
            //    }
            //}
        }

        /// <summary>
        /// Gets the user information from a token. Returns null if not found.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<User> GetUserFromApiToken(string token)
        {
            var apiToken = await _databaseService.FindOne(Builders<ApiToken>.Filter.Eq(x => x.Token, token));

            if (apiToken == null)
                return null;

            return await _databaseService.FindById<User>(apiToken.UserId);

            //// Open database (or create if doesn't exist)
            //using (var db = new LiteDatabase(@"Worldescape.db"))
            //{
            //    // Get ApiTokens collection
            //    var colApiTokens = db.GetCollection<ApiToken>("ApiTokens");

            //    // Use LINQ to query documents (with no index)
            //    var apiToken = colApiTokens.FindOne(x => x.Token == token);

            //    // If no api token was found then invalid key
            //    if (apiToken == null)
            //    {
            //        return null;
            //    }
            //    else
            //    {
            //        // Get Users collection
            //        var colUser = db.GetCollection<User>("Users");

            //        return colUser.FindById(apiToken.UserId);
            //    }
            //}
        }
    }
}
