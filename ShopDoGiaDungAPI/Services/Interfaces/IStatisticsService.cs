using Microsoft.AspNetCore.Mvc;

namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface IStatisticsService
    {
        IActionResult GetSalesStatistics(int year);
    }
}
