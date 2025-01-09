using MongoDB.Driver;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services.Interfaces;
namespace ShopDoGiaDungAPI.Services
{
    public class LogService : ILogService
    {
        private readonly MongoDbContext _mongoDbContext;

        public LogService(MongoDbContext mongoDbContext)
        {
            _mongoDbContext = mongoDbContext;
        }

        public async Task InsertLogAsync(string userId, string action, string objects, string ip)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                userid = userId,
                action = action,
                objects = objects,
                ip = ip
            };

            await _mongoDbContext.UserLogs.InsertOneAsync(logEntry);
        }
        // Hàm lấy danh sách Log
        public async Task<(IEnumerable<LogEntry> Logs, int TotalCount)> GetLogsAsync(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            // Đếm tổng
            var totalCount = (int)await _mongoDbContext.UserLogs.CountDocumentsAsync(FilterDefinition<LogEntry>.Empty);

            // Lấy data phân trang
            var logs = await _mongoDbContext.UserLogs
                .Find(FilterDefinition<LogEntry>.Empty)
                .Skip(skip)
                .Limit(pageSize)
                .SortByDescending(x => x.Timestamp) // sắp xếp mới nhất
                .ToListAsync();

            return (logs, totalCount);
        }
    }
}
