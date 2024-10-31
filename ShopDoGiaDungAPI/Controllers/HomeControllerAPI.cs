using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class HomeControllerAPI : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public HomeControllerAPI(IProductService productService, IOrderService orderService, IUserService userService)
        {
            _productService = productService;
            _orderService = orderService;
            _userService = userService;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            return await _productService.GetTopSellingProducts();
        }

        [HttpGet("SPHang")]
        public async Task<IActionResult> SPHang(int idHang, string ten, int PageIndex = 1, int PageSize = 100, int maxPrice = 0, int minPrice = 0, string orderPrice = "tang")
        {
            return await _productService.GetProductsByBrand(idHang, ten, PageIndex, PageSize, maxPrice, minPrice, orderPrice);
        }

        [HttpGet("SPDanhMuc")]
        public async Task<IActionResult> SPDanhMuc(int idCategory, string ten, int PageIndex = 1, int PageSize = 100, int maxPrice = 0, int minPrice = 0, string orderPrice = "tang")
        {
            return await _productService.GetProductsByCategory(idCategory, ten, PageIndex, PageSize, maxPrice, minPrice, orderPrice);
        }

        [HttpGet("ProductDetail/{id}")]
        public async Task<IActionResult> ProductDetail(int id)
        {
            return await _productService.GetProductDetail(id);
        }

        [HttpGet("AllProduct")]
        public async Task<IActionResult> AllProduct(int PageIndex = 1, int PageSize = 100, int maxPrice = 0, int minPrice = 0, string orderPrice = "tang")
        {
            return await _productService.GetAllProducts(PageIndex, PageSize, maxPrice, minPrice, orderPrice);
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search(string? search, int PageIndex = 1, int PageSize = 100, int maxPrice =100000000, int minPrice = 0, string orderPrice = "tang")
        {
            return await _productService.SearchProducts(search, PageIndex, PageSize, maxPrice, minPrice, orderPrice);
        }

        [HttpGet("MyOrder")]
        public async Task<IActionResult> MyOrder(string typeMenu = "tatca", int PageIndex = 1, int PageSize = 100)
        {
            var userId = HttpContext.Session.GetInt32("Ma");
            if (userId == null)
            {
                return Unauthorized();
            }

            return await _orderService.GetUserOrders(userId.Value, typeMenu, PageIndex, PageSize);
        }

        [HttpPost("ChangeProfile")]
        public async Task<IActionResult> ChangeProfile([FromBody] TaiKhoanDto tk)
        {
            var result = await _userService.UpdateUserProfile(tk);
            if (result is OkObjectResult)
            {
                // Cập nhật session nếu cần
                HttpContext.Session.SetString("email", tk.Email);
                HttpContext.Session.SetString("SDT", tk.Sdt);
                HttpContext.Session.SetString("DiaChi", tk.DiaChi);
            }
            return result;
        }

        [HttpPost("HuyDonHang")]
        public async Task<IActionResult> HuyDonHang(int ma)
        {
            var userId = HttpContext.Session.GetInt32("Ma");
            if (userId == null)
            {
                return Unauthorized();
            }

            return await _orderService.CancelUserOrder(ma, userId.Value);
        }

        [HttpPost("DaNhanHang")]
        public async Task<IActionResult> DaNhanHang(int ma)
        {
            var userId = HttpContext.Session.GetInt32("Ma");
            if (userId == null)
            {
                return Unauthorized();
            }

            return await _orderService.ConfirmOrderReceived(ma, userId.Value);
        }
    }
}
