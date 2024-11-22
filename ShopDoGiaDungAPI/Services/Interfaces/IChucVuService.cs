using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.DTO;
namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface IChucVuService
    {
        Task<List<ChucVu2>> GetAllRolesAsync();
        Task<List<PhanQuyenDto>> GetPermissionsByRoleAsync(int roleId);
        Task<bool> AssignPermissionsToRoleAsync(int roleId, List<PhanQuyenDto> permissions);
    }

}
