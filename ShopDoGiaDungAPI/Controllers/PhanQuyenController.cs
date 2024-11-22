using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Data;

namespace ShopDoGiaDungAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhanQuyenController : ControllerBase
    {
        private readonly OnlineShopContext _context;

        public PhanQuyenController(OnlineShopContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _context.PhanQuyens
                .Include(pq => pq.MaChucNangNavigation)
                .Include(pq => pq.MaHanhDongNavigation)
                .Include(pq => pq.MaDonViNavigation)
                .ToListAsync();

            return Ok(permissions);
        }
    }

}
