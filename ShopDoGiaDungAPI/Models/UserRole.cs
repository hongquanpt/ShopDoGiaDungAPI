using System;
using System.Collections.Generic;

namespace ShopDoGiaDungAPI.Models;

public partial class UserRole
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public string? Ten { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual Taikhoan User { get; set; } = null!;
}
