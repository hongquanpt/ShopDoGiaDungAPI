using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Attributes;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Implementations;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopDoGiaDungAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChucVuController : ControllerBase
    {
        private readonly IChucVuService _chucVuService;

        public ChucVuController(IChucVuService chucVuService)
        {
            _chucVuService = chucVuService;
        }
        // GET: api/Role/roles
        [Authorize]
        [Permission("QuanLyChucVu", "Xem")]
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var roles = await _chucVuService.GetRolesAsync(page, pageSize);
            return Ok(new { data = roles });
        }
        // GET: api/ChucVu
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _chucVuService.GetAllRolesAsync();
            return Ok(roles);
        }
        [Authorize]
        [Permission("QuanLyChucVu", "Xem")]
        [HttpGet("roles1/{roleId}")]
        public async Task<IActionResult> GetRoles(int roleId)
        {
            return _chucVuService.GetRole(roleId);
        }
        // GET: api/ChucVu/{roleId}/permissions
        [HttpGet("{roleId}/permissions")]
        public async Task<IActionResult> GetPermissionsByRole(int roleId)
        {
            var permissions = await _chucVuService.GetPermissionsByRoleAsync(roleId);
            return Ok(permissions);
        }

        // POST: api/ChucVu/{roleId}/permissions
        // POST: api/ChucVu/{roleId}/permissions
        [HttpPost("{roleId}/permissions")]
        public async Task<IActionResult> AssignPermissionsToRole(int roleId, [FromBody] PermissionAssignmentDto dto)
        {
            if (dto == null || dto.Permissions == null || dto.Permissions.Count == 0)
            {
                return BadRequest(new { message = "The permissions field is required and cannot be empty." });
            }

            var result = await _chucVuService.AssignPermissionsToRoleAsync(roleId, dto.Permissions);
            if (result)
            {
                return Ok(new { message = "Cập nhật quyền cho chức vụ thành công." });
            }
            else
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật quyền cho chức vụ." });
            }
        }
        // POST: api/ChucVu
        [Authorize]
        [Permission("QuanLyChucVu", "Them")]
        [HttpPost]
        public async Task<IActionResult> AddRole([FromBody] ChucVu2 newRole)
        {
            if (newRole == null || string.IsNullOrEmpty(newRole.TenChucVu))
            {
                return BadRequest(new { message = "Tên chức vụ không được để trống." });
            }

            var result = await _chucVuService.AddRoleAsync(newRole);
            if (result)
            {
                return Ok(new { message = "Thêm chức vụ thành công." });
            }
            else
            {
                return StatusCode(500, new { message = "Lỗi khi thêm chức vụ." });
            }
        }

        // PUT: api/ChucVu/{roleId}
        [Authorize]
        [Permission("QuanLyChucVu", "Sua")]
        [HttpPut("{roleId}")]
        public async Task<IActionResult> UpdateRole(int roleId, [FromBody] ChucVu2 updatedRole)
        {
            if (updatedRole == null || string.IsNullOrEmpty(updatedRole.TenChucVu))
            {
                return BadRequest(new { message = "Tên chức vụ không được để trống." });
            }

            var result = await _chucVuService.UpdateRoleAsync(roleId, updatedRole);
            if (result)
            {
                return Ok(new { message = "Cập nhật chức vụ thành công." });
            }
            else
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật chức vụ." });
            }
        }

        // DELETE: api/ChucVu/{roleId}
        [Authorize]
        [Permission("QuanLyChucVu", "Xoa")]
        [HttpDelete("{roleId}")]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            var result = await _chucVuService.DeleteRoleAsync(roleId);
            if (result)
            {
                return Ok(new { message = "Xóa chức vụ thành công." });
            }
            else
            {
                return StatusCode(500, new { message = "Lỗi khi xóa chức vụ." });
            }
        }

    }
}
