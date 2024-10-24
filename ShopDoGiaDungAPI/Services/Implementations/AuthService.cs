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

                // Lấy vai trò của người dùng
                var role = await _context.ChucVus.FirstOrDefaultAsync(c => c.MaCv == user.MaCv);
                var roleName = role?.Ten ?? "User"; // Nếu không tìm thấy, mặc định là "User"

                // Tạo claims cho JWT
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.MaTaiKhoan.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, roleName) // Thêm vai trò vào claims
                };

                // Lấy các quyền của vai trò
                var permissions = from qcv in _context.CvQAs
                                  join q in _context.Quyens on qcv.MaQ equals q.MaQ
                                  join a in _context.ActionTs on qcv.MaA equals a.MaA
                                  where qcv.MaCv == user.MaCv
                                  select new
                                  {
                                      q.Ten,
                                      q.ControllerName,
                                      q.ActionName
                                  };

                foreach (var perm in permissions)
                {
                    claims.Add(new Claim("Permission", $"{perm.ControllerName}:{perm.ActionName}"));
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
                        roleName
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
                    NgaySinh = registerInfo.NgaySinh.HasValue ? DateOnly.FromDateTime(registerInfo.NgaySinh.Value) : (DateOnly?)null,
                    MaCv = 3 // Mã chức vụ mặc định, có thể thay đổi tùy theo yêu cầu
                };

                newUser.MatKhau = _passwordHasher.HashPassword(newUser, registerInfo.Password);

                _context.Taikhoans.Add(newUser);
                await _context.SaveChangesAsync();

                // Gán quyền hạn mặc định cho người dùng mới nếu cần

                return new OkObjectResult(new { message = "Đăng ký thành công" });
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                return new StatusCodeResult(500);
            }
        }
    }
}
