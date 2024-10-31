using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;


namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface IRoleService
    {
        Task<List<Role>> GetUserRolesAsync(int userId);
        Task AssignRoleToUserAsync(int userId, int roleId);
        Task RemoveRoleFromUserAsync(int userId, int roleId);
    }

}
