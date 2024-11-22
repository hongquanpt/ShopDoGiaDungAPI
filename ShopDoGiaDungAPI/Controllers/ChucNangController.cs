using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.Models;

namespace ShopDoGiaDungAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChucNangController : ControllerBase
    {
        private readonly OnlineShopContext _context;

        public ChucNangController(OnlineShopContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllChucNangs()
        {
            var chucNangs = await EntityFrameworkQueryableExtensions.ToListAsync(_context.ChucNangs
                .Select(cn => new ChucNang
                {
                    MaChucNang = cn.MaChucNang,
                    TenChucNang = cn.TenChucNang
                })
                );
            return Ok(chucNangs);
        }

    }

}
