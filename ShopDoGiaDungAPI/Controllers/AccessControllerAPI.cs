using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ShopDoGiaDungAPI.DTO;

namespace ShopDoGiaDungAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccessControllerAPI : ControllerBase
    {
        private readonly OnlineShop2Context _context;

        public AccessControllerAPI(OnlineShop2Context context)
        {
            _context = context;
        }
        // Utility method to generate MD5 hash
        private string GetMD5(string str)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] fromData = Encoding.UTF8.GetBytes(str);
                byte[] targetData = md5.ComputeHash(fromData);
                StringBuilder byte2String = new StringBuilder();

                for (int i = 0; i < targetData.Length; i++)
                {
                    byte2String.Append(targetData[i].ToString("x2"));
                }

                return byte2String.ToString();
            }
        }

        // Login API
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginInfo loginInfo)
        {
            try
            {
                var user = await _context.Taikhoans.SingleOrDefaultAsync(c => c.Email == loginInfo.Email);
                if (user == null)
                {
                    return BadRequest(new { message = "Tài khoản không tồn tại" });
                }

                var f_password = GetMD5(loginInfo.Password);
                if (user.MatKhau != f_password)
                {
                    return BadRequest(new { message = "Mật khẩu không chính xác" });
                }

                List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, loginInfo.Email),
                new Claim("OtherProperties", "Example Role")
            };

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

                // Normally, you would generate a JWT token here and return it in the response
                // Example JWT logic is omitted here for brevity

                return Ok(new { message = "Đăng nhập thành công", roles });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đăng nhập thất bại", error = ex.Message });
            }
        }

        // Register API
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterInfo registerInfo)
        {
            try
            {
                var user = await _context.Taikhoans.SingleOrDefaultAsync(c => c.Email == registerInfo.Email);
                if (user != null)
                {
                    return BadRequest(new { message = "Tài khoản đã tồn tại" });
                }

                var f_password = GetMD5(registerInfo.Password);

                Taikhoan newTk = new Taikhoan
                {
                    Ten = registerInfo.Ten,
                    Email = registerInfo.Email,
                    MatKhau = f_password,
                    DiaChi = registerInfo.DiaChi,
                    Sdt = registerInfo.Sdt,
                    NgaySinh = registerInfo.NgaySinh.HasValue? DateOnly.FromDateTime(registerInfo.NgaySinh.Value): (DateOnly?)null,
                    MaCv = 3
                };

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

                return Ok(new { message = "Đăng ký thành công", roles });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đăng ký thất bại", error = ex.Message });
            }
        }

        // Logout API
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // In case you are using JWT, you don't need to clear sessions like in the previous MVC approach.
            // Instead, just let the JWT expire or invalidate it.
            return Ok(new { message = "Đăng xuất thành công" });
        }
    }
}
