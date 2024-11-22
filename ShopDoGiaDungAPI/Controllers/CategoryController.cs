using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Attributes;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("MyAllowedOrigins")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
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
        public IActionResult ThemDM([FromBody] string tendm)
        {
            return _categoryService.AddCategory(tendm);
        }

        [Authorize]
        [Permission("QuanLyDanhMuc", "Sua")]
        [HttpPut("danhmucs/{id}")]
        public IActionResult SuaDM(int id, [FromBody] UpdateCategoryRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name))
            {
                return BadRequest(new { status = false, message = "The name field is required." });
            }

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
        public IActionResult XoaDM(int madm)
        {
            return _categoryService.DeleteCategory(madm);
        }
    }
}
