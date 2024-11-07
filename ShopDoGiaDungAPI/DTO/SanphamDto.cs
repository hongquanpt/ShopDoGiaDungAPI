namespace ShopDoGiaDungAPI.DTO
{
    public class SanphamDto
    {
        public string TenSP { get; set; }
        public string MoTa { get; set; }
        public int SoLuongTrongKho { get; set; }
        public long GiaTien { get; set; }
        public int DanhMuc { get; set; }
        public int Hang { get; set; }
        public IFormFile[] Images { get; set; } // Chú ý tên thuộc tính 'Images'
    }
}
