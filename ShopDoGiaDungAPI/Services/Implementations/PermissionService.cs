using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class PermissionService : IPermissionService
    {
        private readonly OnlineShopContext _context;

        public PermissionService(OnlineShopContext context)
        {
            _context = context;
        }
        public async Task<List<PermissionDto>> GetUserPermissions(int userId)
        {
            // Lấy danh sách MaChucVu của người dùng
            var userRoles = await _context.TaiKhoanChucVus
                .Where(uc => uc.MaTaiKhoan == userId)
                .Select(uc => uc.MaChucVu)
                .ToListAsync();

            if (userRoles == null || !userRoles.Any())
            {
                throw new ArgumentNullException(nameof(userRoles), "User roles cannot be null or empty.");
            }

            // Lấy quyền từ PhanQuyen dựa trên MaChucVu
            var rolePermissions = await _context.PhanQuyens
                .Include(pq => pq.MaChucNangNavigation)
                .Include(pq => pq.MaHanhDongNavigation)
                .Where(pq => userRoles.Contains(pq.MaChucVu))
                .Select(pq => new PermissionDto
                {
                    FunctionCode = pq.MaChucNangNavigation.TenChucNang ?? "UnknownFunction",
                    ActionCode = pq.MaHanhDongNavigation.TenHanhDong ?? "UnknownAction"
                })
                .ToListAsync();

            // Lấy quyền cá nhân từ TaiKhoan_PhanQuyen
            var userPermissions = await _context.TaiKhoanPhanQuyens
                 .Include(pq => pq.MaChucNangNavigation)
                 .Include(pq => pq.MaHanhDongNavigation)
                 .Where(up => up.MaTaiKhoan == userId)
                 .Select(up => new PermissionDto
                 {
                     FunctionCode = up.MaChucNangNavigation.TenChucNang ?? "UnknownFunction",
                     ActionCode = up.MaHanhDongNavigation.TenHanhDong ?? "UnknownAction"
                 })
                 .ToListAsync();

            // Kết hợp và loại bỏ trùng lặp sử dụng HashSet
            var permissionsSet = new HashSet<PermissionDto>(rolePermissions);
            foreach (var perm in userPermissions)
            {
                permissionsSet.Add(perm);
            }

            return permissionsSet.ToList();
        }


        //public async Task<bool> HasPermission(int userId, string functionCode, string actionCode, int maDonVi)
        //{
        //    // Lấy MaChucNang và MaHanhDong từ functionCode và actionCode
        //    var function = await _context.ChucNangs.FirstOrDefaultAsync(f => f.TenChucNang == functionCode);
        //    var action = await _context.HanhDongs.FirstOrDefaultAsync(a => a.TenHanhDong == actionCode);

        //    if (function == null || action == null)
        //    {
        //        return false;
        //    }

        //    // Lấy danh sách MaChucVu của người dùng
        //    var userRoles = await _context.TaiKhoanChucVus
        //        .Where(uc => uc.MaTaiKhoan == userId)
        //        .Select(uc => uc.MaChucVu)
        //        .ToListAsync();

        //    // Lấy quyền từ PhanQuyen dựa trên MaChucVu
        //    var rolePermissions = await _context.PhanQuyens
        //        .Where(pq => userRoles.Contains(pq.MaChucVu) &&
        //                     pq.MaChucNang == function.MaChucNang &&
        //                     pq.MaHanhDong == action.MaHanhDong)
        //        .ToListAsync();

        //    // Lấy quyền cá nhân từ TaiKhoan_PhanQuyen
        //    var userPermissions = await _context.TaiKhoanPhanQuyens
        //        .Where(up => up.MaTaiKhoan == userId &&
        //                     up.MaChucNang == function.MaChucNang &&
        //                     up.MaHanhDong == action.MaHanhDong)
        //        .ToListAsync();

        //    // Kết hợp quyền
        //    var permissions = rolePermissions.Select(p => p.MaDonVi)
        //        .Concat(userPermissions.Select(p => p.MaDonVi))
        //        .ToList();

        //    // Kiểm tra người dùng có quyền trên đơn vị của họ hoặc đơn vị con
        //    var hasPermission = permissions.Any(p => p == maDonVi || IsSubUnit(p, maDonVi));

        //    return hasPermission;
        //}

        private bool IsSubUnit(int parentUnitId, int childUnitId)
        {
            // Triển khai hàm kiểm tra xem childUnitId có phải là đơn vị con của parentUnitId không
            // Bạn có thể sử dụng đệ quy hoặc duyệt cây đơn vị
            // Ví dụ đơn giản:
            if (parentUnitId == childUnitId)
            {
                return true;
            }

            var childUnit = _context.DonVis.Find(childUnitId);
            while (childUnit != null && childUnit.MaDonViCha != null)
            {
                if (childUnit.MaDonViCha == parentUnitId)
                {
                    return true;
                }
                childUnit = _context.DonVis.Find(childUnit.MaDonViCha);
            }

            return false;
        }
    }
}
