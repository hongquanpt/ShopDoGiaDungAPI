using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.DTO;


namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface IRoleService
    {
        Task<IActionResult> GetRoles(int page, int pageSize);
        Task<IActionResult> AddRole(ChucVuQuyen roleDto);
        Task<IActionResult> UpdateRole(int roleId, ChucVuQuyen roleDto);
        Task<IActionResult> DeleteRole(int roleId);
        Task<IActionResult> GetPermissions(int roleId);
        Task<IActionResult> UpdatePermissions(int roleId, ChucVuQuyen permissionUpdateDto);
    }
}
