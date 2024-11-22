using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;

namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface ITaiKhoanService
    {
        Task<List<TaiKhoanDto>> GetAllTaiKhoansAsync();
        Task<List<ChucVu2>> GetRolesByUserAsync(int userId);
        Task<bool> AssignRolesToUserAsync(int userId, List<int> roleIds);
        Task<List<PhanQuyen>> GetUserPermissionsAsync(int userId);
        Task<bool> AssignPermissionsToUserAsync(int userId, List<PhanQuyenDto> permissions);
        Task<List<PhanQuyenDto>> GetPermissionsByUserAsync(int userId);
    }

}
