using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.DTO;

namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface ICartService
    {
        IActionResult GetCart(ISession session);
        JsonResult AddItemToCart(int productId, ISession session);
        ActionResult GetCartTotal(ISession session);
        JsonResult DeleteItemFromCart(long productId, ISession session);
        JsonResult UpdateCartItem(int productId, int amount, ISession session);
        JsonResult ClearCart(ISession session);
        Task<JsonResult> Checkout(ThongTinThanhToan thanhToan, ISession session, int userId);
    }
}
