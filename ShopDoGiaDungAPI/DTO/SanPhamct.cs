namespace ShopDoGiaDungAPI.DTO
{
    public class SanPhamct
    {
        public int MaSp { get; set; }

        public string? TenSp { get; set; }

        public string? MoTa { get; set; }

        public byte[]? Image1 { get; set; }

        public byte[]? Image2 { get; set; }

        public byte[]? Image3 { get; set; }

        public byte[]? Image4 { get; set; }

        public byte[]? Image5 { get; set; }

        public byte[]? Image6 { get; set; }

        public int? SoLuongDaBan { get; set; }

        public int? SoLuongTrongKho { get; set; }

        public long? GiaTien { get; set; }

        public string? Hang { get; set; }

        public string? DanhMuc { get; set; }
        public int? MaH { get; set; }

        public int? MaDM { get; set; }
    }
}
