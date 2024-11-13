
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Security.Claims;

namespace ShopDoGiaDungAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("MyAllowedOrigins")]
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
        public JsonResult AddItem([FromBody] AddItemRequest request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return new JsonResult(new { status = false, message = "Invalid request data" });
            }

            // Chỉ gọi tới _cartService với các thuộc tính cần thiết
            return _cartService.AddItemToCart(request.ProductId, HttpContext.Session, request.CheckOnly);
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
            return _cartService.UpdateCartItemQuantity(productId, amount);
        }

        [HttpDelete("DeleteAll")]
        public JsonResult DeleteAll()
        {
            return _cartService.ClearCart(HttpContext.Session);
        }

        [HttpPost("ThanhToan")]
        public async Task<JsonResult> ThanhToan([FromBody] ThongTinThanhToan thanhToan)
        {
            // Lấy userId từ Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

           
            string userId = userIdClaim.Value.ToString();


            return await _cartService.Checkout(thanhToan, userId);
        }
    }
}
