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
    [EnableCors("MyAllowedOrigins")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogService _logService;
        public CategoryController(ICategoryService categoryService, ILogService logService)
        {
            _categoryService = categoryService;
            _logService = logService;
        }

        [AllowAnonymous]
        [HttpGet("danhmucs")]
        public IActionResult QuanLyDM(string tendm = "", int madm = 0, int page = 1, int pageSize = 10)
        {
            return _categoryService.GetCategories(tendm, madm, page, pageSize);
        }

        [Authorize]
        [Permission("QuanLyDanhMuc", "Them")]
        [HttpPost("danhmucs")]
        public async Task<IActionResult> ThemDM([FromBody] string tendm)
        {
            if (string.IsNullOrWhiteSpace(tendm))
            {
                return BadRequest("Tên danh mục không được để trống.");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Ghi log: Thêm danh mục
            await _logService.InsertLogAsync(
                userId: currentUserId,
                action: "Thêm danh mục",
                objects: $"Thêm danh mục tên={tendm}",
                ip: ip
            );

            return _categoryService.AddCategory(tendm);
        }


        [Authorize]
        [Permission("QuanLyDanhMuc", "Sua")]
        [HttpPut("danhmucs/{id}")]
        public async Task<IActionResult> SuaDM(int id, [FromBody] UpdateCategoryRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name))
            {
                return BadRequest(new { status = false, message = "The name field is required." });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Ghi log: Sửa danh mục
            await _logService.InsertLogAsync(
                userId: currentUserId,
                action: "Sửa danh mục",
                objects: $"Sửa danh mục ID={id} thành tên={request.Name}",
                ip: ip
            );

            var result = _categoryService.UpdateCategory(id, request.Name);
            return Ok(result);
        }


        [AllowAnonymous]
        [HttpGet("danhmucs/{id}")]
        public IActionResult DM(int id)
        {
            return _categoryService.GetCategorie(id);
        }

        [Authorize]
        [Permission("QuanLyDanhMuc", "Xoa")]
        [HttpDelete("danhmucs/{madm}")]
        public async Task<IActionResult> XoaDM(int madm)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Ghi log: Xóa danh mục
            await _logService.InsertLogAsync(
                userId: currentUserId,
                action: "Xóa danh mục",
                objects: $"Xóa danh mục ID={madm}",
                ip: ip
            );

            return _categoryService.DeleteCategory(madm);
        }
    }
}
