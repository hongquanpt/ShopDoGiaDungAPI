using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Security.Claims;


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

        [HttpGet("MyOrder")]
        public async Task<IActionResult> MyOrder(string typeMenu = "tatca", int PageIndex = 1, int PageSize = 100)
        {
            // Lấy userId từ Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            
            string userId = userIdClaim.Value.ToString();
           

            return await _orderService.GetUserOrders(userId, typeMenu, PageIndex, PageSize);
        }

        [HttpPost("ChangeProfile")]
        public async Task<IActionResult> ChangeProfile([FromBody] TaiKhoanDto tk)
        {
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
