using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Attributes;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("MyAllowedOrigins")]
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

        [Authorize]
        [Permission("QuanLyHang", "Them")]
        [HttpPost("hangs")]
        public IActionResult ThemHang([FromBody] string tenhang)
        {
            return _brandService.AddBrand(tenhang);
        }

        [Authorize]
        [Permission("QuanLyHang", "Sua")]
        [HttpPut("hangs/{id}")]
        public IActionResult SuaH(int id, [FromBody] UpdateCategoryRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name))
            {
                return BadRequest(new { status = false, message = "The name field is required." });
            }

            var result = _brandService.UpdateBrand(id, request.Name);
            return Ok(result);
        }

        [Authorize]
        [Permission("QuanLyHang", "Xoa")]
        [HttpDelete("hangs/{id}")]
        public IActionResult XoaHang(int id)
        {
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
