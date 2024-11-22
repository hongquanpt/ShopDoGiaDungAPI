using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.DTO;
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

        // GET: api/ChucVu
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _chucVuService.GetAllRolesAsync();
            return Ok(roles);
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

    }
}
