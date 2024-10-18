using System;
using System.Collections.Generic;

namespace ShopDoGiaDungAPI.Models;

public partial class Sanpham
{
    public int MaSp { get; set; }

    public string? TenSp { get; set; }

    public string? MoTa { get; set; }

    public int? SoLuongDaBan { get; set; }

    public int? SoLuongTrongKho { get; set; }

    public long? GiaTien { get; set; }

    public int? MaHang { get; set; }

    public int? MaDanhMuc { get; set; }

    public byte[]? Image1 { get; set; }

    public byte[]? Image2 { get; set; }

    public byte[]? Image3 { get; set; }

    public byte[]? Image4 { get; set; }

    public byte[]? Image5 { get; set; }

    public byte[]? Image6 { get; set; }

    public virtual ICollection<Chitietdonhang> Chitietdonhangs { get; set; } = new List<Chitietdonhang>();

    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();

    public virtual Danhmucsanpham? MaDanhMucNavigation { get; set; }

    public virtual Hangsanxuat? MaHangNavigation { get; set; }
}
