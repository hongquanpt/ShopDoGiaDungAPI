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

    public int? MaDonVi { get; set; }

    public virtual ICollection<Donhang> Donhangs { get; set; } = new List<Donhang>();

    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();

    public virtual ChucVu? MaCvNavigation { get; set; }

    public virtual DonVi? MaDonViNavigation { get; set; }

    public virtual ICollection<Message> MessageReceivers { get; set; } = new List<Message>();

    public virtual ICollection<Message> MessageSenders { get; set; } = new List<Message>();

    public virtual ICollection<TaiKhoanChucVu> TaiKhoanChucVus { get; set; } = new List<TaiKhoanChucVu>();

    public virtual ICollection<TaiKhoanPhanQuyen> TaiKhoanPhanQuyens { get; set; } = new List<TaiKhoanPhanQuyen>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
