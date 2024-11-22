using System;
using System.Collections.Generic;

namespace ShopDoGiaDungAPI.Models;

public partial class ChucVu2
{
    public int MaChucVu { get; set; }

    public string? TenChucVu { get; set; }

    public virtual ICollection<PhanQuyen> PhanQuyens { get; set; } = new List<PhanQuyen>();

    public virtual ICollection<TaiKhoanChucVu> TaiKhoanChucVus { get; set; } = new List<TaiKhoanChucVu>();
}
