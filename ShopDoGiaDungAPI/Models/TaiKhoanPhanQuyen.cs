using System;
using System.Collections.Generic;

namespace ShopDoGiaDungAPI.Models;

public partial class TaiKhoanPhanQuyen
{
    public int? MaTaiKhoan { get; set; }

    public int? MaChucNang { get; set; }

    public int? MaHanhDong { get; set; }

    public int? MaDonVi { get; set; }

    public virtual ChucNang MaChucNangNavigation { get; set; } = null!;

    public virtual DonVi MaDonViNavigation { get; set; } = null!;

    public virtual HanhDong MaHanhDongNavigation { get; set; } = null!;

    public virtual Taikhoan MaTaiKhoanNavigation { get; set; } = null!;
}
