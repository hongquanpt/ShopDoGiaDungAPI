﻿namespace ShopDoGiaDungAPI.DTO
{
    public class TaiKhoanDto
    {
        public int MaTaiKhoan { get; set; }
        public string? Ten { get; set; }

        public DateOnly? NgaySinh { get; set; }

        public string? Sdt { get; set; }

        public string? DiaChi { get; set; }

        public string? Email { get; set; }
    }
}