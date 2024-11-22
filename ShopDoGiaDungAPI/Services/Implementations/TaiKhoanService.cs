using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.DTO;
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
        public async Task<List<TaiKhoanDto>> GetAllTaiKhoansAsync()
        {
            var taiKhoans = await _context.Taikhoans
                .Include(tk => tk.TaiKhoanChucVus)
                    .ThenInclude(tkcv => tkcv.MaChucVuNavigation)
                .ToListAsync();

            var taiKhoanDtos = taiKhoans.Select(tk => new TaiKhoanDto
            {
                MaTaiKhoan = tk.MaTaiKhoan,
                Ten = tk.Ten,
                NgaySinh = tk.NgaySinh,
                DiaChi = tk.DiaChi,
                Sdt = tk.Sdt,
                Email = tk.Email,
                ChucVus = tk.TaiKhoanChucVus.Select(tkcv => new ChucVuDto
                {
                    MaChucVu = tkcv.MaChucVu,
                    TenChucVu = tkcv.MaChucVuNavigation.TenChucVu
                }).ToList()
            }).ToList();

            return taiKhoanDtos;
        }

        public async Task<bool> AssignRolesToUserAsync(int userId, List<int> roleIds)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var user = await _context.Taikhoans
                        .Include(u => u.TaiKhoanChucVus)
                        .FirstOrDefaultAsync(u => u.MaTaiKhoan == userId);

                    if (user == null)
                    {
                        return false; // User not found
                    }

                    // Lấy danh sách các role hiện tại của user
                    var existingRoleIds = user.TaiKhoanChucVus.Select(tc => tc.MaChucVu).ToList();

                    // Xác định roles cần thêm và roles cần xóa
                    var rolesToAdd = roleIds.Except(existingRoleIds).ToList();
                    var rolesToRemove = existingRoleIds.Except(roleIds).ToList();

                    // Thêm các roles mới
                    foreach (var roleId in rolesToAdd)
                    {
                        var chucVu = await _context.ChucVu2s.FindAsync(roleId);
                        if (chucVu != null)
                        {
                            user.TaiKhoanChucVus.Add(new Models.TaiKhoanChucVu
                            {
                                MaTaiKhoan = userId,
                                MaChucVu = roleId
                            });
                        }
                    }

                    // Xóa các roles đã bị loại bỏ
                    foreach (var roleId in rolesToRemove)
                    {
                        var taiKhoanChucVu = user.TaiKhoanChucVus.FirstOrDefault(tc => tc.MaChucVu == roleId);
                        if (taiKhoanChucVu != null)
                        {
                            _context.TaiKhoanChucVus.Remove(taiKhoanChucVu);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    // Log lỗi nếu cần thiết
                    return false;
                }
            }
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
        // Lấy quyền của người dùng dựa trên các chức vụ đã gán
        public async Task<List<PhanQuyenDto>> GetPermissionsByUserAsync(int userId)
        {
            // Lấy tất cả các chức vụ của người dùng
            var roleIds = await _context.TaiKhoanChucVus
                .Where(tkcv => tkcv.MaTaiKhoan == userId)
                .Select(tkcv => tkcv.MaChucVu)
                .ToListAsync();

            if (!roleIds.Any())
            {
                return new List<PhanQuyenDto>();
            }

            // Lấy tất cả các quyền từ các chức vụ
            var permissions = await _context.PhanQuyens
                .Where(pq => roleIds.Contains(pq.MaChucVu))
                .Select(pq => new PhanQuyenDto
                {
                    MaChucNang = pq.MaChucNang,
                    MaHanhDong = pq.MaHanhDong,
                    MaDonVi = pq.MaDonVi
                })
                .ToListAsync();

            // Loại bỏ các quyền trùng lặp
            var uniquePermissions = permissions
                .GroupBy(p => new { p.MaChucNang, p.MaHanhDong, p.MaDonVi })
                .Select(g => g.First())
                .ToList();

            return uniquePermissions;
        }

        // Gán quyền trực tiếp cho người dùng (nếu cần)
        public async Task<bool> AssignPermissionsToUserAsync(int userId, List<PhanQuyenDto> permissions)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Xóa tất cả các quyền hiện có của người dùng
                    var existingPermissions = _context.TaiKhoanPhanQuyens.Where(tpq => tpq.MaTaiKhoan == userId);
                    _context.TaiKhoanPhanQuyens.RemoveRange(existingPermissions);

                    // Thêm các quyền mới
                    var newPermissions = permissions.Select(p => new TaiKhoanPhanQuyen
                    {
                        MaTaiKhoan = userId,
                        MaChucNang = p.MaChucNang,
                        MaHanhDong = p.MaHanhDong,
                        MaDonVi = p.MaDonVi
                    }).ToList();

                    _context.TaiKhoanPhanQuyens.AddRange(newPermissions);
                    await _context.SaveChangesAsync();

                    // Commit transaction
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    // Rollback transaction nếu có lỗi
                    await transaction.RollbackAsync();
                    // Log lỗi nếu cần
                    return false;
                }
            }
        }

    }

}
