using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("orders")]
        public IActionResult QuanLyDH(int? tinhTrang = null, int page = 1, int pageSize = 5)
        {
            return _orderService.GetOrders(tinhTrang, page, pageSize);
        }

        [HttpPost("orders/confirm/{madh}")]
        public IActionResult XacNhanDH(int madh)
        {
            return _orderService.ConfirmOrder(madh);
        }

        [HttpPost("orders/ship/{madh}")]
        public IActionResult VanChuyenDH(int madh)
        {
            return _orderService.ShipOrder(madh);
        }

        [HttpPost("orders/cancel/{madh}")]
        public IActionResult HuyDH(int madh)
        {
            return _orderService.CancelOrder(madh);
        }

        [HttpGet("orders/{id}/details")]
        public IActionResult MyOrderDetail(int id)
        {
            return _orderService.GetOrderDetails(id);
        }
    }
}
