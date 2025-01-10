using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Attributes;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Security.Claims;

namespace ShopDoGiaDungAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("MyAllowedOrigins")]
    public class HomeControllerAPI : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly ILogService _logService;

        public HomeControllerAPI(IProductService productService, IOrderService orderService, IUserService userService,ILogService logService)
        {
            _productService = productService;
            _orderService = orderService;
            _userService = userService;
            _logService = logService;
        }

        [AllowAnonymous]
        [HttpGet("Search")]
        public async Task<IActionResult> Search(
        string? search,
        string? idCategories,
        string? idHangs,
        int pageIndex = 1,
        int pageSize = 100,
        string maxPrice = "100000000",
        string minPrice = "0",
        string orderPrice = "tang")
        {
            return await _productService.SearchProducts(
                search,
                idCategories,
                idHangs,
                pageIndex,
                pageSize,
                maxPrice,
                minPrice,
                orderPrice);
        }

        [AllowAnonymous]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            return await _productService.GetTopSellingProducts();
        }

        [AllowAnonymous]
        [HttpGet("AllProduct")]
        public async Task<IActionResult> AllProduct(int PageIndex = 1, int PageSize = 100, int maxPrice = 0, int minPrice = 0, string orderPrice = "tang")
        {
            return await _productService.GetAllProducts(PageIndex, PageSize, maxPrice, minPrice, orderPrice);
        }

        [AllowAnonymous]
        [HttpGet("SPHang")]
        public async Task<IActionResult> SPHang(int idHang, string ten, int PageIndex = 1, int PageSize = 100, int maxPrice = 0, int minPrice = 0, string orderPrice = "tang")
        {
            return await _productService.GetProductsByBrand(idHang, ten, PageIndex, PageSize, maxPrice, minPrice, orderPrice);
        }

        [AllowAnonymous]
        [HttpGet("SPDanhMuc")]
        public async Task<IActionResult> SPDanhMuc(int idCategory, string ten, int PageIndex = 1, int PageSize = 100, int maxPrice = 0, int minPrice = 0, string orderPrice = "tang")
        {
            return await _productService.GetProductsByCategory(idCategory, ten, PageIndex, PageSize, maxPrice, minPrice, orderPrice);
        }

        [AllowAnonymous]
        [HttpGet("ProductDetail/{productId}")]
        public async Task<IActionResult> ProductDetail(int productId)
        {
            var sanpham = await _productService.GetProductDetailAsync(productId);

            if (sanpham == null)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại" });
            }

            return Ok(new { sanpham = sanpham });
        }
        [AllowAnonymous]
        [HttpPost("ProductDetail")]
        public async Task<IActionResult> ProductDetail([FromBody] ProductDetailRequest request)
        {
            if (request == null || request.Id <= 0)
            {
                return BadRequest(new { status = false, message = "Invalid request data" });
            }

            return await _productService.GetProductDetail(request.Id);
        }
        [Authorize]
        [Permission("Access", "Xem")]
        [HttpGet("MyOrder")]
        public async Task<IActionResult> MyOrder(string typeMenu = "tatca", int PageIndex = 1, int PageSize = 100)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Ghi log: Xem đơn hàng của chính user
            await _logService.InsertLogAsync(
                userId: userId.ToString(),
                action: "Xem Đơn Hàng Cá Nhân",
                objects: $"typeMenu={typeMenu}, PageIndex={PageIndex}, PageSize={PageSize}",
                ip: ip
            );

            return await _orderService.GetUserOrders(userId, typeMenu, PageIndex, PageSize);
        }


        [Authorize]
        [Permission("Access", "Sua")]
        [HttpPost("ChangeProfile")]
        public async Task<IActionResult> ChangeProfile([FromBody] TaiKhoanDto tk)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            // Ghi log: Thay đổi thông tin cá nhân
            await _logService.InsertLogAsync(
                userId: userIdClaim.Value,
                action: "Thay Đổi Thông Tin Cá Nhân",
                objects: $"Email={tk.Email}, SDT={tk.Sdt}, DiaChi={tk.DiaChi}",
                ip: ip
            );

            var result = await _userService.UpdateUserProfile(tk);
            if (result is OkObjectResult okResult)
            {
                dynamic response = okResult.Value;
                if (response.status == true)
                {
                    HttpContext.Session.SetString("email", tk.Email);
                    HttpContext.Session.SetString("SDT", tk.Sdt);
                    HttpContext.Session.SetString("DiaChi", tk.DiaChi);
                    return new OkObjectResult(response);
                }
            }
            return result;
        }


        [Authorize]
        [Permission("Access", "Xoa")]
        [HttpPost("HuyDonHang")]
        public async Task<IActionResult> HuyDonHang(int ma)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Ghi log: Hủy đơn hàng
            await _logService.InsertLogAsync(
                userId: userId.ToString(),
                action: "Hủy Đơn Hàng",
                objects: $"MaDonHang={ma}",
                ip: ip
            );

            return await _orderService.CancelUserOrder(ma, userId);
        }


        [Authorize]
        [Permission("Access", "Sua")]
        [HttpPost("DaNhanHang")]
        public async Task<IActionResult> DaNhanHang(int ma)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Ghi log: Xác nhận đã nhận hàng
            await _logService.InsertLogAsync(
                userId: userId.ToString(),
                action: "Xác Nhận Đã Nhận Hàng",
                objects: $"MaDonHang={ma}",
                ip: ip
            );

            return await _orderService.ConfirmOrderReceived(ma, userId);
        }

    }
}
