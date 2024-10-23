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

        public AuthService(OnlineShopContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Taikhoan>();
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

                // Tạo danh sách các quyền cho người dùng
                var roles = (from tk in _context.Taikhoans
                             join cv in _context.ChucVus on tk.MaCv equals cv.MaCv
                             join qcv in _context.CvQAs on cv.MaCv equals qcv.MaCv
                             join q in _context.Quyens on qcv.MaQ equals q.MaQ
                             join a in _context.ActionTs on qcv.MaA equals a.MaA
                             where tk.Email == loginInfo.Email
                             select new AccountRole
                             {
                                 MaTaiKhoan = tk.MaTaiKhoan,
                                 MaQ = q.MaQ,
                                 MaCv = cv.MaCv,
                                 TenCV = cv.Ten,
                                 TenQ = q.Ten,
                                 ControllerName = q.ControllerName,
                                 ActionName = q.ActionName,
                                 MaA = a.MaA,
                                 TenA = a.TenA,
                             }).ToList();

                // Tạo JWT token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes("your_secret_key"); // Thay "your_secret_key" bằng khóa bí mật của bạn
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.MaTaiKhoan.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        // Bạn có thể thêm các claim khác nếu cần
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Issuer = "your_issuer", // Thay "your_issuer" bằng giá trị phù hợp
                    Audience = "your_audience", // Thay "your_audience" bằng giá trị phù hợp
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return new OkObjectResult(new { message = "Đăng nhập thành công", token = tokenString, roles });
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        public async Task<IActionResult> Register(RegisterInfo registerInfo)
        {
            try
            {
                var user = await _context.Taikhoans.SingleOrDefaultAsync(c => c.Email == registerInfo.Email);
                if (user != null)
                {
                    return new BadRequestObjectResult(new { message = "Tài khoản đã tồn tại" });
                }

                Taikhoan newTk = new Taikhoan
                {
                    Ten = registerInfo.Ten,
                    Email = registerInfo.Email,
                    DiaChi = registerInfo.DiaChi,
                    Sdt = registerInfo.Sdt,
                    NgaySinh = registerInfo.NgaySinh.HasValue ? DateOnly.FromDateTime(registerInfo.NgaySinh.Value) : (DateOnly?)null,
                    MaCv = 3
                };

                newTk.MatKhau = _passwordHasher.HashPassword(newTk, registerInfo.Password);

                _context.Taikhoans.Add(newTk);
                await _context.SaveChangesAsync();

                var roles = (from tk in _context.Taikhoans
                             join cv in _context.ChucVus on tk.MaCv equals cv.MaCv
                             join qcv in _context.CvQAs on cv.MaCv equals qcv.MaCv
                             join q in _context.Quyens on qcv.MaQ equals q.MaQ
                             join a in _context.ActionTs on qcv.MaA equals a.MaA
                             where tk.Email == registerInfo.Email
                             select new AccountRole
                             {
                                 MaTaiKhoan = tk.MaTaiKhoan,
                                 MaQ = q.MaQ,
                                 MaCv = cv.MaCv,
                                 TenCV = cv.Ten,
                                 TenQ = q.Ten,
                                 ControllerName = q.ControllerName,
                                 ActionName = q.ActionName,
                                 MaA = a.MaA,
                                 TenA = a.TenA,
                             }).ToList();

                return new OkObjectResult(new { message = "Đăng ký thành công", roles });
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }
    }
}
