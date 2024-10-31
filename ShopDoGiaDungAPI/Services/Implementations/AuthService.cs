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
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.MaTaiKhoan.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                };

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
    }
}
