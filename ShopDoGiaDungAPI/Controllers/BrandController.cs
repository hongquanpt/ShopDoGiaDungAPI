using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _brandService;

        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }
        [AllowAnonymous]
        [HttpGet("hangs")]
        public IActionResult QuanLyHang(string tenhang = "", int mahang = 0, int page = 1, int pageSize = 10)
        {
            return _brandService.GetBrands(tenhang, mahang, page, pageSize);
        }
        [Authorize(Roles = "admin")]
        [HttpPost("hangs")]
        public IActionResult ThemHang([FromBody] string tenhang)
        {
            return _brandService.AddBrand(tenhang);
        }
        [Authorize(Roles = "admin")]
        [HttpPut("hangs/{id}")]
        public IActionResult SuaHang(int id, [FromBody] string name)
        {
            return _brandService.UpdateBrand(id, name);
        }
        [Authorize(Roles = "admin")]

        [HttpDelete("hangs/{id}")]
        public IActionResult XoaHang(int id)
        {
            return _brandService.DeleteBrand(id);
        }
    }
}
