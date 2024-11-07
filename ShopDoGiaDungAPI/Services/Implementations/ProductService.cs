using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Linq;

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

        public async Task AddProduct(SanphamDto model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Thông tin sản phẩm không hợp lệ.");
            }

            // Tạo đối tượng Sanpham từ model
            var spmoi = new Sanpham
            {
                TenSp = model.TenSP,
                MoTa = model.MoTa,
                SoLuongTrongKho = model.SoLuongTrongKho,
                GiaTien = model.GiaTien,
                MaDanhMuc = model.DanhMuc,
                MaHang = model.Hang,
                SoLuongDaBan = 0
            };

            // Danh sách để lưu tên hoặc URL ảnh
            var imageUrls = new List<string>();

            // Lưu ảnh lên MinIO và lưu tên ảnh vào cơ sở dữ liệu
            if (model.Images != null && model.Images.Length > 0)
            {
                for (int i = 0; i < model.Images.Length && i < 6; i++)
                {
                    var image = model.Images[i];
                    if (image != null && image.Length > 0)
                    {
                        string imageUrl;
                        try
                        {
                            // Tải ảnh lên MinIO và lấy URL hoặc tên ảnh
                            imageUrl = await _minioService.UploadFileAsync(image);

                            // Thêm URL vào danh sách
                            imageUrls.Add(imageUrl);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Lỗi khi tải ảnh lên MinIO: {ex.Message}");
                        }
                    }
                }

                // Gán URL ảnh vào các thuộc tính tương ứng trong spmoi
                if (imageUrls.Count > 0) spmoi.Anh1 = imageUrls.ElementAtOrDefault(0);
                if (imageUrls.Count > 1) spmoi.Anh2 = imageUrls.ElementAtOrDefault(1);
                if (imageUrls.Count > 2) spmoi.Anh3 = imageUrls.ElementAtOrDefault(2);
                if (imageUrls.Count > 3) spmoi.Anh4 = imageUrls.ElementAtOrDefault(3);
                if (imageUrls.Count > 4) spmoi.Anh5 = imageUrls.ElementAtOrDefault(4);
                if (imageUrls.Count > 5) spmoi.Anh6 = imageUrls.ElementAtOrDefault(5);
            }
            else
            {
                throw new Exception("Vui lòng tải lên ít nhất một ảnh cho sản phẩm.");
            }

            // Thêm sản phẩm vào cơ sở dữ liệu
            try
            {
                _context.Sanphams.Add(spmoi);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lưu sản phẩm: {ex.Message}");
            }
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
        public async Task<(bool IsSuccess, string Message)> UpdateCartItemQuantityAsync(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return (false, "Số lượng phải lớn hơn 0.");
            }

            // Truy xuất sản phẩm từ cơ sở dữ liệu
            var product = await _context.Sanphams.FirstOrDefaultAsync(p => p.MaSp == productId);

            if (product == null)
            {
                return (false, "Sản phẩm không tồn tại.");
            }

            // Kiểm tra tồn kho
            if (product.SoLuongTrongKho < quantity)
            {
                return (false, $"Số lượng yêu cầu vượt quá tồn kho. Tồn kho hiện tại: {product.SoLuongTrongKho}.");
            }

            // Nếu cần, bạn có thể cập nhật số lượng trong cơ sở dữ liệu tại đây
            // Ví dụ: Giảm tồn kho nếu muốn cập nhật tồn kho theo giỏ hàng
            // product.Stock -= quantity;
            // _context.Products.Update(product);
            // await _context.SaveChangesAsync();

            // Trả về phản hồi thành công
            return (true, "Số lượng sản phẩm còn đủ.");
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

            var sp = await _context.Sanphams.FindAsync(productId);

            // Tạo Pre-signed URL cho ảnh sản phẩm
            sp.Anh1 = await _minioService.GetPreSignedUrlAsync(sp.Anh1);
            sp.Anh2 = await _minioService.GetPreSignedUrlAsync(sp.Anh2);
            sp.Anh3 = await _minioService.GetPreSignedUrlAsync(sp.Anh3);
            sp.Anh4 = await _minioService.GetPreSignedUrlAsync(sp.Anh4);
            sp.Anh5 = await _minioService.GetPreSignedUrlAsync(sp.Anh5);
            sp.Anh6 = await _minioService.GetPreSignedUrlAsync(sp.Anh6);

            return new OkObjectResult(new
            {
                sanpham = sp,
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

        public async Task<IActionResult> SearchProducts(
     string? search,
     string? idCategories,
     string? idHangs,
     int pageIndex,
     int pageSize,
     string? maxPrice,
     string? minPrice,
     string orderPrice)
        {
            // Khởi tạo query sản phẩm
            IQueryable<Sanpham> model = _context.Sanphams;

            // Áp dụng bộ lọc theo danh mục nếu có
            if (!string.IsNullOrEmpty(idCategories))
            {
                int[] arrayCategory = Array.ConvertAll(idCategories.Split(","), int.Parse);
                model = model.Where(item => arrayCategory.Contains(item.MaDanhMuc??0));
            }

            // Áp dụng bộ lọc theo hãng nếu có
            if (!string.IsNullOrEmpty(idHangs))
            {
                int[] arrayIdHang = Array.ConvertAll(idHangs.Split(","), int.Parse);
                model = model.Where(item => arrayIdHang.Contains(item.MaHang ?? 0));
            }

            // Áp dụng điều kiện tìm kiếm theo tên sản phẩm nếu có
            if (!string.IsNullOrEmpty(search))
            {
                model = model.Where(s => s.TenSp.Contains(search));
            }

            // Áp dụng bộ lọc theo giá tối đa nếu có
            if (!string.IsNullOrEmpty(maxPrice) && int.TryParse(maxPrice, out int priceMax))
            {
                model = model.Where(item => item.GiaTien <= priceMax);
            }

            // Áp dụng bộ lọc theo giá tối thiểu nếu có
            if (!string.IsNullOrEmpty(minPrice) && int.TryParse(minPrice, out int priceMin))
            {
                model = model.Where(item => item.GiaTien >= priceMin);
            }

            // Sắp xếp theo giá
            model = orderPrice == "tang" ? model.OrderBy(item => item.GiaTien) : model.OrderByDescending(item => item.GiaTien);

            // Đếm tổng số sản phẩm phù hợp với bộ lọc
            var count = await model.CountAsync();

            // Lấy dữ liệu trang
            var dt = await model.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            // Tạo URL tạm thời cho ảnh sản phẩm
            foreach (var sp in dt)
            {
                sp.Anh1 = await _minioService.GetPreSignedUrlAsync(sp.Anh1);
            }

            // Trả về kết quả
            return new OkObjectResult(new
            {
                sanpham = dt,
                maxPrice = maxPrice,
                minPrice = minPrice,
                orderPrice = orderPrice,
                totalCount = count
            });
        }


    }
}
