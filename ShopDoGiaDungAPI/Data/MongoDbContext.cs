using MongoDB.Driver;
using ShopDoGiaDungAPI.DTO;

namespace ShopDoGiaDungAPI.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }
        public IMongoCollection<LogEntry> UserLogs => _database.GetCollection<LogEntry>("user_logs");
    }
}
