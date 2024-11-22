using System;
using System.Collections.Generic;

namespace ShopDoGiaDungAPI.Models;

public partial class HanhDong
{
    public int MaHanhDong { get; set; }

    public string? TenHanhDong { get; set; }

    public virtual ICollection<PhanQuyen> PhanQuyens { get; set; } = new List<PhanQuyen>();

    public virtual ICollection<TaiKhoanPhanQuyen> TaiKhoanPhanQuyens { get; set; } = new List<TaiKhoanPhanQuyen>();
}
