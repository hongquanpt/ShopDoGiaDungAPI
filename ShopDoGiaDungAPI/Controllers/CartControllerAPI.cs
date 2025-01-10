using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ShopDoGiaDungAPI.Attributes;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Security.Claims;

namespace ShopDoGiaDungAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("MyAllowedOrigins")]
    public class CartControllerAPI : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly IHubContext<OrderHub> _orderHubContext;
        private readonly ILogService _logService;
        public CartControllerAPI(ICartService cartService, IHubContext<OrderHub> orderHubContext, ILogService logService)
        {
            _cartService = cartService;
            _orderHubContext = orderHubContext;
            _logService = logService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Index()
        {
            return _cartService.GetCart(HttpContext.Session);
        }

        [AllowAnonymous]
        [HttpPost("AddItem")]
        public JsonResult AddItem([FromBody] AddItemRequest request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return new JsonResult(new { status = false, message = "Invalid request data" });
            }

            return _cartService.AddItemToCart(request.ProductId, HttpContext.Session, request.CheckOnly);
        }

        [AllowAnonymous]
        [HttpGet("Total")]
        public ActionResult Total()
        {
            return _cartService.GetCartTotal(HttpContext.Session);
        }

        [AllowAnonymous]
        [HttpDelete("Delete/{id}")]
        public JsonResult Delete(long id)
        {
            return _cartService.DeleteItemFromCart(id, HttpContext.Session);
        }

        [AllowAnonymous]
        [HttpPut("Update")]
        public JsonResult Update(int productId, int amount)
        {
            return _cartService.UpdateCartItemQuantity(productId, amount);
        }

        [AllowAnonymous]
        [HttpDelete("DeleteAll")]
        public JsonResult DeleteAll()
        {
            return _cartService.ClearCart(HttpContext.Session);
        }

        [AllowAnonymous]
        [HttpPost("ThanhToan")]
        public async Task<JsonResult> ThanhToan([FromBody] ThongTinThanhToan thanhToan)
        {
            // Kiểm tra user đăng nhập, nhưng thay vì trả về Unauthorized, ta sử dụng "Khách lai vãng"
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            string userIdForLog;
            int userId = 0; // giá trị mặc định, dùng cho checkout nếu có

            if (userIdClaim != null)
            {
                userIdForLog = userIdClaim.Value;
                userId = int.Parse(userIdClaim.Value);
            }
            else
            {
                userIdForLog = "Khách lai vãng";
                // Nếu không có userId, tùy theo logic checkout, có thể gán một giá trị mặc định hoặc xử lý khác
                // Ở đây ta để userId = 0, nhưng nên xem xét logic _cartService.Checkout cho trường hợp người dùng chưa đăng nhập
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Ghi log: Thanh toán đơn hàng
            await _logService.InsertLogAsync(
                userId: userIdForLog,
                action: "Thanh Toán",
                objects: $"ThongTinThanhToan: Ten={thanhToan.ten}, SDT={thanhToan.sdt}, DiaChi={thanhToan.diaChi}",
                ip: ip
            );

            var checkoutResult = await _cartService.Checkout(thanhToan, userId);

            dynamic resultValue = checkoutResult.Value;
            bool status = resultValue.status;
            if (status)
            {
                var newOrderInfo = new
                {
                    Ten = thanhToan.ten,
                    SDT = thanhToan.sdt,
                    DiaChi = thanhToan.diaChi,
                    NgayLap = System.DateTime.Now
                };

                await _orderHubContext.Clients.All.SendAsync("ReceiveNewOrder", newOrderInfo);
            }

            return checkoutResult;
        }

    }
}
