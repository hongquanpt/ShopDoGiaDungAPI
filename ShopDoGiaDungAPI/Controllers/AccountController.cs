using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Attributes;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IRoleService _roleService;

        public AccountController(IAccountService accountService, IRoleService roleService)
        {
            _accountService = accountService;
            _roleService = roleService;
        }

        [Permission("QuanLyTaiKhoan", "Xem")]
        [HttpGet("accounts")]
        public IActionResult QuanLyTK(int page = 1, int pageSize = 10)
        {
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
            return _accountService.DeleteAccount(matk);
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
