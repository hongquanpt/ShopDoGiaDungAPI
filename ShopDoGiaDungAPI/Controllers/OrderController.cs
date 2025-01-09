using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Attributes;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Security.Claims;

namespace ShopDoGiaDungAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("MyAllowedOrigins")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogService _logService;
        public OrderController(IOrderService orderService, ILogService logService)
        {
            _orderService = orderService;
            _logService = logService;
        }

        [Permission("QuanLyDonHang", "Xem")]
        [HttpGet("orders")]
        public IActionResult QuanLyDH(int tinhTrang = 10, int page = 1, int pageSize = 100)
        {
            return _orderService.GetOrders(tinhTrang, page, pageSize);
        }

        [Permission("Access", "Xem")]
        [Permission("QuanLyDonHang", "Sua")]
        [HttpPost("orders/confirm/{madh}")]
        public async Task<IActionResult> XacNhanDH(int madh)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Log: Xác nhận đơn hàng
            await _logService.InsertLogAsync(
                userId: currentUserId,
                action: "Xác nhận đơn hàng",
                objects: $"madh={madh}",
                ip: ip
            );

            return _orderService.ConfirmOrder(madh);
        }

        [Permission("QuanLyDonHang", "Sua")]
        [HttpPost("orders/ship/{madh}")]
        public async Task<IActionResult> VanChuyenDH(int madh)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Log: Vận chuyển đơn hàng
            await _logService.InsertLogAsync(
                userId: currentUserId,
                action: "Vận chuyển đơn hàng",
                objects: $"madh={madh}",
                ip: ip
            );

            return _orderService.ShipOrder(madh);
        }

        [Permission("Access", "Xem")]
        [HttpPost("orders/cancel/{madh}")]
        public async Task<IActionResult> HuyDH(int madh)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Log: Hủy đơn hàng
            await _logService.InsertLogAsync(
                userId: currentUserId,
                action: "Hủy đơn hàng",
                objects: $"madh={madh}",
                ip: ip
            );

            return _orderService.CancelOrder(madh);
        }

        [Permission("Access", "Xem")]
        [HttpGet("orders/{orderId}/details")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Log: Xem chi tiết đơn hàng
            await _logService.InsertLogAsync(
                userId: currentUserId,
                action: "Xem chi tiết đơn hàng",
                objects: $"orderId={orderId}",
                ip: ip
            );

            var orderDetails = await _orderService.GetOrderDetails(orderId);
            if (orderDetails == null || orderDetails.Count == 0)
            {
                return NotFound();
            }

            return Ok(orderDetails);
        }

        [Permission("QuanLyDonHang", "Xem")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingOrders()
        {
            var orders = await _orderService.GetPendingOrdersAsync();

            if (orders == null || orders.Count == 0)
            {
                return NotFound(new { message = "No pending orders found." });
            }

            return Ok(orders);
        }
    }
}
