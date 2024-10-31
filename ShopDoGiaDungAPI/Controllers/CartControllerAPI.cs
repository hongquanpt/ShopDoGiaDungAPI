
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Controllers
{
    [AllowAnonymous]
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

        [HttpPost("AddItem/{productId}")]
        public JsonResult AddItem(int productId, bool checkOnly = false)
        {
            return _cartService.AddItemToCart(productId, HttpContext.Session, checkOnly);
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
            int ? userId = HttpContext.Session.GetInt32("Ma");
           

            return await _cartService.Checkout(thanhToan, userId);
        }
    }
}
