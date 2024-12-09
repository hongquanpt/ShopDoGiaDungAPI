using System;
using System.Collections.Generic;

namespace ShopDoGiaDungAPI.Models;

public partial class Message
{
    public int Id { get; set; }

    public int? SenderId { get; set; }

    public int? ReceiverId { get; set; }

    public string? Content { get; set; }

    public short? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Taikhoan? Receiver { get; set; }

    public virtual Taikhoan? Sender { get; set; }
}
