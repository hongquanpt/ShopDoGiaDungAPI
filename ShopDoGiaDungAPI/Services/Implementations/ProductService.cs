using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly OnlineShopContext _context;
        private readonly IMinioService _minioService;

        public ProductService(OnlineShopContext context, IMinioService minioService)
        {
            _context = context;
            _minioService = minioService;
        }

        // Implement all methods from IProductService

        // Admin functions
        public async Task<IActionResult> GetProducts(int page, int pageSize)
        {
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

            var totalItemCount = await query.CountAsync();
            var model = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Tạo Pre-signed URL cho mỗi ảnh trước khi trả về
            foreach (var sp in model)
            {
                sp.Anh1 = await _minioService.GetPreSignedUrlAsync(sp.Anh1);
                sp.Anh2 = await _minioService.GetPreSignedUrlAsync(sp.Anh2);
                sp.Anh3 = await _minioService.GetPreSignedUrlAsync(sp.Anh3);
                sp.Anh4 = await _minioService.GetPreSignedUrlAsync(sp.Anh4);
                sp.Anh5 = await _minioService.GetPreSignedUrlAsync(sp.Anh5);
                sp.Anh6 = await _minioService.GetPreSignedUrlAsync(sp.Anh6);
            }

            return new OkObjectResult(new
            {
                sanpham = model,
                totalItems = totalItemCount,
                page = page,
                pageSize = pageSize
            });
        }

        public async Task<IActionResult> AddProduct(Sanpham spmoi, IFormFile[] images, string DanhMuc, string Hang)
        {
            // Lưu ảnh lên MinIO
            for (int i = 0; i < images.Length && i < 6; i++)
            {
                if (images[i] != null && images[i].Length > 0)
                {
                    string imageUrl;
                    try
                    {
                        imageUrl = await _minioService.UploadFileAsync(images[i]);
                    }
                    catch (Exception ex)
                    {
                        return new BadRequestObjectResult(new { status = false, message = $"Lỗi khi tải ảnh lên MinIO: {ex.Message}" });
                    }

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

            var dm = _context.Danhmucsanphams.FirstOrDefault(s => s.TenDanhMuc == DanhMuc);
            if (dm != null) spmoi.MaDanhMuc = dm.MaDanhMuc;

            var hang = _context.Hangsanxuats.FirstOrDefault(s => s.TenHang == Hang);
            if (hang != null) spmoi.MaHang = hang.MaHang;

            _context.Sanphams.Add(spmoi);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { status = true });
        }

        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var sp = await _context.Sanphams.FindAsync(productId);
            if (sp == null)
            {
                return new NotFoundObjectResult(new { status = false, message = "Không tìm thấy sản phẩm" });
            }

            // Xóa các ảnh liên quan trên MinIO
            if (!string.IsNullOrEmpty(sp.Anh1)) await _minioService.DeleteFileAsync(sp.Anh1);
            if (!string.IsNullOrEmpty(sp.Anh2)) await _minioService.DeleteFileAsync(sp.Anh2);
            if (!string.IsNullOrEmpty(sp.Anh3)) await _minioService.DeleteFileAsync(sp.Anh3);
            if (!string.IsNullOrEmpty(sp.Anh4)) await _minioService.DeleteFileAsync(sp.Anh4);
            if (!string.IsNullOrEmpty(sp.Anh5)) await _minioService.DeleteFileAsync(sp.Anh5);
            if (!string.IsNullOrEmpty(sp.Anh6)) await _minioService.DeleteFileAsync(sp.Anh6);

            _context.Sanphams.Remove(sp);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { status = true });
        }

        public async Task<IActionResult> UpdateProduct(Sanpham spmoi, IFormFile[] images, string DanhMuc, string Hang)
        {
            var sp = await _context.Sanphams.FindAsync(spmoi.MaSp);
            if (sp == null)
            {
                return new NotFoundObjectResult(new { status = false, message = "Không tìm thấy sản phẩm" });
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
                        case 0: if (!string.IsNullOrEmpty(sp.Anh1)) await _minioService.DeleteFileAsync(sp.Anh1); break;
                        case 1: if (!string.IsNullOrEmpty(sp.Anh2)) await _minioService.DeleteFileAsync(sp.Anh2); break;
                        case 2: if (!string.IsNullOrEmpty(sp.Anh3)) await _minioService.DeleteFileAsync(sp.Anh3); break;
                        case 3: if (!string.IsNullOrEmpty(sp.Anh4)) await _minioService.DeleteFileAsync(sp.Anh4); break;
                        case 4: if (!string.IsNullOrEmpty(sp.Anh5)) await _minioService.DeleteFileAsync(sp.Anh5); break;
                        case 5: if (!string.IsNullOrEmpty(sp.Anh6)) await _minioService.DeleteFileAsync(sp.Anh6); break;
                    }

                    // Tải ảnh mới lên MinIO và nhận URL của ảnh
                    string imageUrl;
                    try
                    {
                        imageUrl = await _minioService.UploadFileAsync(images[i]);
                    }
                    catch (Exception ex)
                    {
                        return new BadRequestObjectResult(new { status = false, message = $"Lỗi khi tải ảnh lên MinIO: {ex.Message}" });
                    }

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

            return new OkObjectResult(new { status = true });
        }

        // Home functions
        public async Task<IActionResult> GetTopSellingProducts()
        {
            var sanpham = await _context.Sanphams.OrderByDescending(a => a.SoLuongDaBan).Take(6).ToListAsync();
            var danhmucsp = await _context.Danhmucsanphams.ToListAsync();
            var hang = await _context.Hangsanxuats.ToListAsync();

            // Tạo Pre-signed URL cho ảnh sản phẩm
            foreach (var sp in sanpham)
            {
                sp.Anh1 = await _minioService.GetPreSignedUrlAsync(sp.Anh1);
            }

            return new OkObjectResult(new
            {
                sanpham = sanpham,
                danhmucsp = danhmucsp,
                hang = hang
            });
        }

        public async Task<IActionResult> GetProductsByBrand(int brandId, string brandName, int pageIndex, int pageSize, int maxPrice, int minPrice, string orderPrice)
        {
            IQueryable<Sanpham> query = _context.Sanphams.Where(s => s.MaHang == brandId);

            if (maxPrice != 0)
            {
                query = query.Where(item => item.GiaTien < maxPrice && item.GiaTien > minPrice);
            }

            query = orderPrice == "tang" ? query.OrderBy(item => item.GiaTien) : query.OrderByDescending(item => item.GiaTien);

            var count = await query.CountAsync();
            var model = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            // Tạo Pre-signed URL cho ảnh sản phẩm
            foreach (var sp in model)
            {
                sp.Anh1 = await _minioService.GetPreSignedUrlAsync(sp.Anh1);
            }

            return new OkObjectResult(new
            {
                sanpham = model,
                maxPrice = maxPrice,
                minPrice = minPrice,
                orderPrice = orderPrice,
                tenhang = brandName,
                idHang = brandId,
                totalCount = count
            });
        }

        public async Task<IActionResult> GetProductsByCategory(int categoryId, string categoryName, int pageIndex, int pageSize, int maxPrice, int minPrice, string orderPrice)
        {
            IQueryable<Sanpham> query = _context.Sanphams.Where(s => s.MaDanhMuc == categoryId);

            if (maxPrice != 0)
            {
                query = query.Where(item => item.GiaTien < maxPrice && item.GiaTien > minPrice);
            }

            query = orderPrice == "tang" ? query.OrderBy(item => item.GiaTien) : query.OrderByDescending(item => item.GiaTien);

            var count = await query.CountAsync();
            var model = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            // Tạo Pre-signed URL cho ảnh sản phẩm
            foreach (var sp in model)
            {
                sp.Anh1 = await _minioService.GetPreSignedUrlAsync(sp.Anh1);
            }

            return new OkObjectResult(new
            {
                sanpham = model,
                maxPrice = maxPrice,
                minPrice = minPrice,
                orderPrice = orderPrice,
                tendanhmuc = categoryName,
                idCategory = categoryId,
                totalCount = count
            });
        }

        public async Task<IActionResult> GetProductDetail(int productId)
        {
            var danhgia = await (from a in _context.Taikhoans
                                 join b in _context.Danhgiasanphams on a.MaTaiKhoan equals b.MaTaiKhoan
                                 join c in _context.Sanphams on b.MaSp equals c.MaSp
                                 where c.MaSp == productId
                                 orderby b.NgayDanhGia descending
                                 select new CommentView()
                                 {
                                     TenTaiKhoan = a.Ten,
                                     DanhGia = b.DanhGia,
                                     NoiDung = b.NoiDungBinhLuan,
                                     ThoiGian = b.NgayDanhGia
                                 }).ToListAsync();

            int? sum = danhgia.Sum(item => item.DanhGia);
            double sao = danhgia.Count() > 0 ? Math.Round((double)sum / danhgia.Count(), 1) : 0;

            var sanpham = await _context.Sanphams.FindAsync(productId);

            // Tạo Pre-signed URL cho ảnh sản phẩm
            sanpham.Anh1 = await _minioService.GetPreSignedUrlAsync(sanpham.Anh1);

            return new OkObjectResult(new
            {
                sanpham = sanpham,
                danhgia = danhgia,
                sao = sao
            });
        }

        public async Task<IActionResult> GetAllProducts(int pageIndex, int pageSize, int maxPrice, int minPrice, string orderPrice)
        {
            IQueryable<Sanpham> model = _context.Sanphams;

            if (maxPrice != 0)
            {
                model = model.Where(item => item.GiaTien < maxPrice && item.GiaTien > minPrice);
            }

            model = orderPrice == "tang" ? model.OrderBy(item => item.GiaTien) : model.OrderByDescending(item => item.GiaTien);

            var count = await model.CountAsync();
            var dt = await model.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            // Tạo Pre-signed URL cho ảnh sản phẩm
            foreach (var sp in dt)
            {
                sp.Anh1 = await _minioService.GetPreSignedUrlAsync(sp.Anh1);
            }

            return new OkObjectResult(new
            {
                sanpham = dt,
                maxPrice = maxPrice,
                minPrice = minPrice,
                orderPrice = orderPrice,
                totalCount = count
            });
        }

        public async Task<IActionResult> SearchProducts(string search, int pageIndex, int pageSize, int maxPrice, int minPrice, string orderPrice)
        {
            IQueryable<Sanpham> model = _context.Sanphams.Where(s => s.TenSp.Contains(search));

            if (maxPrice != 0)
            {
                model = model.Where(item => item.GiaTien < maxPrice && item.GiaTien > minPrice);
            }

            model = orderPrice == "tang" ? model.OrderBy(item => item.GiaTien) : model.OrderByDescending(item => item.GiaTien);

            var count = await model.CountAsync();
            var dt = await model.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            // Tạo Pre-signed URL cho ảnh sản phẩm
            foreach (var sp in dt)
            {
                sp.Anh1 = await _minioService.GetPreSignedUrlAsync(sp.Anh1);
            }

            return new OkObjectResult(new
            {
                sanpham = dt,
                search = search,
                maxPrice = maxPrice,
                minPrice = minPrice,
                orderPrice = orderPrice,
                totalCount = count
            });
        }
    }
}
