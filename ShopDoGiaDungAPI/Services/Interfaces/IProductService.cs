using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Models;

namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface IProductService
    {
        // Admin functions
        Task<IActionResult> GetProducts(int page, int pageSize);
        Task<IActionResult> AddProduct(Sanpham product, IFormFile[] images, string category, string brand);
        Task<IActionResult> DeleteProduct(int productId);
        Task<IActionResult> UpdateProduct(Sanpham product, IFormFile[] images, string category, string brand);

        // Home functions
        Task<IActionResult> GetTopSellingProducts();
        Task<IActionResult> GetProductsByBrand(int brandId, string brandName, int pageIndex, int pageSize, int maxPrice, int minPrice, string orderPrice);
        Task<IActionResult> GetProductsByCategory(int categoryId, string categoryName, int pageIndex, int pageSize, int maxPrice, int minPrice, string orderPrice);
        Task<IActionResult> GetProductDetail(int productId);
        Task<IActionResult> GetAllProducts(int pageIndex, int pageSize, int maxPrice, int minPrice, string orderPrice);
        Task<IActionResult> SearchProducts(string search, int pageIndex, int pageSize, int maxPrice, int minPrice, string orderPrice);
    }
}
