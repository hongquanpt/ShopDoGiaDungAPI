using System;
using System.Collections.Generic;

namespace ShopDoGiaDungAPI.Models;

public partial class Taikhoan
{
    public int MaTaiKhoan { get; set; }

    public string? Ten { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? Sdt { get; set; }

    public string? DiaChi { get; set; }

    public string? Email { get; set; }

    public string? MatKhau { get; set; }

    public int? MaCv { get; set; }

    public virtual ICollection<Donhang> Donhangs { get; set; } = new List<Donhang>();

    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();

    public virtual ChucVu? MaCvNavigation { get; set; }
}
