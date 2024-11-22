using System;
using System.Collections.Generic;

namespace ShopDoGiaDungAPI.Models;

public partial class ChucNang
{
    public int MaChucNang { get; set; }

    public string? TenChucNang { get; set; }

    public virtual ICollection<PhanQuyen> PhanQuyens { get; set; } = new List<PhanQuyen>();

    public virtual ICollection<TaiKhoanPhanQuyen> TaiKhoanPhanQuyens { get; set; } = new List<TaiKhoanPhanQuyen>();
}
