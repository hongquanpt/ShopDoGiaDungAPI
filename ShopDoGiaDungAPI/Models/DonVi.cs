using System;
using System.Collections.Generic;

namespace ShopDoGiaDungAPI.Models;

public partial class DonVi
{
    public int MaDonVi { get; set; }

    public string? TenDonVi { get; set; }

    public int? MaDonViCha { get; set; }

    public virtual ICollection<DonVi> InverseMaDonViChaNavigation { get; set; } = new List<DonVi>();

    public virtual DonVi? MaDonViChaNavigation { get; set; }

    public virtual ICollection<PhanQuyen> PhanQuyens { get; set; } = new List<PhanQuyen>();

    public virtual ICollection<TaiKhoanPhanQuyen> TaiKhoanPhanQuyens { get; set; } = new List<TaiKhoanPhanQuyen>();

    public virtual ICollection<Taikhoan> Taikhoans { get; set; } = new List<Taikhoan>();
}
