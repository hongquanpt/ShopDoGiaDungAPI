﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Controllers
{
    [Authorize]
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
        public IActionResult QuanLyDH(int? tinhTrang = null, int page = 1, int pageSize = 100)
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

        [HttpGet("orders/{orderId}/details")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            var orderDetails = await _orderService.GetOrderDetails(orderId);

            if (orderDetails == null || orderDetails.Count == 0)
            {
                return NotFound();
            }

            return Ok(orderDetails);
        }
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