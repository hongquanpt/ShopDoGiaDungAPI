using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly OnlineShopContext _context;

        public AccountService(OnlineShopContext context)
        {
            _context = context;
        }

        public IActionResult GetAccounts( int page, int pageSize)
        {
            var query = from tk in _context.Taikhoans
                        join cv in _context.ChucVus on tk.MaCv equals cv.MaCv
                        select new
                        {
                            MaTaiKhoan = tk.MaTaiKhoan,
                            Ten = tk.Ten,
                            Sdt = tk.Sdt,
                            DiaChi = tk.DiaChi,
                            NgaySinh = tk.NgaySinh,
                            MatKhau = tk.MatKhau,
                            TenChucVu = cv.Ten,
                            Email = tk.Email,
                            MaCV = cv.MaCv
                        };

           
            var totalItemCount = query.Count();
            var model = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Lấy danh sách chức vụ để trả về (nếu cần)
            var chucvuList = _context.ChucVus.ToList();

            return new OkObjectResult(new
            {
                data = model,
                totalItems = totalItemCount,
                pageStartItem = (page - 1) * pageSize + 1,
                pageEndItem = Math.Min(page * pageSize, totalItemCount),
                page = page,
                pageSize = pageSize,
                
                chucvu = chucvuList
            });
        }

        public IActionResult UpdateAccountRole(int matk, int macv)
        {
            var tk = _context.Taikhoans.Find(matk);
            if (tk != null)
            {
                tk.MaCv = macv;
                _context.SaveChanges();
                return new OkObjectResult(new { status = true });
            }
            else
            {
                return new NotFoundObjectResult(new { status = false, message = "Không tìm thấy tài khoản" });
            }
        }

        public IActionResult DeleteAccount(int matk)
        {
            var tk = _context.Taikhoans.Find(matk);
            if (tk != null)
            {
                _context.Taikhoans.Remove(tk);
                _context.SaveChanges();
                return new OkObjectResult(new { status = true });
            }
            else
            {
                return new NotFoundObjectResult(new { status = false, message = "Không tìm thấy tài khoản" });
            }
        }
    }
}
