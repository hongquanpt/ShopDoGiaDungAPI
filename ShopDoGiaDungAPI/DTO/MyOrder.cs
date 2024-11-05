namespace ShopDoGiaDungAPI.DTO
{
    public class MyOrder
    {
        public int MaDonHang { get; set; }
        public long? TongTien { get; set; }
        public string? NguoiNhan { get; set; }11
        public string? DiaChi { get; set; }
        public DateOnly? NgayMua { get; set; }
        public int? TinhTrang { get; set; }
    }
}
