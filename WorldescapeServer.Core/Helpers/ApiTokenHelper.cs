using MongoDB.Driver;
using Worldescape.Common;
using Worldescape.Database;

namespace WorldescapeServer.Core
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
        }
    }
}
