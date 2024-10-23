using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Text.Json;
using System.Text;

namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly OnlineShopContext _context;

        public CartService(OnlineShopContext context)
        {
            _context = context;
        }

        private const string SessionCart = "sessionCart";

        public IActionResult GetCart(ISession session)
        {
            var cart = session.Get(SessionCart);
            var list = new List<CartModel>();
            var tongtien = 0;

            if (cart != null)
            {
                var json = Encoding.UTF8.GetString(cart);
                list = JsonSerializer.Deserialize<List<CartModel>>(json);
                tongtien = list.Sum(item => item.soluong * Convert.ToInt32(item.sanpham.GiaTien));
            }

            return new OkObjectResult(new
            {
                cart = list,
                tongtien = tongtien
            });
        }

        public JsonResult AddItemToCart(int productId, ISession session)
        {
            var product = _context.Sanphams.FirstOrDefault(c => c.MaSp == productId);
            var cart = session.Get(SessionCart);
            var countCart = session.GetInt32("countCart");
            int count = 0;

            if (product.SoLuongTrongKho > 0)
            {
                List<CartModel> list;
                if (cart != null)
                {
                    var json = Encoding.UTF8.GetString(cart);
                    list = JsonSerializer.Deserialize<List<CartModel>>(json);
                    var existingItem = list.FirstOrDefault(x => x.sanpham.MaSp == productId);
                    if (existingItem != null)
                    {
                        existingItem.soluong += 1;
                    }
                    else
                    {
                        list.Add(new CartModel { sanpham = product, soluong = 1 });
                    }
                }
                else
                {
                    list = new List<CartModel> { new CartModel { sanpham = product, soluong = 1 } };
                }

                var jsonSetSession = JsonSerializer.Serialize(list);
                var byteArrayCart = Encoding.UTF8.GetBytes(jsonSetSession);
                session.Set(SessionCart, byteArrayCart);
                count = list.Count;

                session.SetInt32("countCart", count);
                return new JsonResult(new { countCart = count, status = true });
            }
            else
            {
                return new JsonResult(new { status = false });
            }
        }

        public ActionResult GetCartTotal(ISession session)
        {
            var cart = session.Get(SessionCart);
            var json = Encoding.UTF8.GetString(cart);
            var list = JsonSerializer.Deserialize<List<CartModel>>(json);
            int total = list.Sum(item => item.soluong * Convert.ToInt32(item.sanpham.GiaTien));

            return new OkObjectResult(new { tong = total });
        }

        public JsonResult DeleteItemFromCart(long productId, ISession session)
        {
            var cart = session.Get(SessionCart);
            var json = Encoding.UTF8.GetString(cart);
            var list = JsonSerializer.Deserialize<List<CartModel>>(json);
            list.RemoveAll(x => x.sanpham.MaSp == productId);

            var jsonSetSession = JsonSerializer.Serialize(list);
            var byteArrayCart = Encoding.UTF8.GetBytes(jsonSetSession);
            session.Set(SessionCart, byteArrayCart);
            var countCart = session.GetInt32("countCart");
            session.SetInt32("countCart", countCart.HasValue ? countCart.Value - 1 : 0);

            return new JsonResult(new { status = true });
        }

        public JsonResult UpdateCartItem(int productId, int amount, ISession session)
        {
            var product = _context.Sanphams.FirstOrDefault(c => c.MaSp == productId);
            var cart = session.Get(SessionCart);
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
                var countCart = session.GetInt32("countCart");
                session.SetInt32("countCart", countCart.HasValue ? countCart.Value - 1 : 0);
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
            session.Set(SessionCart, byteArrayCart);

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

        public JsonResult ClearCart(ISession session)
        {
            session.Remove(SessionCart);
            session.Remove("countCart");
            return new JsonResult(new { status = true });
        }

        public async Task<JsonResult> Checkout(ThongTinThanhToan thanhToan, ISession session, int userId)
        {
            try
            {
                if (string.IsNullOrEmpty(thanhToan.ten))
                {
                    return new JsonResult(new { status = false, message = "Họ tên người nhận không được bỏ trống" });
                }
                if (string.IsNullOrEmpty(thanhToan.SDT))
                {
                    return new JsonResult(new { status = false, message = "Số điện thoại không được bỏ trống" });
                }
                if (string.IsNullOrEmpty(thanhToan.DiaChi))
                {
                    return new JsonResult(new { status = false, message = "Địa chỉ không được bỏ trống" });
                }

                var order = new Donhang
                {
                    MaTaiKhoan = userId,
                    NgayLap = DateOnly.FromDateTime(DateTime.Now),
                    TongTien = 0,
                    TinhTrang = 1
                };

                var cart = session.Get(SessionCart);
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
                session.Remove(SessionCart);
                session.Remove("countCart");

                return new JsonResult(new { status = true });
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                return new JsonResult(new { status = false });
            }
        }
    }
}
