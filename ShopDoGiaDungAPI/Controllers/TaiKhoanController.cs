using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Implementations;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Security.Claims;

namespace ShopDoGiaDungAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaiKhoanController : ControllerBase
    {
        private readonly ITaiKhoanService _taiKhoanService;
        private readonly OnlineShopContext _context;
        private readonly ILogService _logService;
        public TaiKhoanController(ITaiKhoanService taiKhoanService, OnlineShopContext context, ILogService logService)
        {
            _taiKhoanService = taiKhoanService;
            _context = context;
            _logService = logService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTaiKhoans()
        {
            // Lấy userId từ claim NameIdentifier (JWT)
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";

            // Lấy IP
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Ghi log: hành động "Xem danh sách tài khoản"
            await _logService.InsertLogAsync(
                userId: currentUserId,
                action: "Xem danh sách tài khoản",
                objects: $"Xem tài khoản ",
                ip: ip
            );

            var taiKhoans = await _taiKhoanService.GetAllTaiKhoansAsync();
            return Ok(taiKhoans);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            try
            {
                // Thực hiện xóa
                var result = await _taiKhoanService.DeleteAccountAsync(id);

                // Lấy userId từ claim
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";

                // Lấy IP
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

                // Nếu xóa thành công, ghi log
                if (result)
                {
                    await _logService.InsertLogAsync(
                        userId: currentUserId,
                        action: "Xóa tài khoản",
                        objects: $"Đã xóa tài khoản ID={id}",
                        ip: ip
                    );

                    return Ok(new { message = "Tài khoản đã được xóa thành công." });
                }
                else
                {
                    return NotFound(new { message = "Tài khoản không tồn tại." });
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi (nếu muốn)
                // await _logService.InsertLogAsync(currentUserId, "Error", $"Lỗi khi xóa tài khoản ID={id}: {ex.Message}", ip);

                return StatusCode(500, new { message = "Đã có lỗi xảy ra khi xóa tài khoản." });
            }
        }


        [HttpGet("{userId}/roles")]
        public async Task<IActionResult> GetRolesByUser(int userId)
        {
            // Lấy userId từ JWT (người đang thực hiện) - *nếu cần* log ai đã "xem"
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Ghi log hành động xem Roles (nếu muốn)
            await _logService.InsertLogAsync(
                userId: currentUserId,
                action: "Xem Roles",
                objects: $"Xem Roles của tài khoản ID={userId}",
                ip: ip
            );

            var roles = await _taiKhoanService.GetRolesByUserAsync(userId);
            return Ok(roles);
        }


        // POST: api/TaiKhoan/{userId}/roles
        [HttpPost("{userId}/roles")]
        public async Task<IActionResult> AssignRolesToUser(int userId, [FromBody] RoleAssignmentDto dto)
        {
            if (dto == null || dto.RoleIds == null || dto.RoleIds.Count == 0)
            {
                return BadRequest(new { message = "The roleIds field is required and cannot be empty." });
            }

            try
            {
                var result = await _taiKhoanService.AssignRolesToUserAsync(userId, dto.RoleIds);

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

                if (result)
                {
                    // Ghi log
                    await _logService.InsertLogAsync(
                        userId: currentUserId,
                        action: "Thêm/Sửa Roles",
                        objects: $"Gán roles [{string.Join(",", dto.RoleIds)}] cho userId={userId}",
                        ip: ip
                    );

                    return Ok(new { message = "Cập nhật chức vụ cho tài khoản thành công." });
                }
                else
                {
                    return StatusCode(500, new { message = "Lỗi khi cập nhật chức vụ cho tài khoản." });
                }
            }
            catch (Exception ex)
            {
                // Log lỗi (nếu cần)
                return StatusCode(500, new { message = "Lỗi khi cập nhật chức vụ cho tài khoản.", details = ex.Message });
            }
        }

        // GET: api/TaiKhoan/{userId}/permissions
        [HttpGet("{userId}/permissions")]
        public async Task<IActionResult> GetPermissionsByUser(int userId)
        {
            // Có thể log nếu muốn biết ai đang "xem" permission
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _logService.InsertLogAsync(
                userId: currentUserId,
                action: "Xem Permissions",
                objects: $"Xem permissions của userId={userId}",
                ip: ip
            );

            var permissions = await _taiKhoanService.GetPermissionsByUserAsync(userId);
            return Ok(permissions);
        }


        // POST: api/TaiKhoan/{userId}/permissions
        [HttpPost("{userId}/permissions")]
        public async Task<IActionResult> AssignPermissionsToUser(int userId, [FromBody] PermissionAssignmentDto dto)
        {
            if (dto == null || dto.Permissions == null || !dto.Permissions.Any())
            {
                return BadRequest(new { message = "The permissions field is required." });
            }

            try
            {
                var result = await _taiKhoanService.AssignPermissionsToUserAsync(userId, dto.Permissions);

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

                if (result)
                {
                    // Ghi log
                    await _logService.InsertLogAsync(
                        userId: currentUserId,
                        action: "Thêm/Sửa Permissions",
                        objects: $"Gán permissions [{string.Join(", ", dto.Permissions)}] cho userId={userId}",
                        ip: ip
                    );

                    return Ok(new { message = "Cập nhật quyền thành công." });
                }
                else
                {
                    return StatusCode(500, new { message = "Lỗi khi cập nhật quyền." });
                }
            }
            catch (Exception ex)
            {
                // Log lỗi (nếu cần)
                return StatusCode(500, new { message = "Lỗi khi cập nhật quyền.", details = ex.Message });
            }
        }


    }

}
