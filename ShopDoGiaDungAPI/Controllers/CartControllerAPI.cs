
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartControllerAPI : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartControllerAPI(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return _cartService.GetCart(HttpContext.Session);
        }

        [HttpPost("AddItem")]
        public JsonResult AddItem(int productId)
        {
            return _cartService.AddItemToCart(productId, HttpContext.Session);
        }

        [HttpGet("Total")]
        public ActionResult Total()
        {
            return _cartService.GetCartTotal(HttpContext.Session);
        }

        [HttpDelete("Delete/{id}")]
        public JsonResult Delete(long id)
        {
            return _cartService.DeleteItemFromCart(id, HttpContext.Session);
        }

        [HttpPut("Update")]
        public JsonResult Update(int productId, int amount)
        {
            return _cartService.UpdateCartItem(productId, amount, HttpContext.Session);
        }

        [HttpDelete("DeleteAll")]
        public JsonResult DeleteAll()
        {
            return _cartService.ClearCart(HttpContext.Session);
        }

        [HttpPost("ThanhToan")]
        public async Task<JsonResult> ThanhToan([FromBody] ThongTinThanhToan thanhToan)
        {
            var userId = HttpContext.Session.GetInt32("Ma");
            if (userId == null)
            {
                return new JsonResult(new { status = false, message = "Người dùng chưa đăng nhập" });
            }

            return await _cartService.Checkout(thanhToan, HttpContext.Session, userId.Value);
        }
    }
}
