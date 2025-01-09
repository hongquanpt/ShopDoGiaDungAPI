using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Attributes;
using ShopDoGiaDungAPI.Services;
using ShopDoGiaDungAPI.Services.Interfaces;
using System;
using System.Security.Claims;

namespace ShopDoGiaDungAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IRoleService _roleService;
        private readonly ILogService _logService;
        public AccountController(IAccountService accountService, IRoleService roleService, ILogService logService)
        {
            _accountService = accountService;
            _roleService = roleService;
            _logService = logService;
        }
        [Permission("QuanLyTaiKhoan", "Xem")]
        [HttpGet("accounts")]
        public async Task<IActionResult> QuanLyTK(int page = 1, int pageSize = 10)
        {
            // Lấy userId từ claim NameIdentifier (JWT)
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";

            // Lấy IP
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Ghi log: hành động "Xem danh sách tài khoản"
            await _logService.InsertLogAsync(
                userId: currentUserId,
                action: "Xem danh sách tài khoản",
                objects: $"Xem tài khoản page={page}, pageSize={pageSize}",
                ip: ip
            );

            // Thực hiện logic lấy danh sách tài khoản
            return _accountService.GetAccounts(page, pageSize);
        }


        [Permission("QuanLyTaiKhoan", "Sua")]
        [HttpPut("accounts/{matk}/role")]
        public IActionResult SuaCV(int matk, [FromBody] int macv)
        {
            return _accountService.UpdateAccountRole(matk, macv);
        }

        [Permission("QuanLyTaiKhoan", "Xoa")]
        [HttpDelete("accounts/{matk}")]
        public IActionResult XoaTK(int matk)
        {
            // 1. Xóa tài khoản
            var result = _accountService.DeleteAccount(matk);

            // 2. Sau khi xóa, nếu thành công, ghi log
            // Lấy userId từ NameIdentifier
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";

            // Lấy IP
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Log "Đã xóa tài khoản..."
            _logService.InsertLogAsync(
                userId: currentUserId,
                action: "Xóa tài khoản",
                objects: $"Đã xóa tài khoản ID={matk}",
                ip: ip
            );

            return result;
        }

        [Permission("Access", "Xem")]
        [HttpGet("accounts/{maTaiKhoan}")]
        public async Task<IActionResult> GetAccount(int maTaiKhoan)
        {
            var account = await _accountService.GetAccountByIdAsync(maTaiKhoan);

            if (account == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            return Ok(account);
        }

        [Permission("QuanLyTaiKhoan", "Xem")]
        [HttpGet("{userId}/roles")]
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            var roles = await _roleService.GetUserRolesAsync(userId);
            if (roles == null || roles.Count == 0)
            {
                return NotFound(new { message = "User roles not found." });
            }
            return Ok(roles);
        }

        [Permission("QuanLyTaiKhoan", "Them")]
        [HttpPost("{userId}/roles/{roleId}")]
        public async Task<IActionResult> AssignRoleToUser(int userId, int roleId)
        {
            try
            {
                await _roleService.AssignRoleToUserAsync(userId, roleId);
                return Ok(new { message = "Role assigned successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while assigning the role.", details = ex.Message });
            }
        }

        [Permission("QuanLyTaiKhoan", "Xoa")]
        [HttpDelete("{userId}/roles/{roleId}")]
        public async Task<IActionResult> RemoveRoleFromUser(int userId, int roleId)
        {
            try
            {
                await _roleService.RemoveRoleFromUserAsync(userId, roleId);
                return Ok(new { message = "Role removed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing the role.", details = ex.Message });
            }
        }
    }
}
