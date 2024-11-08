using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("danhmucs")]
        public IActionResult QuanLyDM(string tendm = "", int madm = 0, int page = 1, int pageSize = 10)
        {
            return _categoryService.GetCategories(tendm, madm, page, pageSize);
        }
        [Authorize(Roles = "admin")]
        [HttpPost("danhmucs")]
        public IActionResult ThemDM([FromBody] string tendm)
        {
            return _categoryService.AddCategory(tendm);
        }
        [Authorize(Roles = "admin")]
        [HttpPut("danhmucs/{id}")]
        public IActionResult SuaDM(int id, [FromBody] string name)
        {
            return _categoryService.UpdateCategory(id, name);
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("danhmucs/{madm}")]
        public IActionResult XoaDM(int madm)
        {
            return _categoryService.DeleteCategory(madm);
        }
    }
}
