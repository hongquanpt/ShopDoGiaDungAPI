using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.DTO;
using Microsoft.AspNetCore.Mvc;
namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface IChucVuService
    {
        Task<List<ChucVu2>> GetAllRolesAsync();
        Task<List<PhanQuyenDto>> GetPermissionsByRoleAsync(int roleId);
        Task<bool> AssignPermissionsToRoleAsync(int roleId, List<PhanQuyenDto> permissions);
        Task<bool> AddRoleAsync(ChucVu2 role);
        Task<bool> UpdateRoleAsync(int roleId, ChucVu2 updatedRole);
        Task<bool> DeleteRoleAsync(int roleId);
        Task<List<Role>> GetRolesAsync(int page = 1, int pageSize = 10);
        IActionResult GetRole(int categoryId);
    }

}
