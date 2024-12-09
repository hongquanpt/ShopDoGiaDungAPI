using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;

namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<IActionResult> UpdateUserProfile(TaiKhoanDto userDto);
        Task<IEnumerable<Taikhoan>> GetAllUsersAsync();
    }
}
