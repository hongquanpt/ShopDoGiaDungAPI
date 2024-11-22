using System;
using System.Collections.Generic;

namespace ShopDoGiaDungAPI.Models;

public partial class PhanQuyen
{
    public int MaPhanQuyen { get; set; }

    public int MaChucVu { get; set; }

    public int? MaChucNang { get; set; }

    public int? MaHanhDong { get; set; }

    public int? MaDonVi { get; set; }

    public virtual ChucNang? MaChucNangNavigation { get; set; }

    public virtual ChucVu2? MaChucVuNavigation { get; set; }

    public virtual DonVi? MaDonViNavigation { get; set; }

    public virtual HanhDong? MaHanhDongNavigation { get; set; }
}
