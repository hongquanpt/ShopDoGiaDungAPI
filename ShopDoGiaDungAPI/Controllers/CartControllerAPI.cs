using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.Models;
using System.Text.Json;
using System.Text;
using ShopDoGiaDungAPI.DTO;

namespace ShopDoGiaDungAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartControllerAPI : ControllerBase
    {
        public const string SessionCart = "sessionCart";
        private readonly ILogger<CartControllerAPI> _logger;
        private readonly OnlineShopContext _context;

        public CartControllerAPI(ILogger<CartControllerAPI> logger, OnlineShopContext context)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Cart
        [HttpGet]
        public IActionResult Index()
        {
            var cart = HttpContext.Session.Get(SessionCart);
            var list = new List<CartModel>();
            var tongtien = 0;

            if (cart != null)
            {
                var json = Encoding.UTF8.GetString(cart);
                list = JsonSerializer.Deserialize<List<CartModel>>(json);
                foreach (var item in list)
                {
                    tongtien += item.soluong * Convert.ToInt32(item.sanpham.GiaTien);
                }
            }

            return Ok(new
            {
                cart = list,
                tongtien = tongtien
            });
        }

        // POST: api/Cart/AddItem
        [HttpPost("AddItem")]
        public JsonResult AddItem(int productId)
        {
            var product = _context.Sanphams.FirstOrDefault(c => c.MaSp == productId);
            var cart = HttpContext.Session.Get(SessionCart);
            var countCart = HttpContext.Session.GetInt32("countCart");
            int count = 0;

            if (product.SoLuongTrongKho > 0)
            {
                if (cart != null)
                {
                    var json = Encoding.UTF8.GetString(cart);
                    var list = JsonSerializer.Deserialize<List<CartModel>>(json);
                    if (list.Exists(x => x.sanpham.MaSp == productId))
                    {
                        foreach (var item in list)
                        {
                            if (item.sanpham.MaSp == productId)
                            {
                                item.soluong += 1;
                            }
                        }
                    }
                    else
                    {
                        var item = new CartModel { sanpham = product, soluong = 1 };
                        list.Add(item);
                    }

                    var jsonSetSession = JsonSerializer.Serialize(list);
                    var byteArrayCart = Encoding.UTF8.GetBytes(jsonSetSession);
                    HttpContext.Session.Set(SessionCart, byteArrayCart);
                    count = list.Count;
                }
                else
                {
                    var item = new CartModel { sanpham = product, soluong = 1 };
                    var list = new List<CartModel> { item };
                    var jsonSetSession = JsonSerializer.Serialize(list);
                    var byteArrayCart = Encoding.UTF8.GetBytes(jsonSetSession);
                    HttpContext.Session.Set(SessionCart, byteArrayCart);
                    count = 1;
                }

                HttpContext.Session.SetInt32("countCart", count);
                return new JsonResult(new { countCart = count, status = true });
            }
            else
            {
                return new JsonResult(new { status = false });
            }
        }

        // GET: api/Cart/Total
        [HttpGet("Total")]
        public ActionResult Total()
        {
            var cart = HttpContext.Session.Get(SessionCart);
            var json = Encoding.UTF8.GetString(cart);
            var list = JsonSerializer.Deserialize<List<CartModel>>(json);
            int total = list.Sum(item => item.soluong * Convert.ToInt32(item.sanpham.GiaTien));

            return Ok(new { tong = total });
        }

        // DELETE: api/Cart/Delete/{id}
        [HttpDelete("Delete/{id}")]
        public JsonResult Delete(long id)
        {
            var cart = HttpContext.Session.Get(SessionCart);
            var json = Encoding.UTF8.GetString(cart);
            var list = JsonSerializer.Deserialize<List<CartModel>>(json);
            list.RemoveAll(x => x.sanpham.MaSp == id);

            var jsonSetSession = JsonSerializer.Serialize(list);
            var byteArrayCart = Encoding.UTF8.GetBytes(jsonSetSession);
            HttpContext.Session.Set(SessionCart, byteArrayCart);
            var countCart = HttpContext.Session.GetInt32("countCart");
            HttpContext.Session.SetInt32("countCart", countCart.HasValue ? countCart.Value - 1 : 0);

            return new JsonResult(new { status = true });
        }

        // PUT: api/Cart/Update
        [HttpPut("Update")]
        public JsonResult Update(int productId, int amount)
        {
            var product = _context.Sanphams.FirstOrDefault(c => c.MaSp == productId);
            var cart = HttpContext.Session.Get(SessionCart);
            var json = Encoding.UTF8.GetString(cart);
            var list = JsonSerializer.Deserialize<List<CartModel>>(json);

            float price = 0;
            float tongtien = 0;
            string kiemtrahethang = "";

            if (amount > product.SoLuongTrongKho)
            {
                kiemtrahethang = "hethang";
            }

            if (amount <= 0)
            {
                list.RemoveAll(x => x.sanpham.MaSp == productId);
                var countCart = HttpContext.Session.GetInt32("countCart");
                HttpContext.Session.SetInt32("countCart", countCart.HasValue ? countCart.Value - 1 : 0);
            }
            else
            {
                foreach (var item in list)
                {
                    if (item.sanpham.MaSp == productId)
                    {
                        item.soluong = amount;
                        price = amount * Convert.ToInt32(item.sanpham.GiaTien);
                    }
                    tongtien += item.soluong * Convert.ToInt32(item.sanpham.GiaTien);
                }
            }

            var jsonSetSession = JsonSerializer.Serialize(list);
            var byteArrayCart = Encoding.UTF8.GetBytes(jsonSetSession);
            HttpContext.Session.Set(SessionCart, byteArrayCart);

            return new JsonResult(new
            {
                status = true,
                productId = productId,
                price = price,
                tongtien = tongtien,
                countCart = list.Count,
                kiemtrahethang = kiemtrahethang
            });
        }

        // DELETE: api/Cart/DeleteAll
        [HttpDelete("DeleteAll")]
        public JsonResult DeleteAll()
        {
            HttpContext.Session.Remove(SessionCart);
            HttpContext.Session.Remove("countCart");
            return new JsonResult(new { status = true });
        }

        // POST: api/Cart/ThanhToan
        [HttpPost("ThanhToan")]
        public async Task<JsonResult> ThanhToan([FromBody] ThongTinThanhToan thanhToan)
        {
            try
            {
                if (String.IsNullOrEmpty(thanhToan.ten))
                {
                    return new JsonResult(new { status = false, message = "Họ tên người nhận không được bỏ trống" });
                }
                if (String.IsNullOrEmpty(thanhToan.SDT))
                {
                    return new JsonResult(new { status = false, message = "Số điện thoại không được bỏ trống" });
                }
                if (String.IsNullOrEmpty(thanhToan.DiaChi))
                {
                    return new JsonResult(new { status = false, message = "Địa chỉ không được bỏ trống" });
                }

                var order = new Donhang
                {
                    MaTaiKhoan = HttpContext.Session.GetInt32("Ma"),
                    NgayLap = DateOnly.FromDateTime(DateTime.Now),
                    TongTien = 0,
                    TinhTrang = 1
                };

                var cart = HttpContext.Session.Get(SessionCart);
                var json = Encoding.UTF8.GetString(cart);
                var list = JsonSerializer.Deserialize<List<CartModel>>(json);
                long total = list.Sum(item => item.soluong * Convert.ToInt32(item.sanpham.GiaTien));
                order.TongTien = total;

                _context.Donhangs.Add(order);
                await _context.SaveChangesAsync();

                var id = order.MaDonHang;
                var vc = new Vanchuyen
                {
                    MaDonHang = id,
                    NguoiNhan = thanhToan.ten,
                    DiaChi = thanhToan.DiaChi,
                    Sdt = thanhToan.SDT,
                    HinhThucVanChuyen = "Giao tận nhà"
                };

                _context.Vanchuyens.Add(vc);

                foreach (var item in list)
                {
                    var it = _context.Sanphams.Find(item.sanpham.MaSp);
                    var orderDetail = new Chitietdonhang
                    {
                        MaDonHang = id,
                        MaSp = item.sanpham.MaSp,
                        SoLuongMua = item.soluong
                    };
                    it.SoLuongDaBan += item.soluong;
                    it.SoLuongTrongKho -= item.soluong;

                    _context.Chitietdonhangs.Add(orderDetail);
                }

                await _context.SaveChangesAsync();
                HttpContext.Session.Remove(SessionCart);
                HttpContext.Session.Remove("countCart");

                return new JsonResult(new { status = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ThanhToan");
                return new JsonResult(new { status = false });
            }
        }
    
}
}
