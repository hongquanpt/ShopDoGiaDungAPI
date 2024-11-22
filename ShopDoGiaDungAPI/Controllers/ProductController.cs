using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Attributes;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("MyAllowedOrigins")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IMinioService _minioService;

        public ProductController(IProductService productService, IMinioService minioService)
        {
            _productService = productService;
            _minioService = minioService;
        }

        [Permission("QuanLySanPham", "Xem")]
        [HttpGet("QuanLySP")]
        public async Task<IActionResult> QuanLySP(int page = 1, int pageSize = 10000)
        {
            return await _productService.GetProducts(page, pageSize);
        }

        [Permission("QuanLySanPham", "Them")]
        [HttpPost("ThemSP")]
        public async Task<IActionResult> ThemSP([FromForm] SanphamDto model)
        {
            try
            {
                await _productService.AddProduct(model);
                return Ok(new { status = true, message = "Thêm sản phẩm thành công." });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [Permission("QuanLySanPham", "Xoa")]
        [HttpDelete("XoaSP/{maSP}")]
        public async Task<IActionResult> XoaSP(int maSP)
        {
            return await _productService.DeleteProduct(maSP);
        }

        [Permission("QuanLySanPham", "Sua")]
        [HttpPut("SuaSP")]
        public async Task<IActionResult> SuaSP([FromForm] Sanpham spmoi, [FromForm] IFormFile[] images, [FromForm] string DanhMuc, [FromForm] string Hang)
        {
            return await _productService.UpdateProduct(spmoi, images, DanhMuc, Hang);
        }
        [AllowAnonymous]
        [HttpPost("UpdateCartItemQuantity")]
        public async Task<IActionResult> UpdateCartItemQuantity([FromQuery] int productId, [FromQuery] int quantity)
        {
            var (isSuccess, message) = await _productService.UpdateCartItemQuantityAsync(productId, quantity);

            if (isSuccess)
            {
                return Ok(new { status = true, message });
            }
            else
            {
                return BadRequest(new { status = false, message });
            }
        }
    }
}
