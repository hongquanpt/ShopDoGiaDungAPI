using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Security.Claims;
using ShopDoGiaDungAPI.Attributes;
using ShopDoGiaDungAPI.Services.Implementations;

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

        public AccessControllerAPI(IAuthService authService, IPermissionService permissionService)
        {
            _authService = authService;
            _permissionService= permissionService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginInfo loginInfo)
        {
            var authResult = await _authService.LoginAsync(loginInfo);

            if (authResult.Token == null)
            {
                // Đăng nhập không thành công
                return BadRequest(new { message = authResult.Message });
            }

            // Đăng nhập thành công
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
        public IActionResult Logout()
        {
            // Thông báo cho client xóa JWT token
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
