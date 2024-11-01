using Microsoft.AspNetCore.Http.HttpResults;
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

        // Trong UserService hoặc AccountService
        public async Task<IActionResult> UpdateUserProfile(TaiKhoanDto userDto)
        {
            var user = await _context.Taikhoans.FindAsync(userDto.MaTaiKhoan);
            if (user == null)
            {
                return new BadRequestObjectResult(new { status = false, message = "User not found" });
            }

            // Cập nhật thông tin từ userDto
            user.Ten = userDto.Ten;
            user.Email = userDto.Email;
            user.DiaChi = userDto.DiaChi;
            user.Sdt = userDto.Sdt;

            // Kiểm tra và chuyển đổi ngaySinh
            if (userDto.NgaySinh.HasValue)
            {
                user.NgaySinh = userDto.NgaySinh;
            }
            else
            {
                user.NgaySinh = null; // Nếu ngaySinh không có giá trị, đặt là null
            }

            // Lưu thay đổi
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { status = true });
        }


    }
}  
