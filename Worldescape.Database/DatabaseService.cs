using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Worldescape.Database
{
    public class DatabaseService
    {
        #region Fields

        private readonly string _connectionString;
        private readonly ILogger<DatabaseService> _logger;

        #endregion

        #region Ctor
        public DatabaseService(
           IConfiguration configuration,
           ILogger<DatabaseService> logger)
        {
            _connectionString = configuration.GetConnectionString("WorldescapeDatabase");
            _logger = logger;
        }
        #endregion

        #region Methods

        #region Common

        private IMongoCollection<T> GetCollection<T>()
        {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase("Worldescape");

            var collectionName = typeof(T).Name + "s";

            var collection = database.GetCollection<T>(collectionName);
            return collection;
        }

        #endregion

        #region Document

        public async Task<List<T>> GetDocuments<T>(FilterDefinition<T> filterDefinition)
        {
            var collection = GetCollection<T>();
            var result = await collection.Find(filterDefinition).ToListAsync();

            return result;
        }

        public async Task<bool> InsertDocument<T>(T document)
        {
            var collection = GetCollection<T>();

            await collection.InsertOneAsync(document);

            return true;
        }

        public async Task<bool> InsertDocuments<T>(IEnumerable<T> documents)
        {
            var collection = GetCollection<T>();

            await collection.InsertManyAsync(documents);

            return true;
        }

        public async Task<bool> ReplaceDocument<T>(T document, FilterDefinition<T> filterDefinition)
        {
            var collection = GetCollection<T>();

            var result = await collection.FindOneAndReplaceAsync(filterDefinition, document);

            return result != null;
        }

        public async Task<bool> DeleteDocument<T>(FilterDefinition<T> filterDefinition)
        {
            var collection = GetCollection<T>();

            var result = await collection.FindOneAndDeleteAsync(filterDefinition);

            return result != null;
        }

        public async Task<bool> DeleteDocuments<T>(FilterDefinition<T> filterDefinition)
        {
            var collection = GetCollection<T>();

            var result = await collection.DeleteManyAsync(filterDefinition);

            return result != null;
        }  

        #endregion

        #endregion
    }
}