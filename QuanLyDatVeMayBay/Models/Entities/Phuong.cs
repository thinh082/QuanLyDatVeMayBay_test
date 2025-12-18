using System;
using System.Collections.Generic;

namespace QuanLyDatVeMayBay.Models.Entities;

public partial class Phuong
{
    public int IdPhuong { get; set; }

    public string TenPhuong { get; set; } = null!;

    public int IdQuan { get; set; }

    public virtual Quan IdQuanNavigation { get; set; } = null!;

    public virtual ICollection<KhachHang> KhachHangs { get; set; } = new List<KhachHang>();
}
