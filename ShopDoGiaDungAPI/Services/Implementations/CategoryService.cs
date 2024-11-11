using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly OnlineShopContext _context;

        public CategoryService(OnlineShopContext context)
        {
            _context = context;
        }

        public IActionResult GetCategories(string categoryName, int categoryId, int page, int pageSize)
        {
            var query = _context.Danhmucsanphams.AsQueryable();

            if (!string.IsNullOrEmpty(categoryName))
            {
                query = query.Where(dm => dm.TenDanhMuc.Contains(categoryName));
            }

            if (categoryId != 0)
            {
                query = query.Where(item => item.MaDanhMuc == categoryId);
            }

            var model = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var totalItemCount = query.Count();

            return new OkObjectResult(new
            {
                data = model,
                totalItems = totalItemCount,
                pageStartItem = (page - 1) * pageSize + 1,
                pageEndItem = System.Math.Min(page * pageSize, totalItemCount),
                page = page,
                pageSize = pageSize,
                tendm = categoryName,
                madm = categoryId
            });
        }
        public IActionResult GetCategorie( int categoryId)
        {
            var query = _context.Danhmucsanphams.Find(categoryId);
            return new OkObjectResult(new
            {
               
                tendm = query.TenDanhMuc,
                madm = categoryId
            });
        }
        public IActionResult AddCategory(string categoryName)
        {
            var dm = new Danhmucsanpham
            {
                TenDanhMuc = categoryName
            };
            _context.Danhmucsanphams.Add(dm);
            _context.SaveChanges();
            return new OkObjectResult(new { status = true });
        }

        public IActionResult UpdateCategory(int categoryId, string categoryName)
        {
            var dm = _context.Danhmucsanphams.Find(categoryId);
            if (dm != null)
            {
                dm.TenDanhMuc = categoryName;
                _context.SaveChanges();
                return new OkObjectResult(new { status = true });
            }
            else
            {
                return new NotFoundObjectResult(new { status = false });
            }
        }

        public IActionResult DeleteCategory(int categoryId)
        {
            var dm = _context.Danhmucsanphams.Find(categoryId);
            if (dm != null)
            {
                _context.Danhmucsanphams.Remove(dm);
                _context.SaveChanges();
                return new OkObjectResult(new { status = true });
            }
            else
            {
                return new NotFoundObjectResult(new { status = false });
            }
        }
    }
}
