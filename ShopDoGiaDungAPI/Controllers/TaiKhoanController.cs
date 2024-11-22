using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaiKhoanController : ControllerBase
    {
        private readonly ITaiKhoanService _taiKhoanService;
        private readonly OnlineShopContext _context;
        public TaiKhoanController(ITaiKhoanService taiKhoanService, OnlineShopContext context)
        {
            _taiKhoanService = taiKhoanService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTaiKhoans()
        {
            var taiKhoans = await _taiKhoanService.GetAllTaiKhoansAsync();
            return Ok(taiKhoans);
        }


        [HttpGet("{userId}/roles")]
        public async Task<IActionResult> GetRolesByUser(int userId)
        {
            var roles = await _taiKhoanService.GetRolesByUserAsync(userId);
            return Ok(roles);
        }

        [HttpPost("{userId}/roles")]
        public async Task<IActionResult> AssignRolesToUser(int userId, [FromBody] RoleAssignmentDto dto)
        {
            if (dto == null || dto.RoleIds == null || !dto.RoleIds.Any())
            {
                return BadRequest(new { message = "The roleIds field is required." });
            }

            var result = await _taiKhoanService.AssignRolesToUserAsync(userId, dto.RoleIds);
            if (result)
            {
                return Ok(new { message = "Cập nhật chức vụ thành công." });
            }
            else
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật chức vụ." });
            }
        }
        // GET: api/TaiKhoan/{userId}/permissions
        [HttpGet("{userId}/permissions")]
        public async Task<IActionResult> GetPermissionsByUser(int userId)
        {
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

            var result = await _taiKhoanService.AssignPermissionsToUserAsync(userId, dto.Permissions);
            if (result)
            {
                return Ok(new { message = "Cập nhật quyền thành công." });
            }
            else
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật quyền." });
            }
        }

    }

}
