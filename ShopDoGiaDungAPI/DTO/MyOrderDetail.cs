﻿namespace ShopDoGiaDungAPI.DTO
{
    public class MyOrderDetail
    {
        public int MaSanPham { get; set; }
        public string TenSP { get; set; }
        public byte[]? Image { get; set; }
        public long? GiaBan { get; set; }
        public int? SoLuong { get; set; }
        public long? ThanhTien { get; set; }
    }
}
