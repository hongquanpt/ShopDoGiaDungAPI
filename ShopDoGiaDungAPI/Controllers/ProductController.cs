using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Attributes;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Security.Claims;

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
        private readonly ILogService _logService;
        public ProductController(IProductService productService, IMinioService minioService, ILogService logService)
        {
            _productService = productService;
            _minioService = minioService;
            _logService = logService;
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
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            try
            {
                await _productService.AddProduct(model);

                // Ghi log: Thêm sản phẩm
                await _logService.InsertLogAsync(
                    userId: currentUserId,
                    action: "Thêm Sản Phẩm",
                    objects: $"TenSP={model.TenSP}, Gia={model.GiaTien}, DanhMuc={model.DanhMuc}, Hang={model.Hang}",
                    ip: ip
                );

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
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Ghi log: Xóa sản phẩm
            await _logService.InsertLogAsync(
                userId: currentUserId,
                action: "Xóa Sản Phẩm",
                objects: $"maSP={maSP}",
                ip: ip
            );

            return await _productService.DeleteProduct(maSP);
        }


        [Permission("QuanLySanPham", "Sua")]
        [HttpPut("SuaSP")]
        public async Task<IActionResult> SuaSP([FromForm] Sanpham spmoi, [FromForm] IFormFile[] images, [FromForm] string DanhMuc, [FromForm] string Hang)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Ghi log: Sửa sản phẩm
            // Ở đây, nếu bạn cần log chi tiết cũ - mới, có thể lấy sản phẩm cũ trước khi cập nhật
            await _logService.InsertLogAsync(
                userId: currentUserId,
                action: "Sửa Sản Phẩm",
                objects: $"maSP={spmoi.MaSp}, TenSP={spmoi.TenSp}, DanhMuc={DanhMuc}, Hang={Hang}",
                ip: ip
            );

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
