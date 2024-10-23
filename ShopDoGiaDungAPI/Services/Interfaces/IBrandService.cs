using Microsoft.AspNetCore.Mvc;

namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface IBrandService
    {
        IActionResult GetBrands(string brandName, int brandId, int page, int pageSize);
        IActionResult AddBrand(string brandName);
        IActionResult UpdateBrand(int brandId, string brandName);
        IActionResult DeleteBrand(int brandId);
    }
}
