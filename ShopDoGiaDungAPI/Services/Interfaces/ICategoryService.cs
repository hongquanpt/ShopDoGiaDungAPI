using Microsoft.AspNetCore.Mvc;

namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface ICategoryService
    {
        IActionResult GetCategorie(int categoryId);
        IActionResult GetCategories(string categoryName, int categoryId, int page, int pageSize);
        IActionResult AddCategory(string categoryName);
        IActionResult UpdateCategory(int categoryId, string categoryName);
        IActionResult DeleteCategory(int categoryId);
    }
}
