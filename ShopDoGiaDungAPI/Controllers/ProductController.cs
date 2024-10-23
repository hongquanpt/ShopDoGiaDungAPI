using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly MinioService _minioService;

        public ProductController(IProductService productService, MinioService minioService)
        {
            _productService = productService;
            _minioService = minioService;
        }

        [HttpGet("QuanLySP")]
        public async Task<IActionResult> QuanLySP(int page = 1, int pageSize = 5)
        {
            return await _productService.GetProducts(page, pageSize);
        }

        [HttpPost("ThemSP")]
        public async Task<IActionResult> ThemSP([FromForm] Sanpham spmoi, [FromForm] IFormFile[] images, [FromForm] string DanhMuc, [FromForm] string Hang)
        {
            return await _productService.AddProduct(spmoi, images, DanhMuc, Hang);
        }

        [HttpDelete("XoaSP/{maSP}")]
        public async Task<IActionResult> XoaSP(int maSP)
        {
            return await _productService.DeleteProduct(maSP);
        }

        [HttpPut("SuaSP")]
        public async Task<IActionResult> SuaSP([FromForm] Sanpham spmoi, [FromForm] IFormFile[] images, [FromForm] string DanhMuc, [FromForm] string Hang)
        {
            return await _productService.UpdateProduct(spmoi, images, DanhMuc, Hang);
        }
    }
}
