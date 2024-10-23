using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccessControllerAPI : ControllerBase
    {
        private readonly IAuthService _authService;

        public AccessControllerAPI(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginInfo loginInfo)
        {
            return await _authService.Login(loginInfo);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterInfo registerInfo)
        {
            return await _authService.Register(registerInfo);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Nếu bạn đang sử dụng JWT, bạn có thể không cần method này hoặc chỉ cần thông báo cho client xóa JWT token
            return Ok(new { message = "Đăng xuất thành công" });
        }
    }
}
