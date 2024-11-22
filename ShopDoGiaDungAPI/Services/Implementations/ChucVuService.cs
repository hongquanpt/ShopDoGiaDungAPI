using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class ChucVuService : IChucVuService
    {
        private readonly OnlineShopContext _context;

        public ChucVuService(OnlineShopContext context)
        {
            _context = context;
        }

        public async Task<List<ChucVu2>> GetAllRolesAsync()
        {
            return await EntityFrameworkQueryableExtensions.ToListAsync(_context.ChucVu2s); 
        }

        public async Task<List<PhanQuyen>> GetPermissionsByRoleAsync(int roleId)
        {
            var permissions = await EntityFrameworkQueryableExtensions.ToListAsync(_context.PhanQuyens
                .Include(pq => pq.MaChucNangNavigation)
                .Include(pq => pq.MaHanhDongNavigation)
                .Include(pq => pq.MaDonViNavigation)
                .Where(pq => pq.MaChucVu == roleId));
            return permissions;
        }

        public async Task AssignPermissionsToRoleAsync(int roleId, List<TaiKhoanPhanQuyen> permissions)
        {
            // Xóa các quyền hiện có của chức vụ
            var existingPermissions = _context.PhanQuyens.Where(pq => pq.MaChucVu == roleId);
            _context.PhanQuyens.RemoveRange(existingPermissions);

            // Thêm quyền mới
            var newPermissions = permissions.Select(p => new PhanQuyen
            {
                MaChucVu = roleId,
                MaChucNang = p.MaChucNang,
                MaHanhDong = p.MaHanhDong,
                MaDonVi = p.MaDonVi
            });

            _context.PhanQuyens.AddRange(newPermissions);

            await _context.SaveChangesAsync();
        }
    }

}
