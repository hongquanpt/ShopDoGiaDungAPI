using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.DTO;

namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<IActionResult> UpdateUserProfile(TaiKhoanDto userDto);
    }
}
