namespace ShopDoGiaDungAPI.DTO
{
    public class DonhangDto
    {
        public int MaDonHang { get; set; }
        public int? TinhTrang { get; set; }
        public DateOnly? NgayDatHang { get; set; }
        public VanchuyenDto Vanchuyen { get; set; }
        public long? TongTien { get; set; }
    }

    public class VanchuyenDto
    {
        public string NguoiNhan { get; set; }
        public string DiaChi { get; set; }
    }
}
