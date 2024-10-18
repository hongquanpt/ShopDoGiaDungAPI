using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.DTO;
using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Services;

namespace ShopDoGiaDungAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminControllerAPI : ControllerBase
    {
        private readonly OnlineShopContext _context;

        public AdminControllerAPI(OnlineShopContext context)
        {
            _context = context;
        }
        // API trả về danh sách sản phẩm
        [HttpGet("sanpham")]
        public IActionResult GetSanpham()
        {
            var sanpham = _context.Sanphams.ToList();
            return Ok(new { sanpham });
        }
        // API quản lý quyền hạn
        [HttpGet("quanlyqh")]
        public IActionResult GetQuanLyQH(int page = 1, int pageSize = 10)
        {
            var query = from cv in _context.ChucVus
                        join qcv in _context.CvQAs on cv.MaCv equals qcv.MaCv
                        join q in _context.Quyens on qcv.MaQ equals q.MaQ
                        join a in _context.ActionTs on qcv.MaA equals a.MaA
                        select new ChucVuQuyen
                        {
                            MaCv = cv.MaCv,
                            MaQ = q.MaQ,
                            TenCV = cv.Ten,
                            TenQ = q.Ten,
                            ActionName = q.ActionName,
                            ControllerName = q.ControllerName,
                            MaA = qcv.MaA,
                            TenA = a.TenA
                        };

            var roles = query.ToList();
            var totalItemCount = roles.Count();
            var model = roles.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new
            {
                roles = model,
                page = page,
                pageSize = pageSize,
                totalItemCount = totalItemCount,
                pageStartItem = (page - 1) * pageSize + 1,
                pageEndItem = Math.Min(page * pageSize, totalItemCount)
            });
        }

        // API sửa quyền hạn
        [HttpPost("suaqh")]
        public IActionResult SuaQH(int macv, int maa, int maq, string newValue)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var tk = _context.CvQAs.FirstOrDefault(c => c.MaA == maa && c.MaQ == maq && c.MaCv == macv);
                    if (tk != null)
                    {
                        // Xóa đối tượng cũ
                        _context.CvQAs.Remove(tk);
                        _context.SaveChanges();

                        // Thêm đối tượng mới
                        var newCvQA = new CvQA
                        {
                            MaA = Convert.ToInt32(newValue),
                            MaQ = maq,
                            MaCv = macv
                        };

                        _context.CvQAs.Add(newCvQA);
                        _context.SaveChanges();

                        return Ok(new { success = true, message = "Update successful" });
                    }
                    else
                    {
                        return NotFound(new { success = false, message = "Record not found" });
                    }
                }
                else
                {
                    return BadRequest(new { success = false, message = "Invalid input data" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SuaQH: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Error updating record", error = ex.Message });
            }
        }

        // API thêm chức vụ mới
        [HttpPost("themcv")]
        public IActionResult ThemCV(string tencv)
        {
            if (string.IsNullOrEmpty(tencv))
            {
                return BadRequest(new { status = false, message = "Tên chức vụ không được để trống" });
            }

            try
            {
                int macv = _context.ChucVus.Max(c => c.MaCv);
                var cv = new ChucVu
                {
                    Ten = tencv,
                    MaCv = macv + 1
                };

                _context.ChucVus.Add(cv);

                List<int> maq = _context.Quyens.Select(c => c.MaQ).ToList();
                foreach (var q in maq)
                {
                    var ac = new CvQA
                    {
                        MaCv = macv + 1,
                        MaA = 1,
                        MaQ = q
                    };
                    _context.CvQAs.Add(ac);
                }

                _context.SaveChanges();
                return Ok(new { status = true, message = "Thêm chức vụ thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ThemCV: {ex.Message}");
                return StatusCode(500, new { status = false, message = "Thêm chức vụ thất bại", error = ex.Message });
            }
        }

        // API quản lý tài khoản chỉ với phân trang, không có lọc
        [HttpGet("quanlytk")]
        public IActionResult QuanLyTK(int page = 1, int pageSize = 10)
        {
            // Thực hiện truy vấn
            var query = from tk in _context.Taikhoans
                        join cv in _context.ChucVus on tk.MaCv equals cv.MaCv
                        select new TaiKhoanChucVu
                        {
                            MaTaiKhoan = tk.MaTaiKhoan,
                            Ten = tk.Ten,
                            Sdt = tk.Sdt,
                            DiaChi = tk.DiaChi,
                            NgaySinh = tk.NgaySinh.HasValue? tk.NgaySinh.Value.ToDateTime(TimeOnly.MinValue): (DateTime?)null,
                            MatKhau = tk.MatKhau,
                            TenChucVu = cv.Ten,
                            Email = tk.Email,
                            MaCV = cv.MaCv
                        };

            var totalItemCount = query.Count();
            var model = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Trả về kết quả JSON bao gồm thông tin phân trang
            return Ok(new
            {
                data = model,
                page = page,
                pageSize = pageSize,
                totalItemCount = totalItemCount,
                pageStartItem = (page - 1) * pageSize + 1,
                pageEndItem = Math.Min(page * pageSize, totalItemCount)
            });
        }

        // API sửa chức vụ tài khoản
        [HttpPut("suacv")]
        public IActionResult SuaCV(int matk, int macv)
        {
            var tk = _context.Taikhoans.Find(matk);
            if (tk != null)
            {
                tk.MaCv = macv;
                _context.SaveChanges();
                return Ok(new { status = true, message = "Cập nhật chức vụ thành công" });
            }
            else
            {
                return NotFound(new { status = false, message = "Không tìm thấy tài khoản" });
            }
        }

        // API xóa tài khoản
        [HttpDelete("xoatk")]
        public IActionResult XoaTK(int MaTK)
        {
            var tk = _context.Taikhoans.Find(MaTK);
            if (tk != null)
            {
                _context.Taikhoans.Remove(tk);
                _context.SaveChanges();
                return Ok(new { status = true, message = "Xóa tài khoản thành công" });
            }
            else
            {
                return NotFound(new { status = false, message = "Không tìm thấy tài khoản" });
            }
        }

        //[HttpPost("convert-Anhs")]
        //public IActionResult ConvertAnhsToBinary()
        //{
        //    // Đường dẫn tĩnh của thư mục chứa hình ảnh
        //    string basePath = @"C:\Users\Admin\Desktop\QuanLyShopDoGiaDung\wwwroot\Admin\Anhs";

        //    // Lấy danh sách tất cả sản phẩm có tên tệp ảnh trong các cột Anh1, Anh2, ..., Anh6
        //    var sanphams = _context.Sanphams.ToList();

        //    foreach (var sanpham in sanphams)
        //    {
        //        try
        //        {
        //            // Xử lý ảnh 1 (Anh1 -> Anh1)
        //            if (!string.IsNullOrEmpty(sanpham.Anh1))
        //            {
        //                string fullAnhPath = Path.Combine(basePath, sanpham.Anh1);
        //                if (System.IO.File.Exists(fullAnhPath))
        //                {
        //                    var AnhBytes = System.IO.File.ReadAllBytes(fullAnhPath);
        //                    sanpham.Anh1 = AnhBytes; // Gán dữ liệu ảnh 1 vào Anh1
        //                }
        //            }

        //            // Xử lý ảnh 2 (Anh2 -> Anh2)
        //            if (!string.IsNullOrEmpty(sanpham.Anh2))
        //            {
        //                string fullAnhPath = Path.Combine(basePath, sanpham.Anh2);
        //                if (System.IO.File.Exists(fullAnhPath))
        //                {
        //                    var AnhBytes = System.IO.File.ReadAllBytes(fullAnhPath);
        //                    sanpham.Anh2 = AnhBytes; // Gán dữ liệu ảnh 2 vào Anh2
        //                }
        //            }

        //            // Xử lý ảnh 3 (Anh3 -> Anh3)
        //            if (!string.IsNullOrEmpty(sanpham.Anh3))
        //            {
        //                string fullAnhPath = Path.Combine(basePath, sanpham.Anh3);
        //                if (System.IO.File.Exists(fullAnhPath))
        //                {
        //                    var AnhBytes = System.IO.File.ReadAllBytes(fullAnhPath);
        //                    sanpham.Anh3 = AnhBytes; // Gán dữ liệu ảnh 3 vào Anh3
        //                }
        //            }

        //            // Xử lý ảnh 4 (Anh4 -> Anh4)
        //            if (!string.IsNullOrEmpty(sanpham.Anh4))
        //            {
        //                string fullAnhPath = Path.Combine(basePath, sanpham.Anh4);
        //                if (System.IO.File.Exists(fullAnhPath))
        //                {
        //                    var AnhBytes = System.IO.File.ReadAllBytes(fullAnhPath);
        //                    sanpham.Anh4 = AnhBytes; // Gán dữ liệu ảnh 4 vào Anh4
        //                }
        //            }

        //            // Xử lý ảnh 5 (Anh5 -> Anh5)
        //            if (!string.IsNullOrEmpty(sanpham.Anh5))
        //            {
        //                string fullAnhPath = Path.Combine(basePath, sanpham.Anh5);
        //                if (System.IO.File.Exists(fullAnhPath))
        //                {
        //                    var AnhBytes = System.IO.File.ReadAllBytes(fullAnhPath);
        //                    sanpham.Anh5 = AnhBytes; // Gán dữ liệu ảnh 5 vào Anh5
        //                }
        //            }

        //            // Xử lý ảnh 6 (Anh6 -> Anh6)
        //            if (!string.IsNullOrEmpty(sanpham.Anh6))
        //            {
        //                string fullAnhPath = Path.Combine(basePath, sanpham.Anh6);
        //                if (System.IO.File.Exists(fullAnhPath))
        //                {
        //                    var AnhBytes = System.IO.File.ReadAllBytes(fullAnhPath);
        //                    sanpham.Anh6 = AnhBytes; // Gán dữ liệu ảnh 6 vào Anh6
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Lỗi khi chuyển đổi hình ảnh cho sản phẩm {sanpham.MaSp}: {ex.Message}");
        //        }
        //    }

        //    // Lưu các thay đổi vào CSDL
        //    _context.SaveChanges();

        //    return Ok(new { message = "Chuyển đổi hình ảnh thành công" });
        //}
        // GET: api/Product
        [HttpGet("QuanLySP")]
        public async Task<IActionResult> QuanLySP(int page = 1, int pageSize = 5)
        {
            // Lấy toàn bộ sản phẩm không áp dụng bộ lọc
            IQueryable<SanPhamct> query = from sp in _context.Sanphams
                                          join h in _context.Hangsanxuats on sp.MaHang equals h.MaHang
                                          join dm in _context.Danhmucsanphams on sp.MaDanhMuc equals dm.MaDanhMuc
                                          select new SanPhamct
                                          {
                                              MaSp = sp.MaSp,
                                              TenSp = sp.TenSp,
                                              MoTa = sp.MoTa,
                                              Anh1 = sp.Anh1,
                                              Anh2 = sp.Anh2,
                                              Anh3 = sp.Anh3,
                                              Anh4 = sp.Anh4,
                                              Anh5 = sp.Anh5,
                                              Anh6 = sp.Anh6,
                                              SoLuongDaBan = sp.SoLuongDaBan,
                                              SoLuongTrongKho = sp.SoLuongTrongKho,
                                              GiaTien = sp.GiaTien,
                                              Hang = h.TenHang,
                                              DanhMuc = dm.TenDanhMuc,
                                              MaH = h.MaHang,
                                              MaDM = dm.MaDanhMuc
                                          };

            // Phân trang
            var totalItemCount = await query.CountAsync();
            var model = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new
            {
                sanpham = model,
                totalItems = totalItemCount,
                page = page,
                pageSize = pageSize
            });
        }

        // POST: api/Product/ThemSP
        [HttpPost("ThemSP")]
        public async Task<IActionResult> ThemSP([FromForm] Sanpham spmoi, [FromForm] IFormFile[] images, [FromForm] string DanhMuc, [FromForm] string Hang, [FromServices] MinioService minioService)
        {
            // Lưu ảnh lên MinIO
            for (int i = 0; i < images.Length && i < 6; i++)
            {
                if (images[i] != null && images[i].Length > 0)
                {
                    // Tải ảnh lên MinIO và nhận URL của ảnh
                    string imageUrl = await minioService.UploadFileAsync(images[i]);

                    // Lưu URL vào các trường ảnh tương ứng
                    switch (i)
                    {
                        case 0: spmoi.Anh1 = imageUrl; break;
                        case 1: spmoi.Anh2 = imageUrl; break;
                        case 2: spmoi.Anh3 = imageUrl; break;
                        case 3: spmoi.Anh4 = imageUrl; break;
                        case 4: spmoi.Anh5 = imageUrl; break;
                        case 5: spmoi.Anh6 = imageUrl; break;
                    }
                }
            }

            // Lưu danh mục và hãng
            var dm = _context.Danhmucsanphams.FirstOrDefault(s => s.TenDanhMuc == DanhMuc);
            if (dm != null) spmoi.MaDanhMuc = dm.MaDanhMuc;

            var hang = _context.Hangsanxuats.FirstOrDefault(s => s.TenHang == Hang);
            if (hang != null) spmoi.MaHang = hang.MaHang;

            _context.Sanphams.Add(spmoi);
            await _context.SaveChangesAsync();

            return Ok(new { status = true });
        }


        // DELETE: api/Product/XoaSP/{maSP}
        [HttpDelete("XoaSP/{maSP}")]
        public async Task<IActionResult> XoaSP(int maSP, [FromServices] MinioService minioService)
        {
            var sp = await _context.Sanphams.FindAsync(maSP);
            if (sp == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy sản phẩm" });
            }

            // Xóa các ảnh liên quan trên MinIO
            if (!string.IsNullOrEmpty(sp.Anh1)) await minioService.DeleteFileAsync(sp.Anh1);
            if (!string.IsNullOrEmpty(sp.Anh2)) await minioService.DeleteFileAsync(sp.Anh2);
            if (!string.IsNullOrEmpty(sp.Anh3)) await minioService.DeleteFileAsync(sp.Anh3);
            if (!string.IsNullOrEmpty(sp.Anh4)) await minioService.DeleteFileAsync(sp.Anh4);
            if (!string.IsNullOrEmpty(sp.Anh5)) await minioService.DeleteFileAsync(sp.Anh5);
            if (!string.IsNullOrEmpty(sp.Anh6)) await minioService.DeleteFileAsync(sp.Anh6);

            // Xóa sản phẩm
            _context.Sanphams.Remove(sp);
            await _context.SaveChangesAsync();

            return Ok(new { status = true });
        }



        // PUT: api/Product/SuaSP
        [HttpPut("SuaSP")]
        public async Task<IActionResult> SuaSP([FromForm] Sanpham spmoi, [FromForm] IFormFile[] images, [FromForm] string DanhMuc, [FromForm] string Hang, [FromServices] MinioService minioService)
        {
            var sp = await _context.Sanphams.FindAsync(spmoi.MaSp);
            if (sp == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy sản phẩm" });
            }

            sp.TenSp = spmoi.TenSp;
            sp.MoTa = spmoi.MoTa;
            sp.GiaTien = spmoi.GiaTien;
            sp.SoLuongTrongKho = spmoi.SoLuongTrongKho;
            sp.SoLuongDaBan = spmoi.SoLuongDaBan;

            // Lưu ảnh mới lên MinIO và cập nhật URL
            for (int i = 0; i < images.Length && i < 6; i++)
            {
                if (images[i] != null && images[i].Length > 0)
                {
                    // Xóa ảnh cũ trên MinIO nếu có
                    switch (i)
                    {
                        case 0: if (!string.IsNullOrEmpty(sp.Anh1)) await minioService.DeleteFileAsync(sp.Anh1); break;
                        case 1: if (!string.IsNullOrEmpty(sp.Anh2)) await minioService.DeleteFileAsync(sp.Anh2); break;
                        case 2: if (!string.IsNullOrEmpty(sp.Anh3)) await minioService.DeleteFileAsync(sp.Anh3); break;
                        case 3: if (!string.IsNullOrEmpty(sp.Anh4)) await minioService.DeleteFileAsync(sp.Anh4); break;
                        case 4: if (!string.IsNullOrEmpty(sp.Anh5)) await minioService.DeleteFileAsync(sp.Anh5); break;
                        case 5: if (!string.IsNullOrEmpty(sp.Anh6)) await minioService.DeleteFileAsync(sp.Anh6); break;
                    }

                    // Tải ảnh mới lên MinIO và nhận URL của ảnh
                    string imageUrl = await minioService.UploadFileAsync(images[i]);

                    // Cập nhật URL mới vào các trường ảnh tương ứng
                    switch (i)
                    {
                        case 0: sp.Anh1 = imageUrl; break;
                        case 1: sp.Anh2 = imageUrl; break;
                        case 2: sp.Anh3 = imageUrl; break;
                        case 3: sp.Anh4 = imageUrl; break;
                        case 4: sp.Anh5 = imageUrl; break;
                        case 5: sp.Anh6 = imageUrl; break;
                    }
                }
            }

            // Lưu danh mục và hãng
            var dm = _context.Danhmucsanphams.FirstOrDefault(s => s.TenDanhMuc == DanhMuc);
            if (dm != null) sp.MaDanhMuc = dm.MaDanhMuc;

            var hang = _context.Hangsanxuats.FirstOrDefault(s => s.TenHang == Hang);
            if (hang != null) sp.MaHang = hang.MaHang;

            await _context.SaveChangesAsync();

            return Ok(new { status = true });
        }

        #region Quản lý hãng (Hangsanxuat)

        // GET: api/Admin/QuanLyHang
        [HttpGet("hangs")]
        public IActionResult QuanLyHang(string tenhang = "", int mahang = 0, int page = 1, int pageSize = 10)
        {
            var query = _context.Hangsanxuats.AsQueryable(); // Chuyển đổi sang IQueryable

            if (!string.IsNullOrEmpty(tenhang))
            {
                query = query.Where(dm => dm.TenHang.Contains(tenhang));
            }

            if (mahang != 0)
            {
                query = query.Where(item => item.MaHang == mahang);
            }

            var model = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var totalItemCount = query.Count();

            return Ok(new
            {
                data = model,
                totalItems = totalItemCount,
                pageStartItem = (page - 1) * pageSize + 1,
                pageEndItem = Math.Min(page * pageSize, totalItemCount),
                page = page,
                pageSize = pageSize,
                tenhang = tenhang,
                mahang = mahang
            });
        }

        // POST: api/Admin/hangs
        [HttpPost("hangs")]
        public IActionResult ThemHang([FromBody] string tenhang)
        {
            var hsx = new Hangsanxuat
            {
                TenHang = tenhang
            };
            _context.Hangsanxuats.Add(hsx);
            _context.SaveChanges();
            return Ok(new
            {
                status = true
            });
        }

        // DELETE: api/Admin/hangs/{id}
        [HttpDelete("hangs/{id}")]
        public IActionResult XoaHang(int id)
        {
            var hsx = _context.Hangsanxuats.Find(id);
            if (hsx != null)
            {
                _context.Hangsanxuats.Remove(hsx);
                _context.SaveChanges();
                return Ok(new
                {
                    status = true
                });
            }
            else
            {
                return NotFound(new
                {
                    status = false
                });
            }
        }

        // GET: api/Admin/hangs/{id}
        [HttpGet("hangs/{id}")]
        public IActionResult SuaHang(int id)
        {
            var model = _context.Hangsanxuats.Find(id);
            if (model != null)
            {
                return Ok(model);
            }
            return NotFound();
        }

        // PUT: api/Admin/hangs/{id}
        [HttpPut("hangs/{id}")]
        public IActionResult SuaHang(int id, [FromBody] string name)
        {
            var hsx = _context.Hangsanxuats.Find(id);
            if (hsx != null)
            {
                hsx.TenHang = name;
                _context.SaveChanges();
                return Ok(new
                {
                    status = true
                });
            }
            else
            {
                return NotFound(new
                {
                    status = false
                });
            }
        }

        #endregion
        #region Quản lý danh mục (Danhmucsanpham)

        // GET: api/Admin/danhmucs
        [HttpGet("danhmucs")]
        public IActionResult QuanLyDM(string tendm = "", int madm = 0, int page = 1, int pageSize = 10)
        {
            var query = _context.Danhmucsanphams.AsQueryable(); // Chuyển đổi sang IQueryable

            if (!string.IsNullOrEmpty(tendm))
            {
                query = query.Where(dm => dm.TenDanhMuc.Contains(tendm));
            }

            if (madm != 0)
            {
                query = query.Where(item => item.MaDanhMuc == madm);
            }

            var model = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var totalItemCount = query.Count();

            return Ok(new
            {
                data = model,
                totalItems = totalItemCount,
                pageStartItem = (page - 1) * pageSize + 1,
                pageEndItem = Math.Min(page * pageSize, totalItemCount),
                page = page,
                pageSize = pageSize,
                tendm = tendm,
                madm = madm
            });
        }

        // DELETE: api/Admin/danhmucs/{madm}
        [HttpDelete("danhmucs/{madm}")]
        public IActionResult XoaDM(int madm)
        {
            var dm = _context.Danhmucsanphams.Find(madm);
            if (dm != null)
            {
                _context.Danhmucsanphams.Remove(dm);
                _context.SaveChanges();
                return Ok(new
                {
                    status = true
                });
            }
            else
            {
                return NotFound(new
                {
                    status = false
                });
            }
        }

        // POST: api/Admin/danhmucs
        [HttpPost("danhmucs")]
        public IActionResult ThemDM([FromBody] string tendm)
        {
            var dm = new Danhmucsanpham
            {
                TenDanhMuc = tendm
            };
            _context.Danhmucsanphams.Add(dm);
            _context.SaveChanges();
            return Ok(new
            {
                status = true
            });
        }

        // GET: api/Admin/danhmucs/{id}
        [HttpGet("danhmucs/{id}")]
        public IActionResult SuaDM(int id)
        {
            var dm = _context.Danhmucsanphams.Find(id);
            if (dm != null)
            {
                return Ok(dm);
            }
            return NotFound();
        }

        // PUT: api/Admin/danhmucs/{id}
        [HttpPut("danhmucs/{id}")]
        public IActionResult SuaDM(int id, [FromBody] string name)
        {
            var dm = _context.Danhmucsanphams.Find(id);
            if (dm != null)
            {
                dm.TenDanhMuc = name;
                _context.SaveChanges();
                return Ok(new
                {
                    status = true
                });
            }
            else
            {
                return NotFound(new
                {
                    status = false
                });
            }
        }

        #endregion
        #region Quản lý đơn hàng (Donhang)

        // GET: api/Admin/orders
        [HttpGet("orders")]
        public IActionResult QuanLyDH(int? tinhTrang = null, int page = 1, int pageSize = 5)
        {
            var allOrders = from a in _context.Donhangs
                            join b in _context.Vanchuyens on a.MaDonHang equals b.MaDonHang
                            select new MyOrder()
                            {
                                MaDonHang = a.MaDonHang,
                                TongTien = a.TongTien,
                                NguoiNhan = b.NguoiNhan,
                                DiaChi = b.DiaChi,
                                NgayMua = a.NgayLap,
                                TinhTrang = a.TinhTrang
                            };

            // Lọc theo tình trạng nếu có
            if (tinhTrang.HasValue)
            {
                allOrders = allOrders.Where(o => o.TinhTrang == tinhTrang.Value);
            }

            var model = allOrders.OrderByDescending(o => o.MaDonHang)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToList();

            var totalItemCount = allOrders.Count();

            return Ok(new
            {
                data = model,
                totalItems = totalItemCount,
                page = page,
                pageSize = pageSize,
                totalPages = (totalItemCount + pageSize - 1) / pageSize
            });
        }

        // POST: api/Admin/orders/confirm/{madh}
        [HttpPost("orders/confirm/{madh}")]
        public IActionResult XacNhanDH(int madh)
        {
            var dh = _context.Donhangs.Find(madh);
            if (dh != null)
            {
                dh.TinhTrang = 2; // Xác nhận đơn hàng
                _context.SaveChanges();
                return Ok(new
                {
                    status = true
                });
            }
            else
            {
                return NotFound(new
                {
                    status = false
                });
            }
        }

        // POST: api/Admin/orders/ship/{madh}
        [HttpPost("orders/ship/{madh}")]
        public IActionResult VanChuyenDH(int madh)
        {
            var dh = _context.Donhangs.Find(madh);
            if (dh != null)
            {
                dh.TinhTrang = 3; // Đã vận chuyển
                _context.SaveChanges();
                return Ok(new
                {
                    status = true
                });
            }
            else
            {
                return NotFound(new
                {
                    status = false
                });
            }
        }

        // POST: api/Admin/orders/cancel/{madh}
        [HttpPost("orders/cancel/{madh}")]
        public IActionResult HuyDH(int madh)
        {
            var dh = _context.Donhangs.Find(madh);
            if (dh != null)
            {
                dh.TinhTrang = 4; // Đã hủy đơn hàng
                _context.SaveChanges();
                return Ok(new
                {
                    status = true
                });
            }
            else
            {
                return NotFound(new
                {
                    status = false
                });
            }
        }

        // GET: api/Admin/orders/{id}/details
        [HttpGet("orders/{id}/details")]
        public IActionResult MyOrderDetail(int id)
        {
            var orderDetails = from a in _context.Chitietdonhangs
                               join b in _context.Sanphams on a.MaSp equals b.MaSp
                               where a.MaDonHang == id
                               select new MyOrderDetail()
                               {
                                   MaSanPham = b.MaSp,
                                   TenSP = b.TenSp,
                                   Anh = b.Anh1,
                                   GiaBan = b.GiaTien,
                                   SoLuong = a.SoLuongMua,
                                   ThanhTien = b.GiaTien * a.SoLuongMua
                               };

            var result = orderDetails.ToList();
            if (result.Count == 0)
            {
                return NotFound();
            }

            return Ok(result);
        }
        //Hàm chuyển đổi dữ liệu từ dữ liệu tên ban đầu thành url
        //[HttpPost("UpdateProductImagesToUrl")]
        //public async Task<IActionResult> UpdateProductImagesToUrl()
        //{
        //    // Base URL cho MinIO
        //    string baseUrl = "http://localhost:9001/shopdogiadung/";

        //    // Lấy toàn bộ sản phẩm từ cơ sở dữ liệu
        //    var products = await _context.Sanphams.ToListAsync();

        //    foreach (var product in products)
        //    {
        //        // Nếu sản phẩm có các trường ảnh, cập nhật chúng thành URL đầy đủ
        //        if (!string.IsNullOrEmpty(product.Anh1))
        //        {
        //            product.Anh1 = $"{baseUrl}{product.Anh1}";
        //        }

        //        if (!string.IsNullOrEmpty(product.Anh2))
        //        {
        //            product.Anh2 = $"{baseUrl}{product.Anh2}";
        //        }

        //        if (!string.IsNullOrEmpty(product.Anh3))
        //        {
        //            product.Anh3 = $"{baseUrl}{product.Anh3}";
        //        }

        //        if (!string.IsNullOrEmpty(product.Anh4))
        //        {
        //            product.Anh4 = $"{baseUrl}{product.Anh4}";
        //        }

        //        if (!string.IsNullOrEmpty(product.Anh5))
        //        {
        //            product.Anh5 = $"{baseUrl}{product.Anh5}";
        //        }

        //        if (!string.IsNullOrEmpty(product.Anh6))
        //        {
        //            product.Anh6 = $"{baseUrl}{product.Anh6}";
        //        }

        //        // Đánh dấu sản phẩm này cần được cập nhật trong cơ sở dữ liệu
        //        _context.Sanphams.Update(product);
        //    }

        //    // Lưu thay đổi vào cơ sở dữ liệu
        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "Cập nhật URL thành công cho tất cả ảnh sản phẩm." });
        //}


        #endregion
        #region Thống kê doanh số bán ra

        // POST: api/Admin/statistics
        [HttpPost("statistics")]
        public IActionResult GetSalesStatistics([FromBody] int year)
        {
            var orders = _context.Donhangs
                                 .Where(s => s.NgayLap.HasValue && s.NgayLap.Value.Year == year)
                                 .ToList();

            var salesStatistics = new List<ThongKeDoanhThu>();

            // Khởi tạo doanh thu cho mỗi tháng
            for (int month = 1; month <= 12; month++)
            {
                long? monthlyTotal = orders
                    .Where(order => order.NgayLap.HasValue && order.NgayLap.Value.Month == month)
                    .Sum(order => order.TongTien) ?? 0;

                salesStatistics.Add(new ThongKeDoanhThu
                {
                    Thang = month,
                    DoanhThu = monthlyTotal
                });
            }

            return Ok(new
            {
                status = true,
                data = salesStatistics
            });
        }

        #endregion
    }
}
