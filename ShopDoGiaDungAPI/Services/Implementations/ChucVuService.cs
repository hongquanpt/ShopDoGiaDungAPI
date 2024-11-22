using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class ChucVuService : IChucVuService
    {
        private readonly OnlineShopContext _context;

        public ChucVuService(OnlineShopContext context)
        {
            _context = context;
        }

        // Lấy tất cả các chức vụ
        public async Task<List<ChucVu2>> GetAllRolesAsync()
        {
            return await _context.ChucVu2s.ToListAsync();
        }

        // Lấy quyền của một chức vụ
        public async Task<List<PhanQuyenDto>> GetPermissionsByRoleAsync(int roleId)
        {
            return await _context.PhanQuyens
                .Where(pq => pq.MaChucVu == roleId)
                .Select(pq => new PhanQuyenDto
                {
                    MaChucNang = pq.MaChucNang ?? 0, // Sử dụng giá trị mặc định nếu null
                    MaHanhDong = pq.MaHanhDong ?? 0,
                    MaDonVi = pq.MaDonVi ?? 0
                })
                .ToListAsync();
        }

        // Gán quyền cho một chức vụ
        public async Task<bool> AssignPermissionsToRoleAsync(int roleId, List<PhanQuyenDto> permissions)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Xóa tất cả các quyền hiện có của chức vụ
                    var existingPermissions = _context.PhanQuyens.Where(pq => pq.MaChucVu == roleId);
                    _context.PhanQuyens.RemoveRange(existingPermissions);

                    // Thêm các quyền mới
                    var newPermissions = permissions.Select(p => new PhanQuyen
                    {
                        MaChucVu = roleId,
                        MaChucNang = p.MaChucNang,
                        MaHanhDong = p.MaHanhDong,
                        MaDonVi = p.MaDonVi
                    }).ToList();

                    _context.PhanQuyens.AddRange(newPermissions);
                    await _context.SaveChangesAsync();

                    // Commit transaction
                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    // Rollback transaction nếu có lỗi
                    await transaction.RollbackAsync();
                    // Log lỗi nếu cần thiết
                    return false;
                }
            }
        }
    }
}
