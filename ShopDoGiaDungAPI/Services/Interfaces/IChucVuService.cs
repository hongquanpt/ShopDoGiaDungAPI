using ShopDoGiaDungAPI.Models;

namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface IChucVuService
    {
        Task<List<ChucVu2>> GetAllRolesAsync();
        Task<List<PhanQuyen>> GetPermissionsByRoleAsync(int roleId);
        Task AssignPermissionsToRoleAsync(int roleId, List<TaiKhoanPhanQuyen> permissions);
    }

}
