using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Security.Claims;
using ShopDoGiaDungAPI.Attributes;
using ShopDoGiaDungAPI.Services.Implementations;
using ShopDoGiaDungAPI.Services;

namespace ShopDoGiaDungAPI.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("MyAllowedOrigins")]
    [ApiController]
    public class AccessControllerAPI : ControllerBase
    {
        private readonly ILogger<AccessControllerAPI> _logger;
        private readonly IAuthService _authService;
        private readonly IPermissionService _permissionService;
        private readonly ILogService _logService;

        public AccessControllerAPI(IAuthService authService, IPermissionService permissionService, ILogService logService)
        {
            _authService = authService;
            _permissionService= permissionService;
            _logService = logService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginInfo loginInfo)
        {
            var authResult = await _authService.LoginAsync(loginInfo);

            // Ghi log - ví dụ:
            await _logService.InsertLogAsync(
                userId: authResult.User.Id,           
                action: "Login",                      
                objects: "Người dùng đăng nhập",       
                ip: Request.HttpContext.Connection.RemoteIpAddress?.ToString() 
            );

            if (authResult.Token == null)
            {
                return BadRequest(new { message = authResult.Message });
            }

            return Ok(new
            {
                message = authResult.Message,
                token = authResult.Token,
                user = authResult.User
            });
        }


        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterInfo registerInfo)
        {
            return await _authService.Register(registerInfo);
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var result = await _authService.CreateUserAsync(request);
            return result;
        }
        [Authorize]
        [HttpGet("GetUserPermissions")]
        public async Task<IActionResult> GetUserPermissions()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User is not authenticated." });
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return BadRequest(new { message = "Invalid user ID format." });
            }

            try
            {
                var permissions = await _permissionService.GetUserPermissions(userId);
                return Ok(permissions);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NullReferenceException ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message, stackTrace = ex.StackTrace });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", detail = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [Authorize]
        [Permission("Access", "Xem")]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Ghi log
            await _logService.InsertLogAsync(
                userId: User.Identity?.Name ?? "Unknown", // Nếu bạn lấy được userId từ token/claims
                action: "Logout",
                objects: "Người dùng đăng xuất",
                ip: Request.HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new { message = "Đăng xuất thành công" });
        }


        [Authorize]
        [Permission("Access", "Xem")]
        [HttpGet("GetCurrentUser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = userIdClaim.Value.ToString();
            var user =  _authService.GetUserById(userId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                userId = user.Id,
                email = user.Email,
                fullName = user.FullName,
                roles = user.Roles
            });
        }
    }
}
