namespace ShopDoGiaDungAPI.DTO
{
    public class ThongTinThanhToan
    {
        public string? ten { get; set; }
        public string? sdt { get; set; }
        public string? diaChi { get; set; }
        public string? hinhThucThanhToan { get; set; }
        public List<CartItemWrapper> cartItems { get; set; } = new List<CartItemWrapper>();
    }

    public class CartItemWrapper
    {
        public int quantity { get; set; }
        public CartItem cartItems { get; set; } = new CartItem();
    }

    public class CartItem
    {
        public int maSp { get; set; }
        public string? tenSp { get; set; }
        public string? moTa { get; set; }
        public string? anh1 { get; set; }
        public string? anh2 { get; set; }
        public string? anh3 { get; set; }
        public string? anh4 { get; set; }
        public string? anh5 { get; set; }
        public string? anh6 { get; set; }
        public int soLuongDaBan { get; set; }
        public int soLuongTrongKho { get; set; }
        public decimal giaTien { get; set; }
        public int maHang { get; set; }
        public int maDanhMuc { get; set; }
    }
}
