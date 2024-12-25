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
        private const string SessionCart = "sessionCart";
        private readonly IOrderNotificationService _orderNotificationService;
        public CartService(OnlineShopContext context, IOrderNotificationService orderNotificationService)
        {
            _context = context;
            _orderNotificationService = orderNotificationService;
        }

        // Phương thức lấy giỏ hàng
        public IActionResult GetCart(ISession session)
        {
            var cart = session.Get(SessionCart);
            if (cart == null)
            {
                return new OkObjectResult(new { cart = new List<CartModel>(), tongtien = 0, countCart = 0 });
            }

            var list = JsonSerializer.Deserialize<List<CartModel>>(Encoding.UTF8.GetString(cart)) ?? new List<CartModel>();
            int tongtien = list.Sum(item => item.soluong * Convert.ToInt32(item.sanpham.GiaTien));
            int countCart = list.Sum(item => item.soluong);

            session.SetInt32("countCart", countCart);

            return new OkObjectResult(new
            {
                cart = list,
                tongtien = tongtien,
                countCart = countCart
            });
        }

        // Phương thức thêm sản phẩm vào giỏ hàng
        public JsonResult AddItemToCart(int productId, ISession session, bool checkOnly = false)
        {
            // Lấy thông tin sản phẩm từ cơ sở dữ liệu
            var product = _context.Sanphams.FirstOrDefault(c => c.MaSp == productId);

            // Kiểm tra nếu sản phẩm không tồn tại hoặc đã hết hàng
            if (product == null || product.SoLuongTrongKho <= 0)
            {
                return new JsonResult(new { status = false, message = "Product is out of stock" });
            }

            // Lấy session giỏ hàng hiện tại
            var cart = session.Get(SessionCart);
            List<CartModel> list = cart != null
                ? JsonSerializer.Deserialize<List<CartModel>>(Encoding.UTF8.GetString(cart))
                : new List<CartModel>();

            // Tìm sản phẩm trong giỏ hàng
            var existingItem = list.FirstOrDefault(x => x.sanpham.MaSp == productId);

            // Kiểm tra số lượng tồn kho
            if (existingItem != null)
            {
                // Nếu sản phẩm đã có trong giỏ, kiểm tra nếu số lượng cộng thêm sẽ vượt quá tồn kho
                if (existingItem.soluong + 1 > product.SoLuongTrongKho)
                {
                    return new JsonResult(new { status = false, message = "Cannot add more items. Not enough stock." });
                }

                // Nếu chỉ kiểm tra, trả về kết quả mà không cập nhật giỏ hàng
                if (checkOnly)
                {
                    return new JsonResult(new { status = true, message = "Item is available" });
                }

                // Nếu đủ tồn kho và không chỉ kiểm tra, tăng số lượng sản phẩm trong giỏ hàng
                existingItem.soluong += 1;
            }
            else
            {
                // Nếu sản phẩm chưa có trong giỏ hàng và tồn kho có ít nhất 1 sản phẩm
                if (product.SoLuongTrongKho < 1)
                {
                    return new JsonResult(new { status = false, message = "Product is out of stock" });
                }

                // Nếu chỉ kiểm tra, trả về kết quả mà không cập nhật giỏ hàng
                if (checkOnly)
                {
                    return new JsonResult(new { status = true, message = "Item is available" });
                }

                // Thêm sản phẩm mới vào giỏ hàng
                list.Add(new CartModel { sanpham = product, soluong = 1 });
            }

            // Cập nhật lại giỏ hàng trong session
            session.Set(SessionCart, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(list)));
            session.SetInt32("countCart", list.Sum(item => item.soluong));

            return new JsonResult(new { countCart = list.Sum(item => item.soluong), status = true });
        }



        // Phương thức lấy tổng tiền giỏ hàng
        public ActionResult GetCartTotal(ISession session)
        {
            var cart = session.Get(SessionCart);
            if (cart == null)
            {
                return new OkObjectResult(new { tong = 0 });
            }

            // Deserialize giỏ hàng từ session
            var list = JsonSerializer.Deserialize<List<CartModel>>(Encoding.UTF8.GetString(cart));
            int total = list.Sum(item => item.soluong * Convert.ToInt32(item.sanpham.GiaTien));

            return new OkObjectResult(new { tong = total });
        }


        // Phương thức xóa sản phẩm khỏi giỏ hàng
        public JsonResult DeleteItemFromCart(long productId, ISession session)
        {
            var cart = session.Get(SessionCart);
            if (cart == null) return new JsonResult(new { status = false });

            var list = JsonSerializer.Deserialize<List<CartModel>>(Encoding.UTF8.GetString(cart));
            list.RemoveAll(x => x.sanpham.MaSp == productId);

            session.Set(SessionCart, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(list)));
            int countCart = list.Sum(item => item.soluong);
            session.SetInt32("countCart", countCart);

            return new JsonResult(new { status = true, countCart });
        }

        // Phương thức cập nhật số lượng sản phẩm trong giỏ hàng
        public JsonResult UpdateCartItemQuantity(int productId, int quantity)
        {
            // Lấy thông tin sản phẩm từ cơ sở dữ liệu
            var product = _context.Sanphams.FirstOrDefault(c => c.MaSp == productId);

            // Kiểm tra nếu sản phẩm không tồn tại hoặc đã hết hàng
            if (product == null)
            {
                return new JsonResult(new { status = false, message = "Product does not exist." });
            }

            if (quantity <= 0)
            {
                return new JsonResult(new { status = false, message = "Quantity must be greater than zero." });
            }

            // Kiểm tra nếu số lượng yêu cầu vượt quá tồn kho
            if (product.SoLuongTrongKho < quantity)
            {
                return new JsonResult(new { status = false, message = "Requested quantity exceeds available stock." });
            }

            // Nếu kiểm tra tồn kho thành công
            return new JsonResult(new { status = true });
        }
        // Phương thức xóa toàn bộ giỏ hàng
        public JsonResult ClearCart(ISession session)
        {
            session.Remove(SessionCart);
            session.Remove("countCart");
            return new JsonResult(new { status = true });
        }

        // Phương thức thanh toán
        public async Task<JsonResult> Checkout(ThongTinThanhToan thanhToan, int? user = null)
        {
            int userId = _context.Taikhoans.FirstOrDefault(s => s.MaTaiKhoan == user)?.MaTaiKhoan ?? 0;

            // Kiểm tra thông tin thanh toán
            if (string.IsNullOrEmpty(thanhToan.ten) || string.IsNullOrEmpty(thanhToan.sdt) || string.IsNullOrEmpty(thanhToan.diaChi))
            {
                return new JsonResult(new { status = false, message = "Thông tin thanh toán không đầy đủ" });
            }

            // Kiểm tra giỏ hàng
            if (thanhToan.cartItems == null || !thanhToan.cartItems.Any())
            {
                return new JsonResult(new { status = false, message = "Giỏ hàng trống" });
            }

            // Tính tổng tiền
            long total = thanhToan.cartItems.Sum(item => item.quantity * Convert.ToInt32(item.cartItems.giaTien));

            // Tạo đơn hàng
            var order = new Donhang
            {
                MaTaiKhoan = userId,
                NgayLap = DateOnly.FromDateTime(DateTime.Now),
                TongTien = total,
                TinhTrang = 1 // Chờ xác nhận
            };

            _context.Donhangs.Add(order);
            await _context.SaveChangesAsync();

            var id = order.MaDonHang;
            var vc = new Vanchuyen
            {
                MaDonHang = id,
                NguoiNhan = thanhToan.ten,
                DiaChi = thanhToan.diaChi,
                Sdt = thanhToan.sdt,
                HinhThucVanChuyen = "Giao tận nhà"
            };
            _context.Vanchuyens.Add(vc);
            await _context.SaveChangesAsync();

            // Thêm chi tiết đơn hàng
            foreach (var item in thanhToan.cartItems)
            {
                var product = _context.Sanphams.Find(item.cartItems.maSp);
                if (product != null && product.SoLuongTrongKho >= item.quantity)
                {
                    var orderDetail = new Chitietdonhang
                    {
                        MaDonHang = order.MaDonHang,
                        MaSp = item.cartItems.maSp,
                        SoLuongMua = item.quantity
                    };
                    product.SoLuongDaBan += item.quantity;
                    product.SoLuongTrongKho -= item.quantity;

                    _context.Chitietdonhangs.Add(orderDetail);
                }
                else
                {
                    return new JsonResult(new { status = false, message = $"Sản phẩm {item.cartItems.maSp} không đủ số lượng trong kho" });
                }
            }

            await _context.SaveChangesAsync();

            // Sau khi tạo đơn hàng thành công, gửi thông báo real-time
            // Lấy đầy đủ thông tin đơn hàng để gửi
            var myOrder = (from a in _context.Donhangs
                           join b in _context.Vanchuyens on a.MaDonHang equals b.MaDonHang
                           where a.MaDonHang == order.MaDonHang
                           select new MyOrder()
                           {
                               MaDonHang = a.MaDonHang,
                               TongTien = a.TongTien,
                               NguoiNhan = b.NguoiNhan,
                               DiaChi = b.DiaChi,
                               NgayMua = a.NgayLap,
                               TinhTrang = a.TinhTrang
                           }).FirstOrDefault();

            // Giả sử bạn đã inject IOrderNotificationService vào constructor hoặc service
            await _orderNotificationService.NotifyNewOrderAsync(myOrder);

            // Trả về kết quả thành công
            return new JsonResult(new { status = true });
        }
    }

 }
