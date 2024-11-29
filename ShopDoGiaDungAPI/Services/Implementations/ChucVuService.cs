using Microsoft.AspNetCore.Mvc;
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
        public IActionResult GetRole(int categoryId)
        {
            var query = _context.ChucVu2s.Find(categoryId);
            return new OkObjectResult(new
            {

                tendm = query.TenChucVu,
                madm = categoryId
            });
        }
        // Lấy tất cả chức vụ
        public async Task<List<Role>> GetRolesAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Roles
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        // Thêm một chức vụ mới
        public async Task<bool> AddRoleAsync(ChucVu2 role)
        {
            try
            {
                _context.ChucVu2s.Add(role);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                // Log lỗi nếu cần thiết
                return false;
            }
        }

        // Sửa thông tin một chức vụ
        public async Task<bool> UpdateRoleAsync(int roleId, ChucVu2 updatedRole)
        {
            var existingRole = await _context.ChucVu2s.FindAsync(roleId);
            if (existingRole == null)
            {
                return false; // Chức vụ không tồn tại
            }

            existingRole.TenChucVu = updatedRole.TenChucVu;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                // Log lỗi nếu cần thiết
                return false;
            }
        }

        // Xóa một chức vụ
        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            // Bắt đầu transaction
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Xóa tất cả các bản ghi liên quan trong bảng TaiKhoanChucVu
                    var accountRoles = _context.TaiKhoanChucVus.Where(tkcv => tkcv.MaChucVu == roleId);
                    _context.TaiKhoanChucVus.RemoveRange(accountRoles);

                    // Xóa tất cả các bản ghi liên quan trong bảng PhanQuyen
                    var permissions = _context.PhanQuyens.Where(pq => pq.MaChucVu == roleId);
                    _context.PhanQuyens.RemoveRange(permissions);

                    // Xóa chức vụ khỏi bảng ChucVu2
                    var role = await _context.ChucVu2s.FindAsync(roleId);
                    if (role == null)
                    {
                        return false; // Chức vụ không tồn tại
                    }

                    _context.ChucVu2s.Remove(role);

                    // Lưu tất cả thay đổi
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
