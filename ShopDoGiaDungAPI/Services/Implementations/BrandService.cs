using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class BrandService : IBrandService
    {
        private readonly OnlineShopContext _context;

        public BrandService(OnlineShopContext context)
        {
            _context = context;
        }

        public IActionResult GetBrands(string brandName, int brandId, int page, int pageSize)
        {
            var query = _context.Hangsanxuats.AsQueryable();

            if (!string.IsNullOrEmpty(brandName))
            {
                query = query.Where(dm => dm.TenHang.Contains(brandName));
            }

            if (brandId != 0)
            {
                query = query.Where(item => item.MaHang == brandId);
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
                tenhang = brandName,
                mahang = brandId
            });
        }

        public IActionResult AddBrand(string brandName)
        {
            var hsx = new Hangsanxuat
            {
                TenHang = brandName
            };
            _context.Hangsanxuats.Add(hsx);
            _context.SaveChanges();
            return new OkObjectResult(new { status = true });
        }

        public IActionResult UpdateBrand(int brandId, string brandName)
        {
            var hsx = _context.Hangsanxuats.Find(brandId);
            if (hsx != null)
            {
                hsx.TenHang = brandName;
                _context.SaveChanges();
                return new OkObjectResult(new { status = true });
            }
            else
            {
                return new NotFoundObjectResult(new { status = false });
            }
        }

        public IActionResult DeleteBrand(int brandId)
        {
            var hsx = _context.Hangsanxuats.Find(brandId);
            if (hsx != null)
            {
                _context.Hangsanxuats.Remove(hsx);
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
