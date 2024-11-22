using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.DTO;

namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<IActionResult> Login(LoginInfo loginInfo);
        Task<AuthResult> LoginAsync(LoginInfo loginInfo);
        Task<IActionResult> Register(RegisterInfo registerInfo);
        UserDto GetUserById(string userId);
        //Task<List<PermissionDto>> GetPermissionsByUserId(int userId);
    }
}
