using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaiKhoanController : ControllerBase
    {
        private readonly ITaiKhoanService _taiKhoanService;
        private readonly OnlineShopContext _context;
        public TaiKhoanController(ITaiKhoanService taiKhoanService, OnlineShopContext context)
        {
            _taiKhoanService = taiKhoanService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Taikhoans
                .Include(tk => tk.TaiKhoanChucVus)
                    .ThenInclude(tkcv => tkcv.MaChucVuNavigation)
                .ToListAsync();

            var userDtos = users.Select(user => new TaiKhoanDto
            {
                MaTaiKhoan = user.MaTaiKhoan,
                Ten = user.Ten,
                NgaySinh = user.NgaySinh,
                Sdt = user.Sdt,
                DiaChi = user.DiaChi,
                Email = user.Email,
                MaDonVi = user.MaDonVi,
                ChucVus = user.TaiKhoanChucVus.Select(tkcv => new ChucVu2
                {
                    MaChucVu = tkcv.MaChucVu,
                    TenChucVu = tkcv.MaChucVuNavigation.TenChucVu
                }).ToList()
            }).ToList();

            return Ok(userDtos);
        }


        [HttpGet("{userId}/roles")]
        public async Task<IActionResult> GetRolesByUser(int userId)
        {
            var roles = await _taiKhoanService.GetRolesByUserAsync(userId);
            return Ok(roles);
        }

        [HttpPost("{userId}/roles")]
        public async Task<IActionResult> AssignRolesToUser(int userId, [FromBody] List<int> roleIds)
        {
            await _taiKhoanService.AssignRolesToUserAsync(userId, roleIds);
            return Ok();
        }
    }

}
