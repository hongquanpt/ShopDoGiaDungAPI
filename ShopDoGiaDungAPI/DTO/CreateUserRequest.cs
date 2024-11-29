namespace ShopDoGiaDungAPI.DTO
{
    public class CreateUserRequest
    {
        public string Ten { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Sdt { get; set; }
        public string DiaChi { get; set; }
        public DateTime? NgaySinh { get; set; }
        public List<int> ChucVuIds { get; set; } // Danh sách các ID chức vụ
    }
}
