using MongoDB.Driver;
using ShopDoGiaDungAPI.Services.Interfaces;
using ShopDoGiaDungAPI.DTO;
using MongoDB.Bson;


namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class LogService : ILogService
    {
        private readonly IMongoCollection<LogEntry> _logCollection;

        public LogService(string connectionString, string databaseName, string collectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _logCollection = database.GetCollection<LogEntry>(collectionName);
        }

        public void WriteLog(string userid, string action, string objects, string ip)
        {
            var logEntry = new LogEntry
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Timestamp = DateTime.UtcNow,
                userid = userid,
                action = action,
                objects = objects,
                ip = ip
            };

            _logCollection.InsertOne(logEntry);
        }

        public async Task WriteLogAsync(string userid, string action, string objects, string ip)
        {
            var logEntry = new LogEntry
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Timestamp = DateTime.UtcNow,
                userid = userid,
                action = action,
                objects = objects,
                ip = ip
            };

            await _logCollection.InsertOneAsync(logEntry);
        }
    }
}