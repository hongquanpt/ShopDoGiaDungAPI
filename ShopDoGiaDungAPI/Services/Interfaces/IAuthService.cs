using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.DTO;

namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<IActionResult> Login(LoginInfo loginInfo);
        Task<IActionResult> Register(RegisterInfo registerInfo);
    }
}
