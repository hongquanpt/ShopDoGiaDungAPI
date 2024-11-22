using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly OnlineShopContext _context;
        private readonly PasswordHasher<Taikhoan> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthService(OnlineShopContext context, IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Taikhoan>();
            _configuration = configuration;
        }

        public async Task<IActionResult> Login(LoginInfo loginInfo)
        {
            try
            {
                var user = await _context.Taikhoans.SingleOrDefaultAsync(c => c.Email == loginInfo.Email);
                if (user == null)
                {
                    return new BadRequestObjectResult(new { message = "Tài khoản không tồn tại" });
                }

                var result = _passwordHasher.VerifyHashedPassword(user, user.MatKhau, loginInfo.Password);
                if (result == PasswordVerificationResult.Failed)
                {
                    return new BadRequestObjectResult(new { message = "Mật khẩu không chính xác" });
                }
                var userId = user.MaTaiKhoan;
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

                // Lấy danh sách vai trò của người dùng
                var roles = await (from ur in _context.UserRoles
                                   join r in _context.Roles on ur.RoleId equals r.RoleId
                                   where ur.UserId == user.MaTaiKhoan
                                   select r.RoleName).ToListAsync();

                // Nếu người dùng không có vai trò nào, gán vai trò mặc định là "User"
                if (!roles.Any())
                {
                    roles.Add("User");
                }
              
                // Tạo claims cho JWT
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.MaTaiKhoan.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.MaTaiKhoan.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                };
                foreach (var permission in permissionsSet)
                {
                    claims.Add(new Claim("permissions", $"{permission.FunctionCode}:{permission.ActionCode}"));
                }
                // Thêm tất cả các vai trò vào claims
                foreach (var roleName in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleName));
                }

                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"])),
                    Issuer = _configuration["Jwt:Issuer"],
                    Audience = _configuration["Jwt:Audience"],
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return new OkObjectResult(new
                {
                    message = "Đăng nhập thành công",
                    token = tokenString,
                    user = new
                    {
                        user.MaTaiKhoan,
                        user.Ten,
                        user.Email,
                        user.DiaChi,
                        user.Sdt,
                        user.NgaySinh,
                        roles
                    }
                });
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                return new StatusCodeResult(500);
            }
        }
        public async Task<AuthResult> LoginAsync(LoginInfo loginInfo)
        {
            try
            {
                // Tìm kiếm người dùng theo email và nạp các liên kết cần thiết
                var user = await _context.Taikhoans
                    .Include(u => u.TaiKhoanChucVus)
                        .ThenInclude(uc => uc.MaChucVuNavigation)
                            .ThenInclude(cv => cv.PhanQuyens)
                                .ThenInclude(pq => pq.MaChucNangNavigation)
                    .Include(u => u.TaiKhoanPhanQuyens)
                        .ThenInclude(up => up.MaChucNangNavigation)
                    .SingleOrDefaultAsync(c => c.Email == loginInfo.Email);

                if (user == null)
                {
                    return new AuthResult
                    {
                        Message = "Tài khoản không tồn tại"
                    };
                }

                // Kiểm tra mật khẩu
                var result = _passwordHasher.VerifyHashedPassword(user, user.MatKhau, loginInfo.Password);
                if (result == PasswordVerificationResult.Failed)
                {
                    return new AuthResult
                    {
                        Message = "Mật khẩu không chính xác"
                    };
                }

                var userId = user.MaTaiKhoan;

                // Lấy danh sách MaChucVu của người dùng
                var userRoles = await _context.TaiKhoanChucVus
                    .Where(uc => uc.MaTaiKhoan == userId)
                    .Select(uc => uc.MaChucVu)
                    .ToListAsync();

                if (userRoles == null || !userRoles.Any())
                {
                    // Nếu người dùng không có vai trò nào, gán vai trò mặc định là "User"
                    userRoles = new List<int> { /* ID của vai trò "User" */ };
                    // Lưu ý: Bạn cần xác định ID của vai trò "User" trong hệ thống của mình
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

                // Lấy quyền cá nhân từ TaiKhoanPhanQuyens
                var userPermissions = await _context.TaiKhoanPhanQuyens
                     .Include(up => up.MaChucNangNavigation)
                     .Include(up => up.MaHanhDongNavigation)
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

                // Lấy danh sách vai trò của người dùng
                var roles = await _context.UserRoles
                    .Where(ur => ur.UserId == user.MaTaiKhoan)
                    .Join(_context.Roles, ur => ur.RoleId, r => r.RoleId, (ur, r) => r.RoleName)
                    .ToListAsync();

                // Nếu người dùng không có vai trò nào, gán vai trò mặc định là "User"
                if (!roles.Any())
                {
                    roles.Add("User");
                }

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.MaTaiKhoan.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.MaTaiKhoan.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                // Thêm tất cả các vai trò vào claims
                foreach (var roleName in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleName));
                }

                // Thêm tất cả các permissions vào claims với định dạng "FunctionCode:ActionCode"
                foreach (var permission in permissionsSet)
                {
                    claims.Add(new Claim("permissions", $"{permission.FunctionCode}:{permission.ActionCode}"));
                }

                // Tạo khóa bảo mật
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

                // Tạo token descriptor
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"])),
                    Issuer = _configuration["Jwt:Issuer"],
                    Audience = _configuration["Jwt:Audience"],
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                // Tạo token
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // Tạo UserDto để trả về
                var userDto = new UserDto
                {
                    Id = user.MaTaiKhoan.ToString(),
                    FullName = user.Ten,
                    Email = user.Email,
                    MaDonVi = user.MaDonVi ?? 0,
                    TenDonVi = user.MaDonViNavigation?.TenDonVi,
                    Permissions = permissionsSet.ToList(),
                    Roles = roles
                };

                return new AuthResult
                {
                    Message = "Đăng nhập thành công",
                    Token = tokenString,
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần thiết
                // _logger.LogError(ex, "Lỗi khi đăng nhập");
                return new AuthResult
                {
                    Message = "Đã xảy ra lỗi trên server"
                };
            }
        }
        public async Task<IActionResult> Register(RegisterInfo registerInfo)
        {
            try
            {
                var existingUser = await _context.Taikhoans.SingleOrDefaultAsync(c => c.Email == registerInfo.Email);
                if (existingUser != null)
                {
                    return new BadRequestObjectResult(new { message = "Tài khoản đã tồn tại" });
                }

                Taikhoan newUser = new Taikhoan
                {
                    Ten = registerInfo.Ten,
                    Email = registerInfo.Email,
                    DiaChi = registerInfo.DiaChi,
                    Sdt = registerInfo.Sdt,
                    NgaySinh = registerInfo.NgaySinh.HasValue ? DateOnly.FromDateTime(registerInfo.NgaySinh.Value) : (DateOnly?)null
                    // Không cần MaCv nữa
                };

                newUser.MatKhau = _passwordHasher.HashPassword(newUser, registerInfo.Password);

                _context.Taikhoans.Add(newUser);
                await _context.SaveChangesAsync();

                // Gán vai trò mặc định cho người dùng mới
                var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "user");
                if (defaultRole != null)
                {
                    UserRole userRole = new UserRole
                    {
                        UserId = newUser.MaTaiKhoan,
                        RoleId = defaultRole.RoleId
                    };
                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();
                }

                return new OkObjectResult(new { message = "Đăng ký thành công" });
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                return new StatusCodeResult(500);
            }
        }

        public UserDto GetUserById(string userId)
        {
            int id = int.Parse(userId);
            // Tìm người dùng trong cơ sở dữ liệu bằng userId
            var user = _context.Taikhoans
                .Where(u => u.MaTaiKhoan == id)
                .Select(u => new UserDto
                {
                    Id = u.MaTaiKhoan.ToString(),
                    FullName = u.Ten,
                    Email = u.Email,
                    Roles = (from ur in _context.UserRoles
                             join r in _context.Roles on ur.RoleId equals r.RoleId
                             where ur.UserId == u.MaTaiKhoan
                             select r.RoleName).ToList()
                    // Thêm các thuộc tính cần thiết khác
                })
                .FirstOrDefault();

            return user;
        }
        //public async Task<List<PermissionDto>> GetPermissionsByUserId(int userId)
        //{
        //    // Lấy danh sách MaChucVu của người dùng
        //    var userRoles = await _context.TaiKhoanChucVus
        //        .Where(uc => uc.MaTaiKhoan == userId)
        //        .Select(uc => uc.MaChucVu)
        //        .ToListAsync();

        //    // Lấy quyền từ PhanQuyen dựa trên MaChucVu
        //    var rolePermissions = await (from pq in _context.PhanQuyens
        //                                 where userRoles.Contains(pq.MaChucVu)
        //                                 select new PermissionDto
        //                                 {
        //                                     MaChucNang = pq.MaChucNang,
        //                                     MaHanhDong = pq.MaHanhDong,
        //                                     MaDonVi = pq.MaDonVi
        //                                 }).ToListAsync();

        //    // Lấy quyền cá nhân từ TaiKhoan_PhanQuyen
        //    var userPermissions = await (from up in _context.TaiKhoanPhanQuyens
        //                                 where up.MaTaiKhoan == userId
        //                                 select new PermissionDto
        //                                 {
        //                                     MaChucNang = up.MaChucNang,
        //                                     MaHanhDong = up.MaHanhDong,
        //                                     MaDonVi = up.MaDonVi
        //                                 }).ToListAsync();

        //    // Kết hợp quyền
        //    var permissions = rolePermissions.Concat(userPermissions).ToList();

        //    return permissions;
        //}
    }
}
