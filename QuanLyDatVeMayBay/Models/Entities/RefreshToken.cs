using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class RefreshToken
{
    public long Id { get; set; }

    public string RefreshToken1 { get; set; } = null!;

    public DateTime ExpiryDate { get; set; }

    public long IdTaiKhoan { get; set; }

    public virtual TaiKhoan IdTaiKhoanNavigation { get; set; } = null!;
}
