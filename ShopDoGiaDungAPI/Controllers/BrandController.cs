using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Attributes;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Security.Claims;

namespace ShopDoGiaDungAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("MyAllowedOrigins")]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _brandService;
        private readonly ILogService _logService;
        public BrandController(IBrandService brandService, ILogService logService)
        {
            _brandService = brandService;
            _logService = logService;
        }

        [AllowAnonymous]
        [HttpGet("hangs")]
        public async Task<IActionResult> QuanLyHang(string tenhang = "", int mahang = 0, int page = 1, int pageSize = 10)
        {
            return _brandService.GetBrands(tenhang, mahang, page, pageSize);
        }


        [Authorize]
        [Permission("QuanLyHang", "Them")]
        [HttpPost("hangs")]
        public async Task<IActionResult> ThemHang([FromBody] string tenhang)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Ghi log trước hoặc sau khi thêm
            await _logService.InsertLogAsync(
                userId: currentUserId,
                action: "Thêm Hãng",
                objects: $"Thêm tên Hãng = {tenhang}",
                ip: ip
            );

            return _brandService.AddBrand(tenhang);
        }


        [Authorize]
        [Permission("QuanLyHang", "Sua")]
        [HttpPut("hangs/{id}")]
        public async Task<IActionResult> SuaH(int id, [FromBody] UpdateCategoryRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name))
            {
                return BadRequest(new { status = false, message = "The name field is required." });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Ghi log "Sửa Hãng"
            // Nếu muốn log chi tiết "Từ name cũ sang name mới", bạn có thể lấy name cũ từ DB
            await _logService.InsertLogAsync(
                userId: currentUserId,
                action: "Sửa Hãng",
                objects: $"Sửa hãng ID={id} thành {request.Name}",
                ip: ip
            );

            var result = _brandService.UpdateBrand(id, request.Name);
            return Ok(result);
        }


        [Authorize]
        [Permission("QuanLyHang", "Xoa")]
        [HttpDelete("hangs/{id}")]
        public async Task<IActionResult> XoaHang(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Ghi log
            await _logService.InsertLogAsync(
                userId: currentUserId,
                action: "Xóa Hãng",
                objects: $"Xóa hãng ID={id}",
                ip: ip
            );

            return _brandService.DeleteBrand(id);
        }

        [Authorize]
        [Permission("QuanLyHang", "Xem")]
        [HttpGet("hangs/{id}")]
        public IActionResult Hang(int id)
        {
            return _brandService.GetBand(id);
        }
    }
}
