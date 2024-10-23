using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class StatisticsService : IStatisticsService
    {
        private readonly OnlineShopContext _context;

        public StatisticsService(OnlineShopContext context)
        {
            _context = context;
        }

        public IActionResult GetSalesStatistics(int year)
        {
            var orders = _context.Donhangs
                                 .Where(s => s.NgayLap.HasValue && s.NgayLap.Value.Year == year)
                                 .ToList();

            var salesStatistics = new List<ThongKeDoanhThu>();

            for (int month = 1; month <= 12; month++)
            {
                long? monthlyTotal = orders
                    .Where(order => order.NgayLap.HasValue && order.NgayLap.Value.Month == month)
                    .Sum(order => order.TongTien) ?? 0;

                salesStatistics.Add(new ThongKeDoanhThu
                {
                    Thang = month,
                    DoanhThu = monthlyTotal
                });
            }

            return new OkObjectResult(new
            {
                status = true,
                data = salesStatistics
            });
        }
    }
}
