using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.DTO;

namespace ShopDoGiaDungAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminControllerAPI : ControllerBase
    {
        private readonly OnlineShop2Context _context;

        public AdminControllerAPI(OnlineShop2Context context)
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

        //[HttpPost("convert-images")]
        //public IActionResult ConvertImagesToBinary()
        //{
        //    // Đường dẫn tĩnh của thư mục chứa hình ảnh
        //    string basePath = @"C:\Users\Admin\Desktop\QuanLyShopDoGiaDung\wwwroot\Admin\images";

        //    // Lấy danh sách tất cả sản phẩm có tên tệp ảnh trong các cột Anh1, Anh2, ..., Anh6
        //    var sanphams = _context.Sanphams.ToList();

        //    foreach (var sanpham in sanphams)
        //    {
        //        try
        //        {
        //            // Xử lý ảnh 1 (Anh1 -> Image1)
        //            if (!string.IsNullOrEmpty(sanpham.Anh1))
        //            {
        //                string fullImagePath = Path.Combine(basePath, sanpham.Anh1);
        //                if (System.IO.File.Exists(fullImagePath))
        //                {
        //                    var imageBytes = System.IO.File.ReadAllBytes(fullImagePath);
        //                    sanpham.Image1 = imageBytes; // Gán dữ liệu ảnh 1 vào Image1
        //                }
        //            }

        //            // Xử lý ảnh 2 (Anh2 -> Image2)
        //            if (!string.IsNullOrEmpty(sanpham.Anh2))
        //            {
        //                string fullImagePath = Path.Combine(basePath, sanpham.Anh2);
        //                if (System.IO.File.Exists(fullImagePath))
        //                {
        //                    var imageBytes = System.IO.File.ReadAllBytes(fullImagePath);
        //                    sanpham.Image2 = imageBytes; // Gán dữ liệu ảnh 2 vào Image2
        //                }
        //            }

        //            // Xử lý ảnh 3 (Anh3 -> Image3)
        //            if (!string.IsNullOrEmpty(sanpham.Anh3))
        //            {
        //                string fullImagePath = Path.Combine(basePath, sanpham.Anh3);
        //                if (System.IO.File.Exists(fullImagePath))
        //                {
        //                    var imageBytes = System.IO.File.ReadAllBytes(fullImagePath);
        //                    sanpham.Image3 = imageBytes; // Gán dữ liệu ảnh 3 vào Image3
        //                }
        //            }

        //            // Xử lý ảnh 4 (Anh4 -> Image4)
        //            if (!string.IsNullOrEmpty(sanpham.Anh4))
        //            {
        //                string fullImagePath = Path.Combine(basePath, sanpham.Anh4);
        //                if (System.IO.File.Exists(fullImagePath))
        //                {
        //                    var imageBytes = System.IO.File.ReadAllBytes(fullImagePath);
        //                    sanpham.Image4 = imageBytes; // Gán dữ liệu ảnh 4 vào Image4
        //                }
        //            }

        //            // Xử lý ảnh 5 (Anh5 -> Image5)
        //            if (!string.IsNullOrEmpty(sanpham.Anh5))
        //            {
        //                string fullImagePath = Path.Combine(basePath, sanpham.Anh5);
        //                if (System.IO.File.Exists(fullImagePath))
        //                {
        //                    var imageBytes = System.IO.File.ReadAllBytes(fullImagePath);
        //                    sanpham.Image5 = imageBytes; // Gán dữ liệu ảnh 5 vào Image5
        //                }
        //            }

        //            // Xử lý ảnh 6 (Anh6 -> Image6)
        //            if (!string.IsNullOrEmpty(sanpham.Anh6))
        //            {
        //                string fullImagePath = Path.Combine(basePath, sanpham.Anh6);
        //                if (System.IO.File.Exists(fullImagePath))
        //                {
        //                    var imageBytes = System.IO.File.ReadAllBytes(fullImagePath);
        //                    sanpham.Image6 = imageBytes; // Gán dữ liệu ảnh 6 vào Image6
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
        [HttpGet("products")]
        public IActionResult GetProducts(int page = 1, int pageSize = 5)
        {
            var query = from sp in _context.Sanphams
                        join h in _context.Hangsanxuats on sp.MaHang equals h.MaHang
                        join dm in _context.Danhmucsanphams on sp.MaDanhMuc equals dm.MaDanhMuc
                        select new SanPhamct
                        {
                            MaSp = sp.MaSp,
                            TenSp = sp.TenSp,
                            MoTa = sp.MoTa,
                            Image1 = sp.Image1 ,
                            Image2 = sp.Image2,
                            Image3 = sp.Image3,
                            Image4 = sp.Image4,
                            Image5 = sp.Image5,
                            Image6 = sp.Image6,
                            SoLuongDaBan = sp.SoLuongDaBan,
                            SoLuongTrongKho = sp.SoLuongTrongKho,
                            GiaTien = sp.GiaTien,
                            Hang = h.TenHang,
                            DanhMuc = dm.TenDanhMuc,
                            MaH = h.MaHang,
                            MaDM = dm.MaDanhMuc,
                        };

            var totalItemCount = query.Count();
            var model = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Trả về kết quả phân trang
            return Ok(new
            {
                data = model,
                pageStartItem = (page - 1) * pageSize + 1,
                pageEndItem = Math.Min(page * pageSize, totalItemCount),
                page = page,
                pageSize = pageSize,
                totalItemCount = totalItemCount
            });
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

    }
}
