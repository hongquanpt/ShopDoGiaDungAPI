using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class TaiKhoanService : ITaiKhoanService
    {
        private readonly OnlineShopContext _context;

        public TaiKhoanService(OnlineShopContext context)
        {
            _context = context;
        }

        public async Task<List<ChucVu2>> GetRolesByUserAsync(int userId)
        {
            var roles = await EntityFrameworkQueryableExtensions.ToListAsync(_context.TaiKhoanChucVus
                .Where(tkcv => tkcv.MaTaiKhoan == userId)
                .Select(tkcv => tkcv.MaChucVuNavigation)
                );

            return roles;
        }

        public async Task AssignRolesToUserAsync(int userId, List<int> roleIds)
        {
            // Xóa các chức vụ hiện có của người dùng
            var existingRoles = _context.TaiKhoanChucVus.Where(tkcv => tkcv.MaTaiKhoan == userId);
            _context.TaiKhoanChucVus.RemoveRange(existingRoles);

            // Thêm chức vụ mới
            var newRoles = roleIds.Select(roleId => new TaiKhoanChucVu
            {
                MaTaiKhoan = userId,
                MaChucVu = roleId
            });

            _context.TaiKhoanChucVus.AddRange(newRoles);

            await _context.SaveChangesAsync();
        }

        public async Task<List<PhanQuyen>> GetUserPermissionsAsync(int userId)
        {
            // Lấy quyền từ chức vụ của người dùng
            var roleIds = await _context.TaiKhoanChucVus
                .Where(tkcv => tkcv.MaTaiKhoan == userId)
                .Select(tkcv => tkcv.MaChucVu)
                .ToListAsync();

            var permissionsFromRoles = await _context.PhanQuyens
                .Include(pq => pq.MaChucNangNavigation)
                .Include(pq => pq.MaHanhDongNavigation)
                .Include(pq => pq.MaDonViNavigation)
                .Where(pq => roleIds.Contains(pq.MaChucVu))
                .ToListAsync();

            // Lấy quyền cá nhân của người dùng
            var permissionsFromUser = await _context.TaiKhoanPhanQuyens
                .Include(pq => pq.MaChucNangNavigation)
                .Include(pq => pq.MaHanhDongNavigation)
                .Include(pq => pq.MaDonViNavigation)
                .Where(tpq => tpq.MaTaiKhoan == userId)
                .Select(tpq => new PhanQuyen
                {
                    MaChucNang = tpq.MaChucNang,
                    MaChucNangNavigation = tpq.MaChucNangNavigation,
                    MaHanhDong = tpq.MaHanhDong,
                    MaHanhDongNavigation = tpq.MaHanhDongNavigation,
                    MaDonVi = tpq.MaDonVi,
                    MaDonViNavigation = tpq.MaDonViNavigation
                })
                .ToListAsync();

            // Kết hợp và loại bỏ trùng lặp
            var allPermissions = permissionsFromRoles.Concat(permissionsFromUser)
                .GroupBy(p => new { p.MaChucNang, p.MaHanhDong, p.MaDonVi })
                .Select(g => g.First())
                .ToList();

            return allPermissions;
        }
    }

}
