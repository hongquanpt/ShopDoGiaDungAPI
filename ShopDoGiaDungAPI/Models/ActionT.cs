using System;
using System.Collections.Generic;

namespace ShopDoGiaDungAPI.Models;

public partial class ActionT
{
    public int MaA { get; set; }

    public string? TenA { get; set; }

    public virtual ICollection<CvQA> CvQAs { get; set; } = new List<CvQA>();
}
