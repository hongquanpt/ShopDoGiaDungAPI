using Microsoft.AspNetCore.Mvc;

namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface IOrderService
    {
        // Admin functions
        IActionResult GetOrders(int? status, int page, int pageSize);
        IActionResult ConfirmOrder(int orderId);
        IActionResult ShipOrder(int orderId);
        IActionResult CancelOrder(int orderId);
        IActionResult GetOrderDetails(int orderId);

        // Home functions
        Task<IActionResult> GetUserOrders(int userId, string typeMenu, int pageIndex, int pageSize);
        Task<IActionResult> CancelUserOrder(int orderId, int userId);
        Task<IActionResult> ConfirmOrderReceived(int orderId, int userId);
    }
}
