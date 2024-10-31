using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Security.Claims;

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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginInfo loginInfo)
        {
            return await _authService.Login(loginInfo);
        }

        [AllowAnonymous]
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

        // Thêm phương thức GetCurrentUser
        [Authorize]
        [HttpGet("GetCurrentUser")]
        public IActionResult GetCurrentUser()
        {
            // Lấy thông tin từ JWT token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            // Hoặc lấy thông tin chi tiết từ cơ sở dữ liệu
            var user = _authService.GetUserById(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Trả về thông tin người dùng
            return Ok(new
            {
                userId = user.Id,
                email = user.Email,
                fullName = user.FullName,
                roles = user.Roles // Trả về danh sách vai trò hoặc nối thành chuỗi nếu cần
                                   // Thêm các thuộc tính khác nếu cần
            });
        }

    }
}
