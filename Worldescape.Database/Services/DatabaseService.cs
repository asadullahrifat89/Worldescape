using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

        public async Task<bool> Exists<T>(FilterDefinition<T> filter)
        {
            var collection = GetCollection<T>();
            var result = await collection.Find(filter).FirstOrDefaultAsync();
            return result != null;
        }

        public async Task<T> FindOne<T>(FilterDefinition<T> filter)
        {
            var collection = GetCollection<T>();
            var result = await collection.Find(filter).FirstOrDefaultAsync();
            return result;
        }

        public async Task<T> FindById<T>(int id)
        {
            var filterDefinition = Builders<T>.Filter.Eq("Id", id);
            var collection = GetCollection<T>();
            var result = await collection.Find(filterDefinition).FirstOrDefaultAsync();
            return result;
        }

        public async Task<List<T>> GetDocuments<T>(FilterDefinition<T> filter)
        {
            var collection = GetCollection<T>();
            var result = await collection.Find(filter).ToListAsync();
            return result;
        }

        public async Task<List<T>> GetDocuments<T>(FilterDefinition<T> filter, int skip, int limit)
        {
            var collection = GetCollection<T>();
            var result = await collection.Find(filter).Skip(skip).Limit(limit).ToListAsync();
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

        public async Task<bool> ReplaceDocument<T>(T document, FilterDefinition<T> filter)
        {
            var collection = GetCollection<T>();
            var result = await collection.FindOneAndReplaceAsync(filter, document);
            return result != null;
        }

        public async Task<bool> ReplaceById<T>(T document, int id)
        {
            var filterDefinition = Builders<T>.Filter.Eq("Id", id);
            var collection = GetCollection<T>();
            var result = await collection.FindOneAndReplaceAsync(filterDefinition, document);
            return result != null;
        }

        public async Task<bool> DeleteDocument<T>(FilterDefinition<T> filter)
        {
            var collection = GetCollection<T>();
            var result = await collection.FindOneAndDeleteAsync(filter);
            return result != null;
        }

        public async Task<bool> DeleteDocuments<T>(FilterDefinition<T> filter)
        {
            var collection = GetCollection<T>();
            var result = await collection.DeleteManyAsync(filter);
            return result != null;
        }

        #endregion

        #endregion
    }
}