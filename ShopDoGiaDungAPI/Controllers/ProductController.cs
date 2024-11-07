﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IMinioService _minioService;

        public ProductController(IProductService productService, IMinioService minioService)
        {
            _productService = productService;
            _minioService = minioService;
        }
        [AllowAnonymous]
        [HttpGet("QuanLySP")]
        public async Task<IActionResult> QuanLySP(int page = 1, int pageSize = 10000)
        {
            return await _productService.GetProducts(page, pageSize);
        }

        [HttpPost("ThemSP")]
        public async Task<IActionResult> ThemSP([FromForm] Sanpham spmoi, [FromForm] IFormFile[] images, [FromForm] int DanhMuc, [FromForm] int Hang)
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
        // POST: api/CartControllerAPI/UpdateCartItemQuantity
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
                // Sử dụng HTTP 400 Bad Request cho các lỗi liên quan đến yêu cầu
                return BadRequest(new { status = false, message });
            }
        }
    }
}
