using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly OnlineShopContext _context;

        public UserService(OnlineShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> UpdateUserProfile(TaiKhoanDto userDto)
        {
            var it = await _context.Taikhoans.FindAsync(userDto.MaTaiKhoan);
            if (it == null)
            {
                return new BadRequestObjectResult("User not found");
            }

            it.Ten = userDto.Ten;
            it.Email = userDto.Email;
            it.DiaChi = userDto.DiaChi;
            it.Sdt = userDto.Sdt;
            it.NgaySinh = userDto.NgaySinh;

            await _context.SaveChangesAsync();

            return new OkObjectResult(new { status = true });
        }
    }
}
