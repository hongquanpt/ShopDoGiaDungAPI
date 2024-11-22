using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Attributes;
using ShopDoGiaDungAPI.DTO;
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

        public CartControllerAPI(ICartService cartService)
        {
            _cartService = cartService;
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return new JsonResult(new { status = false, message = "User not authenticated" });
            }

            int userId = int.Parse (userIdClaim.Value);


            return await _cartService.Checkout(thanhToan, userId);
        }
    }
}
