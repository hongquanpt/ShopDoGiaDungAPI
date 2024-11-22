using ShopDoGiaDungAPI.Models;

namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface ITaiKhoanService
    {
        Task<List<ChucVu2>> GetRolesByUserAsync(int userId);
        Task AssignRolesToUserAsync(int userId, List<int> roleIds);
        Task<List<PhanQuyen>> GetUserPermissionsAsync(int userId);
    }

}
