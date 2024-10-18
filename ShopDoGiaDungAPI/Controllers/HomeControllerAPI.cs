using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.Models;
using  ShopDoGiaDungAPI.DTO;

namespace ShopDoGiaDungAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeControllerAPI : ControllerBase
    {
        private readonly OnlineShop2Context _context;
        private readonly ILogger<HomeControllerAPI> _logger;

        public HomeControllerAPI(ILogger<HomeControllerAPI> logger, OnlineShop2Context context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: api/Home
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var sanpham = await _context.Sanphams.OrderByDescending(a => a.SoLuongDaBan).Take(6).ToListAsync();
            var danhmucsp = await _context.Danhmucsanphams.ToListAsync();
            var hang = await _context.Hangsanxuats.ToListAsync();

            return Ok(new
            {
                sanpham = sanpham,
                danhmucsp = danhmucsp,
                hang = hang
            });
        }

        // GET: api/Home/SPHang
        [HttpGet("SPHang")]
        public async Task<IActionResult> SPHang(int idHang, string ten, int PageIndex = 1, int PageSize = 100, int maxPrice = 0, int minPrice = 0, string orderPrice = "tang")
        {
            IQueryable<Sanpham> query = _context.Sanphams.Where(s => s.MaHang == idHang);

            if (maxPrice != 0)
            {
                query = query.Where(item => item.GiaTien < maxPrice && item.GiaTien > minPrice && item.MaHang == idHang);
            }
            else
            {
                query = query.Where(s => s.MaHang == idHang);
            }
            if (orderPrice == "tang")
            {
                query = query.OrderBy(item => item.GiaTien);
            }
            if (orderPrice == "giam")
            {
                query = query.OrderByDescending(item => item.GiaTien);
            }

            var count = await query.CountAsync();
            var model = await query.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToListAsync();

            return Ok(new
            {
                sanpham = model,
                maxPrice = maxPrice,
                minPrice = minPrice,
                orderPrice = orderPrice,
                tenhang = ten,
                idHang = idHang,
                totalCount = count
            });
        }

        // GET: api/Home/SPDanhMuc
        [HttpGet("SPDanhMuc")]
        public async Task<IActionResult> SPDanhMuc(int idCategory, string ten, int PageIndex = 1, int PageSize = 100, int maxPrice = 0, int minPrice = 0, string orderPrice = "tang")
        {
            IQueryable<Sanpham> query = _context.Sanphams.Where(s => s.MaDanhMuc == idCategory);

            if (maxPrice != 0)
            {
                query = query.Where(item => item.GiaTien < maxPrice && item.GiaTien > minPrice);
            }

            query = orderPrice == "tang" ? query.OrderBy(item => item.GiaTien) : query.OrderByDescending(item => item.GiaTien);

            var count = await query.CountAsync();
            var model = await query.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToListAsync();

            return Ok(new
            {
                sanpham = model,
                maxPrice = maxPrice,
                minPrice = minPrice,
                orderPrice = orderPrice,
                tendanhmuc = ten,
                idCategory = idCategory,
                totalCount = count
            });
        }

        // GET: api/Home/ProductDetail/{id}
        [HttpGet("ProductDetail/{id}")]
        public async Task<IActionResult> ProductDetail(int id)
        {
            var danhgia = await (from a in _context.Taikhoans
                                 join b in _context.Danhgiasanphams on a.MaTaiKhoan equals b.MaTaiKhoan
                                 join c in _context.Sanphams on b.MaSp equals c.MaSp
                                 where c.MaSp == id
                                 orderby b.NgayDanhGia descending
                                 select new CommentView()
                                 {
                                     TenTaiKhoan = a.Ten,
                                     DanhGia = b.DanhGia,
                                     NoiDung = b.NoiDungBinhLuan,
                                     ThoiGian = b.NgayDanhGia
                                 }).ToListAsync();

            int? sum = danhgia.Sum(item => item.DanhGia);
            double sao = Math.Round((double)sum / danhgia.Count(), 1);

            var sanpham = await _context.Sanphams.FindAsync(id);

            return Ok(new
            {
                sanpham = sanpham,
                danhgia = danhgia,
                sao = sao
            });
        }

        // GET: api/Home/AllProduct
        [HttpGet("AllProduct")]
        public async Task<IActionResult> AllProduct(int PageIndex = 1, int PageSize = 100, int maxPrice = 0, int minPrice = 0, string orderPrice = "tang")
        {
            IQueryable<Sanpham> model = _context.Sanphams;

            if (maxPrice != 0)
            {
                model = model.Where(item => item.GiaTien < maxPrice && item.GiaTien > minPrice);
            }

            model = orderPrice == "tang" ? model.OrderBy(item => item.GiaTien) : model.OrderByDescending(item => item.GiaTien);

            var count = await model.CountAsync();
            var dt = await model.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToListAsync();

            return Ok(new
            {
                sanpham = dt,
                maxPrice = maxPrice,
                minPrice = minPrice,
                orderPrice = orderPrice,
                totalCount = count
            });
        }

        // GET: api/Home/Search
        [HttpGet("Search")]
        public async Task<IActionResult> Search(string search, int PageIndex = 1, int PageSize = 100, int maxPrice = 0, int minPrice = 0, string orderPrice = "tang")
        {
            IQueryable<Sanpham> model = _context.Sanphams.Where(s => s.TenSp.Contains(search));

            if (maxPrice != 0)
            {
                model = model.Where(item => item.GiaTien < maxPrice && item.GiaTien > minPrice);
            }

            model = orderPrice == "tang" ? model.OrderBy(item => item.GiaTien) : model.OrderByDescending(item => item.GiaTien);

            var count = await model.CountAsync();
            var dt = await model.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToListAsync();

            return Ok(new
            {
                sanpham = dt,
                search = search,
                maxPrice = maxPrice,
                minPrice = minPrice,
                orderPrice = orderPrice,
                totalCount = count
            });
        }

        // GET: api/Home/MyOrder
        [HttpGet("MyOrder")]
        public async Task<IActionResult> MyOrder(string typeMenu = "tatca", int PageIndex = 1, int PageSize = 100)
        {
            var ma = HttpContext.Session.GetInt32("Ma");

            IQueryable<Donhang> query = typeMenu switch
            {
                "tatca" => _context.Donhangs.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == ma),
                "chuathanhtoan" => _context.Donhangs.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == ma && a.TinhTrang == 0),
                "choxacnhan" => _context.Donhangs.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == ma && a.TinhTrang == 1),
                "dangvanchuyen" => _context.Donhangs.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == ma && a.TinhTrang == 2),
                "dahoanthanh" => _context.Donhangs.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == ma && a.TinhTrang == 3),
                "dahuy" => _context.Donhangs.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == ma && a.TinhTrang == 4),
                _ => _context.Donhangs.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == ma)
            };

            var count = await query.CountAsync();
            var dt = await query.OrderByDescending(item => item.MaDonHang).Skip((PageIndex - 1) * PageSize).Take(PageSize).ToListAsync();

            return Ok(new
            {
                donhang = dt,
                typeMenu = typeMenu,
                totalCount = count
            });
        }

        // POST: api/Home/ChangeProfile
        [HttpPost("ChangeProfile")]
        public async Task<IActionResult> ChangeProfile([FromBody] TaiKhoanDto tk)
        {
            var it = await _context.Taikhoans.FindAsync(tk.MaTaiKhoan);
            if (it == null)
            {
                return BadRequest("User not found");
            }

            it.Ten = tk.Ten;
            it.Email = tk.Email;
            it.DiaChi = tk.DiaChi;
            it.Sdt = tk.Sdt;
            it.NgaySinh = tk.NgaySinh;

            HttpContext.Session.SetString("email", it.Email);
            HttpContext.Session.SetString("SDT", it.Sdt);
            HttpContext.Session.SetString("DiaChi", it.DiaChi);

            await _context.SaveChangesAsync();

            return Ok(new { status = true });
        }

        // POST: api/Home/HuyDonHang
        [HttpPost("HuyDonHang")]
        public async Task<IActionResult> HuyDonHang(int ma)
        {
            var dh = await _context.Donhangs.FindAsync(ma);
            if (dh == null)
            {
                return NotFound();
            }

            dh.TinhTrang = 4;
            await _context.SaveChangesAsync();

            return Ok(new { status = true });
        }

        // POST: api/Home/DaNhanHang
        [HttpPost("DaNhanHang")]
        public async Task<IActionResult> DaNhanHang(int ma)
        {
            var dh = await _context.Donhangs.FindAsync(ma);
            if (dh == null)
            {
                return NotFound();
            }

            dh.TinhTrang = 3;
            await _context.SaveChangesAsync();

            return Ok(new { status = true });
        }
    }
}
